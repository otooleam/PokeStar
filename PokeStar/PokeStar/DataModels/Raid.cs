using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeStar.DataModels
{
    class Raid
    {
        private string Gym { get; set; }
        private short Tier
        {
            get { return Tier; }
            set { if (value <= 5) { Tier = value; }; }
        }
        private RaidBoss Boss { get; set; }

        private Dictionary<Player, int> Attending;
        private Dictionary<Player, int> Arrived;

        public void AddNewPlayer(Player player, int partySize)
        {
            Attending.Add(player, partySize);
        }
        
        public void SetPlayerArrived(Player player)
        {
            Arrived.Add(player, Attending[player]);
            Attending.Remove(player);
        }
    }
}
