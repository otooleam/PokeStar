using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PokeStar.Modules
{
   public class RoleCommand : ModuleBase<SocketCommandContext>
   {
      [Command("role")]
      public async Task Role(IGuildUser user, string roleName)
      {
         if (Environment.GetEnvironmentVariable("SETUP_ROLES").Equals("FALSE", StringComparison.OrdinalIgnoreCase))
         {
            await ReplyAsync($"Error: Roles not setup. Please run setup command");
            return;
         }

         var team = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(roleName, StringComparison.OrdinalIgnoreCase));

         if (team == null)
         {
            await ReplyAsync($"Error: {roleName} is not a valid role");
            return;
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

         await ReplyAsync($"{user.Username} now has the Trainer role and the {roleName} role");
      }
   }
}
