using PokeStar.DataModels;
using System;
using System.Collections.Generic;

namespace PokeStar.Calculators
{
   /// <summary>
   /// Calculates CP of a Pokémon.
   /// </summary>
   public static class CPCalculator
   {
      /// <summary>
      /// CPM for whole levels.
      /// </summary>
      private static readonly double[] discrete_cp_multiplier = {
         0.094     ,  0.16639787,  0.21573247,  0.25572005,  0.29024988,
         0.3210876 ,  0.34921268,  0.37523559,  0.39956728,  0.42250001,
         0.44310755,  0.46279839,  0.48168495,  0.49985844,  0.51739395,
         0.53435433,  0.55079269,  0.56675452,  0.58227891,  0.59740001,
         0.61215729,  0.62656713,  0.64065295,  0.65443563,  0.667934  ,
         0.68116492,  0.69414365,  0.70688421,  0.71939909,  0.7317    ,
         0.73776948,  0.74378943,  0.74976104,  0.75568551,  0.76156384,
         0.76739717,  0.7731865 ,  0.77893275,  0.78463697,  0.79030001,
         0.79530001,  0.8003    ,  0.8053    ,  0.81029999,  0.81529999,
      };

      /// <summary>
      /// CPM step for half levels.
      /// </summary>
      private static readonly double[][] cpm_step_list = {
          new double[] { 1, 9.5, 0.009426125469 },
          new double[] { 10, 19.5, 0.008919025675 },
          new double[] { 20, 29.5, 0.008924905903 },
          new double[] { 30, 39.5, 0.00445946079 },
      };

      /// <summary>
      /// Calculates the CPM of a half level (X.5).
      /// It is assumed that the level is a half level.
      /// </summary>
      /// <param name="halfLevel">HalfLevel to calculate CPM for.</param>
      /// <returns>CPM for the half level</returns>
      private static double CalcHalfLevelCPM(double halfLevel)
      {
         double cpmStep = 0;
         for (int i = 0; i < cpm_step_list.Length && cpmStep == 0; i++)
         {
            if (halfLevel <= cpm_step_list[i][1])
            {
               cpmStep = cpm_step_list[i][2];
            }
         }
         double cpmLevelSqrt = Math.Pow(discrete_cp_multiplier[(int)(halfLevel - 1.5)], 2);

         return Math.Sqrt(cpmLevelSqrt + cpmStep);
      }

      /// <summary>
      /// Calculates the CPM for a level.
      /// </summary>
      /// <param name="level">Level to calculate CPM for.</param>
      /// <returns>CPM of the level.</returns>
      private static double CalcCPM(double level)
      {
         if (level % 1.0 != 0)
         {
            return CalcHalfLevelCPM(level);
         }
         else
         {
            return discrete_cp_multiplier[(int)(level - 1)];
         }
      }

      /// <summary>
      /// Calculates the attack value for a Pokémon.
      /// </summary>
      /// <param name="attackStat">Attack stat of the Pokémon.</param>
      /// <param name="attackIv">Attack IV of the Pokémon.</param>
      /// <param name="level">Level of the Pokémon.</param>
      /// <returns>Attack value of the Pokémon.</returns>
      private static double CalcAttack(int attackStat, int attackIv, double level)
      {
         return (attackStat + attackIv) * CalcCPM(level);
      }

      /// <summary>
      /// Calculates the defense value for a Pokémon.
      /// </summary>
      /// <param name="defenseStat">Defense stat of the Pokémon.</param>
      /// <param name="defenseIv">Defense IV of the Pokémon.</param>
      /// <param name="level">Level of the Pokémon.</param>
      /// <returns>Defense value of the Pokémon.</returns>
      private static double CalcDefense(int defenseStat, int defenseIv, double level)
      {
         return (defenseStat + defenseIv) * CalcCPM(level);
      }

      /// <summary>
      /// Calculates the stamina (HP) for a pokemon.
      /// </summary>
      /// <param name="staminaStat">Stamina stat of the pokemon.</param>
      /// <param name="staminaIv">Stamina IV of the pokemon.</param>
      /// <param name="level">Level of the pokemon.</param>
      /// <returns>Stamina value of the pokemon.</returns>
      private static double CalcStamina(int staminaStat, int staminaIv, double level)
      {
         return (staminaStat + staminaIv) * CalcCPM(level);
      }

      /// <summary>
      /// Calculates the CP of Pokémon at a given level.
      /// </summary>
      /// <param name="attackStat">Attack stat of the Pokémon.</param>
      /// <param name="defenseStat">Defense stat of the Pokémon.</param>
      /// <param name="staminaStat">Stamina stat of the Pokémon.</param>
      /// <param name="attackIv">Attack IV of the Pokémon.</param>
      /// <param name="defenseIv">Defense IV of the Pokémon.</param>
      /// <param name="staminaIv">Stamina IV of the Pokémon.</param>
      /// <param name="level">Level of the Pokémon.</param>
      /// <returns>CP of the Pokémon.</returns>
      public static int CalcCPPerLevel(int attackStat, int defenseStat, int staminaStat,
                                       int attackIv, int defenseIv, int staminaIv, double level)
      {
         double attack = CalcAttack(attackStat, attackIv, level);
         double defense = CalcDefense(defenseStat, defenseIv, level);
         double stamina = CalcStamina(staminaStat, staminaIv, level);
         return (int)(attack * Math.Sqrt(defense) * Math.Sqrt(stamina) / 10.0);
      }

      /// <summary>
      /// Calculates the rank 1 PvP IVs for a Pokémon in a given league.
      /// </summary>
      /// <param name="attackStat">Attack stat of the Pokémon.</param>
      /// <param name="defenseStat">Defense stat of the Pokémon.</param>
      /// <param name="staminaStat">Stamina stat of the Pokémon.</param>
      /// <param name="leagueCap">Max CP of the league.</param>
      /// <returns>LeagueIV object with the best IVs for the league.</returns>
      public static LeagueIV CalcPvPIVsPerLeague(int attackStat, int defenseStat, int staminaStat, int leagueCap)
      {
         int bestA = -1;
         int bestD = -1;
         int bestS = -1;
         int bestCP = -1;
         int bestTotal = -1;
         double bestProduct = -1;

         for (double l = 1; l <= Global.MAX_LEVEL; l += Global.LEVEL_STEP)
         {
            for (int a = 0; a <= Global.MAX_IV; a++)
            {
               for (int d = 0; d <= Global.MAX_IV; d++)
               {
                  for (int s = 0; s <= Global.MAX_IV; s++)
                  {
                     int cpLevel = CalcCPPerLevel(attackStat, defenseStat, staminaStat, a, d, s, l);
                     int total = a + d + s;
                     double product = CalcAttack(attackStat, a, l) * CalcDefense(defenseStat, d, l) * Math.Floor(CalcStamina(staminaStat, s, l));
                     if (cpLevel <= leagueCap && (product > bestProduct || (product == bestProduct && total > bestTotal)))
                     {
                        bestA = a;
                        bestD = d;
                        bestS = s;
                        bestTotal = total;
                        bestCP = cpLevel;
                        bestProduct = product;
                     }
                  }
               }
            }
         }
         return new LeagueIV
         {
            Attack = bestA,
            Defense = bestD,
            Stamina = bestS,
            CP = bestCP,
            StatProduct = bestProduct
         };
      }
   }
}