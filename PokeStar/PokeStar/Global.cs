using System;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;

namespace PokeStar
{
   public static class Global
   {
      public static string PROGRAM_PATH { get; set; }
      public static JObject ENV_FILE { get; set; }

      public static string DEFAULT_PREFIX { get; set; }
      public static string VERSION { get; set; }
      public static string HOME_SERVER { get; set; }
      public static LogSeverity LOG_LEVEL { get; set; }
      public static string POGODB_CONNECTION_STRING { get; set; }
      public static string NONADB_CONNECTION_STRING { get; set; }

      public static bool USE_NONA_TEST { get; set; }
      public static bool USE_EMPTY_RAID { get; set; }

      public static List<CommandInfo> COMMAND_INFO { get; set; }
      public static readonly string ADMIN_KEY = "Hidden";
      public static readonly List<string> HIDDEN_COMMANDS = new List<string>()
      {
         "ping",
         "status",
         "help",
         "rave",
         "screm",
         "updatePokemonNames",
         "useEmptyRaid",
         "useNonaTest",
         "toggleUseEmptyRaid",
         "toggleUseNonaTest"
      };

      public static readonly int LIMIT_RAID             = 20;
      public static readonly int LIMIT_RAID_INVITE      = 10;
      public static readonly int LIMIT_RAID_GROUP       = 3;
      public static readonly int LIMIT_RAID_MULE_MULE   = 2;
      public static readonly int LIMIT_RAID_MULE_INVITE = 5;
      public static readonly int LIMIT_RAID_MULE_GROUP  = 6;

      public static readonly int MAX_LEVEL      = 40;
      public static readonly int MIN_WILD_LEVEL = 1;
      public static readonly int MAX_WILD_LEVEL = 35;
      public static readonly int RAID_LEVEL     = 20;
      public static readonly int HATCH_LEVEL    = 20;
      public static readonly int QUEST_LEVEL    = 15;
      public static readonly int WEATHER_BOOST  = 5;
      public static readonly int BUDDY_BOOST    = 1;

      public static readonly int MIN_SPECIAL_IV = 10;
      public static readonly int MAX_IV         = 15;

      public static readonly int UNOWN_NUMBER  = 201;
      public static readonly int ARCEUS_NUMBER = 493;

      public static readonly int ATTEND_MASK  = 7;   // 0000 0XXX mask will clear 4th bit
      public static readonly int REMOTE_MASK  = 112; // 0YYY 0000 mask will clear 8th bit
      public static readonly int REMOTE_SHIFT = 4;  // 0YYY 0XXX where Y is the remote value and X is the attending value 

      public static readonly int NOT_IN_RAID = -1;
      public static readonly string DEFAULT_RAID_BOSS_NAME = "Empty";

      public static readonly string EMPTY_FIELD = "-----";

      public static readonly char REGISTER_STRING_ROLE  = 'P';
      public static readonly char REGISTER_STRING_RAID  = 'R';
      public static readonly char REGISTER_STRING_EX    = 'E';
      public static readonly char REGISTER_STRING_TRAIN = 'T';
      public static readonly char REGISTER_STRING_DEX   = 'D';

      public static readonly string ROLE_TRAINER  = "Trainer";
      public static readonly string ROLE_VALOR    = "Valor";
      public static readonly string ROLE_MYSTIC   = "Mystic";
      public static readonly string ROLE_INSTINCT = "Instinct";

      public static readonly int ROLE_INDEX_NO_TEAM_FOUND = -1;
      public static readonly int ROLE_INDEX_VALOR         = 0;
      public static readonly int ROLE_INDEX_MYSTIC        = 1;
      public static readonly int ROLE_INDEX_INSTINCT      = 2;

      public static readonly Color ROLE_COLOR_TRAINER  = new Color(185, 187, 190);
      public static readonly Color ROLE_COLOR_VALOR    = new Color(153, 45, 34);
      public static readonly Color ROLE_COLOR_MYSTIC   = new Color(39, 126, 205);
      public static readonly Color ROLE_COLOR_INSTINCT = new Color(241, 196, 15);

      public static readonly System.Drawing.Rectangle IMAGE_RECT_NICKNAME   = new System.Drawing.Rectangle(10, 120, 480, 100);
      public static readonly System.Drawing.Rectangle IMAGE_RECT_TEAM_COLOR = new System.Drawing.Rectangle(410, 60, 10, 10);

      public static readonly Emoji[] SELECTION_EMOJIS = {
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
         new Emoji("5️⃣"),
         new Emoji("6️⃣"),
         new Emoji("7️⃣"),
         new Emoji("8️⃣"),
         new Emoji("9️⃣"),
         new Emoji("🔟")
      };

      public static readonly string[] EMOTE_NAMES = {
         "valor_emote", "mystic_emote", "instinct_emote",
         "raid_emote", "ex_emote", "mega_emote", 
         "ex_pass_emote", "remote_pass_emote",

         "bug_emote", "dark_emote", "dragon_emote",
         "electric_emote", "fairy_emote", "fighting_emote",
         "fire_emote", "flying_emote", "ghost_emote",
         "grass_emote", "ground_emote", "ice_emote",
         "normal_emote", "poison_emote", "psychic_emote",
         "rock_emote", "steel_emote", "water_emote",

         "sunny_emote", "clear_emote", "rain_emote",
         "partly_cloudy_emote", "cloudy_emote",
         "windy_emote", "snow_emote", "fog_emote",

         "rave_emote", "scream_emote"
      };

      public static readonly Dictionary<string, string> NONA_EMOJIS = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
   }
}
