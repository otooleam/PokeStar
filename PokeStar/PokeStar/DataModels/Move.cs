using System;
using System.Text;
using System.Collections.Generic;

namespace PokeStar.DataModels
{
   /// <summary>
   /// A Move for a Pokémon.
   /// </summary>
   public class Move
   {
      /// <summary>
      /// Name of the move.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// Type of the move.
      /// </summary>
      public string Type { get; set; }

      /// <summary>
      /// List of weather that boosts the move.
      /// </summary>
      public List<string> Weather { get; set; }

      /// <summary>
      /// Category of the move.
      /// </summary>
      public string Category { get; set; }

      /// <summary>
      /// Power of the move used for PvE.
      /// </summary>
      public int PvEPower { get; set; }

      /// <summary>
      /// Energy of the move used for PvE.
      /// </summary>
      public int PvEEnergy { get; set; }

      /// <summary>
      /// Power of the move used for PvP.
      /// </summary>
      public int PvPPower { get; set; }

      /// <summary>
      /// Energy of the move used for PvP.
      /// </summary>
      public int PvPEnergy { get; set; }

      /// <summary>
      /// Number of turns used for PvP.
      /// </summary>
      public int PvPTurns { get; set; }

      /// <summary>
      /// Cooldown for the move used for PvE.
      /// </summary>
      public int Cooldown { get; set; }

      /// <summary>
      /// Start of the damage window for PvE.
      /// </summary>
      public int DamageWindowStart { get; set; }

      /// <summary>
      /// End of the damage window for PvE.
      /// </summary>
      public int DamageWindowEnd { get; set; }

      /// <summary>
      /// Change to attacker's attack stat.
      /// </summary>
      public int AttackerAtk { get; set; }

      /// <summary>
      /// Change to attacker's defense stat.
      /// </summary>
      public int AttackerDef { get; set; }

      /// <summary>
      /// Change to targets's attack stat.
      /// </summary>
      public int TargetAtk { get; set; }

      /// <summary>
      /// Change to targets's Defense stat.
      /// </summary>
      public int TargetDef { get; set; }

      /// <summary>
      /// Chance to activate the move's buff.
      /// </summary>
      public double BuffChance { get; set; }

      /// <summary>
      /// If the move is a legacy move.
      /// </summary>
      public bool IsLegacy { get; set; }

      /// <summary>
      /// List of Pokémon that can learn this move. 
      /// </summary>
      public int PokemonWithMove { get; set; }

      /// <summary>
      /// Gets the type of the Move as a string.
      /// </summary>
      /// <returns>Type as a string.</returns>
      public string TypeToString()
      {
         return Global.NONA_EMOJIS[$"{Type}_emote"];
      }

      /// <summary>
      /// Gets the weather that boosts the Move as a string.
      /// </summary>
      /// <returns>Weather boost string.</returns>
      public string WeatherToString()
      {
         StringBuilder sb = new StringBuilder();
         foreach (string weather in Weather)
         {
            sb.Append($"{Global.NONA_EMOJIS[$"{weather.Replace(' ', '_')}_emote"]} ");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Formats energy values as a string.
      /// </summary>
      /// <param name="energy">Value for energy.</param>
      /// <returns>Energy as a string.</returns>
      public string EnergyToString(int energy)
      {
         return $"{(Category.Equals(Global.FAST_MOVE_CATEGORY, StringComparison.OrdinalIgnoreCase) ? "+" : "-")}{energy}";
      }

      /// <summary>
      /// Formats the damage window as a string.
      /// </summary>
      /// <returns>Damage window as a string.</returns>
      public string DamageWindowString()
      {
         return $"{DamageWindowStart} ms - {DamageWindowEnd} ms ({DamageWindowEnd - DamageWindowStart} ms)";
      }

      /// <summary>
      /// Formats the buff as a string.
      /// </summary>
      /// <returns>Attack buff as a string.</returns>
      public string BuffString()
      {
         StringBuilder sb = new StringBuilder();
         sb.AppendLine($"{BuffChance * 100}% chance to activate the following:");

         if (AttackerAtk != 0)
         {
            string mod = AttackerAtk > 0 ? "Increase" : "Decrease";
            string s = Math.Abs(AttackerAtk) > 1 ? "stages" : "stage";
            sb.AppendLine($"{mod} Attacker Attack stat by {Math.Abs(AttackerAtk)} {s}.");
         }

         if (AttackerDef != 0)
         {
            string mod = AttackerDef > 0 ? "Increase" : "Decrease";
            string s = Math.Abs(AttackerAtk) > 1 ? "stages" : "stage";
            sb.AppendLine($"{mod} Attacker Defense stat by {Math.Abs(AttackerDef)} {s}.");
         }

         if (TargetAtk != 0)
         {
            string mod = TargetAtk > 0 ? "Increase" : "Decrease";
            string s = Math.Abs(AttackerAtk) > 1 ? "stages" : "stage";
            sb.AppendLine($"{mod} Target Attack stat by {Math.Abs(TargetAtk)} {s}.");
         }

         if (TargetDef != 0)
         {
            string mod = TargetDef > 0 ? "Increase" : "Decrease";
            string s = Math.Abs(AttackerAtk) > 1 ? "stages" : "stage";
            sb.AppendLine($"{mod} Target Defense stat by {Math.Abs(TargetDef)} {s}.");
         }

         return sb.ToString();
      }

      /// <summary>
      /// Gets the Pokémon move as a string.
      /// </summary>
      /// <returns>Pokémon move as a string.</returns>
      public string PokemonMoveToString()
      {
         string str = $@"{Name} {Global.NONA_EMOJIS[$"{Type}_emote"]}";
         if (IsLegacy)
         {
            str += $" {Global.LEGACY_MOVE_SYMBOL}";
         }
         return str;
      }
   }
}