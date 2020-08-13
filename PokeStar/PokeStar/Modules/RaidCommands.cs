using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles raid commands.
   /// </summary>
   public class RaidCommands : ModuleBase<SocketCommandContext>
   {
      private static readonly Dictionary<ulong, Raid> currentRaids = new Dictionary<ulong, Raid>();
      private static readonly Dictionary<ulong, ulong> raidMessages = new Dictionary<ulong, ulong>();

      private static readonly Emoji[] raidEmojis = {
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
         new Emoji("5️⃣"),
         new Emoji("✅"),
         new Emoji("✈️"),
         new Emoji("🤝"),
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

      private static readonly Emoji cancelEmoji = new Emoji("🚫");

      private enum RAID_EMOJI_INDEX
      {
         ADD_PLAYER_1,
         ADD_PLAYER_2,
         ADD_PLAYER_3,
         ADD_PLAYER_4,
         ADD_PLAYER_5,
         PLAYER_READY,
         REQUEST_INVITE,
         INVITE_PLAYER,
         REMOVE_PLAYER,
         HELP
      }

      [Command("raid")]
      [Summary("Creates a new Raid message.")]
      public async Task Raid([Summary("Tier of the raid.")] short tier,
                             [Summary("Time the raid will start.")] string time,
                             [Summary("Where the raid will be.")][Remainder] string location)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "R"))
         {
            List<string> potentials = Connections.GetBossList(tier);
            if (potentials.Count > 1)
            {
               string fileName = $"Egg{tier}.png";
               Connections.CopyFile(fileName);

               var selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
               for (int i = 0; i < potentials.Count; i++)
                  await selectMsg.AddReactionAsync(selectionEmojis[i]);

               Raid raid = new Raid(tier, time, location)
               {
                  RaidBossSelections = potentials
               };
               currentRaids.Add(selectMsg.Id, raid);
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
               var raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
               await raidMsg.AddReactionsAsync(raidEmojis);
               currentRaids.Add(raidMsg.Id, raid);

               Connections.DeleteFile(fileName);
            }
         }
         else
            await Context.Channel.SendMessageAsync("This channel is not registered to process Raid commands.");
         RemoveOldRaids();
      }

      [Command("edit")]
      [Summary("Edit the tier, time, or location of a Raid message.")]
      public async Task Edit([Summary("Raid code given by the help message.")] ulong code,
                             [Summary("Raid attribute to change.")] string attribute,
                             [Summary("New value of the raid attribute.")][Remainder] string edit)
      {

         if (ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "R"))
         {
            if (currentRaids.ContainsKey(code))
            {
               Raid raid = currentRaids[code];
               if (attribute.Equals("time", StringComparison.OrdinalIgnoreCase))
               {
                  raid.Time = edit;
                  var msg = (SocketUserMessage)Context.Channel.GetCachedMessage(code);
                  await msg.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidEmbed(raid, Connections.GetPokemonPicture(raid.Boss.Name));
                  });
               }
               else if (attribute.Equals("location", StringComparison.OrdinalIgnoreCase) ||
                        attribute.Equals("loc", StringComparison.OrdinalIgnoreCase))
               {
                  raid.Location = edit;
                  var msg = (SocketUserMessage)Context.Channel.GetCachedMessage(code);
                  await msg.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidEmbed(raid, Connections.GetPokemonPicture(raid.Boss.Name));
                  });
               }
               else if (attribute.Equals("tier", StringComparison.OrdinalIgnoreCase) ||
                        attribute.Equals("boss", StringComparison.OrdinalIgnoreCase))
               {
                  // TODO: Add editing of raid tier/boss
               }
            }
         }
         else
            await Context.Channel.SendMessageAsync("This channel is not registered to process Raid commands.");
      }

      /// <summary>
      /// Handles a reaction on a raid message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      public static async Task RaidReaction(IMessage message, SocketReaction reaction)
      {
         Raid raid = currentRaids[message.Id];
         SocketGuildUser player = (SocketGuildUser)reaction.User;
         bool needsUpdate = true;

         if (raid.Boss == null)
         {
            bool validReactionAdded = false;
            for (int i = 0; i < raid.RaidBossSelections.Count; i++)
            {
               if (reaction.Emote.Equals(selectionEmojis[i]))
               {
                  raid.SetBoss(raid.RaidBossSelections[i]);
                  validReactionAdded = true;
               }
            }

            if (validReactionAdded)
            {
               await reaction.Channel.DeleteMessageAsync(message);

               string filename = Connections.GetPokemonPicture(raid.Boss.Name);
               var raidMsg = await reaction.Channel.SendFileAsync(filename, embed: BuildRaidEmbed(raid, filename));
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
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.PLAYER_READY]))
            {
               var group = raid.PlayerReady(player);
               if (group != -1)
                  await reaction.Channel.SendMessageAsync(BuildPingList(raid.Groups.ElementAt(group).GetPingList(), raid.Location, group));
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REQUEST_INVITE]))
            {
               raid.PlayerRequestInvite(player);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.INVITE_PLAYER]))
            {
               if (raid.IsInRaid(player, false) != -1)
               {
                  if (raid.GetReadonlyInvite().Count == 0)
                     await reaction.Channel.SendMessageAsync($"{player.Mention}, There are no players to invite.");
                  else
                  {
                     var inviteMsg = await reaction.Channel.SendMessageAsync(text: $"{player.Mention}", embed: BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), (player.Nickname == null ? player.Username : player.Nickname)));
                     for (int i = 0; i < raid.GetReadonlyInvite().Count; i++)
                        await inviteMsg.AddReactionAsync(selectionEmojis[i]);
                     await inviteMsg.AddReactionAsync(cancelEmoji);
                     raidMessages.Add(inviteMsg.Id, message.Id);
                  }
               }
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               int group = raid.RemovePlayer(player);
               if (group != -1)
                  await reaction.Channel.SendMessageAsync(BuildPingList(raid.Groups.ElementAt(group).GetPingList(), raid.Location, group));
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.HELP]))
            {
               string prefix = Connections.Instance().GetPrefix(((SocketGuildChannel)message.Channel).Guild.Id);
               if (prefix == null)
                  prefix = Environment.GetEnvironmentVariable("DEFAULT_PREFIX");

               await player.SendMessageAsync(BuildRaidHelpMessage());
               await player.SendMessageAsync($"{prefix}edit {message.Id}");
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
               x.Embed = BuildRaidEmbed(raid, Connections.GetPokemonPicture(raid.Boss.Name));
            });
         }
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, player);
      }

      /// <summary>
      /// Handles a reaction on a raid invite message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="channel">Channel the reaction happened in</param>
      /// <returns>Completed Task.</returns>
      public static async Task RaidInviteReaction(IMessage message, SocketReaction reaction, ISocketMessageChannel channel)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         var raidMessageId = raidMessages[message.Id];
         Raid raid = currentRaids[raidMessageId];
         if (reaction.Emote.Equals(cancelEmoji))
         {
            await message.DeleteAsync();
            return;
         }
         for (int i = 0; i < raid.GetReadonlyInvite().Count; i++)
         {
            if (reaction.Emote.Equals(selectionEmojis.ElementAt(i)))
            {
               var player = raid.GetReadonlyInvite().Keys.ElementAt(i);
               if (raid.InvitePlayer(player, (SocketGuildUser)reaction.User))
               {
                  var raidMessage = (SocketUserMessage)channel.CachedMessages.FirstOrDefault(x => x.Id == raidMessageId);
                  await raidMessage.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidEmbed(raid, Connections.GetPokemonPicture(raid.Boss.Name));
                  });

                  SocketGuildUser invitingPlayer = (SocketGuildUser)reaction.User.Value;
                  await player.SendMessageAsync($"You have been invited to a raid by {(invitingPlayer.Nickname == null ? invitingPlayer.Username : invitingPlayer.Nickname)}.");
                  raidMessages.Remove(message.Id);
                  await message.DeleteAsync();
               }
               return;
            }
         }
      }

      /// <summary>
      /// Builds the raid embed.
      /// </summary>
      /// <param name="raid">Raid to display in the embed.</param>
      /// <param name="fileName">Name of picture to use for the raid.</param>
      /// <returns>Embed for viewing a raid.</returns>
      private static Embed BuildRaidEmbed(Raid raid, string fileName = null)
      {
         if (fileName != null)
            fileName = Connections.GetPokemonPicture(raid.Boss.Name);
         Connections.CopyFile(fileName);

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.DarkBlue);
         embed.WithTitle($"{(raid.Boss.Name.Equals("Bossless") ? "" : raid.Boss.Name)} {BuildRaidTitle(raid.Tier)}");
         embed.WithDescription("Press ? for help.");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Time", raid.Time, true);
         embed.AddField("Location", raid.Location, true);

         for (int i = 0; i < raid.Groups.Count; i++)
         {
            embed.AddField($"Group {i + 1} Ready ({raid.Groups.ElementAt(i).GetHereCount()}/{raid.Groups.ElementAt(i).TotalPlayers()})", $"{BuildPlayerList(raid.Groups.ElementAt(i).GetReadonlyHere())}");
            embed.AddField($"Group {i + 1} Attending", $"{BuildPlayerList(raid.Groups.ElementAt(i).GetReadonlyAttending())}");
            embed.AddField($"Group {i + 1} Invited", $"{BuildInvitedList(raid.Groups.ElementAt(i).GetReadonlyInvited())}");
         }
         embed.AddField($"Need Invite:", $"{BuildPlayerList(raid.GetReadonlyInvite())}");
         embed.WithFooter("Note: the max number of members in a raid is 20, and the max number of invites is 10.");

         return embed.Build();
      }

      /// <summary>
      /// Builds the raid boss select embed.
      /// </summary>
      /// <param name="potentials">List of potential raid bosses.</param>
      /// <param name="selectPic">Name of picture file to get.</param>
      /// <returns>Embed for selecting raid boss.</returns>
      private static Embed BuildBossSelectEmbed(List<string> potentials, string selectPic)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < potentials.Count; i++)
            sb.AppendLine($"{selectionEmojis[i]} {potentials[i]}");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.DarkBlue);
         embed.WithTitle("Raid");
         embed.WithThumbnailUrl($"attachment://{selectPic}");
         embed.AddField("Please Select Boss", sb.ToString());
         return embed.Build();
      }

      /// <summary>
      /// Builds the raid invite embed.
      /// </summary>
      /// <param name="invite">List of users to invite.</param>
      /// <param name="user">User that wants to invite someone.</param>
      /// <returns>Embed for inviting a player to a raid.</returns>
      private static Embed BuildPlayerInviteEmbed(ImmutableList<SocketGuildUser> invite, string user)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < invite.Count; i++)
         {
            sb.AppendLine($"{raidEmojis[i]} {(invite.ElementAt(i).Nickname == null ? invite.ElementAt(i).Username : invite.ElementAt(i).Nickname)}");
         }
         sb.AppendLine($"{cancelEmoji} Cancel");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.DarkBlue);
         embed.WithTitle($"{user} - Invite");
         embed.AddField("Please Select Player to invite", sb.ToString());

         return embed.Build();
      }

      /// <summary>
      /// Builds the title of the raid.
      /// </summary>
      /// <param name="tier">Raid tier.</param>
      /// <returns>Raid title as a string.</returns>
      private static string BuildRaidTitle(int tier)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("Raid ");
         string raidSymbol = Emote.Parse(Environment.GetEnvironmentVariable("RAID_EMOTE")).ToString();
         for (int i = 0; i < tier; i++)
            sb.Append(raidSymbol);
         return sb.ToString();
      }

      /// <summary>
      /// Builds a list of players to ping when a group is ready.
      /// </summary>
      /// <param name="players">List of players to ping.</param>
      /// <param name="location">Location of the raid.</param>
      /// <param name="groupNumber">Group number the players are part of.</param>
      /// <returns>List of players to ping as a string.</returns>
      private static string BuildPingList(List<SocketGuildUser> players, string location, int groupNumber)
      {
         StringBuilder sb = new StringBuilder();
         foreach (SocketGuildUser player in players)
            sb.Append($"{player.Mention} ");
         sb.Append($"Everyone in group {groupNumber + 1} is ready at {location}");
         return sb.ToString();
      }

      /// <summary>
      /// Builds a list of players in a raid.
      /// </summary>
      /// <param name="players">Dictionary of players and the number of accounts they are bringing.</param>
      /// <returns>List of players as a string.</returns>
      private static string BuildPlayerList(ImmutableDictionary<SocketGuildUser, int> players)
      {
         if (players.Count == 0)
            return "-----";

         StringBuilder sb = new StringBuilder();
         foreach (var player in players)
         {
            string teamString = GetPlayerTeam(player.Key);
            sb.AppendLine($"{raidEmojis[player.Value - 1]} {player.Key.Nickname ?? player.Key.Username} {teamString}");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Builds the invite list for a raid group.
      /// </summary>
      /// <param name="players">Dictionary of invited players and who invited them.</param>
      /// <returns>List of invited players as a string.</returns>
      private static string BuildInvitedList(ImmutableDictionary<SocketGuildUser, SocketGuildUser> players)
      {
         if (players.Count == 0)
            return "-----";

         StringBuilder sb = new StringBuilder();
         foreach (var player in players)
         {
            string teamString = GetPlayerTeam(player.Key);
            sb.AppendLine($"{player.Key.Nickname ?? player.Key.Username} {teamString} invited by {player.Value.Nickname ?? player.Value.Username}");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Builds the raid help message.
      /// </summary>
      /// <returns>Raid help messsage as a string.</returns>
      private static string BuildRaidHelpMessage()
      {
         StringBuilder sb = new StringBuilder();
         sb.AppendLine("Raid Help:");
         sb.AppendLine("The numbers represent the number of accounts that you have with you." +
            " React with one of the numbers to show that you intend to participate in the raid.");
         sb.AppendLine($"Once you are ready for the raid, react with {raidEmojis[(int)RAID_EMOJI_INDEX.PLAYER_READY]} to show others that you are ready." +
            $" When all players have marked that they are ready, Nona will send a message to the group.");
         sb.AppendLine($"If you need an invite to participate in the raid remotely, react with {raidEmojis[(int)RAID_EMOJI_INDEX.REQUEST_INVITE]}.");
         sb.AppendLine($"To invite someone to a raid, react with {raidEmojis[(int)RAID_EMOJI_INDEX.INVITE_PLAYER]} and react with the coresponding emote for the player.");
         sb.AppendLine($"If you wish to remove yourself from the raid, react with {raidEmojis[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]}.");

         sb.AppendLine("\nRaid Edit:");
         sb.AppendLine("To edit the desired raid send the edit command with the following code: ");
         return sb.ToString();
      }

      /// <summary>
      /// Getss the team role registered to a user.
      /// </summary>
      /// <param name="user">User to get team role of.</param>
      /// <returns>Team role name of the user.</returns>
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

      /// <summary>
      /// Removes old raid messages from the list of raid messages.
      /// Old raid messages are messages older than one day.
      /// </summary>
      private static void RemoveOldRaids()
      {
         List<ulong> ids = new List<ulong>();
         foreach (var temp in currentRaids)
            if ((temp.Value.CreatedAt - DateTime.Now).TotalDays >= 1)
               ids.Add(temp.Key);
         foreach (var id in ids)
            currentRaids.Remove(id);
      }

      /// <summary>
      /// Checks if a message is a raid message.
      /// </summary>
      /// <param name="id">Id of the message to check.</param>
      /// <returns>True if the message is a raid message, otherwise false.</returns>
      public static bool IsCurrentRaid(ulong id)
      {
         return currentRaids.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a raid invite message.
      /// </summary>
      /// <param name="id">Id of the message to check.</param>
      /// <returns>True if the message is a raid invite message, otherwise false.</returns>
      public static bool IsRaidInvite(ulong id)
      {
         return raidMessages.ContainsKey(id);
      }
   }
}