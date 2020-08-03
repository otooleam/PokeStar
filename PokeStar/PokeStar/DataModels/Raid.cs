using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using PokeStar.ConnectionInterface;

namespace PokeStar.DataModels
{
   class Raid
   {
      public string Location { get; set; }
      public string Time { get; set; }
      public short Tier { get; set; }
      public RaidBoss Boss { get; private set; }
      public List<PlayerGroup> PlayerGroups { get; private set; }
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
         foreach (PlayerGroup group in PlayerGroups)
         {
            if (group.Players.Contains(player)) //updates player party size 
            {
               if (!group.PlayerAdd(player, partySize))
               {
                  SplitPlayerGroup(group, player, partySize);
               }
               return;
            }
         }
         int smallestGroupIndex = FindMinIndex(PlayerGroups); 
         if (!PlayerGroups[smallestGroupIndex].PlayerAdd(player, partySize)) //adds new player to smallest group
         {
            SplitPlayerGroup(PlayerGroups[smallestGroupIndex], player, partySize);
         }
      }

      private void SplitPlayerGroup(PlayerGroup originalGroup, SocketGuildUser player, int partySize)
      {
         PlayerGroup newGroup = new PlayerGroup();
         Dictionary<SocketGuildUser, int> nonReadyPlayers = new Dictionary<SocketGuildUser, int>();
         foreach (KeyValuePair<SocketGuildUser, int> attendingPlayer in originalGroup.Attending)
            nonReadyPlayers.Add(attendingPlayer.Key, attendingPlayer.Value);
         foreach (SocketGuildUser attendingPlayer in nonReadyPlayers.Keys)
         {
            originalGroup.RemovePlayer(attendingPlayer);
         }
         if (!nonReadyPlayers.ContainsKey(player))
            nonReadyPlayers.Add(player, partySize);
         else
            nonReadyPlayers[player] = partySize;

         foreach (KeyValuePair<SocketGuildUser, int> attendingPlayer in nonReadyPlayers)
         {
            if (originalGroup.Players.Count <= newGroup.Players.Count)
            {
               originalGroup.PlayerAdd(attendingPlayer.Key, attendingPlayer.Value);
            }
            else
            {
               newGroup.PlayerAdd(attendingPlayer.Key, attendingPlayer.Value);
            }
         }
         PlayerGroups.Add(newGroup);
      }

      private void CombinePlayerGroups(PlayerGroup group1, PlayerGroup group2)
      {
         PlayerGroup newGroup = new PlayerGroup();

         newGroup.Players.AddRange(group1.Players);
         newGroup.Players.AddRange(group2.Players);

         foreach (KeyValuePair<SocketGuildUser, int> player in group1.Ready)
            newGroup.Ready.Add(player.Key, player.Value);
         foreach (KeyValuePair<SocketGuildUser, int> player in group2.Ready)
            newGroup.Ready.Add(player.Key, player.Value);

         foreach (KeyValuePair<SocketGuildUser, int> player in group1.Attending)
            newGroup.Attending.Add(player.Key, player.Value);
         foreach (KeyValuePair<SocketGuildUser, int> player in group1.Attending)
            newGroup.Attending.Add(player.Key, player.Value);

         PlayerGroups.Remove(group1);
         PlayerGroups.Remove(group2);
         PlayerGroups.Add(newGroup);
      }

      private int FindMinIndex(List<PlayerGroup> list)
      {
         int min = 100;
         int mindex = -1;
         for (int i = 0; i < list.Count; i++)
         {
            if (list[i].Players.Count < min)
            {
               min = list[i].Players.Count;
               mindex = i;
            }
         }
         return mindex;
      }

      public bool PlayerReady(SocketGuildUser player) //returns true if party all here
      {
         foreach (PlayerGroup group in PlayerGroups)
            if (group.Attending.ContainsKey(player))
               return group.PlayerReady(player);
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
                  return group.PlayerAdd(invitingPlayer, -1); //-1 denotes an invite
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
               {
                  group.RemovePlayer(player);
                  
                  foreach (PlayerGroup group2 in PlayerGroups)
                  {
                     if (group.Players.Count + group2.Players.Count <= PlayerGroup.PARTY_SIZE_LIMIT)
                     {
                        CombinePlayerGroups(group, group2); 
                        return;
                     }
                  }
               }
         }
      }

      public string BuildPingList(SocketGuildUser player)
      {
         foreach (PlayerGroup group in PlayerGroups)
         {
            if (group.Players.Contains(player))
               return BuildPingList(PlayerGroups.IndexOf(group));
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

      public string BuildPingList(int groupNum)
      {
         if (PlayerGroups.Count == 1)
         {
            StringBuilder sb = new StringBuilder();

            foreach (SocketGuildUser player in PlayerGroups[0].Players)
               sb.Append(player.Mention);
            sb.AppendLine($" Everyone is ready at {Location}.");

            return sb.ToString();
         }
         else
         {
            StringBuilder sb = new StringBuilder();

            foreach (SocketGuildUser player in PlayerGroups[groupNum].Players)
               sb.Append(player.Mention);
            sb.AppendLine($" Group {groupNum + 1} is ready at {Location}.");

            return sb.ToString();
         }
      }
   }
}
