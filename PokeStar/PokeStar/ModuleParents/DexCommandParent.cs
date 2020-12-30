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

namespace PokeStar.ModuleParents
{
   public class DexCommandParent : ModuleBase<SocketCommandContext>
   {
      /// <summary>
      /// Empty Pokémon image file name.
      /// </summary>
      protected static readonly string POKEDEX_SELECTION_IMAGE = "quest_pokemon.png";

      /// <summary>
      /// Image file for embeds without images.
      /// </summary>
      protected static readonly string BLANK_IMAGE = "battle.png";

      /// <summary>
      /// Saved dex messages.
      /// </summary>
      protected static readonly Dictionary<ulong, DexSelectionMessage> dexSelectMessages = new Dictionary<ulong, DexSelectionMessage>();

      /// <summary>
      /// 
      /// </summary>
      protected static readonly Dictionary<ulong, Pokemon> dexMessages = new Dictionary<ulong, Pokemon>();

      /// <summary>
      /// 
      /// </summary>
      protected static readonly Dictionary<ulong, CatchSimulation> catchMessages = new Dictionary<ulong, CatchSimulation>();

      /// <summary>
      /// 
      /// </summary>
      private static readonly IEmote[] dexEmojis = {
         new Emoji("1️⃣"),
         new Emoji("2️⃣"),
         new Emoji("3️⃣"),
         new Emoji("4️⃣"),
         new Emoji("5️⃣"),
         new Emoji("6️⃣"),
         new Emoji("❓"),
      };

      /// <summary>
      /// 
      /// </summary>
      protected static readonly Emoji[] catchEmojis = {
         new Emoji("⬅️"),
         new Emoji("⏺️"),
         new Emoji("➡️"),
         new Emoji("❓"),
      };

      /// <summary>
      /// 
      /// </summary>
      private static readonly string[] dexEmojisDesc = {
         "means switch to the Main dex page.",
         "means switch to the CP page.",
         "means switch to the PvP IV page.",
         "means switch to the Form page.",
         "means switch to the Evolution page.",
         "means switch to the Nickname page.",
      };

      /// <summary>
      /// 
      /// </summary>
      private static readonly string[] catchEmojisDesc = {
         "means decrement current modifier value.",
         "means cycle through modifiers to edit.",
         "means increment current modifier value."
      };

      /// <summary>
      /// 
      /// </summary>
      private static readonly string[] catchReplies = {
         "level <level>",
         "radius <radius>"
      };

      /// <summary>
      /// Types of dex sub messages.
      /// Also used for dex emoji index.
      /// </summary>
      protected enum DEX_MESSAGE_TYPES
      {
         DEX_MESSAGE,
         CP_MESSAGE,
         PVP_MESSAGE,
         FORM_MESSAGE,
         EVO_MESSAGE,
         NICKNAME_MESSAGE,
         CATCH_MESSAGE,
         MOVE_MESSAGE,
      }

      /// <summary>
      /// Index of all emotes on catch message.
      /// </summary>
      private enum CATCH_EMOJI_INDEX
      {
         DECREMENT,
         MODIFIER,
         INCREMENT,
         HELP,
      }

      /// Message checkers ****************************************************

