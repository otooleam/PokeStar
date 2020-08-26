using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PokeStar.ImageProcessors
{
   /// <summary>
   /// Processes an image.
   /// </summary>
   public static class ImageProcess
   {
      /// <summary>
      /// Scales an image to a desired size.
      /// </summary>
      /// <param name="image">Original image</param>
      /// <param name="width">New width of the image.</param>
      /// <param name="height">New height of the image.</param>
      /// <returns>Copy of the original image scalled to the desired size.</returns>
      public static Bitmap ScaleImage(Image image, int width, int height)
      {
         Rectangle destRect = new Rectangle(0, 0, width, height);
         Bitmap destImage = new Bitmap(width, height);

         destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

         using (Graphics graphics = Graphics.FromImage(destImage))
         {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (ImageAttributes wrapMode = new ImageAttributes())
            {
               wrapMode.SetWrapMode(WrapMode.TileFlipXY);
               graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
         }

         return destImage;
      }
   }
}