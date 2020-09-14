using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using PokeStar.Modules;
using PokeStar.ImageProcessors;
using PokeStar.ConnectionInterface;
using PokeStar.ModuleParents;

namespace PokeStar
{
   /// <summary>
   /// Runs the main thread of the system.
   /// </summary>
   public class Program
   {
      private static DiscordSocketClient client;
      private static CommandService commands;
      private static IServiceProvider services;

      private bool loggingInProgress = false;

      private readonly int SizeMessageCashe = 100;
      private readonly LogSeverity DefaultLogLevel = LogSeverity.Info;

      /// <summary>
      /// Main function for the system.
      /// Allows the system to run asyncronously.
      /// </summary>
      public static void Main()
         => new Program().MainAsync().GetAwaiter().GetResult();

      /// <summary>
      /// Runs main thread for the function.
      /// Task is blocked until the program is closed.
      /// </summary>
      /// <returns>No task is returned as function ends on system termination.</returns>
      public async Task MainAsync()
      {
         Global.PROGRAM_PATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         Global.ENV_FILE = JObject.Parse(File.ReadAllText($"{Global.PROGRAM_PATH}\\env.json"));

         string token = Global.ENV_FILE.GetValue("token").ToString();
         Global.VERSION = Global.ENV_FILE.GetValue("version").ToString();
         Global.HOME_SERVER = Global.ENV_FILE.GetValue("home_server").ToString();
         Global.POGODB_CONNECTION_STRING = Global.ENV_FILE.GetValue("pogo_db_sql").ToString();
         Global.NONADB_CONNECTION_STRING = Global.ENV_FILE.GetValue("nona_db_sql").ToString();
         Global.DEFAULT_PREFIX = Global.ENV_FILE.GetValue("default_prefix").ToString();

         Global.USE_NONA_TEST = Global.ENV_FILE.GetValue("accept_nona_test").ToString().Equals("TRUE", StringComparison.OrdinalIgnoreCase);
         Global.USE_EMPTY_RAID = Global.ENV_FILE.GetValue("use_empty_raid").ToString().Equals("TRUE", StringComparison.OrdinalIgnoreCase);

         int logLevel = Convert.ToInt32(Global.ENV_FILE.GetValue("log_level").ToString());
         Global.LOG_LEVEL = !Enum.IsDefined(typeof(LogSeverity), logLevel) ? DefaultLogLevel : (LogSeverity)logLevel;

         DiscordSocketConfig clientConfig = new DiscordSocketConfig
         {
            MessageCacheSize = SizeMessageCashe,
            LogLevel = Global.LOG_LEVEL,
            ExclusiveBulkDelete = true
         };
         client = new DiscordSocketClient(clientConfig);
         CommandServiceConfig commandConfig = new CommandServiceConfig
         {
            DefaultRunMode = RunMode.Async,
            LogLevel = Global.LOG_LEVEL
         };
         commands = new CommandService(commandConfig);

         services = new ServiceCollection()
             .AddSingleton(client)
             .AddSingleton(commands)
             .BuildServiceProvider();

         await HookEvents();
         await client.LoginAsync(TokenType.Bot, token);
         await client.StartAsync();
         await client.SetGameAsync($".help | v{Global.VERSION}");

         Global.COMMAND_INFO = commands.Commands.ToList();

         // Block this task until the program is closed.
         await Task.Delay(-1);
      }

