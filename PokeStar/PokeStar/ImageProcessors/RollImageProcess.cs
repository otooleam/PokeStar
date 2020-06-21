using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using Discord.Commands;
using Patagames.Ocr;
using Patagames.Ocr.Enums;

namespace PokeStar.ImageProcessors
{
   public static class RollImageProcess
   {
      public static async void RoleImageProcess(SocketCommandContext context)
      {
         var user = context.User;
         var attachments = context.Message.Attachments;
         string file = attachments.ElementAt(0).Filename;
         string url = attachments.ElementAt(0).Url;
         string imagePath = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\profile\{user.Username}.png";

         using (WebClient client = new WebClient())
         {
            client.DownloadFile(new Uri(url), imagePath);
         }

         var image = Image.FromFile(imagePath);

         int newwidthimg = 200;

         float AspectRatio = (float)image.Size.Width / (float)image.Size.Height;
         int newHeight = 200;

         Bitmap bitMAP1 = new Bitmap(newwidthimg, newHeight);

         Graphics imgGraph = Graphics.FromImage(bitMAP1);

         bitMAP1.imgQuality = CompositingQuality.HighQuality;

         imgGraph.SmoothingMode = SmoothingMode.HighQuality;

         imgGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

         var imgDimesions = new Rectangle(0, 0, newwidthimg, newHeight);

         imgGraph.DrawImage(image, imgDimesions);

         bitMAP1.Save(imagePath, ImageFormat.Jpeg);

         bitMAP1.Dispose();

         bitMAP1.Dispose();

         image.Dispose();


         using (var api = OcrApi.Create())
         {
            api.Init(Languages.English);
            string plainText = api.GetTextFromImage(imagePath);
            Console.WriteLine(plainText);
            Console.Read();
         }

      }
   }
}