      /// <summary>
      /// Checks if a message is a dex select message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a dex select message, otherwise false.</returns>
      public static bool IsDexSelectMessage(ulong id)
      {
         return dexSelectMessages.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a dex message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a dex select message, otherwise false.</returns>
      public static bool IsDexMessage(ulong id)
      {
         return dexMessages.ContainsKey(id);
      }

      /// <summary>
      /// Checks if a message is a catch message.
      /// </summary>
      /// <param name="id">Id of the message.</param>
      /// <returns>True if the message is a catch message, otherwise false.</returns>
      public static bool IsCatchMessage(ulong id)
      {
         return catchMessages.ContainsKey(id);
      }

      /// Message reaction handlers *******************************************

      /// <summary>
      /// Handles a reaction on a dex select message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <param name="guildId">Id of the guild that the message was sent in.</param>
      /// <returns>Completed Task.</returns>
      public static async Task DexSelectMessageReactionHandle(IMessage message, SocketReaction reaction, ulong guildId)
      {
         DexSelectionMessage dexMessage = dexSelectMessages[message.Id];
         for (int i = 0; i < dexMessage.Selections.Count; i++)
         {
            if (reaction.Emote.Equals(Global.SELECTION_EMOJIS[i]))
            {
               await message.DeleteAsync();
               if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.DEX_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  Connections.Instance().GetPokemonStats(ref pokemon);
                  pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE] = true;
                  await SendDexMessage(pokemon, BuildDexEmbed, reaction.Channel, true);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.CP_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  Connections.GetPokemonCP(ref pokemon);
                  pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.CP_MESSAGE] = true;
                  await SendDexMessage(pokemon, BuildCPEmbed, reaction.Channel, true);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.PVP_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  Connections.Instance().GetPokemonPvP(ref pokemon);
                  pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE] = true;
                  await SendDexMessage(pokemon, BuildPvPEmbed, reaction.Channel, true);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.FORM_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemon.Number);

