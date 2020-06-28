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
         int index = raidBossName.IndexOf('\'');
         string name = index == -1 ? raidBossName : raidBossName.Insert(index, "\'");
         RaidBoss raidBoss = dbConnector.GetRaidBoss(name);

         raidBoss.CPLow = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina, 
            CPCalculator.MIN_RAID_IV, CPCalculator.MIN_RAID_IV, 
            CPCalculator.MIN_RAID_IV, CPCalculator.RAID_LEVEL);

         raidBoss.CPHigh = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            CPCalculator.MAX_IV, CPCalculator.MAX_IV,
            CPCalculator.MAX_IV, CPCalculator.RAID_LEVEL);

         raidBoss.CPLowBoosted = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            CPCalculator.MIN_RAID_IV, CPCalculator.MIN_RAID_IV,
            CPCalculator.MIN_RAID_IV, 
            CPCalculator.RAID_LEVEL + CPCalculator.WEATHER_BOOST);

         raidBoss.CPHighBoosted = CPCalculator.CalcCPPerLevel(
            raidBoss.Attack, raidBoss.Defense, raidBoss.Stamina,
            CPCalculator.MAX_IV, CPCalculator.MAX_IV,
            CPCalculator.MAX_IV, 
            CPCalculator.RAID_LEVEL + CPCalculator.WEATHER_BOOST);

         dbConnector.GetRaidBossWeather(ref raidBoss);

         return raidBoss;

      }
   }
}
