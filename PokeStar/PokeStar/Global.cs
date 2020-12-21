using System;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;

namespace PokeStar
{
   public static class Global
   {
      /// <summary>
      /// é
      /// </summary>
      private const string POKEMON = "Pokémon";
      private const string POKEDEX = "PokéDex";

      /// <summary>
      /// File IO
      /// </summary>
      public static string PROGRAM_PATH { get; set; }
      public static JObject ENV_FILE { get; set; }

      /// <summary>
      /// Values read only from env
      /// </summary>
      public static string DEFAULT_PREFIX { get; set; }
      public static string VERSION { get; set; }
      public static string HOME_SERVER { get; set; }
      public static LogSeverity LOG_LEVEL { get; set; }
      public static string POGO_DB_CONNECTION_STRING { get; set; }
      public static string NONA_DB_CONNECTION_STRING { get; set; }

      /// <summary>
      /// Editable values from env file
      /// </summary>
      public static bool USE_NONA_TEST { get; set; }
      public static bool USE_EMPTY_RAID { get; set; }

      /// <summary>
      /// Values for hiding help commands
      /// </summary>
      public static List<CommandInfo> COMMAND_INFO { get; set; }
      public static readonly List<string> HIDDEN_COMMANDS = new List<string>()
      {
         "ping",
         "marco",
         "help",
         "rave",
         "screm",
      };

      public static readonly List<string> ADMIN_COMMANDS = new List<string>()
      {
         "status"
      };

      public static readonly List<string> NONA_ADMIN_COMMANDS = new List<string>()
      {
         "updatePokemonNames",
         "updateMovenNames",
         "toggleUseEmptyRaid",
         "toggleUseNonaTest",
         "updatePokemon",
         "updatePokemonMove",
         "updateEggList",
         "updateRocketList"
      };

      /// <summary>
      /// Embed Colors
      /// </summary>

      public static readonly Color EMBED_COLOR_INFO_RESPONSE      = Color.Purple;
      public static readonly Color EMBED_COLOR_WARNING_RESPONSE   = Color.Orange;
      public static readonly Color EMBED_COLOR_ERROR_RESPONSE     = Color.Red;
      public static readonly Color EMBED_COLOR_HELP_RESPONSE      = Color.Gold;
      public static readonly Color EMBED_COLOR_RAID_RESPONSE      = Color.Blue;
      public static readonly Color EMBED_COLOR_DEX_RESPONSE       = Color.Green;
      public static readonly Color EMBED_COLOR_GAME_INFO_RESPONSE = Color.Teal;

      /// <summary>
      /// Text formatting
      /// </summary>
      public static readonly char STAB_SYMBOL = '*';
      public static readonly char DEFAULT_FORM_SYMBOL = '*';
      public static readonly char WEATHER_BOOST_SYMBOL = '*';
      public static readonly char LEGACY_MOVE_SYMBOL = '!';
      public static readonly int WILD_CP_COLUMN_LENGTH = 12;
      public static readonly int EGG_ROW_LENGTH = 3;

      /// <summary>
      /// Move Category names
      /// </summary>
      public static readonly string FAST_MOVE_CATEGORY = "Fast";
      public static readonly string CHARGE_MOVE_CATEGORY = "Charge";

      /// <summary>
      /// Mega Pokémon delimeters
      /// </summary>
      public static readonly string MEGA_TAG = "mega";
      public static readonly int MAX_LEN_MEGA = 3;

      /// <summary>
      /// Raid Tiers
      /// </summary>
      public static readonly short EX_RAID_TIER        = 9;
      public static readonly short MEGA_RAID_TIER      = 7;
      public static readonly short LEGENDARY_RAID_TIER = 5;
      public static readonly short RARE_RAID_TIER      = 3;
      public static readonly short COMMON_RAID_TIER    = 1;
      public static readonly short INVALID_RAID_TIER   = 0;

      public static readonly string RAID_STRING_MEGA = "Mega";
      public static readonly string RAID_STRING_EX   = "EX";
      public static readonly string RAID_STRING_TIER = "Tier";

      public static readonly Dictionary<string, short> RAID_TIER_STRING = new Dictionary<string, short>(StringComparer.OrdinalIgnoreCase)
      {
         ["EX"]        = EX_RAID_TIER,
         ["E"]         = EX_RAID_TIER,
         ["9"]         = EX_RAID_TIER,
         ["MEGA"]      = MEGA_RAID_TIER,
         ["M"]         = MEGA_RAID_TIER,
         ["7"]         = MEGA_RAID_TIER,
         ["LEGENDARY"] = LEGENDARY_RAID_TIER,
         ["L"]         = LEGENDARY_RAID_TIER,
         ["5"]         = LEGENDARY_RAID_TIER,
         ["RARE"]      = RARE_RAID_TIER,
         ["R"]         = RARE_RAID_TIER,
         ["3"]         = RARE_RAID_TIER,
         ["COMMON"]    = COMMON_RAID_TIER,
         ["C"]         = COMMON_RAID_TIER,
         ["1"]         = COMMON_RAID_TIER,
      };

