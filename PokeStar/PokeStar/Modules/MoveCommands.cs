using System;
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
using Discord.Rest;

namespace PokeStar.Modules
{
   public class MoveCommands : DexCommandParent
   {
      [Command("move")]
      [Summary("Gets information for a given move.")]
      [RegisterChannel('D')]
      public async Task Move([Summary("The move you want info about.")][Remainder] string move)
      {
         Move pkmnMove = Connections.Instance().GetMove(move);
         if (pkmnMove == null)
         {
            List<string> moveNames = Connections.Instance().SearchMove(move);

            string fileName = BLANK_IMAGE;
            Connections.CopyFile(fileName);
            RestUserMessage dexMessage = await Context.Channel.SendFileAsync(fileName, embed: BuildDexSelectEmbed(moveNames, fileName));
            await dexMessage.AddReactionsAsync(Global.SELECTION_EMOJIS);

            dexMessages.Add(dexMessage.Id, new Tuple<int, List<string>>((int)DEX_MESSAGE_TYPES.MOVE_MESSAGE, moveNames));
         }
         else
         {
            string fileName = BLANK_IMAGE;
            Connections.CopyFile(fileName);
            await Context.Channel.SendFileAsync(fileName, embed: BuildMoveEmbed(pkmnMove, fileName));
            Connections.DeleteFile(fileName);
         }
      }

      [Command("movetype")]
      [Summary("Gets information for a given type of move.")]
      [RegisterChannel('D')]
      public async Task MoveType([Summary("The type of move you want info about.")] string type,
                                 [Summary("The category of move you want info about (fast / charge).")] string category)
      {
         if (CheckValidType(type))
         {
            if (category.Equals(Global.FAST_MOVE_CATEGORY, StringComparison.OrdinalIgnoreCase))
            {

            }
            else if (category.Equals(Global.CHARGE_MOVE_CATEGORY, StringComparison.OrdinalIgnoreCase))
            {

            }
            else
            {
               await ResponseMessage.SendErrorMessage(Context.Channel, "movetype", $"{category} is not a valid move category.");
            }
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "movetype", $"{type} is not a valid type.");
         }
      }
   }
}