using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace Bot.Modules
{
   [Name("Help")]
   public class MiscHelpModule : ModuleBase<SocketCommandContext>
   {
      [Command( "help" )]
      public async Task Help()
      {
         await ReplyAsync( "Help Docs Coming Soon!" );
      }

      [Command( "howto" )]
      public async Task Howto()
      {
         await ReplyAsync( "Howto help coming soon!" );
      }

      [Command( "raid" )]
      public async Task Raid()
      {
         await ReplyAsync( "Raid help coming soon!" );
      }

      [Command( "pokemon" )]
      public async Task Pokemon()
      {
         await ReplyAsync( "Pokemon help coming soon!" );
      }
   }
}
