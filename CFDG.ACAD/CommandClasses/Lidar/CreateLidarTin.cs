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
using System.IO;

namespace CFDG.ACAD.CommandClasses.Lidar
{
    public class CreateLidarTin
    {
        private List<string> _lidarPanels;
        private List<Point3d> _collectedPoints;
        private Point2d _minCorner;
        private Point2d _maxCorner;

        [CommandMethod("CreateLidarTin", CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public async void InitialCommand()
        {
            Point2d[] area = UserInput.GetRectange();
            if (area == null)
            {
                Logging.Error("Could not determine the area to gather lidar.");
                return;
            }
            Logging.Debug($"Area valid\nMin: {area[0]}\nMax: {area[1]}");
            _minCorner = area[0];
            _maxCorner = area[1];
            Logging.Info("Processing lidar files.");
            if (!await GetRelevantPanels()){
                Logging.Error("Could not process lidar files.");
                return;
            }
            if (!await ProcessFileMethods())
            {
                Logging.Error("Could not generate points from files.");
                return;
            }
            if (!await CreateTIN())
            {
                Logging.Error("Could not create a TIN surface.");
                return;
            }
        }

        private async Task<bool> GetRelevantPanels()
        {
            _lidarPanels = new List<string>();
            string currentZone = AcApplication.GetSystemVariable("cgeocs").ToString();
            if (string.IsNullOrEmpty(currentZone))
            {
                Logging.Error("The coordinate zone was not properly set.");
                return false;
            }
            currentZone = currentZone.Split('-')[1];
            string indexValue = API.XML.ReadValue("lidar", "indexfile");
            if (string.IsNullOrEmpty(indexValue))
            {
                return false;
            }
            string[] files = File.ReadAllLines(indexValue);
            API.Lidar lidar;
            Point2d[] corners = new Point2d[4] { _minCorner, _maxCorner, new Point2d(_minCorner.X, _maxCorner.Y), new Point2d(_maxCorner.X, _minCorner.Y) };
            bool isValid = false;
            foreach (string file in files)
            {
                isValid = false;
                string lidarFile = file.Split(',')[0];
                string zone = file.Split(',')[1];
                lidar = new API.Lidar(lidarFile);
                if (zone != currentZone)
                {
                    continue;
                }
                foreach (Point2d point in corners)
                {
                    if ((lidar.Meta.WestBound <= point.Y) && (point.Y <= lidar.Meta.EastBound))
                    {
                        if ((lidar.Meta.SouthBound <= point.X) && (point.X <= lidar.Meta.NorthBound))
                        {
                            isValid = true;
                        }
                    }
                }
                if (isValid)
                {
                    _lidarPanels.Add(file);
                }
            }
            return true;
        }

        private async Task<bool> ProcessFileMethods()
        {
            int track = 1;
            foreach (string file in _lidarPanels)
            {
                //TODO: Impliment UI for importing
                bool lidarFileProcessed = await ProcessLidarFile(file, _minCorner, _maxCorner);
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

        private async Task<bool> CreateTIN()
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
                    return false;
                }
            }
            return true;
        }
    }
}
