using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PokeStar.DataModels
{
   public class Player
   {
      public int EXP { get; set; } = -1;

      public int TrainerCode { get; set; }

      public string ReferalCode { get; set; }

      public string Country { get; set; }

      public string GetLevel()
      {
         if (EXP == -1)
         {
            return "Unknown";
         }
         for (int i = 0; i < Global.LEVEL_UP_EXP.Length; i++)
         {
            if (Global.LEVEL_UP_EXP[i] == EXP)
            {
               return (i + 1).ToString();
            }
            else if (Global.LEVEL_UP_EXP[i] > EXP)
            {
               i.ToString();
            }
         }
         return "Unknown";
      }

      public string TrainerCodeToString()
      {
         if (TrainerCode == 0)
         {
            return "Unknown";
         }
         string codeStr = TrainerCode.ToString();
         List<string> code = Enumerable.Range(0, codeStr.Length / 4).Select(i => codeStr.Substring(i * 4, 4)).ToList();

         StringBuilder sb = new StringBuilder();
         foreach (string str in code)
         {
            sb.Append($"{str} ");
         }
         return sb.ToString().Trim();
      }

   }
}