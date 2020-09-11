using System;
using System.Collections.Generic;
using System.Data.SqlClient;

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
      /// <param name="connectionString"></param>
      public NONADatabaseConnector(string connectionString) : base(connectionString) { }

      /// <summary>
      /// Gets the registration for a channel.
      /// </summary>
      /// <param name="guild">Guild that has the channel.</param>
      /// <param name="channel">Channel to get the registration for.</param>
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
      /// Gets the prefix of a guild.
      /// </summary>
      /// <param name="guild">Guild to get the prefix for.</param>
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
      /// <param name="guild">Guild to get setup status for.</param>
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
      /// Adds a new registration to a channel.
      /// </summary>
      /// <param name="guild">Guild that has the channel.</param>
      /// <param name="channel">Channel to add the registration to.</param>
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
      /// 
      /// </summary>
      /// <param name="guild"></param>
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

      public void AddNickname(ulong guild, string pokemon, string nickname)
      {
         string queryString = $@"INSERT INTO nickname (guild, name, nickname)
                                 VALUES ({guild}, '{pokemon}', '{nickname}')";

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
      /// <param name="guild">Guild that has the channel.</param>
      /// <param name="channel">Channel to update the registration for.</param>
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
      /// Updates the prefix of a guild.
      /// </summary>
      /// <param name="guild">Guild to update the prefix for.</param>
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
      /// <param name="guild">Guild that completed setup.</param>
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

      public void UpdateNickname(ulong guild, string nickname, string original)
      {
         string queryString = $@"UPDATE nickname 
                                 SET nickname = '{nickname}'
                                 WHERE guild={guild}
                                 AND nickname = '{original}';";

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
      /// <param name="guild">Guild to remove all registrations from.</param>
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
      /// <param name="guild">Guild that has the channel.</param>
      /// <param name="channel">Channel to remove the registration from.</param>
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

      /// <summary>
      /// Deletes the settings saved for a guild.
      /// </summary>
      /// <param name="guild">Guild to remove the settings from.</param>
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