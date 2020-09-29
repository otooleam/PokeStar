using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PokeStar.ConnectionInterface;
using PokeStar.DataModels;

namespace PokeStar.ModuleParents
{
   public class DexCommandParent : ModuleBase<SocketCommandContext>
   {
      /// <summary>
      /// Color used for dex command embeds.
      /// </summary>
      protected static readonly Color DexMessageColor = Color.Green;

      /// <summary>
      /// Empty raid image file name.
      /// </summary>
      protected static readonly string POKEDEX_SELECTION_IMAGE = "pokeball.png";

      /// <summary>
      /// Saved dex messages.
      /// </summary>
      protected static readonly Dictionary<ulong, Tuple<int, List<string>>> dexMessages = new Dictionary<ulong, Tuple<int, List<string>>>();

      /// <summary>
      /// Dictionary of all Pokémon with form differences flags
      /// Values are tuples with item1 as a the list of form flags
      /// and item2 with the default form flag. Item1 is delimeted
      /// by commas (,).
      /// </summary>
      protected static readonly Dictionary<string, Tuple<string, string>> pokemonForms = new Dictionary<string, Tuple<string, string>>(StringComparer.OrdinalIgnoreCase)
      {
         ["Venusaur"] = new Tuple<string, string>("-mega", ""),
         ["Charizard"] = new Tuple<string, string>("-mega,-megax,-megay,-x,-y", ""),
         ["Blastoise"] = new Tuple<string, string>("-mega", ""),
         ["Beedrill"] = new Tuple<string, string>("-mega", ""),
         ["Pigeot"] = new Tuple<string, string>("-mega", ""),
         ["Rattata"] = new Tuple<string, string>("-alola", ""),
         ["Raticate"] = new Tuple<string, string>("-alola", ""),
         ["Raichu"] = new Tuple<string, string>("-alola", ""),
         ["Sandshrew"] = new Tuple<string, string>("-alola", ""),
         ["Sandslash"] = new Tuple<string, string>("-alola", ""),
         ["Nidoran F"] = new Tuple<string, string>("-f,-m", "-f"),
         ["Vulpix"] = new Tuple<string, string>("-alola", ""),
         ["Ninetales"] = new Tuple<string, string>("-alola", ""),
         ["Diglett"] = new Tuple<string, string>("-alola", ""),
         ["Dugtrio"] = new Tuple<string, string>("-alola", ""),
         ["Meowth"] = new Tuple<string, string>("-alola,-galar", ""),
         ["Persian"] = new Tuple<string, string>("-alola", ""),
         ["Geodude"] = new Tuple<string, string>("-alola", ""),
         ["Graveler"] = new Tuple<string, string>("-alola", ""),
         ["Golem"] = new Tuple<string, string>("-alola", ""),
         ["Farfetch'd"] = new Tuple<string, string>("-galar", ""),
         ["Grimer"] = new Tuple<string, string>("-alola", ""),
         ["Muk"] = new Tuple<string, string>("-alola", ""),
         ["Gengar"] = new Tuple<string, string>("-mega", ""),
         ["Exeggutor"] = new Tuple<string, string>("-alola", ""),
         ["Marowak"] = new Tuple<string, string>("-alola", ""),
         ["Weezing"] = new Tuple<string, string>("-galar", ""),
         ["Mewtwo"] = new Tuple<string, string>("-armor", ""),
         ["Unown F"] = new Tuple<string, string>("-a,-b,-c,-d,-e,-f,-g,-h,-i,-j,-k,-l,-m,-n,-o,-p,-q,-r,-s,-t,-u,-v,-w,-x,-y,-z,-!,-?,", "-f"),
         ["Zigzagoon"] = new Tuple<string, string>("-galar", ""),
         ["Linoone"] = new Tuple<string, string>("-galar", ""),
         ["Houndoom"] = new Tuple<string, string>("-mega", ""),
         ["Castform"] = new Tuple<string, string>("-rain,-snow,-sun", ""),
         ["Deoxys"] = new Tuple<string, string>("-attack,-defense,-speed", ""),
         ["Plant Cloak Burmy"] = new Tuple<string, string>("-plant,-sand,-trash", "-plant"),
         ["Plant Cloak Wormadam"] = new Tuple<string, string>("-plant,-sand,-trash", "-plant"),
         ["Sunshine Cherrim"] = new Tuple<string, string>("-sunshine,-overcast", "-sunshine"),
         ["Shellow"] = new Tuple<string, string>("-east,-west", "-east"),
         ["Gastrodon"] = new Tuple<string, string>("-east,-west", "-east"),
         ["Rotom"] = new Tuple<string, string>("-fan,-frost,-heat,-mow,-wash", ""),
         ["Altered Form Giratina"] = new Tuple<string, string>("-altered,-origin", "-altered"),
         ["Land Form Shayman"] = new Tuple<string, string>("-land,-sky", "-land"),
         ["Arceus Normal"] = new Tuple<string, string>("-normal,-bug,-dark,-dragon,-electric,-fairy,-fighting,-fire,-flying,-ghost,-grass,-ground,-ice,-poison,-psychic,-rock,-steel,-water", "-normal"),
         ["Blue Striped Basculin"] = new Tuple<string, string>("-blue,-red", "-blue"),
         ["Darumaka"] = new Tuple<string, string>("-galar", ""),
         ["Darmanitan"] = new Tuple<string, string>("-galar,-zen,-galar-zen", ""),
         ["Summer Deerling"] = new Tuple<string, string>("-summer,-spring,-winter,-autumn", "-summer"),
         ["Summer Sawsbuck"] = new Tuple<string, string>("-summer,-spring,-winter,-autumn", "-summer"),
         ["Stunfisk"] = new Tuple<string, string>("-galar", ""),
         ["Incarnate Tornadus"] = new Tuple<string, string>("-incarnate,-therian", "-incarnate"),
         ["Incarnate Thundurus"] = new Tuple<string, string>("-incarnate,-therian", "-incarnate"),
         ["Incarnate Landorus"] = new Tuple<string, string>("-incarnate,-therian", "-incarnate"),
         ["Kyurem"] = new Tuple<string, string>("-black,-white", ""),
         ["Keldeo"] = new Tuple<string, string>("-resolute", ""),
         ["Aria Meloetta"] = new Tuple<string, string>("-aria,-pirouette", "-aria"),
      };

