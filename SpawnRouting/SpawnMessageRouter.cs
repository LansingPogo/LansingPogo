using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Bot.SpawnRouting
{
   public class SpawnMessageRouter
   {
      private readonly IConfigurationRoot _config;
      private readonly RangeDmRouter _rangeRouter;
      private Dictionary<ulong, ulong> _routeMap = new Dictionary<ulong, ulong>();
      private Dictionary<int, ulong> _raidMap = new Dictionary<int, ulong>();

      public SpawnMessageRouter( IConfigurationRoot config, RangeDmRouter rangeRouter )
      {
         _config = config;
         _rangeRouter = rangeRouter;
         BuildRouteDictionary();
      }

      private void BuildRouteDictionary()
      {
         //_routeMap.Add( Convert.ToUInt64( _config["channels:TestGen"] ), Convert.ToUInt64( _config["channels:Testing"] ) );
         _routeMap.Add( Convert.ToUInt64( _config["channels:Pokemonivs"] ), Convert.ToUInt64( _config["channels:Pokemon"] ) );
         _routeMap.Add( Convert.ToUInt64( _config["channels:Rareivs"] ), Convert.ToUInt64( _config["channels:Rares"] ) );
         _routeMap.Add( Convert.ToUInt64( _config["channels:Starterivs"] ), Convert.ToUInt64( _config["channels:Starter"] ) );
         _routeMap.Add( Convert.ToUInt64( _config["channels:RaidAlerts"] ), 0 ); //Will decide route based on raid level

         _raidMap.Add( 1, Convert.ToUInt64( _config["channels:Raid1"] ) );
         _raidMap.Add( 2, Convert.ToUInt64( _config["channels:Raid2"] ) );
         _raidMap.Add( 3, Convert.ToUInt64( _config["channels:Raid3"] ) );
         _raidMap.Add( 4, Convert.ToUInt64( _config["channels:Raid4"] ) );
         _raidMap.Add( 5, Convert.ToUInt64( _config["channels:Raid5"] ) );
      }

      internal async Task RouteMessage( SocketUserMessage msg )
      {
         Console.WriteLine( msg.Channel.Name );
         if( !_routeMap.ContainsKey( msg.Channel.Id ) )
            return;
         else if( msg.Channel.Id.Equals( Convert.ToUInt64( _config["channels:RaidAlerts"] ) ) )
            await ProcessRaidMessage( msg );
         else
            await ProcessMessage( msg, _routeMap[msg.Channel.Id] );
      }

      private async Task ProcessRaidMessage( SocketUserMessage msg )
      {
         var disableMessages = false;
         if( Convert.ToBoolean( _config["debug:DisableRaid"] ) )
            disableMessages = true;

         var embed = (msg.Embeds as IReadOnlyList<Embed>)[0];
         var descriptionLines = embed.Description.Split( "\n" );

         var pattern = @"\*\*Level:\*\*\s(?<Level>\d+)";
         Regex rgx = new Regex( pattern, RegexOptions.IgnoreCase );
         MatchCollection matches = rgx.Matches( descriptionLines[0] );
         if( matches.Count != 1 )
         {
            Console.WriteLine( "Regex Failed for Raid: " + descriptionLines[0] );
            return;// Task.CompletedTask; //Regex failed
         }

         var raidLevel = Convert.ToInt32( matches[0].Groups["Level"].Value );
         var raidChannelId = _raidMap[raidLevel];

         var targetChannel = (msg.Channel as SocketGuildChannel).Guild.GetChannel( raidChannelId );
         if( targetChannel == null )
            return;

         var pokemonName = embed.Title.Substring( 0, embed.Title.IndexOf( " " ) );
         var roleName = String.Format( "{0}-r",  pokemonName );
         var raidRoleId = GetRole( (msg.Channel as SocketGuildChannel).Guild.Roles, roleName );

         if( raidRoleId.HasValue && !disableMessages )
         {
            await (targetChannel as ISocketMessageChannel).SendMessageAsync( MentionUtils.MentionRole( raidRoleId.Value ) );
         }

         if( !disableMessages )
            await (targetChannel as ISocketMessageChannel).SendMessageAsync( String.Empty, embed: embed );

         //range
         await _rangeRouter.RoutePokemon( msg, embed, pokemonName );
      }

      private async Task ProcessMessage( SocketUserMessage msg, ulong destinationChannelId )
      {
         var targetChannel = (msg.Channel as SocketGuildChannel).Guild.GetChannel( destinationChannelId );
         if( targetChannel == null )
            return;// Task.CompletedTask;

         if( msg.Embeds.Count == 0 )
            return;// Task.CompletedTask;

         var embed = (msg.Embeds as IReadOnlyList<Embed>)[0];
         var descriptionLines = embed.Description.Split( "\n" );

         string pattern = @"(?<Name>[a-zA-Z]*),\s(?<Iv>\d+\.\d+)%\s(?<Stats>\(\d+\/\d+\/\d+\)),\sCP:\s(?<CP>\d+),\sLv:\s(?<Lvl>\d+)";
         Regex rgx = new Regex( pattern, RegexOptions.IgnoreCase );
         MatchCollection matches = rgx.Matches( descriptionLines[0] );
         if( matches.Count != 1 )
         {
            Console.WriteLine( "Regex Failed for Pokemon: " + descriptionLines[0] );
            return;// Task.CompletedTask; //Regex failed
         }

         var pokemonName = matches[0].Groups["Name"].Value;
         var pokemonRoleId = GetRole( (msg.Channel as SocketGuildChannel).Guild.Roles, pokemonName );

         var disableMessages = Convert.ToBoolean( _config["debug:DisablePokemon"] );
         //Basic routing, no IV here!
         if( !disableMessages )
            await SendMessageToDestination( embed, targetChannel, pokemonRoleId );

         //Check special cases...
         var percent = Convert.ToDecimal( matches[0].Groups["Iv"].Value );
         //100IV channel
         if( percent == 100.0m )
         {
            var hundoChannelId = Convert.ToUInt64( _config["channels:Perfect100"] );
            var hundoChannel = (msg.Channel as SocketGuildChannel).Guild.GetChannel( hundoChannelId );
            var role100 = GetRole( (msg.Channel as SocketGuildChannel).Guild.Roles, "100" );

            if( !disableMessages && hundoChannel != null && role100 != null )
               await SendMessageToDestination( embed, hundoChannel, role100, false, pokemonName );
         }
         else if( percent == 0.0m )
         {
            var spamChannelId = Convert.ToUInt64( _config["channels:DittoSpam"] );
            var spamChannel = (msg.Channel as SocketGuildChannel).Guild.GetChannel( spamChannelId );
            var role0 = GetRole( (msg.Channel as SocketGuildChannel).Guild.Roles, "0" );

            if( !disableMessages && spamChannel != null && role0 != null )
               await SendMessageToDestination( embed, spamChannel, role0, false, pokemonName );
         }

         //Perfect and Lvl30+ channel
         if( matches[0].Groups["Stats"].Value == "(15/15/15)" && Convert.ToInt32( matches[0].Groups["Lvl"].Value ) >= 30 )
         {
            var perfectChannelId = Convert.ToUInt64( _config["channels:Max"] );
            var perfectChannel = (msg.Channel as SocketGuildChannel).Guild.GetChannel( perfectChannelId );
            var rolePerfect = GetRole( (msg.Channel as SocketGuildChannel).Guild.Roles, "lvl30+" );

            if( !disableMessages && perfectChannel != null && rolePerfect != 100 )
               await SendMessageToDestination( embed, perfectChannel, rolePerfect, false, pokemonName );
         }


         await _rangeRouter.RoutePokemon( msg, embed, pokemonName );
      }

      private ulong? GetRole( IReadOnlyCollection<SocketRole> roles, string pokemonName )
      {
         foreach( var role in roles )
         {
            if( role.Name.ToLower().Equals( pokemonName.ToLower() ) )
               return role.Id;
         }

         return null;
      }

      private async Task SendMessageToDestination( Embed embed, SocketGuildChannel targetChannel, ulong? roleId = null, bool removeIvDetails = true, string pokemonName = "" )
      {
         var newDescription = embed.Description;
         if( removeIvDetails )
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

         //Mentions
         if( roleId.HasValue )
         {
            var message = MentionUtils.MentionRole( roleId.Value );
            if( !String.IsNullOrEmpty( pokemonName ) )
               message += String.Format( " {0}", pokemonName );

            await (targetChannel as ISocketMessageChannel).SendMessageAsync( message );
         }

         //Send Main Message
         await (targetChannel as ISocketMessageChannel).SendMessageAsync( String.Empty, embed: eb );
      }
   }
}
