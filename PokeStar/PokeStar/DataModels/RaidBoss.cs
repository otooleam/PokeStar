using System.Collections.Generic;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Raid boss Pokémon.
   /// </summary>
   public class RaidBoss
   {
      /// <summary>
      /// Name of the raid boss.
      /// </summary>
      public string Name { get; set; } = Global.DEFAULT_RAID_BOSS_NAME;

      /// <summary>
      /// Base attack of the raid boss.
      /// </summary>
      public int Attack { get; set; }

      /// <summary>
      /// Base defense of the raid boss.
      /// </summary>
      public int Defense { get; set; }

      /// <summary>
      /// Base stamina(hp) of the raid boss.
      /// </summary>
      public int Stamina { get; set; }

      /// <summary>
      /// List of the raid boss's type(s).
      /// </summary>
      public List<string> Type { get; } = new List<string>();

      /// <summary>
      /// List of weather that boosts the raid boss.
      /// </summary>
      public List<string> Weather { get; set; }

      /// <summary>
      /// List of types the raid boss is weak to.
      /// </summary>
      public List<string> Weakness { get; set; }

      /// <summary>
      /// List of types the raid boss is resistant to.
      /// </summary>
      public List<string> Resistance { get; set; }

      /// <summary>
      /// List of counters to the raid boss.
      /// </summary>
      public List<Counter> Counter { get; set; }

      /// <summary>
      /// Minimum CP of the raid boss.
      /// </summary>
      public int CPLow { get; set; }

      /// <summary>
      /// Maximum CP of the raid boss.
      /// </summary>
      public int CPHigh { get; set; }

      /// <summary>
      /// Minimum weather boosted CP of the raid boss.
      /// </summary>
      public int CPLowBoosted { get; set; }

      /// <summary>
      /// Maximum weather boosted CP of the raid boss.
      /// </summary>
      public int CPHighBoosted { get; set; }
   }
}