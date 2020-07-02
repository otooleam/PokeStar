using Discord;
using System;
using System.Collections.Generic;

namespace PokeStar.DataModels
{
   public class Pokemon
   {
      public int Number { get; set; }
      public int Attack { get; set; }
      public int Defense { get; set; }
      public int Stamina { get; set; }
      public int BuddyDistance { get; set; }
      public bool Shiny { get; set; }
      public bool Shadow { get; set; }
      public bool Obtainable { get; set; }
      public bool Regional { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }
      public string Region { get; set; }
      public string Category { get; set; }

      public int CPMax { get; set; }
      public int CPBestBuddy { get; set; }
      public int CPRaidMin { get; set; }
      public int CPRaidMax { get; set; }
      public int CPRaidBoostedMin { get; set; }
      public int CPRaidBoostedMax { get; set; }
      public int CPQuestMin { get; set; }
      public int CPQuestMax { get; set; }
      public int CPHatchMin { get; set; }
      public int CPHatchMax { get; set; }
      public List<int> CPWild { get; } = new List<int>();

      public List<string> Type { get; } = new List<string>();
      public List<string> Weather { get; set; }
      public List<string> Weakness { get; set; }
      public List<string> Resistance { get; set; }

      public List<Move> FastMove { get; set; }
      public List<Move> ChargeMove { get; set; }

      public List<RaidCounter> Counter { get; set; }


      public string DetailsToString()
      {
         string str = "";
         str += $"Region           : {Region}\n";
         str += $"Category         : {Category}\n";
         str += $"Buddy Distance   : {BuddyDistance} km\n";
         str += $"Can be Obtained  : {Obtainable}\n";
         str += $"Can be Shiny     : {Shiny}\n";
         str += $"Can be Shadow    : {Shadow}\n";
         str += $"Is a Regional    : {Regional}\n";
         return str.Trim();
      }

      public string StatsToString()
      {
         string str = "";
         str += $"Max CP : {CPMax}\n";
         str += $"Attack : {Attack}\n";
         str += $"Defense: {Defense}\n";
         str += $"Stamina: {Stamina}\n";
         return str.Trim();
      }

      public string TypeToString()
      {
         string str = "";
         foreach (string type in Type)
            str += Emote.Parse(Environment.GetEnvironmentVariable($"{type.ToUpper()}_EMOTE")) + " ";
         return str.Trim();
      }

      public string WeatherToString()
      {
         string str = "";
         foreach (string weather in Weather)
            str += weather + "\n";
         return str.Trim();
      }

      public string WeaknessToString()
      {
         string str = "";
         foreach (string weakness in Weakness)
            str += Emote.Parse(Environment.GetEnvironmentVariable($"{weakness.ToUpper()}_EMOTE")) + " ";
         return str.Trim();
      }

      public string ResistanceToString()
      {
         string str = "";
         foreach (string resistance in Resistance)
            str += Emote.Parse(Environment.GetEnvironmentVariable($"{resistance.ToUpper()}_EMOTE")) + " ";
         return str.Trim();
      }

      public string FastMoveToString()
      {
         if (FastMove.Count == 0)
            return "No Fast Move Data";

         string str = "";
         foreach (Move fastMove in FastMove)
         {
            str += fastMove.ToString();
            if (Type.Contains(fastMove.Type))
               str += " *";
            str += "\n";
         }
         return str.Trim();
      }

      public string ChargeMoveToString()
      {
         if (ChargeMove.Count == 0)
            return "No Charge Move Data";

         string str = "";
         foreach (Move chargeMove in ChargeMove)
         {
            str += chargeMove.ToString();
            if (Type.Contains(chargeMove.Type))
               str += " *";
            str += "\n";
         }
         return str.Trim();
      }

      public string CounterToString()
      {
         /*
          * foreach (RaidCounter counter in Counter)
          *    str += counter.ToString() + "\n";
          * return str;
         /**/
         return "Not Implemented";
      }

      public string RaidCPToString()
      {
         return $"{CPRaidMin} - {CPRaidMax}\n{CPRaidBoostedMin}* - {CPRaidBoostedMax}*";
      }

      public string QuestCPToString()
      {
         return $"{CPQuestMin} - {CPQuestMax}";
      }

      public string HatchCPToString()
      {
         return $"{CPHatchMin} - {CPHatchMax}";
      }

      public string WildCPToString()
      {
         string str = "";
         for (int i = 0; i < CPWild.Count; i++)
         {
            str += $"{i + 1}";
            var temp = $"{i + 1}";

            for (int j = temp.Length; j < 20; j++)
               str += "-";
            str += $"{CPWild[i]}";

            if (i >= 30)
               str += "*";
            str += "\n";
         }
         return str;
      }


   }
}
