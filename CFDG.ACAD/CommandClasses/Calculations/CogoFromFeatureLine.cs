using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using CFDG.ACAD.Common;
using Autodesk.Civil.DatabaseServices;
using System.Collections.Generic;

namespace CFDG.ACAD.CommandClasses.Calculations
{
    public class CreateACPointsTwo : ICommandMethod
    {
        [CommandMethod("CogoFromFeatureLine", CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public void InitialCommand()
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            string cogoText = UserInput.GetStringFromUser("Typical text for point description: ");
            //string startPointNumber = UserInput.GetStringFromUser("Starting Point Number: ");
            var features = getFeatureLines();
            if (features.Count == 0)
            {
                Logging.Error("No feature lines selected.");
            }
            CreateCogoPoints(cogoText, features);
        }

        private List<FeatureLine> getFeatureLines()
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            var features = new List<FeatureLine>();
            TypedValue[] tvs = new TypedValue[]
                    {
                            new TypedValue((int)DxfCode.Start, "AECC_FEATURE_LINE")
                    };
            SelectionFilter selFltr = new SelectionFilter(tvs);
            PromptSelectionResult acSSPrompt;
            acSSPrompt = acVariables.Editor.GetSelection(selFltr);
            if (acSSPrompt.Status == PromptStatus.Cancel) { acVariables.Editor.WriteMessage("\nAction aborted."); return features; }
            if (acSSPrompt.Value.Count < 1) { acVariables.Editor.WriteMessage("\nThe selectiond was empty, please try again."); return features; }
            using (Transaction tr = acVariables.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId obj in acSSPrompt.Value.GetObjectIds())
                {
                    FeatureLine feature = (FeatureLine)obj.GetObject(OpenMode.ForRead);
                    features.Add(feature);
                }
            }
            return features;
        }

        private void CreateCogoPoints(string description, List<FeatureLine> lines)
        {
            //if (!int.TryParse(startPointNumber, out int id))
            //{
            //    Logging.Error("Provided number is not valid");
            //}
            foreach (FeatureLine line in lines)
            {
                var points = line.GetPoints(Autodesk.Civil.FeatureLinePointType.AllPoints);
                if (points[points.Count - 1] == points[0])
                {
                    points.RemoveAt(points.Count - 1);
                }
                CreateCogoPoint(points, description);
                //id += points.Count;
            }
        }

        private void CreateCogoPoint(Point3dCollection location, string description)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            ObjectIdCollection objectIdCollection;
            using (Transaction tr = acVariables.Database.TransactionManager.StartTransaction())
            {
                CogoPointCollection pointCollection = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument.CogoPoints;
                
                objectIdCollection = pointCollection.Add(location, description, true);
                tr.Commit();
            }
            //RenumberPoints(objectIdCollection, pointId);
        }
    }
}
