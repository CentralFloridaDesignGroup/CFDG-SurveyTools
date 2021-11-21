using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using CFDG.ACAD.Common;

namespace CFDG.ACAD.CommandClasses.ProjectManagement
{
    public class OpenProjectFolder : ICommandMethod
    {
        #region Public command methods

        [CommandMethod("OpenProjectFolder")]
        public void InitialCommand()
        {
            OpenFolder("");
        }

        /// <summary>
        /// Open the active drawing's project computations folder.
        /// </summary>
        [CommandMethod("OpenCompFolder")]
        public void OpenCompFolder()
        {
            OpenFolder("calc");
        }

        /// <summary>
        /// Open the active drawing's project submittals folder.
        /// </summary>
        [CommandMethod("OpenSubmittalFolder")]
        public void OpenSubmittalFolder()
        {
            OpenFolder("submittal");
        }

        /// <summary>
        /// Open the active drawing's project submittals folder.
        /// </summary>
        [CommandMethod("OpenFieldDataFolder")]
        public void OpenFieldDataFolder()
        {
            OpenFolder("fielddata");
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Opens the specified folder of the active drawing's project.
        /// </summary>
        /// <param name="option">The sub-folder to open.</param>
        private static void OpenFolder(string option)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();

            // determines the job number of the active drawing.
            string jobNumber = DocumentProperties.GetJobNumber(acVariables.Document);
            if (string.IsNullOrEmpty(jobNumber))
            {
                Logging.Warning("Could not find a job number from the file name (is the drawing saved?).");
                return;
            }

            // Gets the base path of the project and exits if it doesn't exist.
            string jobPath = API.JobNumber.GetPath(jobNumber);
            if (string.IsNullOrEmpty(jobPath))
            {
                Logging.Warning("A job path could not be found.");
                return;
            }

            // determine the path
            switch (option.ToLower())
            {
                case "calc":
                {
                    jobPath += @"\Calc";
                    break;
                }
                case "submittal":
                {
                    jobPath += @"\Submittal";
                    break;
                }
                case "fielddata":
                {
                    jobPath += @"\Field Data";
                    break;
                }
                default:
                    break;
            }

            if (!Directory.Exists(jobPath))
            {
                Logging.Warning("The specific folder does not exist.");
                return;
            }

            Process.Start(jobPath);
        }

        #endregion
    }
}
