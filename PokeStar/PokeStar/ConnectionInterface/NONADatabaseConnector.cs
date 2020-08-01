using System;
using System.Data.SqlClient;

namespace PokeStar.ConnectionInterface
{
   public class NONADatabaseConnector : DatabaseConnector
   {
      public NONADatabaseConnector(string connectionString) : base(connectionString) { }

      public string GetRegistration(ulong guild, ulong channel)
      {
         string registration = null;
         using (var conn = GetConnection())
         {
            conn.Open();

            string queryString = $@"SELECT register 
                                 FROM channel_registration 
                                 WHERE guild={guild}
                                 AND channel={channel};";
            using (var reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
                  if(reader["register"].GetType() != typeof(DBNull))
                     registration = Convert.ToString(reader["register"]);
            }
            conn.Close();
         }
         return registration;
      }
      public string GetPrefix(ulong guild)
      {
         string prefix = null;
         using (var conn = GetConnection())
         {
            conn.Open();

            string queryString = $@"SELECT prefix 
                                 FROM command_prefix 
                                 WHERE guild={guild};";
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

      public void AddRegistration(ulong guild, ulong channel, string registration)
      {
         using (var conn = GetConnection())
         {
            conn.Open();

            string queryString = $@"INSERT INTO channel_registration (guild, channel, register)
                                 VALUES ({guild}, {channel}, '{registration}')";
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      public void AddPrefix(ulong guild, string prefix)
      {
         using (var conn = GetConnection())
         {
            conn.Open();

            string queryString = $@"INSERT INTO command_prefix (guild, prefix)
                                 VALUES ({guild}, '{prefix[0]}')";
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      public void UpdateRegistration(ulong guild, ulong channel, string registration)
      {
         using (var conn = GetConnection())
         {
            conn.Open();

            string queryString = $@"UPDATE channel_registration 
                                 SET register = '{registration}'
                                 WHERE guild={guild}
                                 AND channel={channel};";
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      public void UpdatePrefix(ulong guild, string prefix)
      {
         using (var conn = GetConnection())
         {
            conn.Open();

            string queryString = $@"UPDATE command_prefix 
                                 SET prefix = '{prefix}'
                                 WHERE guild={guild};";
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      public void DeleteAllRegistration(ulong guild)
      {
         using (var conn = GetConnection())
         {
            conn.Open();

            string queryString = $@"DELETE FROM channel_registration
                                 WHERE guild={guild};";
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      public void DeleteRegistration(ulong guild, ulong channel)
      {
         using (var conn = GetConnection())
         {
            conn.Open();

            string queryString = $@"DELETE FROM channel_registration
                                 WHERE guild={guild}
                                 AND channel={channel};";
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      public void DeletePrefix(ulong guild)
      {
         using (var conn = GetConnection())
         {
            conn.Open();

            string queryString = $@"DELETE FROM command_prefix
                                 WHERE guild={guild};";
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }
   }
}
