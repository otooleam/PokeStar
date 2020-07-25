using System;
using System.Collections.Generic;
using Discord.WebSocket;
using PokeStar.ConnectionInterface;

namespace PokeStar.DataModels
{
   class Raid
   {
      private const int GROUP_LIMIT = 5;

      public string Location { get; set; }
      public string Time { get; set; }
      public short Tier { get; set; }
      public RaidBoss Boss { get; private set; }
      public PlayerGroup[] RaidPartyList { get; private set; }
      public List<SocketGuildUser> InviteReqs { get; private set; }
      public DateTime CreatedAt { get; private set; }

      public PlayerGroup groupTEST { get; private set; }

      public Raid(short tier, string time, string location, string boss = null)
      {
         Tier = tier;
         Time = time;
         Location = location;
         SetBoss(boss);
         CreatedAt = DateTime.Now;
         InviteReqs = new List<SocketGuildUser>();
         RaidPartyList = new PlayerGroup[GROUP_LIMIT];
         RaidPartyList[0] = new PlayerGroup();
         
         groupTEST = new PlayerGroup();
      }

      public void PlayerAdd(SocketGuildUser player, int partySize)
      {
         if (RaidPartyList.Length == 1)
         {
            if (!RaidPartyList[0].PlayerAdd(player, partySize))
            {
               //split the party

            }
         }
         else
         {
            int[] playerCount = new int[GROUP_LIMIT];
            for (int i = 0; i < GROUP_LIMIT; i++)
            {
               if (RaidPartyList[i].Players.Contains(player))
                  RaidPartyList[i].PlayerAdd(player, partySize); //update player
               else
                  playerCount[i] = RaidPartyList[i].Players.Count;
            }

         }
      }

      private int FindMinIndex(int[] array)
      {
         int min = 100;
         int mindex = -1;
         for (int i = 0; i < array.Length; i++)
         {
            if (array[i] < min)
            {
               min = array[i];
               mindex = i;
            }
         }
         return mindex;
      }

      public bool PlayerReady(SocketGuildUser player) //returns if party all here
      {
         return groupTEST.PlayerReady(player);
      }

      public void PlayerRequestInvite(SocketGuildUser player)
      {
         InviteReqs.Add(player); //TODO check if they already exist
      }

      public bool InvitePlayer(SocketGuildUser invitee, SocketGuildUser invitingPlayer)
      {
         if (InviteReqs.Contains(invitee))
         {
            groupTEST.PlayerAdd(invitingPlayer, -1);
            return true;
         }
         return false;
      }

      public void RemovePlayer(SocketGuildUser player)
      {
         groupTEST.RemovePlayer(player);
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
   }
}
