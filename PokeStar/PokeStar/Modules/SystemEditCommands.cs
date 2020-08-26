using System.Threading.Tasks;
using Discord.Commands;
using PokeStar.ConnectionInterface;
using PokeStar.DataModels;

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
         await ResponseMessage.SendInfoMessage(Context, $"Command prefix has been set to \'{prefix}\' for this server.");
      }

      [Command("updatePokemonNames")]
      [Summary("Updates the saved list of pokemon names from the database.")]
      public async Task UpdatePokemonNames()
      {
         Connections.Instance().UpdateNameList();
         await ResponseMessage.SendInfoMessage(Context, $"Pokemon name list has been updated.");
      }
   }
}