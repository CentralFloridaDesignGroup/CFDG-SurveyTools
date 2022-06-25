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
                PromptPointResult selectResult = UserInput.GetPoint("Please select a point: ");
                if (selectResult == null)
                {
                    Logging.Error("There was an error, please try again or submit an issue ticket.");
                    return;
                }
                if (selectResult.Status == PromptStatus.Cancel)
                {
                    return;
                }
                if (selectResult.Status == PromptStatus.OK && selectResult.Value != API.Helpers.Points.Null3dPoint)
                {
                    UserInput.AddPointToDrawing(selectResult.Value);
                    continue;
                }
                return;
            }
        }
    }
}
