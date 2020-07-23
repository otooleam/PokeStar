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
   public class ChannelRegisterCommand : ModuleBase<SocketCommandContext>
   {
      public static Dictionary<ulong, List<ulong>> registeredChannels = new Dictionary<ulong, List<ulong>>();

      public static bool IsRegisteredChannel(ulong guild, ulong channel)
      {
         return registeredChannels[guild].Contains(channel);
      }

      // Registers the channel this command is run in as a command channel
      // Requires .setup to have been run
      [Command("register channel")]
      public async Task Register(string purpose = null)
      {
         ulong guild = Context.Guild.Id;
         ulong channel = Context.Channel.Id;

         if (registeredChannels.Keys.Contains(guild))
         {
            registeredChannels[guild].Add(channel);
            await Context.Channel.SendMessageAsync("Channel registeration complete");
         }
         else
            await Context.Channel.SendMessageAsync("Please run .setup for this server before registering command channels").ConfigureAwait(false);
      }
   }
}
