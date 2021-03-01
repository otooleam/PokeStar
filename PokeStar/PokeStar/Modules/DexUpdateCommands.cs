using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.ModuleParents;
using PokeStar.PreConditions;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   class DexUpdateCommands : DexCommandParent
   {
      /// <summary>
      /// Valid Pokémon editable attributes.
      /// </summary>
      private readonly List<string> EditableAttributes = new List<string>()
      {
         "SHINY",
         "SHADOW",
         "OBTAINABLE"
      };

      /// <summary>
      /// Handle updatePokemon command.
      /// </summary>
      /// <param name="attribute">Update this attribute.</param>
      /// <param name="value">Update the attribute with this value.</param>
      /// <param name="pokemon">Update attribute of this Pokémon.</param>
      /// <returns>Completed Task.</returns>
      [Command("updatePokemon")]
      [Summary("Edit an attribute of a Pokémon.")]
      [Remarks("Valid attributes to edit are shiny, shadow, and obtainable." +
               "Value can only be set to either 1(true) or 0(false)")]
      [RequireUserPermission(GuildPermission.Administrator)]
      [NonaAdmin()]
      public async Task UpdatePokemon([Summary("Update this attribute.")] string attribute,
                                      [Summary("Update the attribute with this value.")] int value,
                                      [Summary("Update attribute of this Pokémon.")][Remainder] string pokemon)
      {
         if (!EditableAttributes.Contains(attribute.ToUpper()))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemon", $"{attribute} is not a valid attribute to change.");
         }
         else
         {
            Pokemon pkmn = Connections.Instance().GetPokemon(pokemon);
            if (pkmn == null || pkmn.Name.Equals(Global.DUMMY_POKE_NAME, StringComparison.OrdinalIgnoreCase))
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemon", $"Pokémon {pokemon} does not exist.");
            }
            else
            {
               Connections.Instance().UpdatePokemon(pkmn.Name, attribute, value);
               await ResponseMessage.SendInfoMessage(Context.Channel, $"{attribute} has been set to {value} for {pkmn.Name}. Run .dex {pkmn.Name} to ensure value is set correctly.");
            }
         }
      }

      /// <summary>
      /// Handle updatePokemonMove command.
      /// </summary>
      /// <param name="isLegacy">Is the move a legacy move.</param>
      /// <param name="pokemonMove">Add a move to a Pokémon using this string.</param>
      /// <returns>Completed Task.</returns>
      [Command("updatePokemonMove")]
      [Summary("Add a move to a Pokémon.")]
      [Remarks("IsLegacy can only be set to either 1(true) or 0(false)" +
               "To add a move a special character (>) is used.\n" +
               "\nFormat pokemonMove as following:\n" +
               "Pokémon name > Move name")]
      [RequireUserPermission(GuildPermission.Administrator)]
      [NonaAdmin()]
      public async Task UpdatePokemonMove([Summary("Is the move a legacy move.")] int isLegacy,
                                          [Summary("Add a move to a Pokémon using this string.")][Remainder] string pokemonMove)
      {
         int delimeterIndex = pokemonMove.IndexOf(Global.PARSE_DELIMITER);

         if (delimeterIndex == Global.DELIMITER_MISSING)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemonMove", $"Delimeter {Global.PARSE_DELIMITER} not found.");
         }
         else
         {
            string[] arr = pokemonMove.Split(Global.PARSE_DELIMITER);
            if (arr.Length == Global.NUM_PARSE_ARGS)
            {
               string pokemonStr = arr[Global.NEW_PARSE_VALUE].Trim();
               string moveStr = arr[Global.OLD_PARSE_VALUE].Trim();
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonStr);
               Move move = Connections.Instance().GetMove(moveStr);
               if (pkmn == null || pkmn.Name.Equals(Global.DUMMY_POKE_NAME, StringComparison.OrdinalIgnoreCase))
               {
                  await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemonMove", $"Pokémon {pokemonStr} does not exist.");
               }
               else if (move == null)
               {
                  await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemonMove", $"Move {moveStr} does not exist.");
               }
               else
               {
                  Connections.Instance().UpdatePokemonMove(pkmn.Name, move.Name, isLegacy);
                  await ResponseMessage.SendInfoMessage(Context.Channel, $"{pkmn.Name} now has the move {move.Name}. Run .dex {pkmn.Name} to ensure value is set correctly.");
               }
            }
            else
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemonMove", $"Too many delimiters found.");
            }
         }
      }
   }
}
