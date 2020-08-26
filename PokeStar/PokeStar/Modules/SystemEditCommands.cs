using System.Threading.Tasks;
using Discord.Commands;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles system edit commands.
   /// </summary>
   public class SystemEditCommands : ModuleBase<SocketCommandContext>
   {
      [Command("prefix")]
      [Summary("Sets the command prefix for this server.")]
      [Remarks("Prefix may only be a single character long.")]
      public async Task Prefix([Summary("Prefex to set for commands.")] char prefix)
      {
         Connections.Instance().UpdatePrefix(Context.Guild.Id, prefix.ToString());
         await ReplyAsync($"Command prefix has been set to \'{prefix}\' for this server.").ConfigureAwait(false);
      }

      [Command("updatePokemonNames")]
      [Summary("Updates the saved list of pokemon names from the database.")]
      public async Task UpdatePokemonNames()
      {
         Connections.Instance().UpdateNameList();
         await ReplyAsync($"Pokemon name list has been updated.").ConfigureAwait(false);
      }
   }
}