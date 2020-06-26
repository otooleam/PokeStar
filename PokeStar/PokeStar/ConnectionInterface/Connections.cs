using System;

namespace PokeStar.ConnectionInterface
{
   public static class Connections
   {
      private const Uri raid_boss_url = new Uri("https://thesilphroad.com/raid-bosses");
      private const string raid_boss_html = "//*[@class = 'col-md-4']";

      public static Uri RAID_BOSS_URL => raid_boss_url;
      public static string RAID_BOSS_HTML => raid_boss_html;
   }
}
