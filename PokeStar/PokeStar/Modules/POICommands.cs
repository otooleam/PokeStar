using System;
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
      private const int NumArguments = 2;
      private const int IndexNickname = 0;
      private const int IndexName = 1;

      private const string GYM_IMAGE = "gym.png";
      private const string STOP_IMAGE = "pokestop.png";
      private const string UNKNOWN_POI_IMAGE = "unknown_stop.png";

      private static readonly string GOOGLE_MAP = $"http://www.google.com/maps/place/";
      private static readonly string APPLE_MAP = $"http://maps.apple.com/?daddr=";

      private readonly List<string> EditableAttributes = new List<string>()
      {
         "GYM",
         "SPONSORED",
         "EX"
      };

      protected static readonly Dictionary<ulong, List<string>> poiMessages = new Dictionary<ulong, List<string>>();

      [Command("poi")]
      [Alias("gym", "stop", "pokestop")]
      [Summary("Get information on a point of interest.")]
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

      [Command("addPOI")]
      [Summary("Add a new Point of Interest.")]
      [RegisterChannel('S')]
      public async Task AddPOI([Summary("Latitude of the Point of Interest.")] float latitude,
                               [Summary("Longitude of the Point of Interest.")] float longitude,
                               [Summary("Is the Point of Interest a gym.")] int isGym,
                               [Summary("Name of the Point of Interest.")][Remainder] string name)
      {
         ulong guild = Context.Guild.Id;
         if (Connections.Instance().GetPOI(guild, name) == null && Connections.Instance().GetPOI(guild, Connections.Instance().GetPOIWithNickname(guild, name)) == null)
         {
            Connections.Instance().AddPOI(guild, name, latitude, longitude, isGym);
            await ResponseMessage.SendInfoMessage(Context.Channel, $"Added new Point of Interest {name}.");
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "addPOI", $"A point of interest with the name or nickname {name} already exists.");
         }
      }

      [Command("addSponsoredPOI")]
      [Summary("Add a new sponsored Point of Interest.")]
      [RegisterChannel('S')]
      public async Task AddSponsoredPOI([Summary("Latitude of the Point of Interest.")] float latitude,
                                        [Summary("Longitude of the Point of Interest.")] float longitude,
                                        [Summary("Is the Point of Interest a gym.")] int isGym,
                                        [Summary("Name of the Point of Interest.")][Remainder] string name)
      {
         ulong guild = Context.Guild.Id;
         if (Connections.Instance().GetPOI(guild, name) == null && Connections.Instance().GetPOI(guild, Connections.Instance().GetPOIWithNickname(guild, name)) == null)
         {
            Connections.Instance().AddSponsoredPOI(guild, name, latitude, longitude, isGym);
            await ResponseMessage.SendInfoMessage(Context.Channel, $"Added new Sponsored Point of Interest {name}.");
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "addSponsoredPOI", $"A point of interest with the name or nickname {name} already exists.");
         }
      }

      [Command("addExGym")]
      [Summary("Add a new EX Gym.")]
      [RegisterChannel('S')]
      public async Task AddSponsoredPOI([Summary("Latitude of the Point of Interest.")] float latitude,
                                        [Summary("Longitude of the Point of Interest.")] float longitude,
                                        [Summary("Name of the Point of Interest.")][Remainder] string name)
      {
         ulong guild = Context.Guild.Id;
         if (Connections.Instance().GetPOI(guild, name) == null && Connections.Instance().GetPOI(guild, Connections.Instance().GetPOIWithNickname(guild, name)) == null)
         {
            Connections.Instance().AddExGym(guild, name, latitude, longitude);
            await ResponseMessage.SendInfoMessage(Context.Channel, $"Added new EX-Raid Gym {name}.");
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "addExGym", $"A point of interest with the name or nickname {name} already exists.");
         }
      }

      [Command("updatePOI")]
      [Summary("Edit an attribute of a Point of Interest.")]
      [Remarks("Valid attributes to edit are gym, sponsored, and ex." +
               "Value can only be set to either 1(true) or 0(false)")]
      [RegisterChannel('S')]
      public async Task UpdatePOI([Summary("Update this attribute.")] string attribute,
                                  [Summary("Update the attribute with this value.")] int value,
                                  [Summary("Update attribute of this Point of Interest.")][Remainder] string poi)
      {
         if (EditableAttributes.Contains(attribute.ToUpper()))
         {
            ulong guild = Context.Guild.Id;
            POI pkmnPOI = Connections.Instance().GetPOI(guild, poi);

            if (pkmnPOI == null)
            {
               pkmnPOI = Connections.Instance().GetPOI(guild, Connections.Instance().GetPOIWithNickname(guild, poi));

               if (pkmnPOI == null)
               {
                  await ResponseMessage.SendErrorMessage(Context.Channel, "updatePOI", $"Point of Interest, {poi}, does not exist.");
               }
               else
               {
                  if (attribute.Equals("EX", StringComparison.OrdinalIgnoreCase) && !pkmnPOI.IsGym)
                  {
                     await ResponseMessage.SendErrorMessage(Context.Channel, "updatePOI", "Only gyms can be set as EX-Raid Gyms.");
                  }
                  else
                  {
                     Connections.Instance().UpdatePOI(guild, pkmnPOI.Name, attribute, value);
                     await ResponseMessage.SendInfoMessage(Context.Channel, $"{attribute} has been set to {value} for {pkmnPOI.Name}. Run .poi {pkmnPOI.Name} to ensure value is set correctly.");
                  }
               }
            }
            else
            {
               if (attribute.Equals("EX", StringComparison.OrdinalIgnoreCase) && !pkmnPOI.IsGym)
               {
                  await ResponseMessage.SendErrorMessage(Context.Channel, "updatePOI", "Only gyms can be set as EX-Raid Gyms.");
               }
               else
               {
                  Connections.Instance().UpdatePOI(guild, pkmnPOI.Name, attribute, value);
                  await ResponseMessage.SendInfoMessage(Context.Channel, $"{attribute} has been set to {value} for {pkmnPOI.Name}. Run .poi {pkmnPOI.Name} to ensure value is set correctly.");
               }
            }
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "updatePOI", $"{attribute} is not a valid attribute to change.");
         }
      }

      [Command("removePOI")]
      [Summary("Remove a Point of Interest.")]
      [RegisterChannel('S')]
      public async Task RemovePOI([Summary("Point of Interest to remove.")][Remainder] string poi)
      {
         ulong guild = Context.Guild.Id;
         POI pkmnPOI = Connections.Instance().GetPOI(guild, poi);

         if (pkmnPOI == null)
         {
            pkmnPOI = Connections.Instance().GetPOI(guild, Connections.Instance().GetPOIWithNickname(guild, poi));

            if (pkmnPOI == null)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "removePOI", $"Point of Interest, {poi}, does not exist.");
            }
            else
            {
               Connections.Instance().RemovePOI(guild, pkmnPOI.Name);
               await ResponseMessage.SendInfoMessage(Context.Channel, $"{pkmnPOI.Name} has been removed.");
            }
         }
         else
         {
            Connections.Instance().RemovePOI(guild, pkmnPOI.Name);
            await ResponseMessage.SendInfoMessage(Context.Channel, $"{pkmnPOI.Name} has been removed.");
         }
      }

      [Command("editPOINickname")]
      [Alias("editPOINicknames")]
      [Summary("Edit Point of Interest nicknames.")]
      [Remarks("This command is used for adding, updating, and removing Point of Interest nicknames.\n" +
               "To add or update a nickname a special character (>) is used.\n" +
               "\nFor each option format the nicknameString as following:\n" +
               "Add Nickname..............nickname > Point of Interest name\n" +
               "Update Nickname........new nickname > old nickname\n" +
               "Delete Nickname.........nickname\n" +
               "\nNote: Spaces are allowed for nicknames")]
      [RegisterChannel('S')]
      public async Task EditNickname([Summary("Update the nickname of a Point of Interest using this string.")][Remainder] string nicknameString)
      {
         ulong guild = Context.Guild.Id;
         int delimeterIndex = nicknameString.IndexOf(Global.NICKNAME_DELIMITER);

         if (delimeterIndex == Global.NICKNAME_DELIMITER_MISSING)
         {
            string trim = nicknameString.Trim();
            string name = Connections.Instance().GetPOIWithNickname(guild, trim);
            if (name == null)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "editPOINickname", $"The nickname {trim} is not registered to a Point of Interest.");
            }
            else
            {
               Connections.Instance().DeletePOINickname(guild, trim);
               await ResponseMessage.SendInfoMessage(Context.Channel, $"Removed {trim} from {name}.");
            }
         }
         else
         {
            string[] arr = nicknameString.Split(Global.NICKNAME_DELIMITER);
            if (arr.Length == NumArguments)
            {
               string newNickname = arr[IndexNickname].Trim();
               string other = arr[IndexName].Trim();
               POI poi = Connections.Instance().GetPOI(guild, other);

               if (poi == null)
               {
                  poi = Connections.Instance().GetPOI(guild, Connections.Instance().GetPOIWithNickname(guild, other));
                  if (poi == null)
                  {
                     await ResponseMessage.SendErrorMessage(Context.Channel, "editPOINickname", $"{other} is not a registered nickname.");
                  }
                  else
                  {
                     Connections.Instance().UpdatePOINickname(guild, other, newNickname);
                     await ResponseMessage.SendInfoMessage(Context.Channel, $"{newNickname} has replaced {other} as a valid nickname for {poi.Name}.");
                  }
               }
               else
               {
                  Connections.Instance().AddPOINickname(guild, newNickname, poi.Name);
                  await ResponseMessage.SendInfoMessage(Context.Channel, $"{newNickname} is now a valid nickname for {poi.Name}.");
               }
            }
            else
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "editPOINickname", $"Too many delimiters found.");
            }
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
