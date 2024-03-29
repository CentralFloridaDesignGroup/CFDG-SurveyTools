﻿using System;
using System.IO;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using CFDG.API;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace CFDG.ACAD.Common
{
    public class UserInput
    {
        #region Private Properties

        public static bool RequireZenethCheck { get; set; }

        #endregion

        #region Public Methods

        [Obsolete("GetStringFromUser is obsolete, use GetText instead.")]
        public static string GetStringFromUser(string message)
        {
            return GetStringFromUser(message, false);
        }

        [Obsolete("GetStringFromUser is obsolete, use GetText instead.")]
        /// <summary>
        /// Get string from user in AutoCAD.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string GetStringFromUser(string message, bool allowSpaces)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();

            PromptStringOptions pso = new PromptStringOptions(message)
            {
                AllowSpaces = allowSpaces
            };

            PromptResult tr = acVariables.Editor.GetString(pso);
            if (tr.Status == PromptStatus.Cancel)
            {
                return "";
            }
            if (tr.Status == PromptStatus.Keyword)
            {
                return "*keyword*";
            }
            return tr.StringResult;
        }

        /// <summary>
        /// Get text input from user in AutoCAD
        /// </summary>
        /// <param name="prompt">Message for prompt</param>
        /// <returns>PromptResult containing the results of the prompt.</returns>
        public static PromptResult GetText(string prompt)
        {
            return GetText(prompt, true, out _);
        }

        /// <summary>
        /// Get text input from user in AutoCAD
        /// </summary>
        /// <param name="prompt">Message for prompt</param>
        /// <param name="result">The StringResult of the prompt.</param>
        /// <returns>PromptResult containing the results of the prompt.</returns>
        public static PromptResult GetText(string prompt, out string result)
        {
            return GetText(prompt, true, out result);
        }

        /// <summary>
        /// Get text input from user in AutoCAD
        /// </summary>
        /// <param name="prompt">Message for prompt</param>
        /// <param name="allowSpaces">Allow the user to use spaces instead of letting space advance the command.</param>
        /// <returns>PromptResult containing the results of the prompt.</returns>
        public static PromptResult GetText(string prompt, bool allowSpaces)
        {
            return GetText(prompt, allowSpaces, out _);
        }

        /// <summary>
        /// Get text input from user in AutoCAD
        /// </summary>
        /// <param name="prompt">Message for prompt</param>
        /// <param name="allowSpaces">Allow the user to use spaces instead of letting space advance the command.</param>
        /// <param name="result">The StringResult of the prompt.</param>
        /// <returns>PromptResult containing the results of the prompt.</returns>
        public static PromptResult GetText(string prompt, bool allowSpaces, out string result)
        {
            AcVariablesStruct acVariables = GetCurrentDocSpace();

            PromptStringOptions stringOptions = new PromptStringOptions(prompt) { AllowSpaces = allowSpaces };
            PromptResult tr = acVariables.Editor.GetString(stringOptions);
            result = tr.StringResult;
            return tr;
        }

        [Obsolete]
        /// <summary>
        /// Select a point in the current document (3D)
        /// </summary>
        /// <param name="message">Message for the prompt</param>
        /// <returns>A 3D point</returns>
        public static Point3d SelectPointInDoc(string message)
        {
            return SelectPointInDoc(message, new Point3d(-1, -1, -1));
        }

        [Obsolete]
        /// <summary>
        /// Select a point in the current document (3D)
        /// </summary>
        /// <param name="message">Message for the prompt</param>
        /// <param name="basePoint">A base point for reference</param>
        /// <returns>A 3D point</returns>
        public static Point3d SelectPointInDoc(string message, Point3d basePoint)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();

            if (VerifyZenthValues(true))
            {
                PromptPointOptions ppo = new PromptPointOptions($"\n{message}")
                {
                    AllowArbitraryInput = true,
                    AllowNone = false
                };
                if (basePoint != API.Helpers.Points.Null3dPoint)
                {
                    ppo.BasePoint = basePoint;
                    ppo.UseBasePoint = true;
                    ppo.UseDashedLine = true;
                }

                PromptPointResult pr = acVariables.Editor.GetPoint(ppo);
                Logging.Debug($"Prompt return: {pr.Value}; Prompt status: {pr.Status}; StringResult: {pr.StringResult}");
                if (pr.Status == PromptStatus.OK && pr.Value != new Point3d(0, 0, 0))
                {
                    return pr.Value;
                }
                if (pr.Status == PromptStatus.Keyword)
                {
                    if (pr.StringResult == "_u")
                    {
                        return new Point3d(-1011, -1011, -1011);
                    }
                    return new Point3d(-1001, -1001, -1001);
                }
                if (pr.Status == PromptStatus.Cancel)
                {
                    return new Point3d(-1, -1, -1);
                }
            }
            return new Point3d(-1, -1, -1);
        }

        /// <summary>
        /// Requests the user to select a point in the AutoCAD application.
        /// </summary>
        /// <param name="prompt">The message when selecting a point.</param>
        /// <returns>PromptPointResult containing the selected point and relevant information.</returns>
        public static PromptPointResult GetPoint(string prompt)
        {
            return GetPoint(prompt, new Point3d(-1, -1, -1), true);
        }

        /// <summary>
        /// Requests the user to select a point in the AutoCAD application.
        /// </summary>
        /// <param name="prompt">The message when selecting a point.</param>
        /// <param name="basePoint">The base point where AutoCAD draws a temporary line.</param>
        /// <returns>PromptPointResult containing the selected point and relevant information.</returns>
        public static PromptPointResult GetPoint(string prompt, Point3d basePoint)
        {
            return GetPoint(prompt, basePoint, true);
        }

        /// <summary>
        /// Requests the user to select a point in the AutoCAD application.
        /// </summary>
        /// <param name="prompt">The message when selecting a point.</param>
        /// <param name="prefer2dMode">Determine if the osnapz setting should be on or off.</param>
        /// <returns>PromptPointResult containing the selected point and relevant information.</returns>
        public static PromptPointResult GetPoint(string prompt, bool prefer2dMode)
        {
            return GetPoint(prompt, new Point3d(-1, -1, -1), prefer2dMode);
        }

        /// <summary>
        /// Requests the user to select a point in the AutoCAD application.
        /// </summary>
        /// <param name="prompt">The message when selecting a point.</param>
        /// <param name="basePoint">The base point where AutoCAD draws a temporary line.</param>
        /// <param name="prefer2dMode">Determine if the osnapz setting should be on or off.</param>
        /// <returns>PromptPointResult containing the selected point and relevant information.</returns>
        public static PromptPointResult GetPoint(string prompt, Point3d basePoint, bool prefer2dMode)
        {
            AcVariablesStruct acVariables = GetCurrentDocSpace();
            if (!VerifyZenthValues(prefer2dMode))
            {
                Logging.Error($"The command preferrs to have the osnapz setting to {(prefer2dMode ? "1" : "0")}. Cancelling the command.");
                return null;
            }

            PromptPointOptions ppo = new PromptPointOptions($"\n{prompt}")
            {
                AllowArbitraryInput = true,
                AllowNone = false
            };
            if (basePoint != API.Helpers.Points.Null3dPoint)
            {
                ppo.BasePoint = basePoint;
                ppo.UseBasePoint = true;
                ppo.UseDashedLine = true;
            }
            Logging.Debug($"BasePoint: {basePoint}; prefer2dMode: {prefer2dMode}");

            PromptPointResult result = acVariables.Editor.GetPoint(ppo);

            Logging.Debug($"Status: {result.Status}; Value: {result.Value}; StringResult: {result.StringResult}");
            return result;
        }

        /// <summary>
        /// Select a angle in the current document
        /// </summary>
        /// <param name="message">Message for the prompt</param>
        /// <param name="basePoint">A base point for reference</param>
        /// <param name="distance">The distance to break apart.</param>
        /// <returns>A 2D Vector</returns>
        public static Vector2d SelectAngleInDoc(string message, Point3d basePoint, double distance)
        {
            var angle = SelectAngleInDoc(message, basePoint);

            Triangle measures = new Triangle(distance, angle);

            return new Vector2d(measures.SideA, measures.SideB);
        }

        public static double SelectAngleInDoc(string message, Point2d basePoint)
        {
            return SelectAngleInDoc(message, new Point3d(basePoint.X, basePoint.Y, 0));
        }

        public static double SelectAngleInDoc(string message, Point3d basePoint)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();

            PromptAngleOptions pao = new PromptAngleOptions(message)
            {
                AllowArbitraryInput = true,
                AllowNone = true,
                AllowZero = true,
                BasePoint = basePoint,
                UseBasePoint = true,
                UseDashedLine = true,
                DefaultValue = 0,
                UseDefaultValue = true
            };
            PromptDoubleResult ar = acVariables.Editor.GetAngle(pao);
            if (ar.Status == PromptStatus.Cancel)
            {
                return -1;
            }

