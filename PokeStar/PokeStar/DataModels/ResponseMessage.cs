using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PokeStar.ConnectionInterface;

namespace PokeStar.DataModels
{
   /// <summary>
   /// 
   /// </summary>
   public static class ResponseMessage
   {
      private static readonly string ERROR_IMAGE = "angrypuff.png";

      /// <summary>
      /// 
      /// </summary>
      /// <param name="context"></param>
      /// <param name="message"></param>
      /// <returns></returns>
      public static async Task SendInfoMessage(SocketCommandContext context, string message)
      {
         await context.Channel.SendMessageAsync(embed: GenerateInfoEmbed(message));
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="context"></param>
      /// <param name="command"></param>
      /// <param name="message"></param>
      /// <returns></returns>
      public static async Task SendWarningMessage(SocketCommandContext context, string command, string message)
      {
         await context.Channel.SendMessageAsync(embed: GenerateWarningEmbed(command, message));
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="context"></param>
      /// <param name="command"></param>
      /// <param name="message"></param>
      /// <returns></returns>
      public static async Task SendErrorMessage(SocketCommandContext context, string command, string message)
      {
         Connections.CopyFile(ERROR_IMAGE);
         await context.Channel.SendFileAsync(ERROR_IMAGE, embed: GenerateErrorEmbed(command, message));
         Connections.DeleteFile(ERROR_IMAGE);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="message"></param>
      /// <returns></returns>
      private static Embed GenerateInfoEmbed(string message)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.Purple);
         embed.WithDescription(message);
         return embed.Build();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      /// <param name="message"></param>
      /// <returns></returns>

      private static Embed GenerateWarningEmbed(string command, string message)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.Orange);
         embed.WithTitle($"Warning while executing {command}:");
         embed.WithDescription(message);
         return embed.Build();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      /// <param name="message"></param>
      /// <returns></returns>
      private static Embed GenerateErrorEmbed(string command, string message)
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