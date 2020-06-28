using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
    public class RaidCommand : ModuleBase<SocketCommandContext>
    {
        [Command("raid")]
        public async Task Raid(int tier, string time, string location)
        {
            int attendingCount = 0;
            int hereCount = 0;
            string attending = "\0";
            string here = "\0";

            EmbedBuilder embed = new EmbedBuilder();
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

            embed.WithTitle($"Raid");
            embed.AddField("Time", time, true);
            embed.AddField("Location", location, true);
            embed.AddField($"Here ({hereCount}/{attendingCount})", attending);
            embed.AddField("Attending", here);

            var raid = await Context.Channel.SendMessageAsync("", false, embed.Build());
            await raid.AddReactionsAsync(emojis);
            
        }
    }
}
