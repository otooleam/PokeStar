using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles role assignment commands.
   /// </summary>
   public class RoleCommands : ModuleBase<SocketCommandContext>
   {
      [Command("role")]
      [Summary("Sets a player\'s nickname and team.")]
      [Remarks("The user will get their team role and the trainer role.\n" +
               "An error will show if the setup command has not yet been run, the user does not have the trainer role, or the user has a role higher than Nona\'s role.\n" +
               "It is recomended that the user\'s nick name is their Pokémon Go trainer name.")]
      public async Task Role([Summary("Set nickname and role for this user.")] IGuildUser user,
                             [Summary("User\'s nickname.")] string nickname,
                             [Summary("User\'s team (Valor, Mystic, or Instinct)")] string teamName)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "P"))
         {
            if (Environment.GetEnvironmentVariable("SETUP_COMPLETE").Equals("FALSE", StringComparison.OrdinalIgnoreCase))
            {
               await ReplyAsync($"Error: Roles not setup. Please run the setup command").ConfigureAwait(false);
               return;
            }

            if (((SocketGuildUser)Context.User).Roles.FirstOrDefault(x => x.Name.ToString().Equals("Trainer", StringComparison.OrdinalIgnoreCase)) == null)
            {
               await ReplyAsync($"Error: You are not authorized to run this command.").ConfigureAwait(false);
               return;
            }

            SocketRole team = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(teamName, StringComparison.OrdinalIgnoreCase));

            if (team == null)
            {
               await ReplyAsync($"Error: {teamName} is not a valid role").ConfigureAwait(false);
               return;
            }

            try
            {
               await user.ModifyAsync(x => { x.Nickname = nickname; }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
               Console.WriteLine(e.Message);
               await ReplyAsync($"Warning: Unable to set nickname for {user.Username}. Please set your server nickname to match your Pokémon Go trainer name.").ConfigureAwait(false);
            }

            SocketRole valor = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Valor", StringComparison.OrdinalIgnoreCase));
            SocketRole mystic = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Mystic", StringComparison.OrdinalIgnoreCase));
            SocketRole instinct = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Instinct", StringComparison.OrdinalIgnoreCase));
            if (user.RoleIds.Contains(valor.Id))
            {
               await user.RemoveRoleAsync(valor).ConfigureAwait(false);
            }
            else if (user.RoleIds.Contains(mystic.Id))
            {
               await user.RemoveRoleAsync(mystic).ConfigureAwait(false);
            }
            else if (user.RoleIds.Contains(instinct.Id))
            {
               await user.RemoveRoleAsync(instinct).ConfigureAwait(false);
            }
            await user.AddRoleAsync(team).ConfigureAwait(false);

            SocketRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Trainer", StringComparison.OrdinalIgnoreCase));
            await user.AddRoleAsync(role).ConfigureAwait(false);

            await ReplyAsync($"{user.Username} nickname set to {nickname} and now has the \'Trainer\' and \'{teamName}\' roles").ConfigureAwait(false);
         }
         else
         {
            await ReplyAsync("Error: This channel is not registered to process Player Role commands.").ConfigureAwait(false);
         }
      }
   }
}