      /// <summary>
      /// Egg Tiers
      /// </summary>
      public static readonly short EGG_TIER_2KM     = 2;
      public static readonly short EGG_TIER_5KM     = 5;
      public static readonly short EGG_TIER_10KM    = 10;
      public static readonly short EGG_TIER_5AS     = 25;
      public static readonly short EGG_TIER_10AS    = 50;
      public static readonly short EGG_TIER_7KM     = 7;
      public static readonly short EGG_TIER_12KM    = 12;
      public static readonly short EGG_TIER_INVALID = 0;

      public static readonly string EGG_STRING_2KM  = "2KM Eggs";
      public static readonly string EGG_STRING_5KM  = "5KM Eggs";
      public static readonly string EGG_STRING_10KM = "10KM Eggs";
      public static readonly string EGG_STRING_5AS  = "5KM Eggs (25KM)";
      public static readonly string EGG_STRING_10AS = "10KM Eggs (50KM)";
      public static readonly string EGG_STRING_7KM  = "7KM Gift Eggs";
      public static readonly string EGG_STRING_12KM = "12KM Strange Eggs";

      public static readonly Dictionary<string, int> EGG_TIER_TITLE = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
      {
         [EGG_STRING_2KM]  = EGG_TIER_2KM,
         [EGG_STRING_5KM]  = EGG_TIER_5KM,
         [EGG_STRING_10KM] = EGG_TIER_10KM,
         [EGG_STRING_5AS]  = EGG_TIER_5AS,
         [EGG_STRING_10AS] = EGG_TIER_10AS,
         [EGG_STRING_7KM]  = EGG_TIER_7KM,
         [EGG_STRING_12KM] = EGG_TIER_12KM,
      };
      public static readonly Dictionary<string, short> EGG_TIER_STRING = new Dictionary<string, short>(StringComparer.OrdinalIgnoreCase)
      {
         ["2"]     = EGG_TIER_2KM,
         ["2 KM"]  = EGG_TIER_2KM,
         ["5"]     = EGG_TIER_5KM,
         ["5 KM"]  = EGG_TIER_5KM,
         ["10"]    = EGG_TIER_10KM,
         ["10 KM"] = EGG_TIER_10KM,
         ["25"]    = EGG_TIER_5AS,
         ["5 AS"]  = EGG_TIER_5AS,
         ["50"]    = EGG_TIER_10AS,
         ["10 AS"] = EGG_TIER_10AS,
         ["7"]     = EGG_TIER_7KM,
         ["7 KM"]  = EGG_TIER_7KM,
         ["12"]    = EGG_TIER_12KM,
         ["12 KM"] = EGG_TIER_12KM,
      };

      /// <summary>
      /// Raid/Mule limits
      /// </summary>
      public static readonly int LIMIT_RAID_PLAYER      = 20;
      public static readonly int LIMIT_RAID_INVITE      = 10;
      public static readonly int LIMIT_RAID_GROUP       = 3;
      public static readonly int LIMIT_RAID_MULE_MULE   = 2;
      public static readonly int LIMIT_RAID_MULE_INVITE = 5;
      public static readonly int LIMIT_RAID_MULE_GROUP  = 6;

      /// <summary>
      /// Level constants
      /// </summary>
      public static readonly double LEVEL_STEP  = 0.5;
      public static readonly int MAX_LEVEL      = 50;
      public static readonly int MAX_HALF_LEVEL = 40;
      public static readonly int MIN_WILD_LEVEL = 1;
      public static readonly int MAX_WILD_LEVEL = 35;
      public static readonly int RAID_LEVEL     = 20;
      public static readonly int HATCH_LEVEL    = 20;
      public static readonly int QUEST_LEVEL    = 15;
      public static readonly int WEATHER_BOOST  = 5;
      public static readonly int BUDDY_BOOST    = 1;

      /// <summary>
      /// IV constants
      /// </summary>
      public static readonly int MIN_SPECIAL_IV = 10;
      public static readonly int MAX_IV         = 15;

      /// <summary>
      /// League constants
      /// </summary>
      public static readonly int MAX_GREAT_CP = 1500;
      public static readonly int MAX_ULTRA_CP = 2500;

