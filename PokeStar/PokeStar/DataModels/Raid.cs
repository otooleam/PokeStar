using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
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
      public List<RaidGroup> Groups { get; private set; }
      public List<string> RaidBossSelections { get; set; }
      public DateTime CreatedAt { get; private set; }
      private List<SocketGuildUser> Invite { get; set; }

      public Raid(short tier, string time, string location, string boss = null)
      {
         Tier = tier;
         Time = time;
         Location = location;
         SetBoss(boss);
         Groups = new List<RaidGroup>
         {
            new RaidGroup()
         };
         Invite = new List<SocketGuildUser>();
         RaidBossSelections = new List<string>();
         CreatedAt = DateTime.Now;
      }

      public ImmutableDictionary<SocketGuildUser, int> GetReadonlyInvite()
      {
         return Invite.ToImmutableDictionary(k => k, v => 1);
      }

      public bool PlayerAdd(SocketGuildUser player, int partySize, SocketGuildUser invitedBy = null)
      {
         int group;
         if (invitedBy == null)
         {
            group = IsInRaid(player);
            if (group == -1)
               group = FindSmallestGroup();
            Groups.ElementAt(group).Add(player, partySize);
         }
         else // is invite
         {
            group = IsInRaid(invitedBy);
            if (group != -1)
            {
               Groups.ElementAt(group).Invite(player, invitedBy);
               return false;
            }
         }
         var newGroup = Groups.ElementAt(group).SplitGroup();
         if (newGroup != null)
            Groups.Add(newGroup);
         CheckMergeGroups();
         return true;
      }

      public void RemovePlayer(SocketGuildUser player)
      {
         if (Invite.Contains(player))
            Invite.Remove(player);
         else
         {
            foreach (var group in Groups)
            {
               if (group.HasPlayer(player))
               {
                  group.Remove(player);
                  CheckMergeGroups();
                  return;
               }
            }
         }
      }

      public int PlayerReady(SocketGuildUser player)
      {
         for (int i = 0; i < Groups.Count; i++)
         {
            var group = Groups.ElementAt(i);
            if (group.HasPlayer(player))
            {
               group.PlayerReady(player);
               if (group.AllPlayersReady())
                  return i;
               return -1;
            }
         }
         return -1;
      }

      public void PlayerRequestInvite(SocketGuildUser player)
      {
         if (!Invite.Contains(player) && IsInRaid(player) == -1)
         {
            Invite.Add(player);
         }
      }

      public bool InvitePlayer(SocketGuildUser player, SocketGuildUser user)
      {
         if (Invite.Contains(player))
         {
            return PlayerAdd(player, 1, user);
         }
         return false;
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

      public int IsInRaid(SocketGuildUser player, bool checkInvite = true)
      {
         for (int i = 0; i < Groups.Count; i++)
            if (Groups.ElementAt(i).HasPlayer(player, checkInvite))
               return i;
         return -1;
      }

      private int FindSmallestGroup()
      {
         int minSize = int.MaxValue;
         int minGroup = 0;
         for (int i = 0; i < Groups.Count; i++)
         {
            int groupSize = Groups.ElementAt(i).TotalPlayers();
            if (groupSize < minSize)
            {
               minSize = groupSize;
               minGroup = i;
            }
         }
         return minGroup;
      }

      private void CheckMergeGroups()
      {
         foreach (var group in Groups)
            foreach (var check in Groups)
               group.MergeGroup(check);
         Groups.RemoveAll(x => x.TotalPlayers() == 0);
         if (Groups.Count == 0)
            Groups.Add(new RaidGroup());
      }
   }
}