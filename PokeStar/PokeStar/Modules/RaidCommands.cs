using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord;
using Discord.Rest;
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
      private static readonly Dictionary<ulong, Raid> raidMessages = new Dictionary<ulong, Raid>();
      private static readonly Dictionary<ulong, RaidMule> muleMessages = new Dictionary<ulong, RaidMule>();
      private static readonly Dictionary<ulong, ulong> remoteMessages = new Dictionary<ulong, ulong>();
      private static readonly Dictionary<ulong, ulong> raidInviteMessages = new Dictionary<ulong, ulong>();
      private static readonly Dictionary<ulong, ulong> muleInviteMessages = new Dictionary<ulong, ulong>();
      private static readonly Dictionary<ulong, ulong> muleReadyMessages = new Dictionary<ulong, ulong>();

      private static readonly IEmote[] raidEmojis = {
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

      private static readonly IEmote[] muleEmojis = {
         new Emoji("🐎"),
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

      private static readonly Emoji[] extraEmojis = {
         new Emoji("⬅️"),
         new Emoji("➡️"),
         new Emoji("🚫")
      };

      private enum RAID_EMOJI_INDEX
      {
         ADD_PLAYER_1,
         ADD_PLAYER_2,
         ADD_PLAYER_3,
         ADD_PLAYER_4,
         ADD_PLAYER_5,
         PLAYER_READY,
         REMOTE_RAID,
         INVITE_PLAYER,
         REMOVE_PLAYER,
         HELP
      }

      private enum MULE_EMOJI_INDEX
      {
         ADD_MULE,
         RAID_READY,
         REQUEST_INVITE,
         INVITE_PLAYER,
         REMOVE_PLAYER,
         HELP
      }

      private enum EXTRA_EMOJI_INDEX
      {
         BACK_ARROW,
         FORWARD_ARROR,
         CANCEL
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
               RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
               for (int i = 0; i < potentials.Count; i++)
                  await selectMsg.AddReactionAsync(selectionEmojis[i]);

               Raid raid = new Raid(tier, time, location)
               {
                  RaidBossSelections = potentials
               };
               raidMessages.Add(selectMsg.Id, raid);
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
                  raid = new Raid(tier, time, location);
                  fileName = $"Egg{tier}.png";
               }

               Connections.CopyFile(fileName);
               RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
               await raidMsg.AddReactionsAsync(raidEmojis);
               raidMessages.Add(raidMsg.Id, raid);
               Connections.DeleteFile(fileName);
            }
         }
         else
            await Context.Channel.SendMessageAsync("This channel is not registered to process Raid commands.");
         RemoveOldRaids();
      }

      [Command("mule")]
      [Alias("pokedex")]
      [Summary("Creates a new Raid Mule message.")]
      public async Task RaidMule([Summary("Tier of the raid.")] short tier,
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
               RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
               for (int i = 0; i < potentials.Count; i++)
                  await selectMsg.AddReactionAsync(selectionEmojis[i]);

               RaidMule raid = new RaidMule(tier, time, location)
               {
                  RaidBossSelections = potentials
               };
               muleMessages.Add(selectMsg.Id, raid);
               Connections.DeleteFile(fileName);
            }
            else
            {
               string fileName;
               RaidMule raid;
               if (potentials.Count == 1)
               {
                  string boss = potentials.First();
                  raid = new RaidMule(tier, time, location, boss);
                  fileName = Connections.GetPokemonPicture(raid.Boss.Name);
               }
               else //silph is mid-update or something else went wrong
               {
                  raid = new RaidMule(tier, time, location);
                  fileName = $"Egg{tier}.png";
               }

               Connections.CopyFile(fileName);
               RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(raid, fileName));
               await raidMsg.AddReactionsAsync(muleEmojis);
               muleMessages.Add(raidMsg.Id, raid);
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
            if (attribute.Equals("time", StringComparison.OrdinalIgnoreCase))
            {
               SocketUserMessage msg = (SocketUserMessage)Context.Channel.GetCachedMessage(code);
               if (IsRaidMessage(code))
               {
                  Raid raid = raidMessages[code];
                  raid.Time = edit;
                  string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
                  Connections.CopyFile(fileName);
                  await msg.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidEmbed(raid, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
               else if (IsRaidMuleMessage(code))
               {
                  RaidMule raid = muleMessages[code];
                  raid.Time = edit;
                  string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
                  Connections.CopyFile(fileName);
                  await msg.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidMuleEmbed(raid, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
            }
            else if (attribute.Equals("location", StringComparison.OrdinalIgnoreCase) ||
                     attribute.Equals("loc", StringComparison.OrdinalIgnoreCase))
            {
               SocketUserMessage msg = (SocketUserMessage)Context.Channel.GetCachedMessage(code);
               if (IsRaidMessage(code))
               {
                  Raid raid = raidMessages[code];
                  raid.Location = edit;
                  string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
                  Connections.CopyFile(fileName);
                  await msg.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidEmbed(raid, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
               else if (IsRaidMuleMessage(code))
               {
                  RaidMule raid = muleMessages[code];
                  raid.Location = edit;
                  string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
                  Connections.CopyFile(fileName);
                  await msg.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidMuleEmbed(raid, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
            }
            else if (attribute.Equals("tier", StringComparison.OrdinalIgnoreCase) ||
                     attribute.Equals("boss", StringComparison.OrdinalIgnoreCase))
            {
               // TODO: Add editing of raid tier/boss
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
         Raid raid = raidMessages[message.Id];
         SocketGuildUser player = (SocketGuildUser)reaction.User;

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
               Connections.CopyFile(filename);
               RestUserMessage raidMsg = await reaction.Channel.SendFileAsync(filename, embed: BuildRaidEmbed(raid, filename));
               await raidMsg.AddReactionsAsync(raidEmojis);
               raidMessages.Add(raidMsg.Id, raid);
               Connections.DeleteFile(filename);
            }
         }
         else
         {
            if (raid.InvitingPlayer == null || !raid.InvitingPlayer.Equals(player))
            {
               bool needsUpdate = true;
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
                  int group = raid.PlayerReady(player);
                  if (group != -1)
                     await reaction.Channel.SendMessageAsync(BuildRaidPingList(raid.Groups.ElementAt(group).GetPingList(), raid.Location, group + 1));
               }
               else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID]))
               {
                  if (raid.IsInRaid(player) == -1)
                  {
                     RestUserMessage remoteMsg = await reaction.Channel.SendMessageAsync(text: $"{player.Mention}", embed: BuildPlayerRemoteEmbed(player.Nickname ?? player.Username));
                     await remoteMsg.AddReactionAsync(selectionEmojis[0]);
                     await remoteMsg.AddReactionAsync(selectionEmojis[1]);
                     await remoteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
                     remoteMessages.Add(remoteMsg.Id, message.Id);
                  }
               }
               else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.INVITE_PLAYER]))
               {
                  if (raid.IsInRaid(player, false) != -1)
                  {
                     if (raid.GetReadonlyInviteList().Count == 0)
                        await reaction.Channel.SendMessageAsync($"{player.Mention}, There are no players to invite.");
                     else
                     {
                        if (!raid.HasActiveInvite())
                        {
                           raid.InvitingPlayer = player;
                           int offset = raid.InvitePage * selectionEmojis.Length;
                           int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, selectionEmojis.Length);
                           RestUserMessage inviteMsg = await reaction.Channel.SendMessageAsync(text: $"{player.Mention}", 
                              embed: BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), player.Nickname ?? player.Username, offset, listSize));
                           for (int i = 0; i < listSize; i++)
                              await inviteMsg.AddReactionAsync(selectionEmojis[i]);

                           if (raid.GetReadonlyInviteList().Count > selectionEmojis.Length)
                           {
                              await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]);
                              await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]);
                           }

                           await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
                           raidInviteMessages.Add(inviteMsg.Id, message.Id);
                        }
                     }
                  }
               }
               else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]))
               {
                  RemovePlayerReturn returnValue = raid.RemovePlayer(player);

                  foreach (SocketGuildUser invite in returnValue.invited)
                     await invite.SendMessageAsync($"{player.Nickname ?? player.Username} has left the raid. You have been moved back to \"Need Invite\".");

                  if (returnValue.GroupNum != -1)
                     await reaction.Channel.SendMessageAsync(BuildRaidPingList(raid.Groups.ElementAt(returnValue.GroupNum).GetPingList(), raid.Location, returnValue.GroupNum + 1));
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

               if (needsUpdate)
               {
                  SocketUserMessage msg = (SocketUserMessage)message;
                  string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
                  Connections.CopyFile(fileName);
                  await msg.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidEmbed(raid, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
            }
            await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, player);
         }
      }

      /// <summary>
      /// Handles a reaction on a raid message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      public static async Task RaidMuleReaction(IMessage message, SocketReaction reaction)
      {
         RaidMule raid = muleMessages[message.Id];
         SocketGuildUser player = (SocketGuildUser)reaction.User;

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
               Connections.CopyFile(filename);
               RestUserMessage raidMsg = await reaction.Channel.SendFileAsync(filename, embed: BuildRaidMuleEmbed(raid, filename));
               await raidMsg.AddReactionsAsync(raidEmojis);
               muleMessages.Add(raidMsg.Id, raid);
               Connections.DeleteFile(filename);
            }
         }
         else
         {
            if (raid.InvitingPlayer == null || !raid.InvitingPlayer.Equals(player))
            {
               bool needsUpdate = true;
               if (reaction.Emote.Equals(raidEmojis[(int)MULE_EMOJI_INDEX.ADD_MULE]))
               {
                  raid.PlayerAdd(player);
               }
               else if (reaction.Emote.Equals(raidEmojis[(int)MULE_EMOJI_INDEX.RAID_READY]))
               {
                  RestUserMessage readyMsg = await reaction.Channel.SendMessageAsync(text: $"{player.Mention}", 
                     embed: BuildMuleReadyEmbed(raid.Groups.Count, player.Nickname ?? player.Username));
                     for (int i = 0; i < raid.Groups.Count; i++)
                        await readyMsg.AddReactionAsync(selectionEmojis[i]);
                  await readyMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
                  muleReadyMessages.Add(readyMsg.Id, message.Id);
               }
               else if (reaction.Emote.Equals(raidEmojis[(int)MULE_EMOJI_INDEX.REQUEST_INVITE]))
               {
                  raid.PlayerRequestInvite(player);
               }
               else if (reaction.Emote.Equals(raidEmojis[(int)MULE_EMOJI_INDEX.INVITE_PLAYER]))
               {
                  if (raid.GetReadonlyInviteList().Count == 0)
                     await reaction.Channel.SendMessageAsync($"{player.Mention}, There are no players to invite.");
                  else
                  {
                     if (!raid.HasActiveInvite())
                     {
                        raid.InvitingPlayer = player;
                        int offset = raid.InvitePage * selectionEmojis.Length;
                        int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, selectionEmojis.Length);
                        RestUserMessage inviteMsg = await reaction.Channel.SendMessageAsync(text: $"{player.Mention}",
                           embed: BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), player.Nickname ?? player.Username, offset, listSize));
                        for (int i = 0; i < listSize; i++)
                           await inviteMsg.AddReactionAsync(selectionEmojis[i]);

                        if (raid.GetReadonlyInviteList().Count > selectionEmojis.Length)
                        {
                           await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]);
                           await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]);
                        }

                        await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
                        muleInviteMessages.Add(inviteMsg.Id, message.Id);
                     }
                  }
               }
               else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]))
               {
                  List<SocketGuildUser> returnValue = raid.RemovePlayer(player);

                  foreach (SocketGuildUser invite in returnValue)
                     await invite.SendMessageAsync($"{player.Nickname ?? player.Username} has left the raid. You have been moved back to \"Need Invite\".");
               }
               else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.HELP]))
               {
                  string prefix = Connections.Instance().GetPrefix(((SocketGuildChannel)message.Channel).Guild.Id);
                  if (prefix == null)
                     prefix = Environment.GetEnvironmentVariable("DEFAULT_PREFIX");

                  await player.SendMessageAsync(BuildRaidMuleHelpMessage());
                  await player.SendMessageAsync($"{prefix}edit {message.Id}");
                  needsUpdate = false;
               }
               else
                  needsUpdate = false;

               if (needsUpdate)
               {
                  SocketUserMessage msg = (SocketUserMessage)message;
                  string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
                  Connections.CopyFile(fileName);
                  await msg.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidMuleEmbed(raid, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
            }
            await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, player);
         }
      }

      /// <summary>
      /// Handles a reaction on a raid remote message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="channel">Channel the reaction happened in</param>
      /// <returns>Completed Task.</returns>
      public static async Task RaidRemoteReaction(IMessage message, SocketReaction reaction, ISocketMessageChannel channel)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         ulong raidMessageId = remoteMessages[message.Id];
         Raid raid = raidMessages[raidMessageId];
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;

         if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            await message.DeleteAsync();
         else if (reaction.Emote.Equals(selectionEmojis[0])) // Remote pass
         {
            raid.PlayerAdd(reactingPlayer, 1, reactingPlayer);
            SocketUserMessage raidMessage = (SocketUserMessage)channel.CachedMessages.FirstOrDefault(x => x.Id == raidMessageId);
            string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
            Connections.CopyFile(fileName);
            await raidMessage.ModifyAsync(x =>
            {
               x.Embed = BuildRaidEmbed(raid, fileName);
            });
            Connections.DeleteFile(fileName);

            remoteMessages.Remove(message.Id);
            await message.DeleteAsync();
         }
         else if (reaction.Emote.Equals(selectionEmojis[1])) // Need Invite
         {
            raid.PlayerRequestInvite(reactingPlayer);
            SocketUserMessage raidMessage = (SocketUserMessage)channel.CachedMessages.FirstOrDefault(x => x.Id == raidMessageId);
            string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
            Connections.CopyFile(fileName);
            await raidMessage.ModifyAsync(x =>
            {
               x.Embed = BuildRaidEmbed(raid, fileName);
            });
            Connections.DeleteFile(fileName);

            remoteMessages.Remove(message.Id);
            await message.DeleteAsync();
         }
         else
            await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reactingPlayer);
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
         ulong raidMessageId = raidInviteMessages[message.Id];
         Raid raid = raidMessages[raidMessageId];
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;

         if (reactingPlayer.Equals(raid.InvitingPlayer))
         {
            if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            {
               await message.DeleteAsync();
               raid.InvitingPlayer = null;
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]))
            {
               raid.ChangeInvitePage(false, selectionEmojis.Length);
               int offset = raid.InvitePage * selectionEmojis.Length;
               int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, selectionEmojis.Length);
               SocketUserMessage inviteMessage = (SocketUserMessage)channel.CachedMessages.FirstOrDefault(x => x.Id == raidMessageId);
               await inviteMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize);
               });
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]))
            {
               raid.ChangeInvitePage(true, selectionEmojis.Length);
               int offset = raid.InvitePage * selectionEmojis.Length;
               int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, selectionEmojis.Length);
               SocketUserMessage inviteMessage = (SocketUserMessage)channel.CachedMessages.FirstOrDefault(x => x.Id == raidMessageId);
               await inviteMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize);
               });
            }
            else
            {
               for (int i = 0; i < selectionEmojis.Length; i++)
               {
                  if (reaction.Emote.Equals(selectionEmojis[i]))
                  {
                     int offset = raid.InvitePage * selectionEmojis.Length;
                     SocketGuildUser player = raid.GetReadonlyInviteList().ElementAt(i + offset);
                     if (raid.InvitePlayer(player, reactingPlayer))
                     {
                        SocketUserMessage raidMessage = (SocketUserMessage)channel.CachedMessages.FirstOrDefault(x => x.Id == raidMessageId);
                        string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
                        Connections.CopyFile(fileName);
                        await raidMessage.ModifyAsync(x =>
                        {
                           x.Embed = BuildRaidEmbed(raid, fileName);
                        });
                        Connections.DeleteFile(fileName);

                        await player.SendMessageAsync($"You have been invited to a raid by {reactingPlayer.Nickname ?? reactingPlayer.Username}.");
                        raidMessages.Remove(message.Id);
                        raid.InvitingPlayer = null;
                        await message.DeleteAsync();
                     }
                  }
               }
            }
         }
      }

      /// <summary>
      /// Handles a reaction on a raid mule invite message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="channel">Channel the reaction happened in</param>
      /// <returns>Completed Task.</returns>
      public static async Task RaidMuleInviteReaction(IMessage message, SocketReaction reaction, ISocketMessageChannel channel)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         ulong raidMuleMessageId = muleInviteMessages[message.Id];
         RaidMule raid = muleMessages[raidMuleMessageId];
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;

         if (reactingPlayer.Equals(raid.InvitingPlayer))
         {
            if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            {
               await message.DeleteAsync();
               raid.InvitingPlayer = null;
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]))
            {
               raid.ChangeInvitePage(false, selectionEmojis.Length);
               int offset = raid.InvitePage * selectionEmojis.Length;
               int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, selectionEmojis.Length);
               SocketUserMessage inviteMessage = (SocketUserMessage)channel.CachedMessages.FirstOrDefault(x => x.Id == raidMuleMessageId);
               await inviteMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize);
               });
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]))
            {
               raid.ChangeInvitePage(true, selectionEmojis.Length);
               int offset = raid.InvitePage * selectionEmojis.Length;
               int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, selectionEmojis.Length);
               SocketUserMessage inviteMessage = (SocketUserMessage)channel.CachedMessages.FirstOrDefault(x => x.Id == raidMuleMessageId);
               await inviteMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize);
               });
            }
            else
            {
               for (int i = 0; i < selectionEmojis.Length; i++)
               {
                  if (reaction.Emote.Equals(selectionEmojis[i]))
                  {
                     int offset = raid.InvitePage * selectionEmojis.Length;
                     SocketGuildUser player = raid.GetReadonlyInviteList().ElementAt(i + offset);
                     if (raid.InvitePlayer(player, reactingPlayer))
                     {
                        SocketUserMessage raidMessage = (SocketUserMessage)channel.CachedMessages.FirstOrDefault(x => x.Id == raidMuleMessageId);
                        string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
                        Connections.CopyFile(fileName);
                        await raidMessage.ModifyAsync(x =>
                        {
                           x.Embed = BuildRaidMuleEmbed(raid, fileName);
                        });
                        Connections.DeleteFile(fileName);

                        await player.SendMessageAsync($"You have been invited to a raid by {reactingPlayer.Nickname ?? reactingPlayer.Username}.");
                        raidMessages.Remove(message.Id);
                        raid.InvitingPlayer = null;
                        await message.DeleteAsync();
                     }
                  }
               }
            }
         }
      }

      /// <summary>
      /// Handles a reaction on a raid mule ready message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="channel">Channel the reaction happened in</param>
      /// <returns>Completed Task.</returns>
      public static async Task RaidMuleReadyReaction(IMessage message, SocketReaction reaction, ISocketMessageChannel channel)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         ulong raidMuleMessageId = muleInviteMessages[message.Id];
         RaidMule raid = muleMessages[raidMuleMessageId];
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;

         if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            await message.DeleteAsync();
         else
         {
            for (int i = 0; i < selectionEmojis.Length; i++)
            {
               if (reaction.Emote.Equals(selectionEmojis[i]))
               {
                  await channel.SendMessageAsync($"{BuildRaidMulePingList(raid.Groups.ElementAt(i).GetPingList(), raid.Location, i + 1)}");
                  raidMessages.Remove(message.Id);
                  await message.DeleteAsync();
               }
            }
         }
      }

      /// <summary>
      /// Builds the raid embed.
      /// </summary>
      /// <param name="raid">Raid to display in the embed.</param>
      /// <param name="fileName">Name of picture to use for the raid.</param>
      /// <returns>Embed for viewing a raid.</returns>
      private static Embed BuildRaidEmbed(Raid raid, string fileName)
      {
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
            embed.AddField($"Group {i + 1} Remote", $"{BuildInvitedList(raid.Groups.ElementAt(i).GetReadonlyInvited())}");
         }
         embed.AddField($"Need Invite:", $"{BuildRequestInviteList(raid.GetReadonlyInviteList())}");
         embed.WithFooter("Note: the max number of members in a raid is 20, and the max number of invites is 10.");

         return embed.Build();
      }

      /// <summary>
      /// Builds the raid mule embed.
      /// </summary>
      /// <param name="raid">Raid to display in the embed.</param>
      /// <param name="fileName">Name of picture to use for the raid.</param>
      /// <returns>Embed for viewing a raid.</returns>
      private static Embed BuildRaidMuleEmbed(RaidMule raid, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.DarkBlue);
         embed.WithTitle($"{(raid.Boss.Name.Equals("Bossless") ? "" : raid.Boss.Name)} {BuildRaidTitle(raid.Tier)}");
         embed.WithDescription("Press ? for help.");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Time", raid.Time, true);
         embed.AddField("Location", raid.Location, true);

         embed.AddField($"Mules", $"{BuildPlayerList(raid.Mules.GetReadonlyAttending())}");

         for (int i = 0; i < raid.Groups.Count; i++)
         {
            embed.AddField($"Group {i + 1} Remote", $"{BuildInvitedList(raid.Groups.ElementAt(i).GetReadonlyInvited())}");
         }
         embed.AddField($"Need Invite:", $"{BuildRequestInviteList(raid.GetReadonlyInviteList())}");
         embed.WithFooter("Note: The max number of invites is 10, and the max number of invites per person is 5.");

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
      private static Embed BuildPlayerInviteEmbed(ImmutableList<SocketGuildUser> invite, string user, int offset, int listSize)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = offset; i < listSize; i++)
            sb.AppendLine($"{selectionEmojis[i]} {invite.ElementAt(i).Nickname ?? invite.ElementAt(i).Username}");

         if (invite.Count > selectionEmojis.Length)
         {
            sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]} Previous Page");
            sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]} Next Page");
         }

         sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Cancel");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.DarkBlue);
         embed.WithTitle($"{user} - Invite");
         embed.AddField("Please Select Player to Invite.", sb.ToString());

         return embed.Build();
      }

      /// <summary>
      /// Builds the raid remote embed.
      /// </summary>
      /// <param name="user">User that wants to attend raid remotly someone.</param>
      /// <returns>Embed for player to attend a raid remotly.</returns>
      private static Embed BuildPlayerRemoteEmbed(string user)
      {
         StringBuilder sb = new StringBuilder();
         sb.AppendLine($"{selectionEmojis[0]} Remote Pass");
         sb.AppendLine($"{selectionEmojis[1]} Need Invite");
         sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Cancel");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.DarkBlue);
         embed.WithTitle($"{user} - Remote");
         embed.AddField("Please Select How You Will Remote to the Raid.", sb.ToString());

         return embed.Build();
      }

      /// <summary>
      /// Builds the raid mule ready embed.
      /// </summary>
      /// <param name="groups">Number of raid groups.</param>
      /// <param name="user">User acting as the raid mule.</param>
      /// <returns>Embed for user to mark group as ready.</returns>
      private static Embed BuildMuleReadyEmbed(int groups, string user)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < groups; i++)
            sb.AppendLine($"{selectionEmojis[i]} Raid Group {i + 1}");
         sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Cancel");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.DarkBlue);
         embed.WithTitle($"{user} - Raid Mule Ready");
         embed.AddField("Please Select Which Group is Ready.", sb.ToString());

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
      /// Builds a list of players to ping when a raid group is ready.
      /// </summary>
      /// <param name="players">List of players to ping.</param>
      /// <param name="location">Location of the raid.</param>
      /// <param name="groupNumber">Group number the players are part of.</param>
      /// <returns>List of players to ping as a string.</returns>
      private static string BuildRaidPingList(List<SocketGuildUser> players, string location, int groupNumber)
      {
         StringBuilder sb = new StringBuilder();
         foreach (SocketGuildUser player in players)
            sb.Append($"{player.Mention} ");
         sb.Append($"Everyone in Group {groupNumber} is ready at {location}");
         return sb.ToString();
      }

      /// <summary>
      /// Builds a list of players to ping when a raid mule group is ready.
      /// </summary>
      /// <param name="players">List of players to ping.</param>
      /// <param name="location">Location of the raid.</param>
      /// <param name="groupNumber">Group number the players are part of.</param>
      /// <returns>List of players to ping as a string.</returns>
      private static string BuildRaidMulePingList(List<SocketGuildUser> players, string location, int groupNumber)
      {
         StringBuilder sb = new StringBuilder();
         foreach (SocketGuildUser player in players)
            sb.Append($"{player.Mention} ");
         sb.Append($"Invites are going out to Group {groupNumber} at {location}");
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
         foreach (KeyValuePair<SocketGuildUser, int> player in players)
         {
            sb.AppendLine($"{raidEmojis[player.Value - 1]} {player.Key.Nickname ?? player.Key.Username} {GetPlayerTeam(player.Key)}");
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
         foreach (KeyValuePair<SocketGuildUser, SocketGuildUser> player in players)
         {
            if (player.Key.Equals(player.Value))
               sb.AppendLine($"{player.Key.Nickname ?? player.Key.Username} {GetPlayerTeam(player.Key)} will be raiding remotly");
            else
               sb.AppendLine($"{player.Key.Nickname ?? player.Key.Username} {GetPlayerTeam(player.Key)} invited by {player.Value.Nickname ?? player.Value.Username}");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Builds a list of players requesting an invite to a raid.
      /// </summary>
      /// <param name="players">List of players request an invite to a raid.</param>
      /// <returns>List of players as a string.</returns>
      private static string BuildRequestInviteList(ImmutableList<SocketGuildUser> players)
      {
         if (players.Count == 0)
            return "-----";

         StringBuilder sb = new StringBuilder();
         foreach (SocketGuildUser player in players)
         {
            string teamString = GetPlayerTeam(player);
            sb.AppendLine($"{player.Nickname ?? player.Username} {teamString}");
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
            " React with one of the numbers to show that you intend to participate in the raid in person.");
         sb.AppendLine($"Once you are ready for the raid, react with {raidEmojis[(int)RAID_EMOJI_INDEX.PLAYER_READY]} to show others that you are ready." +
            $" When all players have marked that they are ready, Nona will send a message to the group.");
         sb.AppendLine($"If you need an invite to participate in the raid remotely, react with {raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID]}.");
         sb.AppendLine($"To invite someone to a raid, react with {raidEmojis[(int)RAID_EMOJI_INDEX.INVITE_PLAYER]} and react with the coresponding emote for the player.");
         sb.AppendLine($"If you wish to remove yourself from the raid, react with {raidEmojis[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]}.");

         sb.AppendLine("\nRaid Edit:");
         sb.AppendLine("To edit the desired raid send the edit command with the following code: ");
         return sb.ToString();
      }

      /// <summary>
      /// Builds the raid mule help message.
      /// </summary>
      /// <returns>Raid mule help messsage as a string.</returns>
      private static string BuildRaidMuleHelpMessage()
      {
         StringBuilder sb = new StringBuilder();
         sb.AppendLine("Raid Mule Help:");
         sb.AppendLine($"{muleEmojis[(int)MULE_EMOJI_INDEX.ADD_MULE]}: Adds a user as a raid mule.");
         sb.AppendLine($"{muleEmojis[(int)MULE_EMOJI_INDEX.RAID_READY]}: Allows a raid mule to announce that a raid group is ready to go.");
         sb.AppendLine($"{muleEmojis[(int)MULE_EMOJI_INDEX.REQUEST_INVITE]}: Adds a user as requesting an invite.");
         sb.AppendLine($"{muleEmojis[(int)MULE_EMOJI_INDEX.INVITE_PLAYER]}: Adds a user that has requested an invite to a raid group.");
         sb.AppendLine($"{muleEmojis[(int)MULE_EMOJI_INDEX.REMOVE_PLAYER]}: Removes a user from the raid. " +
                       $"Any users invited by a raid mule that leaves will be moved to requesting invite.");
         sb.AppendLine($"{muleEmojis[(int)MULE_EMOJI_INDEX.HELP]}: Generates this help message.");

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
         foreach (KeyValuePair<ulong, Raid> temp in raidMessages)
            if (Math.Abs((temp.Value.CreatedAt - DateTime.Now).TotalDays) >= 1)
               ids.Add(temp.Key);
         foreach (ulong id in ids)
            raidMessages.Remove(id);
      }

      /// <summary>
      /// Checks if a message is a raid message.
      /// </summary>
      /// <param name="id">Id of the message to check.</param>
      /// <returns>True if the message is a raid message, otherwise false.</returns>
      public static bool IsRaidMessage(ulong id)
      {
         return raidMessages.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a raid mule message.
      /// </summary>
      /// <param name="id">Id of the message to check.</param>
      /// <returns>True if the message is a raid mule message, otherwise false.</returns>
      public static bool IsRaidMuleMessage(ulong id)
      {
         return muleMessages.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a raid remote message.
      /// </summary>
      /// <param name="id">Id of the message to check.</param>
      /// <returns>True if the message is a raid remote message, otherwise false.</returns>
      public static bool IsRemoteMessage(ulong id)
      {
         return remoteMessages.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a raid invite message.
      /// </summary>
      /// <param name="id">Id of the message to check.</param>
      /// <returns>True if the message is a raid invite message, otherwise false.</returns>
      public static bool IsRaidInviteMessage(ulong id)
      {
         return raidInviteMessages.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a raid mule invite message.
      /// </summary>
      /// <param name="id">Id of the message to check.</param>
      /// <returns>True if the message is a raid mule invite message, otherwise false.</returns>
      public static bool IsRaidMuleInviteMessage(ulong id)
      {
         return muleInviteMessages.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a raid mule ready message.
      /// </summary>
      /// <param name="id">Id of the message to check.</param>
      /// <returns>True if the message is a raid mule ready message, otherwise false.</returns>
      public static bool IsRaidMuleReadyMessage(ulong id)
      {
         return muleReadyMessages.ContainsKey(id);
      }

      /// <summary>
      /// Sets the remote pass emote on startup.
      /// </summary>
      public static void SetRemotePassEmote()
      {
         raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID] = Emote.Parse(Environment.GetEnvironmentVariable("REMOTE_PASS_EMOTE"));
         muleEmojis[(int)MULE_EMOJI_INDEX.REQUEST_INVITE] = Emote.Parse(Environment.GetEnvironmentVariable("REMOTE_PASS_EMOTE"));
      }
   }
}