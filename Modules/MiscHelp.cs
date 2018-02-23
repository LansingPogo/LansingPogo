using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bot.Repositories;
using Discord.Commands;
using NPoco;

namespace Bot.Modules
{
   [Name( "Help" )]
   public class MiscHelpModule : ModuleBase<SocketCommandContext>
   {
      private readonly PokemonRepository _repo;
      private const string GAMEPRESS_SITE = @"https://pokemongo.gamepress.gg/pokemon/";

      public MiscHelpModule( PokemonRepository repo )
      {
         _repo = repo;
      }

      [Command( "help" )]
      public async Task Help()
      {
         //TODO:
         await ReplyAsync( "Help Docs Coming Soon!" );
      }

      [Command( "howto" )]
      public async Task Howto()
      {
         //TODO:
         await ReplyAsync( "Howto help coming soon!" );
      }

      [Command( "gp" )]
      public async Task GamePress( string pokemonName )
      {
         var pokemon = _repo.GetByName( pokemonName );
         await SendPokemonMessage( pokemon );
      }

      [Command( "gen" )]
      public async Task Generation( string pokemonName )
      {
         var pokemon = _repo.GetByName( pokemonName );
         if( pokemon == null )
            await ReplyAsync( "Pokemon not found!" );
         else
            await ReplyAsync( String.Format( "{0} is generation {1}.", pokemonName, pokemon.Gen ) );
      }

      [Command( "gen" )]
      public async Task Generation( int generation )
      {
         var pokemon = _repo.GetByGeneration( generation );
         if( pokemon == null )
            await ReplyAsync( "Pokemon geration not found!" );
         else
         {
            var pokemonList = String.Join( Environment.NewLine, pokemon )
;
            await ReplyAsync( String.Format( "Pokemon in generation {0}:{1}{2}",
               generation, Environment.NewLine, pokemonList ) );
         }
      }

      [Command( "gp" )]
      public async Task GamePress( int pokemonId )
      {
         var pokemon = _repo.GetByID( pokemonId );
         await SendPokemonMessage( pokemon );
      }

      private async Task SendPokemonMessage( Models.Pokemon pokemon )
      {
         if( pokemon == null )
            await ReplyAsync( "Unknown Pokemon!" );
         else
            await ReplyAsync( String.Format( "{0}{1}", GAMEPRESS_SITE, pokemon.PokemonId ) );
      }
   }
}
