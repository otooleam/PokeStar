using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using PokeStar.DataModels;

namespace PokeStar.ConnectionInterface
{
   /// <summary>
   /// Scrapes The Silph Road raid page to get current raid bosses.
   /// </summary>
   public static class SilphData
   {
      /// <summary>
      /// Values used to scrape raid bosses.
      /// </summary>
      private static Uri RaidBossUrl { get; } = new Uri("https://thesilphroad.com/raid-bosses");
      private const string RaidBossHTMLPattern = "//*[@class='raid-boss-tiers-wrap']";

      /// <summary>
      /// Values used to scrape egg pools.
      /// </summary>
      private static Uri EggUrl { get; } = new Uri("https://thesilphroad.com/egg-distances");
      private const string EggHTMLPattern = "//*[@class='tab-content']";

      /// <summary>
      /// Values used to scrape rocket line ups.
      /// </summary>
      private static Uri RocketUrl { get; } = new Uri("https://thesilphroad.com/rocket-invasions");
      private const string RocketHTMLPattern = "//*[@class='lineupGroup normalGroup']";
      private const string RocketLeaderHTMLPattern = "//*[@class='lineupGroup specialGroup']";

      /// <summary>
      /// Value used to remove HTML tags.
      /// </summary>
      private const string HTML_TAG_PATTERN = "<.*?>";

      private const int SILPH_GUILD_NUM = 0;

      /// <summary>
      /// Gets a list of all current raid bosses.
      /// The list is dependent on the current raid bosses on 
      /// The Silph Road website. A change in the website's format
      /// would constitute a change to this method.
      /// </summary>
      /// <returns>Dictionary of current raid bosses.</returns>
      public static Dictionary<int, List<string>> GetRaidBosses()
      {
         int tier = int.MinValue;
         HtmlWeb web = new HtmlWeb();
         HtmlDocument doc = web.Load(RaidBossUrl);
         HtmlNodeCollection bosses = doc.DocumentNode.SelectNodes(RaidBossHTMLPattern);

         Dictionary<int, List<string>> raidBossList = new Dictionary<int, List<string>>();

         foreach (HtmlNode col in bosses)
         {
            string[] words = col.InnerHtml.Split('\n').Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();
            for (int i = 0; i < words.Length; i++)
            {
               string line = words[i] = words[i].Trim().Replace("&#39;", "\'");
               if (line.Contains("<div class=\"raid-boss-tier-wrap\">"))
               {
                  tier = -1;
               }
               else if (tier == -1)
               {
                  tier = 0;
               }
               else if (tier == 0)
               {
                  tier = Global.RAID_TIER_TITLE[Regex.Replace(line, HTML_TAG_PATTERN, string.Empty).Trim()];
                  raidBossList.Add(tier, new List<string>());
               }
               else if (line.StartsWith("<div class=\"boss-name\">", StringComparison.OrdinalIgnoreCase))
               {
                  raidBossList[tier].Add(ReformatName(Regex.Replace(line, HTML_TAG_PATTERN, string.Empty).Trim()));
               }
            }
         }
         return raidBossList;
      }

      /// <summary>
      /// Gets a list of all current egg pools.
      /// Thie list is dependent on the current egg pools on
      /// The Silph Road website. A change in the website's format
      /// would constitute a change to this method.
      /// </summary>
      /// <returns>Dictionary of Pokémon currently in eggs.</returns>
      public static Dictionary<int, List<string>> GetEggs()
      {
         int eggCategory = 0;
         string poke = "";
         bool pokemonFound = false;
         HtmlWeb web = new HtmlWeb();
         HtmlDocument doc = web.Load(EggUrl);
         HtmlNodeCollection eggs = doc.DocumentNode.SelectNodes(EggHTMLPattern);

         Dictionary<int, List<string>> eggList = new Dictionary<int, List<string>>();

         foreach (HtmlNode col in eggs)
         {
            string[] words = col.InnerHtml.Split('\n').Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();
            for (int i = 0; i < words.Length; i++)
            {
               string line = words[i] = words[i].Trim().Replace("&#39;", "\'");
               if (line.Contains("KM"))
               {
                  eggCategory = Global.EGG_TIER_TITLE[Regex.Replace(line, HTML_TAG_PATTERN, string.Empty).Trim()];
                  eggList.Add(eggCategory, new List<string>());
               }
               else if (line.Contains("<img class=\"regionalIcon\""))
               {
                  poke += Global.EGG_REGIONAL_SYMBOL;
               }
               else if (line.StartsWith("<p class=\"speciesName\">", StringComparison.OrdinalIgnoreCase))
               {
                  pokemonFound = true;
               }
               else if (pokemonFound)
               {
                  eggList[eggCategory].Add(ReformatName(Regex.Replace(line, HTML_TAG_PATTERN, string.Empty).Trim()) + poke);
                  poke = "";
                  pokemonFound = false;
               }

            }
         }
         return eggList;
      }

