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
      /// Group number used for Request Invite list.
      /// </summary>
      protected readonly int InviteListNumber = 101;

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

      /// <summary>
      /// 
      /// </summary>
      /// <param name="tier"></param>
      /// <param name="time"></param>
      /// <param name="location"></param>
      /// <param name="boss"></param>
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
         Boss = (string.IsNullOrEmpty(bossName)) ? null : bossName.Equals(Global.DEFAULT_RAID_BOSS_NAME) ? new RaidBoss() :  Connections.Instance().GetRaidBoss(bossName);
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
      /// 
      /// </summary>
      /// <param name="player"></param>
      /// <param name="partySize"></param>
      /// <param name="invitedBy"></param>
      /// <returns></returns>
      public abstract bool PlayerAdd(SocketGuildUser player, int partySize, SocketGuildUser invitedBy = null);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="player"></param>
      /// <returns></returns>
      public abstract Tuple<int, List<SocketGuildUser>> RemovePlayer(SocketGuildUser player);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="player"></param>
      public abstract void RequestInvite(SocketGuildUser player);
      
      /// <summary>
      /// 
      /// </summary>
      /// <param name="requester"></param>
      /// <param name="accepter"></param>
      /// <returns></returns>
      public abstract bool InvitePlayer(SocketGuildUser requester, SocketGuildUser accepter);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="player"></param>
      /// <param name="checkInvite"></param>
      /// <returns></returns>
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