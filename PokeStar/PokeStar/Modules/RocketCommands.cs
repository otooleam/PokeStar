using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.PreConditions;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles rocket commands.
   /// </summary>
   public class RocketCommands: ModuleBase<SocketCommandContext>
   {
      /// <summary>
      /// Handle rocket command.
      /// </summary>
      /// <param name="type">(Optional) Get information for this type of Rocket.</param>
      /// <returns>Completed Task.</returns>
      [Command("rocket")]
      [Summary("Gets lists of Pokémon currently used by Team GO Rocket.\n" +
               "Leave blank for a list of valid egg tiers.")]
      [RegisterChannel('I')]
      public async Task Rocket([Summary("(Optional) Get information for this type of Rocket.")][Remainder] string type = null)
      {
         if (type == null)
         {
            StringBuilder sb = new StringBuilder();
            foreach (string rocketType in Connections.Instance().GetRocketTypes())
            {
               sb.AppendLine(rocketType);
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(Global.EMBED_COLOR_GAME_INFO_RESPONSE);
            embed.AddField("Valid Rocket types:", sb.ToString());

            await ReplyAsync(embed: embed.Build());
         }
         else
         {
            Rocket rocket = Connections.Instance().GetRocket(type);
            if (rocket == null)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "rocket", $"{type} is not a valid rocket type.");
            }
            else
            {
               EmbedBuilder embed = new EmbedBuilder();
               embed.WithColor(Global.EMBED_COLOR_GAME_INFO_RESPONSE);

               embed.WithTitle(rocket.Name);
               if (rocket.Phrase != null)
               {
                  embed.WithDescription(rocket.Phrase);
               }

               for (int i = 0; i < rocket.Slots.Length; i++)
               {
                  StringBuilder sb = new StringBuilder();
                  foreach(string pokemon in rocket.Slots[i])
                  {
                     sb.AppendLine(pokemon);
                  }
                  embed.AddField($"Slot #{i + 1}:", sb.ToString(), true);
               }

               await ReplyAsync(embed: embed.Build());
            }
         }
      }

      /// <summary>
      /// Handle updateRocketList command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("updateRocketList")]
      [Summary("Updates the saved list of Rocket Grunts from The Silph Road.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      [NonaAdmin()]
      public async Task UpdateRocketList()
      {
         Connections.Instance().UpdateEggList();
         await ResponseMessage.SendInfoMessage(Context.Channel, $"Egg list has been updated.");
      }
   }
}