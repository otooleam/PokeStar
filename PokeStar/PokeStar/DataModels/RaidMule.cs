using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.WebSocket;
using PokeStar.ConnectionInterface;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Raid to fight against a raid boss.
   /// </summary>
   class RaidMule
   {
      /// <summary>
      /// Maximum number of raid groups.
      /// </summary>
      private readonly int RaidGroupLimit = 6;

      /// <summary>
      /// Maximum number of players for mule group.
      /// </summary>
      private readonly int MulePlayerLimit = 1;

      /// <summary>
      /// Maximum number of players per group.
      /// </summary>
      private readonly int PlayerLimit = 5;

      /// <summary>
      /// Maximum number of invites per group.
      /// </summary>
      private readonly int InviteLimit = 5;

      /// <summary>
      /// Group number used for Mule group.
      /// </summary>
      private readonly int MuleGroupNumber = 100;

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
      /// Raid group for raid mules.
      /// </summary>
      public RaidGroup Mules { get; private set; }

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

      public int InvitePage { get; private set; } = 0;

      /// <summary>
      /// Creates a new raid.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">When the raid starts.</param>
      /// <param name="location">Where the raid is.</param>
      /// <param name="boss">Name of the raid boss.</param>
      public RaidMule(short tier, string time, string location, string boss = null)
      {
         Tier = tier;
         Time = time;
         Location = location;
         SetBoss(boss);
         Mules = new RaidGroup(MulePlayerLimit, 0);
         Groups = new List<RaidGroup>
         {
            new RaidGroup(PlayerLimit, InviteLimit)
         };
         Invite = new List<SocketGuildUser>();
         RaidBossSelections = new List<string>();
         CreatedAt = DateTime.Now;
         InvitingPlayer = null;
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
      /// <param name="invitedBy">Who invited the user.</param>
      /// <returns>True if the user was added, otherwise false.</returns>
      public bool PlayerAdd(SocketGuildUser player, SocketGuildUser invitedBy = null)
      {
         if (invitedBy == null)
         {
            if (!Mules.HasPlayer(player, false) && Mules.GetAttendingCount() < MulePlayerLimit)
            {
               Mules.Add(player, 1);
               return true;
            }
         }
         else // is invite
         {
            int group = FindSmallestGroup();
            Groups.ElementAt(group).Invite(player, invitedBy);
            Invite.Remove(player);

            bool shouldSplit = Groups.ElementAt(group).ShouldSplit();

            if (shouldSplit && Groups.Count < RaidGroupLimit)
            {
               RaidGroup newGroup = Groups.ElementAt(group).SplitGroup();
               Groups.Add(newGroup);
               CheckMergeGroups();
               return true;
            }
            else if (!shouldSplit)
               return true;

            Groups.ElementAt(group).Remove(player);
            Invite.Add(player);
         }
         return false;
      }

      /// <summary>
      /// Removes a player from the raid.
      /// </summary>
      /// <param name="player">Player to remove.</param>
      /// <returns>Struct with raid group and list of invited users.</returns>
      public List<SocketGuildUser> RemovePlayer(SocketGuildUser player)
      {
         if (Invite.Contains(player))
            Invite.Remove(player);
         else
         {
            int groupNum = IsInRaid(player);
            if(groupNum != -1)
            {
               if (groupNum == MuleGroupNumber)
               {
                  foreach (RaidGroup group in Groups)
                  {
                     List<SocketGuildUser> invited = new List<SocketGuildUser>();
                     invited.AddRange(group.Remove(player));
                     return invited;
                  }
               }
               else
               {
                  RaidGroup foundGroup = Groups.ElementAt(groupNum);
                  foundGroup.Remove(player);
               }
            }
         }
         return new List<SocketGuildUser>();
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
         if (Invite.Contains(requester) && Mules.HasPlayer(accepter, false))
            return PlayerAdd(requester, accepter);
         return false;
      }

      /// <summary>
      /// Sets the boss of the raid.
      /// </summary>
      /// <param name="bossName">Name of the raid boss.</param>
      public void SetBoss(string bossName)
      {
         Boss = (bossName == null) ? new RaidBoss() : Connections.Instance().GetRaidBoss(bossName);
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
         if (Mules.HasPlayer(player, false))
            return MuleGroupNumber;
         for (int i = 0; i < Groups.Count; i++)
            if (Groups.ElementAt(i).HasPlayer(player, checkInvite))
               return i;
         return -1;
      }

      /// <summary>
      /// Updates the invite page.
      /// </summary>
      /// <param name="isPositiveChage">If the change is positive.</param>
      /// <param name="pageMaxOptions">Total options per page</param>
      public void ChangeInvitePage(bool isPositiveChage, int pageMaxOptions)
      {
         if (isPositiveChage)
         {
            if ((InvitePage + 1) * pageMaxOptions < Invite.Count)
               InvitePage++;
         }
         else
         {
            if (InvitePage != 0)
               InvitePage--;
         }
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
            Groups.Add(new RaidGroup(PlayerLimit, InviteLimit));
      }
   }
}