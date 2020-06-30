using System.Collections.Generic;

namespace PokeStar.DataModels
{
   public class RaidCounter
   {
      public string Name { get; set; }
      public List<string> Type { get; } = new List<string>();
      public Move FastAttack { get; set; }
      public Move ChargedAttack { get; set; }

      public string ToString()
      {
         string str = $@"{Name}: {FastAttack}";
         if (Type.Contains(FastAttack.Type))
            str += " *";
         str += $@"\{ChargedAttack}";
         if (Type.Contains(FastAttack.Type))
            str += " *";
         return str;
      }

   }
}
