using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PokeStar.Modules
{
   public class SetupCommand : ModuleBase<SocketCommandContext>
   {
      [Command("setup")]
      public async Task Setup(string subsystem = "ALL")
      {
         if (Environment.GetEnvironmentVariable("SETUP_ROLES").Equals("FALSE", StringComparison.OrdinalIgnoreCase))
         {
            Environment.SetEnvironmentVariable("SETUP_ROLES", "TRUE");

            if (Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Trainer", StringComparison.OrdinalIgnoreCase)) == null)
            {
               await Context.Guild.CreateRoleAsync("Valor", null, new Color(153, 45, 34), false, false, null).ConfigureAwait(false);
               await Context.Guild.CreateRoleAsync("Mystic", null, new Color(39, 126, 205), false, false, null).ConfigureAwait(false);
               await Context.Guild.CreateRoleAsync("Instinct", null, new Color(241, 196, 15), false, false, null).ConfigureAwait(false);
               await Context.Guild.CreateRoleAsync("Trainer", null, new Color(185, 187, 190), false, false, null).ConfigureAwait(false);
            }
            Discord.Rest.RestCategoryChannel restCategory = null;

            if (Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name.ToString().Equals("back room", StringComparison.OrdinalIgnoreCase)) == null)
            {
               restCategory = await Context.Guild.CreateCategoryChannelAsync("Back Room", null, null).ConfigureAwait(false);
            }

            if (Context.Guild.Channels.FirstOrDefault(x => x.Name.ToString().Equals("verification", StringComparison.OrdinalIgnoreCase)) == null)
            {
               if (restCategory == null)
               {
                  var socketCategory = Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name.ToString().Equals("Back Room", StringComparison.OrdinalIgnoreCase));
                  await Context.Guild.CreateTextChannelAsync("Verification", prop => prop.CategoryId = socketCategory.Id, null).ConfigureAwait(false);
               }
               else
                  await Context.Guild.CreateTextChannelAsync("Verification", prop => prop.CategoryId = restCategory.Id, null).ConfigureAwait(false);
            }

            if (Environment.GetEnvironmentVariable("SETUP_RAIDS").Equals("FALSE", StringComparison.OrdinalIgnoreCase) &&
               (subsystem.Equals("ALL", StringComparison.OrdinalIgnoreCase) || subsystem.Equals("RAID", StringComparison.OrdinalIgnoreCase)))
            {
               Environment.SetEnvironmentVariable("SETUP_RAIDS", "TRUE");
            }

            if (Environment.GetEnvironmentVariable("SETUP_TRADE").Equals("FALSE", StringComparison.OrdinalIgnoreCase) &&
               (subsystem.Equals("ALL", StringComparison.OrdinalIgnoreCase) || subsystem.Equals("TRADE", StringComparison.OrdinalIgnoreCase)))
            {
               Environment.SetEnvironmentVariable("SETUP_TRADE", "TRUE");
            }

            if (Environment.GetEnvironmentVariable("SETUP_DEX").Equals("FALSE", StringComparison.OrdinalIgnoreCase) &&
               (subsystem.Equals("ALL", StringComparison.OrdinalIgnoreCase) || subsystem.Equals("DEX", StringComparison.OrdinalIgnoreCase)))
            {
               Environment.SetEnvironmentVariable("SETUP_DEX", "TRUE");
            }

         }
      }
   }
}
