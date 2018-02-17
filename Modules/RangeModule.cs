using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Bot.Modules
{
   [Name( "Range" )]
   public class RangeModule : ModuleBase<SocketCommandContext>
   {
      [Group( "range" ), Name( "Range" )]
      public class Range : ModuleBase
      {
         private async Task PlaceholderText( string commandName )
         {
            await ReplyAsync( commandName + " coming soon!" );
         }

         [Command("help")]
         [Summary( "Help Info for range feature" )]
         public async Task Help()
         {
            await PlaceholderText( "Range help" );
         }

         [Command( "on" )]
         [Summary( "Turn on range feature" )]
         public async Task On()
         {
            await PlaceholderText( "Range on");
         }

         [Command( "off" )]
         [Summary( "Turn off range feature" )]
         public async Task Off()
         {
            await PlaceholderText( "Range off" );
         }

         [Command( "examples" )]
         [Summary( "Examples for range feature" )]
         public async Task Examples()
         {
            await PlaceholderText( "Range examples" );
         }

         [Command( "gps" )]
         [Summary( "Set current user GPS location for range" )]
         public async Task Gps( decimal latitude, decimal longitude, decimal km )
         {
            await PlaceholderText( "Range gps" );
         }

         [Command( "show" )]
         [Summary( "Show current user GPS location for range" )]
         public async Task Show()
         {
            await PlaceholderText( "Range show" );
         }

         [Command( "set" )]
         [Summary( "Set current user range" )]
         public async Task Set()
         {
            await PlaceholderText( "Range set" );
         }

         [Command( "delete" )]
         [Summary( "Delete current user GPS location for range" )]
         public async Task Delete()
         {
            await PlaceholderText( "Range delete" );
         }

         [Command( "rares" )]
         [Summary( "Turn on/off rares" )]
         public async Task Rares( string toggle )
         {
            await PlaceholderText( "Range rares" );
         }

         [Command( "pokemon" )]
         [Summary( "Turn on/off pokemon" )]
         public async Task Pokemon( string toggle )
         {
            await PlaceholderText( "Range pokemon" );
         }

         [Command( "starters" )]
         [Summary( "Turn on/off starters" )]
         public async Task Starters( string toggle )
         {
            await PlaceholderText( "Range starters" );
         }

         [Command( "raids" )]
         [Summary( "Turn on/off raids" )]
         public async Task Raids( string toggle )
         {
            await PlaceholderText( "Range raids" );
         }

         [Command( "raidlevels" )]
         [Summary( "Set raid levels to be alerted to" )]
         public async Task RaidLevels( params int[] levels )
         {
            await PlaceholderText( "Range Raid Levels" );
         }
      }
   }
}
