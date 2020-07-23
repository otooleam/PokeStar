using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Drawing;
using System.Reflection;
using Discord.Commands;
using Patagames.Ocr;
using Patagames.Ocr.Enums;
using System.Drawing.Imaging;
using System.Collections.Generic;
using PokeStar.ConnectionInterface;
using System.Security.Cryptography.X509Certificates;
using PokeStar.DataModels;
using PokeStar.Modules;
using System.Globalization;

namespace PokeStar.ImageProcessors
{
   public static class RaidImageProcess
   {
      public static async void ProcessImage(SocketCommandContext context)
      {
         if (context == null)
            return;

         var attachments = context.Message.Attachments;
         var user = context.Guild.Users.FirstOrDefault(x => x.Username.ToString().Equals(context.Message.Author.Username, StringComparison.OrdinalIgnoreCase));
         string url = attachments.ElementAt(0).Url;
         string imagePath = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Images\raid\{context.Message.Id}.png";
         string raidName = null;
         string raidLoc = null;
         DateTime raidTime = DateTime.Now;

         using (WebClient client = new WebClient())
         {
            client.DownloadFile(new Uri(url), imagePath);
         }

         using (Image image = Image.FromFile(imagePath))
         {
            using (Bitmap bitmap = ImageProcess.ScaleImage(image, 495, 880))
            {
               Bitmap croppedImage = bitmap.Clone(new Rectangle(10, 245, 480, 60), bitmap.PixelFormat);
               using (var api = OcrApi.Create())
               {
                  api.Init(Languages.English);
                  raidName = api.GetTextFromImage(ContrastText(bitmap.Clone(new Rectangle(10, 245, 480, 60), bitmap.PixelFormat), 245));
                  raidLoc = api.GetTextFromImage(ContrastText(bitmap.Clone(new Rectangle(110, 100, 300, 50), bitmap.PixelFormat), 210));
                  //raidTime = api.GetTextFromImage(ContrastText(bitmap.Clone(new Rectangle(370, 510, 90, 20), bitmap.PixelFormat), 120));
               }
            }
         }

         int closest = int.MaxValue;
         var raidBosses = SilphData.GetRaidBosses();
         RaidBossListElement raidBoss = new RaidBossListElement();
         foreach (var boss in raidBosses)
         {
            int dist = Compute(raidName, boss.Name);
            if (dist < closest)
            {
               closest = dist;
               raidBoss = boss;
            }
         }

         if (raidLoc.Equals(string.Empty))
            raidLoc = "Unknown Location";

         Raid raid = new Raid((short)raidBoss.Tier, raidTime.ToString("t", DateTimeFormatInfo.InvariantInfo), raidLoc, raidBoss.Name);
         string fileName = Connections.GetPokemonPicture(raid.Boss.Name);

         await RaidCommand.CreateRaidMessage(raid, fileName, context);
      }

      public static Bitmap ContrastText(Bitmap b, int threshold = 245)
      {
         for (int x = 0; x < b.Width; x++)
            for (int y = 0; y < b.Height; y++)
            {
               var pixel = b.GetPixel(x, y);
               if ( pixel.R < threshold || pixel.G < threshold || pixel.B < threshold)
                  b.SetPixel(x, y, Color.Black);
            }
         b.Save("temp.png", ImageFormat.Png);
         return b;
      }

      public static int Compute(string s, string t)
      {
         int n = s.Length;
         int m = t.Length;
         int[,] d = new int[n + 1, m + 1];

         // Step 1
         if (n == 0)
         {
            return m;
         }

         if (m == 0)
         {
            return n;
         }

         // Step 2
         for (int i = 0; i <= n; d[i, 0] = i++)
         {
         }

         for (int j = 0; j <= m; d[0, j] = j++)
         {
         }

         // Step 3
         for (int i = 1; i <= n; i++)
         {
            //Step 4
            for (int j = 1; j <= m; j++)
            {
               // Step 5
               int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

               // Step 6
               d[i, j] = Math.Min(
                   Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                   d[i - 1, j - 1] + cost);
            }
         }
         // Step 7
         return d[n, m];
      }
   }
}
