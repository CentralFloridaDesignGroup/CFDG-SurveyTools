using System.Collections.Generic;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;
using CFDG.ACAD.Common;

namespace CFDG.ACAD.CommandClasses.Misc
{
    public class GetCogoPoint : ICommandMethod
    {


        [CommandMethod("GetCogoPoint", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
        public void InitialCommand()
        {
            ObjectId[] pointIdList = SelectPoint();
            ObjectId pointId = pointIdList[0];
            if (pointId == ObjectId.Null)
            {
                Logging.Error("The point did not have a valid object id. Please run the AUDIT command.\n");
                return;
            }
            CogoPoint cogoPoint = GetCogoByID(pointId);
            if (cogoPoint == null)
            {
                Logging.Critical("Could not find a point, please report this issue.\n", true);
                return;
            }

            Logging.Info($"Point: {cogoPoint.PointNumber} | Easting: {cogoPoint.Easting} | Northing: {cogoPoint.Northing} | Elevation: {cogoPoint.Elevation} | Description: {cogoPoint.RawDescription}\n");
            Clipboard.SetText($"{cogoPoint.Easting:0.000}\t{cogoPoint.Northing:0.000}\t{cogoPoint.Elevation:0.000}\t{cogoPoint.RawDescription}", TextDataFormat.Text);
        }

        [CommandMethod("GetLocation", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
        public void GetCogoText()
        {
            PromptPointResult result = UserInput.GetPoint("Select a point: ", false);
            if (result.Status == PromptStatus.OK && result.Value != API.Helpers.Points.Null3dPoint)
            {
                Clipboard.SetText($"{result.Value.X}\t{result.Value.Y}\t{result.Value.Z}", TextDataFormat.Text);
                Logging.Info("Location copied to clipboard.");
                return;
            }
        }

        public static List<ObjectId> GetPoint(bool multipleSelections, System.IntPtr handle)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            List<ObjectId> pointIds = new List<ObjectId> { };

            bool firstPoint = true;

            using (acVariables.Editor.StartUserInteraction(handle))
            using (acVariables.Document.LockDocument())
            {
                while (multipleSelections || firstPoint)
                {
                    ObjectId[] pointIdList = SelectPoint(true);
                    foreach (ObjectId id in pointIdList)
                    {
                        if (!pointIds.Contains(id))
                        {
                            pointIds.Add(id);
                        }
                    }
                    firstPoint = false;
                }
            }

            return pointIds;
        }

        public static CogoPoint GetCogoByID(ObjectId objectId)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            CogoPoint cogoPoint;

            using (Transaction tr = acVariables.Database.TransactionManager.StartTransaction())
            {
                cogoPoint = (CogoPoint)objectId.GetObject(OpenMode.ForRead);
                tr.Commit();
            }
            return cogoPoint;
        }

        private static ObjectId[] SelectPoint(bool isMultiple = false)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            PromptSelectionOptions acPSO;

            TypedValue[] typeValue = new TypedValue[]
                    {
                            new TypedValue((int)DxfCode.Start, "AECC_COGO_POINT")
                    };
            SelectionFilter acSelectionFilter = new SelectionFilter(typeValue);

            if (!isMultiple)
            {
                acPSO = new PromptSelectionOptions
                {
                    SingleOnly = true,
                    SinglePickInSpace = true,
                };
            }
            else
            {
                acPSO = new PromptSelectionOptions
                {
                    SingleOnly = false,
                    SinglePickInSpace = false,
                };
            }
            PromptSelectionResult acPSR;

            acPSR = acVariables.Editor.GetSelection(acPSO, acSelectionFilter);
            if (acPSR.Status != PromptStatus.OK)
            {
                Logging.Info("The operation was cancelled by the user.\n");
                ObjectId[] emptyList = new ObjectId[] { ObjectId.Null };
                return emptyList;
            }
            ObjectId[] objectIds = acPSR.Value.GetObjectIds();
            return objectIds;
        }
    }
}
