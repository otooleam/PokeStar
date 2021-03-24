using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.Calculators;
using PokeStar.ModuleParents;
using PokeStar.PreConditions;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles dex update commands.
   /// </summary>
   public class DexUpdateCommands : DexCommandParent
   {
      /// <summary>
      /// Valid Pokémon editable attributes.
      /// </summary>
      private readonly Dictionary<string, string> EditableAttributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
      {
         ["SHINY"] = "IsShiny",
         ["SHADOW"] = "IsShadow",
         ["OBTAINABLE"] = "IsReleased"
      };

      /// <summary>
      /// Counter timer in seconds.
      /// </summary>
      private readonly int COUNTER_UPDATE_TIMER = 3;

      /// <summary>
      /// Pokémon to display per update message.
      /// </summary>
      private readonly int COUNTER_DISPLAY_COUNT = 25;

      /// <summary>
      /// Is a counter update currently running.
      /// </summary>
      private static bool runningCounterSim = false;

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
         if (!EditableAttributes.ContainsKey(attribute.ToUpper()))
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemon", $"{attribute} is not a valid attribute to change.");
         }
         else
         {
            string name = GetPokemonName(pokemon);
            Pokemon pkmn = Connections.Instance().GetPokemon(name);
            if (pkmn == null || pkmn.Name.Equals(Global.DUMMY_POKE_NAME, StringComparison.OrdinalIgnoreCase))
            {
               pkmn = Connections.Instance().GetPokemon(Connections.Instance().GetPokemonWithNickname(Context.Guild.Id, name));

               if (pkmn == null || pkmn.Name.Equals(Global.DUMMY_POKE_NAME, StringComparison.OrdinalIgnoreCase))
               {
                  await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemon", $"Pokémon {pokemon} does not exist.");
               }
               else
               {
                  Connections.Instance().UpdatePokemon(pkmn.Name, EditableAttributes[attribute], value);
                  await ResponseMessage.SendInfoMessage(Context.Channel, $"{attribute} has been set to {value} for {pkmn.Name}. Run .dex {pkmn.Name} to ensure value is set correctly.");
               }
            }
            else
            {
               Connections.Instance().UpdatePokemon(pkmn.Name, EditableAttributes[attribute], value);
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
               "If the Pokémon already has the move, " +
               "the legacy status will be updated.\n" +
               "Format pokemonMove as following:\n" +
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

               string name = GetPokemonName(pokemonStr);
               Pokemon pkmn = Connections.Instance().GetPokemon(name);
               if (pkmn == null || pkmn.Name.Equals(Global.DUMMY_POKE_NAME, StringComparison.OrdinalIgnoreCase))
               {
                  pkmn = Connections.Instance().GetPokemon(Connections.Instance().GetPokemonWithNickname(Context.Guild.Id, name));
               }
               Move move = Connections.Instance().GetMove(moveStr);
               if (pkmn == null || pkmn.Name.Equals(Global.DUMMY_POKE_NAME, StringComparison.OrdinalIgnoreCase))
               {
                  await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemonMove", $"Pokémon {pokemonStr} does not exist.");
               }
               else if (move == null)
               {
                  await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemonMove", $"Move {moveStr} does not exist.");
               }
               else if (move.Name.Equals(Global.SHADOW_MOVES.ElementAt(Global.SHADOW_INDEX).Name, StringComparison.OrdinalIgnoreCase) ||
                        move.Name.Equals(Global.SHADOW_MOVES.ElementAt(Global.PURIFIED_INDEX).Name, StringComparison.OrdinalIgnoreCase))
               {
                  await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemonMove", $"Cannot assign {move.Name} to {pkmn.Name}.");
               }
               else
               {
                  bool moveAssigned = Connections.Instance().PokemonHasMove(pkmn.Name, move.Name);

                  Connections.Instance().UpdatePokemonMove(pkmn.Name, move.Name, isLegacy, moveAssigned);

                  string message = moveAssigned ? $"Legacy status for {move.Name} on {pkmn.Name}. Run .dex {pkmn.Name} to ensure value is set correctly." : $"{pkmn.Name} now has the move {move.Name}. Run .dex {pkmn.Name} to ensure value is set correctly.";

                  await ResponseMessage.SendInfoMessage(Context.Channel, message);
               }
            }
            else
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemonMove", $"Too many delimiters found.");
            }
         }
      }


      /// <summary>
      /// Handle updateCounters command.
      /// </summary>
      /// <returns>Completed Task.</returns>
      [Command("updateCounters")]
      [Alias("updateCounter", "updatePokemonCounters", "updatePokemonCounter")]
      [Summary("Update counters for Pokémon as necessary.")]
      [Remarks("This command might take some time to run.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      [NonaAdmin()]
      public async Task UpdateCounters()
      {
         if (!runningCounterSim)
         {
            runningCounterSim = true;

            DateTime start = DateTime.Now;
            DateTime timer = new DateTime(start.Ticks);

            IUserMessage message = await ResponseMessage.SendInfoMessage(Context.Channel, "Starting Pokémon counter recalculations...");

            List<Pokemon> allPokemon = Connections.Instance().GetPokemonForSim();
            List<string> updates = new List<string>();

            int count = 0;
            foreach (Pokemon pokemon in allPokemon)
            {
               CounterCalcResults result = CounterCalculator.RunSim(pokemon, allPokemon);

               Pokemon currentCounters = new Pokemon
               {
                  Name = pokemon.Name
               };
               Connections.Instance().GetPokemonCounter(ref currentCounters);

               if (CheckChange(result.Regular, currentCounters.Counter.Take(currentCounters.Counter.Count / 2).ToList()) ||
                   CheckChange(result.Special, currentCounters.Counter.Skip(currentCounters.Counter.Count / 2).ToList()))
               {
                  Connections.Instance().UpdateCounters(pokemon.Name, result.Regular, result.Special);
                  updates.Add(pokemon.Name);
               }
               count++;
               if ((DateTime.Now - timer).TotalSeconds >= COUNTER_UPDATE_TIMER)
               {
                  timer = DateTime.Now;
                  await ResponseMessage.ModifyInfoMessage(message, $"Completed {count} / {allPokemon.Count} Pokémon counter recalculations...");
               }
            }

            double time = Math.Round((DateTime.Now - start).TotalMinutes, 2);

            bool sendInitMessage = true;
            string initString = $"Counters successfully updated.\nTime elapsed: {time} Minutes.\n";
            count = 0;
            updates.Add(string.Empty);
            StringBuilder sb = new StringBuilder();
            foreach (string name in updates)
            {
               sb.AppendLine(name);
               count++;

               if (count % COUNTER_DISPLAY_COUNT == 0 || count == updates.Count)
               {
                  if (sendInitMessage)
                  {
                     await ResponseMessage.ModifyInfoMessage(message, initString + (updates.Count == 1 ? "No updates were necessary." : $"The following have been updated ({updates.Count - 1}):\n{sb}"));
                     sendInitMessage = false;
                  }
                  else
                  {
                     await ResponseMessage.SendInfoMessage(Context.Channel, sb.ToString());
                  }
                  sb.Clear();
               }
            }
            runningCounterSim = false;
         }
         else
         {
            await ResponseMessage.SendWarningMessage(Context.Channel, "updateCounters", $"Counters are already being recalculated.");
         }
      }

      /// <summary>
      /// Checks if any of the counters have changed.
      /// </summary>
      /// <param name="newCounters">List of potential new counters.</param>
      /// <param name="oldCounters">List of current counters.</param>
      /// <returns>True if any counter is different or if there is a length error, otherwise false.</returns>
      private static bool CheckChange(List<Counter> newCounters, List<Counter> oldCounters)
      {
         if (newCounters.Count == 0 || oldCounters.Count == 0 || oldCounters.Count != newCounters.Count)
         {
            return true;
         }

         for (int i = 0; i < oldCounters.Count; i++)
         {
            Counter counter = oldCounters.ElementAt(i);

            int index = counter.Name.IndexOf(' ');
            string name = counter.Name;
            if (index != -1 && counter.Name.Substring(0, index).Equals(Global.SHADOW_TAG, StringComparison.OrdinalIgnoreCase))
            {
               name = counter.Name.Substring(index);
            }
            counter.Name = name.Trim();

            if (!newCounters.ElementAt(i).Equals(oldCounters.ElementAt(i)))
            {
               return true;
            }
         }

         return false;
      }
   }
}