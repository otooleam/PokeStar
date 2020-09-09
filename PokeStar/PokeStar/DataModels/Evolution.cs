using System;
using System.Text;

namespace PokeStar.DataModels
{
   public class Evolution : IEquatable<Evolution>
   {
      public string Start { get; set; }

      public string End { get; set; }

      public int Candy { get; set; }

      public string Item { get; set; }

      public string OtherMethod { get; set; } = null;

      public bool Equals(Evolution evo)
      {
         return evo != null && evo.Start.Equals(Start, StringComparison.OrdinalIgnoreCase) && evo.End.Equals(End, StringComparison.OrdinalIgnoreCase) && evo.Candy == Candy;
      }

      public override bool Equals(object obj)
      {
         return obj != null && Equals(obj as Evolution);
      }

      public string EvolutionMethod()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append($"{Candy}");

         if (Item != null)
         {
            if (Item.Equals("mega", StringComparison.OrdinalIgnoreCase))
            {
               sb.Append(" mega candy");
            }
            else
            {
               sb.Append($" candy and {Item}");
            }
         }
         else
         {
            sb.Append(" candy");
         }
         if (OtherMethod != null)
         {
            sb.Append($" or {OtherMethod}");
         }
         return sb.ToString();
      }

      public void Combine(Evolution evo)
      {
         if (evo.Start.Equals(Start, StringComparison.OrdinalIgnoreCase) &&
             evo.End.Equals(End, StringComparison.OrdinalIgnoreCase) &&
             Candy != 0 && evo.Candy == 0)
         {
            OtherMethod = evo.Item;
            evo.Candy = Global.BAD_EVOLUTION;
         }
      }
   }
}