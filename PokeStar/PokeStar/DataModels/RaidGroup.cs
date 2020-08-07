using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.WebSocket;

namespace PokeStar.DataModels
{
   public class RaidGroup
   {
      public readonly int playerLimit = 12;
      public readonly int inviteLimit = 10;
      private Dictionary<SocketGuildUser, int> Attending { get; set; }
      private Dictionary<SocketGuildUser, int> Ready { get; set; }
      private Dictionary<SocketGuildUser, SocketGuildUser> Invited { get; set; }

      public RaidGroup()
      {
         Attending = new Dictionary<SocketGuildUser, int>();
         Ready = new Dictionary<SocketGuildUser, int>();
         Invited = new Dictionary<SocketGuildUser, SocketGuildUser>();
      }

      public ImmutableDictionary<SocketGuildUser, int> GetReadonlyAttending()
      {
         return Attending.ToImmutableDictionary(k => k.Key, v => v.Value);
      }

      public ImmutableDictionary<SocketGuildUser, int> GetReadonlyHere()
      {
         return Ready.ToImmutableDictionary(k => k.Key, v => v.Value);
      }

      public ImmutableDictionary<SocketGuildUser, SocketGuildUser> GetReadonlyInvited()
      {
         return Invited.ToImmutableDictionary(k => k.Key, v => v.Value);
      }

      public int GetAttendingCount()
      {
         int total = 0;
         foreach (var player in Attending)
            total += player.Value;
         return total;
      }

      public int GetHereCount()
      {
         int total = 0;
         foreach (var player in Ready)
            total += player.Value;
         return total;
      }

      public int GetInvitedCount()
      {
         return Invited.Count;
      }

      public void Add(SocketGuildUser user, int partySize)
      {
         if (Attending.ContainsKey(user))
            Attending[user] = partySize;
         else if (Ready.ContainsKey(user))
            Ready[user] = partySize;
         else
            Attending.Add(user, partySize);
      }

      public void Remove(SocketGuildUser user)
      {
         if (Attending.ContainsKey(user))
            Attending.Remove(user);
         else if (Ready.ContainsKey(user))
            Ready.Remove(user);
         else if (Invited.ContainsKey(user))
            Invited.Remove(user);
      }

      public void PlayerReady(SocketGuildUser user)
      {
         if (Attending.ContainsKey(user))
         {
            Ready.Add(user, Attending[user]);
            Attending.Remove(user);
         }
      }

      public bool Invite(SocketGuildUser requester, SocketGuildUser accepter)
      {
         if (HasPlayer(accepter, false) && !HasPlayer(requester) && (TotalPlayers() + 1) <= playerLimit && (Invited.Count + 1) <= inviteLimit)
         {
            Invited.Add(requester, accepter);
            return true;
         }
         return false;
      }

      public bool AllPlayersReady()
      {
         return Attending.Count == 0 && Ready.Count != 0;
      }

      public List<SocketGuildUser> GetPingList()
      {
         return Ready.Keys.ToList().Union(Invited.Keys.ToList()).ToList();
      }

      public int TotalPlayers()
      {
         return GetAttendingCount() + GetHereCount() + GetInvitedCount();
      }

      public bool HasPlayer(SocketGuildUser user, bool checkInvite = true)
      {
         return Attending.ContainsKey(user) || Ready.ContainsKey(user) || (checkInvite && Invited.ContainsKey(user));
      }

      public RaidGroup SplitGroup()
      {
         if (TotalPlayers() <= playerLimit)
            return null;

         var newGroup = new RaidGroup();
         foreach (var player in Attending)
         {
            if ((newGroup.TotalPlayers() + player.Value) <= playerLimit / 2)
            {
               newGroup.Attending.Add(player.Key, player.Value);

               foreach (var invite in Invited)
                  if (invite.Value.Equals(player.Key))
                     newGroup.Invite(invite.Key, invite.Value);
            }
         }

         if (newGroup.TotalPlayers() < playerLimit / 2)
         {
            foreach (var player in Ready)
            {
               if (newGroup.TotalPlayers() < playerLimit / 2)
               {
                  newGroup.Ready.Add(player.Key, player.Value);
                  foreach (var invite in Invited)
                     if (invite.Value.Equals(player.Key))
                        newGroup.Invite(invite.Key, invite.Value);
               }
            }
         }

         foreach (var player in newGroup.Attending.Keys)
            Attending.Remove(player);
         foreach (var player in newGroup.Ready.Keys)
            Ready.Remove(player);
         foreach (var player in newGroup.Invited.Keys)
            Invited.Remove(player);
         return newGroup;
      }

      public void MergeGroup(RaidGroup group)
      {
         if (!group.Equals(this) &&
            group.TotalPlayers() != 0 && TotalPlayers() != 0 &&
            (group.TotalPlayers() + TotalPlayers()) <= playerLimit)
         {
            Attending = Attending.Union(group.Attending).ToDictionary(k => k.Key, v => v.Value);
            Ready = Ready.Union(group.Ready).ToDictionary(k => k.Key, v => v.Value);
            Invited = Invited.Union(group.Invited).ToDictionary(k => k.Key, v => v.Value);
            group.Attending.Clear();
            group.Ready.Clear();
            group.Invited.Clear();
         }
      }
   }
}