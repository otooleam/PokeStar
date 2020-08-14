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
      /// <summary>
      /// Gets a list of current raid bosses for a given tier.
      /// </summary>
      /// <param name="tier">Tier of bosses to get.</param>
      /// <returns>List of current raid bosses for the tier.</returns>
      public static List<string> GetRaidBossesTier(int tier)
      {
         var raidbossList = GetRaidBosses();
         List<string> bossTier = new List<string>();
         foreach (RaidBossListElement boss in raidbossList)
            if (boss.Tier == tier)
               bossTier.Add(boss.Name);
         return bossTier;
      }

      /// <summary>
      /// Gets a list of all current raid bosses.
      /// The list is dependent on the current raid bosses on 
      /// The Silph Road website. A change in the website's format
      /// would constitute a change to this method.
      /// </summary>
      /// <returns>List of current raid bosses.</returns>
      private static List<RaidBossListElement> GetRaidBosses()
      {
         int tier = -1;
         bool tierStart = false;
         bool nextInTier = false;
         var web = new HtmlWeb();
         var doc = web.Load(Connections.Instance().RAID_BOSS_URL);
         var bosses = doc.DocumentNode.SelectNodes(Connections.RAID_BOSS_HTML);

         List<RaidBossListElement> raidBossList = new List<RaidBossListElement>();

         foreach (var col in bosses)
         {
            string[] words = col.InnerText.Split('\n');
            foreach (string word in words)
            {
               if (!string.IsNullOrEmpty(word.Replace(" ", string.Empty)))
               {
                  var temp = word.Trim();
                  temp = temp.Substring(0, Math.Min(temp.Length, 4));
                  if (temp.Equals("Tier", StringComparison.OrdinalIgnoreCase))
                  {
                     tier = Convert.ToInt32(word.Trim().Substring(4));
                     tierStart = true;
                     nextInTier = false;
                  }
                  else if (temp.Equals("8+", StringComparison.OrdinalIgnoreCase))
                     nextInTier = true;
                  else if (tierStart)
                  {
                     raidBossList.Add(new RaidBossListElement
                     {
                        Tier = tier,
                        Name = ReformatName(word.Trim())
                     });
                     tierStart = false;
                  }
                  else if (nextInTier)
                  {
                     raidBossList.Add(new RaidBossListElement
                     {
                        Tier = tier,
                        Name = ReformatName(word.Trim())
                     });
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
            return "Origin Form Giratina";
         if (name.Equals("GIRATINA (ALTERED FORME)", StringComparison.OrdinalIgnoreCase))
            return "Altered Form Giratina";
         return name;
      }
   }

   /// <summary>
   /// A raid boss scraped from The Silph Road.
   /// </summary>
   public struct RaidBossListElement
   {
      public int Tier;
      public string Name;
   }
}