using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;

namespace CFDG.API.ACAD
{
    public class GetCogoPoint
    {
        public static List<ObjectId> GetPoint(bool multipleSelections, System.IntPtr handle)
        {
            Document acDocument = Application.DocumentManager.MdiActiveDocument;
            Editor acEditor = acDocument.Editor;
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
            Document acDocument = Application.DocumentManager.MdiActiveDocument;
            Editor acEditor = acDocument.Editor;
            Database acDatabase = acDocument.Database;
            CogoPoint cogoPoint;

            using (Transaction tr = acDatabase.TransactionManager.StartTransaction())
            {
                cogoPoint = (CogoPoint)objectId.GetObject(OpenMode.ForRead);
                tr.Commit();
            }
            return cogoPoint;
        }

        public static ObjectId[] selectPoint(bool isMultiple = false)
        {
            Document acDocument = Application.DocumentManager.MdiActiveDocument;
            Editor acEditor = acDocument.Editor;
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
