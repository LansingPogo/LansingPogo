using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bot.Models;
using Bot.Repositories;
using Discord;
using Discord.Commands;
using GeoCoordinatePortable;
using Microsoft.Extensions.Configuration;

namespace Bot.Modules
{
   [Name( "Range" )]
   public class RangeModule : ModuleBase<SocketCommandContext>
   {
      [Group( "range" ), Name( "Range" )]
      public class Range : ModuleBase
      {
         private readonly RangeRepository _repo;
         private readonly IConfigurationRoot _config;

         public Range( RangeRepository repo, IConfigurationRoot config )
         {
            _repo = repo;
            _config = config;
         }

         [Command( "help" )]
         [Summary( "Help Info for range feature" )]
         public async Task Help()
         {
            await SendRangeHelpMessage();
         }

         private async Task SendRangeHelpMessage()
         {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine( "**DISCLAIMER:**" );
            sb.AppendLine( "  **THE FOLLOWING COMMANDS WILL DIRECT MESSAGE YOU.**" );
            sb.AppendLine( "  **PREVENT OTHERS FROM SEEING YOUR SENSITIVE DATA.**" );
            sb.AppendLine( "  **DO NOT DISCLOSE YOUR GPS LOCATION TO ANYBODY (IT\'S LIKELY YOUR HOME).**" );
            sb.AppendLine( "  **AUTHOR IS NOT RESPONSIBLE FOR YOUR ACTIONS.**" );
            sb.AppendLine( "\n" );
            sb.AppendLine( "**What does this do? How does it do it?**" );
            sb.AppendLine( "Range feature will message you directly when a 'Rare' spawn has been found in your selected range." );
            sb.AppendLine( "It does this by calulating the distance between your given GPS coords and the GPS coords of the 'Rare' Pokemon." );
            sb.AppendLine( "If the distance is within your defined range, you will receive a message that a 'Rare' Pokemon is near by." );
            // Part 1
            EmbedBuilder eb = new EmbedBuilder()
               .WithDescription( sb.ToString() );
            await Context.User.SendMessageAsync( "", embed: eb );


            sb = new StringBuilder();
            sb.AppendLine( "**" + _config["prefix"] + "range help** -- This help message." );
            sb.AppendLine( "**" + _config["prefix"] + "range gps** -- Defines GPS coordinates for the user initiating the request." );
            sb.AppendLine( "    Requires 3 arguments: Latitude, Longitude, Range (in km)." );
            sb.AppendLine( "    **Example:** " + _config["prefix"] + "range gps 42.7325 -84.5555 5" );
            sb.AppendLine( "\n" );
            sb.AppendLine( "**" + _config["prefix"] + "range show** -- Displays your data." );
            sb.AppendLine( "**" + _config["prefix"] + "range on** -- Activates your range notifications. (DEFAULT)" );
            sb.AppendLine( "**" + _config["prefix"] + "range off** -- Deactivates your range notifications." );
            sb.AppendLine( "**" + _config["prefix"] + "range set X** -- Where X is a floating point number (xx.y) From 0 to 20. Example: 1.7 means 1.7km" );
            sb.AppendLine( "**" + _config["prefix"] + "range delete** -- Deletes any and all entries of your ranges." );
            // Part 2
            eb = new EmbedBuilder()
               .WithDescription( sb.ToString() );
            await Context.User.SendMessageAsync( "", embed: eb );

            sb = new StringBuilder();
            sb.AppendLine( "\n" );
            sb.AppendLine( "**" + _config["prefix"] + "range rares on|off** -- Turn " + MentionUtils.MentionChannel( Convert.ToUInt64( _config["channels:Rares"] ) ) + " range notifications on/off." );
            sb.AppendLine( "**" + _config["prefix"] + "range pokemon on|off** -- Turn " + MentionUtils.MentionChannel( Convert.ToUInt64( _config["channels:Pokemon"] ) ) + " range notifications on/off." );
            sb.AppendLine( "**" + _config["prefix"] + "range starters on|off** -- Turn " + MentionUtils.MentionChannel( Convert.ToUInt64( _config["channels:Starter"] ) ) + " range notifications on/off." );
            sb.AppendLine( "**" + _config["prefix"] + "range raids on|off** -- Turn raid channels range notifications on/off." );
            sb.AppendLine( "**" + _config["prefix"] + "range raidlevels 1,2,3,4,5** -- Example: " + _config["prefix"] + "range raidlevels 1,4,5 for raids Level: 1, 4 and 5" );
            //rangeList.push("****");
            //rangeList.push("**"+_config["prefix"]+"range mentions on|off** -- Turn mentions on/off. Whether to obey your current set mentions(on) or ignore them(off). [NOT IMPLEMENTED]");
            // Part 3
            eb = new EmbedBuilder()
               .WithDescription( sb.ToString() );
            await Context.User.SendMessageAsync( "", embed: eb );

            sb = new StringBuilder();
            sb.AppendLine( "\n" );
            sb.AppendLine( "**How to find your GPS coordinates?** https://www.maps.ie/coordinates.html" );
            sb.AppendLine( "**FAQs**" );
            sb.AppendLine( "**Q:** Can I setup multiple locations? **A:** No." );
            sb.AppendLine( "**Q:** Do I need to place 'commas' between the arguments of " + _config["prefix"] + "range gps? **A:** Comma is optional between latitude and longitude. GPS needs dots i.e: 42.1234 -84.1234" );
            sb.AppendLine( "**Q:** What will I get notification for? **A:** Any scanned pokemon in the channels you have marked as active, if within your gps locations range." );
            sb.AppendLine( "**Q:** Do I need to setup mentions for this? **A:** No, mentions are not required (currently)." );
            // Part 4
            eb = new EmbedBuilder()
               .WithDescription( sb.ToString() );

            await Context.Message.DeleteAsync();
            await Context.User.SendMessageAsync( "", embed: eb );
         }

