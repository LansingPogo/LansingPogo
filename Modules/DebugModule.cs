using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;

namespace Bot.Modules
{
#if DEBUG

   [Name( "Debug Module" )]
   public class DebugModule : ModuleBase<SocketCommandContext>
   {
      [Command( "debugfetch" )]
      [Summary( "Fetch the last message in a channel and re-deliver it" )]
      [RequireUserPermission( GuildPermission.Administrator )]
      public Task DebugFetch( [Remainder]string channelName )
            => ReDeliverMessage( channelName );

      private async Task ReDeliverMessage( string channelName )
      {
         foreach( var channel in Context.Guild.TextChannels )
         {
            if( channel.Name.ToLower().Equals( channelName ) )
            {
               var messages =  await channel.GetMessagesAsync( 1 ).Flatten();
               foreach( var item in messages )
               {
                  if( item.Embeds.Count > 0 )
                     await item.Channel.SendMessageAsync( "Re-Delivery Test", embed: (item.Embeds as IReadOnlyList<Embed>)[0] );
                  else
                     await item.Channel.SendMessageAsync( "Re-Delivery Test " + Environment.NewLine + item.Content );

               }
            }
         }
      }
   }
#else
#endif
}
