﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Rest;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.PreConditions;
using PokeStar.ModuleParents;
using PokeStar.ConnectionInterface;

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
      [Remarks("Can search by Pokémon name or by number." +
               "Tags can be used to search for specific forms.")]
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
               await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.DEX_MESSAGE, pokemonWithNumber, Context.Channel);
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
               Connections.Instance().GetPokemonStats(ref pkmn);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildDexEmbed, Context.Channel, true);
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
                  await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.DEX_MESSAGE, Connections.Instance().SearchPokemon(name), Context.Channel);
               }
               else
               {
                  Connections.Instance().GetPokemonStats(ref pkmn);
                  pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE] = true;
                  await SendDexMessage(pkmn, BuildDexEmbed, Context.Channel, true);
               }
            }
            else
            {
               Connections.Instance().GetPokemonStats(ref pkmn);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildDexEmbed, Context.Channel, true);
            }
         }
      }

      [Command("cp")]
      [Summary("Gets max CP values for a given Pokémon.")]
      [Remarks("Can search by Pokémon name or by number." +
               "Tags can be used to search for specific forms.")]
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
               await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.CP_MESSAGE, pokemonWithNumber, Context.Channel);
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
               Connections.GetPokemonCP(ref pkmn);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.CP_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildCPEmbed, Context.Channel, true);
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
                  await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.DEX_MESSAGE, Connections.Instance().SearchPokemon(name), Context.Channel);
               }
               else
               {
                  Connections.GetPokemonCP(ref pkmn);
                  pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.CP_MESSAGE] = true;
                  await SendDexMessage(pkmn, BuildCPEmbed, Context.Channel, true);
               }
            }
            else
            {
               Connections.GetPokemonCP(ref pkmn);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.CP_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildCPEmbed, Context.Channel, true);
            }
         }
      }

      [Command("pvp")]
      [Alias("pvpiv")]
      [Summary("Gets rank 1 PvP IV values for a given Pokémon.")]
      [Remarks("Can search by Pokémon name or by number." +
               "Tags can be used to search for specific forms.")]
      [RegisterChannel('D')]
      public async Task PvP([Summary("Get PvP IVs for this Pokémon.")][Remainder] string pokemon)
      {
         bool isNumber = int.TryParse(pokemon, out int pokemonNum);
         if (isNumber)
         {
            List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemonNum);

            if (pokemonWithNumber.Count == 0)
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "pvp", $"Pokémon with number {pokemonNum} cannot be found.");
            }
            else if (pokemonWithNumber.Count > 1 && pokemonNum != Global.UNOWN_NUMBER && pokemonNum != Global.ARCEUS_NUMBER)
            {
               await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.PVP_MESSAGE, pokemonWithNumber, Context.Channel);
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
               Connections.Instance().GetPokemonPvP(ref pkmn);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildPvPEmbed, Context.Channel, true);
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
                  await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.PVP_MESSAGE, Connections.Instance().SearchPokemon(name), Context.Channel);
               }
               else
               {
                  Connections.Instance().GetPokemonPvP(ref pkmn);
                  pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE] = true;
                  await SendDexMessage(pkmn, BuildPvPEmbed, Context.Channel, true);
               }
            }
            else
            {
               Connections.Instance().GetPokemonPvP(ref pkmn);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildPvPEmbed, Context.Channel, true);
            }
         }
      }

      [Command("form")]
      [Summary("Gets all forms for a given Pokémon.")]
      [Remarks("Leave blank to get a list of all Pokémon with forms.\n" +
               "Send \"Alias\" to get variations for form names.")]
      [RegisterChannel('D')]
      public async Task Form([Summary("(Optional) Pokémon with forms.")][Remainder] string pokemon = null)
      {
         if (pokemon == null)
         {
            List<string> baseForms = Connections.Instance().GetBaseForms();
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= baseForms.Count; i++)
            {
               string format = (i % 2 == 0) ? "" : "**";
               sb.Append($"{format}{baseForms.ElementAt(i - 1)}{format} ");
               if (i % 4 == 0)
               {
                  sb.Append('\n');
               }
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.AddField($"Pokémon with form differences:", sb.ToString());
            embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
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
            embed.AddField($"-megax", "-mega-x, -x", true);
            embed.AddField($"-megay", "-mega-y, -y", true);
            embed.AddField($"-male", "-m", true);
            embed.AddField($"-female", "-f", true);
            embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
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
               else
               {
                  Pokemon pkmn = new Pokemon();
                  if (pokemonWithNumber.Count == 1)
                  {
                     pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
                     pkmn.Forms = new Form();
                  }
                  else if (pokemonWithNumber.Count > 1)
                  {
                     string baseName = Connections.Instance().GetBaseForms().Intersect(pokemonWithNumber).First();
                     pkmn = Connections.Instance().GetPokemon(baseName);
                     pkmn.Forms = Connections.Instance().GetFormTags(baseName);
                  }
                  pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE] = true;
                  await SendDexMessage(pkmn, BuildFormEmbed, Context.Channel, true);
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
                     await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.FORM_MESSAGE, Connections.Instance().SearchPokemon(name), Context.Channel);
                  }
                  else
                  {
                     List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pkmn.Number);

                     if (pokemonWithNumber.Count == 1)
                     {
                        pkmn.Forms = new Form();
                     }
                     else if (pokemonWithNumber.Count > 1)
                     {
                        string baseName = Connections.Instance().GetBaseForms().Intersect(pokemonWithNumber).First();
                        pkmn.Forms = Connections.Instance().GetFormTags(baseName);
                     }
                     pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE] = true;
                     await SendDexMessage(pkmn, BuildFormEmbed, Context.Channel, true);
                  }
               }
               else
               {
                  List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pkmn.Number);

                  if (pokemonWithNumber.Count == 1)
                  {
                     pkmn.Forms = new Form();
                  }
                  else if (pokemonWithNumber.Count > 1)
                  {
                     string baseName = Connections.Instance().GetBaseForms().Intersect(pokemonWithNumber).First();
                     pkmn.Forms = Connections.Instance().GetFormTags(baseName);
                  }
                  pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE] = true;
                  await SendDexMessage(pkmn, BuildFormEmbed, Context.Channel, true);
               }
            }
         }
      }

      [Command("evo")]
      [Alias("evolution", "evolve")]
      [Summary("Gets the evolution family for a given Pokémon.")]
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
               await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.EVO_MESSAGE, pokemonWithNumber, Context.Channel);
            }
            else
            {
               Pokemon pkmn = Connections.Instance().GetPokemon(pokemonWithNumber.First());
               pkmn.Evolutions = GenerateEvoDict(pkmn.Name);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildEvoEmbed, Context.Channel, true);
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
                  await SendDexSelectionMessage((int)DEX_MESSAGE_TYPES.EVO_MESSAGE, Connections.Instance().SearchPokemon(name), Context.Channel);
               }
               else
               {
                  pkmn.Evolutions = GenerateEvoDict(pkmn.Name);
                  pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE] = true;
                  await SendDexMessage(pkmn, BuildEvoEmbed, Context.Channel, true);
               }
            }
            else
            {
               pkmn.Evolutions = GenerateEvoDict(pkmn.Name);
               pkmn.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE] = true;
               await SendDexMessage(pkmn, BuildEvoEmbed, Context.Channel, true);
            }
         }
      }
   }
}