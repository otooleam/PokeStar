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

namespace PokeStar.ModuleParents
{
   public class RaidCommandParent : ModuleBase<SocketCommandContext>
   {
      /// <summary>
      /// Saved raid messages.
      /// </summary>
      protected static readonly Dictionary<ulong, RaidParent> raidMessages = new Dictionary<ulong, RaidParent>();

      /// <summary>
      /// Saved raid sub messages.
      /// </summary>
      protected static readonly Dictionary<ulong, RaidSubMessage> subMessages = new Dictionary<ulong, RaidSubMessage>();

      /// Emotes **************************************************************

      /// <summary>
      /// Emotes for a raid message.
      /// </summary>
      protected static readonly IEmote[] raidEmojis = {
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
         new Emoji("5️⃣"),
         new Emoji("✅"),
         new Emoji("✈️"),
         new Emoji("🤝"),
         new Emoji("🚫")
      };

      /// <summary>
      /// Emotes for a raid mule message.
      /// </summary>
      protected static readonly IEmote[] muleEmojis = {
         new Emoji("🐎"),
         new Emoji("✅"),
         new Emoji("✈️"),
         new Emoji("🤝"),
         new Emoji("🚫")
      };

      /// <summary>
      /// Emotes for a remote sub message.
      /// </summary>
      private static readonly IEmote[] remoteEmojis = {
         new Emoji("✈️"),
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
         new Emoji("5️⃣"),
         new Emoji("6️⃣"),
         new Emoji("🚫"),
      };

      /// <summary>
      /// Extra emotes.
      /// These will sometimes be added to messages,
      /// but not everytime.
      /// </summary>
      private static readonly Emoji[] extraEmojis = {
         new Emoji("⬅️"),
         new Emoji("➡️"),
         new Emoji("❌"),
         new Emoji("❓")
      };

      /// <summary>
      /// Descriptions for raid emotes.
      /// </summary>
      private static readonly string[] raidEmojisDesc = {
         "are the number of Trainers in your group that are raiding in person.",
         "means you are ready for the raid to begin. Nona will tag everyone in a post when all trainers are ready for the raid to begin.",
         "means you will be either doing the raid remotely yourself or need another trainer to send you an invite to raid remotely. See raid reply help for more options.",
         "means you are willing to invite one of the trainers who are asking for a remote invite. Nona will ask you which trainer you are inviting, and they will be automatically counted as part of the raid. Nona will send them a message so they know you will invite them. See raid reply help for more options.",
         "means you want to remove yourself from the raid. Nona will send a message to anyone you were planning to invite."
      };

      /// <summary>
      /// Descriptions for raid mule emotes.
      /// </summary>
      private static readonly string[] muleEmojisDesc = {
         "means you are able to invite others to the raid.",
         "means that a raid group is ready to go. Nona will tag you in a post when the raid mule is ready to start the raid. See raid reply help for more options.",
         "means you need a raid mule to send you an invite to the raid.",
         "means you are willing to invite one of the trainers who are asking for a remote invite. Nona will ask you who you want to invite, and that trainer will be sent a message so they know you plan to invite them. See raid reply help for more options.",
         "means you want to remove yourself from the raid. Nona will send a message to anyone you were planning to invite."
      };

      /// <summary>
      /// Descriptions for raid train emotes.
      /// Only emotes added onto raid.
      /// </summary>
      private static readonly string[] trainEmojisDesc = {
         "means return to the previous gym.",
         "means continue to the next gym"
      };

      /// Replies *************************************************************

      /// <summary>
      /// Replies for a raid message.
      /// </summary>
      private static readonly RaidReplyInfo[] raidReplies = {
         new RaidReplyInfo("edit", "Edit the time, location (loc), or tier/boss of a raid.", 
            new List<string> { 
               "<attribute>: Portion of the raid message to edit.", 
               "<value>: New value of the edited attribute."
            }),
         new RaidReplyInfo("invite", "Invite user(s) to the raid. Users must be mentioned with \'@\' to be added.",
            new List<string> {
               "<invites>: Tagged list of users to invite, separated by spaces.",
            }),
         new RaidReplyInfo("request", "Request an invite to the raid.",
            new List<string> ()),
         new RaidReplyInfo("remote", "Participate in the raid remotly without an invite.",
            new List<string> {
               "<amount>: Amount of users raiding remotly 0 - 6.",
            }),
      };

      /// <summary>
      /// Replies for a raid mule message.
      /// </summary>
      private static readonly RaidReplyInfo[] muleReplies = {
         new RaidReplyInfo("edit", "Edit the time, location (loc), or tier/boss of a raid.",
            new List<string> {
               "<attribute>\nPortion of the raid message to edit.",
               "<value>\nNew value of the edited attribute."
            }),
         new RaidReplyInfo("invite", "Invite user(s) to the raid. Users must be mentioned with \'@\' to be added.",
            new List<string> {
               "<invites>\nTagged list of users to invite, separated by spaces.",
            }),
         new RaidReplyInfo("ready", "Participate in the raid remotly without an invite.",
            new List<string> {
               "<groupNum>\nNumber of the group that is ready to start.",
            }),
      };

      /// <summary>
      /// Replies for a raid train message.
      /// </summary>
      private static readonly RaidReplyInfo[] trainReplies = {
         new RaidReplyInfo("edit", "Edit the time, location (loc), or tier/boss of a raid.",
            new List<string> {
               "<attribute>: Portion of the raid message to edit.",
               "<value>: New value of the edited attribute."
            }),
         new RaidReplyInfo("invite", "Invite user(s) to the raid. Users must be mentioned with \'@\' to be added.",
            new List<string> {
               "<invites>: Tagged list of users to invite, separated by spaces.",
            }),
         new RaidReplyInfo("request", "Request an invite to the raid.",
            new List<string> ()),
         new RaidReplyInfo("remote", "Participate in the raid remotly without an invite.",
            new List<string> {
               "<amount>: Amount of users raiding remotly 0 - 6.",
            }),
         new RaidReplyInfo("add", "Add a gym to the end of the raid train.",
            new List<string> {
               "<gym>: Name of the gym.",
            }),
      };

