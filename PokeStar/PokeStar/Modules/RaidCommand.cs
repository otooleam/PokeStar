using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   public class RaidCommand : ModuleBase<SocketCommandContext>
   {
      private static readonly Dictionary<ulong, Raid> currentRaids = new Dictionary<ulong, Raid>();
      private static readonly Dictionary<ulong, List<string>> selections = new Dictionary<ulong, List<string>>();
      private static readonly Emoji[] raidEmojis = {
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
         new Emoji("5️⃣"),
         new Emoji("✅"),
         new Emoji("✈️"),
         new Emoji("🚫"),
         new Emoji("❓")
      };
      private static readonly Emoji[] selectionEmojis = {
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

      private enum RAID_EMOJI_INDEX {
         ADD_PLAYER_1,
         ADD_PLAYER_2,
         ADD_PLAYER_3,
         ADD_PLAYER_4,
         ADD_PLAYER_5,
         PLAYER_HERE,
         REQUEST_INVITE,
         REMOVE_PLAYER,
         HELP
      }

      [Command("raid")]
      public async Task Raid(short tier, string time, [Remainder]string location)
      {
         if (ChannelRegisterCommand.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "R"))
         {
            List<string> potentials = Connections.GetBossList(tier);
            if (potentials.Count > 1)
            {
               string fileName = $"Egg{tier}.png";
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
               string fileName;
               Raid raid;
               if (potentials.Count == 1)
               {
                  string boss = potentials.First();
                  raid = new Raid(tier, time, location, boss);
                  fileName = Connections.GetPokemonPicture(raid.Boss.Name);
               }
               else //silph is mid-update or something else went wrong
               {
                  raid = new Raid(tier, time, location, "noboss");
                  fileName = $"Egg{tier}.png";
               }

               Connections.CopyFile(fileName);
               var raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildEmbed(raid, fileName));
               await raidMsg.AddReactionsAsync(raidEmojis);
               currentRaids.Add(raidMsg.Id, raid);

               Connections.DeleteFile(fileName);
            }
         }
         else
            await Context.Channel.SendMessageAsync("This channel is not registered for commands.");
         RemoveOldRaids();
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

               string filename = Connections.GetPokemonPicture(raid.Boss.Name);
               var raidMsg = await reaction.Channel.SendFileAsync(filename, embed: BuildEmbed(raid, filename));
               await raidMsg.AddReactionsAsync(raidEmojis);
               currentRaids.Add(raidMsg.Id, raid);
               needsUpdate = false;
            }
         }
         else
         {
            if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_1]))
            {
               raid.PlayerAdd(player, 1);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_2]))
            {
               raid.PlayerAdd(player, 2);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_3]))
            {
               raid.PlayerAdd(player, 3);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_4]))
            {
               raid.PlayerAdd(player, 4);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_5]))
            {
               raid.PlayerAdd(player, 5);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.PLAYER_HERE]))
            {
               if (raid.PlayerHere(player)) //true if all players are marked here
               {
                  await reaction.Channel.SendMessageAsync(BuildPingList(raid.Here.Keys.ToList()));
               }
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REQUEST_INVITE]))
            {
               raid.PlayerRequestInvite(player);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               raid.RemovePlayer(player);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.HELP]))
            {
               //help message - needs no update
               await player.SendMessageAsync(BuildRaidHelpMessage(message.Id));
               await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, player);
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
               x.Embed = BuildEmbed(raid, Connections.GetPokemonPicture(raid.Boss.Name));
            });
            await msg.RemoveReactionAsync(reaction.Emote, player);
         }
      }

      [Command("invite")]
      public async Task Invite(ulong id, IGuildUser player)
      {
         Raid raid = currentRaids[id];

         if (raid.InvitePlayer((SocketGuildUser)player, (SocketGuildUser)Context.User))
         {

            var message = (SocketUserMessage)Context.Channel.CachedMessages.FirstOrDefault(x => x.Id == id);
            await message.ModifyAsync(x =>
            {
               x.Embed = BuildEmbed(raid, Connections.GetPokemonPicture(raid.Boss.Name));
            });

            await player.SendMessageAsync($"You have been invited to a raid by {Context.User.Username}. Please mark yourself as \"HERE\" when ready.");
         }
      }

      private static Embed BuildEmbed(Raid raid, string fileName = null)
      {
         if (fileName != null)
            fileName = Connections.GetPokemonPicture(raid.Boss.Name);
         Connections.CopyFile(fileName);

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.DarkBlue);
         embed.WithTitle($"{(raid.Boss.Name.Equals("Bossless") ? "" : raid.Boss.Name)} {BuildRaidTitle(raid.Tier)}");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Time", raid.Time, true);
         embed.AddField("Location", raid.Location, true);
         embed.AddField($"Here ({raid.HereCount}/{raid.PlayerCount})", $"{BuildPlayerList(raid.Here)}");
         embed.AddField("Attending", $"{BuildPlayerList(raid.Attending)}");
         embed.AddField("Need Invite", $"{BuildPlayerList(raid.Invite)}");
         embed.WithDescription("Press ? for help");

         return embed.Build();
      }

      private static Embed BuildBossSelectEmbed(List<string> potentials, string selectPic)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < potentials.Count; i++)
            sb.AppendLine($"{raidEmojis[i]} {potentials[i]}");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.DarkBlue);
         embed.WithTitle("Raid");
         embed.WithThumbnailUrl($"attachment://{selectPic}");
         embed.AddField("Please Select Boss", sb.ToString());

         return embed.Build();
      }

      private static string BuildRaidTitle(int tier)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("Raid ");

         string raidSymbol = Emote.Parse(Environment.GetEnvironmentVariable("RAID_EMOTE")).ToString();

         for (int i = 0; i < tier; i++)
            sb.Append(raidSymbol);

         return sb.ToString();
      }

      private static string BuildPingList(List<SocketGuildUser> players)
      {
         StringBuilder sb = new StringBuilder();

         foreach (SocketGuildUser player in players)
            sb.Append($"{player.Mention} ");
         sb.Append("Everyone is here");
         return sb.ToString();
      }

      private static string BuildPlayerList(Dictionary<SocketGuildUser, int> list)
      {
         if (list.Count == 0)
            return "-----";

         StringBuilder sb = new StringBuilder();

         foreach (KeyValuePair<SocketGuildUser, int> player in list)
         {
            string teamString = GetPlayerTeam(player.Key);
            sb.AppendLine($"{raidEmojis[player.Value - 1]} {player.Key.Nickname ?? player.Key.Username} {teamString}");
         }

         return sb.ToString();
      }

      private static string BuildRaidHelpMessage(ulong code)
      {
         StringBuilder sb = new StringBuilder();

         sb.AppendLine("Raid Help:");
         sb.AppendLine("The numbers represent the number of accounts that you have with you." +
            " React with one of the numbers to show that you intend to participate in the raid.");
         sb.AppendLine($"Once you arrive at the raid, react with {raidEmojis[(int)RAID_EMOJI_INDEX.PLAYER_HERE]} to show others that you have arrived." +
            $" When all players have marked that they have arrived, Nona will send a message to the group.");
         sb.AppendLine($"If you need an invite to participate in the raid remotely, react with {raidEmojis[(int)RAID_EMOJI_INDEX.REQUEST_INVITE]}.");
         sb.AppendLine($"If you wish to remove yourself from the raid, react with {raidEmojis[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]}.");

         sb.AppendLine("\nRaid Invite:");
         sb.AppendLine("To invite someone to a raid through remote send the following command in raid channel:");
         sb.AppendLine($"{Environment.GetEnvironmentVariable("PREFIX_STRING")}invite {code} player");
         sb.AppendLine("Note: Change player to desired name. May be benefitial to @ player.");

         sb.AppendLine("\nRaid Edit:");
         sb.AppendLine("To edit the desired raid send the following command in raid channel:");
         sb.AppendLine($"{Environment.GetEnvironmentVariable("PREFIX_STRING")}edit {code} time location");
         sb.AppendLine("Note: Change time and location to desired time and location. Editing Location is optional.");

         return sb.ToString();
      }

      private static string GetPlayerTeam(SocketGuildUser user)
      {
         if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Valor", StringComparison.OrdinalIgnoreCase)) != null)
            return Emote.Parse(Environment.GetEnvironmentVariable("VALOR_EMOTE")).ToString();
         else if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Mystic", StringComparison.OrdinalIgnoreCase)) != null)
            return Emote.Parse(Environment.GetEnvironmentVariable("MYSTIC_EMOTE")).ToString();
         else if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Instinct", StringComparison.OrdinalIgnoreCase)) != null)
            return Emote.Parse(Environment.GetEnvironmentVariable("INSTINCT_EMOTE")).ToString();
         return "";
      }

      private static void RemoveOldRaids()
      {
         List<ulong> ids = new List<ulong>();
         foreach (var temp in currentRaids)
            if ((temp.Value.CreatedAt - DateTime.Now).TotalDays >= 1)
               ids.Add(temp.Key);
         foreach (var id in ids)
            currentRaids.Remove(id);
      }

      public static bool IsCurrentRaid(ulong id)
      {
         return currentRaids.ContainsKey(id);
      }
   }
}
