using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   public class HelpCommands : ModuleBase<SocketCommandContext>
   {
      [Command("help")]
      public async Task Prefix(string type = null)
      {
         await ReplyAsync(embed: SelectEmbed(type).Build()).ConfigureAwait(false);
      }

      private EmbedBuilder SelectEmbed(string type)
      {
         return HelpRegister();
      }

      private EmbedBuilder HelpGeneral()
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle("General Help");
         embed.WithDescription("Details general command details");
         embed.AddField(".ping", "General testing command", false);
         embed.WithColor(Color.Green);
         return embed;
      }
      private EmbedBuilder HelpRegister()
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle("Register Command Help");
         embed.WithDescription("Registers channels to allow for use of different functions");
         embed.AddField("Command:", ".register", false);
         embed.AddField("Param: purpose", "What to Register", false);
         embed.WithColor(Color.Green);
         return embed;
      }
      private EmbedBuilder HelpDex()
      {
         return null;
      }
      private EmbedBuilder HelpHelp()
      {
         return null;
      }
      private EmbedBuilder HelpRaid()
      {
         return null;
      }
      private EmbedBuilder HelpRole()
      {
         return null;
      }
      private EmbedBuilder HelpSetup()
      {
         return null;
      }
      private EmbedBuilder HelpSystemEdit()
      {
         return null;
      }
   }
}
