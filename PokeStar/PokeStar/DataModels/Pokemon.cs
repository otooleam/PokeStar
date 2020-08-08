using System;
using System.Collections.Generic;
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
      public bool Regional { get; set; }

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

      /// <summary>
      /// Gets the details of the pokemon as a string.
      /// Details include but are not limited to region, 
      /// category, obtainability, shiny status, shadow status,
      /// and if it is a regional.
      /// </summary>
      /// <returns>Pokemon detail string.</returns>
      public string DetailsToString()
      {
         string str = "";
         str += $"Region           : {Region}\n";
         str += $"Category         : {Category}\n";
         str += $"Buddy Distance   : {BuddyDistance} km\n";
         str += $"Can be Obtained  : {Obtainable}\n";
         str += $"Can be Shiny     : {Shiny}\n";
         str += $"Can be Shadow    : {Shadow}\n";
         str += $"Is a Regional    : {Regional}\n";
         return str.Trim();
      }

      /// <summary>
      /// Gets the stats of the pokemon as a string.
      /// Stats include Max CP, attack, defense, and stamina(hp).
      /// </summary>
      /// <returns>Pokemon stat string.</returns>
      public string StatsToString()
      {
         string str = "";
         str += $"Max CP : {CPMax}\n";
         str += $"Attack : {Attack}\n";
         str += $"Defense: {Defense}\n";
         str += $"Stamina: {Stamina}\n";
         return str.Trim();
      }

      /// <summary>
      /// Gets the type(s) of the pokemon as a string.
      /// </summary>
      /// <returns>Pokemon type string.</returns>
      public string TypeToString()
      {
         string str = "";
         foreach (string type in Type)
         {
            string typeString = Emote.Parse(Environment.GetEnvironmentVariable($"{type.ToUpper()}_EMOTE")).ToString();
            str += typeString + " ";
         }
         return str.Trim();
      }

      /// <summary>
      /// Gets the weather that boosts the pokemon as a string.
      /// </summary>
      /// <returns>Pokemon weather string.</returns>
      public string WeatherToString()
      {
         string str = "";
         foreach (string weather in Weather)
            str += weather + "\n";
         return str.Trim();
      }

      /// <summary>
      /// Gets the types the pokemon is weak to as a string.
      /// </summary>
      /// <returns>Pokemon weakness string0</returns>
      public string WeaknessToString()
      {
         string str = "";
         int count = 0;
         foreach (string type in Weakness)
         {
            string typeString = Emote.Parse(Environment.GetEnvironmentVariable($"{type.ToUpper()}_EMOTE")).ToString();
            str += typeString + " ";
            if (count == 3)
            {
               count = -1;
               str += "\n";
            }
            count++;
         }
         return str.Trim();
      }

      /// <summary>
      /// Gets the types the pokemon is resistant to as a string.
      /// </summary>
      /// <returns>Pokemon resistance string.</returns>
      public string ResistanceToString()
      {
         string str = "";
         int count = 0;
         foreach (string type in Resistance)
         {
            string typeString = Emote.Parse(Environment.GetEnvironmentVariable($"{type.ToUpper()}_EMOTE")).ToString();
            str += typeString + " ";
            if (count == 3)
            {
               count = -1;
               str += "\n";
            }
            count++;
         }
         return str.Trim();
      }

      /// <summary>
      /// Gets the fast moves of the pokemon as a string.
      /// </summary>
      /// <returns>Pokemon fast move string.</returns>
      public string FastMoveToString()
      {
         if (FastMove.Count == 0)
            return "No Fast Move Data";

         string str = "";
         foreach (Move fastMove in FastMove)
         {
            str += fastMove.ToString();
            if (Type.Contains(fastMove.Type))
               str += " *";
            str += "\n";
         }
         return str.Trim();
      }

      /// <summary>
      /// Gets the charge moves of the pokemon as a string.
      /// </summary>
      /// <returns>Pokemon charge move string.</returns>
      public string ChargeMoveToString()
      {
         if (ChargeMove.Count == 0)
            return "No Charge Move Data";

         string str = "";
         foreach (Move chargeMove in ChargeMove)
         {
            str += chargeMove.ToString();
            if (Type.Contains(chargeMove.Type))
               str += " *";
            str += "\n";
         }
         return str.Trim();
      }

      /// <summary>
      /// Gets the top counters of the pokemon as a string.
      /// Note this is not yet implemented.
      /// </summary>
      /// <returns>Pokemon counter string.</returns>
      public string CounterToString()
      {
         /*
          * foreach (Counter counter in Counter)
          *    str += counter.ToString() + "\n";
          * return str;
         /**/
         return "Not Implemented";
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
         string str = "";
         for (int i = 0; i < CPWild.Count; i++)
         {
            str += $"{i + 1}";
            var temp = $"{i + 1}";

            for (int j = temp.Length; j < 20; j++)
               str += "-";
            str += $"{CPWild[i]}";

            if (i >= 30)
               str += "*";
            str += "\n";
         }
         return str;
      }
   }
}