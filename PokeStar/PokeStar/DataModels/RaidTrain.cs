using System;
using System.Linq;
using System.Collections.Generic;
using Discord.WebSocket;
using PokeStar.ConnectionInterface;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Raid train to fight multiple raid bosses.
   /// </summary>
   public class RaidTrain : Raid
   {
      /// <summary>
      /// Index of the current location.
      /// </summary>
      private int CurrentLocation;

      /// <summary>
      /// List of all locations the train will visit.
      /// </summary>
      private readonly List<RaidTrainLoc> Locations = new List<RaidTrainLoc>();

      /// <summary>
      /// The player who is incharge of advancing the train.
      /// </summary>
      public SocketGuildUser Conductor { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public Dictionary<int, List<string>> AllBosses { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public short SelectionTier { get; set; }

      /// <summary>
      /// Creates a new raid.
      /// </summary>
      /// <param name="conductor">Player in charge of the raid.</param>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">When the raid starts.</param>
      /// <param name="location">Where the raid starts.</param>
      /// <param name="boss">Name of the raid boss.</param>
      public RaidTrain(SocketGuildUser conductor, short tier, string time, string location, string boss = null) :
         base(tier, time, location, boss)
      {
         CurrentLocation = 0;
         Locations.Add(new RaidTrainLoc(time, location, Boss == null ? Global.EMPTY_FIELD : Boss.Name));
         Conductor = conductor;
         SelectionTier = Tier;
      }

      /// <summary>
      /// Gets the name of the current gym count over the total count.
      /// </summary>
      /// <returns></returns>
      public string GetCurrentGymCount()
      {
         return $"{CurrentLocation + 1} / {Locations.Count}";
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public string GetCurrentTime()
      {
         return Locations.ElementAt(CurrentLocation).Time;
      }

      /// <summary>
      /// Gets the name of the current location.
      /// </summary>
      /// <returns></returns>
      public string GetCurrentLocation()
      {
         return $"{Locations.ElementAt(CurrentLocation).Location}";
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public string GetCurrentBoss()
      {
         return Locations.ElementAt(CurrentLocation).BossName;
      }

      /// <summary>
      /// Gets the name of the next location.
      /// </summary>
      /// <returns></returns>
      public string GetNextLocation()
      {
         return (CurrentLocation + 1 == Locations.Count) ? Global.EMPTY_FIELD : $"{Locations.ElementAt(CurrentLocation + 1).Location} ({Locations.ElementAt(CurrentLocation + 1).BossName})";
      }

      /// <summary>
      /// Adds a location to the list of locations.
      /// </summary>
      /// <param name="time"></param>
      /// <param name="location"></param>
      public void AddLocation(string time, string location)
      {
         if (!Locations.Any(raidTrainLoc => raidTrainLoc.Location.Equals(location, StringComparison.OrdinalIgnoreCase)))
         {
            Locations.Add(new RaidTrainLoc(time, location, Boss.Name));
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="time"></param>
      /// <param name="location"></param>
      public void UpdateLocation(string time = null, string location = null)
      {
         if (time != null)
         {
            Locations[CurrentLocation] = new RaidTrainLoc(time, Locations[CurrentLocation].Location, Locations[CurrentLocation].BossName);
         }
         if (location != null)
         {
            Locations[CurrentLocation] = new RaidTrainLoc(Locations[CurrentLocation].Time, location, Locations[CurrentLocation].BossName);
         }
      }

      /// <summary>
      /// Sets the boss of the raid.
      /// </summary>
      /// <param name="bossName">Name of the raid boss.</param>
      public override void SetBoss(string bossName)
      {
         Boss = string.IsNullOrEmpty(bossName) ? null : bossName.Equals(Global.DEFAULT_RAID_BOSS_NAME, StringComparison.OrdinalIgnoreCase) ? new Pokemon() : Connections.Instance().GetPokemon(bossName);
         if (Locations.Count != 0)
         {
            Locations[0] = new RaidTrainLoc(Locations[0].Time, Locations[0].Location, bossName);
         }
      }

      /// <summary>
      /// Update the name of the boss at the current location.
      /// Selection tier should be updated first to ensure correct
      /// raid boss is selected.
      /// </summary>
      /// <param name="index">Index of the new boss in the tier list.</param>
      public void UpdateBoss(int index)
      {
         if (CurrentLocation != 0)
         {
            string bossName = AllBosses[SelectionTier].ElementAt(index);
            Locations[CurrentLocation] = new RaidTrainLoc(Locations[CurrentLocation].Time, Locations[CurrentLocation].Location, bossName);
            SelectionTier = Tier;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="tier"></param>
      public void SetSelectionTier(short tier)
      {
         SelectionTier = tier;
      }

      /// <summary>
      /// Counts forward to the next location.
      /// If the count can be moved all players are reset to attending.
      /// </summary>
      /// <returns></returns>
      public bool NextLocation()
      {
         int OldLocation = CurrentLocation;
         if (CurrentLocation < Locations.Count - 1)
         {
            CurrentLocation++;
         }
         if (OldLocation != CurrentLocation)
         {
            foreach (RaidGroup group in Groups)
            {
               group.ResetReady();
            }
            return true;
         }
         return false;
      }

      /// <summary>
      /// Counts back to the previous location.
      /// </summary>
      /// <returns></returns>
      public bool PreviousLocation()
      {
         int OldLocation = CurrentLocation;
         CurrentLocation = Math.Max(--CurrentLocation, 0);
         return OldLocation != CurrentLocation;
      }

      /// <summary>
      /// Checks if all participants in the raid are ready.
      /// Checks all raid groups.
      /// </summary>
      /// <returns></returns>
      public bool AllReady()
      {
         bool allReady = true;
         foreach (RaidGroup group in Groups)
         {
            allReady = allReady && group.AllPlayersReady();
         }
         return allReady;
      }
   }
}