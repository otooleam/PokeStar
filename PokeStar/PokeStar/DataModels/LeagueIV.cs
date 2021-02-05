using System.Text;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Best IV for a GBL league.
   /// </summary>
   public class LeagueIV
   {
      /// <summary>
      /// Best attack IV.
      /// </summary>
      public int Attack { get; set; }

      /// <summary>
      /// Best defense IV.
      /// </summary>
      public int Defense { get; set; }

      /// <summary>
      /// Best stamina IV.
      /// </summary>
      public int Stamina { get; set; }

      /// <summary>
      /// Best Level.
      /// </summary>
      public double Level { get; set; }

      /// <summary>
      /// CP of the Pokémon given the IVs.
      /// </summary>
      public int CP { get; set; }

      /// <summary>
      /// Gets the Best league IVs as a string.
      /// </summary>
      /// <returns>League IV as a string.</returns>
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.AppendLine($"**IVs:** {Attack} / {Defense} / {Stamina}");
         sb.AppendLine($"**Level:** {Level}");
         sb.AppendLine($"**CP:** {CP}");
         return sb.ToString();
      }
   }
}