using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Bot.Models;
using Microsoft.Extensions.Configuration;
using NPoco;

namespace Bot.Repositories
{
    public class RangeRepository
    {
      private readonly IConfigurationRoot _config;
      private readonly string connectionString;

      public RangeRepository( IConfigurationRoot config )
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

      public IEnumerable<Range> GetAll()
      {
         using( IDatabase db = Connection )
         {
            return db.Fetch<Range>( "SELECT * FROM Range" );
         }
      }

      public Range GetByID( int rangeId )
      {
         using( IDatabase db = Connection )
         {
            return db.SingleOrDefault<Range>( "where Id = @0", rangeId );
         }
      }

      public Range GetByUserID( ulong userId, ulong serverId )
      {
         using( IDatabase db = Connection )
         {
            return db.SingleOrDefault<Range>( "where UserId = @0 and ServerId = @1", userId.ToString(), serverId.ToString() );
         }
      }

      public IEnumerable<Range> GetByServerID( ulong serverId )
      {
         using( IDatabase db = Connection )
         {
            return db.Fetch<Range>( "where ServerID = @0", serverId.ToString() );
         }
      }

      public void Upsert( Range range )
      {
         using( IDatabase db = Connection )
         {
            db.Save<Range>( range );
         }
      }

      public void Add( Range range )
      {
         using( IDatabase db = Connection )
         {
            db.Insert<Range>( range );
         }
      }

      public void Delete( int id )
      {
         using( IDatabase db = Connection )
         {
            db.Delete<Range>( id );
         }
      }

      public void Update( Range range )
      {
         using( IDatabase db = Connection )
         {
            db.Update( range );
         }
      }
   }
}
