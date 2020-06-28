using System;
using System.Data.SqlClient;
using PokeStar.DataModels;

namespace PokeStar.ConnectionInterface
{
   public class DatabaseConnector
   {
      public string ConnectionString { get; set; }
      public DatabaseConnector(string connectionString)
      {
         ConnectionString = connectionString;
      }

      private SqlConnection GetConnection()
      {
         return new SqlConnection(ConnectionString);
      }

      public RaidBoss GetRaidBoss(string raidBossName)
      {
         RaidBoss raidBoss = null;
         using (var conn = GetConnection())
         {
            conn.Open();
            string queryString = $@"SELECT name, attack, defense, stamina, type_1, type_2
                                    FROM pokemon 
                                    WHERE name='{raidBossName}';
                                   ";
            using (var reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  raidBoss = new RaidBoss
                  {
                     Name = Convert.ToString(reader["name"]),
                     Attack = Convert.ToInt32(reader["attack"]),
                     Defense = Convert.ToInt32(reader["defense"]),
                     Stamina = Convert.ToInt32(reader["stamina"]),
                  };

                  raidBoss.Type.Add(Convert.ToString(reader["type_1"]));

                  if (reader["type_2"].GetType() != typeof(DBNull))
                     raidBoss.Type.Add(Convert.ToString(reader["type_2"]));
               }
            }
            conn.Close();
         }
         return raidBoss;
      }

      public void GetRaidBossWeather(ref RaidBoss raidboss)
      {
         using (var conn = GetConnection())
         {
            conn.Open();
            foreach (string type in raidboss.Type)
            {
               string queryString = $@"SELECT weather 
                                    from weather 
                                    WHERE type='{type}';
                                   ";
               using (var reader = new SqlCommand(queryString, conn).ExecuteReader())
               {
                  while (reader.Read())
                     raidboss.Weather.Add(Convert.ToString(reader["weather"]));
               }
            }
            conn.Close();
         }
      }

   }
}
