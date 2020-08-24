using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.WebSocket;
using PokeStar.ConnectionInterface;

namespace PokeStar.DataModels
{
   public abstract class RaidParent
   {
      /// <summary>
      /// Maximum number of raid groups.
      /// </summary>
      protected int RaidGroupLimit { get; set; }

      /// <summary>
      /// Maximum number of players per group.
      /// </summary>
      protected int PlayerLimit { get; set; }

      /// <summary>
      /// Maximum number of invites per group.
      /// </summary>
      protected int InviteLimit { get; set; }

      /// <summary>
      /// Group number used for Request Invite list.
      /// </summary>
      protected readonly int InviteListNumber = 101;

      /// <summary>
      /// Value used when a user is not in the raid
      /// </summary>
      public static readonly int NotInRaid = -1;

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
      public RaidBoss Boss { get; protected set; }

      /// <summary>
      /// List of raid groups in the raid.
      /// </summary>
      public List<RaidGroup> Groups { get; protected set; }

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
      /// List of players looking for an invite to the raid.
      /// </summary>
      protected List<SocketGuildUser> Invite { get; set; }

      /// <summary>
      /// Player who is in the process of inviting someone.
      /// </summary>
      public SocketGuildUser InvitingPlayer { get; set; }

      /// <summary>
      /// Current page of Invite Embed.
      /// </summary>
      public int InvitePage { get; protected set; } = 0;

      public RaidParent(short tier, string time, string location, string boss = null)
      {
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
         Boss = (bossName == null) ? null : Connections.Instance().GetRaidBoss(bossName);
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

      public abstract bool PlayerAdd(SocketGuildUser player, int partySize, SocketGuildUser invitedBy = null);

      public abstract RemovePlayerReturn RemovePlayer(SocketGuildUser player);

      public abstract void RequestInvite(SocketGuildUser player);

      public abstract bool InvitePlayer(SocketGuildUser requester, SocketGuildUser accepter);

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
         foreach (var group in Groups)
            foreach (var check in Groups)
               group.MergeGroup(check);
         Groups.RemoveAll(x => x.TotalPlayers() == 0);
         if (Groups.Count == 0)
            Groups.Add(new RaidGroup(PlayerLimit, InviteLimit));
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