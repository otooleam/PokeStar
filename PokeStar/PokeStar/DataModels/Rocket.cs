using System.Collections.Generic;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Rocket boss.
   /// </summary>
   public class Rocket
   {
      /// <summary>
      /// 
      /// </summary>
      private readonly string LEADER_TITLE = "Leader";
      private readonly string GRUNT_TITLE = "Grunt";

      /// <summary>
      /// Name of the rocket.
      /// </summary>
      public string Name { get; private set; }

      /// <summary>
      /// Phrase said by the rocket.
      /// Only set for grunts.
      /// </summary>
      public string Phrase { get; private set; } = null;

      /// <summary>
      /// Pokémon used by rockets.
      /// Length is always 3.
      /// </summary>
      public List<string>[] Slots { get; private set; } =
      {
         new List<string>(),
         new List<string>(),
         new List<string>()
      };

      /// <summary>
      /// Sets the name for a rocket leader.
      /// </summary>
      /// <param name="name">Name of the rocket leader.</param>
      public void SetLeader(string name)
      {
         Name = name.Replace(LEADER_TITLE, string.Empty).Replace(GRUNT_TITLE, string.Empty).Trim();
      }

      /// <summary>
      /// Sets the name and phrase for a rocket grunt.
      /// </summary>
      /// <param name="type">Type of rocket grunt.</param>
      /// <param name="phrase">Phrase said by the rocket grunt.</param>
      public void SetGrunt(string type, string phrase)
      {
         Name = $"{type} {GRUNT_TITLE}";
         Phrase = phrase;
      }
   }
}
