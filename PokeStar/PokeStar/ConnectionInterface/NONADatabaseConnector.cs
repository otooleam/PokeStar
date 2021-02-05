using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using PokeStar.DataModels;

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
      public string GetPokemonByNickname(ulong guild, string nickname)
      {
         string pokemonName = null;
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
                  pokemonName = Convert.ToString(reader["name"]);
               }
            }
            conn.Close();
         }
         return pokemonName;
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

      /// POI *****************************************************************

      /// <summary>
      /// Gets a POI by name.
      /// </summary>
      /// <param name="poiName">Name of the POI.</param>
      /// <returns>A gym if the name is in the database, otherwise null.</returns>
      public POI GetPOI(ulong guild, string poiName)
      {
         POI poi = null;
         string queryString = $@"SELECT *
                                 FROM poi 
                                 WHERE guild={guild}
                                 AND name='{poiName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  poi = new POI
                  {
                     Name = Convert.ToString(reader["name"]),
                     Latitude = Convert.ToString(reader["latitude"]),
                     Longitude = Convert.ToString(reader["longitude"]),
                     IsGym = Convert.ToInt32(reader["gym"]) == TRUE,
                     IsSponsored = Convert.ToInt32(reader["sponsored"]) == TRUE,
                     IsExGym = Convert.ToInt32(reader["ex"]) == TRUE,
                  };
               }
            }
            conn.Close();
         }
         return poi;
      }

      /// <summary>
      /// Get a gym by nickname.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname of the gym.</param>
      /// <returns>Name of the gym with the nickname, otherwiase null.</returns>
      public string GetPOIByNickname(ulong guild, string nickname)
      {
         string poi = null;
         string queryString = $@"SELECT name 
                                 FROM poi_nickname 
                                 WHERE guild={guild}
                                 AND nickname = '{nickname}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  poi = Convert.ToString(reader["name"]);
               }
            }
            conn.Close();
         }
         return poi;
      }

      /// <summary>
      /// Gets all nicknames for a POI in a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="poi">Name of the POI.</param>
      /// <returns>List of all nicknames for the POI in the guild.</returns>
      public List<string> GetPOINicknames(ulong guild, string poi)
      {
         List<string> nicknames = new List<string>();
         string queryString = $@"SELECT nickname 
                                 FROM poi_nickname 
                                 WHERE guild={guild}
                                 AND name = '{poi}';";

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
      /// Gets all POIs in a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <returns>List of all POIs in the guild.</returns>
      public List<string> GetGuildPOIs(ulong guild)
      {
         List<string> pois = new List<string>();
         string queryString = $@"SELECT name 
                                 FROM poi 
                                 WHERE guild={guild};";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            using (SqlDataReader reader = new SqlCommand(queryString, conn).ExecuteReader())
            {
               while (reader.Read())
               {
                  pois.Add(Convert.ToString(reader["name"]));
               }
            }
            conn.Close();
         }
         return pois;
      }

      /// <summary>
      /// Adds a Point of Interest to a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="poiName">Name of the Point of Interest.</param>
      /// <param name="latitude">Latitudinal coordinate.</param>
      /// <param name="longitude">Longitudinal coordinate.</param>
      /// <param name="gym">Is the Point of Interest a gym.</param>
      /// <param name="sponsored">Is the Point of Interest sponsored.</param>
      /// <param name="ex">Is the Point of Interest an EX Gym.</param>
      public void AddPOI(ulong guild, string poiName, float latitude, float longitude, int gym, int sponsored, int ex)
      {
         if ((gym != TRUE && gym != FALSE) ||
             (sponsored != TRUE && sponsored != FALSE) ||
             (ex != TRUE && ex != FALSE))
         {
            return;
         }

         string queryString = $@"INSERT INTO poi (guild, name, latitude, longitude, gym, sponsored, ex)
                                 VALUES ({guild}, '{poiName}', '{latitude}', '{longitude}', {gym}, {sponsored}, {ex})";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Updates an attribute of a Point of Interest.
      /// Only updates true/false values.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="poiName">Name of the Point of Interest.</param>
      /// <param name="attribute">Attribute to change.</param>
      /// <param name="value">New value of the attribute</param>
      public void UpdatePOI(ulong guild, string poiName, string attribute, int value)
      {
         if (value != TRUE && value != FALSE)
         {
            return;
         }

         string queryString = $@"UPDATE poi 
                                 SET {attribute} = {value}
                                 WHERE guild = {guild}
                                 AND name = '{poiName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Delete a Point of Interest from a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="poiName">Name of the Point of Interest.</param>
      public void RemovePOI(ulong guild, string poiName)
      {
         string queryString = $@"DELETE FROM poi
                                 WHERE guild={guild}
                                 AND name = '{poiName}';";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Adds a nickname to a Point of Interest for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="poiName">Name of the Point of Interest.</param>
      /// <param name="nickname">Nickname of the Point of Interest.</param>
      public void AddPOINickname(ulong guild, string poiName, string nickname)
      {
         string queryString = $@"INSERT INTO poi (guild, name, nickname)
                                 VALUES ({guild}, '{poiName}', '{nickname}')";

         using (SqlConnection conn = GetConnection())
         {
            conn.Open();
            _ = new SqlCommand(queryString, conn).ExecuteNonQuery();
            conn.Close();
         }
      }

      /// <summary>
      /// Updates a Point of Interest nickname for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="newNickname">New nickname.</param>
      /// <param name="originalNickname">Old nickname.</param>
      public void UpdatePOINickname(ulong guild, string newNickname, string originalNickname)
      {
         string queryString = $@"UPDATE poi 
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
      /// Delete a Point of Interest nickname from a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname of a Pokémon.</param>
      public void DeletePOINickname(ulong guild, string nickname)
      {
         string queryString = $@"DELETE FROM poi
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