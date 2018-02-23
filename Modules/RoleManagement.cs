using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bot.Repositories;
using Discord;
using Discord.Commands;

namespace Bot.Modules
{
   [Name( "Roles" )]
   public class RoleManagement : ModuleBase<SocketCommandContext>
   {
      private readonly PokemonRepository _repo;
      public RoleManagement( PokemonRepository repo )
      {
         _repo = repo;
      }

      [Command( "rm" )]
      public async Task Raid()
      {
         List<string> raidMentions = new List<string>();

         var allPokemon = new List<string>();
         allPokemon.AddRange( _repo.GetPokemonNames() );
         allPokemon = allPokemon.ConvertAll( p => p.ToLower() );

         var roles = Context.Guild.Roles;

         foreach( var role in roles )
         {
            if( !role.Name.EndsWith( "-r" ) )
               continue;

            if( allPokemon.Contains( role.Name.ToLower().Replace( "-r", "" ) ) )
               raidMentions.Add( role.Name );
         }

         var message = String.Format( "Raid Mentions List:{0}{1}", Environment.NewLine, String.Join( Environment.NewLine, raidMentions ) );
         await ReplyAsync( message );
      }

      [Command( "pm" )]
      public async Task Pokemon()
      {
         List<string> mentions = new List<string>();

         var allPokemon = new List<string>();
         allPokemon.AddRange( _repo.GetPokemonNames() );
         allPokemon = allPokemon.ConvertAll( p => p.ToLower() );

         var roles = Context.Guild.Roles;

         foreach( var role in roles )
         {
            if( allPokemon.Contains( role.Name.ToLower() ) )
               mentions.Add( role.Name );
         }

         var message = String.Format( "Pokemon Mentions List:{0}{1}", Environment.NewLine, String.Join( Environment.NewLine, mentions ) );
         await ReplyAsync( message );
      }

      [Command( "checkroles" )]
      [RequireUserPermission( GuildPermission.Administrator )]
      public async Task CheckRoles()
      {
         List<string> nonpokemonroles = new List<string>();

         var allPokemon = new List<string>();
         allPokemon.AddRange( _repo.GetPokemonNames() );
         allPokemon = allPokemon.ConvertAll( p => p.ToLower() );

         var roles = Context.Guild.Roles;

         foreach( var role in roles )
         {
            if( allPokemon.Contains( role.Name.ToLower() ) )
               continue;
            if( allPokemon.Contains( role.Name.ToLower().Replace( "-r", "" ) ) )
               continue;

            if( role.IsEveryone )
               continue;

            nonpokemonroles.Add( role.Name );
         }

         var message = String.Format( "Non-Pokemon Roles:{0}{1}", Environment.NewLine, String.Join( Environment.NewLine, nonpokemonroles ) );
         await ReplyAsync( message );
      }

      [Command( "myroles" )]
      public async Task MyRoles()
      {
         StringBuilder sb = new StringBuilder();
         foreach( var role in this.Context.Guild.GetUser( Context.User.Id ).Roles )
         {
            if( sb.Length > 0 )
               sb.Append( ", " );

            if( role.IsEveryone )
               continue;

            sb.AppendFormat( "[{0}]", role.Name );
         }

         var roleString = sb.ToString();

         await ReplyAsync( "Current roles: " + sb.ToString() );
      }
   }
}

