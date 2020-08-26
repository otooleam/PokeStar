using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Drawing;
using System.Reflection;
using System.Collections.Generic;
using Discord.Commands;
using Patagames.Ocr;
using Patagames.Ocr.Enums;
using Discord.WebSocket;
using PokeStar.ConnectionInterface;

namespace PokeStar.ImageProcessors
{
   /// <summary>
   /// Processes an image of a user's role.
   /// </summary>
   public static class RollImageProcess
   {
      /// <summary>
      /// Processes an image of a user's profile page.
      /// Assigns the user's nickname and team.
      /// </summary>
      /// <param name="context">Command context that has the image.</param>
      public static async void RoleImageProcess(SocketCommandContext context)
      {
         if (context == null || Environment.GetEnvironmentVariable("SETUP_COMPLETE").Equals("FALSE", StringComparison.OrdinalIgnoreCase))
            return;

         IReadOnlyCollection<Discord.Attachment> attachments = context.Message.Attachments;
         SocketGuildUser user = context.Guild.Users.FirstOrDefault(x => x.Username.ToString().Equals(context.Message.Author.Username, StringComparison.OrdinalIgnoreCase));
         if (!Connections.Instance().GetSetupComplete(context.Guild.Id))
            return;
         string url = attachments.ElementAt(0).Url;
         string imagePath = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Images\profile\{user.Username}.png";
         string plainText = null;
         Color teamColor = Color.White;

         using (WebClient client = new WebClient())
         {
            client.DownloadFile(new Uri(url), imagePath);
         }

         using (Image image = Image.FromFile(imagePath))
         {
            using (Bitmap bitmap = ImageProcess.ScaleImage(image, 495, 880))
            {
               using (OcrApi api = OcrApi.Create())
               {
                  api.Init(Languages.English);
                  plainText = api.GetTextFromImage(bitmap, new Rectangle(10, 120, 480, 100));
               }
               Color[] teamColors = { Color.Red, Color.Blue, Color.Yellow };
               Color avgColor = GetAvgColor(bitmap, new Rectangle(410, 60, 10, 10));
               teamColor = teamColors.ElementAt(ClosestColor(new List<Color>(teamColors), avgColor));
            }
         }

         if (plainText != null)
         {
            int nameEndIndex = Math.Min(plainText.IndexOf('\n'), plainText.IndexOf(' '));
            try
            {
               string nickname = plainText.Substring(0, nameEndIndex);
               await user.ModifyAsync(x => { x.Nickname = nickname; }).ConfigureAwait(false);

               await context.Channel.SendMessageAsync($"{user.Username} now has the nickname {nickname}").ConfigureAwait(false);
            }
            catch (Exception e)
            {
               Console.WriteLine(e.Message);
               await context.Channel.SendMessageAsync($"Unable to set nickname for {user.Username}. Please set your nickname to your in game name in \"{context.Guild.Name}\"").ConfigureAwait(false);
            }
         }

         if (!teamColor.Equals(Color.White))
         {
            var valor = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Valor", StringComparison.OrdinalIgnoreCase));
            var mystic = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Mystic", StringComparison.OrdinalIgnoreCase));
            var instinct = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Instinct", StringComparison.OrdinalIgnoreCase));
            if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Valor", StringComparison.OrdinalIgnoreCase)) == null)
            {
               await user.RemoveRoleAsync(valor).ConfigureAwait(false);
            }
            else if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Mystic", StringComparison.OrdinalIgnoreCase)) == null)
            {
               await user.RemoveRoleAsync(mystic).ConfigureAwait(false);
            }
            else if (user.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Instinct", StringComparison.OrdinalIgnoreCase)) == null)
            {
               await user.RemoveRoleAsync(instinct).ConfigureAwait(false);
            }

            string teamName = "";
            if (teamColor.Equals(Color.Red))
            {
               teamName = "Valor";
            }
            if (teamColor.Equals(Color.Blue))
            {
               teamName = "Mystic";
            }
            if (teamColor.Equals(Color.Yellow))
            {
               teamName = "Instinct";
            }

            SocketRole team = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals(teamName, StringComparison.OrdinalIgnoreCase));
            await user.AddRoleAsync(team).ConfigureAwait(false);

            SocketRole role = context.Guild.Roles.FirstOrDefault(x => x.Name.ToString().Equals("Trainer", StringComparison.OrdinalIgnoreCase));
            await user.AddRoleAsync(role).ConfigureAwait(false);

            await context.Channel.SendMessageAsync($"{user.Username} now has the Trainer role and the {teamName} role").ConfigureAwait(false);
         }
      }

      /// <summary>
      /// Get the average color of a bitmap within a rectangle.
      /// </summary>
      /// <param name="bitmap">Bitmap to get the average color of.</param>
      /// <param name="rect">Rectangle to get average color within.</param>
      /// <returns>Average color of the bitmap within a rectangle.</returns>
      private static Color GetAvgColor(Bitmap bitmap, Rectangle rect)
      {
         int[] avgRGB = { 0, 0, 0 };
         for (int x = rect.X; x < (rect.X + rect.Width); x++)
         {
            for (int y = rect.Y; y < (rect.Y + rect.Height); y++)
            {
               Color c = bitmap.GetPixel(x, y);
               avgRGB[0] += c.R;
               avgRGB[1] += c.G;
               avgRGB[2] += c.B;
            }
         }
         int pixelCount = rect.Width * rect.Height;
         for (int i = 0; i < avgRGB.Length; i++)
         {
            avgRGB[i] /= pixelCount;
         }
         return Color.FromArgb(avgRGB[0], avgRGB[1], avgRGB[2]);
      }

      /// <summary>
      /// Gets the closest color from a list of colors to a given color.
      /// </summary>
      /// <param name="colors">List of colors to check against.</param>
      /// <param name="target">Color to check against list.</param>
      /// <returns>Closest color to the target color.</returns>
      private static int ClosestColor(List<Color> colors, Color target)
      {
         float hue1 = target.GetHue();
         IEnumerable<float> diffs = colors.Select(n => GetHueDistance(n.GetHue(), hue1));
         float diffMin = diffs.Min(n => n);
         return diffs.ToList().FindIndex(n => n == diffMin);
      }

      /// <summary>
      /// Gets the distance between two hue values
      /// </summary>
      /// <param name="hue1">First hue to check</param>
      /// <param name="hue2">Second hue to check.</param>
      /// <returns>Distance from 0-360 between the given hues.</returns>
      private static float GetHueDistance(float hue1, float hue2)
      {
         float d = Math.Abs(hue1 - hue2);
         return d > 180 ? 360 - d : d;
      }
   }
}