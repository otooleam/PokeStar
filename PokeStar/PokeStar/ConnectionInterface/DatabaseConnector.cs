using System.Data.SqlClient;

namespace PokeStar.ConnectionInterface
{
   /// <summary>
   /// Connects to a database.
   /// </summary>
   public abstract class DatabaseConnector
   {
      private string ConnectionString { get; set; }
      

      /// <summary>
      /// Creates a new DatabaseConnector.
      /// </summary>
      /// <param name="connectionString">Connection string for the database.</param>
      public DatabaseConnector(string connectionString)
      {
         ConnectionString = connectionString;
      }

      /// <summary>
      /// Creates an SQL connection to the database.
      /// </summary>
      /// <returns>An SQL connection.</returns>
      protected SqlConnection GetConnection()
      {
         return new SqlConnection(ConnectionString);
      }
   }
}