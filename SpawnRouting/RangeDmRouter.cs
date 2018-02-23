using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bot.Models;
using Bot.Repositories;
using Discord;
using Discord.WebSocket;
using GeoCoordinatePortable;
using Microsoft.Extensions.Configuration;

namespace Bot.SpawnRouting
{
   public class RangeDmRouter
   {
      private readonly IConfigurationRoot _config;
      private readonly RangeRepository _repo;
      private readonly ulong _rareChannelId;
      private readonly ulong _starterChannelId;
      private readonly ulong _pokemonChannelId;
      private readonly ulong _raidChannelId;

      public RangeDmRouter( IConfigurationRoot config, RangeRepository repo )
      {
         _config = config;
         _repo = repo;
         _pokemonChannelId = Convert.ToUInt64( _config["channels:Pokemonivs"] );
         _rareChannelId = Convert.ToUInt64( _config["channels:Rareivs"] );
         _starterChannelId = Convert.ToUInt64( _config["channels:Starterivs"] );
         _raidChannelId = Convert.ToUInt64( _config["channels:RaidAlerts"] );
      }

      public async Task RoutePokemon( SocketUserMessage msg, Embed embed, string pokemonName )
      {
         //http://maps.google.com/maps?q=42.7535745003,-84.5337275652
         var guild = (msg.Channel as SocketGuildChannel).Guild;
         var channelId = (msg.Channel as SocketGuildChannel).Id;

         var serverRangeUsers = _repo.GetByServerID( guild.Id );
         var liveMapRole = GetRole( guild.Roles, "livemap" );
         if( liveMapRole == null )
            return;

         var url = embed.Url;
         if( url.IndexOf( "maps?q=" ) == -1 )
            return;

         var gps = url.Substring( url.IndexOf( "maps?q=" ) + 7 );
         var latLong = gps.Split( "," );
         if( latLong.Length != 2 )
            return;

         double latitude;
         if( !double.TryParse( latLong[0], out latitude ) )
            return;

         double longitude;
         if( !double.TryParse( latLong[1], out longitude ) )
            return;

         var sCoord = new GeoCoordinate( latitude, longitude );

         foreach( var userRange in serverRangeUsers )
         {
            if( !userRange.Active )
               continue;

            var user = guild.GetUser( Convert.ToUInt64( userRange.UserId ) );
            if( user == null )
               continue;

            bool stripIv = true;
            if( GetRole( user.Roles, "livemap" ) != null )
               stripIv = false;

            var uCoord = new GeoCoordinate( userRange.Latitude, userRange.Longitude );

            var distance = sCoord.GetDistanceTo( uCoord ) / 1000.0; //meters to km
            if( distance <= Convert.ToDouble( userRange.RangeKm ) )
            {
               await ProcessUser( user, userRange, distance, channelId, embed, pokemonName, stripIv );
            }
         }
      }

      private async Task ProcessUser( SocketGuildUser user, Range userRange, double distance, ulong channelId, Embed embed, string pokemonName, bool stripIv )
      {
         if( _starterChannelId.Equals( channelId ) && userRange.Starters
            || _pokemonChannelId.Equals( channelId ) && userRange.Starters
            || _rareChannelId.Equals( channelId ) && userRange.Rares )
         {
            await SendUserDm( user, userRange, distance, embed, pokemonName, stripIv );
         }
         else if( _raidChannelId.Equals( channelId ) && userRange.Raids )
         {
            await SendUserRaidDm( user, userRange, distance, embed, pokemonName );
         }
      }

