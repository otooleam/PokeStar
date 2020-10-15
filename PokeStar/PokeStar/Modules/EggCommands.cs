using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.PreConditions;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   public class EggCommands : ModuleBase<SocketCommandContext>
   {
      [Command("egg")]
      [Summary("Gets a list of eggs for a given tier." +
               "Leave blank for a list of valid egg tiers.")]
      [RegisterChannel('D')]
      public async Task Egg([Summary("Get information for this Egg tier.")][Remainder]string tier = null)
      {
         if (tier == null)
         {
            StringBuilder sb = new StringBuilder();

            int currentTier = 0;
            foreach (var eggTier in Global.EGG_TIER_STRING)
            {
               if (currentTier != eggTier.Value)
               {
                  if (currentTier != 0)
                  {
                     sb.AppendLine();
                  }
                  currentTier = eggTier.Value;
                  sb.Append(eggTier.Key);
               }
               else
               {
                  sb.Append($" or ({eggTier.Key})");
               }
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
            embed.AddField("Valid Egg Tiers:", sb.ToString());

            await ReplyAsync(embed: embed.Build());
         }
         else
         {
            short calcTier = Global.EGG_TIER_STRING.ContainsKey(tier) ? Global.EGG_TIER_STRING[tier] : Global.INVALID_RAID_TIER;
            if (calcTier == Global.INVALID_RAID_TIER)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "egg", $"{tier} is not a valid egg tier.");
            }
            else
            {
               List<string> eggList = Connections.Instance().GetEggList(calcTier);
               string title = Global.EGG_TIER_TITLE.Where(t => t.Value == calcTier).First().Key;

               bool bold = false;
               int count = 0;
               StringBuilder sb = new StringBuilder();
               foreach (string name in eggList)
               {
                  string boldText = bold ? "**" : "";
                  sb.Append($"{boldText} {name} {boldText}");
                  bold = !bold;
                  count++;
                  if (count == Global.EGG_ROW_LENGTH)
                  {
                     count -= Global.EGG_ROW_LENGTH;
                     sb.Append('\n');
                  }
               }

               EmbedBuilder embed = new EmbedBuilder();
               embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
               embed.AddField(title, sb.ToString());

               await ReplyAsync(embed: embed.Build());
            }
         }
      }

      [Command("updateEggList")]
      [Summary("Updates the saved list of Pokémon in eggs from The Silph Road.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      [NonaAdmin()]
      public async Task UpdateEggList()
      {
         Connections.Instance().UpdateEggList();
         await ResponseMessage.SendInfoMessage(Context.Channel, $"Egg list has been updated.");
      }
   }
}
