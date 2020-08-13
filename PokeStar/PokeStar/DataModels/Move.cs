using System;
using Discord;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Pokemon's move.
   /// </summary>
   public class Move
   {
      /// <summary>
      /// Name of the move.
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// Type of the move.
      /// </summary>
      public string Type { get; set; }

      /// <summary>
      /// If the move is a legacy move.
      /// </summary>
      public bool IsLegacy { get; set; }

      /// <summary>
      /// Gets the move to a string.
      /// </summary>
      /// <returns>Move string.</returns>
      public string ToString()
      {
         string typeString = Emote.Parse(Environment.GetEnvironmentVariable($"{Type.ToUpper()}_EMOTE")).ToString();
         string str = $@"{Name} {typeString}";
         if (IsLegacy)
            str += " !";
         return str;
      }
   }
}