using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using PokeStar.ConnectionInterface;
using PokeStar.ImageProcessors;
using PokeStar.Modules;

namespace PokeStar
{
   public class Program
   {
      // Allows System to run Asynchronously
      public static void Main()
         => new Program().MainAsync().GetAwaiter().GetResult(); //Any Exceptions get thrown here

      private DiscordSocketClient _client;
      private CommandService _commands;
      private IServiceProvider _services;

      private string prefix;

      public async Task MainAsync()
      {
         //sets cache for reaction events
         var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
         _client = new DiscordSocketClient(_config);
         CommandServiceConfig config = new CommandServiceConfig();
         config.DefaultRunMode = RunMode.Async;
         _commands = new CommandService(config);

         _services = new ServiceCollection()
             .AddSingleton(_client)
             .AddSingleton(_commands)
             .BuildServiceProvider();

         _client.Log += Log;

         string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

         var json = JObject.Parse(File.ReadAllText($"{path}\\env.json"));

         var token = json.GetValue("token").ToString();
         Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", json.GetValue("sql").ToString());
         prefix = json.GetValue("prefix").ToString();
         Environment.SetEnvironmentVariable("PREFIX_STRING", prefix);

         await RegisterCommandsAsync().ConfigureAwait(false);
         HookReactionAdded();
         HookSetup();
         HookJoinedGuild();
         HookLeftGuild()
;         await _client.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
         await _client.StartAsync().ConfigureAwait(false);

         Environment.SetEnvironmentVariable("SETUP_COMPLETE", "FALSE");

         var connectors = Connections.Instance();

         // Block this task until the program is closed.
         await Task.Delay(-1).ConfigureAwait(false);
      }

      //TODO set up proper logging framework
      private Task Log(LogMessage msg)
      {
         Console.WriteLine(msg.ToString());
         return Task.CompletedTask;
      }

      public async Task RegisterCommandsAsync()
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

         if (message.Attachments.Count != 0)
         {
            if (ChannelRegisterCommand.IsRegisteredChannel(context.Guild.Id, context.Channel.Id, "P"))
               RollImageProcess.RoleImageProcess(context);
            if (ChannelRegisterCommand.IsRegisteredChannel(context.Guild.Id, context.Channel.Id, "R"))
            {
               //TODO: Add call for raid image processing
            }
            if (ChannelRegisterCommand.IsRegisteredChannel(context.Guild.Id, context.Channel.Id, "E"))
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
         if (message != null && reaction.User.IsSpecified && !user.IsBot && RaidCommand.IsCurrentRaid(message.Id))
         {         
            await RaidCommand.RaidReaction(message, reaction);
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

         ChannelRegisterCommand.LoadChannels(_client.Guilds);

         return Task.CompletedTask;
      }

      private void HookJoinedGuild()
         => _client.JoinedGuild += HandleJoinedGuild;

      private Task HandleJoinedGuild(SocketGuild guild)
      {
         ChannelRegisterCommand.AddGuild(guild.Id, true);
         return Task.CompletedTask;
      }

      private void HookLeftGuild()
         => _client.LeftGuild += HandleLeftGuild;

      private Task HandleLeftGuild(SocketGuild guild)
      {
         ChannelRegisterCommand.RemoveGuild(guild.Id);
         return Task.CompletedTask;
      }

      private static void SetEmotes(SocketGuild server, JObject json)
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
   }
}
