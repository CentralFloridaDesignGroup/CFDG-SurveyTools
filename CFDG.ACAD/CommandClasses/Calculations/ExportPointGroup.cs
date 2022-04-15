using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;
using CFDG.ACAD.Common;
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
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            List<string> groups = GetPointGroupList();

            if (!acVariables.Document.IsNamedDrawing)
            {
                Logging.Info("\nThe file has not been saved. Cannot export points until the file has the job number in its name.");
                return;
            }

            if (string.IsNullOrEmpty(Functions.DocumentProperties.GetJobNumber(acVariables.Document)))
            {
                Logging.Warning("\nNo job number was detected for the project.");
                return;
            }



            UI.windows.Calculations.ExportPointGroup exportPointGroup = new UI.windows.Calculations.ExportPointGroup(groups, acVariables.Document);
            AcApplication.ShowModalWindow(exportPointGroup);
        }

        private static List<string> GetPointGroupList()
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            List<string> groups = new List<string> { };


            using (Transaction tr = acVariables.Database.TransactionManager.StartTransaction())
            {
                PointGroupCollection pgCollection = CivilApp.ActiveDocument.PointGroups;
                foreach (ObjectId group in pgCollection)
                {
                    PointGroup pointGroup = (PointGroup)group.GetObject(OpenMode.ForRead);
                    if (pointGroup.Name.ToLower() != "_all points" && pointGroup.Name.ToLower() != "no display")
                    {
                        groups.Add(pointGroup.Name);
                    }
                }
                tr.Commit();
            }

            return groups;
        }
    }
}
