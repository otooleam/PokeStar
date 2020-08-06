using Discord.WebSocket;
using System.Collections.Generic;

namespace PokeStar.DataModels
{
   class PlayerGroup
   {
      public const int PARTY_SIZE_LIMIT = 7; //TODO 20

      public int AttendingCount { get; set; }
      public int HereCount { get; set; }
      public Dictionary<SocketGuildUser, int> Attending { get; private set; }
      public Dictionary<SocketGuildUser, int> Here { get; private set; }

      public PlayerGroup()
      {
         AttendingCount = 0;
         HereCount = 0;
         Attending = new Dictionary<SocketGuildUser, int>();
         Here = new Dictionary<SocketGuildUser, int>();
      }

      public bool ContainsPlayer(SocketGuildUser player)
      {
         return Attending.ContainsKey(player) || Here.ContainsKey(player);
      }

      //returns false if party needs to be split
      public bool AddPlayer(SocketGuildUser player, int partySize)
      {
         if (ContainsPlayer(player)) //update existing player
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
            else //player is marked here
            {
               int newPlayerCount = AttendingCount + partySize - Here[player];

               if (newPlayerCount <= PARTY_SIZE_LIMIT)
               {
                  AttendingCount += partySize - Here[player];
                  HereCount += partySize - Here[player];
                  Here[player] = partySize;
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
               Attending.Add(player, partySize);
               AttendingCount = newPlayerCount;
               return true;
            }
            return false;
         }
      }

      //returns true if all players in group are ready
      public bool MarkPlayerReady(SocketGuildUser player)
      {
         if (Attending.ContainsKey(player))
         {
            Here.Add(player, Attending[player]);
            Attending.Remove(player);
            HereCount += Here[player];

            if (HereCount == AttendingCount)
               return true;
         }
         return false;
      }

      public void RemovePlayer(SocketGuildUser player)
      {
         if (Attending.ContainsKey(player))
         {
            AttendingCount -= Attending[player];
            Attending.Remove(player);
         }
         else if (Here.ContainsKey(player))
         {
            AttendingCount -= Here[player];
            HereCount -= Here[player];
            Here.Remove(player);
         }
      }
   }
}
