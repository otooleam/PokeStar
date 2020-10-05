using System;
using System.Linq;
using System.Collections.Generic;
using DuoVia.FuzzyStrings;
using PokeStar.DataModels;
using PokeStar.Calculators;

namespace PokeStar.ConnectionInterface
{
   /// <summary>
   /// Manages backend connections to the databases.
   /// </summary>
   public class Connections
   {
      private static Connections connections;

      private const string PokemonImageFolder = "PokemonImages";

      private readonly POGODatabaseConnector POGODBConnector;
      private readonly NONADatabaseConnector NONADBConnector;

      private List<string> PokemonNames;
      private List<string> MoveNames;

      private const int NumSuggestions = 10;

      /// <summary>
      /// Creates a new Connections object.
      /// Private to implement the singleton design patturn.
      /// </summary>
      private Connections()
      {
         
         POGODBConnector = new POGODatabaseConnector(Global.POGO_DB_CONNECTION_STRING);
         NONADBConnector = new NONADatabaseConnector(Global.NONA_DB_CONNECTION_STRING);
         UpdatePokemonNameList();
         UpdateMoveNameList();
      }

      /// <summary>
      /// Gets the current Connections instance.
      /// </summary>
      /// <returns>The Connections instance.</returns>
      public static Connections Instance()
      {
         if (connections == null)
         {
            connections = new Connections();
         }
         return connections;
      }

      /// <summary>
      /// Copy a file from PokemonImages to the location of the application.
      /// </summary>
      /// <param name="fileName">Name of the file.</param>
      public static void CopyFile(string fileName)
      {
         System.IO.File.Copy($"{Global.PROGRAM_PATH}\\{PokemonImageFolder}\\{fileName}", $"{Global.PROGRAM_PATH}\\{fileName}", true);
      }

      /// <summary>
      /// Delete a file from the location of the application.
      /// </summary>
      /// <param name="fileName">Name of the file.</param>
      public static void DeleteFile(string fileName)
      {
         System.IO.File.Delete($"{Global.PROGRAM_PATH}\\{fileName}");
      }

      /// <summary>
      /// Converts a Pokémon's name to its file name.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>Pokémon picture file name.</returns>
      public static string GetPokemonPicture(string pokemonName)
      {
         pokemonName = pokemonName.Replace(" ", "_");
         pokemonName = pokemonName.Replace(".", "");
         pokemonName = pokemonName.Replace("\'", "");
         pokemonName = pokemonName.Replace("?", "QU");
         pokemonName = pokemonName.Replace("!", "EX");
         return pokemonName + ".png";
      }

      /// <summary>
      /// Gets a list of current raid bosses for a given tier.
      /// </summary>
      /// <param name="tier">Tier of bosses.</param>
      /// <returns>List of current raid bosses for the tier.</returns>
      public static List<string> GetBossList(int tier)
      {
         return SilphData.GetRaidBossesTier(tier);
      }

      /// <summary>
      /// Updates the list of Pokémon to use for the fuzzy search.
      /// Only needs to be ran when a Pokémon name has changed.
      /// </summary>
      public void UpdatePokemonNameList()
      {
         PokemonNames = POGODBConnector.GetPokemonNameList();
      }

      /// <summary>
      /// Updates the list of Moves to use for the fuzzy search.
      /// Only needs to be ran when a move name has changed.
      /// </summary>
      public void UpdateMoveNameList()
      {
         MoveNames = POGODBConnector.GetMoveNameList();
      }

      /// <summary>
      /// Searches for the closest Pokémon by name.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>List of closest Pokémon names.</returns>
      public List<string> SearchPokemon(string pokemonName)
      {
         return FuzzyNameSearch(pokemonName, PokemonNames);
      }

      /// <summary>
      /// Searches for the closests moves by name.
      /// </summary>
      /// <param name="moveName">Name of the move.</param>
      /// <returns>List of closest move names.</returns>
      public List<string> SearchMove(string moveName)
      {
         return FuzzyNameSearch(moveName, MoveNames);
      }

      /// <summary>
      /// Searches for the closest strings in a list of strings.
      /// </summary>
      /// <param name="search">Value to search for.</param>
      /// <param name="dir">List of strings to search in.</param>
      /// <returns>List of the closest strings from the list.</returns>
      private static List<string> FuzzyNameSearch(string search, List<string> dir)
      {
         Dictionary<string, double> fuzzy = new Dictionary<string, double>();
         foreach (string value in dir)
         {
            fuzzy.Add(value, value.FuzzyMatch(search));
         }
         List<KeyValuePair<string, double>> myList = fuzzy.ToList();
         myList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
         fuzzy = myList.ToDictionary(x => x.Key, x => x.Value);
         return fuzzy.Keys.Take(NumSuggestions).ToList();
      }

