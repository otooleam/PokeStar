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
      [Command("prefix")]
      [Summary("Sets the command prefix for this server.")]
      [Remarks("Prefix may only be a single character long.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task Prefix([Summary("Prefex to set for commands.")] char prefix)
      {
         Connections.Instance().UpdatePrefix(Context.Guild.Id, prefix.ToString());
         await ResponseMessage.SendInfoMessage(Context.Channel, $"Command prefix has been set to \'{prefix}\' for this server.");
      }

      [Command("updatePokemonNames")]
      [Summary("Updates the saved list of Pokémon names from the database.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      [NonaAdmin()]
      public async Task UpdatePokemonNames()
      {
         Connections.Instance().UpdatePokemonNameList();
         await ResponseMessage.SendInfoMessage(Context, $"Pokémon name list has been updated.");
      }

      [Command("updateMovenNames")]
      [Summary("Updates the saved list of Move names from the database.")]
      public async Task UpdateMoveNames()
      {
         Connections.Instance().UpdateMoveNameList();
         await ResponseMessage.SendInfoMessage(Context, $"Move name list has been updated.");
      }

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

      [Command("toggleUseNonaTest")]
      [Summary("Toggle accepting messages from Nona Test Bot")]
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