      /// Enumerations ********************************************************

      /// <summary>
      /// Index of emotes on a raid message.
      /// </summary>
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
         REMOVE_PLAYER
      }

      /// <summary>
      /// Index of emotes on a raid mule message.
      /// </summary>
      private enum MULE_EMOJI_INDEX
      {
         ADD_MULE,
         RAID_READY,
         REQUEST_INVITE,
         INVITE_PLAYER,
         REMOVE_PLAYER
      }

      /// <summary>
      /// Index of emotes on a remote sub message.
      /// </summary>
      private enum REMOTE_EMOJI_INDEX
      {
         REQUEST_INVITE,
         REMOTE_PLAYER_1,
         REMOTE_PLAYER_2,
         REMOTE_PLAYER_3,
         REMOTE_PLAYER_4,
         REMOTE_PLAYER_5,
         REMOTE_PLAYER_6,
         REMOVE_REMOTE,
      }

      /// <summary>
      /// Index of extra emotes.
      /// </summary>
      private enum EXTRA_EMOJI_INDEX
      {
         BACK_ARROW,
         FORWARD_ARROR,
         CANCEL,
         HELP
      }

      /// <summary>
      /// Types of raid sub messages.
      /// </summary>
      private enum SUB_MESSAGE_TYPES
      {
         INVITE_SUB_MESSAGE,
         RAID_REMOTE_SUB_MESSAGE,
         MULE_READY_SUB_MESSAGE
      }

      /// <summary>
      /// Index of replies to a raid message.
      /// </summary>
      private enum RAID_REPLY_INDEX
      {
         EDIT,
         INVITE,
         REQUEST,
         REMOTE
      }

      /// <summary>
      /// Index of replies to a raid mule message.
      /// </summary>
      private enum MULE_REPLY_INDEX
      {
         EDIT,
         INVITE,
         READY,
      }

      /// <summary>
      /// Index of replies to a raid train message.
      /// </summary>
      private enum TRAIN_REPLY_INDEX
      {
         EDIT,
         INVITE,
         REQUEST,
         REMOTE,
         ADD,
      }

      /// Message checkers ****************************************************

