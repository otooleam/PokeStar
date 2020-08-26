using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using PokeStar.DataModels;

namespace PokeStar.ConnectionInterface
{
   /// <summary>
   /// Connects to the POGO database.
   /// </summary>
   public class POGODatabaseConnector : DatabaseConnector
   {
      /// <summary>
      /// Creates a new POGO database connector.
      /// </summary>
      /// <param name="connectionString">Connection string for the POGO database.</param>
      public POGODatabaseConnector(string connectionString) : base(connectionString) { }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public List<string> GetNameList()
      {
         List<string> names = new List<string>();
         using (var conn = GetConnection())
         {
            string queryString = $@"select name from pokemon order by number;";
            conn.Open();
            using (var reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
                  names.Add(Convert.ToString(reader["name"]));
            }
            conn.Close();
         }
         return names;
      }


      /// <summary>
      /// Gets a raid boss given it's name.
      /// </summary>
      /// <param name="raidBossName">The name of the raid boss.</param>
      /// <returns>A raid boss if the name is in the database, otherwise null.</returns>
      public RaidBoss GetRaidBoss(string raidBossName)
      {
         RaidBoss raidBoss = null;
         string queryString = $@"SELECT name, attack, defense, stamina, type_1, type_2
                                 FROM pokemon 
                                 WHERE name='{raidBossName}';";

         using (var conn = GetConnection())
         {
            conn.Open();
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

      /// <summary>
      /// Gets a pokemon given it's name.
      /// </summary>
      /// <param name="pokemonName">Name of the pokemon.</param>
      /// <returns>A pokemon if the name is in the database, otherwise null.</returns>
      public Pokemon GetPokemon(string pokemonName)
      {
         Pokemon pokemon = null;
         string queryString = $@"SELECT *
                                 FROM pokemon 
                                 WHERE name='{pokemonName}';";

         using (var conn = GetConnection())
         {
            conn.Open();
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
                     Regional = Convert.ToInt32(reader["regional"]) == TRUE
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

      /// <summary>
      /// Gets pokemon with similar names to a string.
      /// </summary>
      /// <param name="str">String to compare to pokemon names.</param>
      /// <returns>List of 10 closest pokemon names.</returns>
      public List<string> GetSimilarPokemon(string str)
      {
         List<string> pokemon = new List<string>();
         string queryString = $@"SELECT TOP 10 name 
                                 FROM pokemon 
                                 WHERE name LIKE '%{str}%'
                                 ORDER BY DIFFERENCE(name, '{str}') DESC;";

         using (var conn = GetConnection())
         {
            conn.Open();
            using (var reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
                  pokemon.Add(Convert.ToString(reader["name"]));
            }
            conn.Close();
         }
         return pokemon;
      }

      /// <summary>
      /// Gets all weather that boosts the given types.
      /// </summary>
      /// <param name="types">List of types to get weather for.</param>
      /// <returns>List of weather that boosts the givent types.</returns>
      public List<string> GetWeather(List<string> types)
      {
         List<string> weather = new List<string>();
         string queryString = $@"SELECT weather 
                                 FROM weather 
                                 WHERE {GetTypeWhere(types, "type")}
                                 GROUP BY weather;";

         using (var conn = GetConnection())
         {
            conn.Open();
            using (var reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
                  weather.Add(Convert.ToString(reader["weather"]));
            }
            conn.Close();
         }
         return weather;
      }

      /// <summary>
      /// Gets defensive type relations for a pokemon's type.
      /// </summary>
      /// <param name="types">List of pokemon types.</param>
      /// <returns>Dictionary of types and modifiers.</returns>
      public Dictionary<string, int> GetTypeDefenseRelations(List<string> types)
      {
         Dictionary<string, int> relations = new Dictionary<string, int>();
         string queryString = $@"SELECT attacker, SUM(modifier) AS total_relation 
                                 FROM (
                                 SELECT attacker, modifier
                                 FROM type_match_up
                                 WHERE {GetTypeWhere(types, "defender")}
                                 ) AS relations
                                 GROUP BY attacker
                                 HAVING SUM(modifier) <> 0
                                 ORDER BY total_relation;";

         using (var conn = GetConnection())
         {
            conn.Open();
            using (var reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
                  relations.Add(Convert.ToString(reader["attacker"]), Convert.ToInt32(reader["total_relation"]));
            }
            conn.Close();
         }
         return relations;
      }

      /// <summary>
      /// Gets offensive type relations for a move's type.
      /// </summary>
      /// <param name="type">Move type.</param>
      /// <returns>Dictionary of types and modifiers.</returns>
      public Dictionary<string, int> GetTypeAttackRelations(string type)
      {
         Dictionary<string, int> relations = new Dictionary<string, int>();
         string queryString = $@"SELECT defender, modifier
                                 FROM type_match_up
                                 WHERE attacker = '{type}'
                                 ORDER BY modifier;";

         using (var conn = GetConnection())
         {
            conn.Open();
            using (var reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
                  relations.Add(Convert.ToString(reader["defender"]), Convert.ToInt32(reader["modifier"]));
            }
            conn.Close();
         }
         return relations;
      }

      /// <summary>
      /// Gets moves of a pokemon.
      /// </summary>
      /// <param name="pokemonName">Name of the pokemon.</param>
      /// <param name="fast">Is the type of move a fast move, else charge move.</param>
      /// <param name="shadowable">Is the pokemon shadowable.</param>
      /// <returns>List of moves of the pokemon.</returns>
      public List<Move> GetMoves(string pokemonName, bool fast = true, bool shadowable = false)
      {
         List<Move> moves = new List<Move>();
         string moveType = fast ? "Fast" : "Charge";
         string queryString = $@"SELECT name, type, is_legacy
                                 FROM pokemon_move
                                 INNER JOIN move 
                                 ON pokemon_move.move=move.name
                                 WHERE pokemon='{pokemonName}'
                                 AND category='{moveType}';";

         using (var conn = GetConnection())
         {
            conn.Open();
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
         if (!fast && shadowable)
         {
            moves.Add(new Move
            {
               Name = "Frustration",
               Type = "Normal",
               IsLegacy = false
            });
            moves.Add(new Move
            {
               Name = "Return",
               Type = "Normal",
               IsLegacy = false
            });
         }
         return moves;
      }

      /// <summary>
      /// Generates the where string for types.
      /// </summary>
      /// <param name="types">List of pokemon types.</param>
      /// <param name="variable">Name of the variable to check.</param>
      /// <returns>SQL where string for pokemon types.</returns>
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