using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using Discord;
using Discord.Commands;

namespace PokeStar.Modules
{
   public class HelpCommands : ModuleBase<SocketCommandContext>
   {
      [Command("Help")]
      [Summary("Displays info about commands.")]
      public async Task Help([Summary("(Optional) Get help with this command.")] string command = null)
      {
         List<CommandInfo> commands = Program.GetCommands();
         EmbedBuilder embedBuilder = new EmbedBuilder();
         embedBuilder.WithColor(Color.Green);

         if (command == null)
         {
            embedBuilder.WithTitle("Command List");
            embedBuilder.WithDescription("List of commands supported by this bot.");
            foreach (var cmdInfo in commands)
               embedBuilder.AddField(cmdInfo.Name, cmdInfo.Summary ?? "No description available");
            embedBuilder.WithFooter("Run \"help command_name\" to get help for a specific command.");
            await ReplyAsync(embed: embedBuilder.Build());
         }
         else if (commands.FirstOrDefault(x => x.Name.Equals(command, StringComparison.OrdinalIgnoreCase)) != null)
         {
            CommandInfo cmdInfo = commands.FirstOrDefault(x => x.Name.Equals(command, StringComparison.OrdinalIgnoreCase));
            embedBuilder.WithTitle($"{command} Command Parameter List");
            embedBuilder.WithDescription(cmdInfo.Summary ?? "No description available");
            if (cmdInfo.Aliases.Count > 1)
            {
               string aliases = "";
               foreach (var alias in cmdInfo.Aliases)
                  if (!alias.Equals(command, StringComparison.OrdinalIgnoreCase))
                     aliases += $"{alias}, ";
               aliases = aliases.TrimEnd().TrimEnd(',');
               embedBuilder.AddField("Aliases:", aliases);
            }
            if (cmdInfo.Remarks != null)
               embedBuilder.AddField("Additional Information:", cmdInfo.Remarks);
            foreach (var param in cmdInfo.Parameters)
               embedBuilder.AddField(param.Name, param.Summary ?? "No description available");
            if (cmdInfo.Parameters.Count == 0)
               embedBuilder.WithFooter("*This command does not take any parameters.");
            await ReplyAsync(embed: embedBuilder.Build());
         }
         else
            await ReplyAsync($"Command \'{command}\' does not exist. Run the help command to get a list of valid commands.");
      }
   }
}
