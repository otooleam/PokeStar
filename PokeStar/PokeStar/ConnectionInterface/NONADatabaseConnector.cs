using System;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace PokeStar.ConnectionInterface
{
   /// <summary>
   /// Connects to the NONA database.
   /// </summary>
   public class NONADatabaseConnector : DatabaseConnector
   {
      /// <summary>
      /// Creates a new NONA database connector.
      /// </summary>
      /// <param name="connectionString">Connections string for the NONA database.</param>
      public NONADatabaseConnector(string connectionString) : base(connectionString) { }

      // Guild Settings *******************************************************

      /// <summary>
      /// Gets the prefix of a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <returns>The guild prefix.</returns>
      public string GetPrefix(ulong guild)
      {
         string prefix = null;
         string queryString = $@"SELECT prefix 
                                 FROM guild_settings 
                                 WHERE guild={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  prefix = Convert.ToString(reader["prefix"]);
               }
            }
            conn.Close();
         }
         return prefix;
      }

      /// <summary>
      /// Checks if setup has been completed for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <returns>True if setup has been complete, otherwise false.</returns>
      public bool GetSetupComplete(ulong guild)
      {
         bool setupComplete = false;
         string queryString = $@"SELECT setup 
                                 FROM guild_settings 
                                 WHERE guild={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  setupComplete = Convert.ToInt32(reader["setup"]) == TRUE;
               }
            }
            conn.Close();
         }
         return setupComplete;
      }

      /// <summary>
      /// Adds the guild with default settings.
      /// Default is default prefix and setup is incomplete.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      public void AddSettings(ulong guild)
      {
         string queryString = $@"INSERT INTO guild_settings (guild, prefix, setup)
                                 VALUES ({guild}, '{Global.DEFAULT_PREFIX}', 0)";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Updates the prefix of a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="prefix">New prefix value.</param>
      public void UpdatePrefix(ulong guild, string prefix)
      {
         string queryString = $@"UPDATE guild_settings 
                                 SET prefix = '{prefix}'
                                 WHERE guild={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Mark setup as completed for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      public void CompleteSetup(ulong guild)
      {
         string queryString = $@"UPDATE guild_settings 
                                 SET setup = 1
                                 WHERE guild={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Deletes the settings saved for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      public void DeleteSettings(ulong guild)
      {
         string queryString = $@"DELETE FROM guild_settings
                                 WHERE guild={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      // Channel Registration *************************************************

      /// <summary>
      /// Gets the registration for a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel.</param>
      /// <returns>The channel registration string, otherwise null.</returns>
      public string GetRegistration(ulong guild, ulong channel)
      {
         string registration = null;
         string queryString = $@"SELECT register 
                                 FROM channel_registration 
                                 WHERE guild={guild}
                                 AND channel={channel};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  if (reader["register"].GetType() != typeof(DBNull))
                  {
                     registration = Convert.ToString(reader["register"]);
                  }
               }
            }
            conn.Close();

         }
         return registration;
      }

      /// <summary>
      /// Adds a new registration to a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel.</param>
      /// <param name="registration">New registration value.</param>
      public void AddRegistration(ulong guild, ulong channel, string registration)
      {
         string queryString = $@"INSERT INTO channel_registration (guild, channel, register)
                                 VALUES ({guild}, {channel}, '{registration}')";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Updates the registration for a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel.</param>
      /// <param name="registration">New registration value.</param>
      public void UpdateRegistration(ulong guild, ulong channel, string registration)
      {
         string queryString = $@"UPDATE channel_registration 
                                 SET register = '{registration}'
                                 WHERE guild={guild}
                                 AND channel={channel};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Deletes the registration for all channels in a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      public void DeleteAllRegistration(ulong guild)
      {
         string queryString = $@"DELETE FROM channel_registration
                                 WHERE guild={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Deletes the registration for a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel.</param>
      public void DeleteRegistration(ulong guild, ulong channel)
      {
         string queryString = $@"DELETE FROM channel_registration
                                 WHERE guild={guild}
                                 AND channel={channel};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      // Nicknames ************************************************************

      /// <summary>
      /// Get a Pokémon by nickname.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname of the Pokémon.</param>
      /// <returns>Name of the Pokémon with the nickname, otherwiase null.</returns>
      public string GetPokemon(ulong guild, string nickname)
      {
         string prefix = null;
         string queryString = $@"SELECT name 
                                 FROM nickname 
                                 WHERE guild={guild}
                                 AND nickname = '{nickname}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  prefix = Convert.ToString(reader["name"]);
               }
            }
            conn.Close();
         }
         return prefix;
      }

      /// <summary>
      /// Gets all nicknames for a Pokémon in a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="pokemon">Name of the Pokémon.</param>
      /// <returns>List of all nicknames for the Pokémon in the guild.</returns>
      public List<string> GetNicknames(ulong guild, string pokemon)
      {
         List<string> nicknames = new List<string>();
         string queryString = $@"SELECT nickname 
                                 FROM nickname 
                                 WHERE guild={guild}
                                 AND name = '{pokemon}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  nicknames.Add(Convert.ToString(reader["nickname"]));
               }
            }
            conn.Close();
         }
         return nicknames;
      }

      /// <summary>
      /// Adds a nickname to a Pokémon for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="nickname">Nickname of the Pokémon.</param>
      public void AddNickname(ulong guild, string pokemonName, string nickname)
      {
         string queryString = $@"INSERT INTO nickname (guild, name, nickname)
                                 VALUES ({guild}, '{pokemonName}', '{nickname}')";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Updates a nickname for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="newNickname">New nickname.</param>
      /// <param name="originalNickname">Old nickname.</param>
      public void UpdateNickname(ulong guild, string newNickname, string originalNickname)
      {
         string queryString = $@"UPDATE nickname 
                                 SET nickname = '{newNickname}'
                                 WHERE guild={guild}
                                 AND nickname = '{originalNickname}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Delete a nickname from a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname of a Pokémon.</param>
      public void DeleteNickname(ulong guild, string nickname)
      {
         string queryString = $@"DELETE FROM nickname
                                 WHERE guild={guild}
                                 AND nickname='{nickname}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }
   }
}