using System.Collections.Generic;

namespace PokeStar.DataModels
{
   public class EvolutionNode
   {
      public string Name { get; set; }
      public string Method { get; set; }

      public readonly List<EvolutionNode> Evolutions = new List<EvolutionNode>();
   }
}
