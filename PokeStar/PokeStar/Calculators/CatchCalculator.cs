using System;

namespace PokeStar.Calculators
{
   /// <summary>
   /// Calculates catch percentage.
   /// </summary>
   public static class CatchCalculator
   {
      /// <summary>
      /// Multiplier for curveball throws.
      /// </summary>
      private const double CurveballMultiplier = 1.7;

      /// <summary>
      /// Calculates the multiplier for multiple medal bonuses.
      /// </summary>
      /// <param name="medal1">First medal bonus.</param>
      /// <param name="medal2">Second medal bonus.</param>
      /// <returns></returns>
      public static double CalculateMultiMedalMultiplier(double medal1, double medal2)
      {
         return (medal1 + medal2) / 2;
      }

      /// <summary>
      /// Calculates the catch chance of a Pokémon.
      /// </summary>
      /// <param name="baseCatchRate">Base catch rate of the Pokémon.</param>
      /// <param name="level">Level of the Pokémon.</param>
      /// <param name="ball">Ball multiplier.</param>
      /// <param name="berry">Berry multiplier.</param>
      /// <param name="radius">Radius of the catch circle.</param>
      /// <param name="curveball">Curveball multiplier..</param>
      /// <param name="medal">Pre-calculated medal multiplier.</param>
      /// <param name="encounter">Encounter multiplier.</param>
      /// <returns>Chance to catch the Pokémon as a decimal from 0 - 1.</returns>
      public static double CalcCatchChance(double baseCatchRate, int level, double ball, double berry,
                                           double radius, double curveball, double medal, double encounter)
      {
         double multiplier =  ball * berry * radius * curveball * encounter * medal;
         double cpm = Global.DISCRETE_CPM[level - 1];

         return 1.0 - Math.Pow(1.0 - Math.Min(1.0, baseCatchRate / (2.0 * cpm)), multiplier);
      }
   }
}