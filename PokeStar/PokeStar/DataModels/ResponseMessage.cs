using System.Threading.Tasks;
using Discord;
using PokeStar.ConnectionInterface;

namespace PokeStar.DataModels
{
   /// <summary>
   /// Response message from Nona.
   /// </summary>
   public static class ResponseMessage
   {
      /// <summary>
      /// Error image file name.
      /// </summary>
      private const string ERROR_IMAGE = "angrypuff.png";

      /// <summary>
      /// Send an info message.
      /// </summary>
      /// <param name="channel">Channel to send message to.</param>
      /// <param name="message">Message to send.</param>
      /// <returns>The sent message.</returns>
      public static async Task<IUserMessage> SendInfoMessage(IMessageChannel channel, string message)
      {
         return await channel.SendMessageAsync(embed: BuildInfoEmbed(message));
      }

      /// <summary>
      /// Send a warning message.
      /// </summary>
      /// <param name="channel">Channel to send message to.</param>
      /// <param name="command">Command that was executing.</param>
      /// <param name="message">Message to send.</param>
      /// <returns>The sent message.</returns>
      public static async Task<IUserMessage> SendWarningMessage(IMessageChannel channel, string command, string message)
      {
         return await channel.SendMessageAsync(embed: BuildWarningEmbed(command, message));
      }

      /// <summary>
      /// Send an error message.
      /// Used for precondition errors.
      /// </summary>
      /// <param name="channel">Channel to send message to.</param>
      /// <param name="command">Command that was executing.</param>
      /// <param name="message">Message to send.</param>
      /// <returns>The sent message.</returns>
      public static async Task<IUserMessage> SendErrorMessage(IMessageChannel channel, string command, string message)
      {
         Connections.CopyFile(ERROR_IMAGE);
         IUserMessage msg = await channel.SendFileAsync(ERROR_IMAGE, embed: BuildErrorEmbed(command, message));
         Connections.DeleteFile(ERROR_IMAGE);
         return msg;
      }

      /// <summary>
      /// Modify an info message.
      /// </summary>
      /// <param name="msg">Message to modify.</param>
      /// <param name="message">Message to send.</param>
      /// <returns>Completed Task.</returns>
      public static async Task<Task> ModifyInfoMessage(IUserMessage msg, string message)
      {
         await msg.ModifyAsync(x =>
         {
            x.Embed = BuildInfoEmbed(message);
         });
         return Task.CompletedTask;
      }

      /// <summary>
      /// Build an embed for an info message.
      /// </summary>
      /// <param name="message">Message to send.</param>
      /// <returns>Embed for an info message.</returns>
      private static Embed BuildInfoEmbed(string message)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_INFO_RESPONSE);
         embed.WithDescription(message);
         return embed.Build();
      }

      /// <summary>
      /// Build an embed for a warning message.
      /// </summary>
      /// <param name="command">Command that was executing.</param>
      /// <param name="message">Message to send.</param>
      /// <returns>Embed for a warning message.</returns>
      private static Embed BuildWarningEmbed(string command, string message)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_WARNING_RESPONSE);
         embed.WithTitle($"Warning while executing {command}:");
         embed.WithDescription(message);
         return embed.Build();
      }

      /// <summary>
      /// Build an embed for an error message.
      /// </summary>
      /// <param name="command">Command that was executing.</param>
      /// <param name="message">Message to send.</param>
      /// <returns>Embed for an error message.</returns>
      private static Embed BuildErrorEmbed(string command, string message)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Global.EMBED_COLOR_ERROR_RESPONSE);
         embed.WithThumbnailUrl($"attachment://{ERROR_IMAGE}");
         embed.WithTitle($"Error while executing {command}:");
         embed.WithDescription(message);
         return embed.Build();
      }
   }
}