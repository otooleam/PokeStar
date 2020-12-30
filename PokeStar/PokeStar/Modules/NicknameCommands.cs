using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.Commands;
using PokeStar.ConnectionInterface;
using PokeStar.DataModels;
using PokeStar.ModuleParents;
using PokeStar.PreConditions;

namespace PokeStar.Modules
{
   public class NicknameCommands : DexCommandParent
   {
      private const int NumArguments = 2;
      private const int IndexNickname = 0;
      private const int IndexName = 1;

      [Command("nickname")]
      [Summary("Edit Pokémon nicknames.")]
      [Remarks("This command is used for adding, updating, and removing nicknames.\n" +
               "To add or update a nickname a special character (>) is used.\n" +
               "\nFor each option format the nicknameString as following:\n" +
               "Add Nickname..............nickname > Pokémon name\n" +
               "Update Nickname........new nickname > old nickname\n" +
               "Delete Nickname.........> nickname\n" +
               "\nNote: Spaces are allowed for nicknames")]
      [RegisterChannel('D')]
      public async Task Nickname([Summary("Update the nickname of a Pokémon using this string.")][Remainder] string nicknameString)
      {
         ulong guild = Context.Guild.Id;
         int delimeterIndex = nicknameString.IndexOf(Global.NICKNAME_DELIMITER);

         if (delimeterIndex == Global.NICKNAME_DELIMITER_MISSING)
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "nickname", $"No nicknam delimiter (>) found.");
         }
         else
         {
            string[] arr = nicknameString.Split(Global.NICKNAME_DELIMITER);
            if (arr.Length == NumArguments)
            {
               string newNickname = arr[IndexNickname].Trim();
               string other = arr[IndexName].Trim();

               if (string.IsNullOrEmpty(newNickname))
               {
                  string name = Connections.Instance().GetPokemonWithNickname(guild, newNickname);
                  if (name == null)
                  {
                     await ResponseMessage.SendErrorMessage(Context.Channel, "nickname", $"The nickname {newNickname} is not registered with a Pokémon.");
                  }
                  else
                  {
                     Connections.Instance().DeleteNickname(guild, newNickname);
                     await ResponseMessage.SendInfoMessage(Context.Channel, $"Removed {newNickname} from {name}.");
                  }
               }
               else
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(GetPokemonName(other));

                  if (pokemon == null)
                  {
                     if (Connections.Instance().GetPokemonWithNickname(guild, other) == null)
                     {
                        await ResponseMessage.SendErrorMessage(Context.Channel, "nickname", $"{other} is not a registered nickname.");
                     }
                     else
                     {
                        Connections.Instance().UpdateNickname(guild, other, newNickname);
                        string pkmn = Connections.Instance().GetPokemonWithNickname(guild, other);
                        await ResponseMessage.SendInfoMessage(Context.Channel, $"{newNickname} has replaced {other} as a valid nickname for {pkmn}.");
                     }
                  }
                  else
                  {
                     Connections.Instance().AddNickname(guild, newNickname, pokemon.Name);
                     await ResponseMessage.SendInfoMessage(Context.Channel, $"{newNickname} is now a valid nickname for {pokemon.Name}.");
                  }
               }
            }
            else
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "nickname", $"Too many delimiters (>) found.");
            }
         }
      }

      [Command("getNickname")]
      [Alias("getNicknames")]
      [Summary("Gets nicknames for a given Pokémon.")]
      [Remarks("Can search by Pokémon name, nickname, or number.")]
      [RegisterChannel('D')]
      public async Task GetNickname([Summary("Get the nicknames for this Pokémon.")][Remainder] string pokemon)
      {
         ulong guild = Context.Guild.Id;
         bool isNumber = int.TryParse(pokemon, out int pokemonNum);
         if (isNumber)
         {
            List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

            if (pokemonWithNumber.Count == 0)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "getNickname", $"Pokémon with number {pokemonNum} cannot be found.");
            }
            else if (pokemonNum == Global.ARCEUS_NUMBER)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "dex", $"Arceus #{pokemonNum} has too many forms to display, please search by name.");
            }
            else if (pokemonWithNumber.Count > 1 && pokemonNum != Global.UNOWN_NUMBER)
            {
               await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.NICKNAME_MESSAGE, pokemonWithNumber, Context.Channel);
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
               pkmn.Nicknames = Connections.Instance().GetNicknames(guild, pkmn.Name);
               await SendDexMessage(pkmn, BuildNicknameEmbed, Context.Channel, true);
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
                  await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.NICKNAME_MESSAGE, Connections.Instance().SearchPokemon(name), Context.Channel);
               }
               else
               {
                  pkmn.Nicknames = Connections.Instance().GetNicknames(guild, pkmn.Name);
                  await SendDexMessage(pkmn, BuildNicknameEmbed, Context.Channel, true);
               }
            }
            else
            {
               pkmn.Nicknames = Connections.Instance().GetNicknames(guild, pkmn.Name);
               await SendDexMessage(pkmn, BuildNicknameEmbed, Context.Channel, true);
            }
         }
      }
   }
}