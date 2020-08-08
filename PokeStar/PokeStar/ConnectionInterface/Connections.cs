using System;
using System.Collections.Generic;
using PokeStar.Calculators;
using PokeStar.DataModels;

namespace PokeStar.ConnectionInterface
{
   public class Connections
   {

      private const string raidBossHTML = "//*[@class = 'col-md-4']";

      public Uri RAID_BOSS_URL { get; } = new Uri("https://thesilphroad.com/raid-bosses");
      public static string RAID_BOSS_HTML => raidBossHTML;

      private static Connections connections;

      private readonly POGODatabaseConnector POGODBConnector;
      private readonly NONADatabaseConnector NONADBConnector;

      private Connections()
      {
         POGODBConnector = new POGODatabaseConnector(Environment.GetEnvironmentVariable("POGO_DB_CONNECTION_STRING"));
         NONADBConnector = new NONADatabaseConnector(Environment.GetEnvironmentVariable("NONA_DB_CONNECTION_STRING"));
      }

      public static Connections Instance()
      {
         if (connections == null)
            connections = new Connections();
         return connections;
      }

      public static void CopyFile(string fileName)
      {
         var location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
         System.IO.File.Copy($"{location}\\PokemonImages\\{fileName}", $"{location}\\{fileName}", true);
      }

      public static void DeleteFile(string fileName)
      {
         var location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
         System.IO.File.Delete($"{location}\\{fileName}");
      }

      public static string GetPokemonPicture(string pokemonName)
      {
         pokemonName = pokemonName.Replace(" ", "_");
         pokemonName = pokemonName.Replace(".", "");
         return pokemonName + ".png";
      }

      public static List<string> GetBossList(short tier)
      {
         return SilphData.GetRaidBossesTier(tier);
      }

      public RaidBoss GetRaidBoss(string raidBossName)
      {
         if (raidBossName == null)
            return null;

         string name = ReformatName(raidBossName);
         RaidBoss raidBoss = POGODBConnector.GetRaidBoss(name);
         if (raidBoss == null) return null;

         raidBoss.Weather = POGODBConnector.GetWeather(raidBoss.Type);
         raidBoss.Weakness = POGODBConnector.GetTypeRelations(raidBoss.Type, true);
         raidBoss.Resistance = POGODBConnector.GetTypeRelations(raidBoss.Type, false);

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

      public Pokemon GetPokemon(string pokemonName)
      {
         if (pokemonName == null)
            return null;

         string name = ReformatName(pokemonName);
         Pokemon pokemon = POGODBConnector.GetPokemon(name);
         if (pokemon == null) return null;

         pokemon.Weather = POGODBConnector.GetWeather(pokemon.Type);
         pokemon.Weakness = POGODBConnector.GetTypeRelations(pokemon.Type, true);
         pokemon.Resistance = POGODBConnector.GetTypeRelations(pokemon.Type, false);
         pokemon.FastMove = POGODBConnector.GetMoves(name, true);
         pokemon.ChargeMove = POGODBConnector.GetMoves(name, false);

         pokemon.CPMax = CPCalculator.CalcCPPerLevel(
            pokemon.Attack, pokemon.Defense, pokemon.Stamina,
            CPCalculator.MAX_IV, CPCalculator.MAX_IV,
            CPCalculator.MAX_IV, CPCalculator.MAX_LEVEL);

         return pokemon;
      }

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

      private static string ReformatName(string originalName)
      {
         if (originalName.Contains("Unown"))
            originalName = "Unown";
         int index = originalName.IndexOf('\'');
         return index == -1 ? originalName : originalName.Insert(index, "\'");
      }

      public string GetPrefix(ulong guild)
      {
         string prefix = NONADBConnector.GetPrefix(guild);
         return (prefix == null) ? null : prefix[0].ToString();
      }
      public void UpdatePrefix(ulong guild, string prefix)
      {
         if (GetPrefix(guild) == null)
            NONADBConnector.AddPrefix(guild, prefix);
         else
            NONADBConnector.UpdatePrefix(guild, prefix);
      }
      public void DeletePrefix(ulong guild)
      {
         if (GetPrefix(guild) != null)
            NONADBConnector.DeletePrefix(guild);
      }

      public string GetRegistration(ulong guild, ulong channel)
      {
         string registration = NONADBConnector.GetRegistration(guild, channel);
         return (registration == null) ? null : registration;
      }
      public void UpdateRegistration(ulong guild, ulong channel, string prefix)
      {
         if (GetRegistration(guild, channel) == null)
            NONADBConnector.AddRegistration(guild, channel, prefix);
         else
            NONADBConnector.UpdateRegistration(guild, channel, prefix);
      }
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
}
