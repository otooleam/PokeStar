using System.Data.SqlClient;

namespace PokeStar.ConnectionInterface
{
   public abstract class DatabaseConnector
   {
      private string ConnectionString { get; set; }
      protected readonly int TRUE = 1;
      public DatabaseConnector(string connectionString)
      {
         ConnectionString = connectionString;
      }

      protected SqlConnection GetConnection()
      {
         return new SqlConnection(ConnectionString);
      }
   }
}
