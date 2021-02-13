using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using PokeStar.DataModels;
using PokeStar.PreConditions;
using PokeStar.ModuleParents;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles raid reply commands.
   /// </summary>
   public class RaidReplyCommands : RaidCommandParent
   {
      /// <summary>
      /// Handle edit command.
      /// </summary>
      /// <param name="attribute">Portion of the raid message to edit.</param>
      /// <param name="value">New value of the edited attribute.</param>
      /// <returns></returns>
      [Command("edit")]
      [Summary("Edit the time, location (loc), or tier/boss of a raid.")]
      [Remarks("Must be a reply to any type of raid message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Edit([Summary("Portion of the raid message to edit.")] string attribute,
                             [Summary("New value of the edited attribute.")][Remainder] string value)
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];

         if (!parent.IsSingleStop() && !Context.Message.Author.Equals(parent.Conductor))
         {
            await ResponseMessage.SendErrorMessage(raidMessage.Channel, "edit", $"Command can only be run by the current conductor.");
         }
         else
         {
            ISocketMessageChannel channel = raidMessage.Channel;
            bool needsEdit = false;
            if (attribute.Equals("time", StringComparison.OrdinalIgnoreCase))
            {
               parent.UpdateRaidInformation(value, null);
               needsEdit = true;
            }
            else if (attribute.Equals("location", StringComparison.OrdinalIgnoreCase) || attribute.Equals("loc", StringComparison.OrdinalIgnoreCase))
            {
               parent.UpdateRaidInformation(null, value);
               needsEdit = true;
            }
            else if (attribute.Equals("tier", StringComparison.OrdinalIgnoreCase) || attribute.Equals("boss", StringComparison.OrdinalIgnoreCase))
            {
               short calcTier = Global.RAID_TIER_STRING.ContainsKey(value) ? Global.RAID_TIER_STRING[value] : Global.INVALID_RAID_TIER;

               if (calcTier == Global.INVALID_RAID_TIER)
               {
                  await ResponseMessage.SendErrorMessage(channel, "edit", $"No raid bosses found for tier {value}.");
               }
               else
               {
                  SocketGuildUser author = (SocketGuildUser)Context.Message.Author;
                  if (parent.IsSingleStop())
                  {
                     parent.BossEditingPlayer = author;
                  }
                  parent.SelectionTier = calcTier;
                  RestUserMessage bossMsg = await Context.Channel.SendMessageAsync(text: $"{author.Mention}",
                     embed: BuildBossSelectEmbed(parent.AllBosses[calcTier], null, true));
                  subMessages.Add(bossMsg.Id, new RaidSubMessage((int)SUB_MESSAGE_TYPES.EDIT_BOSS_SUB_MESSAGE, raidMessage.Id));
                  IEmote[] emotes = Global.SELECTION_EMOJIS.Take(parent.AllBosses[calcTier].Count).ToArray();
                  bossMsg.AddReactionsAsync(emotes.Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CHANGE_TIER]).Append(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]).ToArray());
               }
            }
            else
            {
               await ResponseMessage.SendErrorMessage(channel, "edit", "Please enter a valid field to edit.");
            }

            if (needsEdit)
            {
               await ModifyMessage(raidMessage, parent);
               await Context.Channel.SendMessageAsync(BuildEditPingList(parent.GetAllUsers().ToImmutableList(), (SocketGuildUser)Context.Message.Author, attribute, value));
            }
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle invite command.
      /// </summary>
      /// <param name="invites">List of users to invite, separated by spaces.</param>
      /// <returns>Completed Task.</returns>
      [Command("invite")]
      [Summary("Invite user(s) to the raid.")]
      [Remarks("Users may be mentioned using \'@\' or username/nickname may be used.\n" +
               "The user must be in the raid and not already being invited." +
               "Must be a reply to any type of raid message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Invite([Summary("List of users to invite, separated by spaces.")][Remainder] string invites)
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];
         if (parent.IsInRaid((SocketGuildUser)Context.Message.Author, false) != Global.NOT_IN_RAID)
         {
            if (!parent.HasActiveInvite())
            {
               parent.InvitingPlayer = (SocketGuildUser)Context.Message.Author;

               List<string> inviteList = invites.Split(' ').ToList();
               inviteList.RemoveAll(x => x.StartsWith("<@", StringComparison.OrdinalIgnoreCase));

               List<SocketUser> mentioned = Context.Message.MentionedUsers.ToList();
               bool failedInvites = false;

               foreach (string openInvite in inviteList)
               {
                  SocketGuildUser invite = Context.Guild.Users.FirstOrDefault(x => x.Username.Equals(openInvite, StringComparison.OrdinalIgnoreCase) || (
                                                                                   x.Nickname != null && x.Nickname.Equals(openInvite, StringComparison.OrdinalIgnoreCase)));
                  if (invite != null && !mentioned.Contains(invite))
                  {
                     mentioned.Add(invite);
                  }
                  else
                  {
                     failedInvites = true;
                  }
               }

               foreach (SocketUser invite in mentioned)
               {
                  if (parent.InvitePlayer((SocketGuildUser)invite, parent.InvitingPlayer))
                  {
                     await invite.SendMessageAsync($"You have been invited to a raid by {parent.InvitingPlayer.Nickname ?? parent.InvitingPlayer.Username}.");
                  }
                  else
                  {
                     failedInvites = true;
                  }
               }

               if (failedInvites)
               {
                  await ResponseMessage.SendWarningMessage(Context.Channel, "invite", "Some users where not found to be invited");
               }
               await ModifyMessage(raidMessage, parent);
               parent.InvitingPlayer = null;
            }
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle request command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("request")]
      [Summary("Request an invite to a raid.")]
      [Remarks("The user must not already be in the raid." +
               "Must be a reply to a raid or raid train message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Request()
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];

         parent.RequestInvite((SocketGuildUser)Context.Message.Author);
         await ModifyMessage(raidMessage, parent);

         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle remote command.
      /// </summary>
      /// <param name="groupSize">Amount of users raiding remotly 0 - 6.</param>
      /// <returns>Completed Task.</returns>
      [Command("remote")]
      [Summary("Participate in the raid remotly without an invite.")]
      [Remarks("Must be a reply to a raid or raid train message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Remote([Summary("Amount of users raiding remotly 0 - 6.")] int groupSize)
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];
         if (!(parent is Raid raid))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "remote", $"Command must be a reply to a raid or raid train message.");
         }
         else if (groupSize < 0 || groupSize > 6)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "remote", "Value must be a number between 0 and 6");
         }
         else
         {
            SocketGuildUser author = (SocketGuildUser)Context.Message.Author;
            raid.AddPlayer(author, groupSize, author);
            await ModifyMessage(raidMessage, parent);
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle ready command.
      /// </summary>
      /// <param name="groupNum">Number of the raid group that is ready to start.</param>
      /// <returns>Completed Task.</returns>
      [Command("ready")]
      [Summary("Mark a raid group as ready.")]
      [Remarks("Can only be run by a raid mule for the raid." +
               "Must be a reply to a raid mule message")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Ready([Summary("Number of the raid group that is ready to start.")] int groupNum)
      {
         RaidParent parent = raidMessages[Context.Message.Reference.MessageId.Value];
         if (!(parent is RaidMule mule))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "ready", $"Command must be a reply to a raid mule or raid mule train message.");
         }
         else if (mule.GetTotalGroups() > groupNum || groupNum <= 0)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "ready", $"{groupNum} is not a valid raid group number.");
         }
         else
         {
            await Context.Channel.SendMessageAsync($"{BuildRaidReadyPingList(mule.GetGroup(groupNum - 1).GetPingList(), mule.GetCurrentLocation(), groupNum, false)}");
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle add command.
      /// </summary>
      /// <param name="time">Time of the raid.</param>
      /// <param name="location">Location of the raid.</param>
      /// <returns>Completed Task.</returns>
      [Command("add")]
      [Summary("Add a raid to the end of the raid train.")]
      [Remarks("Can only be run by the train\'s conductor." +
               "Must be a reply to a raid train message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Add([Summary("Time of the raid.")] string time,
                            [Summary("Location of the raid.")][Remainder] string location)
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];
         if (parent.IsSingleStop())
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "add", $"Command must be a reply to a raid train or raid mule train message.");
         }
         else if (!Context.Message.Author.Equals(parent.Conductor))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "add", $"Command can only be run by the current conductor.");
         }
         else
         {
            parent.AddRaid(time, location);
            await ModifyMessage(raidMessage, parent);
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle conductor command.
      /// </summary>
      /// <param name="conductor">User to make new conductor.</param>
      /// <returns>Completed Task.</returns>
      [Command("conductor")]
      [Summary("Change the current conductor of the raid train.")]
      [Remarks("Can only be run by the train\'s conductor." +
               "Must be a reply to a raid train message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Conductor([Summary("User to make new conductor.")] IGuildUser conductor)
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];
         if (parent.IsSingleStop())
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "conductor", $"Command must be a reply to a raid train or raid mule train message.");
         }
         else if (!Context.Message.Author.Equals(parent.Conductor))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "conductor", $"Command can only be run by the current conductor.");
         }
         else if (parent.IsInRaid((SocketGuildUser)conductor, false) == Global.NOT_IN_RAID)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "conductor", $"New conductor must be in the train.");
         }
         else
         {
            parent.Conductor = (SocketGuildUser)conductor;
            await ModifyMessage(raidMessage, parent);
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle remove command.
      /// </summary>
      /// <param name="user">User to remove from raid train.</param>
      /// <returns>Completed Task.</returns>
      [Command("remove")]
      [Summary("Remove a user from a raid train.")]
      [Remarks("This should only be used to remove a user that " +
               "is preventing the train from moving forward." +
               "Can only be run by the train\'s conductor." +
               "Must be a reply to a raid train message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Remove([Summary("User to remove from raid train.")] IGuildUser user)
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(raidMessageId);
         RaidParent parent = raidMessages[raidMessageId];
         if (parent.IsSingleStop())
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "remove", $"Command must be a reply to a raid train or raid mule train message.");
         }
         else if (!Context.Message.Author.Equals(parent.Conductor))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "remove", $"Command can only be run by the current conductor.");
         }
         else if (parent.IsInRaid((SocketGuildUser)user, false) == Global.NOT_IN_RAID)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "remove", $"The user is not in the train.");
         }
         else
         {
            RaidRemoveResult returnValue = parent.RemovePlayer((SocketGuildUser)user);

            foreach (SocketGuildUser invite in returnValue.Users)
            {
               await invite.SendMessageAsync(BuildUnInvitedMessage((SocketGuildUser)user));
            }

            await user.SendMessageAsync(BuildRaidTrainRemoveMessage((SocketGuildUser)Context.Message.Author));

            if (returnValue.Group != Global.NOT_IN_RAID)
            {
               await Context.Channel.SendMessageAsync(BuildRaidReadyPingList(parent.GetGroup(returnValue.Group).GetPingList(), parent.GetCurrentLocation(), returnValue.Group + 1, true));
            }

            await ModifyMessage(raidMessage, parent);
         }
         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle station command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("station")]
      [Alias("stations")]
      [Summary("View a list of upcoming stations.")]
      [Remarks("Must be a reply to a raid train message.")]
      [RegisterChannel('R')]
      [RaidReply()]
      public async Task Station()
      {
         ulong raidMessageId = Context.Message.Reference.MessageId.Value;
         RaidParent parent = raidMessages[raidMessageId];
         if (parent.IsSingleStop())
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "station", $"Command must be a reply to a raid train or raid mule train message.");
         }
         else
         {
            List<RaidTrainLoc> futureRaids = parent.GetIncompleteRaids();
            if (parent.StationMessageId.HasValue && Context.Channel.GetCachedMessage(parent.StationMessageId.Value) != null)
            {
               await Context.Channel.DeleteMessageAsync(parent.StationMessageId.Value);
            }
            RestUserMessage stationMsg = await Context.Channel.SendMessageAsync(embed: BuildStationEmbed(futureRaids, parent.Conductor));
            parent.StationMessageId = stationMsg.Id;
         }
         await Context.Message.DeleteAsync();
      }
   }
}