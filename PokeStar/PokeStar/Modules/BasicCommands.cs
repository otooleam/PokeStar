using System;
using System.Threading.Tasks;
using Discord;
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
      public async Task Ping() => await ResponseMessage.SendInfoMessage(Context.Channel, "Pong");

      [Command("marco")]
      [Summary("Play marco polo.")]
      public async Task Marco() => await ResponseMessage.SendInfoMessage(Context.Channel, "Polo!");

      [Command("status")]
      [Summary("Get Nona's status.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task Status()
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle(Program.GetName());
         embed.WithDescription($"Currently running version v{Global.VERSION}");
         embed.AddField("Latency", $"{Program.GetLatency()}ms");
         embed.AddField("Status", Program.GetStatus());
         embed.AddField("Connection", Program.GetConnectionState());

         if (Context.Guild.Name.Equals(Global.HOME_SERVER, StringComparison.OrdinalIgnoreCase))
         {
            embed.AddField("Guild Count", Program.GetGuildCount());
            embed.AddField("Using empty raid", Global.USE_EMPTY_RAID);
            embed.AddField("Accept from Nona Test Bot", Global.USE_NONA_TEST);
         }
         await ReplyAsync(embed: embed.Build());
      }
   }
}