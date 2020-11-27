using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.WebSocket;
using PokeStar.ConnectionInterface;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Parent class for raid types.
   /// </summary>
   public abstract class RaidParent
   {
      /// <summary>
      /// Maximum number of raid groups.
      /// </summary>
      protected int RaidGroupLimit { get; private set; }

      /// <summary>
      /// Maximum number of players per group.
      /// </summary>
      protected int PlayerLimit { get; private set; }

      /// <summary>
      /// Maximum number of invites per group.
      /// </summary>
      protected int InviteLimit { get; private set; }

      /// <summary>
      /// Group number used for request invite list.
      /// </summary>
      protected const int InviteListNumber = 101;

      /// <summary>
      /// When the raid starts.
      /// </summary>
      public string Time { get; set; }

      /// <summary>
      /// Where the raid is.
      /// </summary>
      public string Location { get; set; }

      /// <summary>
      /// Tier of the raid.
      /// </summary>
      public short Tier { get; set; }

      /// <summary>
      /// Raid boss that the raid is for.
      /// </summary>
      public Pokemon Boss { get; protected set; }

      /// <summary>
      /// List of possible raid bosses.
      /// Only used if no raid boss is selected.
      /// </summary>
      public List<string> RaidBossSelections { get; set; }

      /// <summary>
      /// When the raid was created at.
      /// </summary>
      public DateTime CreatedAt { get; protected set; }

      /// <summary>
      /// Player who is in the process of inviting someone.
      /// </summary>
      public SocketGuildUser InvitingPlayer { get; set; }

      /// <summary>
      /// Current page of Invite Embed.
      /// </summary>
      public int InvitePage { get; protected set; }

      /// <summary>
      /// List of raid groups in the raid.
      /// </summary>
      protected List<RaidGroup> Groups { get; set; }

      /// <summary>
      /// List of players looking for an invite to the raid.
      /// </summary>
      protected List<SocketGuildUser> Invite { get; set; }

      /// <summary>
      /// Creates a new RaidParent.
      /// </summary>
      /// <param name="groupLimit">Max number of groups.</param>
      /// <param name="playerLimit">Max number of players per group.</param>
      /// <param name="inviteLimit">Max number of invites per group.</param>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">When the raid starts.</param>
      /// <param name="location">Where the raid is.</param>
      /// <param name="boss">Name of the raid boss.</param>
      public RaidParent(int groupLimit, int playerLimit, int inviteLimit, short tier, string time, string location, string boss = null)
      {
         RaidGroupLimit = groupLimit;
         PlayerLimit = playerLimit;
         InviteLimit = inviteLimit;
         Tier = tier;
         Time = time;
         Location = location;
         SetBoss(boss);
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
      /// Sets the boss of the raid.
      /// </summary>
      /// <param name="bossName">Name of the raid boss.</param>
      public void SetBoss(string bossName)
      {
         Boss = string.IsNullOrEmpty(bossName) ? null : bossName.Equals(Global.DEFAULT_RAID_BOSS_NAME, StringComparison.OrdinalIgnoreCase) ? new Pokemon() :  Connections.Instance().GetPokemon(bossName);
      }

      /// <summary>
      /// Gets the number of raid groups.
      /// </summary>
      /// <returns>Current number of raid groups.</returns>
      public int GetTotalGroups()
      {
         return Groups.Count;
      }

      /// <summary>
      /// Gets a specific group by number.
      /// </summary>
      /// <param name="groupNumber">Group number to get</param>
      /// <returns>Group at the intended posision</returns>
      public RaidGroup GetGroup(int groupNumber)
      {
         return Groups.ElementAt(groupNumber);
      }

      /// <summary>
      /// Gets all users in all raid groups and who requested invites.
      /// </summary>
      /// <returns>A list of all users.</returns>
      public List<SocketGuildUser> GetAllUsers()
      {
         List<SocketGuildUser> allUsers = new List<SocketGuildUser>();
         allUsers.AddRange(Invite);
         foreach (RaidGroup group in Groups)
         {
            allUsers.AddRange(group.GetNotifyList());
         }
         return allUsers;
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
      /// Updates the invite page.
      /// </summary>
      /// <param name="isPositiveChage">If the change is positive, otherwise negative.</param>
      /// <param name="pageMaxOptions">Total options per page.</param>
      public void ChangeInvitePage(bool isPositiveChage, int pageMaxOptions)
      {
         if (isPositiveChage)
         {
            if ((InvitePage + 1) * pageMaxOptions < Invite.Count)
            {
               InvitePage++;
            }
         }
         else
         {
            if (InvitePage != 0)
            {
               InvitePage--;
            }
         }
      }

      /// <summary>
      /// Adds a player to a raid.
      /// The player will not be added if splitting the group brings the number of
      /// raid groups over the group limit.
      /// </summary>
      /// <param name="player">Player to add.</param>
      /// <param name="invitedBy">Who invited the user.</param>
      /// <returns>True if the user was added, otherwise false.</returns>
      public abstract bool AddPlayer(SocketGuildUser player, int partySize, SocketGuildUser invitedBy = null);

      /// <summary>
      /// Removes a player from the raid.
      /// </summary>
      /// <param name="player">Player to remove.</param>
      /// <returns>RaidRemove with raid group and list of invited users.</returns>
      public abstract RaidRemoveResult RemovePlayer(SocketGuildUser player);

      /// <summary>
      /// Requests an invite to a raid for a player.
      /// </summary>
      /// <param name="player">Player that requested the invite.</param>
      public abstract void RequestInvite(SocketGuildUser player);

      /// <summary>
      /// Accepts an invite of a player.
      /// </summary>
      /// <param name="requester">Player that requested the invite.</param>
      /// <param name="accepter">Player that accepted the invite.</param>
      /// <returns>True if the requester was invited, otherwise false.</returns>
      public abstract bool InvitePlayer(SocketGuildUser requester, SocketGuildUser accepter);

      /// <summary>
      /// Checks if a user is in the raid.
      /// </summary>
      /// <param name="player">Player to check.</param>
      /// <param name="checkInvite">Should invites be checked.</param>
      /// <returns>Raid Group number if the user is in the raid, otherwise -1.</returns>
      public abstract int IsInRaid(SocketGuildUser player, bool checkInvite = true);

      /// <summary>
      /// Finds the smallest group.
      /// </summary>
      /// <returns>Group number of the smallest group.</returns>
      protected int FindSmallestGroup()
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
      protected void CheckMergeGroups()
      {
         foreach (RaidGroup group in Groups)
         { 
            foreach (RaidGroup check in Groups)
            {
               group.MergeGroup(check);
            }
         }
         Groups.RemoveAll(x => x.TotalPlayers() == 0);
         if (Groups.Count == 0)
         {
            Groups.Add(new RaidGroup(PlayerLimit, InviteLimit));
         }
      }
   }
}