using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using CFDG.ACAD.Common;
using System.Windows.Forms;
using System;

namespace CFDG.ACAD.CommandClasses.Lidar
{
    public class CreateLidarTiles : ICommandMethod
    {
        [CommandMethod("CreateLidarTiles", CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public void InitialCommand()
        {
            double minX = 0;
            double minY = 0;
            double maxX = 0;
            double maxY = 0;

            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Filter = "All Supported Formats|*.las;*.laz|Lidar Format File (*.las)|*.las|Lidar Compressed File (*.laz)|*.laz",
                Title = "Select lidar file(s)",
                Multiselect = true
            };
            if (fileDialog.ShowDialog() != DialogResult.OK || fileDialog.FileNames.Length < 1)
            {
                Logging.Error("No files were selected. Exiting...");
                return;
            }
            foreach (var file in fileDialog.FileNames)
            {
                var info = ProcessLidarPanel(file);
                minX = (minX == 0) ? info.SouthBound : Math.Min(minX, info.SouthBound);
                minY = (minY == 0) ? info.WestBound : Math.Min(minY, info.WestBound);
                maxX = (maxX == 0) ? info.NorthBound : Math.Max(maxX, info.NorthBound);
                maxY = (maxY == 0) ? info.EastBound : Math.Max(maxY, info.EastBound);
            }
            ZoomToResult(acVariables.Editor, new Point2d(minX, minY), new Point2d(maxX, maxY));
        }

        private static void ZoomToResult(Editor ed, Point2d min, Point2d max)
        {
            Logging.Debug($"Zooming to ({min}), ({max})");
            ViewTableRecord view = new ViewTableRecord
            {
                CenterPoint = min + ((max - min) / 2),
                Height = max.Y - min.Y,
                Width = max.X - min.X
            };
            ed.SetCurrentView(view);
        }

        private API.Lidar.PanelInfo ProcessLidarPanel(string file)
        {
            Logging.Debug($"Parsing \"{file}\"");
            API.Lidar.PanelInfo info = API.Lidar.GetPanelInfo(file);
            if (info is null)
            {
                Logging.Error("There was a parsing error with the file.");
                return null;
            }
            CreatePolyline(info);
            Logging.Debug("Parsing succeeded!");
            return info;
        }

        private void CreatePolyline(API.Lidar.PanelInfo panelInfo)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            double annoScale = double.Parse(AcApplication.GetSystemVariable("CANNOSCALEVALUE").ToString());
            Logging.Debug("Creating a polyline.");
            using (Transaction transaction = acVariables.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    //Create polyline
                    Polyline polyline = new Polyline(4);
                    polyline.SetDatabaseDefaults();
                    polyline.AddVertexAt(0, new Point2d(panelInfo.NorthBound, panelInfo.EastBound), 0, 0, 0);
                    polyline.AddVertexAt(1, new Point2d(panelInfo.NorthBound, panelInfo.WestBound), 0, 0, 0);
                    polyline.AddVertexAt(2, new Point2d(panelInfo.SouthBound, panelInfo.WestBound), 0, 0, 0);
                    polyline.AddVertexAt(3, new Point2d(panelInfo.SouthBound, panelInfo.EastBound), 0, 0, 0);
                    polyline.Elevation = 0;
                    polyline.Closed = true;

                    //Create MText
                    MText text = new MText
                    {
                        Contents = panelInfo.Name,
                        TextHeight = 50,
                        Attachment = AttachmentPoint.MiddleCenter,
                        Location = new Point3d((panelInfo.NorthBound + panelInfo.SouthBound) / 2, (panelInfo.WestBound + panelInfo.EastBound) / 2, 0),
                        Layer = "0"
                    };

                    //Open the document to append the polyline (entity)
                    BlockTable blockTable = transaction.GetObject(acVariables.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord blockTableRecords = transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    //Add polyline to records
                    blockTableRecords.AppendEntity(polyline);
                    transaction.AddNewlyCreatedDBObject(polyline, true);

                    //Add mtext to records
                    blockTableRecords.AppendEntity(text);
                    transaction.AddNewlyCreatedDBObject(text, true);
                }
                catch
                {
                    Logging.Error("Could not create a polyline for the lidar file.");
                }
                finally
                {
                    transaction.Commit();
                }
            }
        }
    }
}
