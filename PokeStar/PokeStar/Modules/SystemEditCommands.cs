using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.PreConditions;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles system edit commands.
   /// </summary>
   public class SystemEditCommands : ModuleBase<SocketCommandContext>
   {
      /// <summary>
      /// Handle prefix command.
      /// </summary>
      /// <param name="prefix">Prefex to set for commands.</param>
      /// <returns>Completed Task.</returns>
      [Command("prefix")]
      [Summary("Sets the command prefix for this server.")]
      [Remarks("Prefix may only be a single character long.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task Prefix([Summary("Prefex to set for commands.")] char prefix)
      {
         Connections.Instance().UpdatePrefix(Context.Guild.Id, prefix.ToString());
         await ResponseMessage.SendInfoMessage(Context.Channel, $"Command prefix has been set to \'{prefix}\' for this server.");
      }

      /// <summary>
      /// Handle toggleUseEmptyRaid command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("toggleUseEmptyRaid")]
      [Summary("Toggle empty raid feature for all servers.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      [NonaAdmin()]
      public async Task UseEmptyRaid()
      {
         Global.USE_EMPTY_RAID = !Global.USE_EMPTY_RAID;
         string text = Global.USE_EMPTY_RAID ? "" : "not";
         await ResponseMessage.SendInfoMessage(Context.Channel, $"Nona will {text} use the empty raid feature.");
      }

      /// <summary>
      /// Handle toggleUseNonaTest command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("toggleUseNonaTest")]
      [Summary("Toggle accepting messages from Nona Test Bot.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      [NonaAdmin()]
      public async Task UseNonaTest()
      {
         Global.USE_NONA_TEST = !Global.USE_NONA_TEST;
         string text = Global.USE_NONA_TEST ? "" : "not";
         await ResponseMessage.SendInfoMessage(Context.Channel, $"Nona will {text} accept message from a Nona Test Bot.");
      }
   }
}