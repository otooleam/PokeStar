using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;
using Discord.WebSocket;

namespace PokeStar.Modules
{
   public class RaidCommand : ModuleBase<SocketCommandContext>
   {
      int attendingCount = 0;
      int hereCount = 0;
      Dictionary<string, int> attending;
      Dictionary<string, int> here = new Dictionary<string, int>();
      Emoji[] emojis = {
                new Emoji("1️⃣"),
                new Emoji("2️⃣"),
                new Emoji("3️⃣"),
                new Emoji("4️⃣"),
                new Emoji("5️⃣"),
                new Emoji("✅"),
                new Emoji("🚫"),
                new Emoji("❓")
            };

      [Command("raid")]
      public async Task Raid(int tier, string time, [Remainder]string location)
      {
         if (tier > 5)
            tier = 5;

         EmbedBuilder embed = new EmbedBuilder();
         attending = new Dictionary<string, int>();

         embed.WithTitle($"{BuildRaidTitle(tier)}");
         embed.AddField("Time", time, true);
         embed.AddField("Location", location, true);
         embed.AddField($"Here ({hereCount}/{attendingCount})", $"{BuildPlayerList(here)}");
         embed.AddField("Attending", $"{BuildPlayerList(attending)}");

         var raidMsg = await Context.Channel.SendMessageAsync("", false, embed.Build());
         await raidMsg.AddReactionsAsync(emojis);
      }
      
      public static async Task RaidAddPlayerGoing(IMessage msg, SocketReaction reaction)
      {

      }

      private string BuildRaidTitle(int tier)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("Raid ");

         for (int i = 0; i < tier; i++)
            sb.Append("🟊");

         return sb.ToString();
      }

      private string BuildPlayerList(Dictionary<string, int> list)
      {
         if (list.Count == 0)
            return "\0";

         StringBuilder sb = new StringBuilder();

         foreach (KeyValuePair<string, int> player in list)
            sb.AppendLine($"{emojis[player.Value - 1]} {player.Key}");

         return sb.ToString();
      }
   }
}
