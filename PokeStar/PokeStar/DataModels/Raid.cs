using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using PokeStar.ConnectionInterface;

namespace PokeStar.DataModels
{
   class Raid
   {
      private const int maxInvites = 10;

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
            if (group.ContainsPlayer(player)) //updates player party size 
            {
               if (!group.AddPlayer(player, partySize))
               {
                  SplitPlayerGroup(group, player, partySize);
               }
               else
               {
                  CombinePlayerGroups(group);  //it didnt need to split so check for combinability
               }
               return;
            }
         }
         int smallestGroupIndex = FindMinIndex(PlayerGroups);
         if (InviteReqs.Contains(player))
            InviteReqs.Remove(player);
         if (!PlayerGroups[smallestGroupIndex].AddPlayer(player, partySize)) //adds new player to smallest group
         {
            SplitPlayerGroup(PlayerGroups[smallestGroupIndex], player, partySize);
         }
         else
         {
            CombinePlayerGroups(PlayerGroups[smallestGroupIndex]);
         }
      }

      private void SplitPlayerGroup(PlayerGroup originalGroup, SocketGuildUser player, int partySize)
      {
         PlayerGroup newGroup = new PlayerGroup();
         Dictionary<SocketGuildUser, int> attendingPlayerBuffer = new Dictionary<SocketGuildUser, int>();
         Dictionary<SocketGuildUser, int> herePlayerBuffer = new Dictionary<SocketGuildUser, int>();

         foreach (KeyValuePair<SocketGuildUser, int> attendingPlayer in originalGroup.Attending)
         {
            attendingPlayerBuffer.Add(attendingPlayer.Key, attendingPlayer.Value);
         }
         foreach (SocketGuildUser attendingPlayer in attendingPlayerBuffer.Keys)
         {
            originalGroup.RemovePlayer(attendingPlayer);
         }
         foreach (KeyValuePair<SocketGuildUser, int> herePlayer in originalGroup.Here)
         {
            herePlayerBuffer.Add(herePlayer.Key, herePlayer.Value);
         }
         foreach (SocketGuildUser herePlayer in herePlayerBuffer.Keys)
         {
            originalGroup.RemovePlayer(herePlayer);
         }

         //update or add the triggering player
         if (attendingPlayerBuffer.ContainsKey(player))
            attendingPlayerBuffer[player] = partySize;
         else if (herePlayerBuffer.ContainsKey(player))
            herePlayerBuffer[player] = partySize;
         else
            attendingPlayerBuffer.Add(player, partySize);

         foreach (KeyValuePair<SocketGuildUser, int> herePlayer in herePlayerBuffer)
         {
            if (originalGroup.AttendingCount <= newGroup.AttendingCount)
            {
               originalGroup.AddPlayer(herePlayer.Key, herePlayer.Value);
               originalGroup.MarkPlayerReady(herePlayer.Key);
            }
            else
            {
               newGroup.AddPlayer(herePlayer.Key, herePlayer.Value);
               newGroup.MarkPlayerReady(herePlayer.Key);
            }
         }
         foreach (KeyValuePair<SocketGuildUser, int> attendingPlayer in attendingPlayerBuffer)
         {
            if (originalGroup.AttendingCount <= newGroup.AttendingCount)
            {
               originalGroup.AddPlayer(attendingPlayer.Key, attendingPlayer.Value);
            }
            else
            {
               newGroup.AddPlayer(attendingPlayer.Key, attendingPlayer.Value);
            }
         }
         PlayerGroups.Add(newGroup);
      }


      private void CombinePlayerGroups(PlayerGroup group1)
      {
         if (PlayerGroups.Count > 1)
         {
            foreach (PlayerGroup group2 in PlayerGroups)
            {
               if (group1 != group2 && group1.AttendingCount + group2.AttendingCount <= PlayerGroup.PARTY_SIZE_LIMIT)
               {
                  PlayerGroup newGroup = new PlayerGroup();

                  foreach (KeyValuePair<SocketGuildUser, int> player in group1.Here)
                     newGroup.Here.Add(player.Key, player.Value);
                  foreach (KeyValuePair<SocketGuildUser, int> player in group2.Here)
                     newGroup.Here.Add(player.Key, player.Value);
                  newGroup.HereCount = group1.HereCount + group2.HereCount;

                  foreach (KeyValuePair<SocketGuildUser, int> player in group1.Attending)
                     newGroup.Attending.Add(player.Key, player.Value);
                  foreach (KeyValuePair<SocketGuildUser, int> player in group2.Attending)
                     newGroup.Attending.Add(player.Key, player.Value);
                  newGroup.AttendingCount = group1.AttendingCount + group2.AttendingCount;

                  PlayerGroups.Remove(group1);
                  PlayerGroups.Remove(group2);
                  PlayerGroups.Add(newGroup);
                  return;
               }
            }
         }

      }

      private int FindMinIndex(List<PlayerGroup> list)
      {
         int min = 100;
         int mindex = -1;
         for (int i = 0; i < list.Count; i++)
         {
            if (list[i].AttendingCount < min)
            {
               min = list[i].AttendingCount;
               mindex = i;
            }
         }
         return mindex;
      }

      public bool PlayerReady(SocketGuildUser player) //returns true if party all here
      {
         foreach (PlayerGroup group in PlayerGroups)
            if (group.ContainsPlayer(player))
               return group.MarkPlayerReady(player);
         return false;
      }

      public void PlayerRequestInvite(SocketGuildUser player)
      {
         if (!InviteReqs.Contains(player))
         {
            foreach (PlayerGroup group in PlayerGroups)
               if (group.ContainsPlayer(player))
                  return;
            InviteReqs.Add(player);
         }
      }

      public bool InvitePlayer(SocketGuildUser invitee, SocketGuildUser invitingPlayer)
      {
         foreach (PlayerGroup group in PlayerGroups)
            if ((InviteReqs.Count + 1) < maxInvites && group.ContainsPlayer(invitingPlayer))
               return group.AddPlayer(invitee, 1);
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
            foreach (PlayerGroup group in PlayerGroups) //crap (collection edited error)
            {
               if (group.ContainsPlayer(player))
               {
                  group.RemovePlayer(player);
                  CombinePlayerGroups(group);
               }
            }
         }
      }

      public string BuildPingList(SocketGuildUser player)
      {
         foreach (PlayerGroup group in PlayerGroups)
         {
            if (group.ContainsPlayer(player))
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

      public bool PlayerIsAttending(SocketGuildUser player)
      {
         foreach (PlayerGroup group in PlayerGroups)
            if (group.ContainsPlayer(player))
               return true;
         return false;
      }

      public string BuildPingList(int groupNum)
      {
         if (PlayerGroups.Count == 1)
         {
            StringBuilder sb = new StringBuilder();

            foreach (SocketGuildUser player in PlayerGroups[groupNum].Here.Keys)
               sb.Append(player.Mention + " ");
            sb.AppendLine($"Everyone is ready at {Location}.");

            return sb.ToString();
         }
         else
         {
            StringBuilder sb = new StringBuilder();

            foreach (SocketGuildUser player in PlayerGroups[groupNum].Here.Keys)
               sb.Append(player.Mention);
            sb.AppendLine($" Group {groupNum + 1} is ready at {Location}.");

            return sb.ToString();
         }
      }
   }
}
