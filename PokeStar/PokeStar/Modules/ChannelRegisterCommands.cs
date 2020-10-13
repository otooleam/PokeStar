using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles channel registration commands.
   /// </summary>
   public class ChannelRegisterCommands : ModuleBase<SocketCommandContext>
   {
      [Command("register")]
      [Summary("Registers a channel to run a given type of command.")]
      [Remarks("To register a channel\n" +
               "for this..........................use one of these:\n" +
               "Player Registration....player / role / p\n" +
               "Raids.............................raid / r\n" +
               //"EX Raids.......................ex / e\n" +
               //"Raid Trains..................train / t\n" +
               "Pokedex........................pokedex / dex / d" +
               "Leave blank to register for all command types.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task Register([Summary("(Optional) Register the channel for these commands.")] string register = null)
      {
         ulong guild = Context.Guild.Id;
         ulong channel = Context.Channel.Id;
         string registration = Connections.Instance().GetRegistration(guild, channel);
         Tuple<string, bool> result = GenerateRegistrationString(register ?? Global.REGISTER_ALL_STRING, registration ?? "");
         registration = result.Item1;

         if (registration == null)
         {
            await ResponseMessage.SendErrorMessage(Context, "register", "Please enter a valid registration value.");
         }
         else
         {
            Connections.Instance().UpdateRegistration(guild, channel, registration);
            await ResponseMessage.SendInfoMessage(Context, $"Channel is now registered for the following command types {GenerateSummaryString(registration)}");

            if (result.Item2 && !Connections.Instance().GetSetupComplete(guild))
            {
               await ResponseMessage.SendWarningMessage(Context, "register", "Please run the .setup command to ensure required roles have been setup.");
            }
         }
      }

      [Command("unregister")]
      [Summary("Unregisters a channel from a given type of command.")]
      [Remarks("To unregister a channel\n" +
               "from this.......................use one of these:\n" +
               "Player Registration.....player / role / p\n" +
               "Raids..............................raid / r\n" +
               //"EX Raids.......................ex / e\n" +
               //"Raid Trains..................train / t\n" +
               "Pokedex........................pokedex / dex / d" +
               "Leave blank to unregister from all command types.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task Unregister([Summary("(Optional) Unregister the channel from these commands.")] string unregister = null)
      {
         ulong guild = Context.Guild.Id;
         ulong channel = Context.Channel.Id;
         string reg;

         string registration = Connections.Instance().GetRegistration(guild, channel);

         if (registration != null)
         {
            reg = GenerateUnregistrationString(unregister ?? Global.UNREGISTER_ALL_STRING, registration);
            if (reg == null)
            {
               await ResponseMessage.SendErrorMessage(Context, "unregister", "Please enter a valid registration value.");
            }
            else if (string.IsNullOrEmpty(reg))
            {
               Connections.Instance().DeleteRegistration(guild, channel);
               await ResponseMessage.SendInfoMessage(Context, $"Removed all registrations from this channel.");
            }
            else
            {
               Connections.Instance().UpdateRegistration(guild, channel, reg);
               await ResponseMessage.SendInfoMessage(Context, $"Channel is now registered for the following command types {GenerateSummaryString(reg)}");
            }
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context, "unregister", "This channel does not have any commands registered to it");
         }
      }

      /// <summary>
      /// Adds a registered command type from a channel register string.
      /// If the channel is already registed for a command type
      /// nothing will be changed.
      /// </summary>
      /// <param name="register">Command type to register for a channel.</param>
      /// <param name="existing">Channel's existing register string.</param>
      /// <returns>Updated register string.</returns>
      private static Tuple<string, bool> GenerateRegistrationString(string register, string existing = "")
      {
         string add;
         bool CheckSetupComplete = false;

         if (Global.REGISTER_VALIE_STRING.ContainsKey(register) &&
             !register.Equals(Global.UNREGISTER_ALL_STRING, StringComparison.OrdinalIgnoreCase))
         {
            add = Global.REGISTER_VALIE_STRING[register];
            if (add.Equals(Global.FULL_REGISTER_STRING) ||
                add.Equals(Global.REGISTER_STRING_ROLE.ToString()))
            {
               CheckSetupComplete = true;
            }
         }
         else
         {
            return null;
         }

         if (existing.Length == 0)
         {
            return new Tuple<string, bool>(add, CheckSetupComplete);
         }
         else if (existing.Contains(add))
         {
            return new Tuple<string, bool>(existing, CheckSetupComplete);
         }

         string s = existing + add;
         char[] a = s.ToCharArray();
         Array.Sort(a);
         return new Tuple<string, bool>(new string(a).ToUpper(), CheckSetupComplete);
      }

      /// <summary>
      /// Removes a registered command type from a channel register string.
      /// If the channel is not registed for a command type
      /// nothing will be changed.
      /// </summary>
      /// <param name="unregister">Command type to unregister from a channel.</param>
      /// <param name="existing">Channel's existing register string.</param>
      /// <returns>Updated register string.</returns>
      private static string GenerateUnregistrationString(string unregister, string existing)
      {
         string remove;
         if (Global.REGISTER_VALIE_STRING.ContainsKey(unregister) &&
             !unregister.Equals(Global.REGISTER_ALL_STRING, StringComparison.OrdinalIgnoreCase))
         {
            remove = Global.REGISTER_VALIE_STRING[unregister];
         }
         else
         {
            return null;
         }

         int index = existing.IndexOf(remove);
         return (index < 0) ? null : existing.Remove(index, remove.Length);
      }

      /// <summary>
      /// Generates a list of command types registered as a string.
      /// </summary>
      /// <param name="reg">Channel register string.</param>
      /// <returns>Channel register summary string.</returns>
      private static string GenerateSummaryString(string reg)
      {
         StringBuilder sb = new StringBuilder();
         if (reg.ToUpper().Contains(Global.REGISTER_STRING_ROLE.ToString()))
         {
            sb.Append("Player Roles, ");
         }
         if (reg.ToUpper().Contains(Global.REGISTER_STRING_RAID.ToString()))
         {
            sb.Append("Raids, ");
         }
         if (reg.ToUpper().Contains(Global.REGISTER_STRING_EX.ToString()))
         {
            sb.Append("EX-Raids, ");
         }
         if (reg.ToUpper().Contains(Global.REGISTER_STRING_TRAIN.ToString()))
         {
            sb.Append("Raid Trains, ");
         }
         if (reg.ToUpper().Contains(Global.REGISTER_STRING_DEX.ToString()))
         {
            sb.Append("PokeDex, ");
         }
         return sb.ToString().TrimEnd().TrimEnd(',');
      }

      /// <summary>
      /// Checks if the channel is registered for a type of command.
      /// </summary>
      /// <param name="guild">Guild that has the channel.</param>
      /// <param name="channel">Channel to check for registered command type.</param>
      /// <param name="type">Type of command to check if the channel is registered for.</param>
      /// <returns>True if the channel is registed for the command type, else false.</returns>
      public static bool IsRegisteredChannel(ulong guild, ulong channel, char type)
      {
         string registration = Connections.Instance().GetRegistration(guild, channel);
         if (registration == null)
         {
            return false;
         }
         return registration.Contains(type.ToString().ToUpper());
      }
   }
}