      /// <summary>
      /// Gets a given raid boss.
      /// Calculates CP values relevant to a raid boss. This includes
      /// min and max cps and weather boosted min and max cps.
      /// </summary>
      /// <param name="raidBossName">Name of the raid boss.</param>
      /// <returns>The raid boss coresponding to the name, otherwise null.</returns>
      public RaidBoss GetRaidBoss(string raidBossName)
      {
         if (raidBossName == null)
         {
            return null;
         }

         string name = ReformatName(raidBossName);
         RaidBoss raidBoss = POGODBConnector.GetRaidBoss(name);
         if (raidBoss == null)
         {
            return null;
         }

         Tuple<Dictionary<string, int>, Dictionary<string, int>> typeRelations = GetTypeDefenseRelations(raidBoss.Type);
         raidBoss.Weakness = typeRelations.Item2.Keys.ToList();
         raidBoss.Resistance = typeRelations.Item1.Keys.ToList();
         raidBoss.Weather = GetWeather(raidBoss.Type);

         raidBoss.CPLow = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV, 
            Global.MIN_SPECIAL_IV, Global.RAID_LEVEL);

         raidBoss.CPHigh = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, Global.RAID_LEVEL);

         raidBoss.CPLowBoosted = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV, 
            Global.RAID_LEVEL + Global.WEATHER_BOOST);

         raidBoss.CPHighBoosted = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, 
            Global.RAID_LEVEL + Global.WEATHER_BOOST);

         return raidBoss;
      }

      /// <summary>
      /// Gets a given Pokémon.
      /// Calculates max CP value of the Pokémon.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>The Pokémon coresponding to the name, otherwise null.</returns>
      public Pokemon GetPokemon(string pokemonName)
      {
         if (pokemonName == null)
         {
            return null;
         }

         string name = ReformatName(pokemonName);
         Pokemon pokemon = POGODBConnector.GetPokemon(name);
         if (pokemon == null)
         {
            return null;
         }

         pokemon.Forms = POGODBConnector.GetPokemonByNumber(pokemon.Number);

         Tuple<Dictionary<string, int>, Dictionary<string, int>> typeRelations = GetTypeDefenseRelations(pokemon.Type);
         pokemon.Weakness = typeRelations.Item2.Keys.ToList();
         pokemon.Resistance = typeRelations.Item1.Keys.ToList();
         pokemon.Weather = GetWeather(pokemon.Type);
         pokemon.FastMove = POGODBConnector.GetPokemonMoves(name, true);
         pokemon.ChargeMove = POGODBConnector.GetPokemonMoves(name, false, pokemon.Shadow);
         pokemon.Counter = POGODBConnector.GetCounters(name);

         foreach (Counter counter in pokemon.Counter)
         {
            counter.FastAttack = POGODBConnector.GetPokemonMove(counter.Name, counter.FastAttack.Name);
            counter.ChargeAttack = POGODBConnector.GetPokemonMove(counter.Name, counter.ChargeAttack.Name);
         }

         pokemon.CPMax = CPCalculator.CalcCPPerLevel(
            pokemon.Attack, pokemon.Defense, pokemon.Stamina,
            Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, Global.MAX_LEVEL);

         return pokemon;
      }

      /// <summary>
      /// Gets all evolutions in a Pokémon's evolution family.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="namesChecked">Pokémon in the familly already checked.</param>
      /// <returns>List of evolutions in the Pokémon's family.</returns>
      public List<Evolution> GetEvolutionFamily(string pokemonName, List<string> namesChecked = null)
      {
         if (namesChecked == null)
         {
            namesChecked = new List<string>();
         }

         List<Evolution> evolutions = POGODBConnector.GetEvolutions(pokemonName);

         if (evolutions.Count == 0)
         {
            return new List<Evolution>();
         }
         namesChecked.Add(pokemonName);

         List<Evolution> evoList = new List<Evolution>();
         foreach (Evolution evo in evolutions)
         {
            if (!evo.Start.Equals(pokemonName, StringComparison.OrdinalIgnoreCase) && !namesChecked.Contains(evo.Start))
            {
               evoList.AddRange(GetEvolutionFamily(evo.Start, namesChecked));
            }
            else if (!evo.End.Equals(pokemonName, StringComparison.OrdinalIgnoreCase) && !namesChecked.Contains(evo.End))
            {
               evoList.AddRange(GetEvolutionFamily(evo.End, namesChecked));
            }
         }
         evoList.AddRange(evolutions);
         evolutions.Clear();
         evolutions.AddRange(evoList.Where(x => !evolutions.Contains(x)));
         return evolutions;
      }

      /// <summary>
      /// Calculates all of the relevant CP valus of a Pokémon. This
      /// includes the raid, quest, hatch, and wild perfect IV values.
      /// </summary>
      /// <param name="pokemon">Reference to a Pokémon.</param>
      public static void CalcAllCP(ref Pokemon pokemon)
      {
         if (pokemon != null)
         {
            pokemon.CPBestBuddy = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, 
               Global.MAX_LEVEL + Global.BUDDY_BOOST);

            pokemon.CPRaidMin = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV, 
               Global.MIN_SPECIAL_IV, Global.RAID_LEVEL);

            pokemon.CPRaidMax = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, Global.RAID_LEVEL);

            pokemon.CPRaidBoostedMin = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV,
               Global.RAID_LEVEL + Global.WEATHER_BOOST);

            pokemon.CPRaidBoostedMax = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV,
               Global.RAID_LEVEL + Global.WEATHER_BOOST);

            pokemon.CPQuestMin = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV,
               Global.MIN_SPECIAL_IV, Global.QUEST_LEVEL);

            pokemon.CPQuestMax = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, Global.QUEST_LEVEL);

            pokemon.CPHatchMin = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MIN_SPECIAL_IV, Global.MIN_SPECIAL_IV,
               Global.MIN_SPECIAL_IV, Global.HATCH_LEVEL);

            pokemon.CPHatchMax = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, Global.HATCH_LEVEL);

            for (int level = Global.MIN_WILD_LEVEL; level <= Global.MAX_WILD_LEVEL; level++)
            {
               pokemon.CPWild.Add(CPCalculator.CalcCPPerLevel(
                  pokemon.Attack, pokemon.Defense, pokemon.Stamina,
                  Global.MAX_IV, Global.MAX_IV, Global.MAX_IV, level));
            }
         }
      }

      /// <summary>
      /// Gets a given move.
      /// </summary>
      /// <param name="moveName"></param>
      /// <returns></returns>
      public Move GetMove(string moveName)
      {
         if (moveName == null)
         {
            return null;
         }

         string name = ReformatName(moveName);
         Move move = POGODBConnector.GetMove(name);

         if (move == null)
         {
            return null;
         }

         move.Weather = GetWeather(new List<string>() { move.Type });
         move.PokemonWithMove = POGODBConnector.GetPokemonWithMove(name);

         return move;
      }

      /// <summary>
      /// Gets all Pokémon that have a given number.
      /// </summary>
      /// <param name="pokemonNumber">Number of the Pokémon</param>
      /// <returns>List of Pokémon with the given number.</returns>
      public List<string> GetPokemonByNumber(int pokemonNumber)
      {
         return POGODBConnector.GetPokemonByNumber(pokemonNumber);
      }

      /// <summary>
      /// Reformats the name from user input to the POGO database format.
      /// </summary>
      /// <param name="originalName">User input name.</param>
      /// <returns>Name formated for the POGO database</returns>
      private static string ReformatName(string originalName)
      {
         int index = originalName.IndexOf('\'');
         return index == -1 ? originalName : originalName.Insert(index, "\'");
      }

      /// <summary>
      /// Gets defensive type relations for a Pokémon's type.
      /// Separates weaknesses and resistances.
      /// </summary>
      /// <param name="types">List of Pokémon types.</param>
      /// <returns>Dictionaries of types and modifiers.</returns>
      public Tuple<Dictionary<string, int>, Dictionary<string, int>> GetTypeDefenseRelations(List<string> types)
      {
         Dictionary<string, int> allRelations = POGODBConnector.GetTypeDefenseRelations(types);
         return new Tuple<Dictionary<string, int>, Dictionary<string, int>>(
            allRelations.Where(x => x.Value < 0).ToDictionary(k => k.Key, v => v.Value),
            allRelations.Where(x => x.Value > 0).ToDictionary(k => k.Key, v => v.Value)
         );
      }

      /// <summary>
      /// Gets offensive type relations for a move's type.
      /// Separates super and not very effective moves.
      /// </summary>
      /// <param name="type">Move type.</param>
      /// <returns>Dictionaries of types and modifiers.</returns>
      public Tuple<Dictionary<string, int>, Dictionary<string, int>> GetTypeAttackRelations(string type)
      {
         Dictionary<string, int> allRelations = POGODBConnector.GetTypeAttackRelations(type);
         return new Tuple<Dictionary<string, int>, Dictionary<string, int>> (
            allRelations.Where(x => x.Value > 0).ToDictionary(k => k.Key, v => v.Value),
            allRelations.Where(x => x.Value < 0).ToDictionary(k => k.Key, v => v.Value)
         );
      }

      /// <summary>
      /// Gets all weather that boosts the given types.
      /// </summary>
      /// <param name="types">List of types.</param>
      /// <returns>List of weather that boosts the givent types.</returns>
      public List<string> GetWeather(List<string> types)
      {
         return POGODBConnector.GetWeather(types);
      }

      /// <summary>
      /// Adds settings to the database for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      public void InitSettings(ulong guild)
      {
         NONADBConnector.AddSettings(guild);
      }

      /// <summary>
      /// Gets the prefix of a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <returns>Prefix registerd for the guild.</returns>
      public string GetPrefix(ulong guild)
      {
         return NONADBConnector.GetPrefix(guild);
      }

      /// <summary>
      /// Check if setup is completed for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <returns>True if setup is complete for the guild, otherwise false.</returns>
      public bool GetSetupComplete(ulong guild)
      {
         return NONADBConnector.GetSetupComplete(guild);
      }

      /// <summary>
      /// Updates the prefix of the guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="prefix">New prefix value.</param>
      public void UpdatePrefix(ulong guild, string prefix)
      {
         NONADBConnector.UpdatePrefix(guild, prefix);
      }

      /// <summary>
      /// Marks a guild setup as complete
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      public void CompleteSetup(ulong guild)
      {
         NONADBConnector.CompleteSetup(guild);
      }

      /// <summary>
      /// Deletes the settings of a guild.
      /// </summary>
      /// <param name="guild">Id of guild.</param>
      public void DeleteSettings(ulong guild)
      {
         NONADBConnector.DeleteSettings(guild);
      }

      /// <summary>
      /// Gets the registration of a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel in the guild.</param>
      /// <returns>Registration string for the channel, otherwise null.</returns>
      public string GetRegistration(ulong guild, ulong channel)
      {
         return NONADBConnector.GetRegistration(guild, channel);
      }

      /// <summary>
      /// Updates the registration of a channel.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel in the guild.</param>
      /// <param name="register">New registration value.</param>
      public void UpdateRegistration(ulong guild, ulong channel, string register)
      {
         if (GetRegistration(guild, channel) == null)
         {
            NONADBConnector.AddRegistration(guild, channel, register);
         }
         else
         {
            NONADBConnector.UpdateRegistration(guild, channel, register);
         }
      }

      /// <summary>
      /// Deletes the registration of a guild or channel.
      /// If no channel is given then all registrations for the guild are
      /// deleted.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="channel">Id of the channel in the guild.</param>
      public void DeleteRegistration(ulong guild, ulong? channel = null)
      {
         if (channel == null)
            NONADBConnector.DeleteAllRegistration(guild);
         else
            NONADBConnector.DeleteRegistration(guild, (ulong)channel);
      }

      /// <summary>
      /// Gets a Pokémon by its guild nickname.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname of the Pokémon.</param>
      /// <returns>Name of the Pokémon if it is assigned, otherwise null.</returns>
      public string GetPokemonWithNickname(ulong guild, string nickname)
      {
         return NONADBConnector.GetPokemon(guild, nickname);
      }

      /// <summary>
      /// Gets a guild's list of nicknames for a Pokémon.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>List of nicknames assigned to the Pokémon for the guild.</returns>
      public List<string> GetNicknames(ulong guild, string pokemonName)
      {
         return NONADBConnector.GetNicknames(guild, pokemonName);
      }

      /// <summary>
      /// Adds a nickname to a Pokémon for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild</param>
      /// <param name="nickname">Nickname for the Pokémon.</param>
      /// <param name="pokemonName">Pokémon the nickname applies to.</param>
      public void AddNickname(ulong guild, string nickname, string pokemonName)
      {
         NONADBConnector.AddNickname(guild, pokemonName, nickname);
      }

      /// <summary>
      /// Updates the nickname of a Pokémon for a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="oldNickname">Nickname to replace.</param>
      /// <param name="newNickname">Nickname to replace with.</param>
      public void UpdateNickname(ulong guild, string oldNickname, string newNickname)
      {
         NONADBConnector.UpdateNickname(guild, newNickname, oldNickname);
      }

      /// <summary>
      /// Deletes a nickname from a guild.
      /// </summary>
      /// <param name="guild">Id of the guild.</param>
      /// <param name="nickname">Nickname to delete.</param>
      public void DeleteNickname(ulong guild, string nickname)
      {
         NONADBConnector.DeleteNickname(guild, nickname);
      }
   }
}