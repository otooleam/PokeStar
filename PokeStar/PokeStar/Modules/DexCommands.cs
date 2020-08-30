using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Rest;
using Discord.Commands;
using Discord.WebSocket;
using PokeStar.DataModels;
using PokeStar.Calculators;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles pokedex commands.
   /// </summary>
   public class DexCommands : ModuleBase<SocketCommandContext>
   {
      private static readonly Dictionary<ulong, DexSelectionMessage> dexMessages = new Dictionary<ulong, DexSelectionMessage>();

      private static readonly Dictionary<string, PokemonForm> pokemonForms = new Dictionary<string, PokemonForm>(StringComparer.OrdinalIgnoreCase)
      {
         ["Rattata"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Raticate"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Raichu"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Sandshrew"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Sandslash"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Nidoran"] = new PokemonForm { formList = "-f,-m", defaultForm = "-f" },
         ["Vulpix"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Ninetales"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Diglett"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Dugtrio"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Meowth"] = new PokemonForm { formList = "-alola,-galar", defaultForm = "" },
         ["Persian"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Geodude"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Graveler"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Golem"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Farfetch'd"] = new PokemonForm { formList = "-galar", defaultForm = "" },
         ["Grimer"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Muk"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Exeggutor"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Marowak"] = new PokemonForm { formList = "-alola", defaultForm = "" },
         ["Weezing"] = new PokemonForm { formList = "-galar", defaultForm = "" },
         ["Mewtwo"] = new PokemonForm { formList = "-armor", defaultForm = "" },
         ["Unown"] = new PokemonForm { formList = "-a,-b,-c,-d,-e,-f,-g,-h,-i,-j,-k,-l,-m,-n,-o,-p,-q,-r,-s,-t,-u,-v,-w,-x,-y,-z,-!,-?,", defaultForm = "-f" },
         ["Zigzagoon"] = new PokemonForm { formList = "-galar", defaultForm = "" },
         ["Linoone"] = new PokemonForm { formList = "-galar", defaultForm = "" },
         ["Castform"] = new PokemonForm { formList = "-rain,-snow,-sun", defaultForm = "" },
         ["Deoxys"] = new PokemonForm { formList = "-attack,-defense,-speed", defaultForm = "" },
         ["Burmy"] = new PokemonForm { formList = "-plant,-sand,-trash", defaultForm = "-plant" },
         ["Wormadam"] = new PokemonForm { formList = "-plant,-sand,-trash", defaultForm = "-plant" },
         ["Cherrim"] = new PokemonForm { formList = "-sunshine,-overcast", defaultForm = "-sunshine" },
         ["Shellow"] = new PokemonForm { formList = "-east,-west", defaultForm = "-east" },
         ["Gastrodon"] = new PokemonForm { formList = "-east,-west", defaultForm = "-east" },
         ["Rotom"] = new PokemonForm { formList = "-fan,-frost,-heat,-mow,-wash", defaultForm = "" },
         ["Giratina"] = new PokemonForm { formList = "-altered,-origin", defaultForm = "-altered" },
         ["Shayman"] = new PokemonForm { formList = "-land,-sky", defaultForm = "-land" },
         ["Arceus"] = new PokemonForm { formList = "-normal,-bug,-dark,-dragon,-electric,-fairy,-fighting,-fire,-flying,-ghost,-grass,-ground,-ice,-poison,-psychic,-rock,-steel,-water", defaultForm = "-normal" },
         ["Basculin"] = new PokemonForm { formList = "-blue,-red", defaultForm = "-blue" },
         ["Darumaka"] = new PokemonForm { formList = "-galar", defaultForm = "" },
         ["Darmanitan"] = new PokemonForm { formList = "-galar,-zen,-galar-zen", defaultForm = "" },
         ["Deerling"] = new PokemonForm { formList = "-summer,-spring,-winter,-autumn", defaultForm = "-summer" },
         ["Sawsbuck"] = new PokemonForm { formList = "-summer,-spring,-winter,-autumn", defaultForm = "-summer" },
         ["Stunfisk"] = new PokemonForm { formList = "-galar", defaultForm = "" },
         ["Tornadus"] = new PokemonForm { formList = "-incarnate,-therian", defaultForm = "-incarnate" },
         ["Thundurus"] = new PokemonForm { formList = "-incarnate,-therian", defaultForm = "-incarnate" },
         ["Landorus"] = new PokemonForm { formList = "-incarnate,-therian", defaultForm = "-incarnate" },
         ["Kyurem"] = new PokemonForm { formList = "-black,-white", defaultForm = "" },
         ["Keldeo"] = new PokemonForm { formList = "-resolute", defaultForm = "" },
         ["Meloetta"] = new PokemonForm { formList = "-aria,-pirouette", defaultForm = "-aria" },
      };

      private static readonly Emoji[] selectionEmojis = {
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
         new Emoji("5️⃣"),
         new Emoji("6️⃣"),
         new Emoji("7️⃣"),
         new Emoji("8️⃣"),
         new Emoji("9️⃣"),
         new Emoji("🔟")
      };

      public static readonly int UNOWN = 201;
      public static readonly int ARCEUS = 493;

      private enum DEX_MESSAGE_TYPES
      {
         DEX_MESSAGE,
         CP_MESSAGE,
      }

      [Command("dex")]
      [Alias("pokedex")]
      [Summary("Gets information for a pokemon.")]
      [Remarks("Can search by pokemon name or my number.")]
      public async Task Dex([Summary("Get information for this pokemon.")][Remainder] string pkmn)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "D"))
         {
            bool isNumber = int.TryParse(pkmn, out int pokemonNum);

            if (isNumber)
            {
               List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

               if (pokemonWithNumber.Count == 0)
               {
                  await ResponseMessage.SendErrorMessage(Context, "dex", $"Pokemon with number {pokemonNum} cannot be found.");
               }
               else if (pokemonNum == ARCEUS)
               {
                  await ResponseMessage.SendErrorMessage(Context, "dex", $"Arceus #{pokemonNum} has too many forms to display, please search by name.");
               }
               else if (pokemonWithNumber.Count > 1 && pokemonNum != UNOWN)
               {
                  string fileName = "pokeball.png";
                  Connections.CopyFile(fileName);
                  RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(pokemonWithNumber, fileName));
                  for (int i = 0; i < pokemonWithNumber.Count; i++)
                     await dexMessage.AddReactionAsync(selectionEmojis[i]);
                  dexMessages.Add(dexMessage.Id, new DexSelectionMessage
                  {
                     SubMessageType = (int)DEX_MESSAGE_TYPES.DEX_MESSAGE,
                     potentials = pokemonWithNumber
                  });
               }
               else
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(pokemonWithNumber[0]);
                  string fileName = Connections.GetPokemonPicture(pokemon.Name);
                  Connections.CopyFile(fileName);
                  await Context.Channel.SendFileAsync(fileName, embed: BuildDexEmbed(pokemon, fileName)).ConfigureAwait(false);
                  Connections.DeleteFile(fileName);
               }
            }
            else
            {
               string name = GetPokemon(pkmn);
               Pokemon pokemon = Connections.Instance().GetPokemon(name);
               if (pokemon == null)
               {
                  await ResponseMessage.SendErrorMessage(Context, "dex", $"Pokemon {name} cannot be found.");
               }
               else
               {
                  string fileName = Connections.GetPokemonPicture(pokemon.Name);
                  Connections.CopyFile(fileName);
                  await Context.Channel.SendFileAsync(fileName, embed: BuildDexEmbed(pokemon, fileName)).ConfigureAwait(false);
                  Connections.DeleteFile(fileName);
               }
            }
         }
         else
            await ResponseMessage.SendErrorMessage(Context, "dex", "This channel is not registered to process PokéDex commands.");
      }

      [Command("cp")]
      [Summary("Gets common max CP values for a pokemon")]
      [Remarks("Can search by pokemon name or my number.")]
      public async Task CP([Summary("Get CPs for this pokemon.")][Remainder] string pkmn)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "D"))
         {
            bool isNumber = int.TryParse(pkmn, out int pokemonNum);

            if (isNumber)
            {
               List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

               if (pokemonWithNumber.Count == 0)
               {
                  await ResponseMessage.SendErrorMessage(Context, "cp", $"Pokemon with number {pokemonNum} cannot be found.");
               }
               else if (pokemonWithNumber.Count > 1 && pokemonNum != UNOWN && pokemonNum != ARCEUS)
               {
                  string fileName = "pokeball.png";
                  Connections.CopyFile(fileName);
                  RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(pokemonWithNumber, fileName));
                  Connections.DeleteFile(fileName);
                  for (int i = 0; i < pokemonWithNumber.Count; i++)
                     await dexMessage.AddReactionAsync(selectionEmojis[i]);
                  dexMessages.Add(dexMessage.Id, new DexSelectionMessage
                  {
                     SubMessageType = (int)DEX_MESSAGE_TYPES.CP_MESSAGE,
                     potentials = pokemonWithNumber
                  });
               }
               else
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(pokemonWithNumber[0]);
                  Connections.CalcAllCP(ref pokemon);
                  string fileName = Connections.GetPokemonPicture(pokemon.Name);
                  Connections.CopyFile(fileName);
                  await Context.Channel.SendFileAsync(fileName, embed: BuildCPEmbed(pokemon, fileName)).ConfigureAwait(false);
                  Connections.DeleteFile(fileName);
               }
            }
            else
            {
               string name = GetPokemon(pkmn);
               Pokemon pokemon = Connections.Instance().GetPokemon(name);
               if (pokemon == null)
               {
                  await ResponseMessage.SendErrorMessage(Context, "cp", $"Pokemon {name} cannot be found.");
               }
               else
               {
                  Connections.CalcAllCP(ref pokemon);
                  string fileName = Connections.GetPokemonPicture(pokemon.Name);
                  Connections.CopyFile(fileName);
                  await Context.Channel.SendFileAsync(fileName, embed: BuildCPEmbed(pokemon, fileName)).ConfigureAwait(false);
                  Connections.DeleteFile(fileName);
               }
            }
         }
         else
            await ResponseMessage.SendErrorMessage(Context, "cp", "This channel is not registered to process PokéDex commands.");
      }

      [Command("form")]
      [Summary("Gets all forms for a pokemon.")]
      [Remarks("Leave blank to get all pokemon with forms.\n" +
               "Send \"Alias\" to get variations for form names.")]
      public async Task Form([Summary("(Optional) Pokemon with the form.")] string pokemonName = null)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "D"))
         {
            EmbedBuilder embed = new EmbedBuilder();
            if (pokemonName == null)
            {
               StringBuilder sb = new StringBuilder();
               foreach (string key in pokemonForms.Keys)
                  sb.AppendLine(key);
               embed.AddField($"Pokemon With Forms", sb.ToString(), true);
               embed.WithColor(Color.DarkGreen);
            }
            else if (pokemonForms.ContainsKey(pokemonName))
            {
               StringBuilder sb = new StringBuilder();
               PokemonForm forms = pokemonForms[pokemonName];
               var formsList = forms.formList.Split(',');

               foreach (string form in formsList)
               {
                  sb.Append(form);
                  if (form.Equals(forms.defaultForm))
                     sb.Append("*");
                  sb.Append('\n');
               }
               embed.AddField($"Forms for {pokemonName}", sb.ToString(), true);
               embed.WithColor(Color.DarkGreen);
               embed.WithFooter("* Form is default form");
            }
            else if (pokemonName.Equals("Alias", StringComparison.OrdinalIgnoreCase))
            {
               embed.WithTitle("Form tag variations");
               embed.AddField($"-alola", "-alolan", true);
               embed.AddField($"-galar", "-garlarian", true);
               embed.AddField($"-armor", "-armored", true);
               embed.AddField($"-fighting", "-fight", true);
               embed.AddField($"-flying", "-fly", true);
               embed.AddField($"-psychic", "-psy", true);
               embed.AddField($"-galar-zen", "-garlarian-zen", true);
               embed.AddField($"-autumn", "-fall", true);
               embed.WithColor(Color.DarkGreen);
            }
            else
            {
               await ResponseMessage.SendErrorMessage(Context, "form", $"Pokemon {pokemonName} cannot be found or has no forms.");
            }
            await Context.Channel.SendMessageAsync(null, false, embed.Build()).ConfigureAwait(false);
         }
         else
            await ResponseMessage.SendErrorMessage(Context, "form", "This channel is not registered to process PokéDex commands.");
      }

      [Command("type")]
      [Summary("Gets information for a pokemon type.")]
      public async Task PokeType([Summary("Primary type.")] string type1,
                                 [Summary("(Optional) Secondary type.")] string type2 = null)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, "D"))
         {
            List<string> types = new List<string>
            {
               type1,
            };
            if (type2 != null && !type1.Equals(type2, StringComparison.OrdinalIgnoreCase))
               types.Add(type2);

            if (!CheckValidType(type1) || (types.Count == 2 && !CheckValidType(type2)))
            {
               await ResponseMessage.SendErrorMessage(Context, "type", $"{(!CheckValidType(type1) ? type1 : type2)} is not a valid type.");
            }
            else
            {
               string title = $"{type1}";
               if (types.Count == 2)
                  title += $", {type2}";

               string description = Emote.Parse(Environment.GetEnvironmentVariable($"{type1.ToUpper()}_EMOTE")).ToString();
               if (types.Count == 2)
                  description += Emote.Parse(Environment.GetEnvironmentVariable($"{type2.ToUpper()}_EMOTE")).ToString();

               var type1AttackRelations = (types.Count == 2) ? null : Connections.Instance().GetTypeAttackRelations(type1);
               var defenseRelations = Connections.Instance().GetTypeDefenseRelations(types);
               var weather = Connections.Instance().GetWeather(types);

               EmbedBuilder embed = new EmbedBuilder();
               embed.WithTitle($@"Type {title.ToUpper()}");
               embed.WithDescription(description);
               embed.AddField("Weather Boosts:", FormatWeatherList(weather), false);
               if (type1AttackRelations.HasValue)
               {
                  embed.AddField($"Super Effective against:", FormatTypeList(type1AttackRelations.Value.strong), false);
                  embed.AddField($"Not Very Effective against:", FormatTypeList(type1AttackRelations.Value.weak), false);
               }
               embed.AddField($"Weaknesses:", FormatTypeList(defenseRelations.weak), false);
               embed.AddField($"Resistances:", FormatTypeList(defenseRelations.strong), false);
               embed.WithColor(Color.DarkGreen);
               await Context.Channel.SendMessageAsync(null, false, embed.Build()).ConfigureAwait(false);
            }
         }
         else
            await ResponseMessage.SendErrorMessage(Context, "type", "This channel is not registered to process PokéDex commands.");
      }


      public static async Task DexMessageReactionHandle(IMessage message, SocketReaction reaction)
      {
         DexSelectionMessage dexMessage = dexMessages[message.Id];

         for (int i = 0; i < dexMessage.potentials.Count; i++)
         {
            if (reaction.Emote.Equals(selectionEmojis[i]))
            {
               await reaction.Channel.DeleteMessageAsync(message);
               Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.potentials[i]);
               string fileName = Connections.GetPokemonPicture(pokemon.Name);
               Connections.CopyFile(fileName);

               if (dexMessage.SubMessageType == (int)DEX_MESSAGE_TYPES.DEX_MESSAGE)
               {
                  await reaction.Channel.SendFileAsync(fileName, embed: BuildDexEmbed(pokemon, fileName)).ConfigureAwait(false);
               }
               else if (dexMessage.SubMessageType == (int)DEX_MESSAGE_TYPES.CP_MESSAGE)
               {
                  Connections.CalcAllCP(ref pokemon);
                  await reaction.Channel.SendFileAsync(fileName, embed: BuildCPEmbed(pokemon, fileName)).ConfigureAwait(false);
               }
               Connections.DeleteFile(fileName);
               return;
            }
         }
      }

      private static Embed BuildDexEmbed(Pokemon pokemon, string fileName)
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
         embed.WithColor(Color.DarkGreen);
         embed.WithFooter("* denotes STAB move ! denotes Legacy move");

         return embed.Build();
      }

      private static Embed BuildCPEmbed(Pokemon pokemon, string fileName)
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
         embed.WithColor(Color.DarkGreen);
         embed.WithFooter("* denotes Weather Boosted CP");

         return embed.Build();
      }

      /// <summary>
      /// Builds the pokedex select embed.
      /// </summary>
      /// <param name="potentials">List of potential Pokemon.</param>
      /// <param name="selectPic">Name of picture file to get.</param>
      /// <returns>Embed for selecting raid boss.</returns>
      private static Embed BuildDexSelectEmbed(List<string> potentials, string selectPic)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < potentials.Count; i++)
            sb.AppendLine($"{selectionEmojis[i]} {potentials[i]}");

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.DarkGreen);
         embed.WithTitle($"Pokemon Selection");
         embed.WithThumbnailUrl($"attachment://{selectPic}");
         embed.AddField("Please Select Pokemon", sb.ToString());
         return embed.Build();
      }

      /// <summary>
      /// Processes the pokemon name given from a command.
      /// </summary>
      /// <param name="pokemonName">Name of the pokemon.</param>
      /// <returns>Full name of the pokemon</returns>
      private static string GetPokemon(string pokemonName)
      {
         List<string> words = new List<string>(pokemonName.Split(' '));

         string form = words[words.Count - 1];
         if (form.Substring(0, 1).Equals("-", StringComparison.OrdinalIgnoreCase))
            words.RemoveAt(words.Count - 1);
         else
            form = "";

         string name = "";
         foreach (string str in words)
            name += str + " ";
         name = name.TrimEnd(' ');

         return GetFullName(name, form);
      }

      /// <summary>
      /// Gets the full name of a pokemon.
      /// The following pokemon have multiple forms:
      /// Name       Default Form
      /// -----------------------
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
      /// Note: nidoran defaults to the female form.
      /// </summary>
      /// <param name="pokemonName">Name of the pokemon</param>
      /// <param name="form">Form of the pokemon.</param>
      /// <returns>Full name of the pokemon.</returns>
      private static string GetFullName(string pokemonName, string form = "")
      {
         if (form.Length == 2)
            return $"{pokemonName} {form.ToCharArray()[1]}";
         // Alolan
         else if (form.Equals("-alola", StringComparison.OrdinalIgnoreCase) || form.Equals("-alolan", StringComparison.OrdinalIgnoreCase))
            return $"Alolan {pokemonName}";
         // Galarian
         else if (form.Equals("-galar", StringComparison.OrdinalIgnoreCase) || form.Equals("-galarian", StringComparison.OrdinalIgnoreCase))
            return $"Galarian {pokemonName}";
         // Mega
         else if (form.Equals("-mega", StringComparison.OrdinalIgnoreCase))
            return $"Mega {pokemonName}";
         else if (form.Equals("-megax", StringComparison.OrdinalIgnoreCase))
            return $"Mega {pokemonName} X";
         else if (form.Equals("-megay", StringComparison.OrdinalIgnoreCase))
            return $"Mega {pokemonName} Y";
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

      /// <summary>
      /// Formats weather boosts as a string.
      /// </summary>
      /// <param name="weatherList">List of weather that boosts the type(s).</param>
      /// <returns>Weather for type(s) as a string.</returns>
      private static string FormatWeatherList(List<string> weatherList)
      {
         string weatherString = "";
         foreach (var weather in weatherList)
            weatherString += $"{Emote.Parse(Environment.GetEnvironmentVariable($"{weather.Replace(' ', '_').ToUpper()}_EMOTE"))} ";
         return weatherString;
      }

      /// <summary>
      /// Formats type relations as a string.
      /// </summary>
      /// <param name="relations">Dictionary of type relations for the type(s).</param>
      /// <returns>Type relations for type(s) as a string.</returns>
      private static string FormatTypeList(Dictionary<string, int> relations)
      {
         if (relations.Count == 0)
            return "-----";
         string relationString = "";
         foreach (var relation in relations)
         {
            double multiplier = TypeCalculator.CalcTypeEffectivness(relation.Value) * 100.0;
            string typeEmote = Emote.Parse(Environment.GetEnvironmentVariable($"{relation.Key.ToUpper()}_EMOTE")).ToString();
            relationString += $"{typeEmote} {relation.Key}: {multiplier}%\n";
         }
         return relationString;
      }

      /// <summary>
      /// Checks if a type is vaid.
      /// </summary>
      /// <param name="type">The type to check.</param>
      /// <returns>True if the type is valid, otherwise false.</returns>
      private static bool CheckValidType(string type)
      {
         return Environment.GetEnvironmentVariable($"{type.ToUpper()}_EMOTE") != null;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      public static bool IsDexSubMessage(ulong id)
      {
         return dexMessages.ContainsKey(id);
      }
   }

   /// <summary>
   /// 
   /// </summary>
   public struct PokemonForm
   {
      public string formList;
      public string defaultForm;
   }

   /// <summary>
   /// 
   /// </summary>
   public struct DexSelectionMessage
   {
      public int SubMessageType;
      public List<string> potentials;
   }
}