using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
      /// List of Pokémon that can learn this move. 
      /// </summary>
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