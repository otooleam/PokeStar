using System;
using System.Threading.Tasks;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.ModuleParents;

namespace PokeStar.PreConditions
{
   /// <summary>
   /// Checks if message is a reply to a catch message.
   /// </summary>
   class CatchReplyAttribute : PreconditionAttribute
   {
      /// <summary>
      /// Checks message is a reply to a catch message.
      /// </summary>
      /// <param name="context">Context that the command was sent with.</param>
      /// <param name="command">Command that was sent.</param>
      /// <param name="services">Service collection used for dependency injection</param>
      /// <returns>Precondition result.</returns>
      public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
      {
         if (context.Message.Reference != null && DexCommandParent.IsCatchMessage(context.Message.Reference.MessageId.Value))
         {
            return await Task.FromResult(PreconditionResult.FromSuccess());
         }
         else
         {
            string message = $"{command.Name} command must be a reply to a catch simulation message.";
            await ResponseMessage.SendErrorMessage(context.Channel, command.Name, message);
            return await Task.FromResult(PreconditionResult.FromError(""));
         }
      }
   }
}