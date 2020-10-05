using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeStar.DataModels
{
   public class Move
   {
      public string Name { get; set; }

      public string Type { get; set; }

      public List<string> Weather { get; set; }

      public string Category { get; set; }

      public int PvEPower { get; set; }

      public int PvEEnergy { get; set; }

      public int PvPPower { get; set; }

      public int PvPEnergy { get; set; }

      public int PvPTurns { get; set; }

      public int Cooldown { get; set; }

      public int DamageWindowStart { get; set; }

      public int DamageWindowEnd { get; set; }

      public List<PokemonMove> PokemonWithMove { get; set; }

      /// <summary>
      /// Gets the type of the Move as a string.
      /// </summary>
      /// <returns>Move type string.</returns>
      public string TypeToString()
      {
         return Global.NONA_EMOJIS[$"{Type}_emote"];
      }

      /// <summary>
      /// Gets the weather that boosts the Move as a string.
      /// </summary>
      /// <returns>Move weather string.</returns>
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
      /// Formats energy values to a string.
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
   }
}