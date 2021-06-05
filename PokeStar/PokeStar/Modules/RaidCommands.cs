﻿using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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
      /// <summary>
      /// Handle raid command.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">Time the raid will start.</param>
      /// <param name="location">Where the raid will be.</param>
      /// <returns>Completed Task.</returns>
      [Command("raid")]
      [Summary("Creates a new raid coordination message.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task Raid([Summary("Tier of the raid.")] string tier,
                             [Summary("Time the raid will start.")] string time,
                             [Summary("Where the raid will be.")][Remainder] string location)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.Instance().GetFullBossList();
         List<string> potentials = calcTier == Global.INVALID_RAID_TIER || !allBosses.ContainsKey(calcTier) ? new List<string>() : allBosses[calcTier];
         Raid raid;
         string fileName;
         if (potentials.Count > 1)
         {
            fileName = $"Egg{calcTier}.png";
            raid = new Raid(calcTier, time, location)
            {
               AllBosses = allBosses
            };

            Connections.CopyFile(fileName);
            RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
            raidMessages.Add(selectMsg.Id, raid);
            Connections.DeleteFile(fileName);
            selectMsg.AddReactionsAsync(Global.SELECTION_EMOJIS.Take(potentials.Count).ToArray());
         }
         else if (potentials.Count == 1)
         {
            string boss = potentials.First();
            raid = new Raid(calcTier, time, location, boss)
            {
               AllBosses = allBosses
            };
            fileName = Connections.GetPokemonPicture(raid.GetCurrentBoss());

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, raidEmojis);
         }
         else if (Global.USE_EMPTY_RAID)
         {
            string boss = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new Raid(calcTier, time, location, boss)
            {
               AllBosses = allBosses
            };
            fileName = Connections.GetPokemonPicture(raid.GetCurrentBoss());

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);            
            SetEmojis(raidMsg, raidEmojis);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "raid", $"No raid bosses found for tier {tier}");
         }
         RemoveOldRaids();
      }

      /// <summary>
      /// Handle mule command.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">Time the raid will start.</param>
      /// <param name="location">Where the raid will be.</param>
      /// <returns>Completed Task.</returns>
      [Command("mule")]
      [Alias("raidmule")]
      [Summary("Creates a new remote raid coordination message.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task RaidMule([Summary("Tier of the raid.")] string tier,
                                 [Summary("Time the raid will start.")] string time,
                                 [Summary("Where the raid will be.")][Remainder] string location)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.Instance().GetFullBossList();
         List<string> potentials = calcTier == Global.INVALID_RAID_TIER || !allBosses.ContainsKey(calcTier) ? new List<string>() : allBosses[calcTier];
         RaidMule raid;
         string fileName;
         if (potentials.Count > 1)
         {
            fileName = $"Egg{calcTier}.png";
            raid = new RaidMule(calcTier, time, location)
            {
               AllBosses = allBosses
            };

            Connections.CopyFile(fileName);
            RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
            raidMessages.Add(selectMsg.Id, raid);
            Connections.DeleteFile(fileName);
            selectMsg.AddReactionsAsync(Global.SELECTION_EMOJIS.Take(potentials.Count).ToArray());
         }
         else if (potentials.Count == 1)
         {
            string boss = potentials.First();
            raid = new RaidMule(calcTier, time, location, boss)
            {
               AllBosses = allBosses
            };
            fileName = Connections.GetPokemonPicture(raid.GetCurrentBoss());

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, muleEmojis);
         }
         else if (Global.USE_EMPTY_RAID)
         {
            string boss = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new RaidMule(calcTier, time, location, boss)
            {
               AllBosses = allBosses
            };
            fileName = Connections.GetPokemonPicture(raid.GetCurrentBoss());

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, muleEmojis);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "mule", $"No raid bosses found for tier {tier}");
         }
         RemoveOldRaids();
      }

      /// <summary>
      /// Handle train command.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">Time the raid will start.</param>
      /// <param name="location">Where the raid will be.</param>
      /// <returns>Completed Task.</returns>
      [Command("train")]
      [Alias("raidTrain")]
      [Summary("Creates a new raid train coordination message.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task RaidTrain([Summary("Tier of the raids.")] string tier,
                                  [Summary("Time the train will start.")] string time,
                                  [Summary("Where the train will start.")][Remainder] string location)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.Instance().GetFullBossList();
         List<string> potentials = calcTier == Global.INVALID_RAID_TIER || !allBosses.ContainsKey(calcTier) ? new List<string>() : allBosses[calcTier];
         Raid raid;
         string fileName;
         if (potentials.Count > 1)
         {
            fileName = $"Egg{calcTier}.png";
            raid = new Raid(calcTier, time, location, (SocketGuildUser)Context.User)
            {
               AllBosses = allBosses
            };

            Connections.CopyFile(fileName);
            RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
            raidMessages.Add(selectMsg.Id, raid);
            Connections.DeleteFile(fileName);
            selectMsg.AddReactionsAsync(Global.SELECTION_EMOJIS.Take(potentials.Count).ToArray());
         }
         else if (potentials.Count == 1)
         {
            string boss = potentials.First();
            raid = new Raid(calcTier, time, location, (SocketGuildUser)Context.User, boss)
            {
               AllBosses = allBosses
            };
            fileName = RAID_TRAIN_IMAGE_NAME;

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, raidEmojis.Concat(trainEmojis).ToArray());
         }
         else if (Global.USE_EMPTY_RAID)
         {
            string boss = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new Raid(calcTier, time, location, (SocketGuildUser)Context.User, boss)
            {
               AllBosses = allBosses
            };
            fileName = RAID_TRAIN_IMAGE_NAME;

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, raidEmojis.Concat(trainEmojis).ToArray());
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "train", $"No raid bosses found for tier {tier}");
         }
         RemoveOldRaids();
      }

      /// <summary>
      /// Handle muletrain command.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <param name="time">Time the raid will start.</param>
      /// <param name="location">Where the raid will be.</param>
      /// <returns>Completed Task.</returns>
      [Command("muletrain")]
      [Alias("raidMuleTrain")]
      [Summary("Creates a new raid train coordination message.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task RaidMuleTrain([Summary("Tier of the raids.")] string tier,
                                      [Summary("Time the train will start.")] string time,
                                      [Summary("Where the train will start.")][Remainder] string location)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.Instance().GetFullBossList();
         List<string> potentials = calcTier == Global.INVALID_RAID_TIER || !allBosses.ContainsKey(calcTier) ? new List<string>() : allBosses[calcTier];
         RaidMule raid;
         string fileName;
         if (potentials.Count > 1)
         {
            fileName = $"Egg{calcTier}.png";
            raid = new RaidMule(calcTier, time, location, (SocketGuildUser)Context.User)
            {
               AllBosses = allBosses
            };

            Connections.CopyFile(fileName);
            RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
            raidMessages.Add(selectMsg.Id, raid);
            Connections.DeleteFile(fileName);
            selectMsg.AddReactionsAsync(Global.SELECTION_EMOJIS.Take(potentials.Count).ToArray());
         }
         else if (potentials.Count == 1)
         {
            string boss = potentials.First();
            raid = new RaidMule(calcTier, time, location, (SocketGuildUser)Context.User, boss)
            {
               AllBosses = allBosses
            };
            fileName = RAID_TRAIN_IMAGE_NAME;

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleTrainEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, muleEmojis.Concat(trainEmojis).ToArray());
         }
         else if (Global.USE_EMPTY_RAID)
         {
            string boss = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new RaidMule(calcTier, time, location, (SocketGuildUser)Context.User, boss)
            {
               AllBosses = allBosses
            };
            fileName = RAID_TRAIN_IMAGE_NAME;

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidMuleTrainEmbed(raid, fileName));
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
            SetEmojis(raidMsg, muleEmojis.Concat(trainEmojis).ToArray());
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "muleTrain", $"No raid bosses found for tier {tier}");
         }
         RemoveOldRaids();
      }

      /// <summary>
      /// Handle guide command.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <returns>Completed Task.</returns>
      [Command("guide")]
      [Alias("raidguide")]
      [Summary("Gets raid information for a raid boss.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task Guide([Summary("Tier of the raid boss.")] string tier)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.Instance().GetFullBossList();
         List<string> potentials = calcTier == Global.INVALID_RAID_TIER || !allBosses.ContainsKey(calcTier) ? new List<string>() : allBosses[calcTier];
         string fileName;
         if (potentials.Count > 1)
         {
            fileName = $"Egg{calcTier}.png";
            Connections.CopyFile(fileName);
            RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildBossSelectEmbed(potentials, fileName));
            guideMessages.Add(selectMsg.Id, potentials);
            Connections.DeleteFile(fileName);
            selectMsg.AddReactionsAsync(Global.SELECTION_EMOJIS.Take(potentials.Count).ToArray());
         }
         else if (potentials.Count == 1)
         {
            Pokemon pkmn = Connections.Instance().GetPokemon(potentials.First());
            Connections.Instance().GetRaidBoss(ref pkmn);

            fileName = Connections.GetPokemonPicture(pkmn.Name);
            Connections.CopyFile(fileName);
            await Context.Channel.SendFileAsync(fileName, embed: BuildRaidGuideEmbed(pkmn, fileName));
            Connections.DeleteFile(fileName);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "guide", $"No raid bosses found for tier {tier}");
         }
         RemoveOldRaids();
      }

      /// <summary>
      /// Handle poll command.
      /// </summary>
      /// <param name="tier">Tier of the raid.</param>
      /// <returns>Completed Task.</returns>
      [Command("poll")]
      [Alias("raidpoll")]
      [Summary("Creates a poll to vote for a raid boss.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "2, uncommon, U\n" +
         "3, rare, R\n" +
         "4, premium, p\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task Poll([Summary("Tier of the raid bosses.")] string tier)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.Instance().GetFullBossList();
         List<string> potentials = calcTier == Global.INVALID_RAID_TIER || !allBosses.ContainsKey(calcTier) ? new List<string>() : allBosses[calcTier];
         if (potentials.Count > 1)
         {
            string fileName = $"Egg{calcTier}.png";
            Dictionary<string, int> poll = potentials.ToDictionary(x => x, x => 0);

            Connections.CopyFile(fileName);
            RestUserMessage selectMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidPollEmbed(poll, fileName, false));
            pollMessages.Add(selectMsg.Id, poll);
            Connections.DeleteFile(fileName);
            List<IEmote> emotes = Global.SELECTION_EMOJIS.Take(potentials.Count).Cast<IEmote>().ToList();
            emotes.Add(extraEmojis[(int)EXTRA_EMOJI_INDEX.CANCEL]);
            selectMsg.AddReactionsAsync(emotes.ToArray());
         }
         else if (potentials.Count == 1)
         {
            await ResponseMessage.SendWarningMessage(Context.Channel, "poll", $"The only boss found for tier {tier} is {potentials.First()}");
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "poll", $"No raid bosses found for tier {tier}");
         }
         RemoveOldRaids();
      }

      /// <summary>
      /// Handle boss command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("boss")]
      [Alias("bosses", "bosslist", "raidboss", "raidbosses", "raidbosslist")]
      [Summary("Get the current list of raid bosses.")]
      [RegisterChannel('I')]
      public async Task Boss()
      {
         Dictionary<int, List<string>> allBosses = Connections.Instance().GetFullBossList();

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_GAME_INFO_RESPONSE);
         embed.WithTitle("Current Raid Bosses:");
         if (allBosses.ContainsKey(Global.EX_RAID_TIER))
         {
            embed.AddField($"EX Raids {BuildRaidTitle(Global.EX_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.EX_RAID_TIER]), true);
         }
         if (allBosses.ContainsKey(Global.MEGA_RAID_TIER))
         {
            embed.AddField($"Mega Raids {BuildRaidTitle(Global.MEGA_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.MEGA_RAID_TIER]), true);
         }
         if (allBosses.ContainsKey(Global.LEGENDARY_RAID_TIER))
         {
            embed.AddField($"Tier 5 Raids {BuildRaidTitle(Global.LEGENDARY_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.LEGENDARY_RAID_TIER]), true);
         }
         if (allBosses.ContainsKey(Global.PREMIUM_RAID_TIER))
         {
            embed.AddField($"Tier 4 Raids {BuildRaidTitle(Global.PREMIUM_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.PREMIUM_RAID_TIER]), true);
         }
         if (allBosses.ContainsKey(Global.RARE_RAID_TIER))
         {
            embed.AddField($"Tier 3 Raids {BuildRaidTitle(Global.RARE_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.RARE_RAID_TIER]), true);
         }
         if (allBosses.ContainsKey(Global.UNCOMMON_RAID_TIER))
         {
            embed.AddField($"Tier 2 Raids {BuildRaidTitle(Global.UNCOMMON_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.UNCOMMON_RAID_TIER]), true);
         }
         if (allBosses.ContainsKey(Global.COMMON_RAID_TIER))
         {
            embed.AddField($"Tier 1 Raids {BuildRaidTitle(Global.COMMON_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.COMMON_RAID_TIER]), true);
         }

         await Context.Channel.SendMessageAsync(embed: embed.Build());
      }

      /// <summary>
      /// Handle difficulty command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("difficulty")]
      [Alias("raiddifficulty", "bossdifficulty", "raidbossdifficulty")]
      [Summary("Get the raid difficulty definitions.")]
      [RegisterChannel('I')]
      public async Task Difficulty()
      {
         Dictionary<string, string> table = Connections.GetRaidDifficultyTable();

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_GAME_INFO_RESPONSE);
         embed.WithTitle("Raid Boss Difficulty Scale:");

         for (int i = 0; i < table.Count - 1; i++)
         {
            KeyValuePair<string, string> difficulty = table.ElementAt(i);
            embed.AddField(difficulty.Key, difficulty.Value);
         }

         embed.WithFooter(table.ElementAt(table.Count - 1).Value);

         await Context.Channel.SendMessageAsync(embed: embed.Build());
      }
   }
}