using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace PokeStar.ConnectionInterface
{
   public static class SilphData
   {
      public static List<string> GetRaidBossesTier(int tier)
      {
         var raidbossList = GetRaidBosses();
         List<string> bossTier = new List<string>();
         foreach (RaidBossListElement boss in raidbossList)
            if (boss.Tier == tier)
               bossTier.Add(boss.Name);
         return bossTier;
      }

      public static List<RaidBossListElement> GetRaidBosses()
      {
         bool tierStart = false;
         bool nextInTier = false;
         var web = new HtmlWeb();
         var doc = web.Load(Connections.Instance().RAID_BOSS_URL);
         var bosses = doc.DocumentNode.SelectNodes(Connections.Instance().RAID_BOSS_HTML);

         List<RaidBossListElement> raidBossList = new List<RaidBossListElement>();
         int tier = -1;

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
                        Name = word.Trim()
                     });
                     tierStart = false;
                  }

                  else if (nextInTier)
                  {
                     raidBossList.Add(new RaidBossListElement
                     {
                        Tier = tier,
                        Name = word.Trim()
                     });
                     nextInTier = false;
                  }
               }
            }
         }
         return raidBossList;
      }
   }
   public struct RaidBossListElement
   {
      public int Tier { get; set; }
      public string Name { get; set; }
   }
}
