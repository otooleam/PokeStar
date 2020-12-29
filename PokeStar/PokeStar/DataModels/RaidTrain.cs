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
      /// The player in charge of the train.
      /// </summary>
      public SocketGuildUser Conductor { get; set; }

      /// <summary>
      /// List of all current raid bosses.
      /// </summary>
      public Dictionary<int, List<string>> AllBosses { get; set; }

      /// <summary>
      /// Tier of new raid boss to be selected from.
      /// </summary>
      public short SelectionTier { get; set; }

      /// <summary>
      /// Creates a new raid train.
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
      /// Check if the current location is the first location.
      /// </summary>
      /// <returns>True if first location, otherwise false.</returns>
      public bool IsFirstLocation()
      {
         return CurrentLocation == 0;
      }

      /// <summary>
      /// Gets the name of the current gym count over the total count.
      /// </summary>
      /// <returns></returns>
      public string GetCurrentRaidCount()
      {
         return $"{CurrentLocation + 1} / {Locations.Count}";
      }

      /// <summary>
      /// Gets the time for the current raid.
      /// </summary>
      /// <returns>Time of the current raid.</returns>
      public string GetCurrentTime()
      {
         return Locations.ElementAt(CurrentLocation).Time;
      }

      /// <summary>
      /// Gets the location for the current raid.
      /// </summary>
      /// <returns>Location of the current raid.</returns>
      public string GetCurrentLocation()
      {
         return $"{Locations.ElementAt(CurrentLocation).Location}";
      }

      /// <summary>
      /// Gets the name of the current raid boss.
      /// </summary>
      /// <returns>Name of the current raid boss.</returns>
      public string GetCurrentBoss()
      {
         return Locations.ElementAt(CurrentLocation).BossName;
      }

      /// <summary>
      /// Gets the information for the next raid.
      /// </summary>
      /// <returns>Information for the next raid.</returns>
      public string GetNextRaid()
      {
         return (CurrentLocation + 1 == Locations.Count) ? Global.EMPTY_FIELD : $"{Locations.ElementAt(CurrentLocation + 1).Location} ({Locations.ElementAt(CurrentLocation + 1).BossName})";
      }

      /// <summary>
      /// Adds a raid to the list of locations.
      /// </summary>
      /// <param name="time">Time of the raid.</param>
      /// <param name="location">Location of the raid.</param>
      public void AddRaid(string time, string location)
      {
         if (!Locations.Any(raidTrainLoc => raidTrainLoc.Location.Equals(location, StringComparison.OrdinalIgnoreCase)))
         {
            Locations.Add(new RaidTrainLoc(time, location, Boss.Name));
         }
      }

      /// <summary>
      /// Update information for a location.
      /// </summary>
      /// <param name="time">New time of the raid.</param>
      /// <param name="location">New location of the raid.</param>
      public void UpdateRaidInformation(string time = null, string location = null)
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
      /// Sets the tier to select the raid boss from.
      /// </summary>
      /// <param name="tier">Tier of the raid bosses.</param>
      public void SetSelectionTier(short tier)
      {
         SelectionTier = tier;
      }

      /// <summary>
      /// Counts forward to the next location.
      /// If the count can be moved all players are reset to attending.
      /// </summary>
      /// <returns>True if the location changed, otherwise false.</returns>
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
      /// <returns>True if the location changed, otherwise false.</returns>
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
      /// <returns>True if all players in all raid groups are ready, otherwise false.</returns>
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