using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeStar.DataModels
{
   class Raid
   {
      private const int playerLimit = 20;

      public Raid(short tier, string time, string location)
      {
         Tier = tier;
         Time = time;
         Location = location;
         PlayerCount = 0;
         HereCount = 0;
         Boss = SetBoss(tier);
         Attending = new Dictionary<SocketGuildUser, int>();
         Here = new Dictionary<SocketGuildUser, int>();
      }

      public string Location { get; set; }
      public string Time { get; set; }
      public short Tier { get; set; }
      public RaidBoss Boss { get; private set; }
      public int PlayerCount { get; private set; }
      public int HereCount { get; private set; }

      public Dictionary<SocketGuildUser, int> Attending;
      public Dictionary<SocketGuildUser, int> Here;

      public void PlayerAdd(SocketGuildUser player, int partySize)
      {
         if (Attending.ContainsKey(player))
         {
            if ((PlayerCount += (partySize - Attending[player])) <= playerLimit) //TODO doesnt enforce limit and numbers get confused
               Attending[player] = partySize;
         }
         else if (Here.ContainsKey(player))
         {
            if ((PlayerCount += (partySize - Here[player])) <= playerLimit)
            {
               HereCount += partySize - Here[player];
               Here[player] = partySize;
            }
         }
         else
         {
            if ((PlayerCount += partySize) <= 20)
               Attending.Add(player, partySize);
         }
      }

      public bool PlayerHere(SocketGuildUser player)
      {
         if (Attending.ContainsKey(player))
         {
            Here.Add(player, Attending[player]);
            Attending.Remove(player);
            HereCount += Here[player];

            if (Attending.Count == 0)
               return true;
            return false;
         }
         return false;
      }

      public void RemovePlayer(SocketGuildUser player)
      {
         if (Attending.ContainsKey(player))
         {
            PlayerCount -= Attending[player];
            Attending.Remove(player);
         }
         else if (Here.ContainsKey(player))
         {
            PlayerCount -= Here[player];
            HereCount -= Here[player];
            Here.Remove(player);
         }
      }

      private List<RaidBoss> GetBossData(short tier)
      {
         //query magic yay
         return new List<RaidBoss> { new RaidBoss() };
      }

      private RaidBoss SetBoss(short tier)
      {
         List<RaidBoss> potentials = GetBossData(tier);
         if (potentials.Count == 1)
            return potentials.First<RaidBoss>();
         return null; //TODO this needs more logic
      }
   }
}