      /// <summary>
      /// Checks if a message is a raid message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a raid message, otherwise false.</returns>
      public static bool IsRaidMessage(ulong id)
      {
         return raidMessages.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a raid sub message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a raid sub message, otherwise false.</returns>
      public static bool IsRaidSubMessage(ulong id)
      {
         return subMessages.ContainsKey(id);
      }

      /// Message reaction handlers *******************************************

      /// <summary>
      /// Handles a reaction on a general raid message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
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

                  string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
                  Connections.CopyFile(fileName);
                  if (parent is Raid raid)
                  {
                     if (raid is RaidTrain train)
                     {
                        RestUserMessage raidMsg = await reaction.Channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(train, fileName));
                        await SetEmojis(raidMsg, raidEmojis, true);
                        raidMessages.Add(raidMsg.Id, parent);
                     }
                     else
                     {
                        RestUserMessage raidMsg = await reaction.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
                        await SetEmojis(raidMsg, raidEmojis);
                        raidMessages.Add(raidMsg.Id, parent);
                     }
                  }
                  else if (parent is RaidMule mule)
                  {
                     RestUserMessage raidMsg = await reaction.Channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(mule, fileName));
                     await SetEmojis(raidMsg, muleEmojis);
                     raidMessages.Add(raidMsg.Id, parent);
                  }
                  Connections.DeleteFile(fileName);
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
      /// Handles a reaction on a raid sub message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      public static async Task RaidSubMessageReactionHandle(IMessage message, SocketReaction reaction)
      {
         int subMessageType = subMessages[message.Id].Type;
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

      ///  Message reaction handlers ******************************************

      /// <summary>
      /// Handles a reply to a raid message.
      /// </summary>
      /// <param name="message">Rely message.</param>
      /// <param name="prefix">Command prefix used for the guild.</param>
      /// <param name="argPos">Posision of the first argument.</param>
      /// <returns></returns>
      public static async Task RaidMessageReplyHandle(SocketUserMessage message, string prefix, int argPos)
      {
         ISocketMessageChannel channel = message.Channel;
         ulong code = message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await channel.GetMessageAsync(code);
         SocketGuildUser author = (SocketGuildUser)message.Author;
         RaidParent parent = raidMessages[code];
         string[] msgContent = message.Content.Split(' ');
         bool needEdit = false;

         if (msgContent[0].Equals($"{prefix}{raidReplies[(int)RAID_REPLY_INDEX.EDIT].Command}", StringComparison.OrdinalIgnoreCase)
            || msgContent[0].Equals($"{prefix}{muleReplies[(int)MULE_REPLY_INDEX.EDIT].Command}", StringComparison.OrdinalIgnoreCase)
            || msgContent[0].Equals($"{prefix}{trainReplies[(int)TRAIN_REPLY_INDEX.EDIT].Command}", StringComparison.OrdinalIgnoreCase))
         {
            string attribute = msgContent[argPos];
            string edit = string.Join(" ", msgContent, argPos + 1, msgContent.Length - (argPos + 1));
            bool success = await EditRaid(raidMessage, parent, attribute, edit);

            if (success)
            {
               List<SocketGuildUser> allUsers = parent.GetAllUsers();
               await channel.SendMessageAsync(BuildEditPingList(allUsers.ToImmutableList(), author, attribute, edit));
            }
         }
         else if (msgContent[0].Equals($"{prefix}{raidReplies[(int)RAID_REPLY_INDEX.INVITE].Command}", StringComparison.OrdinalIgnoreCase) 
            || msgContent[0].Equals($"{prefix}{muleReplies[(int)MULE_REPLY_INDEX.INVITE].Command}", StringComparison.OrdinalIgnoreCase)
            || msgContent[0].Equals($"{prefix}{trainReplies[(int)TRAIN_REPLY_INDEX.INVITE].Command}", StringComparison.OrdinalIgnoreCase))
         {
            if (!parent.HasActiveInvite())
            {
               parent.InvitingPlayer = author;
               List<SocketUser> invites = message.MentionedUsers.ToList();
               foreach (SocketGuildUser invite in invites)
               {
                  if (parent.InvitePlayer(invite, author))
                  {
                     await invite.SendMessageAsync($"You have been invited to a raid by {author.Nickname ?? author.Username}.");
                  }
               }
               needEdit = true;
               parent.InvitingPlayer = null;
            }
         }
         else if (parent is Raid raid)
         {
            if (msgContent[0].Equals($"{prefix}{raidReplies[(int)RAID_REPLY_INDEX.REQUEST].Command}", StringComparison.OrdinalIgnoreCase)
               || msgContent[0].Equals($"{prefix}{trainReplies[(int)TRAIN_REPLY_INDEX.REQUEST].Command}", StringComparison.OrdinalIgnoreCase))
            {
               raid.RequestInvite(author);
               needEdit = true;
            }
            else if (msgContent[0].Equals($"{prefix}{raidReplies[(int)RAID_REPLY_INDEX.REMOTE].Command}", StringComparison.OrdinalIgnoreCase)
               || msgContent[0].Equals($"{prefix}{trainReplies[(int)TRAIN_REPLY_INDEX.REMOTE].Command}", StringComparison.OrdinalIgnoreCase))
            {
               bool isNumber = int.TryParse(msgContent[argPos], out int groupSize);
               if (isNumber && groupSize >= 0 && groupSize <= 6)
               {
                  raid.AddPlayer(author, groupSize, author);
                  needEdit = true;
               }
               else
               {
                  await ResponseMessage.SendErrorMessage(channel, "remote", "Value must be a number between 0 and 6");
               }
            }
            else if (raid is RaidTrain train)
            {
               if (msgContent[0].Equals($"{prefix}{trainReplies[(int)TRAIN_REPLY_INDEX.ADD].Command}", StringComparison.OrdinalIgnoreCase))
               {
                  train.AddGym(msgContent[argPos]);
                  needEdit = true;
               }
            }
         }
         else if (parent is RaidMule mule)
         {
            if (msgContent[0].Equals($"{prefix}{muleReplies[(int)MULE_REPLY_INDEX.READY].Command}", StringComparison.OrdinalIgnoreCase))
            {
               bool isNumber = int.TryParse(msgContent[argPos], out int groupNum);
               if(isNumber && mule.GetTotalGroups() <= groupNum  && groupNum > 0)
               {
                  await channel.SendMessageAsync($"{BuildRaidPingList(mule.GetGroup(groupNum - 1).GetPingList(), mule.Location, groupNum, false)}");
               }
            }
         }

         if (needEdit && parent.Boss != null)
         {
            string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
            Connections.CopyFile(fileName);
            if (parent is Raid raid)
            {
               if (raid is RaidTrain train)
               {
                  await raidMessage.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidTrainEmbed(train, fileName);
                  });
               }
               else
               {
                  await raidMessage.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidEmbed(raid, fileName);
                  });
               }
            }
            else if (parent is RaidMule mule)
            {
               await raidMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildRaidMuleEmbed(mule, fileName);
               });
            }
            Connections.DeleteFile(fileName);
            subMessages.Remove(message.Id);
         }
         await message.DeleteAsync();
      }

      /// Reaction handlers ***************************************************

      /// <summary>
      /// Handles a reaction on a raid message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="raid">Raid to apply the reaction to.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidReactionHandle(IMessage message, SocketReaction reaction, Raid raid)
      {
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;

         if (raid.InvitingPlayer == null || !raid.InvitingPlayer.Equals(reactingPlayer))
         {
            bool needsUpdate = true;
            if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_1]))
            {
               raid.AddPlayer(reactingPlayer, 1);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_2]))
            {
               raid.AddPlayer(reactingPlayer, 2);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_3]))
            {
               raid.AddPlayer(reactingPlayer, 3);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_4]))
            {
               raid.AddPlayer(reactingPlayer, 4);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_5]))
            {
               raid.AddPlayer(reactingPlayer, 5);
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.PLAYER_READY]))
            {
               int group = raid.MarkPlayerReady(reactingPlayer);
               if (group != Global.NOT_IN_RAID)
               {
                  await reaction.Channel.SendMessageAsync(BuildRaidPingList(raid.GetGroup(group).GetPingList(), raid.Location, group + 1, true));
               }
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID]))
            {
               RestUserMessage remoteMsg = await reaction.Channel.SendMessageAsync(text: $"{reactingPlayer.Mention}",
                  embed: BuildPlayerRemoteEmbed(reactingPlayer.Nickname ?? reactingPlayer.Username));
               await remoteMsg.AddReactionsAsync(remoteEmojis);
               await remoteMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
               subMessages.Add(remoteMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.RAID_REMOTE_SUB_MESSAGE, message.Id));
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
                        subMessages.Add(inviteMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE, message.Id));
                     }
                  }
               }
            }
            else if (reaction.Emote.Equals(raidEmojis[(int)RAID_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               RaidRemoveResult returnValue = raid.RemovePlayer(reactingPlayer);

               foreach (SocketGuildUser invite in returnValue.Users)
               {
                  await invite.SendMessageAsync(BuildUnInvitedMessage(reactingPlayer));
               }

               if (returnValue.Group != Global.NOT_IN_RAID)
               {
                  await reaction.Channel.SendMessageAsync(BuildRaidPingList(raid.GetGroup(returnValue.Group).GetPingList(), raid.Location, returnValue.Group + 1, true));
               }
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]))
            {
               string prefix = Connections.Instance().GetPrefix(((SocketGuildChannel)message.Channel).Guild.Id);
               if (raid is RaidTrain train)
               {
                  List<IEmote> allEmotes = raidEmojis.ToList();
                  allEmotes.Add(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]);
                  allEmotes.Add(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]);
                  await reactingPlayer.SendMessageAsync(BuildHelpMessage(allEmotes.ToArray(), raidEmojisDesc.Concat(trainEmojisDesc).ToArray(), trainReplies, prefix));
               }
               else
               {
                  await reactingPlayer.SendMessageAsync(BuildHelpMessage(raidEmojis, raidEmojisDesc, raidReplies, prefix));
               }
               needsUpdate = false;
            }
            else if (raid is RaidTrain train)
            {
               if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]))
               {
                  needsUpdate = train.PreviousGym();
               }
               else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]))
               {
                  needsUpdate = train.NextGym();
               }
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
               if (raid is RaidTrain train)
               {
                  await msg.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidTrainEmbed(train, fileName);
                  });
               }
               else
               {
                  await msg.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidEmbed(raid, fileName);
                  });
               }
               Connections.DeleteFile(fileName);
            }
         }
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reactingPlayer);
      }

      /// <summary>
      /// Handles a reaction on a raid mule message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="raid">Raid mule to apply the reaction to.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidMuleReactionHandle(IMessage message, SocketReaction reaction, RaidMule raid)
      {
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;
         if (raid.InvitingPlayer == null || !raid.InvitingPlayer.Equals(reactingPlayer))
         {
            bool needsUpdate = true;
            if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.ADD_MULE]))
            {
               raid.AddPlayer(reactingPlayer, 1);
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
                     embed: BuildMuleReadyEmbed(raid.GetTotalGroups(), reactingPlayer.Nickname ?? reactingPlayer.Username));
                  for (int i = 0; i < raid.GetTotalGroups(); i++)
                  {
                     await readyMsg.AddReactionAsync(Global.SELECTION_EMOJIS[i]);
                  }
                  await readyMsg.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
                  subMessages.Add(readyMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.MULE_READY_SUB_MESSAGE, message.Id));
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
                        subMessages.Add(inviteMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.INVITE_SUB_MESSAGE, message.Id));
                     }
                  }
               }
            }
            else if (reaction.Emote.Equals(muleEmojis[(int)MULE_EMOJI_INDEX.REMOVE_PLAYER]))
            {
               List<SocketGuildUser> returnValue = raid.RemovePlayer(reactingPlayer).Users;

               foreach (SocketGuildUser invite in returnValue)
                  await invite.SendMessageAsync($"{reactingPlayer.Nickname ?? reactingPlayer.Username} has left the raid. You have been moved back to \"Need Invite\".");
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]))
            {
               string prefix = Connections.Instance().GetPrefix(((SocketGuildChannel)message.Channel).Guild.Id);
               await reactingPlayer.SendMessageAsync(BuildHelpMessage(muleEmojis, muleEmojisDesc, muleReplies, prefix));
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
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reactingPlayer);
      }

      /// <summary>
      /// Handles a reaction on a raid invite message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      private static async Task RaidInviteReactionHandle(IMessage message, SocketReaction reaction)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         ulong raidMessageId = subMessages[message.Id].MainMessageId;
         RaidParent parent = raidMessages[raidMessageId];
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;

         if (reactingPlayer.Equals(parent.InvitingPlayer) || message.MentionedUserIds.Contains(reactingPlayer.Id))
         {
            if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            {
               await message.DeleteAsync();
               parent.InvitingPlayer = null;
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]))
            {
               parent.ChangeInvitePage(false, Global.SELECTION_EMOJIS.Length);
               int offset = parent.InvitePage * Global.SELECTION_EMOJIS.Length;
               int listSize = Math.Min(parent.GetReadonlyInviteList().Count - offset, Global.SELECTION_EMOJIS.Length);
               SocketUserMessage inviteMessage = (SocketUserMessage)await reaction.Channel.GetMessageAsync(raidMessageId);
               await inviteMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildPlayerInviteEmbed(parent.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize);
               });
            }
            else if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]))
            {
               parent.ChangeInvitePage(true, Global.SELECTION_EMOJIS.Length);
               int offset = parent.InvitePage * Global.SELECTION_EMOJIS.Length;
               int listSize = Math.Min(parent.GetReadonlyInviteList().Count - offset, Global.SELECTION_EMOJIS.Length);
               SocketUserMessage inviteMessage = (SocketUserMessage)await reaction.Channel.GetMessageAsync(raidMessageId);
               await inviteMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildPlayerInviteEmbed(parent.GetReadonlyInviteList(), reactingPlayer.Nickname ?? reactingPlayer.Username, offset, listSize);
               });
            }
            else
            {
               for (int i = 0; i < Global.SELECTION_EMOJIS.Length; i++)
               {
                  if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
                  {
                     int offset = parent.InvitePage * Global.SELECTION_EMOJIS.Length;
                     SocketGuildUser player = parent.GetReadonlyInviteList().ElementAt(i + offset);
                     if (parent.InvitePlayer(player, reactingPlayer))
                     {
                        SocketUserMessage raidMessage = (SocketUserMessage)await reaction.Channel.GetMessageAsync(raidMessageId);
                        string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
                        Connections.CopyFile(fileName);
                        if (parent is Raid raid)
                        {
                           await raidMessage.ModifyAsync(x =>
                           {
                              x.Embed = BuildRaidEmbed(raid, fileName);
                           });
                        }
                        if (parent is RaidMule mule)
                        {
                           await raidMessage.ModifyAsync(x =>
                           {
                              x.Embed = BuildRaidMuleEmbed(mule, fileName);
                           });
                        }
                        Connections.DeleteFile(fileName);

                        await player.SendMessageAsync($"You have been invited to a raid by {reactingPlayer.Nickname ?? reactingPlayer.Username}.");
                        subMessages.Remove(message.Id);
                        parent.InvitingPlayer = null;
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
      /// <returns>Completed Task.</returns>
      private static async Task RaidRemoteReactionHandle(IMessage message, SocketReaction reaction)
      {
         await ((SocketUserMessage)message).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
         ulong raidMessageId = subMessages[message.Id].MainMessageId;
         bool needEdit = false;
         Raid raid = (Raid)raidMessages[raidMessageId];
         SocketGuildUser reactingPlayer = (SocketGuildUser)reaction.User;

         if (message.MentionedUserIds.Contains(reactingPlayer.Id))
         {
            if (reaction.Emote.Equals(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]))
            {
               await message.DeleteAsync();
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REQUEST_INVITE]))
            {
               raid.RequestInvite(reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_1]))
            {
               raid.AddPlayer(reactingPlayer, 1, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_2]))
            {
               raid.AddPlayer(reactingPlayer, 2, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_3]))
            {
               raid.AddPlayer(reactingPlayer, 3, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_4]))
            {
               raid.AddPlayer(reactingPlayer, 4, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_5]))
            {
               raid.AddPlayer(reactingPlayer, 5, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_6]))
            {
               raid.AddPlayer(reactingPlayer, 6, reactingPlayer);
               needEdit = true;
            }
            else if (reaction.Emote.Equals(remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOVE_REMOTE]))
            {
               raid.AddPlayer(reactingPlayer, 0, reactingPlayer);

               Dictionary<SocketGuildUser, List<SocketGuildUser>> empty = raid.ClearEmptyPlayer(reactingPlayer);
               foreach (KeyValuePair<SocketGuildUser, List<SocketGuildUser>> user in empty)
               {
                  foreach (SocketGuildUser invite in user.Value)
                  {
                     await invite.SendMessageAsync(BuildUnInvitedMessage(user.Key));
                  }
               }
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
               for (int i = 0; i < Global.SELECTION_EMOJIS.Length; i++)
               {
                  if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
                  {
                     await reaction.Channel.SendMessageAsync($"{BuildRaidPingList(raid.GetGroup(i).GetPingList(), raid.Location, i + 1, false)}");
                     subMessages.Remove(message.Id);
                     await message.DeleteAsync();
                  }
               }
            }
         }
      }

      /// Embed builders ******************************************************

      /// <summary>
      /// Builds a raid embed.
      /// </summary>
      /// <param name="raid">Raid to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a raid.</returns>
      protected static Embed BuildRaidEmbed(Raid raid, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle(raid.Boss.Name.Equals(Global.DEFAULT_RAID_BOSS_NAME) ? "**Empty Raid**" : $"**{raid.Boss.Name} Raid {BuildRaidTitle(raid.Tier)}**");
         embed.WithDescription("Press ? for help.");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("**Time**", raid.Time, true);
         embed.AddField("**Location**", raid.Location, true);
         for (int i = 0; i < raid.GetTotalGroups(); i++)
         {
            string groupPrefix = raid.GetTotalGroups() == 1 ? "" : $"Group {i + 1} ";
            RaidGroup group = raid.GetGroup(i);
            int total = group.TotalPlayers();
            int ready = group.GetReadyCount() + group.GetReadyRemoteCount() + group.GetInviteCount();
            int remote = group.GetRemoteCount();

            string attendList = BuildPlayerList(group.GetReadonlyAttending());
            string readyList = BuildPlayerList(group.GetReadonlyHere());
            string invitedAttendList = BuildInvitedList(group.GetReadonlyInvitedAttending());
            string invitedReadyList = BuildInvitedList(group.GetReadonlyInvitedReady());

            embed.AddField($"**{groupPrefix}Ready {ready}/{total}** (Remote {remote}/{Global.LIMIT_RAID_INVITE})", $"{BuildTotalList(readyList, invitedReadyList)}");
            embed.AddField($"**{groupPrefix}Attending**", $"{BuildTotalList(attendList, invitedAttendList)}");
         }
         embed.AddField($"**Need Invite:**", $"{BuildRequestInviteList(raid.GetReadonlyInviteList())}");
         embed.WithFooter($"Note: the max number of members in a raid is {Global.LIMIT_RAID_PLAYER}, and the max number of invites is {Global.LIMIT_RAID_INVITE}.");
         return embed.Build();
      }

      /// <summary>
      /// Builds a raid mule embed.
      /// </summary>
      /// <param name="raid">Raid mule to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a raid mule.</returns>
      protected static Embed BuildRaidMuleEmbed(RaidMule raid, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle(raid.Boss.Name.Equals(Global.DEFAULT_RAID_BOSS_NAME) ? "**Empty Raid**" : $"**{raid.Boss.Name} Raid {BuildRaidTitle(raid.Tier)}**");
         embed.WithDescription("Press ? for help.");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("**Time**", raid.Time, true);
         embed.AddField("**Location**", raid.Location, true);
         embed.AddField($"Mules", $"{BuildPlayerList(raid.Mules.GetReadonlyAttending())}");
         for (int i = 0; i < raid.GetTotalGroups(); i++)
         {
            embed.AddField($"{(raid.GetTotalGroups() == 1 ? "" : $"Group {i + 1} ")}Remote", $"{BuildInvitedList(raid.GetGroup(i).GetReadonlyInvitedAll())}");
         }
         embed.AddField($"Need Invite:", $"{BuildRequestInviteList(raid.GetReadonlyInviteList())}");
         embed.WithFooter($"Note: The max number of invites is {Global.LIMIT_RAID_INVITE}, and the max number of invites per person is {Global.LIMIT_RAID_MULE_INVITE}.");
         return embed.Build();
      }

      /// <summary>
      /// Builds a raid train embed.
      /// </summary>
      /// <param name="raid">Raid train to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a raid train.</returns>
      protected static Embed BuildRaidTrainEmbed(RaidTrain raid, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle(raid.Boss.Name.Equals(Global.DEFAULT_RAID_BOSS_NAME) ? "**Empty Raid**" : $"**{raid.Boss.Name} Raid {BuildRaidTitle(raid.Tier)}**");
         embed.WithDescription("Press ? for help.");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("**Time**", raid.Time, true);
         embed.AddField($"**Current Location {raid.GetCurrentGymCount()}**", raid.GetCurrentGym(), true);
         embed.AddField("**Next Location**", raid.GetNextGym(), true);
         for (int i = 0; i < raid.GetTotalGroups(); i++)
         {
            string groupPrefix = raid.GetTotalGroups() == 1 ? "" : $"Group {i + 1} ";
            RaidGroup group = raid.GetGroup(i);
            int total = group.TotalPlayers();
            int ready = group.GetReadyCount() + group.GetReadyRemoteCount() + group.GetInviteCount();
            int remote = group.GetRemoteCount();

            string attendList = BuildPlayerList(group.GetReadonlyAttending());
            string readyList = BuildPlayerList(group.GetReadonlyHere());
            string invitedAttendList = BuildInvitedList(group.GetReadonlyInvitedAttending());
            string invitedReadyList = BuildInvitedList(group.GetReadonlyInvitedReady());

            embed.AddField($"**{groupPrefix}Ready {ready}/{total}** (Remote {remote}/{Global.LIMIT_RAID_INVITE})", $"{BuildTotalList(readyList, invitedReadyList)}");
            embed.AddField($"**{groupPrefix}Attending**", $"{BuildTotalList(attendList, invitedAttendList)}");
         }
         embed.AddField($"**Need Invite:**", $"{BuildRequestInviteList(raid.GetReadonlyInviteList())}");
         embed.WithFooter($"Note: the max number of members in a raid is {Global.LIMIT_RAID_PLAYER}, and the max number of invites is {Global.LIMIT_RAID_INVITE}.");
         return embed.Build();
      }

      /// <summary>
      /// Builds a raid boss select embed.
      /// </summary>
      /// <param name="potentials">List of potential raid bosses.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for selecting a raid boss.</returns>
      protected static Embed BuildBossSelectEmbed(List<string> potentials, string fileName)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < potentials.Count; i++)
         {
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} {potentials[i]}");
         }

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"Boss Selection");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Please Select Boss", sb.ToString());

         return embed.Build();
      }

      /// <summary>
      /// Builds a raid invite embed.
      /// </summary>
      /// <param name="invite">List of players to invite.</param>
      /// <param name="player">Player that wants to invite someone.</param>
      /// <returns>Embed for inviting a player to a raid.</returns>
      private static Embed BuildPlayerInviteEmbed(ImmutableList<SocketGuildUser> invite, string player, int offset, int listSize)
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
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"{player} - Invite");
         embed.AddField("Please Select Player to Invite.", sb.ToString());

         return embed.Build();
      }

      /// <summary>
      /// Builds a raid remote embed.
      /// </summary>
      /// <param name="player">Player that wants to attend raid via remote.</param>
      /// <returns>Embed for player to attend a raid via remote.</returns>
      private static Embed BuildPlayerRemoteEmbed(string player)
      {
         StringBuilder sb = new StringBuilder();
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REQUEST_INVITE]} Need Invite");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_1]} 1 Remote Raider");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_2]} 2 Remote Raiders");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_3]} 3 Remote Raiders");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_4]} 4 Remote Raiders");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_5]} 5 Remote Raiders");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_6]} 6 Remote Raiders");
         sb.AppendLine($"{remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOVE_REMOTE]} Remove Remote Raiders");
         sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Cancel");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"**{player} - Remote**");
         embed.AddField("Please Select How You Will Remote to the Raid.", sb.ToString());

         return embed.Build();
      }

      /// <summary>
      /// Builds a raid mule ready embed.
      /// </summary>
      /// <param name="groups">Number of raid groups.</param>
      /// <param name="player">Player acting as the raid mule.</param>
      /// <returns>Embed for mule to mark group as ready.</returns>
      private static Embed BuildMuleReadyEmbed(int groups, string player)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < groups; i++)
         {
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} Raid Group {i + 1}");
         }
         sb.AppendLine($"{extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]} Cancel");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_RAID_RESPONSE);
         embed.WithTitle($"{player} - Raid Mule Ready");
         embed.AddField("Please Select Which Group is Ready.", sb.ToString());
         return embed.Build();
      }

      /// String builders *****************************************************

      /// <summary>
      /// Builds the title of the raid.
      /// </summary>
      /// <param name="tier">Raid tier.</param>
      /// <returns>Raid title as a string.</returns>
      protected static string BuildRaidTitle(short tier)
      {
         if (tier == Global.MEGA_RAID_TIER)
         {
            return Global.NONA_EMOJIS["mega_emote"];
         }
         if (tier == Global.EX_RAID_TIER)
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
      /// <param name="players">List of players.</param>
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
      /// Builds a list of players to ping when a raid is edited.
      /// </summary>
      /// <param name="players">List of players.</param>
      /// <param name="editor">Player who edited the raid.</param>
      /// <param name="field">Field edited.</param>
      /// <param name="value">New value of the field.</param>
      /// <returns>List of players to ping as a string.</returns>
      protected static string BuildEditPingList(ImmutableList<SocketGuildUser> players, SocketGuildUser editor, string field, string value)
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
            sb.AppendLine($"{Global.NUM_EMOJIS[RaidGroup.GetFullPartySize(player.Value) - 1]} {player.Key.Nickname ?? player.Key.Username} {GetPlayerTeam(player.Key)} ");
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
            sb.AppendLine($"{raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID]} {player.Key.Nickname ?? player.Key.Username} {GetPlayerTeam(player.Key)} invited by {player.Value.Nickname ?? player.Value.Username}");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Combines two lists together.
      /// </summary>
      /// <param name="initList">Initial player list.</param>
      /// <param name="inviteList">Player list to add.</param>
      /// <returns>Combined player list.</returns>
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
      /// <param name="players">List of players.</param>
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
      /// Builds the help message.
      /// </summary>
      /// <param name="emotes">List of emotes.</param>
      /// <param name="descriptions">List of emote descriptions.</param>
      /// <param name="replies">List of reply options.</param>
      /// <param name="prefix">Command prefix used for the guild.</param>
      /// <returns>Help messsage as a string.</returns>
      private static string BuildHelpMessage(IEmote[] emotes, string[] descriptions, RaidReplyInfo[] replies, string prefix)
      {
         int offset = 0;
         IEmote startEmoji = null;
         IEmote endEmoji = null;
         StringBuilder sb = new StringBuilder();
         sb.AppendLine("**Raid Emoji Help**:");

         for (int i = 0; i < emotes.Length; i++)
         {
            if (Global.NUM_EMOJIS.Contains(emotes[i]))
            {
               if (startEmoji == null)
               {
                  startEmoji = emotes[i];
               }
               offset++;
            }
            else
            {
               if (startEmoji != null && endEmoji == null)
               {
                  endEmoji = emotes[i - 1];
                  sb.AppendLine($"{startEmoji} - {endEmoji} {descriptions[i - offset]}");
                  offset--;
               }
               sb.AppendLine($"{emotes[i]} {descriptions[i - offset]}");
            }
         }

         sb.AppendLine($"\nIf you are inviting players who have not requested an invite, please use the {raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID]} to indicate the amount.");

         sb.AppendLine("\n**Raid Reply Help:**");
         sb.AppendLine("Note: The following must be sent in a reply to the raid embed to be run.\n");
         foreach (var reply in replies)
         {
            sb.AppendLine($"**{prefix}{reply.Command}**: {reply.Description}");
            if (reply.Param.Count > 0)
            {
               sb.AppendLine("Parameters:");
            }
            foreach (var param in reply.Param)
            {
               sb.AppendLine(param);
            }
            sb.AppendLine();
         }
         return sb.ToString();
      }

      /// <summary>
      /// Builds the uninvited message.
      /// For when the player that has invited you has 
      /// left the raid.
      /// </summary>
      /// <param name="player">Player that has left the raid.</param>
      /// <returns>Uninvited message as a string</returns>
      private static string BuildUnInvitedMessage(SocketGuildUser player)
      {
         return $"{player.Nickname ?? player.Username} has left the raid. You have been moved back to \"Need Invite\".";
      }

      /// <summary>
      /// Builds a list of raid bosses.
      /// </summary>
      /// <param name="bosses">List of raid bosses.</param>
      /// <returns>List of raid bosses as a string.</returns>
      protected static string BuildRaidBossListString(List<string> bosses)
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
      /// Gets the team role registered to a player.
      /// </summary>
      /// <param name="player">Player to get team role of.</param>
      /// <returns>Team role name of the player.</returns>
      private static string GetPlayerTeam(SocketGuildUser player)
      {
         if (player.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_VALOR, StringComparison.OrdinalIgnoreCase)) != null)
         {
            return Global.NONA_EMOJIS["valor_emote"];
         }
         else if (player.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_MYSTIC, StringComparison.OrdinalIgnoreCase)) != null)
         {
            return Global.NONA_EMOJIS["mystic_emote"];
         }
         else if (player.Roles.FirstOrDefault(x => x.Name.ToString().Equals(Global.ROLE_INSTINCT, StringComparison.OrdinalIgnoreCase)) != null)
         {
            return Global.NONA_EMOJIS["instinct_emote"];
         }
         return "";
      }

      /// Miscellaneous *******************************************************

      /// <summary>
      /// Set emotes on a raid message.
      /// Will add the help emote at the end.
      /// </summary>
      /// <param name="message">Message to add emotes to.</param>
      /// <param name="emotes">Emotes to add.</param>
      /// <returns>Completed Task.</returns>
      protected static async Task SetEmojis(RestUserMessage message, IEmote[] emotes, bool addArrows = false)
      {
         await message.AddReactionsAsync(emotes);
         if (addArrows)
         {
            await message.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.BACK_ARROW]);
            await message.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.FORWARD_ARROR]);
         }
         await message.AddReactionAsync(extraEmojis[(int)EXTRA_EMOJI_INDEX.HELP]);
      }

      /// <summary>
      /// Sets custom emotes used for raid messages.
      /// </summary>
      public static void SetRaidEmotes()
      {
         raidEmojis[(int)RAID_EMOJI_INDEX.REMOTE_RAID] = Emote.Parse(Global.NONA_EMOJIS["remote_pass_emote"]);
         muleEmojis[(int)MULE_EMOJI_INDEX.REQUEST_INVITE] = Emote.Parse(Global.NONA_EMOJIS["remote_pass_emote"]);
         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REQUEST_INVITE] = Emote.Parse(Global.NONA_EMOJIS["remote_pass_emote"]);

         raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_1] = Global.NUM_EMOJIS[(int)RAID_EMOJI_INDEX.ADD_PLAYER_1];
         raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_2] = Global.NUM_EMOJIS[(int)RAID_EMOJI_INDEX.ADD_PLAYER_2];
         raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_3] = Global.NUM_EMOJIS[(int)RAID_EMOJI_INDEX.ADD_PLAYER_3];
         raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_4] = Global.NUM_EMOJIS[(int)RAID_EMOJI_INDEX.ADD_PLAYER_4];
         raidEmojis[(int)RAID_EMOJI_INDEX.ADD_PLAYER_5] = Global.NUM_EMOJIS[(int)RAID_EMOJI_INDEX.ADD_PLAYER_5];

         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_1] = Global.NUM_EMOJIS[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_1 - 1];
         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_2] = Global.NUM_EMOJIS[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_2 - 1];
         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_3] = Global.NUM_EMOJIS[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_3 - 1];
         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_4] = Global.NUM_EMOJIS[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_4 - 1];
         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_5] = Global.NUM_EMOJIS[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_5 - 1];
         remoteEmojis[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_6] = Global.NUM_EMOJIS[(int)REMOTE_EMOJI_INDEX.REMOTE_PLAYER_6 - 1];
      }

      /// <summary>
      /// Removes old raid messages from the list of raid messages.
      /// Old raid messages are messages older than one day.
      /// </summary>
      protected static void RemoveOldRaids()
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
      /// Edits a Raid Parent object.
      /// Values that can be edited include time, location (loc) or boss (tier).
      /// </summary>
      /// <param name="raidMessage">Message the raid is in.</param>
      /// <param name="parent">Raid parent object.</param>
      /// <param name="attribute">Attribute to edit.</param>
      /// <param name="value">New value of the attribute.</param>
      /// <returns></returns>
      protected static async Task<bool> EditRaid(SocketUserMessage raidMessage, RaidParent parent, string attribute, string value)
      {
         ulong code = raidMessage.Id;
         ISocketMessageChannel channel = raidMessage.Channel;
         bool editComplete = false;
         bool simpleEdit = false;
         if (attribute.Equals("time", StringComparison.OrdinalIgnoreCase))
         {
            parent.Time = value;
            simpleEdit = true;
         }
         else if (attribute.Equals("location", StringComparison.OrdinalIgnoreCase) || attribute.Equals("loc", StringComparison.OrdinalIgnoreCase))
         {
            parent.Location = value;
            simpleEdit = true;
         }
         else if (attribute.Equals("tier", StringComparison.OrdinalIgnoreCase) || attribute.Equals("boss", StringComparison.OrdinalIgnoreCase))
         {
            short calcTier = Global.RAID_TIER_STRING.ContainsKey(value) ? Global.RAID_TIER_STRING[value] : Global.INVALID_RAID_TIER;
            List<string> potentials = Connections.GetBossList(calcTier);

            if (potentials.Count > 1)
            {
               parent.Tier = calcTier;
               parent.SetBoss(null);
               parent.RaidBossSelections = potentials;
               string fileName = $"Egg{calcTier}.png";
               await raidMessage.DeleteAsync();
               raidMessages.Remove(code);

               Connections.CopyFile(fileName);
               RestUserMessage selectMsg = await channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
               for (int i = 0; i < potentials.Count; i++)
               {
                  await selectMsg.AddReactionAsync(Global.SELECTION_EMOJIS[i]);
               }
               raidMessages.Add(selectMsg.Id, parent);
               Connections.DeleteFile(fileName);
               editComplete = true;
            }
            else if (potentials.Count == 1)
            {
               parent.Tier = calcTier;
               parent.SetBoss(potentials.First());
               string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
               IEmote[] prevReactions = raidMessage.Reactions.Keys.ToArray();
               await raidMessage.DeleteAsync();
               raidMessages.Remove(code);

               Connections.CopyFile(fileName);
               if (parent is Raid raid)
               {
                  RestUserMessage raidMsg = await channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
                  await raidMsg.AddReactionsAsync(prevReactions);
                  raidMessages.Add(raidMsg.Id, parent);
               }
               else if (parent is RaidMule mule)
               {
                  RestUserMessage raidMsg = await channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(mule, fileName));
                  await raidMsg.AddReactionsAsync(prevReactions);
                  raidMessages.Add(raidMsg.Id, parent);
               }
               Connections.DeleteFile(fileName);
               editComplete = true;
            }
            else if (Global.USE_EMPTY_RAID)
            {
               parent.Tier = calcTier;
               parent.SetBoss(Global.DEFAULT_RAID_BOSS_NAME);
               string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
               IEmote[] prevReactions = raidMessage.Reactions.Keys.ToArray();
               await raidMessage.DeleteAsync();
               raidMessages.Remove(code);

               Connections.CopyFile(fileName);
               if (parent is Raid raid)
               {
                  RestUserMessage raidMsg = await channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
                  await raidMsg.AddReactionsAsync(prevReactions);
                  raidMessages.Add(raidMsg.Id, parent);
               }
               else if (parent is RaidMule mule)
               {
                  RestUserMessage raidMsg = await channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(mule, fileName));
                  await raidMsg.AddReactionsAsync(prevReactions);
                  raidMessages.Add(raidMsg.Id, parent);
               }
               Connections.DeleteFile(fileName);
               editComplete = true;
            }
            else
            {
               await ResponseMessage.SendErrorMessage(channel, "edit", $"No raid bosses found for tier {value}.");
            }
         }
         else
         {
            await ResponseMessage.SendErrorMessage(channel, "edit", "Please enter a valid field to edit.");
         }

         if (simpleEdit)
         {
            string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
            Connections.CopyFile(fileName);
            if (parent is Raid raid)
            {
               await raidMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildRaidEmbed(raid, fileName);
               });
            }
            if (parent is RaidMule mule)
            {
               await raidMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildRaidMuleEmbed(mule, fileName);
               });
            }
            Connections.DeleteFile(fileName);
         }

         return (simpleEdit || editComplete);
      }
   }
}