using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles help commands.
   /// </summary>
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
            string prefix = Connections.Instance().GetPrefix(Context.Guild.Id);
            if (prefix == null)
            {
               prefix = Environment.GetEnvironmentVariable("DEFAULT_PREFIX");
            }

            embedBuilder.WithTitle("Command List");
            embedBuilder.WithDescription($"List of commands supported by this bot.");
            foreach (CommandInfo cmdInfo in commands)
            {
               if (!cmdInfo.Name.Equals("ping"))
               {
                  embedBuilder.AddField($"{prefix}{cmdInfo.Name}", cmdInfo.Summary ?? "No description available");
               }
            }
            embedBuilder.WithFooter($"Run \"{prefix}help <command name>\" to get help for a specific command.");

            await ReplyAsync(embed: embedBuilder.Build()).ConfigureAwait(false);
         }
         else if (commands.FirstOrDefault(x => x.Name.Equals(command, StringComparison.OrdinalIgnoreCase)) != null)
         {
            CommandInfo cmdInfo = commands.FirstOrDefault(x => x.Name.Equals(command, StringComparison.OrdinalIgnoreCase));
            embedBuilder.WithTitle($"{command} Command Parameter List");
            embedBuilder.WithDescription(cmdInfo.Summary ?? "No description available");
            if (cmdInfo.Aliases.Count > 1)
            {
               string aliases = "";
               foreach (string alias in cmdInfo.Aliases)
               {
                  if (!alias.Equals(command, StringComparison.OrdinalIgnoreCase))
                  {
                     aliases += $"{alias}, ";
                  }
               }
               aliases = aliases.TrimEnd().TrimEnd(',');
               embedBuilder.AddField("Aliases:", aliases);
            }
            if (cmdInfo.Remarks != null)
            {
               embedBuilder.AddField("Additional Information:", cmdInfo.Remarks);
            }
            foreach (ParameterInfo param in cmdInfo.Parameters)
            {
               embedBuilder.AddField($"<{param.Name}>", param.Summary ?? "No description available");
            }
            if (cmdInfo.Parameters.Count == 0)
            {
               embedBuilder.WithFooter("*This command does not take any parameters.");
            }
            await ReplyAsync(embed: embedBuilder.Build()).ConfigureAwait(false);
         }
         else
         {
            await ReplyAsync($"Command \'{command}\' does not exist. Run the '.help' command to get a list of valid commands.").ConfigureAwait(false);
         }
      }
   }
}