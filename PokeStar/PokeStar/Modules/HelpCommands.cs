using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles help commands.
   /// </summary>
   public class HelpCommands : ModuleBase<SocketCommandContext>
   {
      [Command("help")]
      [Summary("Displays info about commands.")]
      public async Task Help([Summary("(Optional) Get help with this command.")] string command = null)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.Gold);

         SocketGuildUser user = Context.Guild.Users.FirstOrDefault(x => x.Id == Context.User.Id);
         bool isAdmin = (user.Roles.Where(role => role.Permissions.Administrator).ToList().Count != 0 || Context.Guild.OwnerId == user.Id) ;
         bool isNona = Context.Guild.Name.Equals(Global.HOME_SERVER, StringComparison.OrdinalIgnoreCase);

         if (command == null)
         {
            string prefix = Connections.Instance().GetPrefix(Context.Guild.Id);
            embed.WithTitle("**Command List**");
            embed.WithDescription($"List of commands supported by Nona.");
            foreach (CommandInfo cmdInfo in Global.COMMAND_INFO)
            {
               if (CheckShowCommand(cmdInfo.Name, isAdmin, isNona))
               {
                  embed.AddField($"**{prefix}{cmdInfo.Name}**", cmdInfo.Summary ?? "No description available");
               }
            }
            embed.WithFooter($"Run \"{prefix}help <command name>\" to get help for a specific command.");
            await ReplyAsync(embed: embed.Build());
         }
         else if (Global.COMMAND_INFO.FirstOrDefault(x => x.Name.Equals(command, StringComparison.OrdinalIgnoreCase)) is CommandInfo cmdInfo
            && CheckShowCommand(cmdInfo.Name, isAdmin, isNona))
         {
               embed.WithTitle($"**{command} command help**");
               embed.WithDescription(cmdInfo.Summary ?? "No description available");
               if (cmdInfo.Aliases.Count > 1)
               {
                  StringBuilder sb = new StringBuilder();
                  foreach (string alias in cmdInfo.Aliases)
                  {
                     if (!alias.Equals(command, StringComparison.OrdinalIgnoreCase))
                     {
                        sb.Append($"{alias}, ");
                     }
                  }
                  embed.AddField("Alternate Command:", sb.ToString().TrimEnd().TrimEnd(','));
               }
               if (cmdInfo.Remarks != null)
               {
                  embed.AddField("**Additional Information:**", cmdInfo.Remarks);
               }
               foreach (ParameterInfo param in cmdInfo.Parameters)
               {
                  embed.AddField($"**<{param.Name}>**", param.Summary ?? "No description available");
               }
               if (cmdInfo.Parameters.Count == 0)
               {
                  embed.WithFooter("*This command does not take any parameters.");
               }
               await ReplyAsync(embed: embed.Build());
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "help", $"Command \'{command}\' does not exist. Run the '.help' command to get a list of valid commands.");
         }
      }

      /// <summary>
      /// Checks if the command should be shown to the user.
      /// Uses the following check:
      /// Z = (!A && !B && !C) || (B && D) || (C && D && E)
      /// 
      /// Input:
      /// A: Is hidden command
      /// B: Is admin command
      /// C: Is nona admin command
      /// D: User is an admin on the server
      /// E: Server is the home server
      /// </summary>
      /// <param name="command">Command to check.</param>
      /// <param name="isAdmin">If the user has the admin permission.</param>
      /// <param name="isNona">If ther server is the home server.</param>
      /// <returns>True if the command should be shown, otherwise false.</returns>
      public static bool CheckShowCommand(string command, bool isAdmin, bool isNona)
      {
         return (!Global.HIDDEN_COMMANDS.Contains(command) && !Global.ADMIN_COMMANDS.Contains(command) && !Global.NONA_ADMIN_COMMANDS.Contains(command))
                  || (isAdmin && Global.ADMIN_COMMANDS.Contains(command)) || (isAdmin && isNona && Global.NONA_ADMIN_COMMANDS.Contains(command));
      }
   }
}