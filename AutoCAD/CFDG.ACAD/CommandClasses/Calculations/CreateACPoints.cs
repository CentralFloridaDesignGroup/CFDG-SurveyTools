using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using CFDG.ACAD.Common;

namespace CFDG.ACAD.CommandClasses.Calculations
{
    public class CreateACPoints : ICommandMethod
    {
        [CommandMethod("ACPoints", CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public void InitialCommand()
        {
            while (true)
            {
                Point3d point = UserInput.SelectPointInDoc("Please select a point: ");
                Logging.Debug($"Raw value: {point}");
                Logging.Info($"Value of entry: [{point.X:F3}', {point.Y:F3}', {point.Z:F3}'].");
                if (point == new Point3d(-1,-1,-1))
                {
                    return;
                }
                if (point == new Point3d(0, 0, 0))
                {
                    Logging.Warning("Not a valid point.\n");
                }
                else
                {
                    UserInput.AddPointToDrawing(point, BlockTableRecord.ModelSpace);
                }
            }
        }
    }
}
