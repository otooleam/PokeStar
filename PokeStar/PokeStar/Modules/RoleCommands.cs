using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles role assignment commands.
   /// </summary>
   public class RoleCommands : ModuleBase<SocketCommandContext>
   {
      [Command("role")]
      [Summary("Sets a user\'s nickname and role (team)")]
      [Remarks("The user will get their team role and the trainer role\n" +
               "An error will be thrown if the setup command has not yet been run, the user does not have the trainer role, or the user has a role higher than this bot\'s role.\n" +
               "It is recomended that the user\'s nick name is their in game name.")]
      public async Task Role([Summary("Set nickname and role for this user.")] IGuildUser user,
                             [Summary("User\'s nickname.")] string nickname,
                             [Summary("User's team name (Valor, Mystic, or Instinct)")] string teamName)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "P"))
         {
            if (!Connections.Instance().GetSetupComplete(Context.Guild.Id))
            {
               await ReplyAsync($"Error: Roles not setup. Please run setup command");
               return;
            }

            if (((SocketGuildUser)Context.User).Roles.FirstOrDefault(x => x.Name.ToString().Equals("Trainer", StringComparison.OrdinalIgnoreCase)) == null)
            {
               await ReplyAsync($"Error: You are not authorized to run this command.");
               return;
            }

            var team = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(teamName, StringComparison.OrdinalIgnoreCase));

            if (team == null)
            {
               await ReplyAsync($"Error: {teamName} is not a valid role");
               return;
            }

            try
            {
               await user.ModifyAsync(x => { x.Nickname = nickname; }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
               Console.WriteLine(e.Message);
               await ReplyAsync($"Warning: Unable to set nickname for {user.Username}. Please set your nickname to your in game name in \"{Context.Guild.Name}\"").ConfigureAwait(false);
            }

            var valor = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Valor", StringComparison.OrdinalIgnoreCase));
            var mystic = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Mystic", StringComparison.OrdinalIgnoreCase));
            var instinct = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Instinct", StringComparison.OrdinalIgnoreCase));
            if (user.RoleIds.Contains(valor.Id))
               await user.RemoveRoleAsync(valor);
            else if (user.RoleIds.Contains(mystic.Id))
               await user.RemoveRoleAsync(mystic);
            else if (user.RoleIds.Contains(instinct.Id))
               await user.RemoveRoleAsync(instinct);
            await user.AddRoleAsync(team);

            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Trainer", StringComparison.OrdinalIgnoreCase));
            await user.AddRoleAsync(role);

            await ReplyAsync($"{user.Username} nickname set to {nickname} and now has the Trainer and {teamName} roles");
         }
         else
            await ReplyAsync("Error: This channel is not registered to process Player Role commands.");
      }
   }
}