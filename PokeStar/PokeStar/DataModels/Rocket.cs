using System.Collections.Generic;

namespace PokeStar.DataModels
{
   public class Rocket
   {
      private string LEADER_TITLE = "Leader";
      private string GRUNT_TITLE = "Grunt";

      public string Name { get; private set; }

      public List<string>[] Slots { get; private set; } =
      {
         new List<string>(),
         new List<string>(),
         new List<string>()
      };

      public void SetLeader(string name)
      {
         Name = name.Replace(LEADER_TITLE, string.Empty).Replace(GRUNT_TITLE, string.Empty).Trim();
      }

      public void SetGrunt(string phrase, string type)
      {
         Name = $"{phrase} ({type} Grunt)";
      }
   }
}
