using System;
using System.Threading.Tasks;
using Discord.Commands;
using PokeStar.ModuleParents;
using PokeStar.DataModels;

namespace PokeStar.PreConditions
{
   class RaidReplyAttribute : PreconditionAttribute
   {
      public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
      {
         if (context.Message.Reference != null && RaidCommandParent.IsRaidMessage(context.Message.Reference.MessageId.Value))
         {
            return await Task.FromResult(PreconditionResult.FromSuccess());
         }
         else
         {
            string message = $"{command.Name} command must be a reply to a raid message.";
            await ResponseMessage.SendErrorMessage(context.Channel, command.Name, message);
            return await Task.FromResult(PreconditionResult.FromError(""));
         }
      }
   }
}
