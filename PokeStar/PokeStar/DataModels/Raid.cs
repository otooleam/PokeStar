using System;
using System.Collections.Generic;
using Discord.WebSocket;
using PokeStar.ConnectionInterface;

namespace PokeStar.DataModels
{
   class Raid
   {
      private const int playerLimit = 20;
      private const int maxInvites = 10;
      public string Location { get; set; }
      public string Time { get; set; }
      public short Tier { get; set; }
      public RaidBoss Boss { get; private set; }
      public int PlayerCount { get; private set; }
      public int HereCount { get; private set; }
      public Dictionary<SocketGuildUser, int> Attending { get; private set; }
      public Dictionary<SocketGuildUser, int> Here { get; private set; }
      public Dictionary<SocketGuildUser, int> Invite { get; private set; }
      public DateTime CreatedAt { get; private set; }

      public Raid(short tier, string time, string location, string boss = null)
      {
         Tier = tier;
         Time = time;
         Location = location;
         PlayerCount = 0;
         HereCount = 0;
         SetBoss(boss);
         Attending = new Dictionary<SocketGuildUser, int>();
         Here = new Dictionary<SocketGuildUser, int>();
         Invite = new Dictionary<SocketGuildUser, int>();
         CreatedAt = DateTime.Now;
      }

      public void PlayerAdd(SocketGuildUser player, int partySize, bool isInvite = false)
      {
         if (Attending.ContainsKey(player))
         {
            int newPlayerCount = PlayerCount + (partySize - Attending[player]);
            if (newPlayerCount <= playerLimit)
            {
               Attending[player] = partySize;
               PlayerCount = newPlayerCount;
            }
         }
         else if (Here.ContainsKey(player))
         {
            int newPlayerCount = PlayerCount + (partySize - Here[player]);
            if (newPlayerCount <= playerLimit)
            {
               HereCount += partySize - Here[player];
               PlayerCount = newPlayerCount;
               Here[player] = partySize;
            }
         }
         else if (Invite.ContainsKey(player))
         {
            if (isInvite)
            {
               int newPlayerCount = PlayerCount + partySize;
               if (newPlayerCount <= 20)
               {
                  Attending.Add(player, partySize);
                  PlayerCount = newPlayerCount;
               }
            }
         }
         else
         {
            int newPlayerCount = PlayerCount + partySize;
            if (newPlayerCount <= 20)
            {
               Attending.Add(player, partySize);
               PlayerCount = newPlayerCount;
            }
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
      public void PlayerRequestInvite(SocketGuildUser player)
      {
         if (!Attending.ContainsKey(player) && !Here.ContainsKey(player) && !Invite.ContainsKey(player))
         {
            Invite.Add(player, 1);
         }
      }
      public bool InvitePlayer(SocketGuildUser player, SocketGuildUser user)
      {
         if ((Invite.Count + 1) < maxInvites && Invite.ContainsKey(player) && (Attending.ContainsKey(user) || Here.ContainsKey(user)))
         {
            PlayerAdd(player, 1, true);
            Invite.Remove(player);
            return true;
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
         else if (Invite.ContainsKey(player))
         {
            Invite.Remove(player);
         }
      }
      public void SetBoss(string bossName)
      {
         if (bossName != null)
         {
            if (bossName.Equals("noboss"))
            {
               Boss = new RaidBoss();
               return;
            }
            Boss = Connections.Instance().GetRaidBoss(bossName);
         }
      }

      public bool HasPlayer(SocketGuildUser user)
      {
         return Attending.ContainsKey(user) || Here.ContainsKey(user);
      }
   }
}
