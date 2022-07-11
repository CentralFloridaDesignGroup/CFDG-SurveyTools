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
            API.Lidar lidar;
            foreach (var file in fileDialog.FileNames)
            {
                lidar = new API.Lidar(file);
                minX = (minX == 0) ? lidar.Meta.SouthBound :    Math.Min(minX, lidar.Meta.SouthBound);
                minY = (minY == 0) ? lidar.Meta.WestBound  :    Math.Min(minY, lidar.Meta.WestBound);
                maxX = (maxX == 0) ? lidar.Meta.NorthBound :    Math.Max(maxX, lidar.Meta.NorthBound);
                maxY = (maxY == 0) ? lidar.Meta.EastBound  :    Math.Max(maxY, lidar.Meta.EastBound);
                CreatePolyline(lidar);
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

        private void CreatePolyline(API.Lidar panelInfo)
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
                    polyline.AddVertexAt(0, new Point2d(panelInfo.Meta.NorthBound, panelInfo.Meta.EastBound), 0, 0, 0);
                    polyline.AddVertexAt(1, new Point2d(panelInfo.Meta.NorthBound, panelInfo.Meta.WestBound), 0, 0, 0);
                    polyline.AddVertexAt(2, new Point2d(panelInfo.Meta.SouthBound, panelInfo.Meta.WestBound), 0, 0, 0);
                    polyline.AddVertexAt(3, new Point2d(panelInfo.Meta.SouthBound, panelInfo.Meta.EastBound), 0, 0, 0);
                    polyline.Elevation = 0;
                    polyline.Closed = true;

                    //Create MText
                    MText text = new MText
                    {
                        Contents = panelInfo.Meta.ToString(),
                        TextHeight = 50,
                        Attachment = AttachmentPoint.MiddleCenter,
                        Location = new Point3d((panelInfo.Meta.NorthBound + panelInfo.Meta.SouthBound) / 2, (panelInfo.Meta.WestBound + panelInfo.Meta.EastBound) / 2, 0),
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
