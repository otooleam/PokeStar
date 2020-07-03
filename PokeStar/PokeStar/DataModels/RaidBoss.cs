using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokeStar.ConnectionInterface;

namespace PokeStar.DataModels
{
   public class RaidBoss
   {
      public string Name { get; set; }
      public string[] Type { get; set; }
      public List<string> Weaknesses { get; }
      public List<string> Resistances { get; }

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
         Type = new string[] { "Rock", "Dark"};
         Weaknesses = new List<string> { "weaknesses" };
         Resistances = new List<string> { "resistances" };
         CPLow = 1;
         CPHigh = 2;
         CPLowBoosted = 3;
         CPHighBoosted = 4;
         counters = null;
      }
   }
}
