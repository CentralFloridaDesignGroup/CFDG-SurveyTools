using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using CFDG.API;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using CFDG.ACAD.Common;

namespace CFDG.ACAD.CommandClasses.Calculations
{
    public class MeasureDown : ICommandMethod
    {

        [CommandMethod("Measuredowns", CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public void InitialCommand()
        {
            Document doc = AcApplication.DocumentManager.MdiActiveDocument;
            Point3d startPnt = UserInput.SelectPointInDoc("Select your base point: ");
            if (startPnt == new Point3d(-1, -1, -1))
            {
                return;
            }
            while (true)
            {
                string length = UserInput.GetStringFromUser("Enter the length of the measuredown: ");
                if (length == "" || !double.TryParse(length, out double lengthValue))
                {
                    doc.Editor.WriteMessage("\nThe entered value was not valid, please try again.");
                    break;
                }
                string angle = UserInput.GetStringFromUser("Enter the angle of the measuredown: ");
                if (angle == "" || !double.TryParse(angle, out double angleValue))
                {
                    doc.Editor.WriteMessage("\nThe entered value was not valid, please try again.");
                    break;
                }
                Point3d point = GetMeasureDownCoordinates(startPnt, lengthValue, angleValue);

                using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                {
                    // Open the Block table record for read
                    BlockTable acBlkTbl = tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    DBPoint acPoint = new DBPoint(point);
                    acPoint.SetDatabaseDefaults();
                    acBlkTblRec.AppendEntity(acPoint);
                    tr.AddNewlyCreatedDBObject(acPoint, true);
                    tr.Commit();
                }
            }
        }

        private static Point3d GetMeasureDownCoordinates(Point3d top, double distance, double angle)
        {
            Triangle measures = new Triangle(distance, angle);

            double drop = measures.SideA;
            double offset = measures.SideB;

            Vector2d vector = UserInput.SelectAngleInDoc("Select an angle: ", top, offset);

            return new Point3d(top.X + vector.X, top.Y + vector.Y, top.Z - drop);
        }

    }
}
