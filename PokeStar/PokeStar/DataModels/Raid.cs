using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeStar.DataModels
{
    class Raid
    {
        public Raid(string time, string location)
        {
            Time = time;
            Gym = location;
        }

        public string Gym { get; set; }
        public string Time { get; set; }
        private short Tier
        {
            get { return Tier; }
            set { if (value <= 5) { Tier = value; }; }
        }
        private RaidBoss Boss { get; set; }

        public Dictionary<Player, int> Attending;
        public Dictionary<Player, int> Here;

        public void AddNewPlayer(Player player, int partySize)
        {
            Attending.Add(player, partySize);
        }
        
        public void SetPlayerArrived(Player player)
        {
            Here.Add(player, Attending[player]);
            Attending.Remove(player);
        }
    }
}
