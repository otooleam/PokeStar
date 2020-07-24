using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Discord.Commands;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;

namespace PokeStar.Modules
{
   public class ChannelRegisterCommand : ModuleBase<SocketCommandContext>
   {
      private string master = "DEPRT";
      // Players, Raids, EX, TRAIN, DEX

      public static Dictionary<ulong, Dictionary<ulong, string>> registeredChannels = new Dictionary<ulong, Dictionary<ulong, string>>();

      public static bool IsRegisteredChannel(ulong guild, ulong channel, string type)
      {
         string temp =  registeredChannels[guild][channel];
         return type.Length == 1 && temp.Contains(type.ToUpper());

      }

      // Registers the channel this command is run in as a command channel
      // Requires .setup to have been run
      [Command("register channel")]
      public async Task Register(string purpose = "ALL")
      {
         ulong guild = Context.Guild.Id;
         ulong channel = Context.Channel.Id;

         if (registeredChannels.Keys.Contains(guild))
         {
            if (registeredChannels[guild].ContainsKey(channel))
            {
               string reg = GenerateString(purpose, registeredChannels[guild][channel]);
               if (reg != null)
                  registeredChannels[guild][channel] = reg;
               else
                  await Context.Channel.SendMessageAsync("Please enter a valid registration for one of the following Players(P), Raids(R), EX-Raids(E), Raid Train(T), Pokedex(D) or give no value for all");
            }
            else
            {
               string reg = GenerateString(purpose);
               if (reg != null)
                  registeredChannels[guild].Add(channel, reg);
               else
                  await Context.Channel.SendMessageAsync("Please enter a valid registration for one of the following Players(P), Raids(R), EX-Raids(E), Raid Train(T), Pokedex(D) or give no value for all");
            }
            SaveChannels();
            await Context.Channel.SendMessageAsync("Channel registeration complete");
         }
         else
            await Context.Channel.SendMessageAsync("Please run .setup for this server before registering command channels").ConfigureAwait(false);
      }

      /* for the command that needs to be channel verified
         if (ChannelRegisterCommand.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, 'type'))
         {
         }
         else
            await Context.Channel.SendMessageAsync("This channel is not registered for commands.");
      */

      private static void SaveChannels()
      {
         string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         string json = JsonConvert.SerializeObject(registeredChannels, Formatting.Indented);
         File.WriteAllText($"{path}\\chan_reg.json", json);
      }

      private static void LoadChannels()
      {
         string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         string json = File.ReadAllText($"{path}\\chan_reg.json");
         registeredChannels = JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<ulong, string>>>(json);
      }

      private static string GenerateString(string purpose, string existing = "")
      {
         string add = "";

         if (purpose.ToUpper().Equals("ALL"))
            add = "DEPRT";
         else if (purpose.ToUpper().Equals("PLAYER") || purpose.ToUpper().Equals("P"))
            add = "P";
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
         return new string(a);
      }

   }
}
