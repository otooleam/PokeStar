using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeStar.DataModels
{
   public class Move
   {
      public string Name { get; set; }

      public string Type { get; set; }

      public string Category { get; set; }

      public string PvEPower { get; set; }

      public string PvEEnergy { get; set; }

      public string PvPPower { get; set; }

      public string PvPEnergy { get; set; }

      public string PvPTurns { get; set; }

      public string Cooldown { get; set; }

      public string DamageWindowStart { get; set; }

      public string DamageWindowEnd { get; set; }

      public List<string> PokemonWithMove { get; set; }
   }
}
