using System;
using System.Linq;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace PokeStar.ConnectionInterface
{
   /// <summary>
   /// Scrapes The Silph Road raid page to get current raid bosses.
   /// </summary>
   public static class SilphData
   {
      private static Uri RaidBossUrl { get; } = new Uri("https://thesilphroad.com/raid-bosses");
      private const string RaidBossHTMLPattern = "//*[@class = 'col-md-4']";

      private static Uri EggUrl { get; } = new Uri("https://thesilphroad.com/egg-distances");
      private const string EggHTMLPattern = "//*[@class='tab-content']";

      /// <summary>
      /// Gets a list of current raid bosses for a given tier.
      /// </summary>
      /// <param name="tier">Tier of bosses to get.</param>
      /// <returns>List of current raid bosses for the tier.</returns>
      public static List<string> GetRaidBossesTier(int tier)
      {
         Dictionary<int, List<string>> bossList = GetRaidBosses();
         return bossList.ContainsKey(tier) ? bossList[tier] : new List<string>();
      }

      /// <summary>
      /// Gets a list of all current raid bosses.
      /// The list is dependent on the current raid bosses on 
      /// The Silph Road website. A change in the website's format
      /// would constitute a change to this method.
      /// </summary>
      /// <returns>List of current raid bosses.</returns>
      private static Dictionary<int, List<string>> GetRaidBosses()
      {
         int tier = -1;
         bool tierStart = false;
         bool nextInTier = false;
         bool megaTierStarted = false;
         HtmlWeb web = new HtmlWeb();
         HtmlDocument doc = web.Load(RaidBossUrl);
         HtmlNodeCollection bosses = doc.DocumentNode.SelectNodes(RaidBossHTMLPattern);

         Dictionary<int, List<string>> raidBossList = new Dictionary<int, List<string>>();
         foreach (HtmlNode col in bosses)
         {
            string[] words = col.InnerText.Split('\n').Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();
            foreach (string word in words)
            {
               string line = word.Trim();
               int firstSpace = line.IndexOf(' ');
               string checkTier = firstSpace != -1 ? line.Substring(0, firstSpace) : "";

               if (checkTier.Equals(Global.RAID_STRING_MEGA, StringComparison.OrdinalIgnoreCase) && !megaTierStarted)
               {
                  tier = Global.MEGA_RAID_TIER;
                  raidBossList.Add(tier, new List<string>());
                  tierStart = true;
                  nextInTier = false;
                  megaTierStarted = true;
               }
               else if (checkTier.Equals(Global.RAID_STRING_EX, StringComparison.OrdinalIgnoreCase))
               {
                  tier = Global.EX_RAID_TIER;
                  raidBossList.Add(tier, new List<string>());
                  tierStart = true;
                  nextInTier = false;
               }
               else if (checkTier.Equals(Global.RAID_STRING_TIER, StringComparison.OrdinalIgnoreCase))
               {
                  tier = Convert.ToInt32(line.Trim().Substring(Global.RAID_STRING_TIER.Length));
                  raidBossList.Add(tier, new List<string>());
                  tierStart = true;
                  nextInTier = false;
               } 
               else if (line.Equals("8+", StringComparison.OrdinalIgnoreCase))
               {
                  nextInTier = true;
               }
               else if (tierStart)
               {
                  raidBossList[tier].Add(ReformatName(line));
                  tierStart = false;
               }
               else if (nextInTier)
               {
                  raidBossList[tier].Add(ReformatName(line));
                  nextInTier = false;
               }
            }
         }
         return raidBossList;
      }

      public static Dictionary<int, List<string>> GetEggs()
      {
         int eggCategory = 0;
         int pokemonNameOffset = 0;
         HtmlWeb web = new HtmlWeb();
         HtmlDocument doc = web.Load(EggUrl);
         HtmlNodeCollection eggs = doc.DocumentNode.SelectNodes(EggHTMLPattern);

         Dictionary<int, List<string>> eggList = new Dictionary<int, List<string>>();

         foreach (HtmlNode col in eggs)
         {
            string[] words = col.InnerText.Split('\n').Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();
            foreach (string word in words)
            {
               pokemonNameOffset--;
               string line = word.Trim();
               if (line.Contains("KM"))
               {
                  eggCategory = Global.EGG_TIER_TITLE[line];
                  eggList.Add(eggCategory, new List<string>());
               }
               else if (line.IndexOf('#') != -1)
               {
                  pokemonNameOffset = 2;
               }
               else if (pokemonNameOffset == 0)
               {
                  eggList[eggCategory].Add(ReformatName(line));
               }
            }
         }
         return eggList;
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
         else if (name.Equals("GIRATINA (ALTERED FORME)", StringComparison.OrdinalIgnoreCase))
         {
            return "Altered Form Giratina";
         }
         else if (name.Equals("BURMY (PLANT CLOAK)", StringComparison.OrdinalIgnoreCase))
         {
            return "Plant Cloak Burmy";
         }
         else if (name.Equals("BURMY (TRASH CLOAK)", StringComparison.OrdinalIgnoreCase))
         {
            return "Trash Cloak Burmy";
         }
         else if (name.Equals("BURMY (SANDY CLOAK)", StringComparison.OrdinalIgnoreCase))
         {
            return "Sand Cloak Burmy";
         }
         return name;
      }
   }
}