      /// <summary>
      /// Types of dex sub messages.
      /// </summary>
      protected enum DEX_MESSAGE_TYPES
      {
         DEX_MESSAGE,
         CP_MESSAGE,
         FORM_MESSAGE,
         EVO_MESSAGE,
         NICKNAME_MESSAGE,
      }

      /// Message checkers

      /// <summary>
      /// Checks if a message is a dex message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a dex message, otherwise false.</returns>
      public static bool IsDexSubMessage(ulong id)
      {
         return dexMessages.ContainsKey(id);
      }

      /// Message reaction handlers

      /// <summary>
      /// Handles a reaction on a dex message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns>Completed Task.</returns>
      public static async Task DexMessageReactionHandle(IMessage message, SocketReaction reaction, ulong guildId)
      {
         Tuple<int, List<string>> dexMessage = dexMessages[message.Id];
         for (int i = 0; i < dexMessage.Item2.Count; i++)
         {
            if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
            {
               await message.DeleteAsync();
               Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Item2[i]);
               string fileName = Connections.GetPokemonPicture(pokemon.Name);
               Connections.CopyFile(fileName);
               if (dexMessage.Item1 == (int)DEX_MESSAGE_TYPES.DEX_MESSAGE)
               {
                  await reaction.Channel.SendFileAsync(fileName, embed: BuildDexEmbed(pokemon, fileName));
               }
               else if (dexMessage.Item1 == (int)DEX_MESSAGE_TYPES.CP_MESSAGE)
               {
                  Connections.CalcAllCP(ref pokemon);
                  await reaction.Channel.SendFileAsync(fileName, embed: BuildCPEmbed(pokemon, fileName));
               }
               else if (dexMessage.Item1 == (int)DEX_MESSAGE_TYPES.FORM_MESSAGE)
               {
                  List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemon.Number);

                  if (pokemonWithNumber.Count == 1)
                  {
                     await ResponseMessage.SendErrorMessage(reaction.Channel, "form", $"{pokemonWithNumber.First()} does not have different forms.");
                  }
                  else if (pokemonWithNumber.Count > 1)
                  {
                     string baseName = pokemonWithNumber.Where(form => pokemonForms.ContainsKey(form)).ToList().First();

                     Tuple<string, string> forms = pokemonForms[baseName];
                     List<string> formsList = forms.Item1.Split(',').ToList();

                     string baseFileName = Connections.GetPokemonPicture(baseName);
                     Connections.CopyFile(fileName);
                     await reaction.Channel.SendFileAsync(baseFileName, embed: BuildFormEmbed(baseName, formsList, forms.Item2, fileName));
                     Connections.DeleteFile(baseFileName);
                  }
               }
               else if (dexMessage.Item1 == (int)DEX_MESSAGE_TYPES.EVO_MESSAGE)
               {
                  Dictionary<string, string> evolutions = GenerateEvoDict(pokemon.Name);
                  string firstFileName = Connections.GetPokemonPicture(pokemon.Name);
                  Connections.CopyFile(firstFileName);
                  await reaction.Channel.SendFileAsync(firstFileName, embed: BuildEvoEmbed(evolutions, pokemon.Name, firstFileName));
                  Connections.DeleteFile(firstFileName);
               }
               else if (dexMessage.Item1 == (int)DEX_MESSAGE_TYPES.NICKNAME_MESSAGE)
               {
                  List<string> nicknames = Connections.Instance().GetNicknames(guildId, pokemon.Name);
                  await reaction.Channel.SendFileAsync(fileName, embed: BuildNicknameEmbed(nicknames, pokemon.Name, fileName));
               }
               Connections.DeleteFile(fileName);
               return;
            }
         }
         await message.RemoveReactionAsync(reaction.Emote, (SocketGuildUser)reaction.User);
      }

      /// Embed builders

      /// <summary>
      /// Builds a dex embed.
      /// </summary>
      /// <param name="pokemon">Pokémon to display</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon.</returns>
      protected static Embed BuildDexEmbed(Pokemon pokemon, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($@"#{pokemon.Number} {pokemon.Name}");
         embed.WithDescription(pokemon.Description);
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Type", pokemon.TypeToString(), true);
         embed.AddField("Weather Boosts", pokemon.WeatherToString(), true);
         embed.AddField("Details", pokemon.DetailsToString(), true);
         embed.AddField("Stats", pokemon.StatsToString(), true);
         embed.AddField("Resistances", pokemon.ResistanceToString(), true);
         embed.AddField("Weaknesses", pokemon.WeaknessToString(), true);
         embed.AddField("Fast Moves", pokemon.FastMoveToString(), true);
         embed.AddField("Charge Moves", pokemon.ChargeMoveToString(), true);
         embed.AddField("Counters", pokemon.CounterToString(), false);
         if (pokemon.HasForms())
         {
            embed.AddField("Forms", pokemon.FormsToString(), true);
         }
         if (pokemon.IsRegional())
         {
            embed.AddField("Regions", pokemon.RegionalToString(), true);
         }
         embed.WithColor(DexMessageColor);
         embed.WithFooter("* denotes STAB move ! denotes Legacy move");
         return embed.Build();
      }

      /// <summary>
      /// Builds a cp embed.
      /// </summary>
      /// <param name="pokemon">Pokémon to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon's CP.</returns>
      protected static Embed BuildCPEmbed(Pokemon pokemon, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($@"#{pokemon.Number} {pokemon.Name} CP");
         embed.WithDescription($"Max CP values for {pokemon.Name}");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField($"Max CP (Level 40)", pokemon.CPMax, true);
         embed.AddField($"Max Buddy CP (Level 41)", pokemon.CPBestBuddy, true);
         embed.AddField($"Raid CP (Level 20)", pokemon.RaidCPToString(), false);
         embed.AddField($"Hatch CP (Level 20)", pokemon.HatchCPToString(), false);
         embed.AddField($"Quest CP (Level 15)", pokemon.QuestCPToString(), false);
         embed.AddField("Wild CP (Level 1-35)", pokemon.WildCPToString(), false);
         embed.WithColor(DexMessageColor);
         embed.WithFooter("* denotes Weather Boosted CP");
         return embed.Build();
      }

      /// <summary>
      /// Builds a form embed.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="forms">List of form tags.</param>
      /// <param name="defaultForm">Default form tag.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon's form tags.</returns>
      protected static Embed BuildFormEmbed(string pokemonName, List<string> forms, string defaultForm, string fileName)
      {
         StringBuilder sb = new StringBuilder();
         foreach (string form in forms)
         {
            sb.Append(form);
            if (form.Equals(defaultForm, StringComparison.OrdinalIgnoreCase))
            {
               sb.Append('*');
            }
            sb.Append('\n');
         }

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField($"Forms for {pokemonName}", sb.ToString(), true);
         embed.WithColor(DexMessageColor);
         embed.WithFooter("* Form is default form");
         return embed.Build();
      }

      /// <summary>
      /// Builds an evolution embed.
      /// </summary>
      /// <param name="evolutions">Dictionary of evolutions.</param>
      /// <param name="initialPokemon">Pokémon that was searched for.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon's evolutions.</returns>
      protected static Embed BuildEvoEmbed(Dictionary<string, string> evolutions, string initialPokemon, string fileName)
      {

         EmbedBuilder embed = new EmbedBuilder();
         if (evolutions.Count == 1)
         {
            embed.WithTitle($"Evolution Family for {evolutions.First().Key}");
            embed.WithThumbnailUrl($"attachment://{fileName}");
            embed.WithDescription("This Pokémon does not evolve or evolve from any other Pokémon");
            embed.WithColor(DexMessageColor);
         }
         else
         {
            embed.WithTitle($"Evolution Family for {evolutions.First().Key}");
            embed.WithThumbnailUrl($"attachment://{fileName}");
            foreach (string key in evolutions.Keys)
            {
               string markdown = (key.Equals(initialPokemon, StringComparison.OrdinalIgnoreCase)) ? "***" : "**";

               embed.AddField($"{markdown}{key}{markdown}", evolutions[key]);
            }
            embed.WithColor(DexMessageColor);
         }
         return embed.Build();
      }

      /// <summary>
      /// Builds a nickname embed.
      /// </summary>
      /// <param name="nicknames">List of nicknames.</param>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon's nicknames.</returns>
      protected static Embed BuildNicknameEmbed(List<string> nicknames, string pokemonName, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.WithColor(DexMessageColor);

         if (nicknames.Count == 0)
         {
            embed.WithTitle($"There are no nicknames registered for {pokemonName}.");
         }
         else
         {
            StringBuilder sb = new StringBuilder();
            foreach (string nickname in nicknames)
            {
               sb.AppendLine(nickname);
            }
            embed.AddField($"**Nicknames for {pokemonName}**", sb.ToString());
         }
         return embed.Build();
      }

      /// <summary>
      /// Builds the PokéDex select embed.
      /// </summary>
      /// <param name="potentials">List of potential Pokémon.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for selecting a Pokémon.</returns>
      protected static Embed BuildDexSelectEmbed(List<string> potentials, string fileName)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < potentials.Count; i++)
         {
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} {potentials[i]}");
         }

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(DexMessageColor);
         embed.WithTitle($"Pokemon Selection");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Do you mean...?", sb.ToString());
         return embed.Build();
      }

      /// Name processors

      /// <summary>
      /// Processes the Pokémon name given from a command.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>Full name of the Pokémon.</returns>
      protected static string GetPokemonName(string pokemonName)
      {
         List<string> words = new List<string>(pokemonName.Split(' '));

         string form = words[words.Count - 1];
         if (form.Substring(0, 1).Equals("-", StringComparison.OrdinalIgnoreCase))
         {
            words.RemoveAt(words.Count - 1);
         }
         else
         {
            form = "";
         }

         string name = "";
         foreach (string str in words)
         {
            name += str + " ";
         }
         name = name.TrimEnd(' ');

         return GetFullName(name, form);
      }

      /// <summary>
      /// Gets the full name of a Pokémon.
      /// The following Pokémon have multiple forms:
      /// Name       Default Form
      /// -----------------------
      /// Nidoran    F
      /// Unown      F
      /// Burmy      Plant Cloak
      /// Wormadam   Plant Cloak
      /// Cherrim    Sunshine
      /// Shellos    East Sea
      /// Gastrodon  East Sea
      /// Giratina   Altered Form
      /// Shaymin    Land Form
      /// Arceus     Normal
      /// Basculin   Blue Striped
      /// Deerling   Summer Form
      /// Sawsbuck   Summer Form
      /// Tornadus   Incarnate
      /// Thundurus  Incarnate
      /// Landorus   Incarnate
      /// Meloetta   Aria
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="form">Form of the Pokémon.</param>
      /// <returns>Full name of the Pokémon.</returns>
      private static string GetFullName(string pokemonName, string form = "")
      {
         if (form.Length == 2)
         {
            string mega = "";
            if ((form.Equals("-x", StringComparison.OrdinalIgnoreCase) || form.Equals("-y", StringComparison.OrdinalIgnoreCase)) &&
                (pokemonName.Equals("charizard", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("mewtwo", StringComparison.OrdinalIgnoreCase)))
               mega = "mega ";
            return $"{mega}{pokemonName} {form.ToCharArray()[1]}";
         }
         // Alolan
         else if (form.Equals("-alola", StringComparison.OrdinalIgnoreCase) || form.Equals("-alolan", StringComparison.OrdinalIgnoreCase))
            return $"Alolan {pokemonName}";
         // Galarian
         else if (form.Equals("-galar", StringComparison.OrdinalIgnoreCase) || form.Equals("-galarian", StringComparison.OrdinalIgnoreCase))
            return $"Galarian {pokemonName}";
         // Mega
         else if (form.Equals("-megay", StringComparison.OrdinalIgnoreCase) || form.Equals("-mega-y", StringComparison.OrdinalIgnoreCase) || (form.Equals("-mega", StringComparison.OrdinalIgnoreCase) && (pokemonName.Equals("charizard", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("mewtwo", StringComparison.OrdinalIgnoreCase))))
            return $"Mega {pokemonName} Y";
         else if (form.Equals("-megax", StringComparison.OrdinalIgnoreCase) || form.Equals("-mega-x", StringComparison.OrdinalIgnoreCase))
            return $"Mega {pokemonName} X";
         else if (form.Equals("-mega", StringComparison.OrdinalIgnoreCase))
            return $"Mega {pokemonName}";
         // Nidoran
         else if (form.Equals("-female", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} F";
         else if (form.Equals("-male", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} M";
         // Mewtwo
         else if (form.Equals("-armor", StringComparison.OrdinalIgnoreCase) || form.Equals("-armored", StringComparison.OrdinalIgnoreCase))
            return $"Armored {pokemonName}";
         /// Unown and Nidoran
         else if (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("unown", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("nidoran", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} F";
         // Castform
         else if (form.Equals("-rain", StringComparison.OrdinalIgnoreCase))
            return $"Rainy {pokemonName}";
         else if (form.Equals("-snow", StringComparison.OrdinalIgnoreCase))
            return $"Snowy {pokemonName}";
         else if (form.Equals("-sun", StringComparison.OrdinalIgnoreCase))
            return $"Sunny {pokemonName}";
         // Deoxys
         else if (form.Equals("-attack", StringComparison.OrdinalIgnoreCase))
            return $"Attack Form {pokemonName}";
         else if (form.Equals("-defense", StringComparison.OrdinalIgnoreCase))
            return $"Defense Form {pokemonName}";
         else if (form.Equals("-speed", StringComparison.OrdinalIgnoreCase))
            return $"Speed Form {pokemonName}";
         // Burmy and Wormadam
         else if (form.Equals("-plant", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("burmy", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("wormadam", StringComparison.OrdinalIgnoreCase))))
            return $"Plant Cloak {pokemonName}";
         else if (form.Equals("-sand", StringComparison.OrdinalIgnoreCase))
            return $"Sand Cloak {pokemonName}";
         else if (form.Equals("-trash", StringComparison.OrdinalIgnoreCase))
            return $"Trash Cloak {pokemonName}";
         // Cherrim
         else if (form.Equals("-sunshine", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("cherrim", StringComparison.OrdinalIgnoreCase)))
            return $"Sunshine {pokemonName}";
         else if (form.Equals("-overcast", StringComparison.OrdinalIgnoreCase))
            return $"Overcast {pokemonName}";
         // Shellos and Gastrodon
         else if (form.Equals("-east", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("shellos", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("gastrodon", StringComparison.OrdinalIgnoreCase))))
            return $"East Sea {pokemonName}";
         else if (form.Equals("-west", StringComparison.OrdinalIgnoreCase))
            return $"West Sea {pokemonName}";
         // Rotom
         else if (form.Equals("-fan", StringComparison.OrdinalIgnoreCase))
            return $"Fan {pokemonName}";
         else if (form.Equals("-frost", StringComparison.OrdinalIgnoreCase))
            return $"Frost {pokemonName}";
         else if (form.Equals("-heat", StringComparison.OrdinalIgnoreCase))
            return $"Heat {pokemonName}";
         else if (form.Equals("-mow", StringComparison.OrdinalIgnoreCase))
            return $"Mow {pokemonName}";
         else if (form.Equals("-wash", StringComparison.OrdinalIgnoreCase))
            return $"Wash {pokemonName}";
         // Giratina
         else if (form.Equals("-altered", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("giratina", StringComparison.OrdinalIgnoreCase)))
            return $"Altered Form {pokemonName}";
         else if (form.Equals("-origin", StringComparison.OrdinalIgnoreCase))
            return $"Origin Form {pokemonName}";
         // Shayman
         else if (form.Equals("-land", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("shayman", StringComparison.OrdinalIgnoreCase)))
            return $"Land Form {pokemonName}";
         else if (form.Equals("-sky", StringComparison.OrdinalIgnoreCase))
            return $"Sky Form {pokemonName}";
         // Arceus
         else if (form.Equals("-normal", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("arceus", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} Normal";
         else if (form.Equals("-bug", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Bug";
         else if (form.Equals("-dark", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Dark";
         else if (form.Equals("-dragon", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Dragon";
         else if (form.Equals("-electric", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Electric";
         else if (form.Equals("-fairy", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Fairy";
         else if (form.Equals("-fighting", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("-fight", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Fighting";
         else if (form.Equals("-fire", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Fire";
         else if (form.Equals("-flying", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("-fly", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Flying";
         else if (form.Equals("-ghost", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Ghost";
         else if (form.Equals("-grass", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Grass";
         else if (form.Equals("-ground", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Ground";
         else if (form.Equals("-ice", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Ice";
         else if (form.Equals("-poison", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Poison";
         else if (form.Equals("-psychic", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("-psy", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Psychic";
         else if (form.Equals("-rock", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Rock";
         else if (form.Equals("-steel", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Steel";
         else if (form.Equals("-water", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Water";
         // Basculin
         else if (form.Equals("-blue", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("basculin", StringComparison.OrdinalIgnoreCase)))
            return $"Blue Striped {pokemonName}";
         else if (form.Equals("-red", StringComparison.OrdinalIgnoreCase))
            return $"Red Striped {pokemonName}";
         // Darmanitan
         else if (form.Equals("-zen", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Zen Mode";
         else if (form.Equals("-galar-zen", StringComparison.OrdinalIgnoreCase) || form.Equals("-galarian-zen", StringComparison.OrdinalIgnoreCase))
            return $"Galarian {pokemonName} Zen Mode";
         // Deerling and Sawsbuck
         else if (form.Equals("-summer", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("deerling", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("sawsbuck", StringComparison.OrdinalIgnoreCase))))
            return $"{pokemonName} Summer Form";
         else if (form.Equals("-spring", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Spring Form";
         else if (form.Equals("-winter", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Winter Form";
         else if (form.Equals("-autumn", StringComparison.OrdinalIgnoreCase) || form.Equals("-fall", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Autumn Form";
         // Tornadus, Thundurus, and Landorus
         else if (form.Equals("-incarnate", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("tornadus", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("thundurus", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("landorus", StringComparison.OrdinalIgnoreCase))))
            return $"Incarnate {pokemonName}";
         else if (form.Equals("-therian", StringComparison.OrdinalIgnoreCase))
            return $"Therian {pokemonName}";
         // Kyurem
         else if (form.Equals("-black", StringComparison.OrdinalIgnoreCase))
            return $"Black {pokemonName}";
         else if (form.Equals("-white", StringComparison.OrdinalIgnoreCase))
            return $"White {pokemonName}";
         // Keldeo
         else if (form.Equals("-resolute", StringComparison.OrdinalIgnoreCase))
            return $"Resolute {pokemonName}";
         // Meloetta
         else if (form.Equals("-aria", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("meloetta", StringComparison.OrdinalIgnoreCase)))
            return $"Aria {pokemonName}";
         else if (form.Equals("-pirouette", StringComparison.OrdinalIgnoreCase))
            return $"Pirouette {pokemonName}";
         return pokemonName;
      }

      /// Evolution processors

      /// <summary>
      /// Generage an ordered dictionary of evolutions.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>Ordered dictionary of evolutions.</returns>
      public static Dictionary<string, string> GenerateEvoDict(string pokemonName)
      {
         List<Evolution> initEvoFamily = Connections.Instance().GetEvolutionFamily(pokemonName);

         if (initEvoFamily.Count == 0)
         {
            return new Dictionary<string, string>()
            {
               [pokemonName] = ""
            };
         }

         foreach (Evolution evo in initEvoFamily)
         {
            foreach (Evolution evoComp in initEvoFamily)
            {
               evo.Combine(evoComp);
            }
         }
         List<Evolution> normalEvoFamily = initEvoFamily.Where(x => x.Candy != Global.BAD_EVOLUTION).ToList();

         string basePokemon = normalEvoFamily.First().Start;
         bool baseChanged = true;
         while (baseChanged)
         {
            baseChanged = false;
            foreach (Evolution evo in normalEvoFamily)
            {
               if (evo.End.Equals(basePokemon, StringComparison.OrdinalIgnoreCase))
               {
                  basePokemon = evo.Start;
                  baseChanged = true;
               }
            }
         }

         EvolutionNode tree = BuildEvolutionTree(basePokemon, normalEvoFamily);
         return EvolutionTreeToString(tree);
      }

      /// <summary>
      /// Recursivly builds an evolution tree.
      /// A tree is made up of evolution nodes.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="evolutions">List of evolutions.</param>
      /// <returns>Evolution node that starts the tree.</returns>
      private static EvolutionNode BuildEvolutionTree(string pokemonName, List<Evolution> evolutions)
      {
         string method = "";
         foreach (Evolution evo in evolutions)
         {
            if (pokemonName.Equals(evo.End, StringComparison.OrdinalIgnoreCase))
            {
               method = evo.MethodToString();
            }
         }

         EvolutionNode node = new EvolutionNode
         {
            Name = pokemonName,
            Method = method
         };

         foreach (Evolution evo in evolutions)
         {
            if (pokemonName.Equals(evo.Start, StringComparison.OrdinalIgnoreCase))
            {
               node.Evolutions.Add(BuildEvolutionTree(evo.End, evolutions));
            }
         }
         return node;
      }

      /// <summary>
      /// Converts an evolution tree to a dictionary.
      /// </summary>
      /// <param name="node">Node to convert to dictionary.</param>
      /// <param name="previousEvolution">Name of previous evolution.</param>
      /// <returns>Ordered dictionary of evolutions.</returns>
      private static Dictionary<string, string> EvolutionTreeToString(EvolutionNode node, string previousEvolution = null)
      {
         Dictionary<string, string> evolutions = new Dictionary<string, string>();
         string evoString = previousEvolution == null ? "Base Form" : $"Evolves from {previousEvolution} with {node.Method}";
         evolutions.Add(node.Name, evoString);

         foreach (EvolutionNode evo in node.Evolutions)
         {
            evolutions = evolutions.Union(EvolutionTreeToString(evo, node.Name)).ToDictionary(x => x.Key, x => x.Value);
         }
         return evolutions;
      }
   }
}
