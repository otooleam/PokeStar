using System;
using System.Text;
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
      private readonly List<Move> ShadowMoves = new List<Move>()
      {
         new Move
         {
            Name = "Frustration",
            Type = "Normal",
            IsLegacy = false
         },
         new Move
         {
            Name = "Return",
            Type = "Normal",
            IsLegacy = false
         }
      };


      /// <summary>
      /// Creates a new POGO database connector.
      /// </summary>
      /// <param name="connectionString">Connection string for the POGO database.</param>
      public POGODatabaseConnector(string connectionString) : base(connectionString) { }

      /// <summary>
      /// Gets a list of all Pokémon names.
      /// </summary>
      /// <returns>List of Pokémon names.</returns>
      public List<string> GetNameList()
      {
         List<string> names = new List<string>();
         using (SqlConnection conn = GetConnection())
         {
            string queryString = $@"select name from pokemon order by number;";
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  names.Add(Convert.ToString(reader["name"]));
               }
            }
            conn.Close();
         }
         return names;
      }

      /// <summary>
      /// Gets a raid boss by name.
      /// </summary>
      /// <param name="raidBossName">Name of the raid boss.</param>
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
      /// Gets a Pokémon by name.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>A Pokémon if the name is in the database, otherwise null.</returns>
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
      /// Gets a list of Pokémon by number
      /// </summary>
      /// <param name="pokemonNumber">Number of the Pokémon.</param>
      /// <returns>List of all Pokémon with the number.</returns>
      public List<string> GetPokemonByNumber(int pokemonNumber)
      {
         List<string> pokemon = new List<string>();
         string order = (pokemonNumber == Global.ARCEUS_NUMBER || pokemonNumber == Global.UNOWN_NUMBER) ? "ORDER BY NEWID()" : "";

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
               {
                  pokemon.Add(Convert.ToString(reader["name"]));
               }
            }
            conn.Close();
         }
         return pokemon;
      }

      /// <summary>
      /// Gets all weather that boosts the given types.
      /// </summary>
      /// <param name="types">List of types.</param>
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
      /// Gets defensive type relations for a Pokémon's type.
      /// </summary>
      /// <param name="types">List of the Pokémon's types.</param>
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
      /// Gets moves of a Pokémon.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="fast">Is the type of move a fast move, else charge move.</param>
      /// <param name="shadowable">Is the Pokémon shadowable.</param>
      /// <returns>List of moves of the Pokémon.</returns>
      public List<Move> GetMoves(string pokemonName, bool fast = true, bool shadowable = false)
      {
         List<Move> moves = new List<Move>();
         string moveType = fast ? "Fast" : "Charge";

         var index = pokemonName.IndexOf(' ');
         var name = pokemonName;
         if (index != -1 && pokemonName.Substring(0, index).Equals("mega", StringComparison.OrdinalIgnoreCase))
         {
            name = pokemonName.Substring(index);
            if (pokemonName.Split(' ').Length == 3)
            {
               name = name.TrimEnd(name[name.Length - 1]);
            }
         }

         string queryString = $@"SELECT name, type, is_legacy
                                 FROM pokemon_move
                                 INNER JOIN move 
                                 ON pokemon_move.move=move.name
                                 WHERE pokemon='{name.Trim()}'
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
            moves.AddRange(ShadowMoves);
         }
         return moves;
      }

      /// <summary>
      /// Gets the top counters of a Pokémon.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>List of counters to a Pokémon.</returns>
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
      /// Gets a move that a Pokémon can learn.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
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
      /// Get all evolutions a Pokemon is part of.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>List of evolutions the Pokémon is part of.</returns>
      public List<Evolution> GetEvolutions(string pokemonName)
      {
         List<Evolution> evolutions = new List<Evolution>();
         string queryString = $@"SELECT * 
                                 FROM evolution 
                                 WHERE start_pokemon = '{pokemonName}' 
                                 OR end_pokemon = '{pokemonName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  evolutions.Add(new Evolution
                  {
                     Start = Convert.ToString(reader["start_pokemon"]),
                     End = Convert.ToString(reader["end_pokemon"]),
                     Candy = Convert.ToInt32(reader["candy"]),
                     Item = (reader["item"].GetType() == typeof(DBNull)) ? null : Convert.ToString(reader["item"])
                  });
               }
            }
            conn.Close();
         }
         return evolutions;
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