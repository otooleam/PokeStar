using System.Threading.Tasks;
using Discord.Commands;
using PokeStar.DataModels;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles basic test commands.
   /// </summary>
   public class BasicCommands : ModuleBase<SocketCommandContext>
   {
      [Command("ping")]
      [Summary("Pong Pong Pong")]
      public async Task Ping() => await ResponseMessage.SendInfoMessage(Context, "Pong").ConfigureAwait(false);
   }
}