         [Command( "on" )]
         [Summary( "Turn on range feature" )]
         public async Task On()
         {
            await Toggle( "Active", "on" );
         }

         [Command( "off" )]
         [Summary( "Turn off range feature" )]
         public async Task Off()
         {
            await Toggle( "Active", "off" );
         }

         [Command( "examples" )]
         [Summary( "Examples for range feature" )]
         public async Task Examples()
         {
            await SendExampleMessage();
         }

         private async Task SendExampleMessage()
         {
            EmbedBuilder eb = new EmbedBuilder()
               .WithDescription( "View the full list of Ditto commands here:\n" + "https://pastebin.com/raw/38ADYm4z" );

            await Context.Message.DeleteAsync();
            await Context.User.SendMessageAsync( "", embed: eb );
         }

         [Command( "gps" )]
         [Summary( "Set current user GPS location for range" )]
         public async Task Gps( params string[] coords )
         {
            double latitude = 0;
            double longitude = 0;
            decimal km = 0;

            var pattern = @"^(?<lat>\-?\d+(\.\d+)?),*\s*(?<long>\-?\d+(\.\d+)?)\s*(?<range>\d*.\d*)$";
            Regex rgx = new Regex( pattern, RegexOptions.IgnoreCase );
            MatchCollection matches = rgx.Matches( String.Join( " ", coords ) );
            if( matches.Count != 1 )
            {
               await SendDmSuggestion( "Unable to parse your gps command, please double check the '?range help' and '?range examples' commands" );
               return;
            }

            latitude = Convert.ToDouble( matches[0].Groups["lat"].Value );
            longitude = Convert.ToDouble( matches[0].Groups["long"].Value );
            km = Convert.ToDecimal( matches[0].Groups["range"].Value );

            if( BadLocation( latitude, longitude ) )
            {
               await SendDmSuggestion( "The location you entered appeared to be outside of our scan area.  Please try a different location." );
               return;
            }

            var userRange = GetUserRange( true );

            userRange.Latitude = latitude;
            userRange.Longitude = longitude;
            userRange.RangeKm = Math.Min( Math.Round( km, 2 ), 20 );

            await UpdateAndDeleteMessage( userRange );
         }

         private bool BadLocation( double latitude, double longitude )
         {
            var lansing = new GeoCoordinate( 42.724484, -84.5159882 );
            var uCoord = new GeoCoordinate( latitude, longitude );
            var distance = lansing.GetDistanceTo( uCoord ) / 1000.0; //meters to km
            if( distance > 40 )
               return true;

            return false;
         }

         private async Task SendDmSuggestion( string message )
         {
            await Context.Message.DeleteAsync();
            await Context.User.SendMessageAsync( message );
         }

         private Models.Range GetUserRange( bool creatMissing = false )
         {
            var userRange = _repo.GetByUserID( Context.User.Id, Context.Guild.Id );
            if( userRange == null && creatMissing )
            {
               userRange = new Models.Range()
               {
                  UserId = Context.User.Id.ToString(),
                  ServerId = Context.Guild.Id.ToString(),
                  Latitude = 0,
                  Longitude = 0,
                  Mentions = false,
                  CreatedOn = DateTime.Now,
                  ModifiedOn = DateTime.Now,
                  Pokemon = false,
                  Raids = false,
                  Rares = true,
                  Starters = false,
                  RaidLevels = "1,2,3,4,5",
                  Active = true,
                  RangeKm = 2
               };
            }

            return userRange;
         }

         [Command( "show" )]
         [Summary( "Show current user GPS location for range" )]
         public async Task Show()
         {
            var userRange = GetUserRange();
            if( userRange == null )
            {
               await Context.Message.DeleteAsync();
               await ReplyAsync( "You must first use the 'gps' command to configure your range." );
               return;
            }

            await Context.Message.DeleteAsync();
            await Context.User.SendMessageAsync( BuildStatus(userRange) );
         }

