using System;

namespace PokeStar.Calculators
{
   /// <summary>
   /// Calculates type multipliers.
   /// </summary>
   public static class TypeCalculator
   {
      private static readonly double TypeCoefficient = 1.6;

      /// <summary>
      /// Calculate the type effectivness from the multipler.
      /// </summary>
      /// <param name="multiplier">Effectivness multiplier.</param>
      /// <returns>Effectivness of the type.</returns>
      public static double CalcTypeEffectivness(int multiplier)
      {
         double effectivness = TypeCoefficient;
         for (int i = 1; i < Math.Abs(multiplier); i++)
            effectivness *= TypeCoefficient;
         if (multiplier < 0)
            effectivness = 1.0 / effectivness;
         return effectivness;
      }
   }
}