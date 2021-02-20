using System.Linq;
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
         "2\n" +
         "3, rare, R\n" +
         "4, Snorlax\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task Raid([Summary("Tier of the raid.")] string tier,
                             [Summary("Time the raid will start.")] string time,
                             [Summary("Where the raid will be.")][Remainder] string location)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.GetFullBossList();
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
         "2\n" +
         "3, rare, R\n" +
         "4, Snorlax\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task RaidMule([Summary("Tier of the raid.")] string tier,
                                 [Summary("Time the raid will start.")] string time,
                                 [Summary("Where the raid will be.")][Remainder] string location)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.GetFullBossList();
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
         "2\n" +
         "3, rare, R\n" +
         "4, Snorlax\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task RaidTrain([Summary("Tier of the raids.")] string tier,
                                  [Summary("Time the train will start.")] string time,
                                  [Summary("Where the train will start.")][Remainder] string location)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.GetFullBossList();
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
         "2\n" +
         "3, rare, R\n" +
         "4, Snorlax\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('R')]
      public async Task RaidMuleTrain([Summary("Tier of the raids.")] string tier,
                                      [Summary("Time the train will start.")] string time,
                                      [Summary("Where the train will start.")][Remainder] string location)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         Dictionary<int, List<string>> allBosses = Connections.GetFullBossList();
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
      /// Handle boss command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("boss")]
      [Alias("bosses", "bosslist", "raidboss", "raidbosses", "raidbosslist")]
      [Summary("Get the current list of raid bosses.")]
      [RegisterChannel('I')]
      public async Task BossList()
      {
         Dictionary<int, List<string>> allBosses = Connections.GetFullBossList();

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_GAME_INFO_RESPONSE);
         embed.WithTitle("Current Raid Bosses:");
         embed.AddField($"EX Raids {BuildRaidTitle(Global.EX_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.EX_RAID_TIER]), true);
         embed.AddField($"Mega Raids {BuildRaidTitle(Global.MEGA_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.MEGA_RAID_TIER]), true);
         embed.AddField($"Tier 5 Raids {BuildRaidTitle(Global.LEGENDARY_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.LEGENDARY_RAID_TIER]), true);
         embed.AddField($"Tier 3 Raids {BuildRaidTitle(Global.RARE_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.RARE_RAID_TIER]), true);
         embed.AddField($"Tier 1 Raids {BuildRaidTitle(Global.COMMON_RAID_TIER)}", BuildRaidBossListString(allBosses[Global.COMMON_RAID_TIER]), true);

         await Context.Channel.SendMessageAsync(embed: embed.Build());
      }
   }
}