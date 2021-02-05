using System.Collections.Generic;

namespace PokeStar.DataModels
{
   /// <summary>
   /// 
   /// </summary>
   public class POI
   {
      public string Name { get; set; }

      public string Latitude { get; set; }

      public string Longitude { get; set; }

      public bool IsGym { get; set; }

      public bool IsSponsored { get; set; }

      public bool IsExGym { get; set; }

      public List<string> Nicknames { get; set; } = new List<string>();
   }
}
