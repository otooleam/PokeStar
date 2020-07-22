using System;
using System.IO;
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
         _commands = new CommandService();

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
         await _client.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
         await _client.StartAsync().ConfigureAwait(false);

         Environment.SetEnvironmentVariable("SETUP_ROLES", "FALSE");
         Environment.SetEnvironmentVariable("SETUP_RAIDS", "FALSE");
         Environment.SetEnvironmentVariable("SETUP_TRADE", "FALSE");
         Environment.SetEnvironmentVariable("SETUP_DEX", "FALSE");
         Environment.SetEnvironmentVariable("SETUP_EMOJI", "FALSE");

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

      private async Task HandleCommandAsync(SocketMessage arg)
      {
         var message = arg as SocketUserMessage;
         if (message == null)
            return;
         var context = new SocketCommandContext(_client, message);
         if (message.Author.IsBot) return;

         int argPos = 0;

         if (context.Channel.Name.Equals("Verification", StringComparison.OrdinalIgnoreCase))
         {
            if (message.Attachments.Count != 0)
            {
               RollImageProcess.RoleImageProcess(context);
            }
         }
         else if (message.HasStringPrefix(prefix, ref argPos))
         {
            var result = await _commands.ExecuteAsync(context, argPos, _services).ConfigureAwait(false);
            if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
         }
      }

      private void HookReactionAdded()
         => _client.ReactionAdded += HandleReactionAddedAsync;

      private static async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedMessage,
          ISocketMessageChannel originChannel, SocketReaction reaction)
      {
         var message = await cachedMessage.GetOrDownloadAsync().ConfigureAwait(false);
         var user = reaction.User.Value;
         if (message != null && reaction.User.IsSpecified && !user.IsBot && RaidCommand.IsCurrentRaid(message.Id))
         {         
            await RaidCommand.RaidReaction(message, reaction);
         }
      }
   }
}
