using System;
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

         using (var conn = GetConnection())
         {
            conn.Open();
            using (var reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
                  if (reader["register"].GetType() != typeof(DBNull))
                     registration = Convert.ToString(reader["register"]);
            }
            conn.Close();
         }
         return registration;
      }

      /// <summary>
      /// Gets the prefix of a guild.
      /// </summary>
      /// <param name="guild">Guild to get the prefix for.</param>
      /// <returns>The guild prefix, otherwise null.</returns>
      public string GetPrefix(ulong guild)
      {
         string prefix = null;
         string queryString = $@"SELECT prefix 
                                 FROM command_prefix 
                                 WHERE guild={guild};";

         using (var conn = GetConnection())
         {
            conn.Open();
            using (var reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
                  if (reader["prefix"].GetType() != typeof(DBNull))
                     prefix = Convert.ToString(reader["prefix"]);
            }
            conn.Close();
         }
         return prefix;
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

         using (var conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Adds a new prefix to a server.
      /// </summary>
      /// <param name="guild">Guild to add the prefix to.</param>
      /// <param name="prefix">New prefix value.</param>
      public void AddPrefix(ulong guild, string prefix)
      {
         string queryString = $@"INSERT INTO command_prefix (guild, prefix)
                                 VALUES ({guild}, '{prefix[0]}')";

         using (var conn = GetConnection())
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

         using (var conn = GetConnection())
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
         string queryString = $@"UPDATE command_prefix 
                                 SET prefix = '{prefix}'
                                 WHERE guild={guild};";

         using (var conn = GetConnection())
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

         using (var conn = GetConnection())
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

         using (var conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Deletes the prefix saved for a guild.
      /// </summary>
      /// <param name="guild">Guild to remove the prefix from.</param>
      public void DeletePrefix(ulong guild)
      {
         string queryString = $@"DELETE FROM command_prefix
                                 WHERE guild={guild};";

         using (var conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }
   }
}