      /// <summary>
      /// Hooks events to the Client and Command services.
      /// Runs asyncronously.
      /// </summary>
      /// <returns>Task Complete.</returns>
      private async Task<Task> HookEvents()
      {
         client.Log += Log;
         commands.Log += Log;
         client.MessageReceived += HandleCommandAsync;
         await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
         client.ReactionAdded += HandleReactionAddedAsync;
         client.Ready += HandleReady;
         client.JoinedGuild += HandleJoinGuild;
         client.LeftGuild += HandleLeftGuild;
         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Log event.
      /// </summary>
      /// <param name="msg">Message to log.</param>
      /// <returns>Task Complete.</returns>
      private Task Log(LogMessage msg)
      {
         string fileName = DateTime.Now.ToString("MM-dd-yyyy");
         string logFile = $"{Global.PROGRAM_PATH}\\Logs\\{fileName}.txt";

         string logText = $"{DateTime.Now:hh:mm:ss} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";

         while (loggingInProgress) { }

         loggingInProgress = true;
         File.AppendAllText(logFile, logText + "\n");
         loggingInProgress = false;

         Console.WriteLine(msg.ToString());

         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Command event.
      /// Runs asyncronously.
      /// </summary>
      /// <param name="cmdMessage">Command message that was sent.</param>
      /// <returns>Task Complete.</returns>
      private async Task<Task> HandleCommandAsync(SocketMessage cmdMessage)
      {
         SocketUserMessage message = cmdMessage as SocketUserMessage;
         if (message == null || (message.Author.IsBot && (!Global.USE_NONA_TEST || !message.Author.Username.Equals("NonaTest", StringComparison.OrdinalIgnoreCase))))
         {
            return Task.CompletedTask;
         }
         SocketCommandContext context = new SocketCommandContext(client, message);

         int argPos = 0;
         string prefix = Connections.Instance().GetPrefix(context.Guild.Id);

         if (message.Attachments.Count != 0)
         {
            if (ChannelRegisterCommands.IsRegisteredChannel(context.Guild.Id, context.Channel.Id, Global.REGISTER_STRING_ROLE))
            {
               RollImageProcess.RoleImageProcess(context);
            }
            else if (ChannelRegisterCommands.IsRegisteredChannel(context.Guild.Id, context.Channel.Id, Global.REGISTER_STRING_RAID))
            {
               //TODO: Add call for raid image processing
            }
            else if (ChannelRegisterCommands.IsRegisteredChannel(context.Guild.Id, context.Channel.Id, Global.REGISTER_STRING_EX))
            {
               //TODO: Add call for ex raid image processing
            }
         }
         else if (message.HasStringPrefix(prefix, ref argPos))
         {
            IResult result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
         }
         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Reaction Added event.
      /// Runs asyncronously.
      /// </summary>
      /// <param name="cachedMessage">Message that was reaction is on.</param>
      /// <param name="originChannel">Channel where the message is located.</param>
      /// <param name="reaction">Reaction made on the message.</param>
      /// <returns>Task Complete.</returns>
      private async Task<Task> HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedMessage,
          ISocketMessageChannel originChannel, SocketReaction reaction)
      {
         IUserMessage message = await cachedMessage.GetOrDownloadAsync();

         var chnl = message.Channel as SocketGuildChannel;
         var guild = chnl.Guild.Id;

         IUser user = reaction.User.Value;
         if (message != null && reaction.User.IsSpecified && !user.IsBot)
         {
            if (RaidCommandParent.IsRaidMessage(message.Id))
            {
               await RaidCommandParent.RaidMessageReactionHandle(message, reaction);
            }
            else if (RaidCommandParent.IsRaidSubMessage(message.Id))
            {
               await RaidCommandParent.RaidSubMessageReactionHandle(message, reaction);
            }
            else if (DexCommandParent.IsDexSubMessage(message.Id))
            {
               await DexCommandParent.DexMessageReactionHandle(message, reaction, guild);
            }
         }
         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Ready event.
      /// </summary>
      /// <returns>Task Complete.</returns>
      private Task HandleReady()
      {
         SocketGuild server = client.Guilds.FirstOrDefault(x => x.Name.ToString().Equals(Global.HOME_SERVER, StringComparison.OrdinalIgnoreCase));
         SetEmotes(server);
         RaidCommandParent.SetRaidEmotes();

         foreach (SocketGuild guild in client.Guilds)
         {
            if (Connections.Instance().GetPrefix(guild.Id) == null)
            {
               Connections.Instance().InitSettings(guild.Id);
            }
         }

         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Join Guild event.
      /// </summary>
      /// <param name="guild">Guild that the bot joined.</param>
      /// <returns>Task Complete.</returns>
      private Task HandleJoinGuild(SocketGuild guild)
      {
         Connections.Instance().InitSettings(guild.Id);
         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Left Guild event.
      /// </summary>
      /// <param name="guild">Guild that the bot left.</param>
      /// <returns>Task Complete.</returns>
      private Task HandleLeftGuild(SocketGuild guild)
      {
         Connections.Instance().DeleteRegistration(guild.Id);
         Connections.Instance().DeleteSettings(guild.Id);
         return Task.CompletedTask;
      }

      /// <summary>
      /// Sets the emotes from a JSON file
      /// </summary>
      /// <param name="server">Server that the emotes are on.</param>
      private void SetEmotes(SocketGuild server)
      {
         foreach (string emote in Global.EMOTE_NAMES)
         {
            Global.NONA_EMOJIS[emote] = Emote.Parse(
               server.Emotes.FirstOrDefault(
                  x => x.Name.Equals(
                     Global.ENV_FILE.GetValue(emote).ToString(),
                     StringComparison.OrdinalIgnoreCase)
                  ).ToString()).ToString();
         }
         
         foreach (string emote in Global.NUM_EMOJI_NAMES)
         {
            Global.NUM_EMOJIS.Add(Emote.Parse(
               server.Emotes.FirstOrDefault(
                  x => x.Name.Equals(
                     Global.ENV_FILE.GetValue(emote).ToString(),
                     StringComparison.OrdinalIgnoreCase)
                  ).ToString()));
         }

         for (int i = 1; i <= Global.NUM_SELECTIONS; i++)
         {
            Global.SELECTION_EMOJIS[i - 1] = Global.NUM_EMOJIS[i];
         }
      }

      public static string GetStatus()
      {
         return client.Status.ToString();
      }
      public static string GetConnectionState()
      {
         return client.ConnectionState.ToString();
      }
      public static int GetGuildCount()
      {
         return client.Guilds.Count;
      }
      public static string GetName()
      {
         return client.CurrentUser.Username;
      }
      public static int GetLatency()
      {
         return client.Latency;
      }
   }
}