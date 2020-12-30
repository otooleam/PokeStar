using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;
using PokeStar.PreConditions;
using PokeStar.ModuleParents;

namespace PokeStar.Modules
{
   /// <summary>
   /// Handles catch reply commands.
   /// </summary>
   public class CatchReplyCommands : DexCommandParent
   {
      /// <summary>
      /// Handle level command.
      /// </summary>
      /// <param name="level">New Pokémon level.</param>
      /// <returns>Completed Task</returns>
      [Command("level")]
      [Summary("Set level of Pokémon.")]
      [Remarks("Level must be between 1 and 35 inclusive." +
               "Must be a reply to a catch message.")]
      [RegisterChannel('I')]
      [CatchReply()]
      public async Task Level([Summary("New Pokémon level.")] int level)
      {
         ulong catchMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage catchMessageMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(catchMessageId);
         CatchSimulation catchSim = catchMessages[catchMessageId];

         if (catchSim.SetCustomLevel(level))
         {
            string fileName = Connections.GetPokemonPicture(catchSim.Pokemon.Name);
            Connections.CopyFile(fileName);
            await catchMessageMessage.ModifyAsync(x =>
            {
               x.Embed = BuildCatchEmbed(catchSim, fileName);
            });
            Connections.DeleteFile(fileName);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "level", $"Level must be between {Global.MIN_WILD_LEVEL} and {Global.MAX_WILD_LEVEL}");
         }

         await Context.Message.DeleteAsync();
      }

      /// <summary>
      /// Handle radius command.
      /// </summary>
      /// <param name="radius">New catch radius.</param>
      /// <returns>Completed Task</returns>
      [Command("radius")]
      [Summary("Set custom catch radius of Pokémon.")]
      [Remarks("Radius must be between 1.0 and 2.0 inclusive." +
               "Must be a reply to a catch message.")]
      [RegisterChannel('I')]
      [CatchReply()]
      public async Task Radius([Summary("New catch radius.")] double radius)
      {
         ulong catchMessageId = Context.Message.Reference.MessageId.Value;
         SocketUserMessage catchMessageMessage = (SocketUserMessage)await Context.Channel.GetMessageAsync(catchMessageId);
         CatchSimulation catchSim = catchMessages[catchMessageId];

         if (catchSim.SetCustomRadius(radius))
         {
            string fileName = Connections.GetPokemonPicture(catchSim.Pokemon.Name);
            Connections.CopyFile(fileName);
            await catchMessageMessage.ModifyAsync(x =>
            {
               x.Embed = BuildCatchEmbed(catchSim, fileName);
            });
            Connections.DeleteFile(fileName);
         }
         else
         {
            await ResponseMessage.SendErrorMessage(Context.Channel, "radius", $"Radius must be between 1.0 and 2.0");
         }

         await Context.Message.DeleteAsync();
      }
   }
}
