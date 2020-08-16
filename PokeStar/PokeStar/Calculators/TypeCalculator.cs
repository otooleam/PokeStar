using System;

namespace PokeStar.Calculators
{
   /// <summary>
   /// Calculates type multipliers.
   /// </summary>
   public static class TypeCalculator
   {
      /// <summary>
      /// Calculate the type effectivness from the multipler.
      /// </summary>
      /// <param name="multiplier">Effectivness multiplier.</param>
      /// <returns>Effectivness of the type.</returns>
      public static double CalcTypeEffectivness(int multiplier)
      {
         double effectivness = 1.6;
         for (int i = 1; i < Math.Abs(multiplier); i++)
         {
            effectivness *= 1.6;
         }
         if (multiplier < 0)
         {
            effectivness = 1.0 / effectivness;
         }
         return effectivness;
      }
   }
}
