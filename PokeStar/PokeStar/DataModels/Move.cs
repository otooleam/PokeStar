namespace PokeStar.DataModels
{
   public class Move
   {
      public string Name { get; set; }
      public string Type { get; set; }
      public bool IsLegacy { get; set; }

      public string ToString()
      {
         string str = $@"{Name} {Type}";
         if (IsLegacy)
            str += " !";
         return str;
      }

   }
}