      /// <summary>
      /// Evolution tree
      /// </summary>
      public static readonly int BAD_EVOLUTION = -1;

      /// <summary>
      /// Pokemon numbers with many forms
      /// </summary>
      public static readonly int UNOWN_NUMBER  = 201;
      public static readonly int ARCEUS_NUMBER = 493;

      /// <summary>
      /// Raid Group bit values
      /// </summary>
      public static readonly int ATTEND_MASK  = 0b00000111; // 0000 0XXX mask will clear 4th bit
      public static readonly int REMOTE_MASK  = 0b01110000; // 0YYY 0000 mask will clear 8th bit
      public static readonly int REMOTE_SHIFT = 4;          // 0YYY 0XXX where Y is the remote value and X is the attending value 

      /// <summary>
      /// Bad raid values
      /// </summary>
      public static readonly int NO_ADD_VALUE = -1;
      public static readonly int NOT_IN_RAID  = -1;
      public static readonly string DEFAULT_RAID_BOSS_NAME = "Empty";
      public static readonly string EMPTY_FIELD = "-----";

      /// <summary>
      /// Nickname delimiter
      /// </summary>
      public static readonly char NICKNAME_DELIMITER = '>';
      public static readonly int NICKNAME_DELIMITER_MISSING = -1;

      /// <summary>
      /// Pokémon Move delimiter
      /// </summary>
      public static readonly char POKE_MOVE_DELIMITER = '>';
      public static readonly int POKE_MOVE_DELIMITER_MISSING = -1;

      /// <summary>
      /// Channel Register values
      /// </summary>
      public static readonly char REGISTER_STRING_DEX   = 'D';
      public static readonly char REGISTER_STRING_EX    = 'E';
      public static readonly char REGISTER_STRING_INFO  = 'I';
      public static readonly char REGISTER_STRING_ROLE  = 'P';
      public static readonly char REGISTER_STRING_RAID  = 'R';
      public static readonly char REGISTER_STRING_TRAIN = 'T';
      public static readonly string FULL_REGISTER_STRING = "DEIPRT";
      public static readonly Dictionary<string, string> REGISTER_STRING_TYPE = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
      {
         [REGISTER_STRING_DEX.ToString()]   = "PokéDex",
         [REGISTER_STRING_EX.ToString()]    = "EX Raid",
         [REGISTER_STRING_INFO.ToString()]  = "Information",
         [REGISTER_STRING_ROLE.ToString()]  = "Player Roles",
         [REGISTER_STRING_RAID.ToString()]  = "Raid",
         [REGISTER_STRING_TRAIN.ToString()] = "Raid Train",
      };
      public static readonly Dictionary<string, string> REGISTER_VALIE_STRING = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
      {
         [FULL_REGISTER_STRING] = FULL_REGISTER_STRING,
         ["POKEDEX"]            = REGISTER_STRING_DEX.ToString(),
         ["DEX"]                = REGISTER_STRING_DEX.ToString(),
         ["D"]                  = REGISTER_STRING_DEX.ToString(),
         ["EX"]                 = REGISTER_STRING_EX.ToString(),
         ["E"]                  = REGISTER_STRING_EX.ToString(),
         ["INFORMATION"]        = REGISTER_STRING_INFO.ToString(),
         ["INFO"]               = REGISTER_STRING_INFO.ToString(),
         ["I"]                  = REGISTER_STRING_INFO.ToString(),
         ["PLAYER"]             = REGISTER_STRING_ROLE.ToString(),
         ["ROLE"]               = REGISTER_STRING_ROLE.ToString(),
         ["P"]                  = REGISTER_STRING_ROLE.ToString(),
         ["RAID"]               = REGISTER_STRING_RAID.ToString(),
         ["R"]                  = REGISTER_STRING_RAID.ToString(),
         ["TRAIN"]              = REGISTER_STRING_TRAIN.ToString(),
         ["T"]                  = REGISTER_STRING_TRAIN.ToString()

      };

      /// <summary>
      /// Role names
      /// </summary>
      public static readonly string ROLE_TRAINER  = "Trainer";
      public static readonly string ROLE_VALOR    = "Valor";
      public static readonly string ROLE_MYSTIC   = "Mystic";
      public static readonly string ROLE_INSTINCT = "Instinct";

      /// <summary>
      /// Role indices
      /// </summary>
      public static readonly int ROLE_INDEX_NO_TEAM_FOUND = -1;
      public static readonly int ROLE_INDEX_VALOR         = 0;
      public static readonly int ROLE_INDEX_MYSTIC        = 1;
      public static readonly int ROLE_INDEX_INSTINCT      = 2;

