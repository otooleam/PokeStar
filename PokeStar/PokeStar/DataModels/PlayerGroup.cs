using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeStar.DataModels
{
   class PlayerGroup
   {
      public const int PARTY_SIZE_LIMIT = 7; //TODO set this back to 20

      public int AttendingCount { get; set; }
      public int ReadyCount { get; set; }
      public List<SocketGuildUser> Players { get; private set; }
      public Dictionary<SocketGuildUser, int> Attending { get; private set; }
      public Dictionary<SocketGuildUser, int> Ready { get; private set; }

      public PlayerGroup()
      {
         AttendingCount = 0;
         ReadyCount = 0;
         Players = new List<SocketGuildUser>();
         Attending = new Dictionary<SocketGuildUser, int>();
         Ready = new Dictionary<SocketGuildUser, int>();
      }

      //returns false if party needs to be split
      public bool PlayerAdd(SocketGuildUser player, int partySize)
      {
         if (Players.Contains(player)) //update existing player
         {
            if (Attending.ContainsKey(player))
            {
               int newPlayerCount = AttendingCount + partySize - Attending[player];

               if (newPlayerCount <= PARTY_SIZE_LIMIT)
               {
                  AttendingCount += partySize - Attending[player];
                  Attending[player] = partySize;
                  return true;
               }
               return false;
            }
            else //player is ready
            {
               int newPlayerCount = AttendingCount + partySize - Ready[player];

               if (newPlayerCount <= PARTY_SIZE_LIMIT)
               {
                  AttendingCount += partySize - Ready[player];
                  ReadyCount += partySize - Ready[player];
                  Ready[player] = partySize;
                  return true;
               }
               return false;
            }
         }
         else //new player
         {
            int newPlayerCount = AttendingCount + partySize;

            if (newPlayerCount <= PARTY_SIZE_LIMIT)
            {
               Players.Add(player);
               Attending.Add(player, partySize);
               AttendingCount = newPlayerCount;
               return true;
            }
            return false;
         }
      }

      //true if all players in group are ready
      public bool PlayerReady(SocketGuildUser player) //this is more robust than it needs to be
      {
         if (Attending.ContainsKey(player))
         {
            Ready.Add(player, Attending[player]);
            Attending.Remove(player);
            ReadyCount += Ready[player];

            if (ReadyCount == AttendingCount)
               return true;
         }
         return false;
      }

      public void RemovePlayer(SocketGuildUser player)
      {
         if (Players.Contains(player))
         {
            Players.Remove(player);
            if (Attending.ContainsKey(player))
            {
               AttendingCount -= Attending[player];
               Attending.Remove(player);
            }
            else if (Ready.ContainsKey(player))
            {
               AttendingCount -= Ready[player];
               ReadyCount -= Ready[player];
               Ready.Remove(player);
            }
         }
      }
   }
}
