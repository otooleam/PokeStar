using System;
using System.Collections.Generic;
using Discord.WebSocket;
using PokeStar.ConnectionInterface;

namespace PokeStar.DataModels
{
   class Raid
   {
      private const int GROUP_LIMIT = 5;

      public string Location { get; set; }
      public string Time { get; set; }
      public short Tier { get; set; }
      public RaidBoss Boss { get; private set; }
      public List<PlayerGroup> PlayerGroups { get; private set; } //TODO change array based code
      public List<SocketGuildUser> InviteReqs { get; private set; } 
      public DateTime CreatedAt { get; private set; }

      public Raid(short tier, string time, string location, string boss = null)
      {
         Tier = tier;
         Time = time;
         Location = location;
         SetBoss(boss);
         CreatedAt = DateTime.Now;
         InviteReqs = new List<SocketGuildUser>();
         PlayerGroups = new List<PlayerGroup>() { new PlayerGroup() };
      }

      public void PlayerAdd(SocketGuildUser player, int partySize)
      {
         if (PlayerGroups.Count == 1)
         {
            if (!PlayerGroups[0].PlayerAdd(player, partySize))
            {
               //split the party

            }
         }
         else
         {
            List<int> playerCount = new List<int>();
            foreach (PlayerGroup group in PlayerGroups)
            {
               if (group.Players.Contains(player))
                  group.PlayerAdd(player, partySize); //update player
               else
                  playerCount.Add(group.Players.Count);
            }
         }
      }

      private int FindMinIndex(List<int> list)
      {
         int min = 100;
         int mindex = -1;
         for (int i = 0; i < list.Count; i++)
         {
            if (list[i] < min)
            {
               min = list[i];
               mindex = i;
            }
         }
         return mindex;
      }

      public bool PlayerReady(SocketGuildUser player) //returns true if party all here
      {
         for (int i = 0; i < GROUP_LIMIT; i++)
            if (PlayerGroups[i].Attending.ContainsKey(player))
               return PlayerGroups[i].PlayerReady(player);
         return false;
      }

      public void PlayerRequestInvite(SocketGuildUser player)
      {
         if (!InviteReqs.Contains(player))
         {
            foreach (PlayerGroup group in PlayerGroups)
               if (group.Players.Contains(player))
                  return;
            InviteReqs.Add(player);
         }
      }

      public bool InvitePlayer(SocketGuildUser invitee, SocketGuildUser invitingPlayer)
      {
         if (InviteReqs.Contains(invitee))
         {
            foreach (PlayerGroup group in PlayerGroups)
               if (group.Players.Contains(invitingPlayer))
                  return group.PlayerAdd(invitingPlayer, -1);
         }
         return false;
      }

      public void RemovePlayer(SocketGuildUser player)
      {
         if (InviteReqs.Contains(player))
         {
            InviteReqs.Remove(player);
         }
         else
         {
            foreach (PlayerGroup group in PlayerGroups)
               if (group.Players.Contains(player))
                  group.RemovePlayer(player);
         }
      }

      public string BuildPingList(SocketGuildUser player)
      {
         foreach (PlayerGroup group in PlayerGroups)
         {
            if (group.Players.Contains(player))
               return group.BuildPingList();
         }
         return "Everyone is Here.";
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
   }
}
