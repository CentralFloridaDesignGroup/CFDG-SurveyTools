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
            ObjectId pointId = selectPoint();
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

        private CogoPoint GetCogoByID(ObjectId objectId)
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

        private ObjectId selectPoint()
        {
            (Document acDocument, Editor acEditor) = UserInput.GetCurrentDocSpace();
            Database acDatabase = acDocument.Database;

            var typeValue = new TypedValue[]
                    {
                            new TypedValue((int)DxfCode.Start, "AECC_COGO_POINT")
                    };
            var acSelectionFilter = new SelectionFilter(typeValue);

            var acPSO = new PromptSelectionOptions
            {
                SingleOnly = true,
                SinglePickInSpace = true
            };
            PromptSelectionResult acPSR;

            acPSR = acEditor.GetSelection(acPSO, acSelectionFilter);
            if (acPSR.Status != PromptStatus.OK)
            {
                acEditor.WriteMessage("The operation was cancelled by the user.\n");
                return ObjectId.Null;
            }
            ObjectId objectId = acPSR.Value[0].ObjectId;
            return objectId;
        }
    }
}
