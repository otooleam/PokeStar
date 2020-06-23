using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeStar.DataModels
{
    public class RaidBoss
    {
        private string Name { get; set; }
        private string[] Type { get; set; }
        private List<string> Weakness { get; }
        private List<string> Resistances { get; }

        private int CPLow { get; set; }
        private int CPHigh { get; set; }
        private int CPLowBoosted { get; set; }
        private int CPHighBoosted { get; set; }

        private RaidCounter[] counters { get; }


    }
}
