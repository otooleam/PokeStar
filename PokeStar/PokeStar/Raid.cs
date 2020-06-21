using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeStar
{
    class Raid
    {
        private string _gym;
        private DateTime _hatchTime;
        private DateTime _despawnTime;
        private DateTime _jumpTime;

        //            player name, number attending
        private Dictionary<Player, int> _attending;
        private Dictionary<Player, int> _arrived;

        public string Gym { get => _gym; set => _gym = value; }
        public DateTime HatchTime { get => _hatchTime; set => _hatchTime = value; }
        public DateTime DespawnTime { get => _despawnTime; set => _despawnTime = value; }
        public DateTime JumpTime { get => _jumpTime; set => _jumpTime = value; }
    }
}
