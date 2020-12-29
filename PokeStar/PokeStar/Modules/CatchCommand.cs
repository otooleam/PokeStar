using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Rest;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.ModuleParents;
using PokeStar.PreConditions;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   public class CatchCommand : DexCommandParent
   {
      [Command("catch")]
      [Summary("Simulates catching a Pokémon.")]
      [RegisterChannel('I')]
      public async Task Catch([Summary("Simulate catching this Pokémon.")][Remainder] string pokemon)
      {
         bool isNumber = int.TryParse(pokemon, out int pokemonNum);
         if (isNumber)
         {
            List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

            if (pokemonWithNumber.Count == 0)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "catch", $"Pokémon with number {pokemonNum} cannot be found.");
            }
            else if (pokemonWithNumber.Count > 1 && pokemonNum != Global.UNOWN_NUMBER && pokemonNum != Global.ARCEUS_NUMBER)
            {
               string fileName = POKEDEX_SELECTION_IMAGE;
               Connections.CopyFile(fileName);
               RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(pokemonWithNumber, fileName));
               dexMessages.Add(dexMessage.Id, new DexSelectionMessage((int)DEX_MESSAGE_TYPES.CATCH_MESSAGE, pokemonWithNumber));
               Connections.DeleteFile(fileName);
               dexMessage.AddReactionsAsync(Global.SELECTION_EMOJIS.Take(pokemonWithNumber.Count).ToArray());
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
               CatchSimulation catchSim = new CatchSimulation(pkmn);
               string fileName = Connections.GetPokemonPicture(pkmn.Name);
               Connections.CopyFile(fileName);
               RestUserMessage catchMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildCatchEmbed(catchSim, fileName));
               catchMessages.Add(catchMessage.Id, catchSim);
               Connections.DeleteFile(fileName);
               catchMessage.AddReactionsAsync(catchEmojis);
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
                  List<string> pokemonNames = Connections.Instance().SearchPokemon(name);

                  string fileName = POKEDEX_SELECTION_IMAGE;
                  Connections.CopyFile(fileName);
                  RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(pokemonNames, fileName));
                  dexMessages.Add(dexMessage.Id, new DexSelectionMessage((int)DEX_MESSAGE_TYPES.CATCH_MESSAGE, pokemonNames));
                  Connections.DeleteFile(fileName);
                  dexMessage.AddReactionsAsync(Global.SELECTION_EMOJIS);
               }
               else
               {
                  CatchSimulation catchSim = new CatchSimulation(pkmn);
                  string fileName = Connections.GetPokemonPicture(pkmn.Name);
                  Connections.CopyFile(fileName);
                  RestUserMessage catchMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildCatchEmbed(catchSim, fileName));
                  catchMessages.Add(catchMessage.Id, catchSim);
                  Connections.DeleteFile(fileName);
                  catchMessage.AddReactionsAsync(catchEmojis);
               }
            }
            else
            {
               CatchSimulation catchSim = new CatchSimulation(pkmn);
               string fileName = Connections.GetPokemonPicture(pkmn.Name);
               Connections.CopyFile(fileName);
               RestUserMessage catchMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildCatchEmbed(catchSim, fileName));
               catchMessages.Add(catchMessage.Id, catchSim);
               Connections.DeleteFile(fileName);
               catchMessage.AddReactionsAsync(catchEmojis);
            }
         }
      }
   }
}
