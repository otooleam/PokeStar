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
using PokeStar.PreConditions;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles raid commands.
   /// </summary>
   public class RaidCommands : ModuleBase<SocketCommandContext>
   {
      private static readonly Color RaidMessageColor = Color.Blue;

      private static readonly Dictionary<ulong, RaidParent> raidMessages = new Dictionary<ulong, RaidParent>();
      private static readonly Dictionary<ulong, Tuple<int, ulong>> subMessages = new Dictionary<ulong, Tuple<int, ulong>>();

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

      private static readonly string[] raidEmojisDesc = {
         "are the number of Trainers in your group, physically with you or that you are inviting remotely.",
         "means you are ready for the raid to begin. Nona will tag everyone in a post when all trainers are ready for the raid to begin.",
         "means you will be either doing the raid remotely yourself or need another trainer to send you an invite to raid remotely.",
         "means you are willing to invite one of the trainers who are asking for a remote invite. Nona will ask you which trainer you are inviting, and they will be automatically counted as part of the raid. Nona will send them a message so they know you will invite them.",
         "means you want to remove yourself from the raid. Nona will send a message to anyone you were planning to invite."
      };

      private static readonly string[] muleEmojisDesc = {
         "means you are able to invite others to the raid.",
         "means that a raid group is ready to go. Nona will tag you in a post when the raid mule is ready to start the raid.",
         "means you need a raid mule to send you an invite to the raid.",
         "means you are willing to invite one of the trainers who are asking for a remote invite. Nona will ask you who you want to invite, and that trainer will be sent a message so they know you plan to invite them.",
         "means you want to remove yourself from the raid. Nona will send a message to anyone you were planning to invite."
      };

      private static readonly IEmote[] remoteEmojis = {
         new Emoji("✈️"),
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
         new Emoji("5️⃣"),
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

      private enum REMOTE_EMOJI_INDEX
      {
         REQUEST_INVITE,
         REMOTE_PLAYER_1,
         REMOTE_PLAYER_2,
         REMOTE_PLAYER_3,
         REMOTE_PLAYER_4,
         REMOTE_PLAYER_5,
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

      private static readonly short EX_RAID_TIER = 9;
      private static readonly short MEGA_RAID_TIER = 7;
      private static readonly short LEGENDARY_RAID_TIER = 5;
      private static readonly short RARE_RAID_TIER = 3;
      private static readonly short COMMON_RAID_TIER = 1;
      private static readonly short INVALID_TIER = 0;

      [Command("raid")]
      [Summary("Creates a new interactive raid coordination message.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "3, rare, R\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task Raid([Summary("Tier of the raid.")] string tier,
                             [Summary("Time the raid will start.")] string time,
                             [Summary("Where the raid will be.")][Remainder] string location)
      {
         await GenerateRaidMessage("raid", raidEmojis, tier, time, location);
         RemoveOldRaids();
      }

      [Command("mule")]
      [Alias("raidmule")]
      [Summary("Creates a new interactive remote raid coordination message.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "3, rare, R\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task RaidMule([Summary("Tier of the raid.")] string tier,
                                 [Summary("Time the raid will start.")] string time,
                                 [Summary("Where the raid will be.")][Remainder] string location)
      {
         await GenerateRaidMessage("mule", muleEmojis, tier, time, location);
         RemoveOldRaids();
      }


      [Command("edit")]
      [Alias("editRaid", "raidEdit")]
      [Summary("Edit the time or location of a raid.")]
      [RegisterChannel('R')]
      public async Task Edit([Summary("Raid code given by the raid help message. (copy and paste this)")] ulong code,
                             [Summary("Portion of the raid message you want to change.")] string attribute,
                             [Summary("New value of you want the raid to display.")][Remainder] string edit)
      {
         if (raidMessages.ContainsKey(code))
         {
            SocketUserMessage readMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(code);
            RaidParent raid = raidMessages[code];
            bool editComplete = false;
            bool simpleEdit = false;
            if (attribute.Equals("time", StringComparison.OrdinalIgnoreCase))
            {
               raid.Time = edit;
               simpleEdit = true;
            }
            else if (attribute.Equals("location", StringComparison.OrdinalIgnoreCase) || attribute.Equals("loc", StringComparison.OrdinalIgnoreCase))
            {
               raid.Location = edit;
               simpleEdit = true;
            }
            else if (attribute.Equals("tier", StringComparison.OrdinalIgnoreCase) || attribute.Equals("boss", StringComparison.OrdinalIgnoreCase))
            {
               short calcTier = GenerateTier(edit);
               List<string> potentials = Connections.GetBossList(calcTier);

               if (potentials.Count > 1)
               {
                  raid.Tier = calcTier;
                  raid.SetBoss(null);
                  raid.RaidBossSelections = potentials;
                  string fileName = $"Egg{calcTier}.png";
                  await readMessage.DeleteAsync();
                  raidMessages.Remove(code);

                  Connections.CopyFile(fileName);
                  RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
                  for (int i = 0; i < potentials.Count; i++)
                  {
                     await selectMsg.AddReactionAsync(Global.SELECTION_EMOJIS[i]);
                  }
                  raidMessages.Add(selectMsg.Id, raid);
                  Connections.DeleteFile(fileName);
                  editComplete = true;
               }
               else if (potentials.Count == 1)
               {
                  raid.Tier = calcTier;
                  raid.SetBoss(potentials.First());
                  string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
                  IEmote[] prevReactions = readMessage.Reactions.Keys.ToArray();
                  await readMessage.DeleteAsync();
                  raidMessages.Remove(code);

                  Connections.CopyFile(fileName);
                  RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
                  await raidMsg.AddReactionsAsync(prevReactions);
                  raidMessages.Add(raidMsg.Id, raid);
                  Connections.DeleteFile(fileName);
                  editComplete = true;
               }
               else if (Global.USE_EMPTY_RAID)
               {
                  raid.Tier = calcTier;
                  raid.SetBoss(Global.DEFAULT_RAID_BOSS_NAME);
                  string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
                  IEmote[] prevReactions = readMessage.Reactions.Keys.ToArray();
                  await readMessage.DeleteAsync();
                  raidMessages.Remove(code);

                  Connections.CopyFile(fileName);
                  RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
                  await raidMsg.AddReactionsAsync(prevReactions);
                  raidMessages.Add(raidMsg.Id, raid);
                  Connections.DeleteFile(fileName);
                  editComplete = true;
               }
               else
               {
                  await ResponseMessage.SendErrorMessage(Context, "edit", $"No raid bosses found for tier {edit}.");
               }
            }
            else
            {
               await ResponseMessage.SendErrorMessage(Context, "edit", "Please enter a valid field to edit.");
            }

            if (simpleEdit)
            {
               string fileName = Connections.GetPokemonPicture(raid.Boss.Name);
               Connections.CopyFile(fileName);
               await readMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildRaidEmbed(raid, fileName);
               });
               Connections.DeleteFile(fileName);
            }

            if (simpleEdit || editComplete)
            {
               List<SocketGuildUser> allUsers = new List<SocketGuildUser>();
               foreach (RaidGroup group in raid.Groups)
                  allUsers.AddRange(group.GetNotifyList());
               allUsers.AddRange(raid.GetReadonlyInviteList());
               if (raid is RaidMule mule)
               {
                  allUsers.AddRange(mule.Mules.GetReadonlyAttending().Keys);
               }
               await ReplyAsync(BuildEditPingList(allUsers.ToImmutableList(), (SocketGuildUser)Context.User, attribute, edit));
               await Context.Message.DeleteAsync();
            }
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context, "edit", "Raid message could not be found.");
         }
      }

      [Command("bosslist")]
      [Alias("boss", "bossess", "raidboss", "raidbosses")]
      [Summary("Get the current list of raid bosses.")]
      [RegisterChannel('R')]
      public async Task Bosses()
      {
         List<string> exBosses = SilphData.GetRaidBossesTier(EX_RAID_TIER);
         List<string> megaBosses = SilphData.GetRaidBossesTier(MEGA_RAID_TIER);
         List<string> legendaryBosses = SilphData.GetRaidBossesTier(LEGENDARY_RAID_TIER);
         List<string> rareBosses = SilphData.GetRaidBossesTier(RARE_RAID_TIER);
         List<string> commonBosses = SilphData.GetRaidBossesTier(COMMON_RAID_TIER);

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(RaidMessageColor);
         embed.WithTitle("Current Raid Bosses:");
         embed.AddField($"EX Raids {BuildRaidTitle(EX_RAID_TIER)}", BuildRaidBossListString(exBosses), true);
         embed.AddField($"Mega Raids {BuildRaidTitle(MEGA_RAID_TIER)}", BuildRaidBossListString(megaBosses), true);
         embed.AddField($"Tier 5 Raids {BuildRaidTitle(LEGENDARY_RAID_TIER)}", BuildRaidBossListString(legendaryBosses), true);
         embed.AddField($"Tier 3 Raids {BuildRaidTitle(RARE_RAID_TIER)}", BuildRaidBossListString(rareBosses), true);
         embed.AddField($"Tier 1 Raids {BuildRaidTitle(COMMON_RAID_TIER)}", BuildRaidBossListString(commonBosses), true);

         await Context.Channel.SendMessageAsync(embed: embed.Build());
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
               await selectMsg.AddReactionAsync(Global.SELECTION_EMOJIS[i]);
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
         else if (Global.USE_EMPTY_RAID)
         {
            string boss = Global.DEFAULT_RAID_BOSS_NAME;
            raid = GenerateRaidType(command.Equals("raid", StringComparison.OrdinalIgnoreCase), calcTier, time, location, boss);
            fileName = Connections.GetPokemonPicture(raid.Boss.Name);

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
            await raidMsg.AddReactionsAsync(emojis);
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context, command, $"No raid bosses found for tier {tier}");
         }
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
         {
            return new Raid(tier, time, location, boss);
         }
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
         {
            return MEGA_RAID_TIER;
         }
         if (tier.Equals("l", StringComparison.OrdinalIgnoreCase) ||
             tier.Equals("legendary", StringComparison.OrdinalIgnoreCase) ||
             tier.Equals("5", StringComparison.OrdinalIgnoreCase))
         {
            return LEGENDARY_RAID_TIER;
         }
         if (tier.Equals("r", StringComparison.OrdinalIgnoreCase) ||
             tier.Equals("rare", StringComparison.OrdinalIgnoreCase) ||
             tier.Equals("3", StringComparison.OrdinalIgnoreCase))
         {
            return RARE_RAID_TIER;
         }
         if (tier.Equals("c", StringComparison.OrdinalIgnoreCase) ||
             tier.Equals("common", StringComparison.OrdinalIgnoreCase) ||
             tier.Equals("1", StringComparison.OrdinalIgnoreCase))
         {
            return COMMON_RAID_TIER;
         }
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
               if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
               {
                  parent.SetBoss(parent.RaidBossSelections[i]);
                  await message.DeleteAsync();
                  raidMessages.Remove(message.Id);

                  string filename = Connections.GetPokemonPicture(parent.Boss.Name);
                  Connections.CopyFile(filename);
                  RestUserMessage raidMsg = await reaction.Channel.SendFileAsync(filename, embed: BuildRaidEmbed(parent, filename));

                  if (parent is Raid)
                  {
                     await raidMsg.AddReactionsAsync(raidEmojis);
                  }
                  else if (parent is RaidMule)
                  {
                     await raidMsg.AddReactionsAsync(muleEmojis);
                  }

                  raidMessages.Add(raidMsg.Id, parent);
                  Connections.DeleteFile(filename);
                  return;
               }
            }
         }
         else
         {
            if (parent is Raid raid)
            {
               await RaidReactionHandle(message, reaction, raid);
            }
            else if (parent is RaidMule mule)
            {
               await RaidMuleReactionHandle(message, reaction, mule);
            }
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
         int subMessageType = subMessages[message.Id].Item1;
         if (subMessageType == (int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE)
         {
            await RaidInviteReactionHandle(message, reaction);
         }
         else if (subMessageType == (int)SUB_MESSAGE_TYPES.RAID_REMOTE_SUB_MESSAGE)
         {
            await RaidRemoteReactionHandle(message, reaction);
         }
         else if (subMessageType == (int)SUB_MESSAGE_TYPES.MULE_READY_SUB_MESSAGE)
         {
            await RaidMuleReadyReactionHandle(message, reaction);
         }
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
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.PLAYER_READY]))
            {
               int group = raid.PlayerReady(reactingPlayer);
               if (group != Global.NOT_IN_RAID)
               {
                  await reaction.Channel.SendMessageAsync(BuildRaidPingList(raid.Groups.ElementAt(group).GetPingList(), raid.Location, group + 1, true));
               }
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID]))
            {
               RestUserMessage remoteMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.Mention}",
                  embed: BuildPlayerRemoteEmbed(reactingPlayer.Nickname ?? reactingPlayer.Username));
               await remoteMsg.AddReactionsAsync(remoteEmojis);
               await remoteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
               subMessages.Add(remoteMsg.Id, new Tuple<int, ulong>((int)SUB_MESSAGE_TYPES.RAID_REMOTE_SUB_MESSAGE, message.Id));
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.INVITE_PLAYER]))
            {
               if (raid.IsInRaid(reactingPlayer, false) != Global.NOT_IN_RAID)
               {
                  if (raid.GetReadonlyInviteList().IsEmpty)
                  {
                     await reaction.Channel.SendMessageAsync($"{reactingPlayer.Mention}, There are no players to invite.");
                  }
                  else
                  {
                     if (!raid.HasActiveInvite())
                     {
                        raid.InvitingPlayer = reactingPlayer;
                        int offset = raid.InvitePage * Global.SELECTION_EMOJIS.Length;
                        int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, Global.SELECTION_EMOJIS.Length);
                        RestUserMessage inviteMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.Mention}",
                           embed: BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize));
                        for (int i = 0; i < listSize; i++)
                           await inviteMsg.AddReactionAsync(Global.SELECTION_EMOJIS[i]);

                        if (raid.GetReadonlyInviteList().Count > Global.SELECTION_EMOJIS.Length)
                        {
                           await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]);
                           await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]);
                        }

                        await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
                        subMessages.Add(inviteMsg.Id, new Tuple<int, ulong>((int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE, message.Id));
                     }
                  }
               }
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               Tuple<int, List<SocketGuildUser>> returnValue = raid.RemovePlayer(reactingPlayer);

               foreach (SocketGuildUser invite in returnValue.Item2)
                  await invite.SendMessageAsync($"{reactingPlayer.Nickname ?? reactingPlayer.Username} has left the raid. You have been moved back to \"Need Invite\".");

               if (returnValue.Item1 != Global.NOT_IN_RAID)
                  await reaction.Channel.SendMessageAsync(BuildRaidPingList(raid.Groups.ElementAt(returnValue.Item1).GetPingList(), raid.Location, returnValue.Item1 + 1, true));
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.HELP]))
            {
               string prefix = Connections.Instance().GetPrefix(((SocketGuildChannel)message.Channel).Guild.Id);
               await reactingPlayer.SendMessageAsync(BuildRaidHelpMessage(raidEmojis, raidEmojisDesc));
               await reactingPlayer.SendMessageAsync($"{prefix}edit {message.Id}");
               needsUpdate = false;
            }
            else
            {
               needsUpdate = false;
            }

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
               {
                  await reaction.Channel.SendMessageAsync($"{reactingPlayer.Mention}, There are no players Invited.");
               }
               else if (raid.IsInRaid(reactingPlayer, false) != Global.NOT_IN_RAID)
               {
                  RestUserMessage readyMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.Mention}",
                     embed: BuildMuleReadyEmbed(raid.Groups.Count, reactingPlayer.Nickname ?? reactingPlayer.Username));
                  for (int i = 0; i < raid.Groups.Count; i++)
                  {
                     await readyMsg.AddReactionAsync(Global.SELECTION_EMOJIS[i]);
                  }
                  await readyMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
                  subMessages.Add(readyMsg.Id, new Tuple<int, ulong>((int)SUB_MESSAGE_TYPES.MULE_READY_SUB_MESSAGE, message.Id));
               }
            }
            else if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.REQUEST_INVITE]))
            {
               raid.RequestInvite(reactingPlayer);
            }
            else if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.INVITE_PLAYER]))
            {
               if (raid.IsInRaid(reactingPlayer, false) != Global.NOT_IN_RAID)
               {
                  if (raid.GetReadonlyInviteList().Count == 0)
                     await reaction.Channel.SendMessageAsync($"{reactingPlayer.Mention}, There are no players to invite.");
                  else
                  {
                     if (!raid.HasActiveInvite())
                     {
                        raid.InvitingPlayer = reactingPlayer;
                        int offset = raid.InvitePage * Global.SELECTION_EMOJIS.Length;
                        int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, Global.SELECTION_EMOJIS.Length);
                        RestUserMessage inviteMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.Mention}",
                           embed: BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize));
                        for (int i = 0; i < listSize; i++)
                           await inviteMsg.AddReactionAsync(Global.SELECTION_EMOJIS[i]);

                        if (raid.GetReadonlyInviteList().Count > Global.SELECTION_EMOJIS.Length)
                        {
                           await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]);
                           await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]);
                        }

                        await inviteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
                        subMessages.Add(inviteMsg.Id, new Tuple<int, ulong>((int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE, message.Id));
                     }
                  }
               }
            }
            else if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               List<SocketGuildUser> returnValue = raid.RemovePlayer(reactingPlayer).Item2;

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
         ulong raidMessageId = subMessages[message.Id].Item2;
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
               raid.ChangeInvitePage(false, Global.SELECTION_EMOJIS.Length);
               int offset = raid.InvitePage * Global.SELECTION_EMOJIS.Length;
               int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, Global.SELECTION_EMOJIS.Length);
               SocketUserMessage inviteMessage = (SocketUserMessage)await reaction.Channel.GetMessageAsync(raidMessageId);
               await inviteMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize);
               });
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]))
            {
               raid.ChangeInvitePage(true, Global.SELECTION_EMOJIS.Length);
               int offset = raid.InvitePage * Global.SELECTION_EMOJIS.Length;
               int listSize = Math.Min(raid.GetReadonlyInviteList().Count - offset, Global.SELECTION_EMOJIS.Length);
               SocketUserMessage inviteMessage = (SocketUserMessage)await reaction.Channel.GetMessageAsync(raidMessageId);
               await inviteMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildPlayerInviteEmbed(raid.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize);
               });
            }
            else
            {
               for (int i = 0; i < Global.SELECTION_EMOJIS.Length; i++)
               {
                  if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
                  {
                     int offset = raid.InvitePage * Global.SELECTION_EMOJIS.Length;
                     SocketGuildUser player = raid.GetReadonlyInviteList().ElementAt(i + offset);
                     if (raid.InvitePlayer(player, reactingPlayer))
                     {
                        SocketUserMessage raidMessage = (SocketUserMessage)await reaction.Channel.GetMessageAsync(raidMessageId);
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
         ulong raidMessageId = subMessages[message.Id].Item2;
         bool needEdit = false;
         Raid raid = (Raid)raidMessages[raidMessageId];
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;

         if (message.MentionedUserIds.Contains(reactingPlayer.Id))
         {
            if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            {
               await message.DeleteAsync();
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REQUEST_INVITE])) // Need Invite
            {
               raid.RequestInvite(reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_1]))
            {
               raid.PlayerAdd(reactingPlayer, 1, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_2]))
            {
               raid.PlayerAdd(reactingPlayer, 2, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_3]))
            {
               raid.PlayerAdd(reactingPlayer, 3, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_4]))
            {
               raid.PlayerAdd(reactingPlayer, 4, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_5]))
            {
               raid.PlayerAdd(reactingPlayer, 5, reactingPlayer);
               needEdit = true;
            }

            if (needEdit)
            {
               SocketUserMessage raidMessage = (SocketUserMessage)await reaction.Channel.GetMessageAsync(raidMessageId);
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
         ulong raidMuleMessageId = subMessages[message.Id].Item2;
         RaidMule raid = (RaidMule)raidMessages[raidMuleMessageId];

         if (message.MentionedUserIds.Contains(reaction.User.Value.Id))
         {

            if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
               await message.DeleteAsync();
            else
            {
               for (int i = 0; i < Global.SELECTION_EMOJIS.Length; i++)
               {
                  if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
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
         embed.WithColor(RaidMessageColor);
         embed.WithTitle(raid.Boss.Name.Equals(Global.DEFAULT_RAID_BOSS_NAME) ? "**Empty Raid**" : $"**{raid.Boss.Name} Raid {BuildRaidTitle(raid.Tier)}**");
         embed.WithDescription("Press ? for help.");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("**Time**", raid.Time, true);
         embed.AddField("**Location**", raid.Location, true);

         if (raid is Raid)
         {
            for (int i = 0; i < raid.Groups.Count; i++)
            {
               string groupPrefix = raid.Groups.Count == 1 ? "" : $"Group {i + 1} ";
               RaidGroup group = raid.Groups.ElementAt(i);
               int total = group.TotalPlayers();
               int ready = group.GetReadyCount() + group.GetReadyRemoteCount() + group.GetInviteCount();
               int remote = group.GetRemoteCount();

               string attendList = BuildPlayerList(group.GetReadonlyAttending());
               string readyList = BuildPlayerList(group.GetReadonlyHere());
               string invitedAttendList = BuildInvitedList(group.GetReadonlyInvitedAttending());
               string invitedReadyList = BuildInvitedList(group.GetReadonlyInvitedReady());

               embed.AddField($"**{groupPrefix}Ready {ready}/{total}** (Remote {remote}/{Global.LIMIT_RAID_INVITE})", $"{BuildTotalList(readyList, invitedAttendList)}");
               embed.AddField($"**{groupPrefix}Attending**", $"{BuildTotalList(attendList, invitedReadyList)}");
            }
            embed.AddField($"**Need Invite:**", $"{BuildRequestInviteList(raid.GetReadonlyInviteList())}");
            embed.WithFooter($"Note: the max number of members in a raid is {Global.LIMIT_RAID}, and the max number of invites is {Global.LIMIT_RAID_INVITE}.");
         }
         else if (raid is RaidMule mule)
         {
            embed.AddField($"Mules", $"{BuildPlayerList(mule.Mules.GetReadonlyAttending())}");
            for (int i = 0; i < raid.Groups.Count; i++)
            {
               embed.AddField($"{(raid.Groups.Count == 1 ? "" : $"Group {i + 1} ")}Remote", $"{BuildInvitedList(raid.Groups.ElementAt(i).GetReadonlyInvitedAll())}");
            }
            embed.AddField($"Need Invite:", $"{BuildRequestInviteList(raid.GetReadonlyInviteList())}");
            embed.WithFooter($"Note: The max number of invites is {Global.LIMIT_RAID_INVITE}, and the max number of invites per person is {Global.LIMIT_RAID_MULE_INVITE}.");
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
         {
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} {potentials[i]}");
         }

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(RaidMessageColor);
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
         {
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} {invite.ElementAt(i).Nickname ?? invite.ElementAt(i).Username}");
         }

         if (invite.Count > Global.SELECTION_EMOJIS.Length)
         {
            sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]} Previous Page");
            sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]} Next Page");
         }
         sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Cancel");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(RaidMessageColor);
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
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REQUEST_INVITE]} Need Invite");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_1]} 1 Remote Raider");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_2]} 2 Remote Raiders");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_3]} 3 Remote Raiders");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_4]} 4 Remote Raiders");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_5]} 5 Remote Raiders");
         sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Cancel");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(RaidMessageColor);
         embed.WithTitle($"**{user} - Remote**");
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
         {
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} Raid Group {i + 1}");
         }
         sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Cancel");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(RaidMessageColor);
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
         if (tier == MEGA_RAID_TIER)
         {
            return Global.NONA_EMOJIS["mega_emote"];
         }
         if (tier == EX_RAID_TIER)
         {
            return Global.NONA_EMOJIS["ex_emote"];
         }
         StringBuilder sb = new StringBuilder();
         string raidSymbol = Global.NONA_EMOJIS["raid_emote"]; ;
         for (int i = 0; i < tier; i++)
         {
            sb.Append(raidSymbol);
         }
         return sb.ToString();
      }

      /// <summary>
      /// Builds a list of players to ping when a raid group is ready.
      /// </summary>
      /// <param name="players">List of players to ping.</param>
      /// <param name="location">Location of the raid.</param>
      /// <param name="groupNumber">Group number the players are part of.</param>
      /// <returns>List of players to ping as a string.</returns>
      private static string BuildRaidPingList(ImmutableList<SocketGuildUser> players, string location, int groupNumber, bool isNormalRaid)
      {
         StringBuilder sb = new StringBuilder();
         foreach (SocketGuildUser player in players)
         {
            sb.Append($"{player.Mention} ");
         }
         if (isNormalRaid)
         {
            sb.Append($"Everyone in Group {groupNumber} is ready at {location}");
         }
         else
         {
            sb.Append($"Invites are going out to Group {groupNumber} at {location}");
         }
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
      private static string BuildEditPingList(ImmutableList<SocketGuildUser> players, SocketGuildUser editor, string field, string value)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("Edit Alert: ");
         foreach (SocketGuildUser player in players)
         {
            sb.Append($"{player.Mention} ");
         }
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
         if (players.IsEmpty)
         {
            return Global.EMPTY_FIELD;
         }

         StringBuilder sb = new StringBuilder();
         foreach (KeyValuePair<SocketGuildUser, int> player in players)
         {
            int attend = RaidGroup.GetAttending(player.Value);
            int remote = RaidGroup.GetRemote(player.Value);
            sb.AppendLine($"{Global.SELECTION_EMOJIS[attend + remote - 1]} {player.Key.Nickname ?? player.Key.Username} {GetPlayerTeam(player.Key)} ");
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
         if (players.IsEmpty)
         {
            return Global.EMPTY_FIELD;
         }

         StringBuilder sb = new StringBuilder();
         foreach (KeyValuePair<SocketGuildUser, SocketGuildUser> player in players)
         {
            sb.AppendLine($"{raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID]} {player.Key.Nickname ?? player.Key.Username} {GetPlayerTeam(player.Key)} invited by {player.Value.Nickname ?? player.Value.Username} {GetPlayerTeam(player.Value)}");
         }
         return sb.ToString();
      }

      private static string BuildTotalList(string initList, string inviteList)
      {
         if (initList.Equals(Global.EMPTY_FIELD))
         {
            return inviteList;
         }
         else if (inviteList.Equals(Global.EMPTY_FIELD))
         {
            return initList;
         }
         return initList + inviteList;
      }

      /// <summary>
      /// Builds a list of players requesting an invite to a raid.
      /// </summary>
      /// <param name="players">List of players request an invite to a raid.</param>
      /// <returns>List of players as a string.</returns>
      private static string BuildRequestInviteList(ImmutableList<SocketGuildUser> players)
      {
         if (players.Count == 0)
         {
            return Global.EMPTY_FIELD;
         }

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

         if (Global.SELECTION_EMOJIS.Contains(emojis[0]))
         {
            foreach (IEmote emoji in emojis)
            {
               if (Global.SELECTION_EMOJIS.Contains(emoji))
               {
                  offset++;
               }
            }
            sb.AppendLine($"{emojis[0]} - {emojis[offset - 1]} {descriptions[0]}");
         }

         for (int i = offset; i < emojis.Length; i++)
         {
            if (!emojis[i].Equals(raidEmojis[(int)RAID_EMOJI_INDEX.HELP]))
            {
               if (offset == 0)
               {
                  sb.AppendLine($"{emojis[i]} {descriptions[i]}");
               }
               else
               {
                  sb.AppendLine($"{emojis[i]} {descriptions[i - offset + 1]}");
               }
            }
         }
         sb.AppendLine($"\nIf you are inviting players who have not requested an invite, please use the {raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID]} to indicate the amount.");

         sb.AppendLine("\nHow To Edit:");
         sb.AppendLine("To edit the raid copy and paste the following command, and add the part of the raid you want to change and the new value: ");
         return sb.ToString();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="bosses"></param>
      /// <returns></returns>
      private static string BuildRaidBossListString(List<string> bosses)
      {
         if (bosses.Count == 0)
         {
            return Global.EMPTY_FIELD;
         }

         StringBuilder sb = new StringBuilder();
         foreach (string boss in bosses)
         {
            sb.AppendLine(boss);
         }
         return sb.ToString();
      }

      /// <summary>
      /// Getss the team role registered to a user.
      /// </summary>
      /// <param name="user">User to get team role of.</param>
      /// <returns>Team role name of the user.</returns>
      private static string GetPlayerTeam(SocketGuildUser user)
      {
         if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_VALOR, StringComparison.OrdinalIgnoreCase)) != null)
         {
            return Global.NONA_EMOJIS["valor_emote"];
         }
         else if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_MYSTIC, StringComparison.OrdinalIgnoreCase)) != null)
         {
            return Global.NONA_EMOJIS["mystic_emote"];
         }
         else if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_INSTINCT, StringComparison.OrdinalIgnoreCase)) != null)
         {
            return Global.NONA_EMOJIS["instinct_emote"];
         }
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
         {
            if (Math.Abs((temp.Value.CreatedAt - DateTime.Now).TotalDays) >= 1)
            {
               ids.Add(temp.Key);
            }
         }
         foreach (ulong id in ids)
         {
            raidMessages.Remove(id);
         }
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
         raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID] = Emote.Parse(Global.NONA_EMOJIS["remote_pass_emote"]);
         muleEmojis[(int)MULE_EMOJI_INDEX.REQUEST_INVITE] = Emote.Parse(Global.NONA_EMOJIS["remote_pass_emote"]);
         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REQUEST_INVITE] = Emote.Parse(Global.NONA_EMOJIS["remote_pass_emote"]);
      }
   }
}
