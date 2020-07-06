﻿using Discord.WebSocket;
using PokeStar.ConnectionInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeStar.DataModels
{
   class Raid
   {
      private const int playerLimit = 20;

      public Raid(short tier, string time, string location, string boss = null)
      {
         Tier = tier;
         Time = time;
         Location = location;
         PlayerCount = 0;
         HereCount = 0;
         SetBoss(boss);
         Attending = new Dictionary<SocketGuildUser, int>();
         Here = new Dictionary<SocketGuildUser, int>();
      }

      public string Location { get; set; }
      public string Time { get; set; }
      public short Tier { get; set; }
      public RaidBoss Boss { get; private set; }
      public int PlayerCount { get; private set; }
      public int HereCount { get; private set; }

      public Dictionary<SocketGuildUser, int> Attending;
      public Dictionary<SocketGuildUser, int> Here;

      public void PlayerAdd(SocketGuildUser player, int partySize)
      {
         if (Attending.ContainsKey(player))
         {
            int newPlayerCount = PlayerCount + (partySize - Attending[player]);
            if (newPlayerCount <= playerLimit)
            {
               Attending[player] = partySize;
               PlayerCount = newPlayerCount;
            }
         }
         else if (Here.ContainsKey(player))
         {
            int newPlayerCount = PlayerCount + (partySize - Here[player]);
            if (newPlayerCount <= playerLimit)
            {
               HereCount += partySize - Here[player];
               PlayerCount = newPlayerCount;
               Here[player] = partySize;
            }
         }
         else
         {
            int newPlayerCount = PlayerCount + partySize;
            if (newPlayerCount <= 20)
            {
               Attending.Add(player, partySize);
               PlayerCount = newPlayerCount;
            }
         }
      }

      public bool PlayerHere(SocketGuildUser player)
      {
         if (Attending.ContainsKey(player))
         {
            Here.Add(player, Attending[player]);
            Attending.Remove(player);
            HereCount += Here[player];

            if (Attending.Count == 0)
               return true;
            return false;
         }
         return false;
      }

      public void RemovePlayer(SocketGuildUser player)
      {
         if (Attending.ContainsKey(player))
         {
            PlayerCount -= Attending[player];
            Attending.Remove(player);
         }
         else if (Here.ContainsKey(player))
         {
            PlayerCount -= Here[player];
            HereCount -= Here[player];
            Here.Remove(player);
         }
      }

      private List<string> GetBossData(short tier)
      {
         return Connections.Instance().GetBossList(tier);
      }

      public void SetBoss(string bossName)
      {
         if (bossName != null)
            Boss = Connections.Instance().GetRaidBoss(bossName);
      }
   }
}
