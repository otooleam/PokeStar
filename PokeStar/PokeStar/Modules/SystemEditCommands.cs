using System.Threading.Tasks;
using Discord.Commands;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   public class SystemEditCommands : ModuleBase<SocketCommandContext>
   {
      [Command("prefix")]
      public async Task Prefix(char prefix = '.')
      {
         Connections.Instance().UpdatePrefix(Context.Guild.Id, prefix.ToString());
         await ReplyAsync($"Command prefix has been set to \'{prefix}\' for this server.").ConfigureAwait(false);
      }
   }
}
