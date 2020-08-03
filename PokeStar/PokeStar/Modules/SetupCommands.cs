using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PokeStar.Modules
{
   public class SetupCommands : ModuleBase<SocketCommandContext>
   {
      [Command("setup")]
      [Summary("Creates roles used by Nona.")]
      [Remarks("Roles created include Trainer, Valor, Mystic, and Instinct.\n" +
               "This needs to be run to use the role commands.")]
      public async Task Setup()
      {
         if (Environment.GetEnvironmentVariable("SETUP_COMPLETE").Equals("FALSE", StringComparison.OrdinalIgnoreCase))
         {
            if (Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Trainer", StringComparison.OrdinalIgnoreCase)) == null)
               await Context.Guild.CreateRoleAsync("Trainer", null, new Color(185, 187, 190), false, false, null).ConfigureAwait(false);
            if (Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Valor", StringComparison.OrdinalIgnoreCase)) == null)
               await Context.Guild.CreateRoleAsync("Valor", null, new Color(153, 45, 34), false, false, null).ConfigureAwait(false);
            if (Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Mystic", StringComparison.OrdinalIgnoreCase)) == null)
               await Context.Guild.CreateRoleAsync("Mystic", null, new Color(39, 126, 205), false, false, null).ConfigureAwait(false);
            if (Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Instinct", StringComparison.OrdinalIgnoreCase)) == null)
               await Context.Guild.CreateRoleAsync("Instinct", null, new Color(241, 196, 15), false, false, null).ConfigureAwait(false);
            Environment.SetEnvironmentVariable("SETUP_COMPLETE", "TRUE");
         }
         await ReplyAsync("Setup for Nona has been complete.");
      }
   }
}
