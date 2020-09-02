using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Pokemon data.
   /// </summary>
   public class Pokemon
   {
      /// <summary>
      /// Number of the pokemon.
      /// </summary>
      public int Number { get; set; }

      /// <summary>
      /// Name of the pokemon.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// Description of the pokemon.
      /// </summary>
      public string Description { get; set; }

      /// <summary>
      /// Base attack of the pokemon.
      /// </summary>
      public int Attack { get; set; }

      /// <summary>
      /// Base defense of the pokemon.
      /// </summary>
      public int Defense { get; set; }

      /// <summary>
      /// Base stamina(hp) of the pokemon.
      /// </summary>
      public int Stamina { get; set; }

      /// <summary>
      /// List of the pokemon's type(s).
      /// </summary>
      public List<string> Type { get; } = new List<string>();

      /// <summary>
      /// List of weather that boosts the pokemon.
      /// </summary>
      public List<string> Weather { get; set; }

      /// <summary>
      /// List of types the pokemon is weak to.
      /// </summary>
      public List<string> Weakness { get; set; }

      /// <summary>
      /// List of types the pokemon is resistant to.
      /// </summary>
      public List<string> Resistance { get; set; }

      /// <summary>
      /// Region the pokemon is originally from.
      /// </summary>
      public string Region { get; set; }

      /// <summary>
      /// Category of the pokemon.
      /// </summary>
      public string Category { get; set; }

      /// <summary>
      /// Buddy distance of the pokemon.
      /// </summary>
      public int BuddyDistance { get; set; }

      /// <summary>
      /// Is the pokemon obtainable.
      /// </summary>
      public bool Obtainable { get; set; }

      /// <summary>
      /// Is the pokemon shinyable.
      /// </summary>
      public bool Shiny { get; set; }

      /// <summary>
      /// Is the pokemon shadowable.
      /// </summary>
      public bool Shadow { get; set; }

      /// <summary>
      /// Is the pokemon a regional.
      /// </summary>
      public string Regional { get; set; }

      /// <summary>
      /// List of the pokemon's fast moves.
      /// </summary>
      public List<Move> FastMove { get; set; }

      /// <summary>
      /// List of the pokemon's charge moves.
      /// </summary>
      public List<Move> ChargeMove { get; set; }

      /// <summary>
      /// List of counters of the pokemon.
      /// </summary>
      public List<Counter> Counter { get; set; }

      /// <summary>
      /// Maximum CP of the pokemon.
      /// </summary>
      public int CPMax { get; set; }

      /// <summary>
      /// Maximum best buddy CP of the pokemon.
      /// </summary>
      public int CPBestBuddy { get; set; }

      /// <summary>
      /// Minimum raid CP of the pokemon.
      /// </summary>
      public int CPRaidMin { get; set; }

      /// <summary>
      /// Maximum raid CP of the pokemon.
      /// </summary>
      public int CPRaidMax { get; set; }

      /// <summary>
      /// Minimum weather boosted CP of the pokemon.
      /// </summary>
      public int CPRaidBoostedMin { get; set; }

      /// <summary>
      /// Maximum weather boosted CP of the pokemon.
      /// </summary>
      public int CPRaidBoostedMax { get; set; }

      /// <summary>
      /// Minimum quest CP of the pokemon.
      /// </summary>
      public int CPQuestMin { get; set; }

      /// <summary>
      /// Maximum quest CP of the pokemon.
      /// </summary>
      public int CPQuestMax { get; set; }

      /// <summary>
      /// Minimum hatch CP of the pokemon.
      /// </summary>
      public int CPHatchMin { get; set; }

      /// <summary>
      /// Maximum hatch CP of the pokemon.
      /// </summary>
      public int CPHatchMax { get; set; }

      /// <summary>
      /// List of maximum wild CP of the pokemon.
      /// </summary>
      public List<int> CPWild { get; } = new List<int>();

      public bool IsRegional()
      {
         return Regional != null;
      }

      /// <summary>
      /// Gets the details of the pokemon as a string.
      /// Details include but are not limited to region, 
      /// category, obtainability, shiny status, shadow status,
      /// and if it is a regional.
      /// </summary>
      /// <returns>Pokemon detail string.</returns>
      public string DetailsToString()
      {
         StringBuilder sb = new StringBuilder();

         sb.AppendLine($"Region\t: {Region}");
         sb.AppendLine($"Category\t: {Category}");
         sb.AppendLine($"Buddy Distance\t: {BuddyDistance} km");
         sb.AppendLine($"Can be Obtained\t: {(Obtainable ? "Yes" : "No")}");
         sb.AppendLine($"Can be Shiny\t: {(Shiny ? "Yes" : "No")}");
         sb.AppendLine($"Can be Shadow\t: {(Shadow ? "Yes" : "No")}");

         return sb.ToString();
      }

      public string RegionalToString()
      {
         List<string> regions = Regional.Split(',').ToList();
         StringBuilder sb = new StringBuilder();
         foreach (string r in regions)
         {
            sb.AppendLine($"-{r}\n");
         }
         return sb.ToString().Trim();
      }

      /// <summary>
      /// Gets the stats of the pokemon as a string.
      /// Stats include Max CP, attack, defense, and stamina(hp).
      /// </summary>
      /// <returns>Pokemon stat string.</returns>
      public string StatsToString() 
      {
         StringBuilder sb = new StringBuilder();

         sb.AppendLine($"Max CP : {CPMax}");
         sb.AppendLine($"Attack : {Attack}");
         sb.AppendLine($"Defense: {Defense}");
         sb.AppendLine($"Stamina: {Stamina}");

         return sb.ToString();
      }

      /// <summary>
      /// Gets the type(s) of the pokemon as a string.
      /// </summary>
      /// <returns>Pokemon type string.</returns>
      public string TypeToString()
      {
         StringBuilder sb = new StringBuilder();

         foreach (string type in Type)
         {
            sb.Append($"{Global.NONA_EMOJIS[$"{type}_emote"]} ");
         }

         return sb.ToString();
      }

      /// <summary>
      /// Gets the weather that boosts the pokemon as a string.
      /// </summary>
      /// <returns>Pokemon weather string.</returns>
      public string WeatherToString()
      {
         StringBuilder sb = new StringBuilder();
         foreach (string weather in Weather)
         {
            sb.Append($"{Global.NONA_EMOJIS[$"{weather.Replace(' ','_')}_emote"]} ");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Gets the types the pokemon is weak to as a string.
      /// </summary>
      /// <returns>Pokemon weakness string0</returns>
      public string WeaknessToString()
      {
         StringBuilder sb = new StringBuilder();
         int count = 0;
         foreach (string type in Weakness)
         {
            sb.Append($"{Global.NONA_EMOJIS[$"{type}_emote"]} ");
            if (count == 3)
            {
               count = -1;
               sb.AppendLine();
            }
            count++;
         }
         return sb.ToString();
      }

      /// <summary>
      /// Gets the types the pokemon is resistant to as a string.
      /// </summary>
      /// <returns>Pokemon resistance string.</returns>
      public string ResistanceToString()
      {
         StringBuilder sb = new StringBuilder();
         int count = 0;
         foreach (string type in Resistance)
         {
            sb.Append($"{Global.NONA_EMOJIS[$"{type}_emote"]} ");
            if (count == 3)
            {
               count = -1;
               sb.AppendLine();
            }
            count++;
         }
         return sb.ToString();
      }

      /// <summary>
      /// Gets the fast moves of the pokemon as a string.
      /// </summary>
      /// <returns>Pokemon fast move string.</returns>
      public string FastMoveToString()
      {
         if (FastMove.Count == 0)
         {
            return "No Fast Move Data";
         }

         StringBuilder sb = new StringBuilder();
         foreach (Move fastMove in FastMove)
         {
            if (Name.Equals("Mew"))
            {
               sb.Append($"{fastMove.Name} ({fastMove.Type})");
            }
            else
            {
               sb.Append(fastMove.ToString());
            }
            if (Type.Contains(fastMove.Type))
            {
               sb.Append(" *");
            }
            sb.AppendLine();
         }
         return sb.ToString();
      }

      /// <summary>
      /// Gets the charge moves of the pokemon as a string.
      /// </summary>
      /// <returns>Pokemon charge move string.</returns>
      public string ChargeMoveToString()
      {
         if (ChargeMove.Count == 0)
         {
            return "No Charge Move Data";
         }

         StringBuilder sb = new StringBuilder();
         foreach (Move chargeMove in ChargeMove)
         {
            if (Name.Equals("Mew"))
            {
               sb.Append($"{chargeMove.Name} ({chargeMove.Type})");
            }
            else
            {
               sb.Append(chargeMove.ToString());
            }
            if (Type.Contains(chargeMove.Type))
            {
               sb.Append(" *");
            }
            sb.AppendLine();
         }
         return sb.ToString();
      }

      /// <summary>
      /// Gets the top counters of the pokemon as a string.
      /// Note this is not yet implemented.
      /// </summary>
      /// <returns>Pokemon counter string.</returns>
      public string CounterToString()
      {
         if (Counter.Count == 0)
         {
            return "No Counters Listed.";
         }
         string str = "";
         int num = 1;
         foreach (Counter counter in Counter)
         {
            str += $"#{num++} {counter}\n";
         }
         return str;
      }

      /// <summary>
      /// Gets raid CPs of the pokemon as a string.
      /// Includes min and max CPs and weather boosted
      /// min and max CPs.
      /// </summary>
      /// <returns>Pokemon raid CP string.</returns>
      public string RaidCPToString()
      {
         return $"{CPRaidMin} - {CPRaidMax}\n{CPRaidBoostedMin}* - {CPRaidBoostedMax}*";
      }

      /// <summary>
      /// Gets the min and max quest CPs of the pokemon as a string.
      /// </summary>
      /// <returns>Pokemon quest CP string.</returns>
      public string QuestCPToString()
      {
         return $"{CPQuestMin} - {CPQuestMax}";
      }

      /// <summary>
      /// Gets the min and max hatch CPs of the pokemon as a string.
      /// </summary>
      /// <returns>Pokemon hatch CP string.</returns>
      public string HatchCPToString()
      {
         return $"{CPHatchMin} - {CPHatchMax}";
      }

      /// <summary>
      /// Gets all max wild CPs of the pokemon as a string.
      /// Wild CP goes from level 1 to 35 in full level 
      /// increments.
      /// </summary>
      /// <returns>Pokemon max wild CP string.</returns>
      public string WildCPToString()
      {
         StringBuilder sb = new StringBuilder();
         string dash = "-";
         int columnLength = 12;

         for (int i = 0; i < columnLength; i++)
         {
            int column1level = i + 1;
            int column2level = i + 1 + columnLength;
            int column3level = i + 1 + columnLength * 2;

            sb.Append($"**{column1level}** {(i >= 10 ? dash : dash)} {CPWild[column1level - 1]} . ");
            sb.Append($"**{column2level}** {dash} {CPWild[column2level - 1]}");
            sb.AppendLine($"{(column3level <= 35 ? $" . **{column3level}** {dash} {CPWild[column3level - 1]}{(column3level > 30 ? "\\*" : "")}" : "")}");
         }

         return sb.ToString();
      }
   }
}