using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace CFDG.ACAD.CommandClasses.Calculations
{
    public class CreateACPoints : ICommandMethod
    {
        [CommandMethod("CreateACPoints", CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public void InitialCommand()
        {
            (Document _, Editor AcEditor) = UserInput.GetCurrentDocSpace();

            while (true)
            {
                Point3d point = UserInput.SelectPointInDoc("Please select a point: ");
                AcEditor.WriteMessage($"Value of entry: [{point.X:F3}', {point.Y:F3}', {point.Z:F3}'].\n");
                if (point == new Point3d(-1,-1,-1))
                {
                    return;
                }
                if (point == new Point3d(0, 0, 0))
                {
                    AcEditor.WriteMessage("Not a valid point.\n");
                }
                else
                {
                    UserInput.AddPointToDrawing(point, BlockTableRecord.ModelSpace);
                }
            }
        }
    }
}
