﻿using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;

namespace CFDG.ACAD.CommandClasses.Misc
{
    public class GetCogoPoint
    {


        [CommandMethod("GetCogoPoint", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
        public void InitialCommand()
        {
            (_, Editor acEditor) = UserInput.GetCurrentDocSpace();
            ObjectId[] pointIdList = selectPoint();
            ObjectId pointId = pointIdList[0];
            if (pointId == ObjectId.Null)
            {
                acEditor.WriteMessage("The point did not have a valid object id. Please run the AUDIT command.\n");
                return;
            }
            CogoPoint cogoPoint = GetCogoByID(pointId);
            if (cogoPoint == null)
            {
                acEditor.WriteMessage("Could not find a point, please report this issue.\n");
                return;
            }

            acEditor.WriteMessage($"Point: {cogoPoint.PointNumber} | Easting: {cogoPoint.Easting} | Northing: {cogoPoint.Northing} | Elevation: {cogoPoint.Elevation} | Description: {cogoPoint.RawDescription}\n");
        }

        public static List<ObjectId> GetPoint(bool multipleSelections, System.IntPtr handle)
        {
            (Document acDocument, Editor acEditor) = UserInput.GetCurrentDocSpace();
            List<ObjectId> pointIds = new List<ObjectId> { };

            bool firstPoint = true;

            using (acEditor.StartUserInteraction(handle))
            using (acDocument.LockDocument())
            {
                while (multipleSelections || firstPoint)
                {
                    var pointIdList = selectPoint(true);
                    foreach (var id in pointIdList)
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
            (Document acDocument, Editor acEditor) = UserInput.GetCurrentDocSpace();
            Database acDatabase = acDocument.Database;
            CogoPoint cogoPoint;

            using (Transaction tr = acDatabase.TransactionManager.StartTransaction())
            {
                cogoPoint = (CogoPoint)objectId.GetObject(OpenMode.ForRead);
                tr.Commit();
            }
            return cogoPoint;
        }

        private static ObjectId[] selectPoint(bool isMultiple = false)
        {
            (Document acDocument, Editor acEditor) = UserInput.GetCurrentDocSpace();
            PromptSelectionOptions acPSO;

            var typeValue = new TypedValue[]
                    {
                            new TypedValue((int)DxfCode.Start, "AECC_COGO_POINT")
                    };
            var acSelectionFilter = new SelectionFilter(typeValue);

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

            acPSR = acEditor.GetSelection(acPSO, acSelectionFilter);
            if (acPSR.Status != PromptStatus.OK)
            {
                acEditor.WriteMessage("The operation was cancelled by the user.\n");
                var emptyList = new ObjectId[] { ObjectId.Null };
                return emptyList;
            }
            ObjectId[] objectIds = acPSR.Value.GetObjectIds();
            return objectIds;
        }
    }
}
