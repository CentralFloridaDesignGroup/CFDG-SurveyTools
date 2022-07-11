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
using System.Collections.Generic;
using System.Threading;

namespace CFDG.ACAD.CommandClasses.Lidar
{
    public class CreateLidarTin
    {
        private List<Point3d> _collectedPoints;

        private UI.Lidar.StatusWindow _statusWindow;

        [CommandMethod("CreateLidarTin", CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public async void InitialCommand()
        {
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
            Point2d[] area = UserInput.GetRectange();
            if (area == null)
            {
                Logging.Error("Could not determine the area to gather lidar.");
                return;
            }
            Logging.Debug($"Area valid\nMin: {area[0]}\nMax: {area[1]}");
            Logging.Info("Processing lidar files.");

            var lidarFilesProcess = ProcessFileMethods(fileDialog.FileNames, area).Result;

            if (!lidarFilesProcess)
            {
                Logging.Error("Something happened during the processing of the lidar file. Exiting.");
                return;
            }

            Logging.Info($"Returned {_collectedPoints.Count} points.");
            if (_collectedPoints.Count < 3)
            {
                Logging.Error("Not enough points were returned.");
                return;
            }
            await CreateTIN();
        }

        private async Task<bool> ProcessFileMethods(string[] files, Point2d[] area)
        {
            int track = 1;
            foreach (string file in files)
            {
                //TODO: Impliment UI for importing
                bool lidarFileProcessed = ProcessLidarFile(file, area[0], area[1]).Result;
                if (!lidarFileProcessed)
                {
                    return false;
                }
                track++;
            }
            return true;
        }

        private async Task<bool> ProcessLidarFile(string path, Point2d min, Point2d max)
        {
            if (_collectedPoints == null)
            {
                _collectedPoints = new List<Point3d>();
            }

            API.Lidar lidar = new API.Lidar(path);
            
            var success = await lidar.CompileGroundShotsAsync(min, max);
            if (!success)
            {
                return false;
            }
            _collectedPoints.AddRange(lidar.GroundPoints);
            return true;
        }

        private async Task CreateTIN()
        {
            Logging.Info("Preparing TIN surface.");
            Point3dCollection collection = new Point3dCollection(_collectedPoints.ToArray());
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
