using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeStar.Modules
{
    class RaidCommand : ModuleBase<SocketCommandContext>
    {
        [Command("raid")]
        public async Task Raid(IGuildUser user)
        {

        }
    }
}
