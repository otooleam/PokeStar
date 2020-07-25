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
      private const int PARTY_SIZE_LIMIT = 20;

      public int AttendingCount { get; private set; }
      public int ReadyCount { get; private set; }
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

      //false if party needs to be split
      public bool PlayerAdd(SocketGuildUser player, int partySize)
      {
         if (partySize == -1) //true if invite
         {
            if (Players.Contains(player))
            {
               int newPlayerCount = AttendingCount + 1;

               if (newPlayerCount <= PARTY_SIZE_LIMIT)
               {
                  if (Attending.ContainsKey(player))
                  {
                     Attending[player]++;
                  }
                  else
                  {
                     Ready[player]++;
                  }
                  AttendingCount = newPlayerCount;
                  return true;
               }
               return false;
            }
            else
            {
               int newPlayerCount = AttendingCount + 2;

               if (newPlayerCount <= PARTY_SIZE_LIMIT)
               {
                  Attending.Add(player, 2); //add the player and the person they invited
                  Players.Add(player);
                  AttendingCount = newPlayerCount;
                  return true;
               }
               return false;
            }
         }
         else
         {
            if (Players.Contains(player))
            {
               int newPlayerCount = AttendingCount + partySize;

               if (newPlayerCount <= PARTY_SIZE_LIMIT)
               {
                  if (Attending.ContainsKey(player))
                  {
                     AttendingCount += partySize - Attending[player];
                     Attending[player] = partySize;
                  }
                  else
                  {
                     AttendingCount += partySize - Ready[player];
                     ReadyCount += partySize - Ready[player];
                     Ready[player] = partySize;
                  }
                  return true;
               }
               return false;
            }
            else
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
      }
      
      //true if all players in group are ready
      public bool PlayerReady(SocketGuildUser player)
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

      public string BuildPingList()
      {
         StringBuilder sb = new StringBuilder();

         foreach (SocketGuildUser player in Players)
            sb.Append(player.Mention);
         sb.AppendLine(" Everyone is ready, time to jump!");

         return sb.ToString();
      }
   }
}
