using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.PreConditions;
using PokeStar.ConnectionInterface;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles system edit commands.
   /// </summary>
   public class SystemEditCommands : ModuleBase<SocketCommandContext>
   {
      private const int NumArguments = 2;
      private const int IndexPokemon = 0;
      private const int IndexMove = 1;

      private readonly List<string> EditableAttributes = new List<string>()
      {
         "SHINY",
         "SHADOW",
         "OBTAINABLE"
      };

      [Command("prefix")]
      [Summary("Sets the command prefix for this server.")]
      [Remarks("Prefix may only be a single character long.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      public async Task Prefix([Summary("Prefex to set for commands.")] char prefix)
      {
         Connections.Instance().UpdatePrefix(Context.Guild.Id, prefix.ToString());
         await ResponseMessage.SendInfoMessage(Context.Channel, $"Command prefix has been set to \'{prefix}\' for this server.");
      }

      [Command("updatePokemonNames")]
      [Summary("Updates the saved list of Pokémon names from the database.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      [NonaAdmin()]
      public async Task UpdatePokemonNames()
      {
         Connections.Instance().UpdatePokemonNameList();
         await ResponseMessage.SendInfoMessage(Context.Channel, $"Pokémon name list has been updated.");
      }

      [Command("updateMovenNames")]
      [Summary("Updates the saved list of Move names from the database.")]
      public async Task UpdateMoveNames()
      {
         Connections.Instance().UpdateMoveNameList();
         await ResponseMessage.SendInfoMessage(Context.Channel, $"Move name list has been updated.");
      }

      [Command("toggleUseEmptyRaid")]
      [Summary("Toggle empty raid feature for all servers.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      [NonaAdmin()]
      public async Task UseEmptyRaid()
      {
         Global.USE_EMPTY_RAID = !Global.USE_EMPTY_RAID;
         string text = Global.USE_EMPTY_RAID ? "" : "not";
         await ResponseMessage.SendInfoMessage(Context.Channel, $"Nona will {text} use the empty raid feature.");
      }

      [Command("toggleUseNonaTest")]
      [Summary("Toggle accepting messages from Nona Test Bot.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      [NonaAdmin()]
      public async Task UseNonaTest()
      {
         Global.USE_NONA_TEST = !Global.USE_NONA_TEST;
         string text = Global.USE_NONA_TEST ? "" : "not";
         await ResponseMessage.SendInfoMessage(Context.Channel, $"Nona will {text} accept message from a Nona Test Bot.");
      }

      [Command("updatePokemon")]
      [Summary("Edit an attribute of a Pokémon.")]
      [RequireUserPermission(GuildPermission.Administrator)]
      [NonaAdmin()]
      public async Task UpdatePokemon(string attribute, int value, [Remainder]string pokemon)
      {
         if(EditableAttributes.Contains(attribute.ToUpper()))
         {
            Pokemon pkmn = Connections.Instance().GetPokemon(pokemon);
            if (pkmn != null)
            {
               Connections.Instance().UpdatePokemon(pkmn.Name, attribute, value);
               await ResponseMessage.SendInfoMessage(Context.Channel, $"{attribute} has been set to {value} for {pkmn.Name}. Run .dex {pkmn.Name} to ensure value is set correctly.");
            }
            else
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemon", $"Pokémon {pokemon} does not exist.");
            }
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemon", $"{attribute} is not a valid attribute to change.");
         }
      }

      [Command("updatePokemonMove")]
      [Summary("Toggle accepting messages from Nona Test Bot")]
      [RequireUserPermission(GuildPermission.Administrator)]
      [NonaAdmin()]
      public async Task UpdatePokemonMove(int isLegacy, [Remainder] string pokemonMove)
      {
         int delimeterIndex = pokemonMove.IndexOf(Global.POKE_MOVE_DELIMITER);

         if (delimeterIndex != Global.NICKNAME_DELIMITER_MISSING)
         {
            string[] arr = pokemonMove.Split(Global.NICKNAME_DELIMITER);
            if (arr.Length == NumArguments)
            {
               string pokemonStr = arr[IndexPokemon].Trim();
               string moveStr = arr[IndexMove].Trim();
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonStr);
               Move move = Connections.Instance().GetMove(moveStr);
               if (pkmn == null)
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
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "updatePokemonMove", $"Delimeter {Global.POKE_MOVE_DELIMITER} not found.");
         }
      }
   }
}