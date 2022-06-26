using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace CFDG.ACAD.CommandClasses.Calculations
{
    public class GroupPoints : ICommandMethod
    {
        [CommandMethod("GroupPoints", CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public void InitialCommand()
        {
            Document acDoc = AcApplication.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;
            Editor adEd = acDoc.Editor;
            CivilDocument cApp = CivilApplication.ActiveDocument;

            List<string> points = new List<string> { };
            string pointStr = "";
            string descriptionStr = "";

            string groupName = Common.UserInput.GetStringFromUser("Please enter the point group name: ", true);

            // If the string was empty (or null somehow)
            if (string.IsNullOrEmpty(groupName)) { adEd.WriteMessage("\nThe string entered was empty, please try again."); return; }

            //Selection method
            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("\nPlease select a method of point selection: ");
            pKeyOpts.Keywords.Add("List");
            pKeyOpts.Keywords.Add("Selection");
            pKeyOpts.Keywords.Add("Descriptions");
            pKeyOpts.Keywords.Default = "List";
            pKeyOpts.AllowNone = true;

            PromptResult pRlt = adEd.GetKeywords(pKeyOpts);
            string selType = pRlt.StringResult;
            switch (selType)
            {
                case "List":
                {
                    string pointList = Common.UserInput.GetStringFromUser("Enter the point range using dashes (-) and commas (,): ", true);
                    
                    if (string.IsNullOrEmpty(pointList)) { adEd.WriteMessage("\nThe string entered was empty, please try again."); return; }
                    pointStr = pointList;
                    break;
                }
                case "Selection":
                {
                    TypedValue[] tvs = new TypedValue[]
                    {
                            new TypedValue((int)DxfCode.Start, "AECC_COGO_POINT")
                    };
                    SelectionFilter selFltr = new SelectionFilter(tvs);
                    PromptSelectionResult acSSPrompt;
                    acSSPrompt = adEd.GetSelection(selFltr);
                    if (acSSPrompt.Status == PromptStatus.Cancel) { adEd.WriteMessage("\nAction aborted."); return; }
                    if (acSSPrompt.Value.Count < 1) { adEd.WriteMessage("\nThe selectiond was empty, please try again."); return; }
                    using (Transaction tr = acDb.TransactionManager.StartTransaction())
                    {
                        foreach (ObjectId obj in acSSPrompt.Value.GetObjectIds())
                        {
                            CogoPoint pnt = (CogoPoint)obj.GetObject(OpenMode.ForRead);
                            points.Add(pnt.PointNumber.ToString());
                        }
                        points.Sort();
                    }
                    pointStr = BeautifyPointList(points);
                    break;
                }
                case "Descriptions":
                {
                    descriptionStr = Common.UserInput.GetStringFromUser("\nEnter the raw descriptions including wildcards (*) and commas (,): ", true);
                    if (string.IsNullOrEmpty(descriptionStr)) { adEd.WriteMessage("\nThe string entered was empty, please try again."); return; }

                    break;
                }
                default:
                {
                    MessageBox.Show($"The selected input for GroupCalc Command was not valid: {pRlt.StringResult}");
                    return;
                }
            }
            if (string.IsNullOrEmpty(pointStr) && string.IsNullOrEmpty(descriptionStr)) { adEd.WriteMessage("\nNo points were detected"); return; }
            using (Transaction tr = acDb.TransactionManager.StartTransaction())
            {
                //Establish points
                groupName = $"[{DateTime.Now:MM-dd-yyyy}] {groupName.ToUpper()}";

                StandardPointGroupQuery query = new StandardPointGroupQuery();
                if (selType == "Descriptions")
                {
                    query.IncludeRawDescriptions = descriptionStr;
                }
                else
                {
                    query.IncludeNumbers = pointStr;
                }

                PointGroupCollection pointGroups = cApp.PointGroups;
                if (pointGroups.Contains(groupName))
                {
                    adEd.WriteMessage("\nThe point group already exists, edit the existing group or come up with another name...");
                    return;
                }
                ObjectId groupId = pointGroups.Add(groupName);
                PointGroup group = (PointGroup)groupId.GetObject(OpenMode.ForRead);
                group.SetQuery(query);
                tr.Commit();
            }
            adEd.WriteMessage("\nPoint group created successfully!");
        }

        private string BeautifyPointList(List<string> points)
        {
            string outp = "";
            int curPoint = 0;
            foreach (string point in points)
            {
                int pNumber = int.Parse(point);
                if (curPoint == 0)
                {
                    outp += pNumber;
                    curPoint = pNumber;
                }
                else if (int.Parse(points.Last()) == pNumber)
                {
                    outp += pNumber == curPoint + 1 && outp.Last() != '-' ? "-" : ", ";

                    outp += pNumber;
                }
                else if (pNumber == curPoint + 1)
                {
                    curPoint = pNumber;
                }
                else
                {
                    outp += "-" + curPoint + ", " + pNumber;
                    curPoint = pNumber;
                }
            }
            return outp;
        }
    }
}
