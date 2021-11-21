using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;
using CFDG.ACAD.Extensions;

namespace CFDG.ACAD
{
    [Obsolete("Class is being seperated into different files", true)]
    public class Calculations
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

        /// <summary>
        /// Creates a point file from a selected point group.
        /// </summary>
        /*[CommandMethod("ExportPointGroups")]
        public static void ExportPointGroups()
        {
            Document acDoc = AcApp.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;
            Editor acEd = acDoc.Editor;
            CivilDocument cApp = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;

            PointGroupCollection pointGroups;
            ExportPoints form;

            #region Job Number
            var jobNumber = Path.GetFileNameWithoutExtension(acDoc.Name);
            if (jobNumber.Length > 9)
            {
                jobNumber = jobNumber.Remove(9);
            }

            if (!API.JobNumber.TryParse(jobNumber))
            {
                acEd.WriteMessage("Job number could not be determined." + Environment.NewLine);
                return;
            }
            string path = API.JobNumber.GetPath(jobNumber);
            if (!Directory.Exists(path))
            {
                acEd.WriteMessage("Project folder could not be determined." + Environment.NewLine);
            }
            #endregion

            using (Transaction tr = acDb.TransactionManager.StartTransaction())
            {
                pointGroups = cApp.PointGroups;
                form = new ExportPoints(pointGroups, path);
                var fResult = AcApp.ShowModalDialog(form);

                if (fResult == DialogResult.Cancel) { acEd.WriteMessage("\nExport operation cancelled."); return; }
                acEd.WriteMessage("\nExport operation continued.");

                if (!Directory.Exists(form.FolderPath)) { Directory.CreateDirectory(form.FolderPath); }

                string file = form.FolderPath + "\\" + form.FileName.Trim();
                acEd.WriteMessage($"\nWriting {form.PointGroup} points to {file}");

                switch (form.PointGroup.ToLower())
                {
                    case "!all points":
                        {
                            foreach (ObjectId pointId in cApp.CogoPoints)
                            {
                                CogoPoint point = (CogoPoint)pointId.GetObject(OpenMode.ForRead);
                                string pointTxt = $"{point.PointNumber},{Math.Round(point.Northing, 4)},{Math.Round(point.Easting, 4)},{Math.Round(point.Elevation, 4)},{point.RawDescription}{Environment.NewLine}";
                                File.AppendAllText(file, pointTxt);
                            }
                            break;
                        }
                    case "!comp points":
                        {
                            foreach (ObjectId pointId in cApp.CogoPoints)
                            {
                                CogoPoint point = (CogoPoint)pointId.GetObject(OpenMode.ForRead);
                                int pnumber = Convert.ToInt32(point.PointNumber);
                                if (pnumber >= 1000 && pnumber < 10000)
                                {
                                    string pointTxt = $"{point.PointNumber},{Math.Round(point.Northing, 4)},{Math.Round(point.Easting, 4)},{Math.Round(point.Elevation, 4)},{point.RawDescription}{Environment.NewLine}";
                                    File.AppendAllText(file, pointTxt);
                                }
                            }
                            break;
                        }
                    case "!control points":
                        {
                            foreach (ObjectId pointId in cApp.CogoPoints)
                            {
                                CogoPoint point = (CogoPoint)pointId.GetObject(OpenMode.ForRead);
                                string description = point.RawDescription;
                                if (controlCodes.Any(s => description.Contains(s)))
                                {
                                    string pointTxt = $"{point.PointNumber},{Math.Round(point.Northing, 4)},{Math.Round(point.Easting, 4)},{Math.Round(point.Elevation, 4)},{point.RawDescription}{Environment.NewLine}";
                                    File.AppendAllText(file, pointTxt);
                                }
                            }
                            break;
                        }
                    default:
                        {
                            foreach (ObjectId pgId in pointGroups)
                            {
                                PointGroup group = (PointGroup)pgId.GetObject(OpenMode.ForRead);
                                if (group.Name.ToLower() == form.PointGroup.ToLower())
                                {
                                    var cogoPoints = group.GetCogoPoints();
                                    acEd.WriteMessage($"\nPoint Group {form.PointGroup} contains {cogoPoints.Count} points");
                                    foreach (CogoPoint point in cogoPoints)
                                    {
                                        string pointTxt = $"{point.PointNumber},{Math.Round(point.Northing, 4)},{Math.Round(point.Easting, 4)},{Math.Round(point.Elevation, 4)},{point.RawDescription}{Environment.NewLine}";
                                        File.AppendAllText(file, pointTxt);
                                    }
                                }
                            }

                            break;
                        }
                }
                acEd.WriteMessage($"\nPoints exported successfully.");
                form.Dispose();
                tr.Commit();
            };
        }*/

        /// <summary>
        /// Creates a point group with the proper name that 
        /// </summary>

    }
}
