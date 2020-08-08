using System.Collections.Generic;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Counter to a pokemon.
   /// </summary>
   public class Counter
   {
      /// <summary>
      /// Name of the pokemon.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// List of pokemon's type(s).
      /// </summary>
      public List<string> Type { get; } = new List<string>();

      /// <summary>
      /// Recommended Fast move.
      /// </summary>
      public Move FastAttack { get; set; }

      /// <summary>
      /// Recommended charge move.
      /// </summary>
      public Move ChargedAttack { get; set; }

      /// <summary>
      /// Gets the counter to a string.
      /// </summary>
      /// <returns>Counter string.</returns>
      public string ToString()
      {
         string str = $@"{Name}: {FastAttack}";
         if (Type.Contains(FastAttack.Type))
            str += " * ";
         str += $@"\{ChargedAttack}";
         if (Type.Contains(FastAttack.Type))
            str += " *";
         return str;
      }
   }
}