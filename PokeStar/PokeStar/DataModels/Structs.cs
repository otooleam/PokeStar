using System.Collections.Generic;
using Discord.Commands;
using Discord.WebSocket;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Remove from raid result.
   /// </summary>
   public struct RaidRemoveResult
   {
      /// <summary>
      /// Group the user was removed from.
      /// </summary>
      public int Group { get; }

      /// <summary>
      /// Users that were invited by the removed user.
      /// </summary>
      public List<SocketGuildUser> Users { get; }

      /// <summary>
      /// Creates a new RaidRemoveResult.
      /// </summary>
      /// <param name="group">Group the user was removed from.</param>
      /// <param name="users">Users that were invited by the removed user.</param>
      public RaidRemoveResult(int group, List<SocketGuildUser> users)
      {
         Group = group;
         Users = users;
      }
   }

   /// <summary>
   /// Location in a raid train.
   /// </summary>
   public struct RaidTrainLoc
   {
      /// <summary>
      /// Time the raid starts
      /// </summary>
      public string Time { get; }
      /// <summary>
      /// Location of the raid.
      /// </summary>
      public string Location { get; }

      /// <summary>
      /// Boss that is at the location.
      /// </summary>
      public string BossName { get; set; }

      /// <summary>
      /// Creates a new RaidTrainLoc.
      /// </summary>
      /// <param name="time">Time the raid starts.</param>
      /// <param name="location">Location of the raid.</param>
      /// <param name="bossName">Boss that is at the location.</param>
      public RaidTrainLoc(string time, string location, string bossName)
      {
         Time = time;
         Location = location;
         BossName = bossName;
      }
   }

   /// <summary>
   /// Relation between types.
   /// </summary>
   public struct TypeRelation
   {
      /// <summary>
      /// Types the type is strong against.
      /// </summary>
      public Dictionary<string, int> Strong { get; }

      /// <summary>
      /// Types the type is weak against.
      /// </summary>
      public Dictionary<string, int> Weak { get; }

      /// <summary>
      /// Creates a new TypeRelation.
      /// </summary>
      /// <param name="strong">Types the type is strong against.</param>
      /// <param name="weak">Types the type is weak against.</param>
      public TypeRelation(Dictionary<string, int> strong, Dictionary<string, int> weak)
      {
         Strong = strong;
         Weak = weak;
      }
   }

   /// <summary>
   /// Chammel registration result.
   /// </summary>
   public struct RegisterResult
   {
      /// <summary>
      /// Current registration string.
      /// </summary>
      public string RegistrationString { get; }

      /// <summary>
      /// Should setup be checked for completion.
      /// </summary>
      public bool CheckSetupComplete { get; }

      /// <summary>
      /// Creates a new RegisterResult.
      /// </summary>
      /// <param name="registration">Current registration string.</param>
      /// <param name="checkSetupComplete">Should setup be checked for completion.</param>
      public RegisterResult(string registration, bool checkSetupComplete)
      {
         RegistrationString = registration;
         CheckSetupComplete = checkSetupComplete;
      }
   }

   /// <summary>
   /// Pokémon forms.
   /// </summary>
   public struct Form
   {
      /// <summary>
      /// List of all form tags.
      /// </summary>
      public List<string> FormList { get; }

      /// <summary>
      /// Default form tag.
      /// Empty string if no default form.
      /// </summary>
      public string DefaultForm { get; }

      /// <summary>
      /// Creates a new Form.
      /// </summary>
      /// <param name="fromList"></param>
      /// <param name="defaultForm"></param>
      public Form(List<string> fromList, string defaultForm)
      {
         FormList = fromList;
         DefaultForm = defaultForm;
      }
   }

   /// <summary>
   /// Raid sub message.
   /// </summary>
   public struct RaidSubMessage
   {
      /// <summary>
      /// Type of sub message.
      /// </summary>
      public int Type { get; }

      /// <summary>
      /// Base message Id.
      /// </summary>
      public ulong MainMessageId { get; }

      /// <summary>
      /// Creates a new RaidSubMessage.
      /// </summary>
      /// <param name="type">Type of sub message.</param>
      /// <param name="mainMessageId">Base message Id.</param>
      public RaidSubMessage(int type, ulong mainMessageId)
      {
         Type = type;
         MainMessageId = mainMessageId;
      }
   }

   /// <summary>
   /// Dex selection message.
   /// </summary>
   public struct DexSelectionMessage
   {
      /// <summary>
      /// Type of selection message.
      /// </summary>
      public int Type { get; }

      /// <summary>
      /// Possible selections.
      /// </summary>
      public List<string> Selections { get; }

      /// <summary>
      /// Creates a new DexSelectionMessage.
      /// </summary>
      /// <param name="type">Type of selection message.</param>
      /// <param name="selections">Possible selections.</param>
      public DexSelectionMessage(int type, List<string> selections)
      {
         Type = type;
         Selections = selections;
      }
   }

   /// <summary>
   /// Help message.
   /// </summary>
   public struct HelpMessage
   {
      /// <summary>
      /// Page the message is on. 0 based.
      /// </summary>
      public int Page { get; set; }

      /// <summary>
      /// All commands that can be shown.
      /// </summary>
      public List<CommandInfo> Commands { get; }

      /// <summary>
      /// Creates a new HelpMessage.
      /// </summary>
      /// <param name="commands">All commands that can be shown.</param>
      /// <param name="page">Current Page. Defaults to 0.</param>
      public HelpMessage(List<CommandInfo> commands, int page = 0)
      {
         Page = page;
         Commands = commands;
      }
   }
}