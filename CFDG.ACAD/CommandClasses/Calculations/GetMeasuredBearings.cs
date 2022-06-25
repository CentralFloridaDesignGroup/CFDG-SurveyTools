using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Aec.DatabaseServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using CFDG.ACAD.Common;
using CFDG.API.Calcs;

namespace CFDG.ACAD.CommandClasses.Calculations
{
    public class GetMeasuredBearings : ICommandMethod
    {
        internal double RotationValue { get; set; }

        [CommandMethod("GetMeasuredBearings", CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public void InitialCommand()
        {
            Point3d startPoint = GetPoint("Select your base point: ");
            if (startPoint.X == -1000 && startPoint.Y == -1000)
            {
                Logging.Debug("Cancelling command.");
                return;
            }
            Point3d endPoint = GetPoint("Select your rotation point: ", startPoint);
            if (endPoint.X == -1000 && endPoint.Y == -1000)
            {
                Logging.Debug("Cancelling command.");
                return;
            }
            API.Calcs.LineInfo info = API.Calcs.Lines.CalculateLine(startPoint, endPoint);
            Logging.Debug($"Bearing: {info.Bearing}, Distance: {info.Length}, Azimuth: {info.Azimuth}");
            var baseAzimuth = getBaseAzimuth();

            if (RotationValue == -1)
            {
                Logging.Debug("Cancelling command.");
                return;
            }
            RotationValue = baseAzimuth - info.Azimuth;
            Logging.Debug($"Angle difference: {Math.Round(RotationValue, 6)}");
            GatherReferencePoints();
        }

        internal double getBaseAzimuth()
        {
            string reference = getString("Enter the plat bearing: ");
            string convertedRef = API.Calcs.Angles.ConvertBearing(reference);
            if (reference == "*keyword*" || convertedRef == "")
            {
                Logging.Info("Invalid value, please enter again.");
                return getBaseAzimuth();
            }
            if (reference == "cancelled")
            {
                return -1;
            }
            return Math.Round(API.Calcs.Angles.BearingToAzimuth(convertedRef), 6);
        }

        internal Point3d GetPoint(string prompt)
        {
            return GetPoint(prompt, API.Helpers.Points.Null3dPoint);
        }

        internal Point3d GetPoint(string prompt, Point3d refPoint)
        {
            PromptPointResult result = UserInput.GetPoint(prompt, refPoint, true);
            if (result == null || result.Status == PromptStatus.Cancel)
            {
                Logging.Info("Action cancelled by user.");
                return new Point3d(-1000, -1000, -1000);
            }
            if (result.Status == PromptStatus.Keyword && result.Value == API.Helpers.Points.Base3dPoint)
            {
                Logging.Info("Invalid point, please select another point.");
                GetPoint(prompt, refPoint);
            }
            return result.Value;
        }

        internal void GatherReferencePoints()
        {
            List<Point3d> referencePoints = new List<Point3d>(); //Point list
            bool commandActive = true;
            Point3d previousPoint;
            PromptPointResult result;
            //List<Entity> entities = new List<Entity>(); //Reference line list - deleted on command completion.

            while (commandActive)
            {
                previousPoint = (referencePoints.Count > 0) ? referencePoints[referencePoints.Count - 1] : API.Helpers.Points.Null3dPoint;
                result = UserInput.GetPoint("Select the next measured point: ", previousPoint);
                if (result == null)
                {
                    Logging.Error("Something went wrong, cancelling.");
                    commandActive = false;
                    return;
                }
                if (result.Status == PromptStatus.Keyword && result.StringResult == "_u")
                {
                    if (referencePoints.Count < 1)
                    {
                        Logging.Info("Cannot remove, no points in the list, or press ENTER to finish.");
                        continue;
                    }
                    else
                    {
                        Logging.Info($"Point removed, {referencePoints.Count} total.");
                        referencePoints.RemoveAt(referencePoints.Count - 1);
                        Logging.Info($"Point removed, {referencePoints.Count} total.");
                        continue;
                    }
                }
                //This is if you press "ENTER", the result just doesn't say enter.
                if (result.Status == PromptStatus.Keyword && string.IsNullOrEmpty(result.StringResult))
                {
                    commandActive = false;
                    break;
                }
                if (result.Status == PromptStatus.Cancel)
                {
                    commandActive = false;
                    return;
                }
                referencePoints.Add(result.Value);
                Logging.Info($"Point added, {referencePoints.Count} total.");
            }

            ProcessData(referencePoints);
        }

        private void ProcessData(List<Point3d> referencePoints)
        {
            Logging.Info("Starting processing of points.");
            List<ProcessedLine> processedLines = new List<ProcessedLine>();

            for (int i = 0; i < referencePoints.Count; i++)
            {
                Logging.Debug($"Processing line: {i + 1}");
                if (i == referencePoints.Count - 1)
                {

                    processedLines.Add(ProcessLine(referencePoints[i], referencePoints[0])); //Goes from last to first
                }
                else
                {
                    processedLines.Add(ProcessLine(referencePoints[i], referencePoints[i + 1])); //Goes from n to n+1.
                }
            }
            foreach (var line in processedLines)
            {
                CreateMText(line);
            }
        }

        private ProcessedLine ProcessLine(Point3d start, Point3d end)
        {
            ProcessedLine result = new ProcessedLine();
            Logging.Debug($"Start: {start}; End: {end}");
            result.lineInfo = Lines.CalculateLine(start, end);
            result.lineInfo.Azimuth += RotationValue;
            Logging.Debug($"Corrected Azimuth: {result.lineInfo.Azimuth}; Bearing: {result.lineInfo.Bearing}");
            result.CenterPoint = new Point3d((start.X + end.X) / 2, (start.Y + end.Y) / 2, 0);
            return result;
        }

        internal struct ProcessedLine
        {
            internal API.Calcs.LineInfo lineInfo;

            internal Point3d CenterPoint;

            internal ProcessedLine(API.Calcs.LineInfo info, Point3d center)
            {
                lineInfo = info;
                CenterPoint = center;
            }
        }

        private void CreateMText(ProcessedLine pLine)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            using(Transaction acTrans = acVariables.Database.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = acTrans.GetObject(acVariables.Database.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord blockTableRecord = acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (MText mTextObj = new MText())
                {
                    mTextObj.Location = pLine.CenterPoint;
                    mTextObj.Width = 0;
                    mTextObj.Contents = $"{pLine.lineInfo.Bearing} {pLine.lineInfo.Length.ToString("#.00")}'";
                    mTextObj.Layer = "defpoints";
                    mTextObj.Attachment = AttachmentPoint.MiddleCenter;
                    mTextObj.TextHeight = 1.6;

                    blockTableRecord.AppendEntity(mTextObj);
                    acTrans.AddNewlyCreatedDBObject(mTextObj, true);
                }

                acTrans.Commit();
            }
        }

        internal string getString(string prompt)
        {
            string value = UserInput.GetStringFromUser(prompt);
            if (value == "*keyword*")
            {
                Logging.Info("Invalid value, please select again.");
                return getString(prompt);
            }
            if (string.IsNullOrEmpty(value))
            {
                Logging.Info("Action cancelled.");
                return "cancelled";
            }
            return value;
        }
    }
}
