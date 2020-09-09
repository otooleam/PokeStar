using System;
using System.Linq;
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
      private static readonly Color DexMessageColor = Color.Green;

      private static readonly Dictionary<ulong, Tuple<int, List<string>>> dexMessages = new Dictionary<ulong, Tuple<int, List<string>>>();

      private static readonly Dictionary<string, Tuple<string, string>> pokemonForms = new Dictionary<string, Tuple<string, string>>(StringComparer.OrdinalIgnoreCase)
      {
         ["Venusaur"] = new Tuple<string, string>("-mega", ""),
         ["Charizard"] = new Tuple<string, string>("-mega,-megax,-megay,-x,-y", ""),
         ["Blastoise"] = new Tuple<string, string>("-mega", ""),
         ["Beedrill"] = new Tuple<string, string>("-mega", ""),
         ["Rattata"] = new Tuple<string, string>("-alola", ""),
         ["Raticate"] = new Tuple<string, string>("-alola", ""),
         ["Raichu"] = new Tuple<string, string>("-alola", ""),
         ["Sandshrew"] = new Tuple<string, string>("-alola", ""),
         ["Sandslash"] = new Tuple<string, string>("-alola", ""),
         ["Nidoran"] = new Tuple<string, string>("-f,-m", "-f"),
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
         ["Exeggutor"] = new Tuple<string, string>("-alola", ""),
         ["Marowak"] = new Tuple<string, string>("-alola", ""),
         ["Weezing"] = new Tuple<string, string>("-galar", ""),
         ["Mewtwo"] = new Tuple<string, string>("-armor", ""),
         ["Unown"] = new Tuple<string, string>("-a,-b,-c,-d,-e,-f,-g,-h,-i,-j,-k,-l,-m,-n,-o,-p,-q,-r,-s,-t,-u,-v,-w,-x,-y,-z,-!,-?,", "-f"),
         ["Zigzagoon"] = new Tuple<string, string>("-galar", ""),
         ["Linoone"] = new Tuple<string, string>("-galar", ""),
         ["Castform"] = new Tuple<string, string>("-rain,-snow,-sun", ""),
         ["Deoxys"] = new Tuple<string, string>("-attack,-defense,-speed", ""),
         ["Burmy"] = new Tuple<string, string>("-plant,-sand,-trash", "-plant"),
         ["Wormadam"] = new Tuple<string, string>("-plant,-sand,-trash", "-plant"),
         ["Cherrim"] = new Tuple<string, string>("-sunshine,-overcast", "-sunshine"),
         ["Shellow"] = new Tuple<string, string>("-east,-west", "-east"),
         ["Gastrodon"] = new Tuple<string, string>("-east,-west", "-east"),
         ["Rotom"] = new Tuple<string, string>("-fan,-frost,-heat,-mow,-wash", ""),
         ["Giratina"] = new Tuple<string, string>("-altered,-origin", "-altered"),
         ["Shayman"] = new Tuple<string, string>("-land,-sky", "-land"),
         ["Arceus"] = new Tuple<string, string>("-normal,-bug,-dark,-dragon,-electric,-fairy,-fighting,-fire,-flying,-ghost,-grass,-ground,-ice,-poison,-psychic,-rock,-steel,-water", "-normal"),
         ["Basculin"] = new Tuple<string, string>("-blue,-red", "-blue"),
         ["Darumaka"] = new Tuple<string, string>("-galar", ""),
         ["Darmanitan"] = new Tuple<string, string>("-galar,-zen,-galar-zen", ""),
         ["Deerling"] = new Tuple<string, string>("-summer,-spring,-winter,-autumn", "-summer"),
         ["Sawsbuck"] = new Tuple<string, string>("-summer,-spring,-winter,-autumn", "-summer"),
         ["Stunfisk"] = new Tuple<string, string>("-galar", ""),
         ["Tornadus"] = new Tuple<string, string>("-incarnate,-therian", "-incarnate"),
         ["Thundurus"] = new Tuple<string, string>("-incarnate,-therian", "-incarnate"),
         ["Landorus"] = new Tuple<string, string>("-incarnate,-therian", "-incarnate"),
         ["Kyurem"] = new Tuple<string, string>("-black,-white", ""),
         ["Keldeo"] = new Tuple<string, string>("-resolute", ""),
         ["Meloetta"] = new Tuple<string, string>("-aria,-pirouette", "-aria"),
      };

      private static readonly string POKEDEX_SELECTION_IMAGE = "pokeball.png";

      private enum DEX_MESSAGE_TYPES
      {
         DEX_MESSAGE,
         CP_MESSAGE,
         EVO_MESSAGE,
      }

      [Command("dex")]
      [Alias("pokedex")]
      [Summary("Gets the PokéDex entry for a given Pokémon.")]
      [Remarks("Can search by Pokémon name or by number.")]
      public async Task Dex([Summary("Get information for this Pokémon.")][Remainder] string pokemon)
      {
         if (!ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, Global.REGISTER_STRING_DEX))
         {
            await ResponseMessage.SendErrorMessage(Context, "dex", "This channel is not registered to process PokéDex commands.");
         }
         else
         {
            bool isNumber = int.TryParse(pokemon, out int pokemonNum);
            if (isNumber)
            {
               List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

               if (pokemonWithNumber.Count == 0)
               {
                  await ResponseMessage.SendErrorMessage(Context, "dex", $"Pokémon with number {pokemonNum} cannot be found.");
               }
               else if (pokemonNum == Global.ARCEUS_NUMBER)
               {
                  await ResponseMessage.SendErrorMessage(Context, "dex", $"Arceus #{pokemonNum} has too many forms to display, please search by name.");
               }
               else if (pokemonWithNumber.Count > 1 && pokemonNum != Global.UNOWN_NUMBER)
               {
                  string fileName = POKEDEX_SELECTION_IMAGE;
                  Connections.CopyFile(fileName);
                  RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(pokemonWithNumber, fileName));
                  for (int i = 0; i < pokemonWithNumber.Count; i++)
                  {
                     await dexMessage.AddReactionAsync(Global.SELECTION_EMOJIS[i]);
                  }
                  dexMessages.Add(dexMessage.Id, new Tuple<int, List<string>>((int)DEX_MESSAGE_TYPES.DEX_MESSAGE, pokemonWithNumber));
               }
               else
               {
                  Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
                  string fileName = Connections.GetPokemonPicture(pkmn.Name);
                  Connections.CopyFile(fileName);
                  await Context.Channel.SendFileAsync(fileName, embed: BuildDexEmbed(pkmn, fileName));
                  Connections.DeleteFile(fileName);
               }
            }
            else
            {
               string name = GetPokemon(pokemon);
               Pokemon pkmn = Connections.Instance().GetPokemon(name);
               if (pkmn == null)
               {
                  List<string> pokemonNames = Connections.Instance().FuzzyNameSearch(name);

                  string fileName = POKEDEX_SELECTION_IMAGE;
                  Connections.CopyFile(fileName);
                  RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(pokemonNames, fileName));
                  await dexMessage.AddReactionsAsync(Global.SELECTION_EMOJIS);

                  dexMessages.Add(dexMessage.Id, new Tuple<int, List<string>>((int)DEX_MESSAGE_TYPES.DEX_MESSAGE, pokemonNames));
               }
               else
               {
                  string fileName = Connections.GetPokemonPicture(pkmn.Name);
                  Connections.CopyFile(fileName);
                  await Context.Channel.SendFileAsync(fileName, embed: BuildDexEmbed(pkmn, fileName));
                  Connections.DeleteFile(fileName);
               }
            }
         }
      }

      [Command("cp")]
      [Summary("Gets max CP values for a given Pokémon.")]
      [Remarks("Can search by Pokémon name or by number.")]
      public async Task CP([Summary("Get CPs for this Pokémon.")][Remainder] string pokemon)
      {
         if (!ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, Global.REGISTER_STRING_DEX))
         {
            await ResponseMessage.SendErrorMessage(Context, "cp", "This channel is not registered to process PokéDex commands.");
         }
         else
         {
            bool isNumber = int.TryParse(pokemon, out int pokemonNum);
            if (isNumber)
            {
               List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

               if (pokemonWithNumber.Count == 0)
               {
                  await ResponseMessage.SendErrorMessage(Context, "cp", $"Pokémon with number {pokemonNum} cannot be found.");
               }
               else if (pokemonWithNumber.Count > 1 && pokemonNum != Global.UNOWN_NUMBER && pokemonNum != Global.ARCEUS_NUMBER)
               {
                  string fileName = POKEDEX_SELECTION_IMAGE;
                  Connections.CopyFile(fileName);
                  RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(pokemonWithNumber, fileName));
                  Connections.DeleteFile(fileName);
                  for (int i = 0; i < pokemonWithNumber.Count; i++)
                  {
                     await dexMessage.AddReactionAsync(Global.SELECTION_EMOJIS[i]);
                  }
                  dexMessages.Add(dexMessage.Id, new Tuple<int, List<string>>((int)DEX_MESSAGE_TYPES.CP_MESSAGE, pokemonWithNumber));
               }
               else
               {
                  Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
                  Connections.CalcAllCP(ref pkmn);
                  string fileName = Connections.GetPokemonPicture(pkmn.Name);
                  Connections.CopyFile(fileName);
                  await Context.Channel.SendFileAsync(fileName, embed: BuildCPEmbed(pkmn, fileName));
                  Connections.DeleteFile(fileName);
               }
            }
            else
            {
               string name = GetPokemon(pokemon);
               Pokemon pkmn = Connections.Instance().GetPokemon(name);
               if (pkmn == null)
               {
                  List<string> pokemonNames = Connections.Instance().FuzzyNameSearch(name);

                  string fileName = POKEDEX_SELECTION_IMAGE;
                  Connections.CopyFile(fileName);
                  RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(pokemonNames, fileName));
                  await dexMessage.AddReactionsAsync(Global.SELECTION_EMOJIS);
                  Connections.DeleteFile(fileName);

                  dexMessages.Add(dexMessage.Id, new Tuple<int, List<string>>((int)DEX_MESSAGE_TYPES.CP_MESSAGE, pokemonNames));
               }
               else
               {
                  Connections.CalcAllCP(ref pkmn);
                  string fileName = Connections.GetPokemonPicture(pkmn.Name);
                  Connections.CopyFile(fileName);
                  await Context.Channel.SendFileAsync(fileName, embed: BuildCPEmbed(pkmn, fileName));
                  Connections.DeleteFile(fileName);
               }
            }
         }
      }

      [Command("form")]
      [Summary("Gets all forms for a given Pokémon.")]
      [Remarks("Leave blank to get all Pokémon with forms.\n" +
               "Send \"Alias\" to get variations for form names.")]
      public async Task Form([Summary("Pokémon with forms.")] string pokemon)
      {
         if (!ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, Global.REGISTER_STRING_DEX))
         {
            await ResponseMessage.SendErrorMessage(Context, "form", "This channel is not registered to process PokéDex commands.");
         }
         else
         {
            if (pokemon.Equals("Alias", StringComparison.OrdinalIgnoreCase))
            {
               EmbedBuilder embed = new EmbedBuilder();
               embed.WithTitle("Form tag variations");
               embed.AddField($"-alola", "-alolan", true);
               embed.AddField($"-galar", "-garlarian", true);
               embed.AddField($"-armor", "-armored", true);
               embed.AddField($"-fighting", "-fight", true);
               embed.AddField($"-flying", "-fly", true);
               embed.AddField($"-psychic", "-psy", true);
               embed.AddField($"-galar-zen", "-garlarian-zen", true);
               embed.AddField($"-autumn", "-fall", true);
               embed.AddField($"-megax", "-megay-x, -x", true);
               embed.AddField($"-megay", "-megay-y, -y", true);
               embed.WithColor(DexMessageColor);
               await ReplyAsync(embed: embed.Build());
            }
            else
            {
               bool isNumber = int.TryParse(pokemon, out int pokemonNum);
               if (isNumber)
               {
                  List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

                  if (pokemonWithNumber.Count == 0)
                  {
                     await ResponseMessage.SendErrorMessage(Context, "form", $"Pokémon with number {pokemonNum} cannot be found.");
                  }
                  if (pokemonWithNumber.Count == 1)
                  {
                     await ResponseMessage.SendErrorMessage(Context, "form", $"{pokemonWithNumber.First()} does not have different forms.");
                  }
                  else if (pokemonWithNumber.Count > 1)
                  {
                     EmbedBuilder embed = new EmbedBuilder();
                     StringBuilder sb = new StringBuilder();

                     foreach (string form in pokemonWithNumber)
                     {
                        sb.Append($"{form}\n");
                     }
                     embed.AddField($"Forms for pokemon with #{pokemon}", sb.ToString(), true);
                     embed.WithColor(DexMessageColor);
                     await ReplyAsync(embed: embed.Build());
                  }
               }
               else if (pokemonForms.ContainsKey(pokemon))
               {
                  EmbedBuilder embed = new EmbedBuilder();
                  StringBuilder sb = new StringBuilder();
                  Tuple<string, string> forms = pokemonForms[pokemon];
                  string[] formsList = forms.Item1.Split(',');

                  foreach (string form in formsList)
                  {
                     sb.Append(form);
                     if (form.Equals(forms.Item2))
                     {
                        sb.Append("*");
                     }
                     sb.Append('\n');
                  }
                  embed.AddField($"Forms for {pokemon}", sb.ToString(), true);
                  embed.WithColor(DexMessageColor);
                  embed.WithFooter("* Form is default form");
                  await ReplyAsync(embed: embed.Build());
               }
               else
               {
                  Pokemon pkmn = Connections.Instance().GetPokemon(GetPokemon(pokemon));
                  if (pkmn == null)
                  {
                     await ResponseMessage.SendErrorMessage(Context, "form", $"Pokémon with name {pokemon} cannot be found.");
                  }
                  else
                  {
                     await ResponseMessage.SendErrorMessage(Context, "form", $"{pkmn.Name} does not have different forms.");
                  }
               }
            }
         }
      }

      [Command("type")]
      [Summary("Gets information for a given Pokémon type.")]
      public async Task PokeType([Summary("(Optional) Typing to get info for.")] string type1 = null,
                                 [Summary("(Optional) Secondary typing to get info for.")] string type2 = null)
      {
         if (!ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, Global.REGISTER_STRING_DEX))
         {
            await ResponseMessage.SendErrorMessage(Context, "type", "This channel is not registered to process PokéDex commands.");
         }
         else if (type1 == null)
         {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{Global.NONA_EMOJIS["bug_emote"]} Bug");
            sb.AppendLine($"{Global.NONA_EMOJIS["dark_emote"]} Dark");
            sb.AppendLine($"{Global.NONA_EMOJIS["dragon_emote"]} Dragon");
            sb.AppendLine($"{Global.NONA_EMOJIS["electric_emote"]} Electric");
            sb.AppendLine($"{Global.NONA_EMOJIS["fairy_emote"]} Fairy");
            sb.AppendLine($"{Global.NONA_EMOJIS["fighting_emote"]} Fighting");
            sb.AppendLine($"{Global.NONA_EMOJIS["fire_emote"]} Fire");
            sb.AppendLine($"{Global.NONA_EMOJIS["flying_emote"]} Flying");
            sb.AppendLine($"{Global.NONA_EMOJIS["ghost_emote"]} Ghost");
            sb.AppendLine($"{Global.NONA_EMOJIS["grass_emote"]} Grass");
            sb.AppendLine($"{Global.NONA_EMOJIS["ground_emote"]} Ground");
            sb.AppendLine($"{Global.NONA_EMOJIS["ice_emote"]} Ice");
            sb.AppendLine($"{Global.NONA_EMOJIS["normal_emote"]} Normal");
            sb.AppendLine($"{Global.NONA_EMOJIS["poison_emote"]} Poison");
            sb.AppendLine($"{Global.NONA_EMOJIS["psychic_emote"]} Psychic");
            sb.AppendLine($"{Global.NONA_EMOJIS["rock_emote"]} Rock");
            sb.AppendLine($"{Global.NONA_EMOJIS["steel_emote"]} Steel");
            sb.AppendLine($"{Global.NONA_EMOJIS["water_emote"]} Water");

            EmbedBuilder embed = new EmbedBuilder();
            embed.AddField($"Pokémon Types:", sb.ToString());
            embed.WithColor(DexMessageColor);
            embed.WithFooter("Pokémon have 1 or 2 types. Moves always have 1 type.");
            await ReplyAsync(embed: embed.Build());
         }
         else
         {
            List<string> types = new List<string> { type1 };
            if (type2 != null && !type1.Equals(type2, StringComparison.OrdinalIgnoreCase))
            {
               types.Add(type2);
            }

            if (!CheckValidType(type1) || (types.Count == 2 && !CheckValidType(type2)))
            {
               await ResponseMessage.SendErrorMessage(Context, "type", $"{(!CheckValidType(type1) ? type1 : type2)} is not a valid type.");
            }
            else
            {
               string title = $"{type1}";
               if (types.Count == 2)
               {
                  title += $", {type2}";
               }

               string description = Global.NONA_EMOJIS[$"{type1}_emote"];
               if (types.Count == 2)
               {
                  description += Global.NONA_EMOJIS[$"{type2}_emote"];
               }

               Tuple<Dictionary<string, int>, Dictionary<string, int>> type1AttackRelations = (types.Count == 2) ? null : Connections.Instance().GetTypeAttackRelations(type1);
               Tuple<Dictionary<string, int>, Dictionary<string, int>> defenseRelations = Connections.Instance().GetTypeDefenseRelations(types);
               List<string> weather = Connections.Instance().GetWeather(types);

               EmbedBuilder embed = new EmbedBuilder();
               embed.WithTitle($"Type {title.ToUpper()}");
               embed.WithDescription(description);
               embed.AddField("Weather Boosts:", FormatWeatherList(weather), false);
               if (type1AttackRelations != null)
               {
                  embed.AddField($"Super Effective against:", FormatTypeList(type1AttackRelations.Item1), false);
                  embed.AddField($"Not Very Effective against:", FormatTypeList(type1AttackRelations.Item2), false);
               }
               embed.AddField($"Weaknesses:", FormatTypeList(defenseRelations.Item2), false);
               embed.AddField($"Resistances:", FormatTypeList(defenseRelations.Item1), false);
               embed.WithColor(DexMessageColor);
               await ReplyAsync(embed: embed.Build());
            }
         }
      }

      [Command("evo")]
      [Alias("evolution")]
      [Summary("Gets evolution family for a given Pokémon.")]
      public async Task Evolution([Summary("Get evolution family for this Pokémon.")][Remainder] string pokemon)
      {
         if (!ChannelRegisterCommands.IsRegisteredChannel(Context.Guild.Id, Context.Channel.Id, Global.REGISTER_STRING_DEX))
         {
            await ResponseMessage.SendErrorMessage(Context, "evo", "This channel is not registered to process PokéDex commands.");
         }
         else
         {
            string name = GetPokemon(pokemon);
            Pokemon pkmn = Connections.Instance().GetPokemon(name);
            if (pkmn == null)
            {
               List<string> pokemonNames = Connections.Instance().FuzzyNameSearch(name);

               string fileName = POKEDEX_SELECTION_IMAGE;
               Connections.CopyFile(fileName);
               RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(pokemonNames, fileName));
               await dexMessage.AddReactionsAsync(Global.SELECTION_EMOJIS);
               Connections.DeleteFile(fileName);

               dexMessages.Add(dexMessage.Id, new Tuple<int, List<string>>((int)DEX_MESSAGE_TYPES.EVO_MESSAGE, pokemonNames));
            }
            else
            {
               Dictionary<string, string> evolutions = GenerateEvoDict(pkmn.Name);
               string firstFileName = Connections.GetPokemonPicture(evolutions.First().Key);
               Connections.CopyFile(firstFileName);
               await Context.Channel.SendFileAsync(firstFileName, embed: BuildEvoEmbed(evolutions, pkmn.Name, firstFileName));
               Connections.DeleteFile(firstFileName);
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="message"></param>
      /// <param name="reaction"></param>
      /// <returns></returns>
      public static async Task DexMessageReactionHandle(IMessage message, SocketReaction reaction)
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
               else if (dexMessage.Item1 == (int)DEX_MESSAGE_TYPES.EVO_MESSAGE)
               {
                  Dictionary<string, string> evolutions = GenerateEvoDict(pokemon.Name);
                  string firstFileName = Connections.GetPokemonPicture(pokemon.Name);
                  Connections.CopyFile(firstFileName);
                  await reaction.Channel.SendFileAsync(firstFileName, embed: BuildEvoEmbed(evolutions, pokemon.Name, firstFileName));
                  Connections.DeleteFile(firstFileName);
               }
               Connections.DeleteFile(fileName);
               return;
            }
         }
         await message.RemoveReactionAsync(reaction.Emote, (SocketGuildUser)reaction.User);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="pokemon"></param>
      /// <param name="fileName"></param>
      /// <returns></returns>
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
      /// 
      /// </summary>
      /// <param name="pokemon"></param>
      /// <param name="fileName"></param>
      /// <returns></returns>
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
         embed.WithColor(DexMessageColor);
         embed.WithFooter("* denotes Weather Boosted CP");
         return embed.Build();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="evolutions"></param>
      /// <param name="initialPokemon"></param>
      /// <param name="fileName"></param>
      /// <returns></returns>
      private static Embed BuildEvoEmbed(Dictionary<string, string> evolutions, string initialPokemon, string fileName)
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

      public static Dictionary<string, string> GenerateEvoDict(string pokemon)
      {
         List<Evolution> initEvoFamily = Connections.Instance().GetEvolutionFamily(pokemon);

         if (initEvoFamily.Count == 0)
         {
            return new Dictionary<string, string>()
            {
               [pokemon] = ""
            };
         }

         List<Evolution> normalEvoFamily = NormalizeEvolutions(initEvoFamily);
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

         EvolutionNode tree = CreateEvolutionNode(basePokemon, normalEvoFamily);
         return EvolutionNodesToString(tree);
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
         {
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} {potentials[i]}");
         }

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(DexMessageColor);
         embed.WithTitle($"Pokemon Selection");
         embed.WithThumbnailUrl($"attachment://{selectPic}");
         embed.AddField("Do you mean...?", sb.ToString());
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

      /// <summary>
      /// Formats weather boosts as a string.
      /// </summary>
      /// <param name="weatherList">List of weather that boosts the type(s).</param>
      /// <returns>Weather for type(s) as a string.</returns>
      private static string FormatWeatherList(List<string> weatherList)
      {
         StringBuilder sb = new StringBuilder();
         foreach (string weather in weatherList)
         {
            sb.Append($"{Global.NONA_EMOJIS[$"{weather.Replace(' ', '_')}_emote"]} ");
         }
         return sb.ToString();
      }

      /// <summary>
      /// Formats type relations as a string.
      /// </summary>
      /// <param name="relations">Dictionary of type relations for the type(s).</param>
      /// <returns>Type relations for type(s) as a string.</returns>
      private static string FormatTypeList(Dictionary<string, int> relations)
      {
         if (relations.Count == 0)
         {
            return Global.EMPTY_FIELD;
         }

         string relationString = "";
         foreach (KeyValuePair<string, int> relation in relations)
         {
            double multiplier = TypeCalculator.CalcTypeEffectivness(relation.Value) * 100.0;
            string typeEmote = Global.NONA_EMOJIS[$"{relation.Key.ToUpper()}_EMOTE"];
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
         return Global.NONA_EMOJIS.ContainsKey($"{type}_emote");
      }

      private static List<Evolution> NormalizeEvolutions(List<Evolution> evolutions)
      {
         foreach (Evolution evo in evolutions)
         {
            foreach (Evolution evoComp in evolutions)
            {
               evo.Combine(evoComp);
            }
         }
         return evolutions.Where(x => x.Candy != Global.BAD_EVOLUTION).ToList();
      }

      private static EvolutionNode CreateEvolutionNode(string name, List<Evolution> evolutions)
      {
         string method = "";
         foreach (Evolution evo in evolutions)
         {
            if (name.Equals(evo.End, StringComparison.OrdinalIgnoreCase))
            {
               method = evo.EvolutionMethod();
            }
         }

         EvolutionNode node = new EvolutionNode
         {
            Name = name,
            Method = method
         };

         foreach (Evolution evo in evolutions)
         {
            if (name.Equals(evo.Start, StringComparison.OrdinalIgnoreCase))
            {
               node.Evolutions.Add(CreateEvolutionNode(evo.End, evolutions));
            }
         }
         return node;
      }

      private static Dictionary<string, string> EvolutionNodesToString(EvolutionNode node, string prevEvo = null)
      {
         Dictionary<string, string> evolutions = new Dictionary<string, string>();

         string evoString = prevEvo == null ? "Base Form" : $"Evolves from {prevEvo} with {node.Method}";

         evolutions.Add(node.Name, evoString);

         foreach (EvolutionNode evo in node.Evolutions)
         {
            evolutions = evolutions.Union(EvolutionNodesToString(evo, node.Name)).ToDictionary(x => x.Key, x => x.Value);
         }
         return evolutions;
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
}