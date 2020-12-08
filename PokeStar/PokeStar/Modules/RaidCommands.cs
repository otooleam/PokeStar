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
      [Command("raid")]
      [Summary("Creates a new raid coordination message.")]
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
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
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
            await SetEmojis(raidMsg, raidEmojis);
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
            await SetEmojis(raidMsg, raidEmojis);
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "raid", $"No raid bosses found for tier {tier}");
         }
         RemoveOldRaids();
      }

      [Command("mule")]
      [Alias("raidmule")]
      [Summary("Creates a new remote raid coordination message.")]
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
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
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
            await SetEmojis(raidMsg, muleEmojis);
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
            await SetEmojis(raidMsg, muleEmojis);
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "mule", $"No raid bosses found for tier {tier}");
         }
         RemoveOldRaids();
      }

      [Command("train")]
      [Alias("raidtrain")]
      [Summary("Creates a new raid train coordination message.")]
      [Remarks("Valid Tier values:\n" +
         "0 (raid with no boss assigned)\n" +
         "1, common, C\n" +
         "3, rare, R\n" +
         "5, legendary, L\n" +
         "7, mega, M\n")]
      [RegisterChannel('T')]
      public async Task RaidTrain([Summary("Tier of the raids.")] string tier,
                           [Summary("Time the train will start.")] string time,
                           [Summary("Where the train will start.")][Remainder] string location)
      {
         short calcTier = Global.RAID_TIER_STRING.ContainsKey(tier) ? Global.RAID_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
         List<string> potentials = Connections.GetBossList(calcTier);
         RaidTrain raid;
         string fileName;
         if (potentials.Count > 1)
         {
            fileName = $"Egg{calcTier}.png";
            raid = new RaidTrain((SocketGuildUser)Context.User, calcTier, time, location)
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
            raid = new RaidTrain((SocketGuildUser)Context.User, calcTier, time, location, boss);
            fileName = Connections.GetPokemonPicture(raid.Boss.Name);

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(raid, fileName));
            await SetEmojis(raidMsg, raidEmojis, true);
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
         }
         else if (Global.USE_EMPTY_RAID)
         {
            string boss = Global.DEFAULT_RAID_BOSS_NAME;
            raid = new RaidTrain((SocketGuildUser)Context.User, calcTier, time, location, boss);
            fileName = Connections.GetPokemonPicture(raid.Boss.Name);

            Connections.CopyFile(fileName);
            RestUserMessage raidMsg = await Context.Channel.SendFileAsync(fileName, embed: BuildRaidTrainEmbed(raid, fileName));
            await SetEmojis(raidMsg, raidEmojis, true);
            raidMessages.Add(raidMsg.Id, raid);
            Connections.DeleteFile(fileName);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "train", $"No raid bosses found for tier {tier}");
         }
         RemoveOldRaids();
      }

      [Command("bosslist")]
      [Alias("boss", "bossess", "raidboss", "raidbosses", "raidbosslist")]
      [Summary("Get the current list of raid bosses.")]
      [RegisterChannel('I')]
      public async Task BossList()
      {
         List<string> exBosses = Connections.GetBossList(Global.EX_RAID_TIER);
         List<string> megaBosses = Connections.GetBossList(Global.MEGA_RAID_TIER);
         List<string> legendaryBosses = Connections.GetBossList(Global.LEGENDARY_RAID_TIER);
         List<string> rareBosses = Connections.GetBossList(Global.RARE_RAID_TIER);
         List<string> commonBosses = Connections.GetBossList(Global.COMMON_RAID_TIER);

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_GAME_INFO_RESPONSE);
         embed.WithTitle("Current Raid Bosses:");
         embed.AddField($"EX Raids {BuildRaidTitle(Global.EX_RAID_TIER)}", BuildRaidBossListString(exBosses), true);
         embed.AddField($"Mega Raids {BuildRaidTitle(Global.MEGA_RAID_TIER)}", BuildRaidBossListString(megaBosses), true);
         embed.AddField($"Tier 5 Raids {BuildRaidTitle(Global.LEGENDARY_RAID_TIER)}", BuildRaidBossListString(legendaryBosses), true);
         embed.AddField($"Tier 3 Raids {BuildRaidTitle(Global.RARE_RAID_TIER)}", BuildRaidBossListString(rareBosses), true);
         embed.AddField($"Tier 1 Raids {BuildRaidTitle(Global.COMMON_RAID_TIER)}", BuildRaidBossListString(commonBosses), true);

         await Context.Channel.SendMessageAsync(embed: embed.Build());
      }
   }
}