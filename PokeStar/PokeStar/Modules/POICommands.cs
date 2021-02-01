using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.Rest;
using PokeStar.DataModels;
using PokeStar.PreConditions;
using PokeStar.ConnectionInterface;
using Discord.WebSocket;

namespace PokeStar.Modules
{
   public class POICommands : ModuleBase<SocketCommandContext>
   {
      private const string GYM_IMAGE = "gym.png";
      private const string STOP_IMAGE = "pokestop.png";
      private const string UNKNOWN_POI_IMAGE = "unknown_stop.png";

      private static readonly string GOOGLE_MAP = $"http://www.google.com/maps/place/";
      private static readonly string APPLE_MAP = $"http://maps.apple.com/?daddr=";

      protected static readonly Dictionary<ulong, List<string>> poiMessages = new Dictionary<ulong, List<string>>();

      [Command("poi")]
      [Alias("gym", "stop", "pokestop")]
      [Summary("")]
      [RegisterChannel('S')]
      public async Task POI([Summary("Get information for this Point of Interest.")][Remainder] string poi)
      {
         ulong guild = Context.Guild.Id;
         POI pkmnPOI = Connections.Instance().GetPOI(guild, poi);
         
         if (pkmnPOI == null)
         {
            pkmnPOI = Connections.Instance().GetPOI(guild, Connections.Instance().GetPOIWithNickname(guild, poi));

            if (pkmnPOI == null)
            {
               List<string> gymNames = Connections.Instance().SearchPOI(guild, poi);
               Connections.CopyFile(UNKNOWN_POI_IMAGE);
               RestUserMessage poiMessage = await Context.Channel.SendFileAsync(UNKNOWN_POI_IMAGE, embed: BuildSelectEmbed(gymNames, UNKNOWN_POI_IMAGE));
               Connections.DeleteFile(UNKNOWN_POI_IMAGE);
               poiMessages.Add(poiMessage.Id, gymNames);
               poiMessage.AddReactionsAsync(Global.SELECTION_EMOJIS.Take(gymNames.Count).ToArray());
            }
            else
            {
               string fileName = pkmnPOI.IsGym ? GYM_IMAGE : STOP_IMAGE;
               pkmnPOI.Nicknames = Connections.Instance().GetPOINicknames(guild, pkmnPOI.Name);
               Connections.CopyFile(fileName);
               await Context.Channel.SendFileAsync(fileName, embed: BuildPOIEmbed(pkmnPOI, fileName));
               Connections.CopyFile(fileName);
            }
         }
         else
         {
            string fileName = pkmnPOI.IsGym ? GYM_IMAGE : STOP_IMAGE;
            pkmnPOI.Nicknames = Connections.Instance().GetPOINicknames(guild, pkmnPOI.Name);
            Connections.CopyFile(fileName);
            await Context.Channel.SendFileAsync(fileName, embed: BuildPOIEmbed(pkmnPOI, fileName));
            Connections.CopyFile(fileName);
         }
      }

      /// <summary>
      /// Checks if a message is a poi select message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a poi select message, otherwise false.</returns>
      public static bool IsPOISubMessage(ulong id)
      {
         return poiMessages.ContainsKey(id);
      }

      /// <summary>
      /// Handles a reaction on a poi select message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      public static async Task POIMessageReactionHandle(IMessage message, SocketReaction reaction, ulong guildId)
      {
         List<string> poiMessage = poiMessages[message.Id];
         for (int i = 0; i < poiMessage.Count; i++)
         {
            if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
            {
               await message.DeleteAsync();
               POI poi = Connections.Instance().GetPOI(guildId, poiMessage[i]);

               string fileName = poi.IsGym ? GYM_IMAGE : STOP_IMAGE;
               poi.Nicknames = Connections.Instance().GetPOINicknames(guildId, poi.Name);
               Connections.CopyFile(fileName);
               await reaction.Channel.SendFileAsync(fileName, embed: BuildPOIEmbed(poi, fileName));
               Connections.CopyFile(fileName);
               poiMessages.Remove(message.Id);
               return;
            }
         }
         await message.RemoveReactionAsync(reaction.Emote, (SocketGuildUser)reaction.User);
      }

      /// <summary>
      /// Builds a POI embed.
      /// </summary>
      /// <param name="poi">POI to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a POI.</returns>
      private static Embed BuildPOIEmbed(POI poi, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         string title = poi.IsGym ? poi.IsExGym ? "Ex Gym" : "Gym" : "Poké Stop";
         string sponsored = poi.IsSponsored ? "Sponsored" : "";
         embed.WithTitle($"{sponsored} {title}: {poi.Name}");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.WithColor(Global.EMBED_COLOR_GYM_RESPONSE);
         embed.AddField($"**Google Maps**", $"{GOOGLE_MAP}{poi.Latitude},{poi.Longitude}", true);
         embed.AddField($"**Apple Maps**", $"{APPLE_MAP}{poi.Latitude},{poi.Longitude}", true);
         if (poi.Nicknames.Count != 0)
         {
            StringBuilder sb = new StringBuilder();
            foreach (string nickname in poi.Nicknames)
            {
               sb.AppendLine(nickname);
            }
            embed.AddField($"**Registered Nicknames**", sb.ToString(), false);
         }
         return embed.Build();
      }

      /// <summary>
      /// Builds the POI select embed.
      /// </summary>
      /// <param name="potentials">List of potential POIs.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for selecting a POI.</returns>
      private static Embed BuildSelectEmbed(List<string> potentials, string fileName)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < potentials.Count; i++)
         {
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} {potentials[i]}");
         }

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_GYM_RESPONSE);
         embed.WithTitle("Do you mean...?");
         embed.WithDescription(sb.ToString());
         embed.WithThumbnailUrl($"attachment://{fileName}");
         return embed.Build();
      }
   }
}
