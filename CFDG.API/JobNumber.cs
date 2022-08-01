using System.IO;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.ApplicationServices;

namespace CFDG.API
{
    public enum JobNumberFormats
    {
        ShortNoHyphan,
        ShortHyphan,
        LongNoHyphan,
        LongHyphan
    }

    public class JobNumber
    {
        #region CONSTANTS

        /// <summary>
        /// Regex formats of job numbers.
        /// </summary>
        public class Formats
        {
            /// <summary>
            /// XXXXXXX (7 Numbers)
            /// </summary>
            public static string ShortNoHyphan
            {
                get
                {
                    return @"^\d{7}$";
                }
            }

            /// <summary>
            /// XX-XX-XXX (7 Numbers with hyphans after the 2nd and 4th)
            /// </summary>
            public static string ShortHyphan
            {
                get
                {
                    return @"^\d{2}-\d{2}-\d{3}";
                }
            }

            /// <summary>
            /// XXXXXXXXX (9 Numbers)
            /// </summary>
            public static string LongNoHyphan
            {
                get
                {
                    return @"^\d{9}$";
                }
            }

            /// <summary>
            /// XXXX-XX-XXX (9 Numbers with hyphans after the 4th and 6th)
            /// </summary>
            public static string LongHyphan
            {
                get
                {
                    return @"^\d{4}-\d{2}-\d{3}";
                }
            }
        }
        #endregion

        /// <summary>
        /// Check the input to determine if it is a job number.
        /// </summary>
        /// <param name="Input">The string to check.</param>
        /// <returns>True if a job number format, false if not.</returns>
        public static bool TryParse(string Input)
        {
            return TryParse(Input, JobNumberFormats.LongHyphan, out _);
        }

        /// <summary>
        /// Check the input to determine if it is a job number.
        /// </summary>
        /// <param name="Input">String to check.</param>
        /// <param name="Format">The format you wish to output in <paramref name="FormatNumber"/>.</param>
        /// <param name="FormatNumber">The formatted string if it can be transformed.</param>
        /// <returns>True if a job number format, false if not.</returns>
        public static bool TryParse(string Input, JobNumberFormats Format, out string FormatNumber)
        {
            if (string.IsNullOrEmpty(Input))
            {
                FormatNumber = null;
                return false;
            }

            //If the input string is not a correctly formatted number, return false
            if (
                !Regex.IsMatch(Input, Formats.ShortHyphan) &&
                !Regex.IsMatch(Input, Formats.ShortNoHyphan) &&
                !Regex.IsMatch(Input, Formats.LongHyphan) &&
                !Regex.IsMatch(Input, Formats.LongNoHyphan)
            )
            {
                FormatNumber = null;
                return false;
            }

            //make the string a long no hyphan format by default to easily format into 
            string formatted = Input.Replace("-", "");
            if (formatted.Length != 9)
            {
                formatted = formatted.Insert(0, "20");
            }

            //Format number and output.
            switch (Format)
            {
                case JobNumberFormats.ShortHyphan:
                {
                    FormatNumber = formatted.Remove(0, 2).Insert(2, "-").Insert(5, "-");
                    return true;
                }
                case JobNumberFormats.LongNoHyphan:
                {
                    FormatNumber = formatted;
                    return true;
                }
                case JobNumberFormats.LongHyphan:
                {
                    FormatNumber = formatted.Insert(4, "-").Insert(7, "-");
                    return true;
                }
                case JobNumberFormats.ShortNoHyphan:
                {
                    FormatNumber = formatted.Remove(0, 2);
                    return true;
                }
                default:
                {
                    FormatNumber = null;
                    return false;
                }
            }

        }

        /// <summary>
        /// Return the full path of the Job Number directory
        /// </summary>
        /// <param name="JobNumber">Job number to find.</param>
        /// <returns>string of path or null if not found.</returns>
        public static string GetPath(string JobNumber)
        {
            //Base variables setup
            string dir = Settings.GetValue<string>("General.DefaultProjectPath");

            //Error checks
            if (!TryParse(JobNumber, JobNumberFormats.LongHyphan, out string fullNumber))
            {
                //Log.AddWarning($"Job number {JobNumber} failed to correctly parse.");
                return null;
            }

            if (!Directory.Exists(dir))
            {
                //Log.AddWarning($"base path \"{dir}\" does not exist.");
                return null;
            }


            string[] parts = fullNumber.Split('-');
            dir = Path.Combine(dir, $@"{parts[0]}\{parts[1]}-{parts[0]}\{parts[1]}-{parts[2]}");

            if (!Directory.Exists(dir))
            {
                //Log.AddWarning($"Path \"{dir}\" does not exist.");
                return null;
            }

            return dir;
        }

        /// <summary>
        /// Parse number to another format.
        /// </summary>
        /// <param name="Input">Source job number.</param>
        /// <param name="Format">New format.</param>
        /// <returns>Reformatted job number.</returns>
        public static string Parse(string Input, JobNumberFormats Format)
        {
            if (TryParse(Input, Format, out string reformatted))
            {
                return reformatted;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Retrieve the job number from a file name.
        /// </summary>
        /// <param name="document">AutoCAD Document object</param>
        /// <returns>Job number found or empty if not found.</returns>
        public static string GetJobNumber(Document document)
        {
            string jobNumber = Path.GetFileNameWithoutExtension(document.Name);
            return ParseFileName(jobNumber);
        }

        /// <summary>
        /// Retrieve the job number from a file name.
        /// </summary>
        /// <param name="document">Document path</param>
        /// <returns>Job number found or empty if not found.</returns>
        public static string GetJobNumber(string document)
        {
            string jobNumber = Path.GetFileNameWithoutExtension(document);
            return ParseFileName(jobNumber);
        }

        /// <summary>
        /// Parses a file name to determine if the job number is in the filename.
        /// </summary>
        /// <param name="fileName">Filename to search for a job number.</param>
        /// <returns>Job number or <paramref name="empty"/> string</returns>
        private static string ParseFileName(string fileName)
        {
            dynamic match = Regex.Match(fileName, Settings.GetValue<string>("General.DefaultProjectNumber"));
            if (match.Success)
            {
                return match.Value;
            }
            return "";
        }
    }
}
