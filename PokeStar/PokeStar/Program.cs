using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace PokeStar
{
   public class Program
   {
      // Allows System to run Asynchronously
      public static void Main(string[] args) 
         => new Program().MainAsync().GetAwaiter().GetResult(); //Any Exceptions get thrown here

      private DiscordSocketClient _client;
      private CommandService _commands;
      private IServiceProvider _services;

      private string prefix;

      public async Task MainAsync()
      {
         _client = new DiscordSocketClient();
         _commands = new CommandService();

         _services = new ServiceCollection()
             .AddSingleton(_client)
             .AddSingleton(_commands)
             .BuildServiceProvider();

         _client.Log += Log;

         string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

         var json = JObject.Parse(File.ReadAllText($"{path}\\env.json"));

         var token = json.First.First.ToString();
         prefix = json.Last.First.ToString();

         await RegisterCommandsAsync();
         await _client.LoginAsync(TokenType.Bot, token);
         await _client.StartAsync();

         // Block this task until the program is closed.
         await Task.Delay(-1);
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
         await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
      }

      private async Task HandleCommandAsync(SocketMessage arg)
      {
         var message = arg as SocketUserMessage;
         var context = new SocketCommandContext(_client, message);
         if (message.Author.IsBot) return;

         int argPos = 0;
         if (message.HasStringPrefix(prefix, ref argPos))
         {
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
         }
      }
   }
}
