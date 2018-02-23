using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Bot.Models;
using Microsoft.Extensions.Configuration;
using NPoco;

namespace Bot.Repositories
{
   public class PokemonRepository
   {
      private readonly IConfigurationRoot _config;
      private readonly string connectionString;

      public PokemonRepository( IConfigurationRoot config )
      {
         _config = config;
         connectionString = _config["connectionstring"].ToString();
      }

      public IDatabase Connection
      {
         get
         {
            return new Database( connectionString, DatabaseType.SqlServer2012, SqlClientFactory.Instance );
         }
      }

      public IEnumerable<Pokemon> GetAll()
      {
         using( IDatabase db = Connection )
         {
            return db.Fetch<Pokemon>( "SELECT * FROM Pokemon" );
         }
      }

      public IEnumerable<string> GetPokemonNames()
      {
         using( IDatabase db = Connection )
         {
            return db.Fetch<string>( "SELECT Name FROM Pokemon" );
         }
      }

      public Pokemon GetByID( int pokemonId )
      {
         using( IDatabase db = Connection )
         {
            return db.SingleOrDefault<Pokemon>( "where PokemonId = @0", pokemonId );
         }
      }

      public Pokemon GetByName( string pokemonName )
      {
         using( IDatabase db = Connection )
         {
            return db.SingleOrDefault<Pokemon>( "where Name = @0", pokemonName );
         }
      }

      public IEnumerable<string> GetByGeneration( int generation )
      {
         using( IDatabase db = Connection )
         {
            return db.Fetch<string>( "SELECT Name FROM Pokemon where gen = @0", generation );
         }
      }

      //NOT ALLOWING FULL CRUD RIGHT NOW
      //public void Add( Pokemon pokemon )
      //{
      //   using( IDatabase db = Connection )
      //   {
      //      db.Insert<Pokemon>( pokemon );
      //   }
      //}

      //public void Delete( int id )
      //{
      //   using( IDatabase db = Connection )
      //   {
      //      db.Delete<Pokemon>( id );
      //   }
      //}

      //public void Update( Pokemon prod )
      //{
      //   using( IDatabase db = Connection )
      //   {
      //      db.Update( prod );
      //   }
      //}
   }
}
