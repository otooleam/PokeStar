using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.WebSocket;
using PokeStar.ConnectionInterface;
using System.Runtime.CompilerServices;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Raid to fight against a raid boss.
   /// </summary>
   class Raid
   {
      //if we have more groups, the embed breaks
      private const int GROUP_LIMIT = 3;

      /// <summary>
      /// When the raid starts.
      /// </summary>
      public string Time { get; set; }

      /// <summary>
      /// Where the raid is.
      /// </summary>
      public string Location { get; set; }

      /// <summary>
      /// Tier of the raid (1-5).
      /// </summary>
      public short Tier { get; set; }

      /// <summary>
      /// Raid boss that the raid is for.
      /// </summary>
      public RaidBoss Boss { get; private set; }

      /// <summary>
      /// List of raid groups in the raid.
      /// </summary>
      public List<RaidGroup> Groups { get; private set; }

      /// <summary>
      /// List of possible raid bosses.
      /// Only used if no raid boss is selected.
      /// </summary>
      public List<string> RaidBossSelections { get; set; }

      /// <summary>
      /// When the raid was created at.
      /// </summary>
      public DateTime CreatedAt { get; private set; }

      /// <summary>
      /// List of players looking for an invite to the raid.
      /// </summary>
      private List<SocketGuildUser> Invite { get; set; }

      /// <summary>
      /// Player who is in the process of inviting someone.
      /// </summary>
      public SocketGuildUser InvitingPlayer { get; set; }

      /// <summary>
      /// Creates a new raid.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">When the raid starts.</param>
      /// <param name="location">Where the raid is.</param>
      /// <param name="boss">Name of the raid boss.</param>
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
         InvitingPlayer = null;
      }

      /// <summary>
      /// Gets all users that want an invite to the raid.
      /// </summary>
      /// <returns>List of users that want an invite.</returns>
      public ImmutableList<SocketGuildUser> GetReadonlyInvite()
      {
         return Invite.ToImmutableList();
      }

      /// <summary>
      /// Gets all users that want an invite to the raid.
      /// </summary>
      /// <returns>List of users.</returns>
      public ImmutableList<SocketGuildUser> GetReadonlyInviteList()
      {
         return Invite.ToImmutableList();
      }

      /// <summary>
      /// Checks if there is an active invite message.
      /// </summary>
      /// <returns>True if there is an active message, otherwise false.</returns>
      public bool HasActiveInvite()
      {
         return InvitingPlayer != null;
      }

      /// <summary>
      /// Adds a player to a raid.
      /// The player will not be added if splitting the group brings the number of
      /// raid groups over the group limit.
      /// </summary>
      /// <param name="player">Player to add.</param>
      /// <param name="partySize">Number of accounts the user is bringing.</param>
      /// <param name="invitedBy">Who invited the user.</param>
      /// <returns>True if the user was added, otherwise false.</returns>
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
         else // is remote
         {
            group = IsInRaid(invitedBy);
            if (group != -1)
            {
               Groups.ElementAt(group).Invite(player, invitedBy);
               Invite.Remove(player);
            }
            else if (player.Equals(invitedBy))
            {
               group = FindSmallestGroup();
               Groups.ElementAt(group).Invite(player, invitedBy);
            }
            else
               return false;
         }

         RaidGroup newGroup;
         if (Groups.Count < GROUP_LIMIT)
         {
            newGroup = Groups.ElementAt(group).SplitGroup();
            if (newGroup != null)
               Groups.Add(newGroup);
            CheckMergeGroups();
            return true;
         }
         Groups.ElementAt(group).Remove(player);
         return false;
      }

      /// <summary>
      /// Removes a player from the raid.
      /// </summary>
      /// <param name="player">Player to remove.</param>
      /// <returns>Struct with raid group and list of invited users.</returns>
      public RemovePlayerReturn RemovePlayer(SocketGuildUser player)
      {
         var returnValue = new RemovePlayerReturn
         {
            GroupNum = -1,
            invited = new List<SocketGuildUser>()
         };
         if (Invite.Contains(player))
         {
            Invite.Remove(player);
            return returnValue;
         }
         else
         {
            for (int i = 0; i < Groups.Count; i++)
            {
               RaidGroup group = Groups.ElementAt(i);
               if (group.HasPlayer(player))
               {
                  returnValue.invited = group.Remove(player);
                  foreach (var invite in returnValue.invited)
                     Invite.Add(invite);

                  if (group.AllPlayersReady())
                     returnValue.GroupNum =  i;
                  return returnValue;
               }
            }
         }
         return returnValue;
      }

      /// <summary>
      /// Marks the player as ready.
      /// </summary>
      /// <param name="player">Player to mark ready.</param>
      /// <returns>Group number if all members of the group are ready, otherwise -1.</returns>
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

      /// <summary>
      /// Requests an invite to a raid for a player.
      /// </summary>
      /// <param name="player">Player that requested the invite.</param>
      public void PlayerRequestInvite(SocketGuildUser player)
      {
         if (!Invite.Contains(player) && IsInRaid(player) == -1)
            Invite.Add(player);
      }

      /// <summary>
      /// Accepts an invite of a player.
      /// </summary>
      /// <param name="requester">Player that requested the invite.</param>
      /// <param name="accepter">Player that accepted the invite.</param>
      /// <returns>True if the requester was invited, otherwise false.</returns>
      public bool InvitePlayer(SocketGuildUser requester, SocketGuildUser accepter)
      {
         if (Invite.Contains(requester))
            return PlayerAdd(requester, 1, accepter);
         return false;
      }

      /// <summary>
      /// Sets the boss of the raid.
      /// </summary>
      /// <param name="bossName">Name of the raid boss.</param>
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

      /// <summary>
      /// Checks if a player is in the raid.
      /// This does not check the raid request invite list.
      /// </summary>
      /// <param name="player">Player to check.</param>
      /// <param name="checkInvite">If invited players should be checked.</param>
      /// <returns>Group number the player is in, else -1.</returns>
      public int IsInRaid(SocketGuildUser player, bool checkInvite = true)
      {
         for (int i = 0; i < Groups.Count; i++)
            if (Groups.ElementAt(i).HasPlayer(player, checkInvite))
               return i;
         return -1;
      }

      /// <summary>
      /// Finds the smallest group.
      /// </summary>
      /// <returns>Group number of the smallest group.</returns>
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

      /// <summary>
      /// Attempts to merge groups.
      /// </summary>
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

   /// <summary>
   /// Return values for remove player method.
   /// </summary>
   public struct RemovePlayerReturn
   {
      public int GroupNum;
      public List<SocketGuildUser> invited;
   }
}