using System;
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
         await ReplyAsync(Environment.GetEnvironmentVariable("RAVE_EMOTE")).ConfigureAwait(false);
      }

      [Command("screm")]
      [Summary("AAAHHHHHHHHHHHHHHH!")]
      public async Task Screm()
      {
         await ReplyAsync(Environment.GetEnvironmentVariable("SCREAM_EMOTE")).ConfigureAwait(false);
      }
   }
}
