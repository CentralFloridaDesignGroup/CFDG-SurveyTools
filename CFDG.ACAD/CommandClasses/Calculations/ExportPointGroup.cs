using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using CivilApp = Autodesk.Civil.ApplicationServices.CivilApplication;

namespace CFDG.ACAD.CommandClasses.Calculations
{
    public class ExportPointGroup : ICommandMethod
    {
        internal static List<string> controlCodes = new List<string>
        {
            "IRC=",
            "IR=",
            "NL=",
            "NLD=",
            "CM=",
            "PIP=",
            "OM="
        };

        [CommandMethod("ExportPointGroup")]
        public void InitialCommand()
        {
            GetPointGroupCollection();
        }

        private void GetPointGroupCollection()
        {
            PointGroupCollection pgCollection;
            using (Transaction AcTran = AcApplication.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                (Document AcDocument, Editor AcEditor) = UserInput.GetCurrentDocSpace();

                if (!AcDocument.IsNamedDrawing)
                {
                    AcEditor.WriteMessage("\nThe file has not been saved. Cannot export points until the file has the job number in its name.");
                    return;
                }

                if (string.IsNullOrEmpty(Functions.DocumentProperties.GetJobNumber(AcDocument)))
                {
                    AcEditor.WriteMessage("\nNo job number was detected for the project.");
                    return;
                }

                pgCollection = CivilApp.ActiveDocument.PointGroups;
                List<string> groups = new List<string> { };

                foreach (ObjectId group in pgCollection)
                {
                    PointGroup pointGroup = (PointGroup)group.GetObject(OpenMode.ForRead);
                    if (pointGroup.Name.ToLower() != "_all points" && pointGroup.Name.ToLower() != "no display") { groups.Add(pointGroup.Name); }
                }

                UI.windows.Calculations.ExportPointGroup exportPointGroup = new UI.windows.Calculations.ExportPointGroup(groups, Functions.DocumentProperties.GetJobNumber(AcDocument));
                AcApplication.ShowModalWindow(exportPointGroup);
                AcTran.Commit();
            }
        }
    }
}