#pragma warning disable IDE0047 // Remove unnecessary parentheses
            return (180 / Math.PI) * ar.Value;
#pragma warning restore IDE0047 // Remove unnecessary parentheses
        }

        public static void AddObjectToDrawing(Entity entity)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();

            using (Transaction acTrans = acVariables.Database.TransactionManager.StartTransaction())
            {
                // Open the Block table record for read
                BlockTable acBlkTbl = acTrans.GetObject(acVariables.Database.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                entity.SetDatabaseDefaults();
                acBlkTblRec.AppendEntity(entity);
                acTrans.AddNewlyCreatedDBObject(entity, true);
                acTrans.Commit();
            }
        }

        public static void AddPointToDrawing(Point3d point)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            using (Transaction trans = acVariables.Database.TransactionManager.StartTransaction())
            {
                // Open the Block table record for read
                BlockTable acBlkTbl = trans.GetObject(acVariables.Database.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                DBPoint pointObj = new DBPoint(point);
                pointObj.SetDatabaseDefaults();
                acBlkTblRec.AppendEntity(pointObj);
                trans.AddNewlyCreatedDBObject(pointObj, true);
                trans.Commit();
            }
        }

        /// <summary>
        /// Check if current work space is the model space.
        /// </summary>
        /// <returns>true if in model space, false if in a layout page.</returns>
        public static bool IsInModel()
        {
            return AcApplication.DocumentManager.MdiActiveDocument.Database.TileMode;
        }

        /// <summary>
        /// Check if current work space is a layout page.
        /// </summary>
        /// <returns>true if in a layout page, false if in model space.</returns>
        public static bool IsInLayout()
        {
            return !IsInModel();
        }

        /// <summary>
        /// Get the project folder
        /// </summary>
        /// <param name="AcDoc"></param>
        public static bool CheckForProjectFolder(Document AcDoc, out string projectFolder)
        {
            // determines the job number of the active drawing.
            string jobNumber = API.JobNumber.GetJobNumber(AcDoc);
            if (string.IsNullOrEmpty(jobNumber))
            {
                Logging.Warning("Could not determine the project number.");
                projectFolder = string.Empty;
                return false;
            }

            // Gets the base path of the project and exits if it doesn't exist.
            string jobPath = API.JobNumber.GetPath(jobNumber);
            if (string.IsNullOrEmpty(jobPath))
            {
                Logging.Warning("Could not determine the project folder.");
                projectFolder = string.Empty;
                return false;
            }

            // Check for the project directory. If it doesn't exist, exit the command.
            if (!Directory.Exists(jobPath))
            {
                Logging.Warning("Could not find the project directory on the server.");
                projectFolder = string.Empty;
                return false;
            }
            projectFolder = jobPath;
            return true;
        }

        /// <summary>
        /// Get the current document and editor
        /// </summary>
        /// <returns>Document and Editor</returns>
        public static AcVariablesStruct GetCurrentDocSpace()
        {
            AcVariablesStruct acVariables;
            acVariables.Document = AcApplication.DocumentManager.MdiActiveDocument;
            if (acVariables.Document == null)
            {
                acVariables.Editor = null;
                acVariables.Database = null;
                return acVariables;
            }
            acVariables.Editor = acVariables.Document.Editor;
            acVariables.Database = acVariables.Document.Database;
            return acVariables;
        }

        #endregion

        #region Private Methods

        //TODO: Add more reliability 
        /// <summary>
        /// Verify the ZenethSnap (OSnapZ) is set to the preferred setting.
        /// </summary>
        /// <param name="preferredValue">false - disabled, true - enabled</param>
        /// <returns>true if match or override, false if cancel.</returns>
        private static bool VerifyZenthValues(bool preferredValue)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            bool OSnapZ = Convert.ToBoolean(AcApplication.TryGetSystemVariable("OSnapZ")); //0 [false] - Disabled / 1 [true] - enabled

            // If the preferred value is equal to the set value, return true (passed) or return true (passed) if not required.
            if (preferredValue == OSnapZ || !RequireZenethCheck)
            {
                RequireZenethCheck = false;
                return true;
            }

            //FEATURE: Enable temporary disablement of AutoCAD variables.
            PromptKeywordOptions pkwo = new PromptKeywordOptions($"OSnapZ is {(preferredValue ? "disabled" : "enabled")}, Do you want to continue?");
            pkwo.Keywords.Add("Yes");
            pkwo.Keywords.Add("No");
            pkwo.Keywords.Default = "Yes";

            //Ask for user input.
            PromptResult KeywordResult = acVariables.Editor.GetKeywords(pkwo);
            switch (KeywordResult.StringResult)
            {
                case "No":
                    RequireZenethCheck = false;
                    return false;
                default:
                    RequireZenethCheck = false;
                    return true;
            }
        }

        #endregion
    }
}
