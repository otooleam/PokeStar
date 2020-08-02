using System;
using System.Threading.Tasks;
using Discord.Commands;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   public class ChannelRegisterCommands : ModuleBase<SocketCommandContext>
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

      private static bool CheckSetupComplete = false;

      // Registers the channel this command is run in as a command channel
      // Requires .setup to have been run
      [Command("register")]
      [Summary("Registers a channel for a type of command.")]
      [Remarks("Valid registration values are:\n" +
               "Command Type.....Registration Value\n" +
               "Players/Roles.........player / role / p\n" +
               "Raids.........................raid / r\n" +
               "EX Raids...................ex / e\n" +
               "Raid Trains..............train / t\n" +
               "Pokedex...................pokedex / dex / d\n" +
               "Leave blank to register for all.")]
      public async Task Register([Summary("(Optional) Register the channel for these commands.")] string register = "ALL")
      {
         ulong guild = Context.Guild.Id;
         ulong channel = Context.Channel.Id;
         string registration = Connections.Instance().GetRegistration(guild, channel);
         registration = GenerateRegistrationString(register, registration ?? "");

         if (registration == null)
         {
            await Context.Channel.SendMessageAsync("Please enter a valid registration value.");
            return;
         }

         Connections.Instance().UpdateRegistration(guild, channel, registration);

         await Context.Channel.SendMessageAsync($"Channel is now registered for the following command types {GenerateSummaryString(registration)}");

         if (CheckSetupComplete && Environment.GetEnvironmentVariable("SETUP_COMPLETE").Equals("FALSE", StringComparison.OrdinalIgnoreCase))
         {
            await Context.Channel.SendMessageAsync($"Please run the .setup command to ensure required roles have been setup.");
            CheckSetupComplete = false;
         }
      }

      [Command("unregister")]
      [Summary("Unregisters a channel from a type of command.")]
      [Remarks("Valid unregistration values are:\n" +
               "Command Type.....Unregistration Value\n" +
               "Players/Roles.........player / role / p\n" +
               "Raids.........................raid / r\n" +
               "EX Raids...................ex / e\n" +
               "Raid Trains..............train / t\n" +
               "Pokedex...................pokedex / dex / d\n" +
               "Leave blank to unregister for all.")]
      public async Task Unregister([Summary("(Optional) Unregister the channel from these commands.")] string unregister = "ALL")
      {
         ulong guild = Context.Guild.Id;
         ulong channel = Context.Channel.Id;
         string reg;

         string registration = Connections.Instance().GetRegistration(guild, channel);

         if (registration != null)
         {
            reg = GenerateUnregistrationString(unregister, registration);
            if (reg == null)
            {
               await Context.Channel.SendMessageAsync("Please enter a valid registration value.");
               return;
            }
            else if (reg.Equals(string.Empty))
               Connections.Instance().DeleteRegistration(guild, channel);
            else
               Connections.Instance().UpdateRegistration(guild, channel, reg);
         }
         else
         {
               await Context.Channel.SendMessageAsync("This channel does not have any commands registered to it");
               return;
         }
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
         else if (purpose.ToUpper().Equals("PLAYER") || purpose.ToUpper().Equals("ROLE") || purpose.ToUpper().Equals("P"))
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
         else if (purpose.ToUpper().Equals("POKEDEX") || purpose.ToUpper().Equals("DEX") || purpose.ToUpper().Equals("D"))
            add = "D";
         else
            return null;

         if (existing.Equals(string.Empty))
            return add;

         if (existing.Contains(add))
            return existing;

         string s = existing + add;
         char[] a = s.ToCharArray();
         Array.Sort(a);
         return new string(a).ToUpper();
      }

      private static string GenerateUnregistrationString(string purpose, string existing)
      {
         string remove;
         if (purpose.ToUpper().Equals("ALL"))
            return "";
         else if (purpose.ToUpper().Equals("PLAYER") || purpose.ToUpper().Equals("ROLE") || purpose.ToUpper().Equals("P"))
            remove = "P";
         else if (purpose.ToUpper().Equals("RAID") || purpose.ToUpper().Equals("R"))
            remove = "R";
         else if (purpose.ToUpper().Equals("EX") || purpose.ToUpper().Equals("E"))
            remove = "E";
         else if (purpose.ToUpper().Equals("TRAIN") || purpose.ToUpper().Equals("T"))
            remove = "T";
         else if (purpose.ToUpper().Equals("POKEDEX") || purpose.ToUpper().Equals("DEX") || purpose.ToUpper().Equals("D"))
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
         return summary.TrimEnd().TrimEnd(',');
      }

      public static bool IsRegisteredChannel(ulong guild, ulong channel, string type)
      {
         string registration = Connections.Instance().GetRegistration(guild, channel);
         if (registration == null)
            return false;
         return registration.Contains(type.ToUpper());
      }
   }
}
