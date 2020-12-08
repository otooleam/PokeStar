using System;
using System.Linq;
using System.Collections.Generic;
using Discord.WebSocket;

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
      private readonly List<string> Locations = new List<string>();

      /// <summary>
      /// The player who is incharge of advancing the train.
      /// </summary>
      public SocketGuildUser Conductor { get; set; }

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
         Locations.Add(location);
         Conductor = conductor;
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
      /// Gets the name of the current location.
      /// </summary>
      /// <returns></returns>
      public string GetCurrentLocation()
      {
         return Locations.ElementAt(CurrentLocation);
      }

      /// <summary>
      /// Gets the name of the next location.
      /// </summary>
      /// <returns></returns>
      public string GetNextLocation()
      {
         return (CurrentLocation + 1 == Locations.Count) ? Global.EMPTY_FIELD : Locations.ElementAt(CurrentLocation + 1);
      }

      /// <summary>
      /// Adds a location to the list of locations.
      /// </summary>
      /// <param name="location"></param>
      public void AddLocation(string location)
      {
         if (!Locations.Contains(location))
         {
            Locations.Add(location);
         }
      }

      /// <summary>
      /// Counts forward to the next location.
      /// If the count can be moved all players are reset to attending.
      /// </summary>
      /// <returns></returns>
      public bool NextLocation()
      {
         int OldGym = CurrentLocation;
         if (CurrentLocation < Locations.Count - 1)
         {
            CurrentLocation++;
         }
         if (OldGym != CurrentLocation)
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
         int OldGym = CurrentLocation;
         CurrentLocation = Math.Max(--CurrentLocation, 0);
         return OldGym != CurrentLocation;
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