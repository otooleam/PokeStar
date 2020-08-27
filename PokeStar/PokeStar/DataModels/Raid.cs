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
      public Raid(short tier, string time, string location, string boss = null) : base(tier, time, location, boss) 
      {
         RaidGroupLimit = 3;
         PlayerLimit = 20;
         InviteLimit = 10;
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
      public override bool PlayerAdd(SocketGuildUser player, int partySize, SocketGuildUser invitedBy = null)
      {
         int group;
         if (invitedBy == null)
         {
            group = IsInRaid(player);
            if (group == NotInRaid)
               group = FindSmallestGroup();
            if (group != InviteListNumber)
               Groups.ElementAt(group).Add(player, partySize);
            else
               return false;
         }
         else // is remote
         {
            group = IsInRaid(invitedBy);
            if (group != NotInRaid)
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
            return true;

         Groups.ElementAt(group).Remove(player);
         if (invitedBy != null)
            Invite.Add(player);
         return false;
      }

      /// <summary>
      /// Removes a player from the raid.
      /// </summary>
      /// <param name="player">Player to remove.</param>
      /// <returns>Struct with raid group and list of invited users.</returns>
      public override RemovePlayerReturn RemovePlayer(SocketGuildUser player)
      {
         RemovePlayerReturn returnValue = new RemovePlayerReturn
         {
            GroupNum = NotInRaid,
            invited = new List<SocketGuildUser>()
         };

         int group = IsInRaid(player);
         if (group == InviteListNumber)
            Invite.Remove(player);
         else
         {
            if (group != NotInRaid)
            {
               RaidGroup foundGroup = Groups.ElementAt(group);
               returnValue.invited = foundGroup.Remove(player);
               foreach (SocketGuildUser invite in returnValue.invited)
                  Invite.Add(invite);
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
         if (IsInRaid(player) == NotInRaid)
            Invite.Add(player);
      }

      /// <summary>
      /// Accepts an invite of a player.
      /// </summary>
      /// <param name="requester">Player that requested the invite.</param>
      /// <param name="accepter">Player that accepted the invite.</param>
      /// <returns>True if the requester was invited, otherwise false.</returns>
      public override bool InvitePlayer(SocketGuildUser requester, SocketGuildUser accepter)
      {
         if ((IsInRaid(requester) == InviteListNumber && IsInRaid(accepter, false) != 1) || requester.Equals(accepter))
            return PlayerAdd(requester, 1, accepter);
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
            return InviteListNumber;
         for (int i = 0; i < Groups.Count; i++)
         {
            if (Groups.ElementAt(i).HasPlayer(player, checkInvite))
            {
               return i;
            }
         }
         return -1;
      }

      public int PlayerReady(SocketGuildUser player)
      {
         int groupNum = IsInRaid(player, false);
         if (groupNum != NotInRaid && groupNum != InviteListNumber)
         {
            RaidGroup group = Groups.ElementAt(groupNum);
            return (group.PlayerReady(player) && group.AllPlayersReady()) ? groupNum : NotInRaid;
         }
         return NotInRaid;
      }
   }
}