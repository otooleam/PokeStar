using System;
using System.Linq;
using System.Collections.Generic;
using PokeStar.DataModels;
using PokeStar.Calculators;

namespace PokeStar.ConnectionInterface
{
   public class Connections
   {
      private static Connections connections;

      private readonly POGODatabaseConnector POGODBConnector;
      private readonly NONADatabaseConnector NONADBConnector;

      public Uri RAID_BOSS_URL { get; } = new Uri("https://thesilphroad.com/raid-bosses");
      public static string RAID_BOSS_HTML => raidBossHTML;
      private const string raidBossHTML = "//*[@class = 'col-md-4']";

      /// <summary>
      /// Creates a new Connections object.
      /// Private to implement the singleton design patturn.
      /// </summary>
      private Connections()
      {
         POGODBConnector = new POGODatabaseConnector(Environment.GetEnvironmentVariable("POGO_DB_CONNECTION_STRING"));
         NONADBConnector = new NONADatabaseConnector(Environment.GetEnvironmentVariable("NONA_DB_CONNECTION_STRING"));
      }

      /// <summary>
      /// Gets the current Connections instance.
      /// </summary>
      /// <returns>The Connections instance.</returns>
      public static Connections Instance()
      {
         if (connections == null)
            connections = new Connections();
         return connections;
      }

      /// <summary>
      /// Copy a file from PokemonImages to the location of the application.
      /// </summary>
      /// <param name="fileName">Name of file to copy.</param>
      public static void CopyFile(string fileName)
      {
         var location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
         System.IO.File.Copy($"{location}\\PokemonImages\\{fileName}", $"{location}\\{fileName}", true);
      }

      /// <summary>
      /// Delete a file from the location of the application.
      /// </summary>
      /// <param name="fileName">Name of file to delete.</param>
      public static void DeleteFile(string fileName)
      {
         var location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
         System.IO.File.Delete($"{location}\\{fileName}");
      }

      /// <summary>
      /// Converts a pokemon's name to its file name.
      /// </summary>
      /// <param name="pokemonName">Name of pokemon.</param>
      /// <returns>Pokemon picture file name.</returns>
      public static string GetPokemonPicture(string pokemonName)
      {
         pokemonName = pokemonName.Replace(" ", "_");
         pokemonName = pokemonName.Replace(".", "");
         return pokemonName + ".png";
      }

      /// <summary>
      /// Gets a list of current raid bosses from The Silph Road.
      /// </summary>
      /// <param name="tier">Tier of bosses to get.</param>
      /// <returns>List of current raid bosses in the given tier.</returns>
      public static List<string> GetBossList(short tier)
      {
         return SilphData.GetRaidBossesTier(tier);
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
            return null;

         string name = ReformatName(raidBossName);
         RaidBoss raidBoss = POGODBConnector.GetRaidBoss(name);
         if (raidBoss == null) return null;

         var typeRelations = GetTypeDefenseRelations(raidBoss.Type);
         raidBoss.Weakness = typeRelations.weak.Keys.ToList();
         raidBoss.Resistance = typeRelations.strong.Keys.ToList();
         raidBoss.Weather = GetWeather(raidBoss.Type);

         raidBoss.CPLow = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            CPCalculator.MIN_SPECIAL_IV, CPCalculator.MIN_SPECIAL_IV,
            CPCalculator.MIN_SPECIAL_IV, CPCalculator.RAID_LEVEL);

         raidBoss.CPHigh = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            CPCalculator.MAX_IV, CPCalculator.MAX_IV,
            CPCalculator.MAX_IV, CPCalculator.RAID_LEVEL);

