using System;
using System.Linq;
using System.Collections.Generic;
using Discord.WebSocket;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Raid to fight against a raid boss.
   /// </summary>
   public class RaidMule : RaidParent
   {
      /// <summary>
      /// Group number used for Mule group.
      /// </summary>
      private readonly int MuleGroupNumber = 100;

      /// <summary>
      /// Raid group for raid mules.
      /// </summary>
      public RaidGroup Mules { get; private set; }

      /// <summary>
      /// Creates a new raid.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">When the raid starts.</param>
      /// <param name="location">Where the raid is.</param>
      /// <param name="boss">Name of the raid boss.</param>
      public RaidMule(short tier, string time, string location, string boss = null) : base(tier, time, location, boss)
      {
         RaidGroupLimit = Global.LIMIT_RAID_MULE_GROUP;
         PlayerLimit = Global.LIMIT_RAID_MULE_INVITE;
         InviteLimit = Global.LIMIT_RAID_MULE_INVITE;
         Mules = new RaidGroup(Global.LIMIT_RAID_MULE_MULE, 0);
      }

      /// <summary>
      /// Adds a player to a raid.
      /// The player will not be added if splitting the group brings the number of
      /// raid groups over the group limit.
      /// </summary>
      /// <param name="player">Player to add.</param>
      /// <param name="invitedBy">Who invited the user.</param>
      /// <returns>True if the user was added, otherwise false.</returns>
      public override bool PlayerAdd(SocketGuildUser player, int partySize, SocketGuildUser invitedBy = null)
      {
         if (invitedBy == null)
         {
            if (IsInRaid(player) == Global.NOT_IN_RAID && Mules.GetAttendingCount() < MulePlayerLimit)
            {
               Mules.Add(player, partySize, 0);
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
            {
               return true;
            }

            Groups.ElementAt(group).Remove(player);
            Invite.Add(player);
         }
         return false;
      }

      /// <summary>
      /// Removes a player from the raid.
      /// </summary>
      /// <param name="player">Player to remove.</param>
      /// <returns>Tuple with raid group and list of invited users.</returns>
      public override Tuple<int, List<SocketGuildUser>> RemovePlayer(SocketGuildUser player)
      {
         Tuple<int, List<SocketGuildUser>> returnValue = new Tuple<int, List<SocketGuildUser>>(Global.NOT_IN_RAID, new List<SocketGuildUser>());

         int groupNum = IsInRaid(player);
         if (groupNum == InviteListNumber)
         {
            Invite.Remove(player);
         }
         else if (groupNum == MuleGroupNumber)
         {
            Mules.Remove(player);
            foreach (RaidGroup group in Groups)
            {
               returnValue.Item2.AddRange(group.Remove(player));
            }
            foreach (SocketGuildUser invite in returnValue.Item2)
            {
               Invite.Add(invite);
            }
            return returnValue;
         }
         else if (groupNum != Global.NOT_IN_RAID)
         {
            RaidGroup foundGroup = Groups.ElementAt(groupNum);
            foundGroup.Remove(player);
         }
         return returnValue;
      }

      /// <summary>
      /// Requests an invite to a raid for a player.
      /// </summary>
      /// <param name="player">Player that requested the invite.</param>
      public override void RequestInvite(SocketGuildUser player)
      {
         if (IsInRaid(player) == Global.NOT_IN_RAID)
         {
            Invite.Add(player);
         }
      }

      /// <summary>
      /// Accepts an invite of a player.
      /// </summary>
      /// <param name="requester">Player that requested the invite.</param>
      /// <param name="accepter">Player that accepted the invite.</param>
      /// <returns>True if the requester was invited, otherwise false.</returns>
      public override bool InvitePlayer(SocketGuildUser requester, SocketGuildUser accepter)
      {
         if (Invite.Contains(requester) && Mules.HasPlayer(accepter, false))
         {
            return PlayerAdd(requester, 1, accepter);
         }
         return false;
      }

      /// <summary>
      /// Checks if a player is in the raid.
      /// This does not check the raid request invite list.
      /// </summary>
      /// <param name="player">Player to check.</param>
      /// <param name="checkInvite">If invited players should be checked.</param>
      /// <returns>Group number the player is in, else NotInRaid.</returns>
      public override int IsInRaid(SocketGuildUser player, bool checkInvite = true)
      {
         if (Mules.HasPlayer(player, false))
         {
            return MuleGroupNumber;
         }
         if (checkInvite && Invite.Contains(player))
         {
            return InviteListNumber;
         }
         for (int i = 0; i < Groups.Count; i++)
         {
            if (Groups.ElementAt(i).HasPlayer(player, checkInvite))
            {
               return i;
            }
         }
         return Global.NOT_IN_RAID;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public bool HasInvites()
      {
         foreach (RaidGroup group in Groups)
         {
            if (!group.GetReadonlyInvitedAll().IsEmpty)
            {
               return true;
            }
         }
         return false;
      }
   }
}