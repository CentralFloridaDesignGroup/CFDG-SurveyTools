using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using CivilApplication = Autodesk.Civil.ApplicationServices.CivilApplication;
using CFDG.ACAD.Common;
using System.Windows.Forms;
using System;
using System.Threading.Tasks;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices.Styles;

namespace CFDG.ACAD.CommandClasses.Lidar
{
    public class CreateLidarTin : ICommandMethod
    {
        [CommandMethod("CreateLidarTin", CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public void InitialCommand()
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Filter = "All Supported Formats|*.las;*.laz|Lidar Format File (*.las)|*.las|Lidar Compressed File (*.laz)|*.laz",
                Title = "Select lidar file(s)",
                Multiselect = false
            };
            if (fileDialog.ShowDialog() != DialogResult.OK || fileDialog.FileNames.Length < 1)
            {
                Logging.Error("No files were selected. Exiting...");
                return;
            }
            Point2d[] area = UserInput.GetRectange();
            if (area == null)
            {
                Logging.Error("Could not determine the area to gather lidar.");
            }
            Logging.Debug($"Area valid\nMin: {area[0]}\nMax: {area[1]}");
            Logging.Info("Collecting points");
            var points = GetPointsFromFile(fileDialog.FileNames[0], area[0], area[1]);
            if (points is null)
            {
                Logging.Error("Points returned null.");
                return;
            }
            Logging.Info($"Returned {points.Count} points.");
            if (points.Count < 3)
            {
                Logging.Error("Not enough points were returned.");
                return;
            }
            CreateTIN(points);
        }

        private Point3dCollection GetPointsFromFile(string path, Point2d min, Point2d max)
        {
            return API.Lidar.GetValidPoints(path, min, max);
        }

        private void CreateTIN(Point3dCollection collection)
        {
            using (Transaction transaction = AcApplication.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    CivilDocument civilDocument = CivilApplication.ActiveDocument;
                    //if (!((StyleCollectionBase)civilDocument.Styles.SurfaceStyles).Contains("Border Only"))
                    //{
                    //    civilDocument.Styles.SurfaceStyles.Add("Border Only");
                    //}
                    ObjectId styleId = ((StyleCollectionBase)civilDocument.Styles.SurfaceStyles)[0];

                    string name = $"Lidar - {DateTime.Now:yy-MM-dd HH-mm-ss}";
                    ObjectId surfaceId = TinSurface.Create(name, styleId);
                    TinSurface tinSurface = surfaceId.GetObject(OpenMode.ForWrite) as TinSurface;
                    tinSurface.Description = "Created using Survey Toolbox by C.F.D.G.";
                    tinSurface.AddVertices(collection);
                    tinSurface.Rebuild();
                    transaction.Commit();
                }
                catch
                {
                    Logging.Error("An issue occured with creating the surface.");
                    transaction.Commit();
                }
            }
        }
    }
}
