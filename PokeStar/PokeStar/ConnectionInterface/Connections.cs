using System;
using PokeStar.Calculators;
using PokeStar.DataModels;

namespace PokeStar.ConnectionInterface
{
   public class Connections
   {

      private const string raidBossHTML = "//*[@class = 'col-md-4']";

      public Uri RAID_BOSS_URL { get; } = new Uri("https://thesilphroad.com/raid-bosses");
      public string RAID_BOSS_HTML => raidBossHTML;

      private static Connections connections;

      private DatabaseConnector dbConnector;

      private Connections() 
      {
         dbConnector = new DatabaseConnector(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"));
      }

      public static Connections Instance()
      {
         if (connections == null)
            connections = new Connections();
         return connections;
      }

      public RaidBoss GetRaidBoss(string raidBossName)
      {
         string name = ReformatName(raidBossName);
         RaidBoss raidBoss = dbConnector.GetRaidBoss(name);

         raidBoss.Weather = dbConnector.GetWeather(raidBoss.Type);
         raidBoss.Weakness = dbConnector.GetTypeRelations(raidBoss.Type, true);
         raidBoss.Resistance = dbConnector.GetTypeRelations(raidBoss.Type, false);

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

         return raidBoss;
      }


      public Pokemon GetPokemon(string pokemonName)
      {
         string name = ReformatName(pokemonName);
         Pokemon pokemon = dbConnector.GetPokemon(name);

         pokemon.Weather = dbConnector.GetWeather(pokemon.Type);
         pokemon.Weakness = dbConnector.GetTypeRelations(pokemon.Type, true);
         pokemon.Resistance = dbConnector.GetTypeRelations(pokemon.Type, false);
         pokemon.FastMove = dbConnector.GetMoves(name, true);
         pokemon.ChargeMove = dbConnector.GetMoves(name, false);

         pokemon.CPMax = CPCalculator.CalcCPPerLevel(
            pokemon.Attack, pokemon.Defense, pokemon.Stamina,
            CPCalculator.MAX_IV, CPCalculator.MAX_IV,
            CPCalculator.MAX_IV, CPCalculator.MAX_LEVEL);

         return pokemon;
      }

      public void CalcAllCP(ref Pokemon pokemon)
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


      private static string ReformatName(string originalName)
      {
         int index = originalName.IndexOf('\'');
         return index == -1 ? originalName : originalName.Insert(index, "\'");
      }

   }
}
