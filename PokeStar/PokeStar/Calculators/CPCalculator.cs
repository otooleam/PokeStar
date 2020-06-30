using System;

namespace PokeStar.Calculators
{
   public static class CPCalculator
   {
      public static readonly int MAX_LEVEL = 40;
      public static readonly int MIN_WILD_LEVEL = 1;
      public static readonly int MAX_WILD_LEVEL = 35;
      public static readonly int RAID_LEVEL = 20;
      public static readonly int QUEST_LEVEL = 15;
      public static readonly int HATCH_LEVEL = 20;
      public static readonly int WEATHER_BOOST = 5;
      public static readonly int BUDDY_BOOST = 1;

      public static readonly int MIN_SPECIAL_IV = 10;
      public static readonly int MAX_IV = 15;

      private static double[] discrete_cp_multiplier = {
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

      private static double[][] cpm_step_list = {
          new double[] { 1, 9.5, 0.009426125469 },
          new double[] { 10, 19.5, 0.008919025675 },
          new double[] { 20, 29.5, 0.008924905903 },
          new double[] { 30, 39.5, 0.00445946079 },
      };

      private static double CalcHalfLevelCPM(double level)
      {
         double cpmStep = 0;
         for (int i = 0; i < cpm_step_list.Length && cpmStep == 0; i++)
            if (level <= cpm_step_list[i][1])
               cpmStep = cpm_step_list[i][2];
         double cpmLevelSqrt = Math.Pow(discrete_cp_multiplier[(int)(level - 1.5)], 2);

         return Math.Sqrt(cpmLevelSqrt + cpmStep);
      }

      private static double CalcCPM(double level)
      {
         if (level % 1.0 == 0)
            return CalcHalfLevelCPM(level);
         else
            return discrete_cp_multiplier[(int)(level - 1)];
      }

      private static double CalcAttack(int attack, int attackIV, double level)
      {
         return (attack + attackIV) * CalcCPM(level);
      }

      private static double CalcDefense(int defense, int defenseIV, double level)
      {
         return (defense + defenseIV) * CalcCPM(level);
      }

      private static double CalcStamina(int stamina, int staminaIV, double level)
      {
         return (stamina + staminaIV) * CalcCPM(level);
      }

      public static int CalcCPPerLevel(int baseAttack, int baseDefense, int baseStamina, int attackIV, int defenseIV, int staminaIV, double level)
      {
         double attack = CalcAttack(baseAttack, attackIV, level);
         double defense = CalcDefense(baseDefense, defenseIV, level);
         double stamina = CalcStamina(baseStamina, staminaIV, level);
         return (int)(attack * Math.Sqrt(defense) * Math.Sqrt(stamina) / 10.0);
      }
   }
}
