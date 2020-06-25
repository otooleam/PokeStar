using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PokeStar.Modules
{
    class RaidCommand : ModuleBase<SocketCommandContext>
    {
        [Command("raid")]
        public async Task Raid(IGuildUser user)
        {
            var embed = new EmbedBuilder();


            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
