using System;
using System.Threading.Tasks;
using Discord.Commands;
using PokeStar.DataModels;
using PokeStar.Modules;

namespace PokeStar.PreConditions
{
   public class RegisterChannelAttribute : PreconditionAttribute
   {
      private readonly char register;

      public RegisterChannelAttribute(char registerString) => register = registerString;


      public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(context.Guild.Id, context.Channel.Id, register))
         {
            return Task.FromResult(PreconditionResult.FromSuccess());
         }
         else
         {
            string message = $"This channel is not registered to process {Global.REGISTER_STRING_TYPE[register]} commands.";
            ResponseMessage.SendErrorMessage(context, command.Name, message);
            return Task.FromResult(PreconditionResult.FromError(""));
         }
      }
   }
}
