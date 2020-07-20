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

      public RaidBoss() //This is for when silph is mid-update
      {
         Name = "Bossless";
      }
   }
}
