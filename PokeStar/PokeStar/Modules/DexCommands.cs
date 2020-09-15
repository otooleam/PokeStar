using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Rest;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;
using PokeStar.PreConditions;
using PokeStar.ModuleParents;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles pokedex commands.
   /// </summary>
   public class DexCommands : DexCommandParent
   {
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

      [Command("dex")]
      [Alias("pokedex")]
      [Summary("Gets the PokéDex entry for a given Pokémon.")]
      [Remarks("Can search by Pokémon name or by number.")]
      [RegisterChannel('D')]
      public async Task Dex([Summary("Get information for this Pokémon.")][Remainder] string pokemon)
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
         else // Is string
         {
            string name = GetPokemonName(pokemon);
            Pokemon pkmn = Connections.Instance().GetPokemon(name);
            if (pkmn == null)
            {
               pkmn = Connections.Instance().GetPokemon(Connections.Instance().GetPokemonWithNickname(Context.Guild.Id, name));

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
            else
            {
               string fileName = Connections.GetPokemonPicture(pkmn.Name);
               Connections.CopyFile(fileName);
               await Context.Channel.SendFileAsync(fileName, embed: BuildDexEmbed(pkmn, fileName));
               Connections.DeleteFile(fileName);
            }
         }
      }

      [Command("cp")]
      [Summary("Gets max CP values for a given Pokémon.")]
      [Remarks("Can search by Pokémon name or by number.")]
      [RegisterChannel('D')]
      public async Task CP([Summary("Get CPs for this Pokémon.")][Remainder] string pokemon)
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
            string name = GetPokemonName(pokemon);
            Pokemon pkmn = Connections.Instance().GetPokemon(name);
            if (pkmn == null)
            {
               pkmn = Connections.Instance().GetPokemon(Connections.Instance().GetPokemonWithNickname(Context.Guild.Id, name));

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

      [Command("form")]
      [Summary("Gets all forms for a given Pokémon.")]
      [Remarks("Leave blank to get all Pokémon with forms.\n" +
               "Send \"Alias\" to get variations for form names.")]
      [RegisterChannel('D')]
      public async Task Form([Summary("Pokémon with forms.")] string pokemon)
      {
         if (pokemon == null)
         {
            List<string> keys = pokemonForms.Keys.ToList();
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= keys.Count; i++)
            {
               string bold = (i % 2 == 0) ? "" : "**";
               sb.Append($"{bold}{keys.ElementAt(i - 1)}{bold} ");
               if (i % 4 == 0)
               {
                  sb.Append("\n");
               }
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.AddField($"Pokémon with form differences:", sb.ToString());
            embed.WithColor(DexMessageColor);
            await ReplyAsync(embed: embed.Build());
         }
         else if (pokemon.Equals("Alias", StringComparison.OrdinalIgnoreCase))
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
               Pokemon pkmn = Connections.Instance().GetPokemon(GetPokemonName(pokemon));
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

      [Command("evo")]
      [Alias("evolution")]
      [Summary("Gets evolution family for a given Pokémon.")]
      [RegisterChannel('D')]
      public async Task Evolution([Summary("Get evolution family for this Pokémon.")][Remainder] string pokemon)
      {
         string name = GetPokemonName(pokemon);
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
}