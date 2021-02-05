using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;
using PokeStar.PreConditions;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles role assignment commands.
   /// </summary>
   public class RoleCommands : ModuleBase<SocketCommandContext>
   {
      /// <summary>
      /// Handles role command.
      /// </summary>
      /// <param name="user">Set nickname and role for this user.</param>
      /// <param name="nickname">User's nickname.</param>
      /// <param name="teamName">User's team (Valor, Mystic, or Instinct)</param>
      /// <returns>Completed Task.</returns>
      [Command("role")]
      [Summary("Sets a player\'s nickname and team.")]
      [Remarks("The user will get their team role and the trainer role.\n" +
               "An error will show if the setup command has not yet been run, the user does not have the trainer role, or the user has a role higher than Nona\'s role.\n" +
               "It is recommended that the user\'s nickname is their Pokémon Go trainer name.")]
      [RegisterChannel('P')]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task Role([Summary("Set nickname and role for this user.")] IGuildUser user,
                             [Summary("User\'s nickname.")] string nickname,
                             [Summary("User\'s team (Valor, Mystic, or Instinct).")] string teamName)
      {
         if (!Connections.Instance().GetSetupComplete(Context.Guild.Id))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "role", "Roles not setup. Please run the setup command");
         }
         else if (((SocketGuildUser)Context.User).Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_TRAINER, StringComparison.OrdinalIgnoreCase)) == null)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "role", "Error: You are not authorized to run this command.");
         }
         else
         {
            SocketRole team = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(teamName, StringComparison.OrdinalIgnoreCase));
            if (team == null)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "role", $"{teamName} is not a valid role");
            }
            else
            {
               try
               {
                  await user.ModifyAsync(x => { x.Nickname = nickname; });
               }
               catch (Discord.Net.HttpException e)
               {
                  Console.WriteLine(e.Message);
                  await ResponseMessage.SendWarningMessage(Context.Channel, "role", $"Unable to set nickname for {user.Username}.\nPlease set your server nickname to match your Pokémon Go trainer name.");
               }

               SocketRole valor = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_VALOR, StringComparison.OrdinalIgnoreCase));
               SocketRole mystic = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_MYSTIC, StringComparison.OrdinalIgnoreCase));
               SocketRole instinct = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_INSTINCT, StringComparison.OrdinalIgnoreCase));
               if (user.RoleIds.Contains(valor.Id))
               {
                  await user.RemoveRoleAsync(valor);
               }
               else if (user.RoleIds.Contains(mystic.Id))
               {
                  await user.RemoveRoleAsync(mystic);
               }
               else if (user.RoleIds.Contains(instinct.Id))
               {
                  await user.RemoveRoleAsync(instinct);
               }
               await user.AddRoleAsync(team);

               SocketRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_TRAINER, StringComparison.OrdinalIgnoreCase));
               await user.AddRoleAsync(role);

               await ResponseMessage.SendInfoMessage(Context.Channel, $"{user.Username} nickname set to {nickname} and now has the \'Trainer\' and \'{teamName}\' roles");
            }
         }
      }
   }
}