
namespace PokeStar.DataModels
{
   /// <summary>
   /// Pokémon's move.
   /// </summary>
   public class PokemonMove
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
      /// Gets the move as a string.
      /// </summary>
      /// <returns>Move string.</returns>
      public override string ToString()
      {
         string str = $@"{Name} {Global.NONA_EMOJIS[$"{Type}_emote"]}";
         if (IsLegacy)
         {
            str += $" {Global.LEGACY_MOVE_SYMBOL}";
         }
         return str;
      }
   }
}