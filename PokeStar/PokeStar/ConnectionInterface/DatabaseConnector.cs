using System.Data.SqlClient;

namespace PokeStar.ConnectionInterface
{
   /// <summary>
   /// Connects to a database.
   /// </summary>
   public abstract class DatabaseConnector
   {
      private string ConnectionString { get; set; }

      protected const int TRUE = 1;
      protected const int FALSE = 0;

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