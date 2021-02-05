using System.Threading.Tasks;
using Discord.Commands;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles joke commands.
   /// </summary>
   public class JokeCommands : ModuleBase<SocketCommandContext>
   {
      /// <summary>
      /// Handle rave command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("rave")]
      [Summary("Its time for a rave.")]
      public async Task Rave()
      {
         await ReplyAsync(Global.NONA_EMOJIS["rave_emote"]);
      }

      /// <summary>
      /// Handle screm command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("screm")]
      [Summary("AAAHHHHHHHHHHHHHHH!")]
      public async Task Screm()
      {
         await ReplyAsync(Global.NONA_EMOJIS["scream_emote"]);
      }
   }
}