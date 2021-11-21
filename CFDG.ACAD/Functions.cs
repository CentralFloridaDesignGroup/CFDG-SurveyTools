using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Autodesk.AutoCAD.ApplicationServices;

namespace CFDG.ACAD.Functions
{
    /// <summary>
    /// Methods relating to the handling of AutoCAD Documents
    /// </summary>
    public class DocumentProperties
    {
        /// <summary>
        /// Retrieve the job number from a file name.
        /// </summary>
        /// <param name="document">AutoCAD Document object</param>
        /// <returns>Job number found or empty if not found.</returns>
        public static string GetJobNumber(Document document)
        {
            string jobNumber = Path.GetFileNameWithoutExtension(document.Name);
            return Parse(jobNumber);
        }

        /// <summary>
        /// Retrieve the job number from a file name.
        /// </summary>
        /// <param name="document">Document path</param>
        /// <returns>Job number found or empty if not found.</returns>
        public static string GetJobNumber(string document)
        {
            string jobNumber = Path.GetFileNameWithoutExtension(document);
            return Parse(jobNumber);
        }

        /// <summary>
        /// Parses a file name to determine if the job number is in the filename.
        /// </summary>
        /// <param name="fileName">Filename to search for a job number.</param>
        /// <returns>Job number or <paramref name="empty"/> string</returns>
        private static string Parse(string fileName)
        {
            dynamic match = Regex.Match(fileName, API.XML.ReadValue("General", "DefaultProjectNumber"));
            if (match.Success)
            {
                return match.Value;
            }
            return "";
        }
    }

    /// <summary>
    /// Functions relating to Imaging
    /// </summary>
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
