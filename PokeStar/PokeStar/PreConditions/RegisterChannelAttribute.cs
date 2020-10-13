using System;
using System.Threading.Tasks;
using Discord.Commands;
using PokeStar.Modules;
using PokeStar.DataModels;

namespace PokeStar.PreConditions
{
   /// <summary>
   /// Checks if a channel is registerd for a speciffic command type.
   /// </summary>
   public class RegisterChannelAttribute : PreconditionAttribute
   {
      private readonly char register;

      /// <summary>
      /// Creates a new RegisterChannelAttribute.
      /// </summary>
      /// <param name="registerString">Registration type to check.</param>
      public RegisterChannelAttribute(char registerString) => register = registerString;

      /// <summary>
      /// Checks permissions for the command.
      /// </summary>
      /// <param name="context">Context that the command was sent with.</param>
      /// <param name="command">Command that was sent.</param>
      /// <param name="services">Service collection used for dependency injection</param>
      /// <returns></returns>
      public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
      {
         if (ChannelRegisterCommands.IsRegisteredChannel(context.Guild.Id, context.Channel.Id, register))
         {
            return await Task.FromResult(PreconditionResult.FromSuccess());
         }
         else
         {
            string message = $"This channel is not registered to process {Global.REGISTER_STRING_TYPE[register]} commands.";
            await ResponseMessage.SendErrorMessage(context, command.Name, message);
            return await Task.FromResult(PreconditionResult.FromError(""));
         }
      }
   }
}
