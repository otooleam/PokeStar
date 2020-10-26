using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Pokémon data.
   /// </summary>
   public class Pokemon
   {
      /// <summary>
      /// Number of the Pokémon.
      /// </summary>
      public int Number { get; set; }

      /// <summary>
      /// Name of the Pokémon.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// List of alternate forms.
      /// </summary>
      public List<string> Forms { get; set; }

      /// <summary>
      /// Description of the Pokémon.
      /// </summary>
      public string Description { get; set; }

      /// <summary>
      /// Base attack of the Pokémon.
      /// </summary>
      public int Attack { get; set; }

      /// <summary>
      /// Base defense of the Pokémon.
      /// </summary>
      public int Defense { get; set; }

      /// <summary>
      /// Base stamina(hp) of the Pokémon.
      /// </summary>
      public int Stamina { get; set; }

      /// <summary>
      /// List of the Pokémon's type(s).
      /// </summary>
      public List<string> Type { get; } = new List<string>();

      /// <summary>
      /// List of weather that boosts the Pokémon.
      /// </summary>
      public List<string> Weather { get; set; }

      /// <summary>
      /// List of types the Pokémon is weak to.
      /// </summary>
      public List<string> Weakness { get; set; }

      /// <summary>
      /// List of types the Pokémon is resistant to.
      /// </summary>
      public List<string> Resistance { get; set; }

      /// <summary>
      /// Region the Pokémon is originally from.
      /// </summary>
      public string Region { get; set; }

      /// <summary>
      /// Category of the Pokémon.
      /// </summary>
      public string Category { get; set; }

      /// <summary>
      /// Buddy distance of the Pokémon.
      /// </summary>
      public int BuddyDistance { get; set; }

      /// <summary>
      /// Is the Pokémon obtainable.
      /// </summary>
      public bool Obtainable { get; set; }

      /// <summary>
      /// Is the Pokémon shinyable.
      /// </summary>
      public bool Shiny { get; set; }

      /// <summary>
      /// Is the Pokémon shadowable.
      /// </summary>
      public bool Shadow { get; set; }

      /// <summary>
      /// Is the Pokémon a regional.
      /// </summary>
      public string Regional { get; set; }

      /// <summary>
      /// List of the Pokémon's fast moves.
      /// </summary>
      public List<PokemonMove> FastMove { get; set; }

      /// <summary>
      /// List of the Pokémon's charge moves.
      /// </summary>
      public List<PokemonMove> ChargeMove { get; set; }

      /// <summary>
      /// List of counters of the Pokémon.
      /// </summary>
      public List<Counter> Counter { get; set; }

      /// <summary>
      /// Maximum CP of the Pokémon.
      /// </summary>
      public int CPMax { get; set; }

      /// <summary>
      /// Maximum best buddy CP of the Pokémon.
      /// </summary>
      public int CPBestBuddy { get; set; }

      /// <summary>
      /// Minimum raid CP of the Pokémon.
      /// </summary>
      public int CPRaidMin { get; set; }

      /// <summary>
      /// Maximum raid CP of the Pokémon.
      /// </summary>
      public int CPRaidMax { get; set; }

      /// <summary>
      /// Minimum weather boosted CP of the Pokémon.
      /// </summary>
      public int CPRaidBoostedMin { get; set; }

      /// <summary>
      /// Maximum weather boosted CP of the Pokémon.
      /// </summary>
      public int CPRaidBoostedMax { get; set; }

      /// <summary>
      /// Minimum quest CP of the Pokémon.
      /// </summary>
      public int CPQuestMin { get; set; }

      /// <summary>
      /// Maximum quest CP of the Pokémon.
      /// </summary>
      public int CPQuestMax { get; set; }

      /// <summary>
      /// Minimum hatch CP of the Pokémon.
      /// </summary>
      public int CPHatchMin { get; set; }

      /// <summary>
      /// Maximum hatch CP of the Pokémon.
      /// </summary>
      public int CPHatchMax { get; set; }

      /// <summary>
      /// List of maximum wild CP of the Pokémon.
      /// </summary>
      public List<int> CPWild { get; } = new List<int>();

      /// <summary>
      /// Best IVs for Great League.
      /// </summary>
      public LeagueIV GreatIVs { get; set; }

      /// <summary>
      /// Best IVs for Ultra League.
      /// </summary>
      public LeagueIV UltraIVs { get; set; }

      /// <summary>
      /// Checks if the Pokémon is region locked.
      /// </summary>
      /// <returns>True if the Pokémon is region locked, otherwise false.</returns>
      public bool IsRegional()
      {
         return Regional != null;
      }

      /// <summary>
      /// Checks if the Pokémon has alternate forms.
      /// </summary>
      /// <returns>True if the Pokémon has alternate forms, otherwise false.</returns>
      public bool HasForms()
      {
         return Forms.Count > 1;
      }

      /// <summary>
      /// Gets the details of the Pokémon as a string.
      /// Details include but are not limited to region, 
      /// category, obtainability, shiny status, and
      /// shadow status.
      /// </summary>
      /// <returns>Pokémon detail string.</returns>
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

      /// <summary>
      /// Gets the regions to find the Pokémon as a string.
      /// </summary>
      /// <returns>Pokémon region string.</returns>
      public string RegionalToString()
      {
         List<string> regions = Regional.Split(',').ToList();
         StringBuilder sb = new StringBuilder();
         foreach (string region in regions)
         {
            sb.AppendLine(region);
         }
         return sb.ToString().Trim();
      }

      /// <summary>
      /// Gets the forms of the Pokémon as a string.
      /// </summary>
      /// <returns>Pokémon form string.</returns>
      public string FormsToString()
      {
         StringBuilder sb = new StringBuilder();
         foreach (string form in Forms)
         {
            sb.AppendLine(form);
         }
         return sb.ToString().Trim();
      }

      /// <summary>
      /// Gets the stats of the Pokémon as a string.
      /// Stats include Max CP, attack, defense, and stamina(hp).
      /// </summary>
      /// <returns>Pokémon stats string.</returns>
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
      /// Gets the type(s) of the Pokémon as a string.
      /// </summary>
      /// <returns>Pokémon type string.</returns>
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
      /// Gets the weather that boosts the Pokémon as a string.
      /// </summary>
      /// <returns>Pokémon weather string.</returns>
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
      /// Gets the types the Pokémon is weak to as a string.
      /// </summary>
      /// <returns>Pokémon weakness string.</returns>
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
      /// Gets the types the Pokémon is resistant to as a string.
      /// </summary>
      /// <returns>Pokémon resistance string.</returns>
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
      /// Gets the fast moves of the Pokémon as a string.
      /// </summary>
      /// <returns>Pokémon fast move string.</returns>
      public string FastMoveToString()
      {
         if (FastMove.Count == 0)
         {
            return "No Fast Move Data";
         }

         StringBuilder sb = new StringBuilder();
         foreach (PokemonMove fastMove in FastMove)
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
               sb.Append($" {Global.STAB_SYMBOL}");
            }
            sb.AppendLine();
         }
         return sb.ToString();
      }

      /// <summary>
      /// Gets the charge moves of the Pokémon as a string.
      /// </summary>
      /// <returns>Pokémon charge move string.</returns>
      public string ChargeMoveToString()
      {
         if (ChargeMove.Count == 0)
         {
            return "No Charge Move Data";
         }

         StringBuilder sb = new StringBuilder();
         foreach (PokemonMove chargeMove in ChargeMove)
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
               sb.Append($" {Global.STAB_SYMBOL}");
            }
            sb.AppendLine();
         }
         return sb.ToString();
      }

      /// <summary>
      /// Gets the top counters of the Pokémon as a string.
      /// </summary>
      /// <returns>Pokémon counter string.</returns>
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
      /// Gets raid CPs of the Pokémon as a string.
      /// Includes min and max CPs and weather boosted
      /// min and max CPs.
      /// </summary>
      /// <returns>Pokémon raid CP string.</returns>
      public string RaidCPToString()
      {
         return $"{CPRaidMin} - {CPRaidMax}\n{CPRaidBoostedMin}{Global.WEATHER_BOOST_SYMBOL} - {CPRaidBoostedMax}{Global.WEATHER_BOOST_SYMBOL}";
      }

      /// <summary>
      /// Gets the min and max quest CPs of the Pokémon as a string.
      /// </summary>
      /// <returns>Pokémon quest CP string.</returns>
      public string QuestCPToString()
      {
         return $"{CPQuestMin} - {CPQuestMax}";
      }

      /// <summary>
      /// Gets the min and max hatch CPs of the Pokémon as a string.
      /// </summary>
      /// <returns>Pokémon hatch CP string.</returns>
      public string HatchCPToString()
      {
         return $"{CPHatchMin} - {CPHatchMax}";
      }

      /// <summary>
      /// Gets all max wild CPs of the Pokémon as a string.
      /// Wild CP goes from level 1 to 35 in full level 
      /// increments.
      /// </summary>
      /// <returns>Pokémon max wild CP string.</returns>
      public string WildCPToString()
      {
         StringBuilder sb = new StringBuilder();
         int maxNonBoostedLevel = Global.MAX_WILD_LEVEL - Global.WEATHER_BOOST;

         for (int i = 0; i < Global.WILD_CP_COLUMN_LENGTH; i++)
         {
            int column1level = i + 1;
            int column2level = i + 1 + Global.WILD_CP_COLUMN_LENGTH;
            int column3level = i + 1 + Global.WILD_CP_COLUMN_LENGTH  + Global.WILD_CP_COLUMN_LENGTH;

            sb.Append($"**{column1level}** {(i < 10 ? "--" : "-")} {CPWild[column1level - 1]} . ");
            sb.Append($"**{column2level}** - {CPWild[column2level - 1]}");
            sb.AppendLine($"{(column3level <= Global.MAX_WILD_LEVEL ? $" . **{column3level}** - {CPWild[column3level - 1]}{(column3level > maxNonBoostedLevel ? Global.WEATHER_BOOST_SYMBOL.ToString() : "")}" : "")}");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Gets league IVs as a string.
      /// </summary>
      /// <returns></returns>
      public string LeagueIVToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.AppendLine($"*Great League:*\n {GreatIVs}");
         sb.AppendLine($"*Ultra League:*\n {UltraIVs}");
         return sb.ToString();
      }
   }
}