using System.Threading.Tasks;
using Discord.Commands;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles basic test commands.
   /// </summary>
   public class BasicCommands : ModuleBase<SocketCommandContext>
   {
      [Command("ping")]
      [Summary("Pong Pong Pong")]
      public async Task Ping()
      {
         await ReplyAsync("Pong").ConfigureAwait(false);
      }
   }
}