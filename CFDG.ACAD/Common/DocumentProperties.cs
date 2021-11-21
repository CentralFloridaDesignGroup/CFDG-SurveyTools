using System.IO;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.ApplicationServices;

namespace CFDG.ACAD.Common
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
}
