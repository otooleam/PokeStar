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
               await ResponseMessage.SendErrorMessage(Context.Channel, "dex", $"Pokémon with number {pokemonNum} cannot be found.");
            }
            else if (pokemonNum == Global.ARCEUS_NUMBER)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "dex", $"Arceus #{pokemonNum} has too many forms to display, please search by name.");
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
               await ResponseMessage.SendErrorMessage(Context.Channel, "cp", $"Pokémon with number {pokemonNum} cannot be found.");
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
      public async Task Form([Summary("(Optional) Get forms for this Pokémon.")][Remainder] string pokemon = null)
      {
         if (pokemon == null)
         {
            List<string> keys = pokemonForms.Keys.ToList();
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= keys.Count; i++)
            {
               string format = (i % 2 == 0) ? "" : "**";
               sb.Append($"{format}{keys.ElementAt(i - 1)}{format} ");
               if (i % 4 == 0)
               {
                  sb.Append('\n');
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
                  await ResponseMessage.SendErrorMessage(Context.Channel, "form", $"Pokémon with number {pokemonNum} cannot be found.");
               }
               if (pokemonWithNumber.Count == 1)
               {
                  await ResponseMessage.SendErrorMessage(Context.Channel, "form", $"{pokemonWithNumber.First()} does not have different forms.");
               }
               else if (pokemonWithNumber.Count > 1)
               {
                  string baseName = pokemonWithNumber.Where(form => pokemonForms.ContainsKey(form)).ToList().First();

                  Tuple<string, string> forms = pokemonForms[baseName];
                  List<string> formsList = forms.Item1.Split(',').ToList();

                  string fileName = Connections.GetPokemonPicture(baseName);
                  Connections.CopyFile(fileName);
                  await Context.Channel.SendFileAsync(fileName, embed: BuildFormEmbed(baseName, formsList, forms.Item2, fileName));
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

                     dexMessages.Add(dexMessage.Id, new Tuple<int, List<string>>((int)DEX_MESSAGE_TYPES.FORM_MESSAGE, pokemonNames));
                  }
                  else
                  {
                     List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pkmn.Number);

                     if (pokemonWithNumber.Count == 1)
                     {
                        await ResponseMessage.SendErrorMessage(Context.Channel, "form", $"{pokemonWithNumber.First()} does not have different forms.");
                     }
                     else if (pokemonWithNumber.Count > 1)
                     {
                        string baseName = pokemonWithNumber.Where(form => pokemonForms.ContainsKey(form)).ToList().First();

                        Tuple<string, string> forms = pokemonForms[baseName];
                        List<string> formsList = forms.Item1.Split(',').ToList();

                        string fileName = Connections.GetPokemonPicture(baseName);
                        Connections.CopyFile(fileName);
                        await Context.Channel.SendFileAsync(fileName,embed: BuildFormEmbed(baseName, formsList, forms.Item2, fileName));
                        Connections.DeleteFile(fileName);
                     }
                  }
               }
               else
               {
                  List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pkmn.Number);

                  if (pokemonWithNumber.Count == 1)
                  {
                     await ResponseMessage.SendErrorMessage(Context.Channel, "form", $"{pokemonWithNumber.First()} does not have different forms.");
                  }
                  else if (pokemonWithNumber.Count > 1)
                  {
                     string baseName = pokemonWithNumber.Where(form => pokemonForms.ContainsKey(form)).ToList().First();

                     Tuple<string, string> forms = pokemonForms[baseName];
                     List<string> formsList = forms.Item1.Split(',').ToList();

                     string fileName = Connections.GetPokemonPicture(baseName);
                     Connections.CopyFile(fileName);
                     await Context.Channel.SendFileAsync(fileName, embed: BuildFormEmbed(baseName, formsList, forms.Item2, fileName));
                     Connections.DeleteFile(fileName);
                  }
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
         bool isNumber = int.TryParse(pokemon, out int pokemonNum);
         if (isNumber)
         {
            List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

            if (pokemonWithNumber.Count == 0)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "evo", $"Pokémon with number {pokemonNum} cannot be found.");
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
               dexMessages.Add(dexMessage.Id, new Tuple<int, List<string>>((int)DEX_MESSAGE_TYPES.EVO_MESSAGE, pokemonWithNumber));
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());

               Dictionary<string, string> evolutions = GenerateEvoDict(pkmn.Name);
               string fileName = Connections.GetPokemonPicture(evolutions.First().Key);
               Connections.CopyFile(fileName);
               await Context.Channel.SendFileAsync(fileName, embed: BuildEvoEmbed(evolutions, pkmn.Name, fileName));
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

                  dexMessages.Add(dexMessage.Id, new Tuple<int, List<string>>((int)DEX_MESSAGE_TYPES.EVO_MESSAGE, pokemonNames));
               }
               else
               {
                  Dictionary<string, string> evolutions = GenerateEvoDict(pkmn.Name);
                  string fileName = Connections.GetPokemonPicture(evolutions.First().Key);
                  Connections.CopyFile(fileName);
                  await Context.Channel.SendFileAsync(fileName, embed: BuildEvoEmbed(evolutions, pkmn.Name, fileName));
                  Connections.DeleteFile(fileName);
               }
            }
            else
            {
               Dictionary<string, string> evolutions = GenerateEvoDict(pkmn.Name);
               string fileName = Connections.GetPokemonPicture(evolutions.First().Key);
               Connections.CopyFile(fileName);
               await Context.Channel.SendFileAsync(fileName, embed: BuildEvoEmbed(evolutions, pkmn.Name, fileName));
               Connections.DeleteFile(fileName);
            }
         }
      }
   }
}