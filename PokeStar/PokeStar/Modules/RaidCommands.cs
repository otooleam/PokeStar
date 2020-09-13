using System;
using System.Linq;
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
using PokeStar.ModuleParents;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles raid commands.
   /// </summary>
   public class RaidCommands : RaidCommandParent
   {
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
         short calcTier = GenerateTier(tier);
         List<string> potentials = Connections.GetBossList(calcTier);
         Raid raid;
         string fileName;
         if (potentials.Count > 1)
         {
            fileName = $"Egg{calcTier}.png";
            raid = new Raid(calcTier, time, location)
            {
               RaidBossSelections = potentials
            };

            Connections.CopyFile(fileName);
            RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
            for (int i = 0; i < potentials.Count; i++)
            {
               await selectMsg.AddReactionAsync(Global.SELECTION_EMOJIS[i]);
            }
            raidMessages.Add(selectMsg.Id, raid);
            Connections.DeleteFile(fileName);
         }
         else if (potentials.Count == 1)
         {
            string boss = potentials.First();
            raid = new Raid(calcTier, time, location, boss);
            fileName = Connections.GetPokemonPicture(raid.Boss.Name);

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
            await raidMsg.AddReactionsAsync(raidEmojis);
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
         }
         else if (Global.USE_EMPTY_RAID)
         {
            string boss = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new Raid(calcTier, time, location, boss);
            fileName = Connections.GetPokemonPicture(raid.Boss.Name);

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
            await raidMsg.AddReactionsAsync(raidEmojis);
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context, "raid", $"No raid bosses found for tier {tier}");
         }
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
         short calcTier = GenerateTier(tier);
         List<string> potentials = Connections.GetBossList(calcTier);
         RaidMule raid;
         string fileName;
         if (potentials.Count > 1)
         {
            fileName = $"Egg{calcTier}.png";
            raid = new RaidMule(calcTier, time, location)
            {
               RaidBossSelections = potentials
            };

            Connections.CopyFile(fileName);
            RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
            for (int i = 0; i < potentials.Count; i++)
            {
               await selectMsg.AddReactionAsync(Global.SELECTION_EMOJIS[i]);
            }
            raidMessages.Add(selectMsg.Id, raid);
            Connections.DeleteFile(fileName);
         }
         else if (potentials.Count == 1)
         {
            string boss = potentials.First();
            raid = new RaidMule(calcTier, time, location, boss);
            fileName = Connections.GetPokemonPicture(raid.Boss.Name);

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(raid, fileName));
            await raidMsg.AddReactionsAsync(raidEmojis);
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
         }
         else if (Global.USE_EMPTY_RAID)
         {
            string boss = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new RaidMule(calcTier, time, location, boss);
            fileName = Connections.GetPokemonPicture(raid.Boss.Name);

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(raid, fileName));
            await raidMsg.AddReactionsAsync(raidEmojis);
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context, "mule", $"No raid bosses found for tier {tier}");
         }
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
            SocketUserMessage raidMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(code);
            RaidParent parent = raidMessages[code];
            bool editComplete = false;
            bool simpleEdit = false;
            if (attribute.Equals("time", StringComparison.OrdinalIgnoreCase))
            {
               parent.Time = edit;
               simpleEdit = true;
            }
            else if (attribute.Equals("location", StringComparison.OrdinalIgnoreCase) || attribute.Equals("loc", StringComparison.OrdinalIgnoreCase))
            {
               parent.Location = edit;
               simpleEdit = true;
            }
            else if (attribute.Equals("tier", StringComparison.OrdinalIgnoreCase) || attribute.Equals("boss", StringComparison.OrdinalIgnoreCase))
            {
               short calcTier = GenerateTier(edit);
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
                  RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
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
                     RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
                     await raidMsg.AddReactionsAsync(prevReactions);
                     raidMessages.Add(raidMsg.Id, parent);
                  }
                  else if (parent is RaidMule mule)
                  {
                     RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(mule, fileName));
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
                     RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
                     await raidMsg.AddReactionsAsync(prevReactions);
                     raidMessages.Add(raidMsg.Id, parent);
                  }
                  else if (parent is RaidMule mule)
                  {
                     RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(mule, fileName));
                     await raidMsg.AddReactionsAsync(prevReactions);
                     raidMessages.Add(raidMsg.Id, parent);
                  }
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

            if (simpleEdit || editComplete)
            {
               List<SocketGuildUser> allUsers = new List<SocketGuildUser>();
               foreach (RaidGroup group in parent.Groups)
               {
                  allUsers.AddRange(group.GetNotifyList());
               }
               allUsers.AddRange(parent.GetReadonlyInviteList());
               if (parent is RaidMule mule)
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
      /// <param name="tier"></param>
      /// <returns></returns>
      protected short GenerateTier(string tier)
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
   }
}