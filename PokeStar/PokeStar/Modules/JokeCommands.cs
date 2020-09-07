using System.Threading.Tasks;
using Discord.Commands;

namespace PokeStar.Modules
{
   public class JokeCommands : ModuleBase<SocketCommandContext>
   {
      [Command("rave")]
      [Summary("Its time for a rave.")]
      public async Task Rave()
      {
         await ReplyAsync(Global.NONA_EMOJIS["rave_emote"]);
      }

      [Command("screm")]
      [Summary("AAAHHHHHHHHHHHHHHH!")]
      public async Task Screm()
      {
         await ReplyAsync(Global.NONA_EMOJIS["scream_emote"]);
      }
   }
}