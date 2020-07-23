using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeStar.Modules
{
   class ChannelRegisterCommand : ModuleBase<SocketCommandContext>
   {
      public static Dictionary<ulong, List<ulong>> registeredChannels = new Dictionary<ulong, List<ulong>>();

      //Registers the channel this command is run in as a command channel
      [Command("register channel")]
      public async Task Register(string purpose = null)
      {
         ulong guild = Context.Guild.Id;
         ulong channel = Context.Channel.Id;

         if (registeredChannels.Keys.Contains(guild))
            registeredChannels[guild].Add(channel);
         else
         {
            registeredChannels.Add(guild, new List<ulong> { channel });
         }
      }
   }
}
