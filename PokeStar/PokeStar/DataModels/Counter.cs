using System;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Counter to a Pokémon.
   /// </summary>
   public class Counter
   {
      /// <summary>
      /// Name of the Pokémon.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// Recommended Fast move.
      /// </summary>
      public Move FastAttack { get; set; }

      /// <summary>
      /// Recommended charge move.
      /// </summary>
      public Move ChargeAttack { get; set; }

      /// <summary>
      /// Rating of the Pokémon.
      /// </summary>
      public double Rating { get; set; }

      /// <summary>
      /// Checks if two counters are the same.
      /// </summary>
      /// <param name="counter">Counter to check against.</param>
      /// <returns>True if the counters are the same, otherwise false.</returns>
      public bool Equals(Counter counter)
      {
         return counter != null && counter.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) &&
            counter.FastAttack.Name.Equals(FastAttack.Name, StringComparison.OrdinalIgnoreCase) &&
            counter.ChargeAttack.Name.Equals(ChargeAttack.Name, StringComparison.OrdinalIgnoreCase);
      }

      /// <summary>
      /// Checks if this is equal to an object.
      /// </summary>
      /// <param name="obj">Object to check against.</param>
      /// <returns>True if the object is equal, otherwise false.</returns>
      public override bool Equals(object obj)
      {
         return obj != null && obj is Counter && Equals(obj as Counter);
      }

      /// <summary>
      /// Gets the counter as a string.
      /// </summary>
      /// <returns>Counter as a string.</returns>
      public override string ToString()
      {
         return $@"**{Name}**: {FastAttack.PokemonMoveToString()} / {ChargeAttack.PokemonMoveToString()}";
      }
   }
}