using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using LASzip.Net;

namespace CFDG.API
{
    public class Lidar
    {
        #region Properties
        public MetaData Meta { get; set; }

        public List<Point3d> GroundPoints { get
            {
                if (_groundPoints == null)
                {
                    throw new Exception("The GroundPoints list was not compiled. Please call Lidar.CompileGroundShotsAsync() to compile the list.");
                }
                return _groundPoints;
            } 
        }

        public string FileName { get
            {
                return _lidarFile;
            } 
        }
        
        private List<Point3d> _groundPoints;
        private string _lidarFile { get; set; }
        #endregion

        #region Public Classes
        public class MetaData
        {
            public string Name { get; internal set; }

            public double WestBound { get; internal set; }

            public double EastBound { get; internal set; }

            public double NorthBound { get; internal set; }

            public double SouthBound { get; internal set; }

            public double PointCount { get; internal set; }

            public double MinimumElevation { get; internal set; }

            public double MaximumElevation { get; internal set; }

            public long NumberOfPoints { get; internal set; }

            internal MetaData(string lidarFile)
            {
                laszip panel = new laszip();
                panel.open_reader(lidarFile, out _);
                Name = Path.GetFileNameWithoutExtension(lidarFile);
                NorthBound = panel.header.max_x;
                SouthBound = panel.header.min_x;
                WestBound = panel.header.min_y;
                EastBound = panel.header.max_y;
                MinimumElevation = panel.header.min_z;
                MaximumElevation = panel.header.max_z;
                NumberOfPoints = panel.header.number_of_point_records;
                if (NumberOfPoints == 0)
                {
                    NumberOfPoints = (long)panel.header.extended_number_of_point_records;
                }
                panel.close_reader();
            }

            public override string ToString()
            {
                string output = $"{Name}{Environment.NewLine}" +
                    $"SW Corner: {WestBound:0.000}, {SouthBound:0.000}{Environment.NewLine}" +
                    $"NE Corner: {EastBound:0.000}, {NorthBound:0.000}{Environment.NewLine}" +
                    $"Elevations: {MinimumElevation:0.000} to {MaximumElevation:0.000} ({(MaximumElevation - MinimumElevation):0.000}){Environment.NewLine}" +
                    $"Number of Points: {NumberOfPoints}";
                return output;
            }
        }
        #endregion

        #region Public Initalizers 

        public Lidar(string file)
        {
            _lidarFile = file;
            Meta = new MetaData(file);
        }
        #endregion

        #region Event Handlers
        public delegate void ProgressChangedHandler(long processedPoints);
        public event ProgressChangedHandler ProgressChanged;
        private void TriggerPointsProcessed(long pointsProcessed)
        {
            ProgressChanged(pointsProcessed);
        }
        #endregion

        #region Public Methods

        public async Task<bool> CompileGroundShotsAsync()
        {
            _groundPoints = new List<Point3d>();
            laszip panel = new laszip();
            panel.open_reader(_lidarFile, out _);
            double[] coordinates = new double[3];
            int i = 0;
            try
            {
                for (long curPoint = 0; curPoint < Meta.NumberOfPoints; curPoint++)
                {
                    panel.read_point();
                    panel.get_coordinates(coordinates);
                    var classification = ClassValue(panel.point.classification);
                    if (classification == 2)
                    {
                        _groundPoints.Add(new Point3d(coordinates));
                        TriggerPointsProcessed(curPoint);
                    }
                    i++;
                    if (i > 24)
                    {
                        TriggerPointsProcessed(curPoint);
                        i = 0;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("There was an error processing the ground points.", ex);
            }
        }

        public async Task<bool> CompileGroundShotsAsync(Point2d min, Point2d max)
        {
            _groundPoints = new List<Point3d>();
            laszip panel = new laszip();
            panel.open_reader(_lidarFile, out _);
            double[] coordinates = new double[3];
            int i = 0;
            try
            {
                for (long curPoint = 0; curPoint < Meta.NumberOfPoints; curPoint++)
                {
                    panel.read_point();
                    panel.get_coordinates(coordinates);
                    var classification = ClassValue(panel.point.classification);
                    if ((min.X <= coordinates[0]) && (coordinates[0] <= max.X))
                    {
                        if ((min.Y <= coordinates[1]) && (coordinates[1] <= max.Y))
                        {
                            if (classification == 2)
                            {
                                _groundPoints.Add(new Point3d(coordinates));
                            }
                        }
                    }
                    i++;
                    if (i > 24)
                    {
                        TriggerPointsProcessed(curPoint);
                        i = 0;
                    }
                }
                panel.close_reader();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("There was an error processing the ground points.", ex);
            }
        }

        #endregion

        #region Private Methods
        private static byte ClassValue(byte BitCod)
        {
            byte num = 0;
            BitArray bitArray = new BitArray(new byte[1] { BitCod });
            if (bitArray[0])
            {
                checked { ++num; }
            }

            if (bitArray[1])
            {
                checked { num += (byte)2; }
            }

            if (bitArray[2])
            {
                checked { num += (byte)4; }
            }
            
            if (bitArray[3])
            {
                checked { num += (byte)8; }
            }

            if (bitArray[4])
            {
                checked { num += (byte)16; }
            }

            return num;
        } 
        #endregion
    }
}
