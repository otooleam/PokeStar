using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using PokeStar.DataModels;
using System.Text;
using PokeStar.Modules;

namespace PokeStar.ConnectionInterface
{
   /// <summary>
   /// Connects to the POGO database.
   /// </summary>
   public class POGODatabaseConnector : DatabaseConnector
   {
      private readonly int TRUE = 1;

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

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
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
                  {
                     raidBoss.Type.Add(Convert.ToString(reader["type_2"]));
                  }
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

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
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
                     Regional = (reader["regional"].GetType() == typeof(DBNull))? null :Convert.ToString(reader["regional"])
                  };
                  pokemon.Type.Add(Convert.ToString(reader["type_1"]));
                  if (reader["type_2"].GetType() != typeof(DBNull))
                  {
                     pokemon.Type.Add(Convert.ToString(reader["type_2"]));
                  }
               }
            }
            conn.Close();
         }
         return pokemon;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="pokemonNumber"></param>
      /// <returns></returns>
      public List<string> GetPokemonByNumber(int pokemonNumber)
      {
         List<string> pokemon = new List<string>();
         string order = (pokemonNumber == DexCommands.ARCEUS || pokemonNumber == DexCommands.UNOWN) ? "ORDER BY NEWID()" : "";

         string queryString = $@"SELECT name 
                                 FROM pokemon 
                                 WHERE number={pokemonNumber}
                                 {order};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
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

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  if (!weather.Contains(Convert.ToString(reader["weather"])))
                  {
                     weather.Add(Convert.ToString(reader["weather"]));
                  }
               }
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

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  relations.Add(Convert.ToString(reader["attacker"]), Convert.ToInt32(reader["total_relation"]));
               }
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

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  relations.Add(Convert.ToString(reader["defender"]), Convert.ToInt32(reader["modifier"]));
               }
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

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  Move move = new Move
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
      /// Gets the top counters of a pokemon.
      /// </summary>
      /// <param name="pokemonName">Pokemon to get counters for.</param>
      /// <returns>List of counters to a pokemon.</returns>
      public List<Counter> GetCounters(string pokemonName)
      {
         List<Counter> counters = new List<Counter>();
         int numCounters = 6;
         string queryString = $@"SELECT TOP {numCounters} counter, type_1, type_2, fastAttack, chargeAttack
                                 FROM pokemon_counter 
                                 INNER JOIN pokemon
                                 ON pokemon_counter.counter = pokemon.name
                                 WHERE pokemon = '{pokemonName}' 
                                 AND obtainable = 1
                                 ORDER BY rank;";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  Counter counter = new Counter
                  {
                     Name = Convert.ToString(reader["counter"]),
                     FastAttack = new Move { Name = Convert.ToString(reader["fastAttack"]) },
                     ChargeAttack = new Move { Name = Convert.ToString(reader["chargeAttack"]) },
                  };
                  counters.Add(counter);
               }
            }
            conn.Close();
         }
         return counters;
      }

      /// <summary>
      /// Gets a move that a pokemon can learn.
      /// Returns null if pokemon cannot learn the move.
      /// </summary>
      /// <param name="pokemonName">Pokemon to get move for.</param>
      /// <param name="moveName">Name of the move.</param>
      /// <returns>Move that the pokemon can learn, otherwise null.</returns>
      public Move GetPokemonMove(string pokemonName, string moveName)
      {
         Move move = null;
         string queryString = $@"SELECT move, type, is_legacy
                                 FROM pokemon_move
                                 INNER JOIN move
                                 ON pokemon_move.move = move.name
                                 WHERE pokemon = '{pokemonName}' 
                                 AND move = '{moveName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  move = new Move
                  {
                     Name = Convert.ToString(reader["move"]),
                     Type = Convert.ToString(reader["type"]),
                     IsLegacy = Convert.ToInt32(reader["is_legacy"]) == TRUE,
                  };
               }
            }
            conn.Close();
         }
         return move;
      }

      /// <summary>
      /// Generates the where string for types.
      /// </summary>
      /// <param name="types">List of pokemon types.</param>
      /// <param name="variable">Name of the variable to check.</param>
      /// <returns>SQL where string for pokemon types.</returns>
      private static string GetTypeWhere(List<string> types, string variable)
      {
         StringBuilder sb = new StringBuilder();

         sb.Append($@"({variable}='{types[0]}'");
         if (types.Count == 2)
         {
            sb.Append($@" or {variable}='{types[1]}'");
         }
         sb.Append(')');
         return sb.ToString();
      }
   }
}