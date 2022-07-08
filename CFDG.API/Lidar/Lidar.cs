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
        public class PanelInfo
        {
            public string Name { get; set; }
            public double WestBound { get; set; }
            public double EastBound { get; set; }
            public double NorthBound { get; set; }
            public double SouthBound { get; set; }
            public PanelInfo(double northBound, double eastBound, double southBound, double westBound)
            {
                WestBound = westBound;
                EastBound = eastBound;
                NorthBound = northBound;
                SouthBound = southBound;
            }
        }

        public static PanelInfo GetPanelInfo(string path)
        {
            laszip panel = new laszip();
            panel.open_reader(path, out bool _);
            var output = new PanelInfo(panel.header.max_x, panel.header.max_y, panel.header.min_x, panel.header.min_y)
            {
                Name = Path.GetFileNameWithoutExtension(path)
            };
            panel.close_reader();
            return output;
        }

        

        public static Point3dCollection GetValidPoints(string path, Point2d min, Point2d max)
        {
            Point3dCollection points = new Point3dCollection();
            laszip panel = new laszip();
            panel.open_reader(path, out bool _);
            long lidarPoints = panel.header.number_of_point_records;
            if (lidarPoints == 0)
            {
                lidarPoints = (long)panel.header.extended_number_of_point_records;
            }
            double[] coordinates = new double[3];
            for (int i = 0; i < lidarPoints; i++)
            {
                // Automatically reads next point and assigns it to it's internal variables.
                panel.read_point();

                // Passes the X, Y, and Z in the internal method to the 'coordinates' variable.
                // Equivilent to out double[3] coordinates (i think?)
                panel.get_coordinates(coordinates);

                // Gets the classification byte (we want a value of 2)
                byte classification = ClassValue(panel.point.classification);

                if ((min.X <= coordinates[0]) && (coordinates[0] <= max.X))
                {
                    if ((min.Y <= coordinates[1]) && (coordinates[1] <= max.Y))
                    {
                        if (classification == 2)
                        {
                            points.Add(new Point3d(coordinates[0], coordinates[1], coordinates[2]));
                        }
                    }
                }
            }
            panel.close_reader();
            return points;
        }

        public static byte ClassValue(byte BitCod)
        {
            byte num = 0;
            BitArray bitArray = new BitArray(new byte[1] {BitCod});
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
    }
}
