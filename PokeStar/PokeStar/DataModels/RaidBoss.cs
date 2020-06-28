
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
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Stamina { get; set; }

        public List<string> Type { get; } = new List<string>();
        public List<string> Weather { get; } = new List<string>();
        public List<string> Weakness { get; } = new List<string>();
        public List<string> Resistances { get; } = new List<string>();
        public List<RaidCounter> Counters { get; } = new List<RaidCounter>();

        public int CPLow { get; set; }
        public int CPHigh { get; set; }
        public int CPLowBoosted { get; set; }
        public int CPHighBoosted { get; set; }

        public RaidCounter[] counters { get; }

        public RaidBoss(string boss)
        {
            

        }


    }
}
