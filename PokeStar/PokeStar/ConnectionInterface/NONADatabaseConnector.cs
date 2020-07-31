using System;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace PokeStar.ConnectionInterface
{
   public class NONADatabaseConnector : DatabaseConnector
   {
      public NONADatabaseConnector(string connectionString) : base(connectionString) { }
   }
}
