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
      static Dictionary<ulong, Raid> currentRaids = new Dictionary<ulong, Raid>();
      static Emoji[] emojis = {
                new Emoji("1️⃣"),
                new Emoji("2️⃣"),
                new Emoji("3️⃣"),
                new Emoji("4️⃣"),
                new Emoji("5️⃣"),
                new Emoji("✅"),
                new Emoji("🚫"),
                new Emoji("❓")
            };

      public static bool IsCurrentRaid(ulong id)
      {
         return currentRaids.Keys.ToList<ulong>().Contains(id);
      }

      [Command("raid")]
      public async Task Raid(short tier, string time, [Remainder]string location)
      {
         Raid raid = new Raid(tier, time, location);

         var raidMsg = await Context.Channel.SendMessageAsync("", false, BuildEmbed(raid));
         await raidMsg.AddReactionsAsync(emojis);
         currentRaids.Add(raidMsg.Id, raid);
      }

      public static async Task RaidReaction(IMessage message, SocketReaction reaction)
      {
         Raid raid = currentRaids[message.Id];
         SocketGuildUser player = (SocketGuildUser)reaction.User;
         bool needsUpdate = true;

         if (reaction.Emote.Equals(emojis[0]))
         {
            raid.PlayerAdd(player, 1);
         }
         else if (reaction.Emote.Equals(emojis[1]))
         {
            raid.PlayerAdd(player, 2);
         }
         else if (reaction.Emote.Equals(emojis[2]))
         {
            raid.PlayerAdd(player, 3);
         }
         else if (reaction.Emote.Equals(emojis[3]))
         {
            raid.PlayerAdd(player, 4);
         }
         else if (reaction.Emote.Equals(emojis[4]))
         {
            raid.PlayerAdd(player, 5);
         }
         else if (reaction.Emote.Equals(emojis[5]))
         {
            if (raid.PlayerHere(player)) //true if all players are marked here
            {
               await reaction.Channel.SendMessageAsync(BuildPingList(raid.Here.Keys.ToList<SocketGuildUser>()));
            }
         }
         else if (reaction.Emote.Equals(emojis[6]))
         {
            raid.RemovePlayer(player);
         }
         else if (reaction.Emote.Equals(emojis[7]))
         {
            //help message - needs no update
            //not implemented
            needsUpdate = false;
         }
         else
            needsUpdate = false;

         if (needsUpdate)
         {
            var msg = (SocketUserMessage)message;
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildEmbed(raid);
            });
         }
      }

      private static Embed BuildEmbed(Raid raid)
      {
         EmbedBuilder embed = new EmbedBuilder();

         embed.WithTitle($"{BuildRaidTitle(raid.Tier)}");
         embed.AddField("Time", raid.Time, true);
         embed.AddField("Location", raid.Location, true);
         embed.AddField($"Here ({raid.HereCount}/{raid.PlayerCount})", $"{BuildPlayerList(raid.Here)}");
         embed.AddField("Attending", $"{BuildPlayerList(raid.Attending)}");
         embed.WithDescription($"Press {emojis[7]} for help");

         return embed.Build();
      }

      private static string BuildRaidTitle(int tier)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("Raid ");

         for (int i = 0; i < tier; i++)
            sb.Append("⭐"); 

         return sb.ToString();
      }

      private static string BuildPingList(List<SocketGuildUser> players)
      {
         StringBuilder sb = new StringBuilder();

         foreach (SocketGuildUser player in players)
         {
            sb.Append(player.Mention);
            sb.Append(" ");
         }
         sb.Append("Everyone is here");
         return sb.ToString();
      }

      private static string BuildPlayerList(Dictionary<SocketGuildUser, int> list)
      {
         if (list.Count == 0)
            return "\0";

         StringBuilder sb = new StringBuilder();

         foreach (KeyValuePair<SocketGuildUser, int> player in list)
            sb.AppendLine($"{emojis[player.Value - 1]} {(player.Key.Nickname != null ? player.Key.Nickname : player.Key.Username)}");

         return sb.ToString();
      }
   }
}
