using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace PokeStar.ConnectionInterface
{
   /// <summary>
   /// Scrapes the silph road raid page to get current raid bosses.
   /// </summary>
   public static class SilphData
   {
      private static Uri RaidBossUrl { get; } = new Uri("https://thesilphroad.com/raid-bosses");
      private const string RaidBossHTMLPattern = "//*[@class = 'col-md-4']";

      /// <summary>
      /// Gets a list of current raid bosses for a given tier.
      /// </summary>
      /// <param name="tier">Tier of bosses to get.</param>
      /// <returns>List of current raid bosses for the tier.</returns>
      public static List<string> GetRaidBossesTier(int tier)
      {
         List<Tuple<int, string>> raidbossList = GetRaidBosses();
         List<string> bossTier = new List<string>();
         foreach (Tuple<int, string> boss in raidbossList)
         {
            if (boss.Item1 == tier)
            {
               bossTier.Add(boss.Item2);
            }
         }
         return bossTier;
      }

      /// <summary>
      /// Gets a list of all current raid bosses.
      /// The list is dependent on the current raid bosses on 
      /// The Silph Road website. A change in the website's format
      /// would constitute a change to this method.
      /// </summary>
      /// <returns>List of current raid bosses.</returns>
      private static List<Tuple<int, string>> GetRaidBosses()
      {
         int tier = -1;
         bool tierStart = false;
         bool nextInTier = false;
         bool exTierStarted = true; // Change to false when ex raids are back
         bool megaTierStarted = false;
         HtmlWeb web = new HtmlWeb();
         HtmlDocument doc = web.Load(RaidBossUrl);
         HtmlNodeCollection bosses = doc.DocumentNode.SelectNodes(RaidBossHTMLPattern);

         List<Tuple<int, string>> raidBossList = new List<Tuple<int, string>>();

         foreach (HtmlNode col in bosses)
         {
            string[] words = col.InnerText.Split('\n');
            foreach (string word in words)
            {
               if (!string.IsNullOrEmpty(word.Replace(" ", string.Empty)))
               {
                  string temp = word.Trim();
                  temp = temp.Substring(0, Math.Min(temp.Length, 4));
                  if (temp.Equals("Tier", StringComparison.OrdinalIgnoreCase))
                  {
                     tier = Convert.ToInt32(word.Trim().Substring(4));
                     tierStart = true;
                     nextInTier = false;
                  }
                  else if (temp.Equals("ex", StringComparison.OrdinalIgnoreCase) && !exTierStarted)
                  {
                     tier = 9;
                     tierStart = true;
                     nextInTier = false;
                     exTierStarted = true;
                  }
                  else if (temp.Equals("Mega", StringComparison.OrdinalIgnoreCase) && !megaTierStarted)
                  {
                     tier = 7;
                     tierStart = true;
                     nextInTier = false;
                     megaTierStarted = true;
                  }
                  else if (temp.Equals("8+", StringComparison.OrdinalIgnoreCase))
                  {
                     nextInTier = true;
                  }
                  else if (tierStart)
                  {
                     raidBossList.Add(new Tuple<int, string>(tier, ReformatName(word.Trim())));
                     tierStart = false;
                  }
                  else if (nextInTier)
                  {
                     raidBossList.Add(new Tuple<int, string>(tier, ReformatName(word.Trim())));
                     nextInTier = false;
                  }
               }
            }
         }
         return raidBossList;
      }

      /// <summary>
      /// Reformats names to comply with the database names.
      /// </summary>
      /// <param name="name">Name read from The Silph Road.</param>
      /// <returns>Name formated for the database.</returns>
      private static string ReformatName(string name)
      {
         if (name.Equals("GIRATINA (ORIGIN FORME)", StringComparison.OrdinalIgnoreCase))
         {
            return "Origin Form Giratina";
         }
         if (name.Equals("GIRATINA (ALTERED FORME)", StringComparison.OrdinalIgnoreCase))
         {
            return "Altered Form Giratina";
         }
         return name;
      }
   }
}