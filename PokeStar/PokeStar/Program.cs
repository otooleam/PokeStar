﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using PokeStar.Modules;
using PokeStar.ModuleParents;
using PokeStar.ImageProcessors;
using PokeStar.ConnectionInterface;

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
      
      private readonly int SizeMessageCashe = 100;
      private readonly LogSeverity DefaultLogLevel = LogSeverity.Info;

      private bool loggingInProgress;

      private static Timer SilphUpdate;

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
         Global.EMOTE_SERVER = Global.ENV_FILE.GetValue("emote_server").ToString();
         Global.POGO_DB_CONNECTION_STRING = Global.ENV_FILE.GetValue("pogo_db_sql").ToString();
         Global.NONA_DB_CONNECTION_STRING = Global.ENV_FILE.GetValue("nona_db_sql").ToString();
         Global.DEFAULT_PREFIX = Global.ENV_FILE.GetValue("default_prefix").ToString();

         Global.USE_NONA_TEST = Global.ENV_FILE.GetValue("accept_nona_test").ToString().Equals("TRUE", StringComparison.OrdinalIgnoreCase);
         Global.USE_EMPTY_RAID = Global.ENV_FILE.GetValue("use_empty_raid").ToString().Equals("TRUE", StringComparison.OrdinalIgnoreCase);

         int logLevel = Convert.ToInt32(Global.ENV_FILE.GetValue("log_level").ToString());
         Global.LOG_LEVEL = !Enum.IsDefined(typeof(LogSeverity), logLevel) ? DefaultLogLevel : (LogSeverity)logLevel;

         DiscordSocketConfig clientConfig = new DiscordSocketConfig
         {
            MessageCacheSize = SizeMessageCashe,
            LogLevel = Global.LOG_LEVEL
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

         Global.COMMAND_INFO = commands.Commands.ToList().OrderBy(c => c.Name).ToList();

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
         client.ReactionAdded += HandleReactionAdded;
         client.ReactionRemoved += HandleReactionRemoved;
         client.Ready += HandleReady;
         client.JoinedGuild += HandleJoinGuild;
         client.LeftGuild += HandleLeftGuild;
         SilphUpdate = new Timer(async _ => await Connections.Instance().RunSilphUpdate(client.Guilds.ToList()), new AutoResetEvent(false), 0, 300000);
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
         if (!(cmdMessage is SocketUserMessage message) || 
             (message.Author.IsBot && 
             (!Global.USE_NONA_TEST || !message.Author.Username.Equals("NonaTest", StringComparison.OrdinalIgnoreCase)))
             || cmdMessage.Channel is IPrivateChannel)
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
      /// </summary>
      /// <param name="cachedMessage">Message that was reaction is on.</param>
      /// <param name="originChannel">Channel where the message is located.</param>
      /// <param name="reaction">Reaction made on the message.</param>
      /// <returns>Task Complete.</returns>
      private async Task<Task> HandleReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage,
          Cacheable<IMessageChannel, ulong> originChannel, SocketReaction reaction)
      {
         IMessage message = await reaction.Channel.GetMessageAsync(cachedMessage.Id);

         SocketGuildChannel chnl = message.Channel as SocketGuildChannel;
         ulong guild = chnl.Guild.Id;

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
            else if (RaidCommandParent.IsRaidGuideMessage(message.Id))
            {
               await RaidCommandParent.RaidGuideMessageReactionHandle(message, reaction);
            }
            else if (DexCommandParent.IsDexSelectMessage(message.Id))
            {
               await DexCommandParent.DexSelectMessageReactionHandle(message, reaction, guild);
            }
            else if (DexCommandParent.IsDexMessage(message.Id))
            {
               await DexCommandParent.DexMessageReactionHandle(message, reaction, guild);
            }
            else if (DexCommandParent.IsCatchMessage(message.Id))
            {
               await DexCommandParent.CatchMessageReactionHandle(message, reaction);
            }
            else if (POICommands.IsPOISubMessage(message.Id))
            {
               await POICommands.POIMessageReactionHandle(message, reaction, guild);
            }
            else if (HelpCommands.IsHelpMessage(message.Id))
            {
               await HelpCommands.HelpMessageReactionHandle(message, reaction, guild);
            }
            else if (Connections.IsNotifyMessage(message.Id))
            {
               await Connections.NotifyMessageReactionAddedHandle(reaction, chnl.Guild);
            }
         }
         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Reaction Removed event.
      /// </summary>
      /// <param name="cachedMessage">Message that was reaction is on.</param>
      /// <param name="originChannel">Channel where the message is located.</param>
      /// <param name="reaction">Reaction made on the message.</param>
      /// <returns>Task Complete.</returns>
      private async Task<Task> HandleReactionRemoved(Cacheable<IUserMessage, ulong> cachedMessage,
          Cacheable<IMessageChannel, ulong> originChannel, SocketReaction reaction)
      {
         IMessage message = await reaction.Channel.GetMessageAsync(cachedMessage.Id);

         SocketGuildChannel chnl = message.Channel as SocketGuildChannel;
         ulong guild = chnl.Guild.Id;

         IUser user = reaction.User.Value;


         if (message != null && reaction.User.IsSpecified && !user.IsBot)
         {
            if (Connections.IsNotifyMessage(message.Id))
            {
               await Connections.NotifyMessageReactionRemovedHandle(reaction, chnl.Guild);
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
         RaidCommandParent.SetInitialEmotes();
         DexCommandParent.SetInitialEmotes();

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
      private static void SetEmotes(SocketGuild server)
      {
         List<string> nonaEmojiKeys = Global.NONA_EMOJIS.Keys.ToList();
         foreach (string emote in nonaEmojiKeys)
         {
            Global.NONA_EMOJIS[emote] = Emote.Parse(
               server.Emotes.FirstOrDefault(
                  x => x.Name.Equals(
                     Global.NONA_EMOJIS[emote],
                     StringComparison.OrdinalIgnoreCase)
                  ).ToString()).ToString();
         }

         List<string> noumEmojiKeys = Global.NUM_EMOJI_NAMES.Keys.ToList();
         foreach (string emote in noumEmojiKeys)
         {
            Global.NUM_EMOJIS.Add(Emote.Parse(
               server.Emotes.FirstOrDefault(
                  x => x.Name.Equals(
                     Global.NUM_EMOJI_NAMES[emote], 
                     StringComparison.OrdinalIgnoreCase)
                  ).ToString()));
         }

         for (int i = 0; i < Global.NUM_SELECTIONS; i++)
         {
            Global.SELECTION_EMOJIS[i] = Global.NUM_EMOJIS[i];
         }
      }

      /// <summary>
      /// Get the name of the bot.
      /// </summary>
      /// <returns>Bot name as a string.</returns>
      public static string GetName()
      {
         return client.CurrentUser.Username;
      }

      /// <summary>
      /// Get the status of the bot.
      /// </summary>
      /// <returns>Bot status as a string.</returns>
      public static string GetStatus()
      {
         return client.Status.ToString();
      }

      /// <summary>
      /// Get the connection status of the bot.
      /// </summary>
      /// <returns>Bot connection state as a string.</returns>
      public static string GetConnectionState()
      {
         return client.ConnectionState.ToString();
      }

      /// <summary>
      /// Get the number of guilds the bot is in.
      /// </summary>
      /// <returns>Number of guilds the bot is in.</returns>
      public static int GetGuildCount()
      {
         return client.Guilds.Count;
      }

      /// <summary>
      /// Get the latency of the bot and the server in milliseconds (ms).
      /// </summary>
      /// <returns>Latency of the bot and server.</returns>
      public static int GetLatency()
      {
         return client.Latency;
      }
   }
}