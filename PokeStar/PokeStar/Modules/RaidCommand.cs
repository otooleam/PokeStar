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
      static Dictionary<ulong, List<string>> selections = new Dictionary<ulong, List<string>>();
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
      static Emoji[] selectionEmojis = {
                new Emoji("1️⃣"),
                new Emoji("2️⃣"),
                new Emoji("3️⃣"),
                new Emoji("4️⃣"),
                new Emoji("5️⃣"),
                new Emoji("6️⃣"),
                new Emoji("7️⃣"),
                new Emoji("8️⃣"),
                new Emoji("9️⃣"),
                new Emoji("🔟")
            };

      [Command("raid")]
      public async Task Raid(short tier, string time, [Remainder]string location)
      {
         List<string> potentials = Connections.Instance().GetBossList(tier);
         string boss = null;
         if (potentials.Count != 1)
         {
            string fileName = "Egg" + tier + ".png";
            Connections.CopyFile(fileName);
            var selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
            for (int i = 0; i < potentials.Count; i++)
               await selectMsg.AddReactionAsync(selectionEmojis[i]);

            currentRaids.Add(selectMsg.Id, new Raid(tier, time, location));
            selections.Add(selectMsg.Id, potentials);

            Connections.DeleteFile(fileName);
         }
         else
         {
            boss = potentials.First<string>();
            Raid raid = new Raid(tier, time, location, boss);
            var fileName = GetPokemonPicture(raid.Boss.Name);
            Connections.CopyFile(fileName);

            var raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildEmbed(raid));
            await raidMsg.AddReactionsAsync(emojis);
            currentRaids.Add(raidMsg.Id, raid);

            Connections.DeleteFile(fileName);
         }
      }

      public static async Task RaidReaction(IMessage message, SocketReaction reaction)
      {
         Raid raid = currentRaids[message.Id];
         SocketGuildUser player = (SocketGuildUser)reaction.User;
         bool needsUpdate = true;

         if (raid.Boss == null)
         {
            bool validReactionAdded = false;
            for (int i = 0; i < selections[message.Id].Count; i++)
            {
               if (reaction.Emote.Equals(selectionEmojis[i]))
               {
                  raid.SetBoss(selections[message.Id][i]);
                  validReactionAdded = true;
               }
            }

            if (validReactionAdded)
            {
               await reaction.Channel.DeleteMessageAsync(message);

               var raidMsg = await reaction.Channel.SendFileAsync(GetPokemonPicture(raid.Boss.Name), embed: BuildEmbed(raid));
               await raidMsg.AddReactionsAsync(emojis);
               currentRaids.Add(raidMsg.Id, raid);
               needsUpdate = false;
            }
         }
         else
         {
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
               await player.SendMessageAsync(BuildRaidHelpMessage());
               needsUpdate = false;
            }
            else
               needsUpdate = false;
         }

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
         var fileName = GetPokemonPicture(raid.Boss.Name);
         Connections.CopyFile(fileName);
         EmbedBuilder embed = new EmbedBuilder();
         embed.Color = Color.DarkBlue;

         embed.WithTitle($"{raid.Boss.Name} {BuildRaidTitle(raid.Tier)}");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Time", raid.Time, true);
         embed.AddField("Location", raid.Location, true);
         embed.AddField($"Here ({raid.HereCount}/{raid.PlayerCount})", $"{BuildPlayerList(raid.Here)}");
         embed.AddField("Attending", $"{BuildPlayerList(raid.Attending)}");
         //embed.WithDescription($"Press {emojis[7]} for help");

         return embed.Build();
      }

      private static Embed BuildBossSelectEmbed(List<string> potentials, string selectPic)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < potentials.Count; i++)
         {
            sb.AppendLine($"{emojis[i]} {potentials[i]}");
         }

         EmbedBuilder embed = new EmbedBuilder();
         embed.Color = Color.DarkBlue;
         embed.WithTitle("Raid");
         embed.WithThumbnailUrl($"attachment://{selectPic}");
         embed.AddField("Please Select Boss", sb.ToString());

         return embed.Build();
      }

      private static string GetPokemonPicture(string pokemonName)
      {
         pokemonName = pokemonName.Replace(" ", "_");
         pokemonName = pokemonName.Replace(".", "");
         return pokemonName + ".png";
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

      private static string BuildRaidHelpMessage()
      {
         StringBuilder sb = new StringBuilder();

         sb.AppendLine("Raid Help:");
         sb.AppendLine("The numbers represent the number of accounts that you have with you." +
            " React with one of the numbers to show that you intend to participate in the raid");
         sb.AppendLine($"Once you arrive at the raid, react with {emojis[5]} to show others that you have arrived." +
            $" When all players have marked that they have arrived, Nona will send a message to the group");
         sb.AppendLine($"If you wish to remove yourself from the raid, react with {emojis[6]}");

         return sb.ToString();
      }

      public static bool IsCurrentRaid(ulong id)
      {
         return currentRaids.Keys.ToList<ulong>().Contains(id);
      }
   }
}