         raidBoss.CPLowBoosted = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            CPCalculator.MIN_SPECIAL_IV, CPCalculator.MIN_SPECIAL_IV,
            CPCalculator.MIN_SPECIAL_IV,
            CPCalculator.RAID_LEVEL + CPCalculator.WEATHER_BOOST);

         raidBoss.CPHighBoosted = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            CPCalculator.MAX_IV, CPCalculator.MAX_IV,
            CPCalculator.MAX_IV,
            CPCalculator.RAID_LEVEL + CPCalculator.WEATHER_BOOST);

         if (raidBoss.Name.Equals("Unown"))
            raidBoss.Name = raidBossName;

         return raidBoss;
      }

      /// <summary>
      /// Gets a given pokemon.
      /// Calculates max CP value of the pokemon.
      /// </summary>
      /// <param name="pokemonName">Name of the pokemon.</param>
      /// <returns>The pokemon coresponding to the name, otherwise null.</returns>
      public Pokemon GetPokemon(string pokemonName)
      {
         if (pokemonName == null)
            return null;

         string name = ReformatName(pokemonName);
         Pokemon pokemon = POGODBConnector.GetPokemon(name);
         if (pokemon == null) return null;

         var typeRelations = GetTypeDefenseRelations(pokemon.Type);
         pokemon.Weakness = typeRelations.weak.Keys.ToList();
         pokemon.Resistance = typeRelations.strong.Keys.ToList();
         pokemon.Weather = GetWeather(pokemon.Type);
         pokemon.FastMove = POGODBConnector.GetMoves(name, true);
         pokemon.ChargeMove = POGODBConnector.GetMoves(name, false, pokemon.Shadow);

         pokemon.CPMax = CPCalculator.CalcCPPerLevel(
            pokemon.Attack, pokemon.Defense, pokemon.Stamina,
            CPCalculator.MAX_IV, CPCalculator.MAX_IV,
            CPCalculator.MAX_IV, CPCalculator.MAX_LEVEL);

         return pokemon;
      }

      /// <summary>
      /// Calculates all of the relevant CP valus of a pokemon. This
      /// includes the raid, quest, hatch, and wild perfect IV values.
      /// </summary>
      /// <param name="pokemon"></param>
      public static void CalcAllCP(ref Pokemon pokemon)
      {
         if (pokemon != null)
         {
            pokemon.CPBestBuddy = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               CPCalculator.MAX_IV, CPCalculator.MAX_IV,
               CPCalculator.MAX_IV,
               CPCalculator.MAX_LEVEL + CPCalculator.BUDDY_BOOST);

            pokemon.CPRaidMin = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               CPCalculator.MIN_SPECIAL_IV, CPCalculator.MIN_SPECIAL_IV,
               CPCalculator.MIN_SPECIAL_IV, CPCalculator.RAID_LEVEL);

            pokemon.CPRaidMax = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               CPCalculator.MAX_IV, CPCalculator.MAX_IV,
               CPCalculator.MAX_IV, CPCalculator.RAID_LEVEL);

            pokemon.CPRaidBoostedMin = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               CPCalculator.MIN_SPECIAL_IV, CPCalculator.MIN_SPECIAL_IV,
               CPCalculator.MIN_SPECIAL_IV,
               CPCalculator.RAID_LEVEL + CPCalculator.WEATHER_BOOST);

            pokemon.CPRaidBoostedMax = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               CPCalculator.MAX_IV, CPCalculator.MAX_IV,
               CPCalculator.MAX_IV,
               CPCalculator.RAID_LEVEL + CPCalculator.WEATHER_BOOST);

            pokemon.CPQuestMin = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               CPCalculator.MIN_SPECIAL_IV, CPCalculator.MIN_SPECIAL_IV,
               CPCalculator.MIN_SPECIAL_IV, CPCalculator.QUEST_LEVEL);

            pokemon.CPQuestMax = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               CPCalculator.MAX_IV, CPCalculator.MAX_IV,
               CPCalculator.MAX_IV, CPCalculator.QUEST_LEVEL);

            pokemon.CPHatchMin = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               CPCalculator.MIN_SPECIAL_IV, CPCalculator.MIN_SPECIAL_IV,
               CPCalculator.MIN_SPECIAL_IV, CPCalculator.HATCH_LEVEL);

            pokemon.CPHatchMax = CPCalculator.CalcCPPerLevel(
               pokemon.Attack, pokemon.Defense, pokemon.Stamina,
               CPCalculator.MAX_IV, CPCalculator.MAX_IV,
               CPCalculator.MAX_IV, CPCalculator.HATCH_LEVEL);

            for (int level = CPCalculator.MIN_WILD_LEVEL; level <= CPCalculator.MAX_WILD_LEVEL; level++)
               pokemon.CPWild.Add(CPCalculator.CalcCPPerLevel(
                  pokemon.Attack, pokemon.Defense, pokemon.Stamina,
                  CPCalculator.MAX_IV, CPCalculator.MAX_IV,
                  CPCalculator.MAX_IV, level));
         }
      }

      /// <summary>
      /// Reformats the name from user input to the POGO database format.
      /// </summary>
      /// <param name="originalName">User input name.</param>
      /// <returns>Name formated for the POGO database</returns>
      private static string ReformatName(string originalName)
      {
         if (originalName.Contains("Unown"))
            originalName = "Unown";
         int index = originalName.IndexOf('\'');
         return index == -1 ? originalName : originalName.Insert(index, "\'");
      }

      /// <summary>
      /// Gets defensive type relations for a pokemon's type.
      /// Separates weaknesses and resistances.
      /// </summary>
      /// <param name="types">List of pokemon types.</param>
      /// <returns>Dictionaries of types and modifiers.</returns>
      public TypeRelation GetTypeDefenseRelations(List<string> types)
      {
         var allRelations = POGODBConnector.GetTypeDefenseRelations(types);
         return new TypeRelation
         {
            strong = allRelations.Where(x => x.Value < 0).ToDictionary(k => k.Key, v => v.Value),
            weak = allRelations.Where(x => x.Value > 0).ToDictionary(k => k.Key, v => v.Value)
         };
      }

      /// <summary>
      /// Gets offensive type relations for a move's type.
      /// Separates super and not very effective moves.
      /// </summary>
      /// <param name="type">Move type.</param>
      /// <returns>Dictionaries of types and modifiers.</returns>
      public Nullable<TypeRelation> GetTypeAttackRelations(string type)
      {
         var allRelations = POGODBConnector.GetTypeAttackRelations(type);
         return new TypeRelation
         {
            strong = allRelations.Where(x => x.Value > 0).ToDictionary(k => k.Key, v => v.Value),
            weak = allRelations.Where(x => x.Value < 0).ToDictionary(k => k.Key, v => v.Value)
         };
      }

      /// <summary>
      /// Gets all weather that boosts the given types.
      /// </summary>
      /// <param name="types">List of types to get weather for.</param>
      /// <returns>List of weather that boosts the givent types.</returns>
      public List<string> GetWeather(List<string> types)
      {
         return POGODBConnector.GetWeather(types);
      }


      /// <summary>
      /// Gets the prefix of a guild.
      /// </summary>
      /// <param name="guild">Id of guild to get prefix of.</param>
      /// <returns>Prefix of the guild if it is not default, otherwise null.</returns>
      public string GetPrefix(ulong guild)
      {
         string prefix = NONADBConnector.GetPrefix(guild);
         return prefix?[0].ToString();
      }

      /// <summary>
      /// Updates the prefix of the guild.
      /// </summary>
      /// <param name="guild">Id of the guild to set the prefix for.</param>
      /// <param name="prefix">New prefix value.</param>
      public void UpdatePrefix(ulong guild, string prefix)
      {
         if (GetPrefix(guild) == null)
            NONADBConnector.AddPrefix(guild, prefix);
         else
            NONADBConnector.UpdatePrefix(guild, prefix);
      }

      /// <summary>
      /// Deletes the prefix of a guild.
      /// </summary>
      /// <param name="guild">Id of guild to delete prefix of.</param>
      public void DeletePrefix(ulong guild)
      {
         if (GetPrefix(guild) != null)
            NONADBConnector.DeletePrefix(guild);
      }

      /// <summary>
      /// Gets the registration of a channel.
      /// </summary>
      /// <param name="guild">Id of the guild that has the channel.</param>
      /// <param name="channel">Id of the channel that the registration is for.</param>
      /// <returns>Registration string for the channel, otherwise null.</returns>
      public string GetRegistration(ulong guild, ulong channel)
      {
         string registration = NONADBConnector.GetRegistration(guild, channel);
         return registration ?? null;
      }

      /// <summary>
      /// Updates the registration of a channel.
      /// </summary>
      /// <param name="guild">Id of the guild that has the channel.</param>
      /// <param name="channel">Id of the channel to update the registration of.</param>
      /// <param name="register">New registration value.</param>
      public void UpdateRegistration(ulong guild, ulong channel, string register)
      {
         if (GetRegistration(guild, channel) == null)
            NONADBConnector.AddRegistration(guild, channel, register);
         else
            NONADBConnector.UpdateRegistration(guild, channel, register);
      }

      /// <summary>
      /// Deletes the registration of a guild or channel.
      /// If no channel is given then all registrations for the guild are
      /// deleted.
      /// </summary>
      /// <param name="guild">Id of the guild that has the channel, or to remove registrations from.</param>
      /// <param name="channel">Id of the channel to remove the registration from.</param>
      public void DeleteRegistration(ulong guild, ulong? channel = null)
      {
         if (GetPrefix(guild) != null)
         {
            if (channel == null)
               NONADBConnector.DeleteAllRegistration(guild);
            else
               NONADBConnector.DeleteRegistration(guild, (ulong)channel);
         }
      }
   }

   /// <summary>
   /// Relations for pokemon type(s).
   /// </summary>
   public struct TypeRelation
   {
      public Dictionary<string, int> strong;
      public Dictionary<string, int> weak;
   }
}