      private async Task SendUserRaidDm( SocketGuildUser user, Range userRange, double distance, Embed embed, string pokemonName )
      {
         var dmChannel = await user.GetOrCreateDMChannelAsync();
         if( dmChannel == null )
            return;

         var descriptionLines = embed.Description.Split( "\n" );
         var pattern = @"\*\*Level:\*\*\s(?<Level>\d+)";
         Regex rgx = new Regex( pattern, RegexOptions.IgnoreCase );
         MatchCollection matches = rgx.Matches( descriptionLines[0] );
         if( matches.Count != 1 )
         {
            Console.WriteLine( "Regex Failed for Raid: " + descriptionLines[0] );
            return; //Regex failed
         }

         var raidLevel = Convert.ToInt32( matches[0].Groups["Level"].Value );

         var raids = userRange.RaidLevels.Split( "," );
         bool userSubscribed = false;
         foreach( var raidLvl in raids )
         {
            if( Convert.ToInt32( raidLvl ) == raidLevel )
            {
               userSubscribed = true;
               break;
            }
         }

         if( !userSubscribed )
            return;

         var text = String.Format( "Level {0} {1} raid within range! ({2} km)", raidLevel, pokemonName, Math.Round( distance, 2 ) );

         //Send DM Message
         if( Debugger.IsAttached || Convert.ToBoolean( _config["debug:DisableRange"] ) )
            return;
         else
            await dmChannel.SendMessageAsync( text, embed: embed );
      }

      private async Task SendUserDm( SocketGuildUser user, Range userRange, double distance, Embed embed, string pokemonName, bool stripIv )
      {
         var dmChannel = await user.GetOrCreateDMChannelAsync();
         if( dmChannel == null )
            return;

         var newDescription = embed.Description;
         var text = String.Format( "{0} within range ({1} km)", pokemonName, Math.Round( distance, 2 ) );

         if( stripIv )
         {
            string[] descriptionLines = newDescription.Split( "\n" );
            if( descriptionLines.Length == 3 )
            {
               newDescription = String.Format( "{0}{1}{2}{1}{3}",
                  descriptionLines[2],
                  Environment.NewLine,
                  "For IV and Moveset Info, sign",
                  "up for the Livemap in #livemap"
                  );
            }
            else
               newDescription = String.Empty; //something bad happened, or something changed
         }
         else
         {
            newDescription = String.Format( "{0}",
               newDescription );

            var descriptionLines = embed.Description.Split( "\n" );
            string pattern = @"(?<Name>[a-zA-Z]*),\s(?<Iv>\d+\.\d+)%\s(?<Stats>\(\d+\/\d+\/\d+\)),\sCP:\s(?<CP>\d+),\sLv:\s(?<Lvl>\d+)";
            Regex rgx = new Regex( pattern, RegexOptions.IgnoreCase );
            MatchCollection matches = rgx.Matches( descriptionLines[0] );
            if( matches.Count == 1 )
            {
               var percent = Convert.ToDecimal( matches[0].Groups["Iv"].Value );
               var lvl = Convert.ToInt32( matches[0].Groups["Lvl"].Value );
               text = String.Format( "LVL {0}, IV: {1} {2} within range! ({3} km)",
                  lvl, percent, pokemonName, Math.Round( distance, 2 ) );
            }
         }

         EmbedBuilder eb = new EmbedBuilder()
            .WithTitle( embed.Title )
            .WithDescription( newDescription )
            .WithUrl( embed.Url );

         if( embed.Color.HasValue )
            eb.WithColor( embed.Color.Value );
         if( embed.Thumbnail.HasValue )
            eb.WithThumbnailUrl( embed.Thumbnail.Value.Url );
         if( embed.Image.HasValue )
            eb.WithImageUrl( embed.Image.Value.Url );

         //Send DM Message
         if( Debugger.IsAttached || Convert.ToBoolean( _config["debug:DisableRange"] ) )
            return;
         else
            await dmChannel.SendMessageAsync( text, embed: eb );
      }

      private ulong? GetRole( IReadOnlyCollection<SocketRole> roles, string roleName )
      {
         foreach( var role in roles )
         {
            if( role.Name.ToLower().Equals( roleName.ToLower() ) )
               return role.Id;
         }

         return null;
      }
   }
}
