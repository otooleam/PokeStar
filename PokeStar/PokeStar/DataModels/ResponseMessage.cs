using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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
      /// <param name="channel">Channel to send message in.</param>
      /// <param name="message">Message to send.</param>
      /// <returns>Task Complete.</returns>
      public static async Task<Task> SendInfoMessage(IMessageChannel channel, string message)
      {
         await channel.SendMessageAsync(embed: BuildInfoEmbed(message));
         return Task.CompletedTask;
      }

      /// <summary>
      /// Send a warning message.
      /// </summary>
      /// <param name="channel">Channel to send message in.</param>
      /// <param name="command">Command that was executing.</param>
      /// <param name="message">Message to send.</param>
      /// <returns>Task Complete.</returns>
      public static async Task<Task> SendWarningMessage(IMessageChannel channel, string command, string message)
      {
         await channel.SendMessageAsync(embed: BuildWarningEmbed(command, message));
         return Task.CompletedTask;
      }

      /// <summary>
      /// Send an error message.
      /// </summary>
      /// <param name="channel">Channel to send message in.</param>
      /// <param name="command">Command that was executing.</param>
      /// <param name="message">Message to send.</param>
      /// <returns>Task Complete.</returns>
      public static async Task<Task> SendErrorMessage(IMessageChannel channel, string command, string message)
      {
         Connections.CopyFile(ERROR_IMAGE);
         await channel.SendFileAsync(ERROR_IMAGE, embed: BuildErrorEmbed(command, message));
         Connections.DeleteFile(ERROR_IMAGE);
         return Task.CompletedTask;
      }

      /// <summary>
      /// Build an embed for an info message.
      /// </summary>
      /// <param name="message">Message to send.</param>
      /// <returns></returns>
      private static Embed BuildInfoEmbed(string message)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.Purple);
         embed.WithDescription(message);
         return embed.Build();
      }

      /// <summary>
      /// Build an embed for a warning message.
      /// </summary>
      /// <param name="command">Command that was executing.</param>
      /// <param name="message">Message to send.</param>
      /// <returns></returns>

      private static Embed BuildWarningEmbed(string command, string message)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.Orange);
         embed.WithTitle($"Warning while executing {command}:");
         embed.WithDescription(message);
         return embed.Build();
      }

      /// <summary>
      /// Build an embed for an error message.
      /// </summary>
      /// <param name="command">Command that was executing.</param>
      /// <param name="message">Message to send.</param>
      /// <returns></returns>
      private static Embed BuildErrorEmbed(string command, string message)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.Red);
         embed.WithThumbnailUrl($"attachment://{ERROR_IMAGE}");
         embed.WithTitle($"Error while executing {command}:");
         embed.WithDescription(message);
         return embed.Build();
      }
   }
}