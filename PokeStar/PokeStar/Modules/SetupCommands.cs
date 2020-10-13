using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles bot setup commands.
   /// </summary>
   public class SetupCommands : ModuleBase<SocketCommandContext>
   {
      [Command("setup")]
      [Summary("Creates roles used by Nona.")]
      [Remarks("Roles created include Trainer, Valor, Mystic, and Instinct.\n" +
               "This needs to be run to use the role commands.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task Setup()
      {
         if (!Connections.Instance().GetSetupComplete(Context.Guild.Id))
         {
            if (Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_VALOR, StringComparison.OrdinalIgnoreCase)) == null)
            {
               await Context.Guild.CreateRoleAsync(Global.ROLE_VALOR, null, Global.ROLE_COLOR_VALOR, false, false, null);
            }
            if (Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_MYSTIC, StringComparison.OrdinalIgnoreCase)) == null)
            {
               await Context.Guild.CreateRoleAsync(Global.ROLE_MYSTIC, null, Global.ROLE_COLOR_MYSTIC, false, false, null);
            }
            if (Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_INSTINCT, StringComparison.OrdinalIgnoreCase)) == null)
            {
               await Context.Guild.CreateRoleAsync(Global.ROLE_INSTINCT, null, Global.ROLE_COLOR_INSTINCT, false, false, null);
            }
            if (Context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_TRAINER, StringComparison.OrdinalIgnoreCase)) == null)
            {
               await Context.Guild.CreateRoleAsync(Global.ROLE_TRAINER, null, Global.ROLE_COLOR_TRAINER, false, false, null);
            }
            Connections.Instance().CompleteSetup(Context.Guild.Id);
         }
         await ResponseMessage.SendInfoMessage(Context.Channel, "Setup for Nona has been complete.");
      }
   }
}