
using System.Collections.Generic;

namespace PokeStar.DataModels
{
   public class RaidBoss
   {
      public string Name { get; set; }
      public int Attack { get; set; }
      public int Defense { get; set; }
      public int Stamina { get; set; }

      public List<string> Type { get; } = new List<string>();
      public List<string> Weather { get; set; }
      public List<string> Weakness { get; set; }
      public List<string> Resistance { get; set; }
      public List<RaidCounter> Counter { get; set; }

      public int CPLow { get; set; }
      public int CPHigh { get; set; }
      public int CPLowBoosted { get; set; }
      public int CPHighBoosted { get; set; }

      public RaidCounter[] counters { get; }

      public RaidBoss(string boss)
      {
         //query

      }

      public RaidBoss() //for raid attendance testing
      {
         Name = "Tyranitar";
         Type = new List<string> { "Rock", "Dark"};
         Weakness = new List<string> { "weaknesses" };
         Resistance = new List<string> { "resistances" };
         CPLow = 1;
         CPHigh = 2;
         CPLowBoosted = 3;
         CPHighBoosted = 4;
         counters = null;
      }
   }
}
