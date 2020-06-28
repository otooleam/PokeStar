using System.Collections.Generic;

namespace PokeStar.DataModels
{
   public class Pokemon
   {
      public int Number { get; set; }
      public string Name { get; set; }
      public int Attack { get; set; }
      public int Defense { get; set; }
      public int Stamina { get; set; }
      public int MaxCP { get; set; }
      public bool Shiny { get; set; }
      public bool Shadow { get; set; }
      public bool Obtainable { get; set; }
      public string Region { get; set; }

      public List<string> Types { get; } = new List<string>();
      public List<string> Weather { get; } = new List<string>();
      public List<string> Weaknesses { get; } = new List<string>();
      public List<string> Resistances { get; } = new List<string>();
      public List<RaidCounter> Counters { get; } = new List<RaidCounter>();
   }
}
