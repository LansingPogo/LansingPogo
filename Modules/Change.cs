using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace Bot.Modules
{
   [Name( "Change" )]
   public class Change : ModuleBase<SocketCommandContext>
   {
      [Group( "change" ), Name( "Change" )]
      public class Range : ModuleBase
      {
         [Command( "day" )]
         [Summary( "Set mentions to day mode" )]
         public async Task Day()
         {
            await ReplyAsync( "Change Day Coming Soon!" );
         }

         [Command( "night" )]
         [Summary( "Set mentions to night mode" )]
         public async Task Night()
         {
            await ReplyAsync( "Change Night Coming Soon!" );
         }
      }
   }
}
