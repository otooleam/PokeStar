using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace PokeStar.Modules
{
   public class SystemEditCommands : ModuleBase<SocketCommandContext>
   {
      private static Dictionary<ulong, char> commandPrefix = new Dictionary<ulong, char>();

      [Command("prefix")]
      public async Task Prefix(char prefix = '.')
      {
         ulong guild = Context.Guild.Id;
         if (commandPrefix.ContainsKey(guild))
            commandPrefix[guild] = prefix;
         else
            commandPrefix.Add(guild, prefix);
         SavePrefix();
         await ReplyAsync($"Command prefix has been set to \'{prefix}\' for this server.").ConfigureAwait(false);
      }

      private static void SavePrefix()
      {
         string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         string json = JsonConvert.SerializeObject(commandPrefix, Formatting.Indented);
         File.WriteAllText($"{path}\\prefix.json", json);
      }

      public static void LoadPrefix(IReadOnlyCollection<SocketGuild> guilds, char defaultPrefix)
      {
         string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         string json = File.ReadAllText($"{path}\\prefix.json");
         commandPrefix = JsonConvert.DeserializeObject<Dictionary<ulong, char>>(json);
         if (commandPrefix == null)
            commandPrefix = new Dictionary<ulong, char>();

         foreach (var guild in guilds)
            if (!commandPrefix.ContainsKey(guild.Id))
               AddGuild(guild.Id, defaultPrefix);
      }

      public static void AddGuild(ulong guild, char defaultPrefix, bool save = false)
      {
         commandPrefix.Add(guild, defaultPrefix);
         if (save)
            SavePrefix();
      }

      public static void RemoveGuild(ulong guild)
      {
         commandPrefix.Remove(guild);
         SavePrefix();
      }

      public static char GetPrefix(ulong guild)
      {
         return commandPrefix[guild];
      }
   }
}
