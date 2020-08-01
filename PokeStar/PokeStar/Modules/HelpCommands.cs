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
      [Summary("Displays info about commands")]
      public async Task Help([Summary("(Optional) Get help for given command.")] string type = null)
      {
         List<CommandInfo> commands = Program.GetCommands();
         EmbedBuilder embedBuilder = new EmbedBuilder();
         embedBuilder.WithColor(Color.Green);

         if (type == null)
         {
            embedBuilder.WithTitle("Command List");
            embedBuilder.WithDescription("List of commands supported by this bot.");
            foreach (CommandInfo command in commands)
               embedBuilder.AddField(command.Name, command.Summary ?? "No description available\n");
            await ReplyAsync(embed: embedBuilder.Build());
         }
         else if (commands.FirstOrDefault(x => x.Name.Equals(type, StringComparison.OrdinalIgnoreCase)) != null)
         {
            embedBuilder.WithTitle($"{type.ToLower()} Command Parameter List");
            embedBuilder.WithDescription("List of parameters for the command.");
            CommandInfo command = commands.FirstOrDefault(x => x.Name.Equals(type, StringComparison.OrdinalIgnoreCase));
            foreach (var param in command.Parameters)
               embedBuilder.AddField(param.Name, param.Summary ?? "No description available\n");
            if (command.Parameters.Count == 0)
               embedBuilder.WithFooter("This command does not take any parameters.");
            await ReplyAsync(embed: embedBuilder.Build());
         }
         else
            await ReplyAsync($"Command \'{type}\' does not exist.");
      }
   }
}
