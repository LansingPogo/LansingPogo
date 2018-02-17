using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Bot.SpawnRouting;
using Microsoft.Extensions.Configuration;

namespace Bot.Services
{
   public class PokemonMessageHandler
   {
      private readonly DiscordSocketClient _discord;
      private readonly CommandService _commands;
      private readonly IConfigurationRoot _config;
      private readonly IServiceProvider _provider;
      private readonly SpawnMessageRouter _router;

      // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
      public PokemonMessageHandler(
          DiscordSocketClient discord,
          CommandService commands,
          IConfigurationRoot config,
          IServiceProvider provider,
          SpawnRouting.SpawnMessageRouter router )
      {
         _discord = discord;
         _commands = commands;
         _config = config;
         _provider = provider;
         _router = router;

         _discord.MessageReceived += OnMessageReceivedAsync;
      }

      private async Task OnMessageReceivedAsync( SocketMessage s )
      {
         var msg = s as SocketUserMessage;     // Ensure the message is from a user/bot
         if( msg == null )
            return;
         if( msg.Author.Id == _discord.CurrentUser.Id )
            return;     // Ignore self when checking commands

         await _router.RouteMessage( msg );
         
      }
   }
}
