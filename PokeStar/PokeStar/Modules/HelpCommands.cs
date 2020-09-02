using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;
using System.Text;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles help commands.
   /// </summary>
   public class HelpCommands : ModuleBase<SocketCommandContext>
   {
      private static readonly List<string> HiddenCommands = new List<string>()
      {
         "ping",
         "help",
         "rave",
         "screm",
         "updatePokemonNames"
      };

      [Command("help")]
      [Summary("Displays info about commands.")]
      public async Task Help([Summary("(Optional) Get help with this command.")] string command = null)
      {
         List<CommandInfo> commands = Program.GetCommands();
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.Gold);

         if (command == null)
         {
            string prefix = Connections.Instance().GetPrefix(Context.Guild.Id);
            embed.WithTitle("**Command List**");
            embed.WithDescription($"List of commands supported by Nona.");
            foreach (CommandInfo cmdInfo in commands)
            {
               if (!HiddenCommands.Contains(cmdInfo.Name))
               {
                  embed.AddField($"**{prefix}{cmdInfo.Name}**", cmdInfo.Summary ?? "No description available");
               }
            }
            embed.WithFooter($"Run \"{prefix}help <command name>\" to get help for a specific command.");
            await ReplyAsync(embed: embed.Build());
         }
         else if (commands.FirstOrDefault(x => x.Name.Equals(command, StringComparison.OrdinalIgnoreCase)) != null)
         {
            CommandInfo cmdInfo = commands.FirstOrDefault(x => x.Name.Equals(command, StringComparison.OrdinalIgnoreCase));
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
            await ResponseMessage.SendErrorMessage(Context, "help", $"Command \'{command}\' does not exist. Run the '.help' command to get a list of valid commands.");
         }
      }
   }
}