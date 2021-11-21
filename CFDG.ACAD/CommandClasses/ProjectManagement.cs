using System;
using System.Diagnostics;
using System.IO;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using CFDG.ACAD.Functions;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace CFDG.ACAD
{
    public class ProjectManagement
    {
        #region IMPORT JOB DATA

        #endregion

        #region OPEN SERVER FOLDERS

        /// <summary>
        /// Opens the specified folder of the active drawing's project.
        /// </summary>
        /// <param name="option">The sub-folder to open.</param>
        private static void OpenFolder(string option)
        {
            Autodesk.AutoCAD.ApplicationServices.Document doc = AcApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            // determines the job number of the active drawing.
            string jobNumber = DocumentProperties.GetJobNumber(doc);
            if (string.IsNullOrEmpty(jobNumber))
            {
                return;
            }

            // Gets the base path of the project and exits if it doesn't exist.
            string jobPath = API.JobNumber.GetPath(jobNumber);
            if (string.IsNullOrEmpty(jobPath))
            {
                ed.WriteMessage("\nA job number could not be determined.");
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
                ed.WriteMessage("\nProject folder was not found." + Environment.NewLine);
                return;
            }

            Process.Start(jobPath);
        }

        /// <summary>
        /// Open the active drawing's project folder.
        /// </summary>
        [CommandMethod("OpenProjectFolder")]
        public static void OpenProjectFolder()
        {
            OpenFolder("");
        }

        /// <summary>
        /// Open the active drawing's project computations folder.
        /// </summary>
        [CommandMethod("OpenCompFolder")]
        public static void OpenCompFolder()
        {
            OpenFolder("calc");
        }

        /// <summary>
        /// Open the active drawing's project submittals folder.
        /// </summary>
        [CommandMethod("OpenSubmittalFolder")]
        public static void OpenSubmittalFolder()
        {
            OpenFolder("submittal");
        }

        /// <summary>
        /// Open the active drawing's project submittals folder.
        /// </summary>
        [CommandMethod("OpenFieldDataFolder")]
        public static void OpenFieldDataFolder()
        {
            OpenFolder("fielddata");
        }

        #endregion

        #region XREF HELPERS

        // FIX: Refactor

        // FIX: Refactor and unify with similar functions.


        // FIX: Refactor and unify with similar functions.

        #endregion
    }
}
