using System.Collections.Generic;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Node used for building an evolution tree.
   /// </summary>
   public class EvolutionNode
   {
      /// <summary>
      /// Name of Pokémon.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// Method to get this evolution.
      /// </summary>
      public string Method { get; set; }

      /// <summary>
      /// List of Pokémon evolution nodes that evolve from this Pokémon.
      /// The method of evolution is stored in these nodes.
      /// </summary>
      public List<EvolutionNode> Evolutions { get; private set; } = new List<EvolutionNode>();
   }
}