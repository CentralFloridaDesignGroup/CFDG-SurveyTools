using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace CFDG.ACAD.Common
{
    public class Imaging
    {
        /// <summary>
        /// Creates an ImageSource object out of the <paramref name="bitmap"/> provided.
        /// </summary>
        /// <param name="bitmap">Bitmap to convert</param>
        /// <returns>ImageSource of Bitmap</returns>
        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        /// <summary>
        /// Creates an ImageSource object out of the <paramref name="bitmaps"/> provided. <paramref name="Bitmaps"/> are stacked on top of each other in provided order.
        /// </summary>
        /// <param name="bitmaps">Bitmaps to convert</param>
        /// <returns>ImageSource of Bitmaps stacked</returns>
        public static BitmapImage BitmapToImageSource(params Bitmap[] bitmaps)
        {
            if (bitmaps.Length == 0)
            {
                return new BitmapImage();
            }

            int width = bitmaps.Max(map => map.Width);
            int height = bitmaps.Max(map => map.Height);
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                foreach (Bitmap map in bitmaps)
                {
                    g.DrawImage(map, Point.Empty);
                }
            }
            return BitmapToImageSource(result);
        }
    }
}
