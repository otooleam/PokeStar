using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PokeStar.DataModels;

namespace PokeStar.Modules
{
   public class PlayerAccountCommands : ModuleBase<SocketCommandContext>
   {
      [Command("profile")]
      [Summary("View current profile of a user.")]
      [Remarks("Leave blank to view your own profile.")]
      public async Task Profile([Summary("(Optional) See profile for this user Must be mentioned using @.")] string user = null)
      {
         SocketGuildUser player = user == null ? (SocketGuildUser)Context.User : (SocketGuildUser)Context.Message.MentionedUsers.FirstOrDefault();

         if (player == null)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "profile", "No users found mentioned in message. Make sure you are using @.");
         }
         else
         {
            string team = "Unknown";
            foreach (SocketRole role in player.Roles)
            {
               if (team.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
               {
                  if (role.Name.Equals(Global.ROLE_VALOR, StringComparison.OrdinalIgnoreCase) ||
                      role.Name.Equals(Global.ROLE_MYSTIC, StringComparison.OrdinalIgnoreCase) ||
                      role.Name.Equals(Global.ROLE_INSTINCT, StringComparison.OrdinalIgnoreCase))
                  {
                     team = role.Name;
                  }
               }
            }

            Player p = new Player();
            await ReplyAsync(embed: BuildProfileEmbed(p, player, team));
         }
      }

      [Command("exp")]
      [Summary("Set your current total in game experiance.")]
      public async Task Exp(int totalExp)
      {
         await ReplyAsync($"Exp {totalExp}");
      }

      [Command("trainer")]
      [Summary("Set your trainer friend code.")]
      public async Task Trainer()
      {
         await ReplyAsync($"Trainer");
      }

      [Command("refer")]
      [Summary("Set your trainer referal code.")]
      public async Task Refer()
      {
         await ReplyAsync($"Refer");
      }

      [Command("country")]
      [Summary("Set the country you typically play in.")]
      public async Task Country()
      {
         await ReplyAsync($"Country");
      }

      private static Embed BuildProfileEmbed(Player player, SocketGuildUser user, string team)
      {
         string silph = user.Nickname != null ? $"https://sil.ph/{user.Nickname}" : "Unknown";

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($"Profile for {user.Nickname ?? user.Username}");
         embed.AddField("Current Discord Tag", user.Mention);
         embed.AddField("In Game Name", $"{user.Nickname ?? "Unknown"}");
         embed.AddField("Team", $"{team}");
         embed.AddField("Level", $"{player.GetLevel()}");
         embed.AddField("Trainer Code", player.TrainerCodeToString());
         embed.AddField("Referal Code", player.ReferalCode);
         embed.AddField("Country", player.Country);
         embed.AddField("Silph Card", silph);

         if (team.Equals(Global.ROLE_VALOR, StringComparison.OrdinalIgnoreCase))
         {
            embed.WithColor(Global.ROLE_COLOR_VALOR);
         }
         else if (team.Equals(Global.ROLE_MYSTIC, StringComparison.OrdinalIgnoreCase))
         {
            embed.WithColor(Global.ROLE_COLOR_MYSTIC);
         }
         else if (team.Equals(Global.ROLE_INSTINCT, StringComparison.OrdinalIgnoreCase))
         {
            embed.WithColor(Global.ROLE_COLOR_INSTINCT);
         }
         else
         {
            embed.WithColor(Global.ROLE_COLOR_TRAINER);
         }

         return embed.Build();
      }
   }
}