                  if (pokemonWithNumber.Count == 1)
                  {
                     pokemon.Forms = new Form();
                  }
                  else if (pokemonWithNumber.Count > 1)
                  {
                     string baseName = Connections.Instance().GetBaseForms().Intersect(pokemonWithNumber).First();
                     pokemon.Forms = Connections.Instance().GetFormTags(baseName);
                  }
                  pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE] = true;
                  await SendDexMessage(pokemon, BuildFormEmbed, reaction.Channel, true);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.EVO_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  pokemon.Evolutions = GenerateEvoDict(pokemon.Name);
                  pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE] = true;
                  await SendDexMessage(pokemon, BuildEvoEmbed, reaction.Channel, true);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.NICKNAME_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  pokemon.Nicknames = Connections.Instance().GetNicknames(guildId, pokemon.Name);
                  await SendDexMessage(pokemon, BuildNicknameEmbed, reaction.Channel, true);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.MOVE_MESSAGE)
               {
                  Move pkmnMove = Connections.Instance().GetMove(dexMessage.Selections[i]);
                  string fileName = BLANK_IMAGE;
                  Connections.CopyFile(fileName);
                  await reaction.Channel.SendFileAsync(fileName, embed: BuildMoveEmbed(pkmnMove, fileName));
                  Connections.DeleteFile(fileName);
               }
               else if (dexMessage.Type == (int)DEX_MESSAGE_TYPES.CATCH_MESSAGE)
               {
                  Pokemon pokemon = Connections.Instance().GetPokemon(dexMessage.Selections[i]);
                  CatchSimulation catchSim = new CatchSimulation(pokemon);
                  string fileName = Connections.GetPokemonPicture(pokemon.Name);
                  Connections.CopyFile(fileName);
                  RestUserMessage catchMessage = await reaction.Channel.SendFileAsync(fileName, embed: BuildCatchEmbed(catchSim, fileName));
                  catchMessages.Add(catchMessage.Id, catchSim);
                  Connections.DeleteFile(fileName);
                  catchMessage.AddReactionsAsync(catchEmojis);
               }
               dexSelectMessages.Remove(message.Id);
               return;
            }
         }
         await message.RemoveReactionAsync(reaction.Emote, (SocketGuildUser)reaction.User);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="message"></param>
      /// <param name="reaction"></param>
      /// <param name="guildId"></param>
      /// <returns></returns>
      public static async Task DexMessageReactionHandle(IMessage message, SocketReaction reaction, ulong guildId)
      {
         Pokemon pokemon = dexMessages[message.Id];
         SocketUserMessage msg = (SocketUserMessage)message;
         string fileName = Connections.GetPokemonPicture(pokemon.Name);
         Connections.CopyFile(fileName);
         if (reaction.Emote.Equals(dexEmojis[dexEmojis.Length - 1]))
         {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("**Dex Message Emoji Help:**");

            for (int i = 0; i < dexEmojis.Length - 1; i++)
            {
               sb.AppendLine($"{dexEmojis[i]} {dexEmojisDesc[i]}");
            }
            await reaction.User.Value.SendMessageAsync(sb.ToString());
         }
         else if (reaction.Emote.Equals(dexEmojis[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE]))
         {
            if (!pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE])
            {
               Connections.Instance().GetPokemonStats(ref pokemon);
               pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE] = true;
            }
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildDexEmbed(pokemon, fileName);
            });
         }
         else if (reaction.Emote.Equals(dexEmojis[(int)DEX_MESSAGE_TYPES.CP_MESSAGE]))
         {
            if (!pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.CP_MESSAGE])
            {
               Connections.GetPokemonCP(ref pokemon);
               pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.CP_MESSAGE] = true;
            }
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildCPEmbed(pokemon, fileName);
            });
         }
         else if (reaction.Emote.Equals(dexEmojis[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE]))
         {
            if (!pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE])
            {
               Connections.Instance().GetPokemonPvP(ref pokemon);
               pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE] = true;
            }
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildPvPEmbed(pokemon, fileName);
            });
         }
         else if (reaction.Emote.Equals(dexEmojis[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE]))
         {
            if (!pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE])
            {
               List<string> pokemonWithNumber = Connections.Instance().GetPokemonByNumber(pokemon.Number);

               if (pokemonWithNumber.Count == 1)
               {
                  pokemon.Forms = new Form();
               }
               else if (pokemonWithNumber.Count > 1)
               {
                  string baseName = Connections.Instance().GetBaseForms().Intersect(pokemonWithNumber).First();
                  pokemon.Forms = Connections.Instance().GetFormTags(baseName);
               }
               pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE] = true;
            }
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildFormEmbed(pokemon, fileName);
            });
         }
         else if (reaction.Emote.Equals(dexEmojis[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE]))
         {
            if (!pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE])
            {
               pokemon.Evolutions = GenerateEvoDict(pokemon.Name);
               pokemon.CompleteDataLookUp[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE] = true;
            }
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildEvoEmbed(pokemon, fileName);
            });
         }
         else if (reaction.Emote.Equals(dexEmojis[(int)DEX_MESSAGE_TYPES.NICKNAME_MESSAGE]))
         {
            pokemon.Nicknames = Connections.Instance().GetNicknames(guildId, pokemon.Name);
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildNicknameEmbed(pokemon, fileName);
            });
         }
         Connections.DeleteFile(fileName);
         await message.RemoveReactionAsync(reaction.Emote, (SocketGuildUser)reaction.User);
      }

      /// <summary>
      /// Handles a reaction on a catch message.
      /// </summary>
      /// <param name="message">Message that was reacted on.</param>
      /// <param name="reaction">Reaction that was sent.</param>
      /// <returns></returns>
      public static async Task CatchMessageReactionHandle(IMessage message, SocketReaction reaction)
      {
         CatchSimulation catchSim = catchMessages[message.Id];
         bool needsUpdate = true;
         if (reaction.Emote.Equals(catchEmojis[(int)CATCH_EMOJI_INDEX.DECREMENT]))
         {
            catchSim.DecrementModifierValue();
         }
         else if (reaction.Emote.Equals(catchEmojis[(int)CATCH_EMOJI_INDEX.MODIFIER]))
         {
            catchSim.UpdateModifier();
         }
         else if (reaction.Emote.Equals(catchEmojis[(int)CATCH_EMOJI_INDEX.INCREMENT]))
         {
            catchSim.IncrementModifierValue();
         }
         else if (reaction.Emote.Equals(catchEmojis[(int)CATCH_EMOJI_INDEX.HELP]))
         {
            string prefix = Connections.Instance().GetPrefix(((SocketGuildChannel)message.Channel).Guild.Id);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("**Catch Emoji Help:**");
            for(int i = 0; i < catchEmojisDesc.Length; i++)
            {
               sb.AppendLine($"{catchEmojis[i]} {catchEmojisDesc[i]}");
            }

            sb.AppendLine("\n**Catch Reply Help:**");
            foreach (string reply in catchReplies)
            {
               sb.AppendLine($"{prefix}{reply}");
            }

            await ((SocketGuildUser)reaction.User).SendMessageAsync(sb.ToString());
            needsUpdate = false;
         }
         else
         {
            needsUpdate = false;
         }

         if (needsUpdate)
         {
            SocketUserMessage msg = (SocketUserMessage)message;
            string fileName = Connections.GetPokemonPicture(catchSim.Pokemon.Name);
            Connections.CopyFile(fileName);
            await msg.ModifyAsync(x =>
            {
               x.Embed = BuildCatchEmbed(catchSim, fileName);
            });
            Connections.DeleteFile(fileName);
         }

         await message.RemoveReactionAsync(reaction.Emote, (SocketGuildUser)reaction.User);
      }

      /// Embed builders ******************************************************

      /// <summary>
      /// Builds a dex embed.
      /// </summary>
      /// <param name="pokemon">Pokémon to display</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon.</returns>
      protected static Embed BuildDexEmbed(Pokemon pokemon, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($@"#{pokemon.Number} {pokemon.Name}");
         embed.WithDescription(pokemon.Description);
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Type", pokemon.TypeToString(), true);
         embed.AddField("Weather Boosts", pokemon.WeatherToString(), true);
         embed.AddField("Status", pokemon.StatusToString(), true);
         embed.AddField("Resistances", pokemon.ResistanceToString(), true);
         embed.AddField("Weaknesses", pokemon.WeaknessToString(), true);
         embed.AddField("Stats", pokemon.StatsToString(), true);
         embed.AddField("Fast Moves", pokemon.FastMoveToString(), true);
         embed.AddField("Charge Moves", pokemon.ChargeMoveToString(), true);
         embed.AddField("Details", pokemon.DetailsToString(), true);
         embed.AddField("Counters", pokemon.CounterToString(), false);
         if (pokemon.IsRegional())
         {
            embed.AddField("Regions", pokemon.RegionalToString(), true);
         }
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         embed.WithFooter($"{Global.STAB_SYMBOL} denotes STAB move {Global.LEGACY_MOVE_SYMBOL} denotes Legacy move");
         return embed.Build();
      }

      /// <summary>
      /// Builds a cp embed.
      /// </summary>
      /// <param name="pokemon">Pokémon to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon's CP.</returns>
      protected static Embed BuildCPEmbed(Pokemon pokemon, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($@"#{pokemon.Number} {pokemon.Name} CP");
         embed.WithDescription($"Max CP values for {pokemon.Name}");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField($"Max Half Level CP (Level 40)", pokemon.CPMaxHalf, true);
         embed.AddField($"Max CP (Level 50)", pokemon.CPMax, true);
         embed.AddField($"Max Buddy CP (Level 51)", pokemon.CPBestBuddy, true);
         embed.AddField($"Raid CP (Level 20)", pokemon.RaidCPToString(), true);
         embed.AddField($"Hatch CP (Level 20)", pokemon.HatchCPToString(), true);
         embed.AddField($"Quest CP (Level 15)", pokemon.QuestCPToString(), true);
         embed.AddField("Wild CP (Level 1-35)", pokemon.WildCPToString(), false);
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         embed.WithFooter($"{Global.WEATHER_BOOST_SYMBOL} denotes Weather Boosted CP");
         return embed.Build();
      }

      /// <summary>
      /// Builds a pvp embed.
      /// </summary>
      /// <param name="pokemon">Pokémon to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon's CP.</returns>
      protected static Embed BuildPvPEmbed(Pokemon pokemon, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($@"#{pokemon.Number} {pokemon.Name} CP");
         embed.WithDescription($"Max PvP IV values for {pokemon.Name}");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         if (pokemon.CanBeLittleLeague)
         {
            embed.AddField($"Little League (Max Level 41)", pokemon.LittleIVs, false);
            embed.AddField($"Little League XL (Max Level 51)", pokemon.LittleXLIVs, false);
         }
         embed.AddField($"Great League (Max Level 41)", pokemon.GreatIVs, false);
         embed.AddField($"Great League XL (Max Level 51)", pokemon.GreatXLIVs, false);
         embed.AddField($"Ultra League (Max Level 41)", pokemon.UltraIVs, false);
         embed.AddField($"Ultra League XL (Max Level 51)", pokemon.UltraXLIVs, false);
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         return embed.Build();
      }

      /// <summary>
      /// Builds a form embed.
      /// </summary>
      /// <param name="baseName">Name of the base form of the Pokémon.</param>
      /// <param name="forms">List of forms of the Pokémon.</param>
      /// <param name="defaultForm">Default form of the Pokémon.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns></returns>
      protected static Embed BuildFormEmbed(Pokemon pokemon, string fileName)
      {

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         embed.WithFooter($"{Global.DEFAULT_FORM_SYMBOL} denotes default form.");

         StringBuilder sb = new StringBuilder();
         if (pokemon.Forms.FormList == null)
         {
            sb.AppendLine($"There are no alternate forms for {pokemon.Name}.");
         }
         else
         {
            foreach (string form in pokemon.Forms.FormList)
            {
               sb.AppendLine($"{form}{(form.Equals(pokemon.Forms.DefaultForm, StringComparison.OrdinalIgnoreCase) ? $"{Global.DEFAULT_FORM_SYMBOL}" : "")}");
            }
         }
         embed.AddField($"Forms for {pokemon.Name}", sb.ToString(), false);
         return embed.Build();
      }

      /// <summary>
      /// Builds an evolution embed.
      /// </summary>
      /// <param name="evolutions">Dictionary of evolutions.</param>
      /// <param name="initialPokemon">Pokémon that was searched for.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon's evolutions.</returns>
      protected static Embed BuildEvoEmbed(Pokemon pokemon, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($"Evolution Family for {pokemon.Name}");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         if (pokemon.Evolutions.Count == 1)
         {
            embed.WithDescription("This Pokémon does not evolve in to or from any other Pokémon.");
         }
         else
         {
            foreach (KeyValuePair<string, string> pkmn in pokemon.Evolutions)
            {
               embed.AddField($"{pkmn.Key}", pkmn.Value);
            }
         }
         return embed.Build();
      }

      /// <summary>
      /// Builds a nickname embed.
      /// </summary>
      /// <param name="nicknames">List of nicknames.</param>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for viewing a Pokémon's nicknames.</returns>
      protected static Embed BuildNicknameEmbed(Pokemon pokemon, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);

         if (pokemon.Nicknames.Count == 0)
         {
            embed.WithTitle($"There are no nicknames registered for {pokemon.Name}.");
         }
         else
         {
            StringBuilder sb = new StringBuilder();
            foreach (string nickname in pokemon.Nicknames)
            {
               sb.AppendLine(nickname);
            }
            embed.AddField($"**Nicknames for {pokemon.Name}**", sb.ToString());
         }
         return embed.Build();
      }

      /// <summary>
      /// Builds a move embed.
      /// </summary>
      /// <param name="move">Move to display.</param>
      /// <returns>Embed for viewing a move.</returns>
      protected static Embed BuildMoveEmbed(Move move, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle(move.Name);
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField("Type", move.TypeToString(), true);
         embed.AddField("Weather Boosts", move.WeatherToString(), true);
         embed.AddField("Category", move.Category, true);
         embed.AddField("PvP Power", move.PvPPower, true);
         embed.AddField("PvP Energy", move.EnergyToString(move.PvPEnergy), true);
         embed.AddField("PvP Turns", move.PvPTurns, true);
         embed.AddField("PvE Power", move.PvEPower, true);
         embed.AddField("PvE Energy", move.EnergyToString(move.PvEEnergy), true);
         embed.AddField("PvE Cooldown", $"{move.Cooldown} ms", true);
         embed.AddField("PvE Damage Window", move.DamageWindowString(), true);
         embed.AddField("Number of Pokémon that can learn this move", move.PokemonWithMove.Count, false);
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         return embed.Build();
      }

      /// <summary>
      /// Builds a catch embed.
      /// </summary>
      /// <param name="catchSim">Catch simulator to display.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns></returns>
      protected static Embed BuildCatchEmbed(CatchSimulation catchSim, string fileName)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithTitle($@"#{catchSim.Pokemon.Number} {catchSim.Pokemon.Name}");
         embed.WithDescription($"**Catch Chance:** {catchSim.CatchChance}%");
         embed.WithThumbnailUrl($"attachment://{fileName}");
         embed.AddField($"Base Catch Rate:", $"{catchSim.Pokemon.CatchRate * 100.0}%", true);
         embed.AddField("Pokémon Level:", $"{catchSim.GetLevel()}", true);
         embed.AddField("Pokéball Type:", $"{catchSim.GetBall()}", true);
         embed.AddField("Berry Type:", $"{catchSim.GetBerry()}", true);
         embed.AddField("Throw Type:", $"{catchSim.GetThrow()}", true);
         embed.AddField("Is Curveball:", $"{catchSim.GetCurveball()}", true);
         embed.AddField("Medal 1 Bonus:", $"{catchSim.GetMedal1()}", true);
         if (catchSim.Pokemon.Type.Count != 1)
         {
            embed.AddField("Medal 2 Bonus:", $"{catchSim.GetMedal2()}", true);
         }
         embed.AddField("Encounter Type:", $"{catchSim.GetEncounter()}", true);
         embed.WithColor(catchSim.CalcRingColor());
         embed.WithFooter($"Currently editing: {catchSim.GetCurrentModifier()}");
         return embed.Build();
      }

      /// <summary>
      /// Builds the PokéDex select embed.
      /// </summary>
      /// <param name="potentials">List of potential Pokémon.</param>
      /// <param name="fileName">Name of image file.</param>
      /// <returns>Embed for selecting a Pokémon.</returns>
      protected static Embed BuildDexSelectEmbed(List<string> potentials, string fileName)
      {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < potentials.Count; i++)
         {
            sb.AppendLine($"{Global.SELECTION_EMOJIS[i]} {potentials[i]}");
         }

         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_DEX_RESPONSE);
         embed.WithTitle("Do you mean...?");
         embed.WithDescription(sb.ToString());
         embed.WithThumbnailUrl($"attachment://{fileName}");
         return embed.Build();
      }

      /// Name processors *****************************************************

      /// <summary>
      /// Processes the Pokémon name given from a command.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>Full name of the Pokémon.</returns>
      protected static string GetPokemonName(string pokemonName)
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
      /// Gets the full name of a Pokémon.
      /// The following Pokémon have multiple forms:
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
      /// Note: Nidoran defaults to the female form.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="form">Form of the Pokémon.</param>
      /// <returns>Full name of the Pokémon.</returns>
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
         // Gender
         else if (form.Equals("-male", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && (pokemonName.Equals("nidoran", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("pyroar", StringComparison.OrdinalIgnoreCase) || pokemonName.Equals("meowstic", StringComparison.OrdinalIgnoreCase))))
            return $"{pokemonName} M";
         // Unown and Gender
         else if ((string.IsNullOrWhiteSpace(form) && pokemonName.Equals("unown", StringComparison.OrdinalIgnoreCase)) || form.Equals("-female", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} F";
         // Mewtwo
         else if (form.Equals("-armor", StringComparison.OrdinalIgnoreCase) || form.Equals("-armored", StringComparison.OrdinalIgnoreCase))
            return $"Armored {pokemonName}";
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
         // Genesect
         else if (form.Equals("-burn", StringComparison.OrdinalIgnoreCase))
            return $"Burn Drive {pokemonName}";
         else if (form.Equals("-chill", StringComparison.OrdinalIgnoreCase))
            return $"Chill Drive {pokemonName}";
         else if (form.Equals("-douse", StringComparison.OrdinalIgnoreCase))
            return $"Douse Drive {pokemonName}";
         else if (form.Equals("-shock", StringComparison.OrdinalIgnoreCase))
            return $"Shock Drive {pokemonName}";
         // Aegislash
         else if (form.Equals("-blade", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("aegislash", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} Blade Form";
         else if (form.Equals("-shield", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Shield Form";
         // Hoopa
         else if (form.Equals("-confined", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("hoopa", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} Confined";
         else if (form.Equals("-unbound", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Unbound";
         // Zygarde
         else if (form.Equals("-50", StringComparison.OrdinalIgnoreCase) || (string.IsNullOrWhiteSpace(form) && pokemonName.Equals("zygarde", StringComparison.OrdinalIgnoreCase)))
            return $"{pokemonName} 50% Form";
         else if (form.Equals("-10", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} 10% Form";
         else if (form.Equals("-complete", StringComparison.OrdinalIgnoreCase))
            return $"{pokemonName} Complete Form";

         return pokemonName;
      }

      /// Evolution processors ************************************************

      /// <summary>
      /// Generage an ordered dictionary of evolutions.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <returns>Ordered dictionary of evolutions.</returns>
      public static Dictionary<string, string> GenerateEvoDict(string pokemonName)
      {
         List<Evolution> initEvoFamily = Connections.Instance().GetEvolutionFamily(pokemonName);

         if (initEvoFamily.Count == 0)
         {
            return new Dictionary<string, string>()
            {
               [pokemonName] = ""
            };
         }

         foreach (Evolution evo in initEvoFamily)
         {
            foreach (Evolution evoComp in initEvoFamily)
            {
               evo.Combine(evoComp);
            }
         }
         List<Evolution> normalEvoFamily = initEvoFamily.Where(x => x.Candy != Global.BAD_EVOLUTION).ToList();

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

         EvolutionNode tree = BuildEvolutionTree(basePokemon, normalEvoFamily);
         return EvolutionTreeToString(tree);
      }

      /// <summary>
      /// Recursivly builds an evolution tree.
      /// A tree is made up of evolution nodes.
      /// </summary>
      /// <param name="pokemonName">Name of the Pokémon.</param>
      /// <param name="evolutions">List of evolutions.</param>
      /// <returns>Evolution node that starts the tree.</returns>
      private static EvolutionNode BuildEvolutionTree(string pokemonName, List<Evolution> evolutions)
      {
         string method = "";
         foreach (Evolution evo in evolutions)
         {
            if (pokemonName.Equals(evo.End, StringComparison.OrdinalIgnoreCase))
            {
               method = evo.MethodToString();
            }
         }

         EvolutionNode node = new EvolutionNode
         {
            Name = pokemonName,
            Method = method
         };

         foreach (Evolution evo in evolutions)
         {
            if (pokemonName.Equals(evo.Start, StringComparison.OrdinalIgnoreCase))
            {
               node.Evolutions.Add(BuildEvolutionTree(evo.End, evolutions));
            }
         }
         return node;
      }

      /// <summary>
      /// Converts an evolution tree to a dictionary.
      /// </summary>
      /// <param name="node">Node to convert to dictionary.</param>
      /// <param name="previousEvolution">Name of previous evolution.</param>
      /// <returns>Ordered dictionary of evolutions.</returns>
      private static Dictionary<string, string> EvolutionTreeToString(EvolutionNode node, string previousEvolution = null)
      {
         Dictionary<string, string> evolutions = new Dictionary<string, string>();
         string evoString = previousEvolution == null ? "Base Form" : $"Evolves from {previousEvolution} with {node.Method}";
         evolutions.Add(node.Name, evoString);

         foreach (EvolutionNode evo in node.Evolutions)
         {
            evolutions = evolutions.Union(EvolutionTreeToString(evo, node.Name)).ToDictionary(x => x.Key, x => x.Value);
         }
         return evolutions;
      }

      /// Type processors ************************************************

      /// <summary>
      /// Formats weather boosts as a string.
      /// </summary>
      /// <param name="weatherList">List of weather that boosts the type(s).</param>
      /// <returns>Weather for type(s) as a string.</returns>
      protected static string FormatWeatherList(List<string> weatherList)
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
      protected static string FormatTypeList(Dictionary<string, int> relations)
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
      /// Checks if a type is valid.
      /// </summary>
      /// <param name="type">Type to check.</param>
      /// <returns>True if the type is valid, otherwise false.</returns>
      protected static bool CheckValidType(string type)
      {
         return Global.NONA_EMOJIS.ContainsKey($"{type}_emote");
      }

      /// Message senders *****************************************************

      /// <summary>
      /// 
      /// </summary>
      /// <param name="messageType"></param>
      /// <param name="options"></param>
      /// <param name="channel"></param>
      /// <returns></returns>
      protected static async Task SendDexSelectionMessage(int messageType, List<string> options, ISocketMessageChannel channel)
      {
         string fileName = POKEDEX_SELECTION_IMAGE;
         Connections.CopyFile(fileName);
         RestUserMessage dexMessage = await channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(options, fileName));
         dexSelectMessages.Add(dexMessage.Id, new DexSelectionMessage(messageType, options));
         Connections.DeleteFile(fileName);
         dexMessage.AddReactionsAsync(Global.SELECTION_EMOJIS.Take(options.Count).ToArray());
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="pokemon"></param>
      /// <param name="EmbedMethod"></param>
      /// <param name="channel"></param>
      /// <param name="addEmojis"></param>
      /// <returns></returns>
      protected static async Task SendDexMessage(Pokemon pokemon, Func<Pokemon, string, Embed> EmbedMethod, ISocketMessageChannel channel, bool addEmojis = false)
      {
         string fileName = Connections.GetPokemonPicture(pokemon.Name);
         Connections.CopyFile(fileName);
         RestUserMessage message = await channel.SendFileAsync(fileName, embed: EmbedMethod(pokemon, fileName));
         dexMessages.Add(message.Id, pokemon);
         Connections.DeleteFile(fileName);
         if (addEmojis)
         {
            message.AddReactionsAsync(dexEmojis);
         }
      }

      /// Miscellaneous *******************************************************

      /// <summary>
      /// Sets custom emotes used for dex messages.
      /// </summary>
      public static void SetInitialEmotes()
      {
         dexEmojis[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE] = Global.NUM_EMOJIS[(int)DEX_MESSAGE_TYPES.DEX_MESSAGE];
         dexEmojis[(int)DEX_MESSAGE_TYPES.CP_MESSAGE] = Global.NUM_EMOJIS[(int)DEX_MESSAGE_TYPES.CP_MESSAGE];
         dexEmojis[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE] = Global.NUM_EMOJIS[(int)DEX_MESSAGE_TYPES.PVP_MESSAGE];
         dexEmojis[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE] = Global.NUM_EMOJIS[(int)DEX_MESSAGE_TYPES.FORM_MESSAGE];
         dexEmojis[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE] = Global.NUM_EMOJIS[(int)DEX_MESSAGE_TYPES.EVO_MESSAGE];
         dexEmojis[(int)DEX_MESSAGE_TYPES.NICKNAME_MESSAGE] = Global.NUM_EMOJIS[(int)DEX_MESSAGE_TYPES.NICKNAME_MESSAGE];
      }

      /// <summary>
      /// Removes old dex messages from the list of dex messages.
      /// Old dex messages are messages older than one day.
      /// </summary>
      protected static void RemoveOldDexMessages()
      {
         List<ulong> ids = new List<ulong>();
         foreach (KeyValuePair<ulong, Pokemon> dexMessage in dexMessages)
         {
            if (Math.Abs((DateTime.Now - dexMessage.Value.CreatedAt).TotalDays) >= 1)
            {
               ids.Add(dexMessage.Key);
            }
         }
         foreach (ulong id in ids)
         {
            dexMessages.Remove(id);
         }
      }
   }
}