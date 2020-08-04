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
using System.Collections.Generic;
using PokeStar.DataModels;

namespace PokeStar
{
   public class Program
   {
      // Allows System to run Asynchronously
      public static void Main()
         => new Program().MainAsync().GetAwaiter().GetResult(); //Any Exceptions get thrown here

      private DiscordSocketClient _client;
      private static CommandService _commands;
      private IServiceProvider _services;

      private bool logging = false;

      public async Task MainAsync()
      {
         string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         var json = JObject.Parse(File.ReadAllText($"{path}\\env.json"));

         var token = json.GetValue("token").ToString();
         Environment.SetEnvironmentVariable("POGO_DB_CONNECTION_STRING", json.GetValue("pogo_db_sql").ToString());
         Environment.SetEnvironmentVariable("NONA_DB_CONNECTION_STRING", json.GetValue("nona_db_sql").ToString());
         Environment.SetEnvironmentVariable("DEFAULT_PREFIX", json.GetValue("prefix").ToString());

         var logLevel = Convert.ToInt32(json.GetValue("log_level").ToString());
         var logSeverity = !Enum.IsDefined(typeof(LogSeverity), logLevel) ? LogSeverity.Info : (LogSeverity)logLevel;

         //sets cache for reaction events
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

         HookLog();
         await RegisterCommandsAsync().ConfigureAwait(false);
         HookReactionAdded();
         HookSetup();
         HookLeftGuild();
         await _client.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
         await _client.StartAsync().ConfigureAwait(false);

         await _client.SetGameAsync(".help | v0.1.dev");

         Environment.SetEnvironmentVariable("SETUP_COMPLETE", "FALSE");

         // Block this task until the program is closed.
         await Task.Delay(-1).ConfigureAwait(false);
      }

      private void HookLog()
      {
         _client.Log += Log;
         _commands.Log += Log;
      }

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

      private async Task RegisterCommandsAsync()
      {
         _client.MessageReceived += HandleCommandAsync;
         await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services).ConfigureAwait(false);
      }

      private async Task<Task> HandleCommandAsync(SocketMessage arg)
      {
         SocketUserMessage message = arg as SocketUserMessage;
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

      private void HookReactionAdded()
         => _client.ReactionAdded += HandleReactionAddedAsync;

      private static async Task<Task> HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedMessage,
          ISocketMessageChannel originChannel, SocketReaction reaction)
      {
         var message = await cachedMessage.GetOrDownloadAsync().ConfigureAwait(false);
         var user = reaction.User.Value;
         if (message != null && reaction.User.IsSpecified && !user.IsBot)
         {
            if (RaidCommands.IsCurrentRaid(message.Id))
               await RaidCommands.RaidReaction(message, reaction);
            else if (RaidCommands.isRaidInvite(message.Id))
               await RaidCommands.RaidInviteReaction(message, reaction, originChannel);
         }
         return Task.CompletedTask;
      }

      private void HookSetup()
         => _client.Ready += HandleReady;

      private Task HandleReady()
      {
         string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         var json = JObject.Parse(File.ReadAllText($"{path}\\env.json"));
         var homeGuildName = json.GetValue("home_server").ToString();
         var server = _client.Guilds.FirstOrDefault(x => x.Name.ToString().Equals(homeGuildName, StringComparison.OrdinalIgnoreCase));

         SetEmotes(server, json);

         return Task.CompletedTask;
      }

      private void HookLeftGuild()
         => _client.LeftGuild += HandleLeftGuild;

      private Task HandleLeftGuild(SocketGuild guild)
      {
         Connections.Instance().DeleteRegistration(guild.Id);
         Connections.Instance().DeletePrefix(guild.Id);
         return Task.CompletedTask;
      }

      private void SetEmotes(SocketGuild server, JObject json)
      {
         string[] emoteNames = {
            "bug_emote", "dark_emote", "dragon_emote", "electric_emote", "fairy_emote", "fighting_emote",
            "fire_emote", "flying_emote", "ghost_emote", "grass_emote", "ground_emote", "ice_emote",
            "normal_emote", "poison_emote", "psychic_emote", "rock_emote", "steel_emote", "water_emote",
            "raid_emote", "valor_emote", "mystic_emote", "instinct_emote"
         };

         foreach (string emote in emoteNames)
            Environment.SetEnvironmentVariable(emote.ToUpper(),
               server.Emotes.FirstOrDefault(x => x.Name.ToString().Equals(json.GetValue(emote.ToLower()).ToString(), StringComparison.OrdinalIgnoreCase)).ToString());
      }

      public static List<CommandInfo> GetCommands()
      {
         return _commands.Commands.ToList();
      }
   }
}
