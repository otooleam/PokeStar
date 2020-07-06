using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using PokeStar.DataModels;

namespace PokeStar.ConnectionInterface
{
   public class DatabaseConnector
   {
      public string ConnectionString { get; set; }
      private readonly int TRUE = 1;
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
                                    WHERE name='{raidBossName}';";
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

      public Pokemon GetPokemon(string pokemonName)
      {
         Pokemon pokemon = null;
         using (var conn = GetConnection())
         {
            conn.Open();
            string queryString = $@"SELECT *
                                    FROM pokemon 
                                    WHERE name='{pokemonName}';";
            using (var reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  pokemon = new Pokemon
                  {
                     Number = Convert.ToInt32(reader["number"]),
                     Name = Convert.ToString(reader["name"]),
                     Description = Convert.ToString(reader["description"]),
                     Attack = Convert.ToInt32(reader["attack"]),
                     Defense = Convert.ToInt32(reader["defense"]),
                     Stamina = Convert.ToInt32(reader["stamina"]),
                     Region = Convert.ToString(reader["region"]),
                     Category = Convert.ToString(reader["category"]),
                     BuddyDistance = Convert.ToInt32(reader["buddy_distance"]),
                     Shadow = Convert.ToInt32(reader["shadow"]) == TRUE,
                     Shiny = Convert.ToInt32(reader["shiny"]) == TRUE,
                     Obtainable = Convert.ToInt32(reader["obtainable"]) == TRUE,
                     Regional = Convert.ToInt32(reader["obtainable"]) == TRUE
                  };

                  pokemon.Type.Add(Convert.ToString(reader["type_1"]));
                  if (reader["type_2"].GetType() != typeof(DBNull))
                     pokemon.Type.Add(Convert.ToString(reader["type_2"]));
               }
            }
            conn.Close();
         }
         return pokemon;
      }

      public List<string> GetWeather(List<string> types)
      {
         List<string> weather = new List<string>();
         using (var conn = GetConnection())
         {
            conn.Open();
            
            string queryString = $@"SELECT weather 
                                 FROM weather 
                                 WHERE {GetTypeWhere(types, "type")};";
            using (var reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
                  weather.Add(Convert.ToString(reader["weather"]));
            }
            conn.Close();
         }
         return weather;
      }

      public List<string> GetTypeRelations(List<string> types, bool weaknesses = true)
      {
         List<string> relations = new List<string>();
         using (var conn = GetConnection())
         {
            conn.Open();
            string queryString = $@"SELECT attacker, SUM(modifier) AS total_relation 
                                    FROM (
                                    SELECT attacker, modifier
                                    FROM type_match_up
                                    WHERE {GetTypeWhere(types, "defender")}
                                    ) AS relations
                                    GROUP BY attacker
                                    HAVING SUM(modifier) <> 0
                                    ORDER BY total_relation;";
            using (var reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  int modifier = Convert.ToInt32(reader["total_relation"]);
                  if ((weaknesses && modifier > 0) || !weaknesses && modifier < 0)
                     relations.Add(Convert.ToString(reader["attacker"]));
               }
            }
            conn.Close();
         }
         return relations;
      }

      public List<Move> GetMoves(string pokemonName, bool fast = true)
      {
         List<Move> moves = new List<Move>();
         using (var conn = GetConnection())
         {
            conn.Open();
            string moveType = fast ? "Fast" : "Charge";
            string queryString = $@"SELECT name, type, is_legacy
                                    FROM pokemon_move
                                    Inner JOIN move 
                                    ON pokemon_move.move=move.name
                                    WHERE pokemon='{pokemonName}'
                                    AND category='{moveType}';";
            using (var reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  var move = new Move
                  {
                     Name = Convert.ToString(reader["name"]),
                     Type = Convert.ToString(reader["type"]),
                     IsLegacy = Convert.ToInt32(reader["is_legacy"]) == TRUE
                  };
                  moves.Add(move);
               }
            }
            conn.Close();
         }
         return moves;
      }

      private static string GetTypeWhere(List<string> types, string variable)
      {
         string where = $@"({variable}='{types[0]}'";
         if (types.Count == 2)
            where += $@" or {variable}='{types[1]}'";
         where += ")";
         return where;
      }
   }
}
