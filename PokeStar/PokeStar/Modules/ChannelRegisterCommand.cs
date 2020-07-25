using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Discord.Commands;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace PokeStar.Modules
{
   public class ChannelRegisterCommand : ModuleBase<SocketCommandContext>
   {
      // KEY: Players(P), Raids(R), EX-Raids(E), Raid Train(T), Pokedex(D)

      /* 
       for the command that needs to be channel verified
       if (ChannelRegisterCommand.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, 'type'))
       {
       }
       else
          await ReplyAsync("This channel is not registered to process "type" commands.");
      */

      private static Dictionary<ulong, Dictionary<ulong, string>> registeredChannels = new Dictionary<ulong, Dictionary<ulong, string>>();

      private static bool CheckSetupComplete = false;

      // Registers the channel this command is run in as a command channel
      // Requires .setup to have been run
      [Command("register")]
      public async Task Register(string purpose = "ALL")
      {
         ulong guild = Context.Guild.Id;
         ulong channel = Context.Channel.Id;
         string reg;

         if (registeredChannels[guild].ContainsKey(channel))
            reg = GenerateRegistrationString(purpose, registeredChannels[guild][channel]);
         else
            reg = GenerateRegistrationString(purpose);

         if (reg != null)
            registeredChannels[guild].Add(channel, reg);
         else
         {
            await Context.Channel.SendMessageAsync("Please enter a valid registration for one of the following Players(P), Raids(R), EX-Raids(E), Raid Train(T), Pokedex(D) or give no value for all");
            return;
         }

         SaveChannels();
         await Context.Channel.SendMessageAsync($"Channel is now registered for the following command types {GenerateSummaryString(reg)}");

         if (CheckSetupComplete && Environment.GetEnvironmentVariable("SETUP_COMPLETE").Equals("FALSE", StringComparison.OrdinalIgnoreCase))
         {
            await Context.Channel.SendMessageAsync($"Please run the .setup command to ensure required roles have been setup.");
            CheckSetupComplete = false;
         }
      }

      [Command("unregister")]
      public async Task Unregister(string purpose = "ALL")
      {
         ulong guild = Context.Guild.Id;
         ulong channel = Context.Channel.Id;
         string reg;

         if (registeredChannels[guild].ContainsKey(channel))
         {
            reg = GenerateUnregistrationString(purpose, registeredChannels[guild][channel]);
            if (reg == null)
            {
               await Context.Channel.SendMessageAsync("Please enter a valid registration for one of the following Players(P), Raids(R), EX-Raids(E), Raid Train(T), Pokedex(D) or give no value for all");
               return;
            }
            else if (reg.Equals(string.Empty))
               registeredChannels.Remove(guild);
            else
               registeredChannels[guild][channel] = reg;
         }
         else
         {
               await Context.Channel.SendMessageAsync("This channel does not have any commands registered to it");
               return;
         }
         SaveChannels();
         if (reg.Equals(string.Empty))
            await Context.Channel.SendMessageAsync($"Removed all registrations from this channel.");
         else
            await Context.Channel.SendMessageAsync($"Channel is now registered for the following command types {GenerateSummaryString(reg)}");
      }






      private static string GenerateRegistrationString(string purpose, string existing = "")
      {
         string add;
         if (purpose.ToUpper().Equals("ALL"))
         {
            add = "DEPRT";
            CheckSetupComplete = true;
         }
         else if (purpose.ToUpper().Equals("PLAYER") || purpose.ToUpper().Equals("P"))
         {
            add = "P";
            CheckSetupComplete = true;
         }
         else if (purpose.ToUpper().Equals("RAID") || purpose.ToUpper().Equals("R"))
            add = "R";
         else if (purpose.ToUpper().Equals("EX") || purpose.ToUpper().Equals("E"))
            add = "E";
         else if (purpose.ToUpper().Equals("TRAIN") || purpose.ToUpper().Equals("T"))
            add = "T";
         else if (purpose.ToUpper().Equals("DEX") || purpose.ToUpper().Equals("D"))
            add = "D";
         else
            return null;

         if (existing.Equals(string.Empty))
            return add.ToString();

         if (existing.Contains(add))
            return existing;

         string s = existing + add;
         char[] a = s.ToCharArray();
         Array.Sort(a);
         return new string(a).ToUpper();
      }

      private static string GenerateUnregistrationString(string purpose, string existing = "")
      {
         string remove;
         if (purpose.ToUpper().Equals("ALL"))
            return "";
         else if (purpose.ToUpper().Equals("PLAYER") || purpose.ToUpper().Equals("P"))
            remove = "P";
         else if (purpose.ToUpper().Equals("RAID") || purpose.ToUpper().Equals("R"))
            remove = "R";
         else if (purpose.ToUpper().Equals("EX") || purpose.ToUpper().Equals("E"))
            remove = "E";
         else if (purpose.ToUpper().Equals("TRAIN") || purpose.ToUpper().Equals("T"))
            remove = "T";
         else if (purpose.ToUpper().Equals("DEX") || purpose.ToUpper().Equals("D"))
            remove = "D";
         else
            return null;

         int index = existing.IndexOf(remove);
         return (index < 0) ? null : existing.Remove(index, remove.Length);

      }

      private static string GenerateSummaryString(string reg)
      {
         string summary = "";
         if (reg.ToUpper().Contains("P"))
            summary += "Player Roles, ";
         if (reg.ToUpper().Contains("R"))
            summary += "Raids, ";
         if (reg.ToUpper().Contains("E"))
            summary += "EX-Raids, ";
         if (reg.ToUpper().Contains("T"))
            summary += "Raid Trains, ";
         if (reg.ToUpper().Contains("D"))
            summary += "PokeDex, ";
         summary = summary.Trim();
         return summary.TrimEnd(',');
      }

      private static void SaveChannels()
      {
         string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         string json = JsonConvert.SerializeObject(registeredChannels, Formatting.Indented);
         File.WriteAllText($"{path}\\chan_reg.json", json);
      }

      public static void LoadChannels(IReadOnlyCollection<SocketGuild> guilds)
      {
         string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         string json = File.ReadAllText($"{path}\\chan_reg.json");
         registeredChannels = JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<ulong, string>>>(json);
         if (registeredChannels == null)
            registeredChannels = new Dictionary<ulong, Dictionary<ulong, string>>();

         foreach (var guild in guilds)
            if (!registeredChannels.ContainsKey(guild.Id))
               AddGuild(guild.Id);
      }

      public static void AddGuild(ulong guild, bool save = false)
      {
         registeredChannels.Add(guild, new Dictionary<ulong, string>());
         if (save)
            SaveChannels();
      }

      public static void RemoveGuild(ulong guild)
      {
         registeredChannels.Remove(guild);
         SaveChannels();
      }

      public static bool IsRegisteredChannel(ulong guild, ulong channel, string type)
      {
         if (registeredChannels.Keys.Contains(guild))
         {
            if (registeredChannels[guild].ContainsKey(channel))
            {
               string temp = registeredChannels[guild][channel];
               return type.Length == 1 && temp.Contains(type.ToUpper());
            }
         }
         return false;
      }
   }
}