         private string BuildStatus( Models.Range userRange )
         {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine( "**Username:** " + Context.User.Username );
            sb.AppendLine( "**Latitude:** " + userRange.Latitude );
            sb.AppendLine( "**Longitude:** " + userRange.Longitude );
            sb.AppendLine( "**Range:** " + userRange.RangeKm + "km" );
            sb.AppendLine( "**Status:** " + (userRange.Active ? "ON" : " OFF") );
            sb.AppendLine( "\n" );
            sb.AppendLine( "**--Selections--**" );
            sb.AppendLine( "**Rares:**" + (userRange.Rares ? " ON" : " OFF") );
            sb.AppendLine( "**Pokemon:**" + (userRange.Pokemon ? " ON" : " OFF") );
            sb.AppendLine( "**Starters:**" + (userRange.Starters ? " ON" : " OFF") );
            sb.AppendLine( "**Raids:**" + (userRange.Raids ? " ON" : " OFF") );
            sb.AppendLine( "**Raid Levels:** " + userRange.RaidLevels );

            return sb.ToString();
         }

         [Command( "set" )]
         [Summary( "Set current user range" )]
         public async Task Set( decimal rangeKm )
         {
            var userRange = GetUserRange();
            if( userRange == null )
            {
               await Context.Message.DeleteAsync();
               await ReplyAsync( "You must first use the 'gps' command to configure your range." );
               return;
            }

            userRange.RangeKm = Math.Min( Math.Round( rangeKm, 2 ), 20 );

            await UpdateAndDeleteMessage( userRange );
         }

         private async Task UpdateAndDeleteMessage( Models.Range userRange )
         {
            userRange.ModifiedOn = DateTime.Now;
            await Context.Message.DeleteAsync();
            _repo.Upsert( userRange );
         }

         [Command( "delete" )]
         [Summary( "Delete current user GPS location for range" )]
         public async Task Delete()
         {
            var userRange = GetUserRange();
            if( userRange == null )
               return;

            await Context.Message.DeleteAsync();
            _repo.Delete( userRange.Id );
         }

         [Command( "rares" )]
         [Summary( "Turn on/off rares" )]
         public async Task Rares( string toggle )
         {
            await Toggle( "Rares", toggle );
         }

         private async Task Toggle( string toggleField, string toggle )
         {
            var userRange = GetUserRange();
            if( userRange == null )
            {
               await Context.Message.DeleteAsync();
               await ReplyAsync( "You must first use the 'gps' command to configure your range." );
               return;
            }

            if( !toggle.ToLower().Equals( "on" ) && !toggle.ToLower().Equals( "off" ) )
            {
               await Context.Message.DeleteAsync();
               await ReplyAsync( "Select a valid status option (on|off)" );
               return;
            }

            var newToggleVal = toggle.ToLower().Equals( "on" ) ? true : false;
            if( toggleField.Equals( "Rares" ) )
               userRange.Rares = newToggleVal;
            else if( toggleField.Equals( "Raids" ) )
               userRange.Raids = newToggleVal;
            else if( toggleField.Equals( "Pokemon" ) )
               userRange.Pokemon = newToggleVal;
            else if( toggleField.Equals( "Starters" ) )
               userRange.Starters = newToggleVal;
            else if( toggleField.Equals( "Active" ) )
               userRange.Active = newToggleVal;

            await UpdateAndDeleteMessage( userRange );
         }

         [Command( "pokemon" )]
         [Summary( "Turn on/off pokemon" )]
         public async Task Pokemon( string toggle )
         {
            await Toggle( "Pokemon", toggle );
         }

         [Command( "starters" )]
         [Summary( "Turn on/off starters" )]
         public async Task Starters( string toggle )
         {
            await Toggle( "Starters", toggle );
         }

         [Command( "raids" )]
         [Summary( "Turn on/off raids" )]
         public async Task Raids( string toggle )
         {
            await Toggle( "Raids", toggle );
         }

         [Command( "raidlevels" )]
         [Summary( "Set raid levels to be alerted to" )]
         public async Task RaidLevels( params int[] levels )
         {
            var userRange = GetUserRange();
            if( userRange == null )
            {
               await Context.Message.DeleteAsync();
               await ReplyAsync( "You must first use the 'gps' command to configure your range." );
               return;
            }

            var sb = new StringBuilder();
            foreach( var raidLevel in levels )
            {
               if( raidLevel < 0 || raidLevel > 5 )
                  continue;

               if( sb.Length > 0 )
                  sb.Append( "," );

               sb.Append( raidLevel.ToString() );
            }

            userRange.RaidLevels = sb.ToString();

            await UpdateAndDeleteMessage( userRange );
         }
      }
   }
}
