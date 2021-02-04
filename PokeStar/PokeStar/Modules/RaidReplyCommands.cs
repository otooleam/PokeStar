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
using PokeStar.ConnectionInterface;
using PokeStar.PreConditions;
using PokeStar.ModuleParents;

namespace PokeStar.Modules
{
   public class RaidReplyCommands : RaidCommandParent
   {
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

         if (parent is RaidTrain initTrain && !Context.Message.Author.Equals(initTrain.Conductor))
         {
            await ResponseMessage.SendErrorMessage(raidMessage.Channel, "edit", $"Command can only be run by the current conductor.");
         }
         else
         {
            ulong code = raidMessage.Id;
            ISocketMessageChannel channel = raidMessage.Channel;
            bool editComplete = false;
            bool simpleEdit = false;
            if (attribute.Equals("time", StringComparison.OrdinalIgnoreCase))
            {
               if (parent is RaidTrain train)
               {
                  train.UpdateRaidInformation(value, null);
               }
               else
               {
                  parent.Time = value;
               }
               simpleEdit = true;
            }
            else if (attribute.Equals("location", StringComparison.OrdinalIgnoreCase) || attribute.Equals("loc", StringComparison.OrdinalIgnoreCase))
            {
               if (parent is RaidTrain train)
               {
                  train.UpdateRaidInformation(null, value);
               }
               else
               {
                  parent.Location = value;
               }
               simpleEdit = true;
            }
            else if (attribute.Equals("tier", StringComparison.OrdinalIgnoreCase) || attribute.Equals("boss", StringComparison.OrdinalIgnoreCase))
            {
               short calcTier = Global.RAID_TIER_STRING.ContainsKey(value) ? Global.RAID_TIER_STRING[value] : Global.INVALID_RAID_TIER;
               Dictionary<int, List<string>> allBosses = Connections.GetFullBossList();
               List<string> potentials = calcTier == Global.INVALID_RAID_TIER ? new List<string>() : allBosses[calcTier];

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
                  raidMessages.Add(selectMsg.Id, parent);
                  Connections.DeleteFile(fileName);
                  selectMsg.AddReactionsAsync(Global.SELECTION_EMOJIS.Take(potentials.Count).ToArray());
                  editComplete = true;
               }
               else if (potentials.Count == 1)
               {
                  parent.Tier = calcTier;
                  parent.SetBoss(potentials.First());
                  IEmote[] prevReactions = raidMessage.Reactions.Keys.ToArray();
                  await raidMessage.DeleteAsync();
                  raidMessages.Remove(code);

                  if (parent is RaidTrain train)
                  {
                     string fileName = Global.RAID_TRAIN_IMAGE_NAME;
                     Connections.CopyFile(fileName);
                     RestUserMessage raidMsg = await channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(train, fileName));
                     raidMessages.Add(raidMsg.Id, parent);
                     Connections.DeleteFile(fileName);
                     raidMsg.AddReactionsAsync(prevReactions);
                  }
                  else if (parent is Raid raid)
                  {
                     string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
                     Connections.CopyFile(fileName);
                     RestUserMessage raidMsg = await channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
                     raidMessages.Add(raidMsg.Id, parent);
                     Connections.DeleteFile(fileName);
                     raidMsg.AddReactionsAsync(prevReactions);
                  }
                  else if (parent is RaidMule mule)
                  {
                     string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
                     Connections.CopyFile(fileName);
                     RestUserMessage raidMsg = await channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(mule, fileName));
                     raidMessages.Add(raidMsg.Id, parent);
                     Connections.DeleteFile(fileName);
                     raidMsg.AddReactionsAsync(prevReactions);
                  }
                  editComplete = true;
               }
               else if (Global.USE_EMPTY_RAID)
               {
                  parent.Tier = calcTier;
                  parent.SetBoss(Global.DEFAULT_RAID_BOSS_NAME);
                  IEmote[] prevReactions = raidMessage.Reactions.Keys.ToArray();
                  await raidMessage.DeleteAsync();
                  raidMessages.Remove(code);

                  if (parent is RaidTrain train)
                  {
                     string fileName = Global.RAID_TRAIN_IMAGE_NAME;
                     Connections.CopyFile(fileName);
                     RestUserMessage raidMsg = await channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(train, fileName));
                     raidMessages.Add(raidMsg.Id, parent);
                     Connections.DeleteFile(fileName);
                     raidMsg.AddReactionsAsync(prevReactions);
                  }
                  else if (parent is Raid raid)
                  {
                     string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
                     Connections.CopyFile(fileName);
                     RestUserMessage raidMsg = await channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
                     raidMessages.Add(raidMsg.Id, parent);
                     Connections.DeleteFile(fileName);
                     raidMsg.AddReactionsAsync(prevReactions);
                  }
                  else if (parent is RaidMule mule)
                  {
                     string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
                     Connections.CopyFile(fileName);
                     RestUserMessage raidMsg = await channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(mule, fileName));
                     raidMessages.Add(raidMsg.Id, parent);
                     Connections.DeleteFile(fileName);
                     raidMsg.AddReactionsAsync(prevReactions);
                  }
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
               if (parent is RaidTrain train)
               {
                  string fileName = Global.RAID_TRAIN_IMAGE_NAME;
                  Connections.CopyFile(fileName);
                  await raidMessage.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidTrainEmbed(train, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
               else if (parent is Raid raid)
               {
                  string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
                  Connections.CopyFile(fileName);
                  await raidMessage.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidEmbed(raid, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
               else if (parent is RaidMule mule)
               {
                  string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
                  Connections.CopyFile(fileName);
                  await raidMessage.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidMuleEmbed(mule, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
            }

            if (simpleEdit || editComplete)
            {
               await Context.Channel.SendMessageAsync(BuildEditPingList(parent.GetAllUsers().ToImmutableList(), (SocketGuildUser)Context.Message.Author, attribute, value));
            }
         }
         await Context.Message.DeleteAsync();
      }

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

               if (parent is RaidTrain train)
               {
                  string fileName = Global.RAID_TRAIN_IMAGE_NAME;
                  Connections.CopyFile(fileName);
                  await raidMessage.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidTrainEmbed(train, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
               else if (parent is Raid raid)
               {
                  string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
                  Connections.CopyFile(fileName);
                  await raidMessage.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidEmbed(raid, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
               else if (parent is RaidMule mule)
               {
                  string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
                  Connections.CopyFile(fileName);
                  await raidMessage.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidMuleEmbed(mule, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
               parent.InvitingPlayer = null;
            }
         }
         await Context.Message.DeleteAsync();
      }

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
         if (!(parent is Raid raid))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "request", $"Command must be a reply to a raid or raid train message.");
         }
         else
         {
            raid.RequestInvite((SocketGuildUser)Context.Message.Author);

            if (parent is RaidTrain train)
            {
               string fileName = Global.RAID_TRAIN_IMAGE_NAME;
               Connections.CopyFile(fileName);
               await raidMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildRaidTrainEmbed(train, fileName);
               });
               Connections.DeleteFile(fileName);
            }
            else
            {
               string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
               Connections.CopyFile(fileName);
               await raidMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildRaidEmbed(raid, fileName);
               });
               Connections.DeleteFile(fileName);
            }
         }
         await Context.Message.DeleteAsync();
      }

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
         else
         {
            if (groupSize < 0 || groupSize > 6)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "remote", "Value must be a number between 0 and 6");
            }
            else
            {
               SocketGuildUser author = (SocketGuildUser)Context.Message.Author;
               raid.AddPlayer(author, groupSize, author);

               if (parent is RaidTrain train)
               {
                  string fileName = Global.RAID_TRAIN_IMAGE_NAME;
                  Connections.CopyFile(fileName);
                  await raidMessage.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidTrainEmbed(train, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
               else
               {
                  string fileName = Connections.GetPokemonPicture(parent.Boss.Name);
                  Connections.CopyFile(fileName);
                  await raidMessage.ModifyAsync(x =>
                  {
                     x.Embed = BuildRaidEmbed(raid, fileName);
                  });
                  Connections.DeleteFile(fileName);
               }
            }
         }
         await Context.Message.DeleteAsync();
      }

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
            await ResponseMessage.SendErrorMessage(Context.Channel, "ready", $"Command must be a reply to a raid mule message.");
         }
         else
         {
            if (mule.GetTotalGroups() > groupNum || groupNum <= 0)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "ready", $"{groupNum} is not a valid raid group number.");
            }
            else
            {
               await Context.Channel.SendMessageAsync($"{BuildRaidReadyPingList(mule.GetGroup(groupNum - 1).GetPingList(), mule.Location, groupNum, false)}");
            }
         }
         await Context.Message.DeleteAsync();
      }

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
         if (!(parent is RaidTrain train))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "add", $"Command must be a reply to a raid train message.");
         }
         else
         {
            if (!Context.Message.Author.Equals(train.Conductor))
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "add", $"Command can only be run by the current conductor.");
            }
            else
            {
               train.AddRaid(time, location);
               string fileName = Global.RAID_TRAIN_IMAGE_NAME;
               Connections.CopyFile(fileName);
               await raidMessage.ModifyAsync(x =>
               {
                  x.Embed = BuildRaidTrainEmbed(train, fileName);
               });
               Connections.DeleteFile(fileName);
            }
         }
         await Context.Message.DeleteAsync();
      }

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
         if (!(parent is RaidTrain train))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "conductor", $"Command must be a reply to a raid train message.");
         }
         else
         {
            if (!Context.Message.Author.Equals(train.Conductor))
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "conductor", $"Command can only be run by the current conductor.");
            }
            else
            {
               if (train.IsInRaid((SocketGuildUser)conductor, false) == Global.NOT_IN_RAID)
               {
                  await ResponseMessage.SendErrorMessage(Context.Channel, "conductor", $"New conductor must be in the raid.");
               }
               else
               {
                  train.Conductor = (SocketGuildUser)conductor;
               }
            }

            string fileName = Global.RAID_TRAIN_IMAGE_NAME;
            Connections.CopyFile(fileName);
            await raidMessage.ModifyAsync(x =>
            {
               x.Embed = BuildRaidTrainEmbed(train, fileName);
            });
            Connections.DeleteFile(fileName);
         }
         await Context.Message.DeleteAsync();
      }
   }
}