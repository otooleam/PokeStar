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

namespace PokeStar
{
   /// <summary>
   /// Runs the main thread of the system.
   /// </summary>
   public class Program
   {
      private static DiscordSocketClient _client;
      private static CommandService _commands;
      private static IServiceProvider _services;

      private bool logging = false;

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
         string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         var json = JObject.Parse(File.ReadAllText($"{path}\\env.json"));

         var token = json.GetValue("token").ToString();
         Environment.SetEnvironmentVariable("POGO_DB_CONNECTION_STRING", json.GetValue("pogo_db_sql").ToString());
         Environment.SetEnvironmentVariable("NONA_DB_CONNECTION_STRING", json.GetValue("nona_db_sql").ToString());
         Environment.SetEnvironmentVariable("DEFAULT_PREFIX", json.GetValue("default_prefix").ToString());

         Environment.SetEnvironmentVariable("SETUP_COMPLETE", "FALSE");

         var logLevel = Convert.ToInt32(json.GetValue("log_level").ToString());
         var logSeverity = !Enum.IsDefined(typeof(LogSeverity), logLevel) ? LogSeverity.Info : (LogSeverity)logLevel;

         var _config = new DiscordSocketConfig
         {
            MessageCacheSize = 100,
            LogLevel = logSeverity
         };
         _client = new DiscordSocketClient(_config);
         CommandServiceConfig config = new CommandServiceConfig
         {
            DefaultRunMode = RunMode.Async,
            LogLevel = logSeverity
         };
         _commands = new CommandService(config);

         _services = new ServiceCollection()
             .AddSingleton(_client)
             .AddSingleton(_commands)
             .BuildServiceProvider();

         await HookEvents();
         await _client.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
         await _client.StartAsync().ConfigureAwait(false);

         string version = json.GetValue("version").ToString();
         await _client.SetGameAsync($".help | v{version}");

         // Block this task until the program is closed.
         await Task.Delay(-1).ConfigureAwait(false);
      }

      /// <summary>
      /// Hooks events to the Client and Command services.
      /// Runs asyncronously.
      /// </summary>
      /// <returns>Task Complete.</returns>
      private async Task<Task> HookEvents()
      {
         _client.Log += Log;
         _commands.Log += Log;
         _client.MessageReceived += HandleCommandAsync;
         await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services).ConfigureAwait(false);
         _client.ReactionAdded += HandleReactionAddedAsync;
         _client.Ready += HandleReady;
         _client.LeftGuild += HandleLeftGuild;
         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Log event.
      /// </summary>
      /// <param name="msg">Message to log.</param>
      /// <returns>Task Complete.</returns>
      private Task Log(LogMessage msg)
      {
         string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         string fileName = DateTime.Now.ToString("MM-dd-yyyy");
         string logFile = $"{path}\\Logs\\{fileName}.txt";

         string logText = $"{DateTime.Now:hh:mm:ss} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";

         while (logging) { }

         logging = true;
         File.AppendAllText(logFile, logText + "\n");
         logging = false;

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
         if (message == null || message.Author.IsBot)
            return Task.CompletedTask;
         var context = new SocketCommandContext(_client, message);

         int argPos = 0;

         string prefix = Connections.Instance().GetPrefix(context.Guild.Id);
         if (prefix == null)
            prefix = Environment.GetEnvironmentVariable("DEFAULT_PREFIX");

         if (message.Attachments.Count != 0)
         {
            if (ChannelRegisterCommands.IsRegisteredChannel(context.Guild.Id, context.Channel.Id, "P"))
               RollImageProcess.RoleImageProcess(context);
            else if (ChannelRegisterCommands.IsRegisteredChannel(context.Guild.Id, context.Channel.Id, "R"))
            {
               //TODO: Add call for raid image processing
            }
            else if (ChannelRegisterCommands.IsRegisteredChannel(context.Guild.Id, context.Channel.Id, "E"))
            {
               //TODO: Add call for ex raid image processing
            }
         }
         else if (message.HasStringPrefix(prefix, ref argPos))
         {
            var result = await _commands.ExecuteAsync(context, argPos, _services).ConfigureAwait(false);
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
         var message = await cachedMessage.GetOrDownloadAsync().ConfigureAwait(false);
         var user = reaction.User.Value;
         if (message != null && reaction.User.IsSpecified && !user.IsBot)
         {
            if (RaidCommands.IsCurrentRaid(message.Id))
               await RaidCommands.RaidReaction(message, reaction);
            else if (RaidCommands.IsRaidInvite(message.Id))
               await RaidCommands.RaidInviteReaction(message, reaction, originChannel);
         }
         return Task.CompletedTask;
      }

      /// <summary>
      /// Handles the Ready event.
      /// </summary>
      /// <returns>Task Complete.</returns>
      private Task HandleReady()
      {
         string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         var json = JObject.Parse(File.ReadAllText($"{path}\\env.json"));
         var homeGuildName = json.GetValue("home_server").ToString();
         var server = _client.Guilds.FirstOrDefault(x => x.Name.ToString().Equals(homeGuildName, StringComparison.OrdinalIgnoreCase));

         SetEmotes(server, json);

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
         Connections.Instance().DeletePrefix(guild.Id);
         return Task.CompletedTask;
      }

      /// <summary>
      /// Sets the emotes from a JSON file
      /// </summary>
      /// <param name="server">Server that the emotes are on.</param>
      /// <param name="json">JSON file that has the emote names.</param>
      private void SetEmotes(SocketGuild server, JObject json)
      {
         string[] emoteNames = {
            "bug_emote", "dark_emote", "dragon_emote", "electric_emote", "fairy_emote", "fighting_emote",
            "fire_emote", "flying_emote", "ghost_emote", "grass_emote", "ground_emote", "ice_emote",
            "normal_emote", "poison_emote", "psychic_emote", "rock_emote", "steel_emote", "water_emote",
            "raid_emote", "valor_emote", "mystic_emote", "instinct_emote"
         };

         foreach (string emote in emoteNames)
            Environment.SetEnvironmentVariable(
               emote.ToUpper(), server.Emotes.FirstOrDefault(
                  x => x.Name.ToString().Equals(
                     json.GetValue(emote.ToLower()).ToString(),
                     StringComparison.OrdinalIgnoreCase)).ToString());
      }

      /// <summary>
      /// Gets the list of commands.
      /// </summary>
      /// <returns>List of command info objects for the commands.</returns>
      public static List<CommandInfo> GetCommands()
      {
         return _commands.Commands.ToList();
      }
   }
}