      /// <summary>
      /// Gets a list of all current Rocket Grunts.
      /// Thie list is dependent on the current rocket grunts on
      /// The Silph Road website. A change in the website's format
      /// would constitute a change to this method.
      /// </summary>
      /// <returns>Dictionary of line ups used by rocket grunts.</returns>
      public static Dictionary<string, Rocket> GetRockets()
      {
         HtmlWeb web = new HtmlWeb();
         HtmlDocument doc = web.Load(RocketUrl);
         HtmlNodeCollection rockets = doc.DocumentNode.SelectNodes(RocketHTMLPattern);

         Dictionary<string, Rocket> rocketList = new Dictionary<string, Rocket>();

         foreach (HtmlNode col in rockets)
         {
            int slot = -1;
            string[] words = col.InnerHtml.Split('\n').Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();
            Rocket rocket = new Rocket();
            string type = "";
            string phrase = "";
            string poke = "";

            for (int i = 0; i < words.Length; i++)
            {
               string line = words[i] = words[i].Trim().Replace("&#39;", "\'");

               if (line.StartsWith("<p class=\"quote\">", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrEmpty(phrase))
               {
                  phrase += Regex.Replace(line, HTML_TAG_PATTERN, string.Empty) + '\n';
               }
               else if (line.StartsWith("<h3", StringComparison.OrdinalIgnoreCase))
               {
                  type = Regex.Replace(line, HTML_TAG_PATTERN, string.Empty);
                  rocket.SetGrunt(type);
               }
               else if (line.StartsWith("<div class=\"lineupSlot\">", StringComparison.OrdinalIgnoreCase))
               {
                  slot++;
               }
               else if (line.StartsWith("<p class=\"speciesName\">", StringComparison.OrdinalIgnoreCase))
               {
                  rocket.Slots[slot].Add(ReformatName(Regex.Replace(line, HTML_TAG_PATTERN, string.Empty)) + poke);
                  poke = "";
               }
               else if (line.StartsWith("<img class=\"pokeballIcon", StringComparison.OrdinalIgnoreCase))
               {
                  poke = Global.ROCKET_CATCH_SYMBOL + poke;
               }
               else if (line.StartsWith("<span class=\"unverifiedIcon\">", StringComparison.OrdinalIgnoreCase))
               {
                  poke += Global.UNVERIFIED_SYMBOL;
               }
               if (!string.IsNullOrEmpty(phrase) && line.EndsWith("</p>", StringComparison.OrdinalIgnoreCase))
               {
                  rocket.Phrase = phrase;
                  phrase = "";
               }
            }
            rocketList.Add(type, rocket);
         }
         return rocketList;
      }

      /// <summary>
      /// Gets a list of all current Rocket Grunts.
      /// Thie list is dependent on the current rocket leaders on
      /// The Silph Road website. A change in the website's format
      /// would constitute a change to this method.
      /// </summary>
      /// <returns>Dictionary of line ups used by rocket leaders.</returns>
      public static Dictionary<string, Rocket> GetRocketLeaders()
      {
         HtmlWeb web = new HtmlWeb();
         HtmlDocument doc = web.Load(RocketUrl);
         HtmlNodeCollection leaders = doc.DocumentNode.SelectNodes(RocketLeaderHTMLPattern);

         Dictionary<string, Rocket> leaderList = new Dictionary<string, Rocket>();

         foreach (HtmlNode col in leaders)
         {
            int slot = -1;
            string[] words = col.InnerHtml.Split('\n').Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();
            Rocket rocket = new Rocket();
            string name = "";
            string poke = "";

            for (int i = 0; i < words.Length; i++)
            {
               string line = words[i] = words[i].Trim().Replace("&#39;", "\'");

               if (line.StartsWith("<h3", StringComparison.OrdinalIgnoreCase))
               {
                  name = Regex.Replace(line, HTML_TAG_PATTERN, string.Empty);
                  rocket.SetLeader(name);
               }
               else if (line.StartsWith("<div class=\"lineupSlot\">", StringComparison.OrdinalIgnoreCase))
               {
                  slot++;
               }
               else if (line.StartsWith("<p class=\"speciesName\">", StringComparison.OrdinalIgnoreCase))
               {
                  rocket.Slots[slot].Add(ReformatName(Regex.Replace(line, HTML_TAG_PATTERN, string.Empty)) + poke);
                  poke = "";
               }
               else if (line.StartsWith("<img class=\"pokeballIcon", StringComparison.OrdinalIgnoreCase))
               {
                  poke = Global.ROCKET_CATCH_SYMBOL + poke;
               }
               else if (line.StartsWith("<span class=\"unverifiedIcon\">", StringComparison.OrdinalIgnoreCase))
               {
                  poke += Global.UNVERIFIED_SYMBOL;
               }
            }
            leaderList.Add(rocket.Name, rocket);
         }
         return leaderList;
      }

      /// <summary>
      /// Reformats names to comply with the database names.
      /// </summary>
      /// <param name="name">Name read from The Silph Road.</param>
      /// <returns>Name formated for the database.</returns>
      private static string ReformatName(string name)
      {
         string silphName = name.IndexOf('’') != -1 ? name.Replace('’', '\'') : name;
         return Connections.Instance().GetPokemonWithNickname(SILPH_GUILD_NUM, silphName) ?? name;
      }
   }
}