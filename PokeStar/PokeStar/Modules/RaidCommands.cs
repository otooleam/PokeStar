﻿using System;
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
      private static readonly Dictionary<ulong, RaidParent> raidMessages = new Dictionary<ulong, RaidParent>();
      private static readonly Dictionary<ulong, RaidSubMessage> subMessages = new Dictionary<ulong, RaidSubMessage>();

      private static readonly IEmote[] raidEmojis = {
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
         new Emoji("5️⃣"),
         new Emoji("6️⃣"),
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

      private static readonly string[] raidEmojisDesc = {
         "are the number of Trainers in your party, physically at the raid and that you are inviting remotely.",
         "means you are present/ready for the raid to begin. Nona will tag you in a post when all trainers are ready and the raid can then begin.",
         "means you will be either doing the raid remotely yourself or need another trainer to send you an invite to be done remotely.",
         "means you are willing to invite one of the trainers who are asking for a remote invite. Nona will walk you thru who you want to invite and you do NOT need to count that person in YOUR PARTY; they will be part of the physical count that Nona makes.",
         "means you are not able to make the raid any longer and you will be removed from the list. Nona will send a message to anyone you were planning to invite.",
         "generates this help message."
      };

      private static readonly string[] muleEmojisDesc = {
         "means you are planning on being a raid mule.",
         "means that a raid group is ready to go. Nona will tag you in a post when the raid mule is ready to start the raid.",
         "means you need a raid mule to send you an invite to be done remotely.",
         "means you are willing to invite one of the trainers who are asking for a remote invite. Nona will walk you thru who you want to invite.",
         "means you are not able to make the raid any longer and you will be removed from the list. Nona will send a message to anyone you were planning to invite.",
         "generates this help message."
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
         ADD_PLAYER_6,
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

      private enum SUB_MESSAGE_TYPES
      {
         INVITE_SUB_MESSAGE,
         RAID_REMOTE_SUB_MESSAGE,
         MULE_READY_SUB_MESSAGE,
      }

      private static readonly short MEGA_RAID_TIER = 7;
      private static readonly short LEGENDARY_RAID_TIER = 5;
      private static readonly short RARE_RAID_TIER = 3;
      private static readonly short COMMON_RAID_TIER = 1;
      private static readonly short INVALID_TIER = 0;

      [Command("raid")]
      [Summary("Creates a new Raid message.")]
      [Remarks("Valid raid values are:\n" +
               "Raid Tier........Raid Value\n" +
               "Tier 1...............1 / Common / C\n" +
               "Tier 3..............3 / Rare / R\n" +
               "Tier 5..............5 / Legendary / L\n" +
               "Mega Tier......7 / Mega / M\n")]
      public async Task Raid([Summary("Tier of the raid.")] string tier,
                             [Summary("Time the raid will start.")] string time,
                             [Summary("Where the raid will be.")][Remainder] string location)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "R"))
            await GenerateRaidMessage("raid", raidEmojis, tier, time, location);
         else
            await ResponseMessage.SendErrorMessage(Context, "raid", "This channel is not registered to process Raid commands.");
         RemoveOldRaids();
      }

      [Command("mule")]
      [Alias("raidmule")]
      [Summary("Creates a new Raid Mule message.")]
      [Remarks("Valid raid values are:\n" +
               "Raid Tier........Raid Value\n" +
               "Tier 1...............1 / Common / C\n" +
               "Tier 3..............3 / Rare / R\n" +
               "Tier 5..............5 / Legendary / L\n" +
               "Mega Tier......7 / Mega / M\n")]
      public async Task RaidMule([Summary("Tier of the raid.")] string tier,
                                 [Summary("Time the raid will start.")] string time,
                                 [Summary("Where the raid will be.")][Remainder] string location)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "R"))
            await GenerateRaidMessage("mule", muleEmojis, tier, time, location);
         else
            await ResponseMessage.SendErrorMessage(Context, "mule", "This channel is not registered to process Raid commands.");
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
            SocketUserMessage msg = (SocketUserMessage)Context.Channel.GetCachedMessage(code);
            RaidParent raid = raidMessages[code];
            bool simpleEdit = false;
            if (attribute.Equals("time", StringComparison.OrdinalIgnoreCase))
            {
               raid.Time = edit;
               simpleEdit = true;
            }
            else if (attribute.Equals("location", StringComparison.OrdinalIgnoreCase) ||
                     attribute.Equals("loc", StringComparison.OrdinalIgnoreCase))
            {
               raid.Location = edit;
               simpleEdit = true;
            }
            else if (attribute.Equals("tier", StringComparison.OrdinalIgnoreCase) ||
                     attribute.Equals("boss", StringComparison.OrdinalIgnoreCase))
            {
               // TODO: Add editing of raid tier/boss
            }
            else
               await ResponseMessage.SendErrorMessage(Context, "edit", "Please enter a valid field to edit.");

            if (simpleEdit)
            {
               string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
               Connections.CopyFile(fileName);
               await msg.ModifyAsync(x =>
               {
                  x.Embed = BuildRaidEmbed(raid, fileName);
               });
               Connections.DeleteFile(fileName);

               List<SocketGuildUser> allUsers = new List<SocketGuildUser>();
               foreach (RaidGroup group in raid.Groups)
                  allUsers.AddRange(group.GetPingList());
               allUsers.AddRange(raid.GetReadonlyInviteList());
               if (raid is RaidMule mule)
                  allUsers.AddRange(mule.Mules.GetReadonlyAttending().Keys);
               await ReplyAsync(BuildEditPingList(allUsers, (SocketGuildUser)Context.User, attribute, edit));
            }
         }
         else
            await ResponseMessage.SendErrorMessage(Context, "edit", "This channel is not registered to process Raid commands.");
      }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      /// <param name="emojis"></param>
      /// <param name="tier"></param>
      /// <param name="time"></param>
      /// <param name="location"></param>
      /// <returns></returns>
      private async Task GenerateRaidMessage(string command, IEmote[] emojis, string tier, string time, string location)
      {
         short calcTier = GenerateTier(tier);
         List<string> potentials = Connections.GetBossList(calcTier);
         RaidParent raid;
         string fileName;
         if (potentials.Count > 1)
         {
            fileName = $"Egg{calcTier}.png";
            raid = GenerateRaidType(command.Equals("raid", StringComparison.OrdinalIgnoreCase), calcTier, time, location);
            raid.RaidBossSelections = potentials;

            Connections.CopyFile(fileName);
            RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
            for (int i = 0; i < potentials.Count; i++)
               await selectMsg.AddReactionAsync(selectionEmojis[i]);
            raidMessages.Add(selectMsg.Id, raid);
            Connections.DeleteFile(fileName);
         }
         else if (potentials.Count == 1)
         {
            string boss = potentials.First();
            raid = GenerateRaidType(command.Equals("raid", StringComparison.OrdinalIgnoreCase), calcTier, time, location, boss);
            fileName = Connections.GetPokemonPicture(raid.Boss.Name);

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
            await raidMsg.AddReactionsAsync(emojis);
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
         }
         else if (Environment.GetEnvironmentVariable("USE_EMPTY_RAID").Equals("TRUE", StringComparison.OrdinalIgnoreCase))
         {
            string boss = RaidBoss.DefaultName;
            raid = GenerateRaidType(command.Equals("raid", StringComparison.OrdinalIgnoreCase), calcTier, time, location, boss);
            fileName = Connections.GetPokemonPicture(raid.Boss.Name);

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
            await raidMsg.AddReactionsAsync(emojis);
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
         }
         else
            await ErrorMessage.SendErrorMessage(Context, command, $"No raid bosses found for tier {tier}");
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="check"></param>
      /// <param name="tier"></param>
      /// <param name="time"></param>
      /// <param name="location"></param>
      /// <param name="boss"></param>
      /// <returns></returns>
      private RaidParent GenerateRaidType(bool check, short tier, string time, string location, string boss = null)
      {
         if (check)
            return new Raid(tier, time, location, boss);
         return new RaidMule(tier, time, location, boss);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="tier"></param>
      /// <returns></returns>
      private short GenerateTier(string tier)
      {
         if (tier.Equals("m", StringComparison.OrdinalIgnoreCase) ||
             tier.Equals("mega", StringComparison.OrdinalIgnoreCase) ||
             tier.Equals("7", StringComparison.OrdinalIgnoreCase))
            return MEGA_RAID_TIER;
         if (tier.Equals("l", StringComparison.OrdinalIgnoreCase) ||
             tier.Equals("legendary", StringComparison.OrdinalIgnoreCase) ||
             tier.Equals("5", StringComparison.OrdinalIgnoreCase))
            return LEGENDARY_RAID_TIER;
         if (tier.Equals("r", StringComparison.OrdinalIgnoreCase) ||
             tier.Equals("rare", StringComparison.OrdinalIgnoreCase) ||
             tier.Equals("3", StringComparison.OrdinalIgnoreCase))
            return RARE_RAID_TIER;
         if (tier.Equals("c", StringComparison.OrdinalIgnoreCase) ||
             tier.Equals("common", StringComparison.OrdinalIgnoreCase) ||
             tier.Equals("1", StringComparison.OrdinalIgnoreCase))
            return COMMON_RAID_TIER;
         return INVALID_TIER;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="message"></param>
      /// <param name="reaction"></param>
      /// <returns></returns>
      public static async Task RaidMessageReactionHandle(IMessage message, SocketReaction reaction)
      {
         RaidParent parent = raidMessages[message.Id];

         if (parent.Boss == null)
         {
            for (int i = 0; i < parent.RaidBossSelections.Count; i++)
            {
               if (reaction.Emote.Equals(selectionEmojis[i]))
               {
                  parent.SetBoss(parent.RaidBossSelections[i]);
                  await reaction.Channel.DeleteMessageAsync(message);

                  string filename = Connections.GetPokemonPicture(parent.Boss.Name);
                  Connections.CopyFile(filename);
                  RestUserMessage raidMsg = await reaction.Channel.SendFileAsync(filename, embed: BuildRaidEmbed(parent, filename));

                  if (parent is Raid)
                     await raidMsg.AddReactionsAsync(raidEmojis);
                  else if (parent is RaidMule)
                     await raidMsg.AddReactionsAsync(muleEmojis);

                  raidMessages.Add(raidMsg.Id, parent);
                  Connections.DeleteFile(filename);
                  return;
               }
            }
         }
         else
         {
            if (parent is Raid raid)
               await RaidReactionHandle(message, reaction, raid);
            else if (parent is RaidMule mule)
               await RaidMuleReactionHandle(message, reaction, mule);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="message"></param>
      /// <param name="reaction"></param>
      /// <returns></returns>
      public static async Task RaidSubMessageReactionHandle(IMessage message, SocketReaction reaction)
      {
         int subMessageType = subMessages[message.Id].SubMessageType;
         if (subMessageType == (int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE)
            await RaidInviteReactionHandle(message, reaction);
         else if (subMessageType == (int)SUB_MESSAGE_TYPES.RAID_REMOTE_SUB_MESSAGE)
            await RaidRemoteReactionHandle(message, reaction);
         else if (subMessageType == (int)SUB_MESSAGE_TYPES.MULE_READY_SUB_MESSAGE)
            await RaidMuleReadyReactionHandle(message, reaction);

      }

      /// <summary>
      /// Handles a reaction on a raid message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidReactionHandle(IMessage message, SocketReaction reaction, Raid raid)
      {
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;

         if (raid.InvitingPlayer == null || !raid.InvitingPlayer.Equals(reactingPlayer))
         {
            bool needsUpdate = true;
            if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_1]))
            {
               raid.PlayerAdd(reactingPlayer, 1);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_2]))
            {
               raid.PlayerAdd(reactingPlayer, 2);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_3]))
            {
               raid.PlayerAdd(reactingPlayer, 3);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_4]))
            {
               raid.PlayerAdd(reactingPlayer, 4);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_5]))
            {
               raid.PlayerAdd(reactingPlayer, 5);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_6]))
            {
               raid.PlayerAdd(reactingPlayer, 6);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.PLAYER_READY]))
            {
               int group = raid.PlayerReady(reactingPlayer);
               if (group != RaidParent.NotInRaid)
                  await reaction.Channel.SendMessageAsync(BuildRaidPingList(raid.Groups.ElementAt(group).GetPingList(), raid.Location, group + 1, true));
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID]))
            {
               if (raid.IsInRaid(reactingPlayer) == RaidParent.NotInRaid)
               {
                  RestUserMessage remoteMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.Mention}",
                     embed: BuildPlayerRemoteEmbed(reactingPlayer.Nickname ?? reactingPlayer.Username));
                  await remoteMsg.AddReactionAsync(selectionEmojis[0]);
                  await remoteMsg.AddReactionAsync(selectionEmojis[1]);
                  await remoteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
                  subMessages.Add(remoteMsg.Id, new RaidSubMessage
                  {
                     SubMessageType = (int)SUB_MESSAGE_TYPES.RAID_REMOTE_SUB_MESSAGE,
                     MainMessageId = message.Id
                  });
               }
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.INVITE_PLAYER]))
            {
               if (raid.IsInRaid(reactingPlayer, false) != RaidParent.NotInRaid)
               {
                  if (raid.GetReadonlyInviteList().Count == 0)
                     await reaction.Channel.SendMessageAsync($"{reactingPlayer.Mention}, There are no players to invite.");
                  else
                  {
                     if (!raid.HasActiveInvite())
                     {
                        raid.InvitingPlayer = reactingPlayer;
                        int offset = raid.InvitePage * selectionEmojis.Length;
                        int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, selectionEmojis.Length);
                        RestUserMessage inviteMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.Mention}",
                           embed: BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize));
                        for (int i = 0; i < listSize; i++)
                           await inviteMsg.AddReactionAsync(selectionEmojis[i]);

                        if (raid.GetReadonlyInviteList().Count > selectionEmojis.Length)
                        {
                           await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]);
                           await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]);
                        }

                        await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
                        subMessages.Add(inviteMsg.Id, new RaidSubMessage
                        {
                           SubMessageType = (int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE,
                           MainMessageId = message.Id
                        });
                     }
                  }
               }
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               RemovePlayerReturn returnValue = raid.RemovePlayer(reactingPlayer);

               foreach (SocketGuildUser invite in returnValue.invited)
                  await invite.SendMessageAsync($"{reactingPlayer.Nickname ?? reactingPlayer.Username} has left the raid. You have been moved back to \"Need Invite\".");

               if (returnValue.GroupNum != RaidParent.NotInRaid)
                  await reaction.Channel.SendMessageAsync(BuildRaidPingList(raid.Groups.ElementAt(returnValue.GroupNum).GetPingList(), raid.Location, returnValue.GroupNum + 1, true));
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.HELP]))
            {
               string prefix = Connections.Instance().GetPrefix(((SocketGuildChannel)message.Channel).Guild.Id);
               await reactingPlayer.SendMessageAsync(BuildRaidHelpMessage(raidEmojis, raidEmojisDesc));
               await reactingPlayer.SendMessageAsync($"{prefix}edit {message.Id}");
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
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reactingPlayer);
      }

      /// <summary>
      /// Handles a reaction on a raid message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidMuleReactionHandle(IMessage message, SocketReaction reaction, RaidMule raid)
      {
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;
         if (raid.InvitingPlayer == null || !raid.InvitingPlayer.Equals(reactingPlayer))
         {
            bool needsUpdate = true;
            if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.ADD_MULE]))
            {
               raid.PlayerAdd(reactingPlayer, 1);
            }
            else if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.RAID_READY]))
            {
               if (!raid.HasInvites())
                  await reaction.Channel.SendMessageAsync($"{reactingPlayer.Mention}, There are no players Invited.");
               else if (raid.IsInRaid(reactingPlayer, false) != RaidParent.NotInRaid)
               {
                  RestUserMessage readyMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.Mention}",
                     embed: BuildMuleReadyEmbed(raid.Groups.Count, reactingPlayer.Nickname ?? reactingPlayer.Username));
                  for (int i = 0; i < raid.Groups.Count; i++)
                     await readyMsg.AddReactionAsync(selectionEmojis[i]);
                  await readyMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
                  subMessages.Add(readyMsg.Id, new RaidSubMessage
                  {
                     SubMessageType = (int)SUB_MESSAGE_TYPES.MULE_READY_SUB_MESSAGE,
                     MainMessageId = message.Id
                  });
               }
            }
            else if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.REQUEST_INVITE]))
            {
               raid.RequestInvite(reactingPlayer);
            }
            else if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.INVITE_PLAYER]))
            {
               if (raid.IsInRaid(reactingPlayer, false) != RaidParent.NotInRaid)
               {
                  if (raid.GetReadonlyInviteList().Count == 0)
                     await reaction.Channel.SendMessageAsync($"{reactingPlayer.Mention}, There are no players to invite.");
                  else
                  {
                     if (!raid.HasActiveInvite())
                     {
                        raid.InvitingPlayer = reactingPlayer;
                        int offset = raid.InvitePage * selectionEmojis.Length;
                        int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, selectionEmojis.Length);
                        RestUserMessage inviteMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.Mention}",
                           embed: BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize));
                        for (int i = 0; i < listSize; i++)
                           await inviteMsg.AddReactionAsync(selectionEmojis[i]);

                        if (raid.GetReadonlyInviteList().Count > selectionEmojis.Length)
                        {
                           await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]);
                           await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]);
                        }

                        await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
                        subMessages.Add(inviteMsg.Id, new RaidSubMessage
                        {
                           SubMessageType = (int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE,
                           MainMessageId = message.Id
                        });
                     }
                  }
               }
            }
            else if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               List<SocketGuildUser> returnValue = raid.RemovePlayer(reactingPlayer).invited;

               foreach (SocketGuildUser invite in returnValue)
                  await invite.SendMessageAsync($"{reactingPlayer.Nickname ?? reactingPlayer.Username} has left the raid. You have been moved back to \"Need Invite\".");
            }
            else if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.HELP]))
            {
               string prefix = Connections.Instance().GetPrefix(((SocketGuildChannel)message.Channel).Guild.Id);
               await reactingPlayer.SendMessageAsync(BuildRaidHelpMessage(muleEmojis, muleEmojisDesc));
               await reactingPlayer.SendMessageAsync($"{prefix}edit {message.Id}");
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
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reactingPlayer);
      }

      /// <summary>
      /// Handles a reaction on a raid invite message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="channel">Channel the reaction happened in</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidInviteReactionHandle(IMessage message, SocketReaction reaction)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         ulong raidMessageId = subMessages[message.Id].MainMessageId;
         RaidParent raid = raidMessages[raidMessageId];
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;

         if (reactingPlayer.Equals(raid.InvitingPlayer) || message.MentionedUserIds.Contains(reactingPlayer.Id))
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
               SocketUserMessage inviteMessage = (SocketUserMessage)reaction.Channel.CachedMessages.FirstOrDefault(x => x.Id == raidMessageId);
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
               SocketUserMessage inviteMessage = (SocketUserMessage)reaction.Channel.CachedMessages.FirstOrDefault(x => x.Id == raidMessageId);
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
                        SocketUserMessage raidMessage = (SocketUserMessage)reaction.Channel.CachedMessages.FirstOrDefault(x => x.Id == raidMessageId);
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
      /// Handles a reaction on a raid remote message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="channel">Channel the reaction happened in</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidRemoteReactionHandle(IMessage message, SocketReaction reaction)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         ulong raidMessageId = subMessages[message.Id].MainMessageId;
         Raid raid = (Raid)raidMessages[raidMessageId];
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;

         if (message.MentionedUserIds.Contains(reactingPlayer.Id))
         {
            if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
               await message.DeleteAsync();
            else if (reaction.Emote.Equals(selectionEmojis[0])) // Remote pass
            {
               raid.PlayerAdd(reactingPlayer, 1, reactingPlayer);
               SocketUserMessage raidMessage = (SocketUserMessage)reaction.Channel.CachedMessages.FirstOrDefault(x => x.Id == raidMessageId);
               string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
               Connections.CopyFile(fileName);
               await raidMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildRaidEmbed(raid, fileName);
               });
               Connections.DeleteFile(fileName);

               subMessages.Remove(message.Id);
               await message.DeleteAsync();
            }
            else if (reaction.Emote.Equals(selectionEmojis[1])) // Need Invite
            {
               raid.RequestInvite(reactingPlayer);
               SocketUserMessage raidMessage = (SocketUserMessage)reaction.Channel.CachedMessages.FirstOrDefault(x => x.Id == raidMessageId);
               string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
               Connections.CopyFile(fileName);
               await raidMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildRaidEmbed(raid, fileName);
               });
               Connections.DeleteFile(fileName);

               subMessages.Remove(message.Id);
               await message.DeleteAsync();
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
      private static async Task RaidMuleReadyReactionHandle(IMessage message, SocketReaction reaction)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         ulong raidMuleMessageId = subMessages[message.Id].MainMessageId;
         RaidMule raid = (RaidMule)raidMessages[raidMuleMessageId];

         if (message.MentionedUserIds.Contains(reaction.User.Value.Id))
         {

            if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
               await message.DeleteAsync();
            else
            {
               for (int i = 0; i < selectionEmojis.Length; i++)
               {
                  if (reaction.Emote.Equals(selectionEmojis[i]))
                  {
                     await reaction.Channel.SendMessageAsync($"{BuildRaidPingList(raid.Groups.ElementAt(i).GetPingList(), raid.Location, i + 1, false)}");
                     subMessages.Remove(message.Id);
                     await message.DeleteAsync();
                  }
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
      private static Embed BuildRaidEmbed(RaidParent raid, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.DarkBlue);
         embed.WithTitle(raid.Boss.Name.Equals(RaidBoss.DefaultName) ? "Empty Raid" : $"{raid.Boss.Name} Raid {BuildRaidTitle(raid.Tier)}");
         embed.WithDescription("Press ? for help.");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Time", raid.Time, true);
         embed.AddField("Location", raid.Location, true);

         if (raid is Raid)
         {
            for (int i = 0; i < raid.Groups.Count; i++)
            {
               embed.AddField($"Group {i + 1} Ready ({raid.Groups.ElementAt(i).GetHereCount()}/{raid.Groups.ElementAt(i).TotalPlayers()})", $"{BuildPlayerList(raid.Groups.ElementAt(i).GetReadonlyHere())}");
               embed.AddField($"Group {i + 1} Attending", $"{BuildPlayerList(raid.Groups.ElementAt(i).GetReadonlyAttending())}");
               embed.AddField($"Group {i + 1} Remote", $"{BuildInvitedList(raid.Groups.ElementAt(i).GetReadonlyInvited())}");
            }
            embed.AddField($"Need Invite:", $"{BuildRequestInviteList(raid.GetReadonlyInviteList())}");
            embed.WithFooter("Note: the max number of members in a raid is 20, and the max number of invites is 10.");
         }
         else if (raid is RaidMule mule)
         {
            embed.AddField($"Mules", $"{BuildPlayerList(mule.Mules.GetReadonlyAttending())}");
            for (int i = 0; i < raid.Groups.Count; i++)
               embed.AddField($"Group {i + 1} Remote", $"{BuildInvitedList(raid.Groups.ElementAt(i).GetReadonlyInvited())}");
            embed.AddField($"Need Invite:", $"{BuildRequestInviteList(raid.GetReadonlyInviteList())}");
            embed.WithFooter("Note: The max number of invites is 10, and the max number of invites per person is 5.");
         }
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
         embed.WithTitle($"Boss Selection");
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
      private static string BuildRaidTitle(short tier)
      {
         if(tier == MEGA_RAID_TIER)
            return Emote.Parse(Environment.GetEnvironmentVariable("MEGA_EMOTE")).ToString();
         StringBuilder sb = new StringBuilder();
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
      private static string BuildRaidPingList(List<SocketGuildUser> players, string location, int groupNumber, bool isNormalRaid)
      {
         StringBuilder sb = new StringBuilder();
         foreach (SocketGuildUser player in players)
            sb.Append($"{player.Mention} ");
         if (isNormalRaid)
            sb.Append($"Everyone in Group {groupNumber} is ready at {location}");
         else
            sb.Append($"Invites are going out to Group {groupNumber} at {location}");
         return sb.ToString();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="players"></param>
      /// <param name="editor"></param>
      /// <param name="field"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      private static string BuildEditPingList(List<SocketGuildUser> players, SocketGuildUser editor, string field, string value)
      {
         StringBuilder sb = new StringBuilder();
         foreach (SocketGuildUser player in players)
            sb.Append($"{player.Mention} ");
         sb.Append($"{editor.Nickname ?? editor.Username} has changed {field} to {value} for a raid you are in.");
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
            sb.AppendLine($"{player.Nickname ?? player.Username} {GetPlayerTeam(player)}");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Builds the raid help message.
      /// </summary>
      /// <returns>Raid help messsage as a string.</returns>
      private static string BuildRaidHelpMessage(IEmote[] emojis, string[] descriptions)
      {
         int offset = 0;
         StringBuilder sb = new StringBuilder();
         sb.AppendLine("Raid Help:");

         if (selectionEmojis.Contains(emojis[0]))
         {
            foreach (IEmote emoji in emojis)
               if (selectionEmojis.Contains(emoji))
                  offset++;
            sb.AppendLine($"{emojis[0]} - {emojis[offset - 1]} {descriptions[0]}");
         }

         for (int i = offset; i < emojis.Length; i++)
         {
            if (offset == 0)
               sb.AppendLine($"{emojis[i]} {descriptions[i]}");
            else
               sb.AppendLine($"{emojis[i]} {descriptions[i - offset + 1]}");
         }

         sb.AppendLine("\nRaid Edit:");
         sb.AppendLine("To edit the raid send the edit command with the following code: ");
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
         foreach (KeyValuePair<ulong, RaidParent> temp in raidMessages)
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
      /// Checks if a message is a raid sub message.
      /// </summary>
      /// <param name="id">Id of the message to check.</param>
      /// <returns>True if the message is a raid sub message, otherwise false.</returns>
      public static bool IsRaidSubMessage(ulong id)
      {
         return subMessages.ContainsKey(id);
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

   /// <summary>
   /// 
   /// </summary>
   public struct RaidSubMessage
   {
      public int SubMessageType;
      public ulong MainMessageId;
   }
}