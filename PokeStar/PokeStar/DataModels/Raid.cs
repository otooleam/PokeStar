using System;
using System.Linq;
using System.Collections.Generic;
using Discord.WebSocket;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Raid to fight against a raid boss.
   /// </summary>
   public class Raid : RaidParent
   {
      /// <summary>
      /// Creates a new raid.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">When the raid starts.</param>
      /// <param name="location">Where the raid is.</param>
      /// <param name="boss">Name of the raid boss.</param>
      public Raid(short tier, string time, string location, string boss = null) : 
         base(Global.LIMIT_RAID_GROUP, Global.LIMIT_RAID_PLAYER, Global.LIMIT_RAID_INVITE, tier, time, location, boss) { }

      /// <summary>
      /// Adds a player to a raid.
      /// The player will not be added if splitting the group brings the number of
      /// raid groups over the group limit.
      /// </summary>
      /// <param name="player">Player to add.</param>
      /// <param name="partySize">Number of accounts the player is bringing.</param>
      /// <param name="invitedBy">Who invited the player.</param>
      /// <returns>True if the player was added, otherwise false.</returns>
      public override bool AddPlayer(SocketGuildUser player, int partySize, SocketGuildUser invitedBy = null)
      {
         int group;
         if (invitedBy == null) // Add in person
         {
            group = IsInRaid(player);
            if (group == Global.NOT_IN_RAID)
            {
               group = FindSmallestGroup();
            }
            if (group != InviteListNumber)
            {
               Groups.ElementAt(group).AddPlayer(player, partySize, Global.NO_ADD_VALUE);
            }
            else
            {
               return false;
            }
         }
         else if (player.Equals(invitedBy)) // Remote
         {
            group = IsInRaid(player);
            if (group == Global.NOT_IN_RAID)
            {
               group = FindSmallestGroup();
            }
            if (group != InviteListNumber)
            {
               Groups.ElementAt(group).AddPlayer(player, Global.NO_ADD_VALUE, partySize);
            }
            else
            {
               return false;
            }
         }
         else // accept invite
         {
            group = IsInRaid(invitedBy);
            if (group != Global.NOT_IN_RAID)
            {
               Groups.ElementAt(group).InvitePlayer(player, invitedBy);
               Invite.Remove(player);
            }
            else if (player.Equals(invitedBy))
            {
               group = FindSmallestGroup();
               Groups.ElementAt(group).InvitePlayer(player, invitedBy);
            }
            else
            {
               return false;
            }
         }

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
            CheckMergeGroups();
            return true;
         }

         Groups.ElementAt(group).RemovePlayer(player);
         if (invitedBy != null)
         {
            Invite.Add(player);
         }
         return false;
      }

      /// <summary>
      /// Removes a player from the raid.
      /// </summary>
      /// <param name="player">Player to remove.</param>
      /// <returns>RaidRemove with raid group and list of invited users.</returns>
      public override RaidRemoveResult RemovePlayer(SocketGuildUser player)
      {
         RaidRemoveResult returnValue = new RaidRemoveResult(Global.NOT_IN_RAID, new List<SocketGuildUser>());

         int group = IsInRaid(player);
         if (group == InviteListNumber)
         {
            Invite.Remove(player);
         }
         else
         {
            if (group != Global.NOT_IN_RAID)
            {
               RaidGroup foundGroup = Groups.ElementAt(group);
               List<SocketGuildUser> tempList = foundGroup.RemovePlayer(player);
               foreach (SocketGuildUser invite in tempList)
               {
                  returnValue.Users.Add(invite);
                  Invite.Add(invite);
               }
            }
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
         if ((IsInRaid(requester) == InviteListNumber && IsInRaid(accepter, false) != Global.NOT_IN_RAID))
         {
            return AddPlayer(requester, 1, accepter);
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
      /// Removes a player if their party size is zero.
      /// Any players invited by them are moved back to requesting invite.
      /// </summary>
      /// <param name="player">Player to remove.</param>
      /// <returns>Return a dictionary of all users invited by the player.</returns>
      public Dictionary<SocketGuildUser, List<SocketGuildUser>> ClearEmptyPlayer(SocketGuildUser player)
      {
         int group = IsInRaid(player, false);
         if (group != Global.NOT_IN_RAID)
         {
            Dictionary<SocketGuildUser, List<SocketGuildUser>> empty = Groups.ElementAt(group).ClearEmptyPlayers();
            foreach (KeyValuePair<SocketGuildUser, List<SocketGuildUser>> user in empty)
            {
               Invite.AddRange(user.Value);
            }
            return empty;
         }
         return new Dictionary<SocketGuildUser, List<SocketGuildUser>>();
      }

      /// <summary>
      /// Marks a player as ready in the raid.
      /// </summary>
      /// <param name="player">Player to mark ready.</param>
      /// <returns></returns>
      public int MarkPlayerReady(SocketGuildUser player)
      {
         int groupNum = IsInRaid(player, false);
         if (groupNum != Global.NOT_IN_RAID && groupNum != InviteListNumber)
         {
            RaidGroup group = Groups.ElementAt(groupNum);
            return (group.MarkPlayerReady(player) && group.AllPlayersReady()) ? groupNum : Global.NOT_IN_RAID;
         }
         return Global.NOT_IN_RAID;
      }
   }
}