using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace PokeStar.DataModels
{
   /// <summary>
   /// 
   /// </summary>
   public static class ResponseMessage
   {
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
      public static async Task SendErrorMessage(SocketCommandContext context, string command, string message)
      {
         await context.Channel.SendMessageAsync(embed: GenerateErrorEmbed(command, message));
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
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
      private static Embed GenerateErrorEmbed(string command, string message)
      {
         EmbedBuilder embed = new EmbedBuilder();
         embed.WithColor(Color.Orange);
         embed.WithTitle($"Error executing {command}");
         embed.WithDescription(message);
         return embed.Build();
      }
   }
}