      /// <summary>
      /// Role colors
      /// </summary>
      public static readonly Color ROLE_COLOR_TRAINER  = new Color(185, 187, 190);
      public static readonly Color ROLE_COLOR_VALOR    = new Color(153, 45, 34);
      public static readonly Color ROLE_COLOR_MYSTIC   = new Color(39, 126, 205);
      public static readonly Color ROLE_COLOR_INSTINCT = new Color(241, 196, 15);

      /// <summary>
      /// Image processing
      /// </summary>
      public static readonly int SCALE_WIDTH = 495;
      public static readonly int SCALE_HEIGHT = 880;
      public static readonly System.Drawing.Rectangle IMAGE_RECT_NICKNAME   = new System.Drawing.Rectangle(10, 120, 480, 100);
      public static readonly System.Drawing.Rectangle IMAGE_RECT_TEAM_COLOR = new System.Drawing.Rectangle(410, 60, 10, 10);

      /// <summary>
      /// Nona emojis
      /// </summary>
      public static readonly Dictionary<string, string> NONA_EMOJIS = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
      { 
         ["valor_emote"]         = "Badge_Team_Valor_01",
         ["mystic_emote"]        = "Badge_Team_Mystic_01",
         ["instinct_emote"]      = "Badge_Team_Instinct_01",

         ["raid_emote"]          = "Badge_Raid_01",
         ["ex_emote"]            = "Badge_EX_01",
         ["mega_emote"]          = "Badge_Mega_01",
         ["ex_pass_emote"]       = "Badge_EX_Pass_01",
         ["remote_pass_emote"]   = "Badge_Remote_Pass_01",

         ["bug_emote"]           = "Badge_Type_Bug_01",
         ["dark_emote"]          = "Badge_Type_Dark_01",
         ["dragon_emote"]        = "Badge_Type_Dragon_01",
         ["electric_emote"]      = "Badge_Type_Electric_01",
         ["fairy_emote"]         = "Badge_Type_Fairy_01",
         ["fighting_emote"]      = "Badge_Type_Fighting_01",
         ["fire_emote"]          = "Badge_Type_Fire_01",
         ["flying_emote"]        = "Badge_Type_Flying_01",
         ["ghost_emote"]         = "Badge_Type_Ghost_01",
         ["grass_emote"]         = "Badge_Type_Grass_01",
         ["ground_emote"]        = "Badge_Type_Ground_01",
         ["ice_emote"]           = "Badge_Type_Ice_01",
         ["normal_emote"]        = "Badge_Type_Normal_01",
         ["poison_emote"]        = "Badge_Type_Poison_01",
         ["psychic_emote"]       = "Badge_Type_Psychic_01",
         ["rock_emote"]          = "Badge_Type_Rock_01",
         ["steel_emote"]         = "Badge_Type_Steel_01",
         ["water_emote"]         = "Badge_Type_Water_01",

         ["sunny_emote"]         = "Badge_Weather_Sunny_01",
         ["clear_emote"]         = "Badge_Weather_Clear_01",
         ["rain_emote"]          = "Badge_Weather_Rain_01",
         ["partly_cloudy_emote"] = "Badge_Weather_Partly_Cloudy_01",
         ["cloudy_emote"]        = "Badge_Weather_Cloudy_01",
         ["windy_emote"]         = "Badge_Weather_Windy_01",
         ["snow_emote"]          = "Badge_Weather_Snow_01",
         ["fog_emote"]           = "Badge_Weather_Fog_01",

         ["rave_emote"]          = "ravewizard",
         ["scream_emote"]        = "rowletscrem",
      };

      /// <summary>
      /// Number Emojis
      /// </summary>
      public static readonly Dictionary<string, string> NUM_EMOJI_NAMES = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
      {
         ["number_1_emote"] = "number_1",
         ["number_2_emote"] = "number_2",
         ["number_3_emote"] = "number_3",
         ["number_4_emote"] = "number_4",
         ["number_5_emote"] = "number_5",
         ["number_6_emote"] = "number_6",
         ["number_7_emote"] = "number_7",
         ["number_8_emote"] = "number_8",
         ["number_9_emote"] = "number_9",
         ["number_10_emote"] = "number_10",
         ["number_11_emote"] = "number_11",
      };
      public static readonly int NUM_SELECTIONS = 10;
      public static readonly Emote[] SELECTION_EMOJIS = new Emote[NUM_SELECTIONS];
      public static readonly List<Emote> NUM_EMOJIS = new List<Emote>();
   }
}
