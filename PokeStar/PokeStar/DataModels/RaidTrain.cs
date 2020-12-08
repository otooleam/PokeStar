using System;
using System.Linq;
using System.Collections.Generic;

namespace PokeStar.DataModels
{
   public class RaidTrain : Raid
   {
      private readonly List<string> Locations = new List<string>();
      private int CurrentGym;

      public RaidTrain(short tier, string time, string location, string boss = null) : base(tier, time, location, boss) 
      {
         Locations.Add(location);
         CurrentGym = 0;
      }

      public string GetCurrentGymCount()
      {
         return $"{CurrentGym + 1} / {Locations.Count}";
      }

      public string GetCurrentGym()
      {
         return Locations.ElementAt(CurrentGym);
      }

      public string GetNextGym()
      {
         return (CurrentGym + 1 == Locations.Count) ? Global.EMPTY_FIELD : Locations.ElementAt(CurrentGym + 1);
      }

      public void AddGym(string gym)
      {
         if (!Locations.Contains(gym))
         {
            Locations.Add(gym);
         }
      }

      public bool NextGym()
      {
         int OldGym = CurrentGym;
         if (CurrentGym < Locations.Count - 1)
         {
            CurrentGym++;
         }
         if (OldGym != CurrentGym)
         {
            foreach (RaidGroup group in Groups)
            {
               group.ResetReady();
            }
            return true;
         }
         return false;
      }

      public bool PreviousGym()
      {
         int OldGym = CurrentGym;
         CurrentGym = Math.Max(--CurrentGym, 0);
         return OldGym != CurrentGym;
      }
   }
}