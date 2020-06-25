using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeStar.DataModels
{
    public class RaidBoss
    {
        public string Name { get; set; }
        public string[] Type { get; set; }
        public List<string> Weakness { get; }
        public List<string> Resistances { get; }

        public int CPLow { get; set; }
        public int CPHigh { get; set; }
        public int CPLowBoosted { get; set; }
        public int CPHighBoosted { get; set; }

        public RaidCounter[] counters { get; }



    }
}
