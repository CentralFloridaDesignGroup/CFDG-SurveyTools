using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace CFDG.API.Calcs
{
    public class LineInfo
    {
        public double Length { get; set; }
        public double Azimuth { get; set; }
        public string Bearing { get
            {
                return Angles.AzimuthToBearing(Azimuth);
            }
        }

        public LineInfo()
        {
            Length = -1;
            Azimuth = -1;
        }
        public LineInfo(double length, double azimuth)
        {
            Length = length;
            Azimuth = azimuth;
        }
    }

    public static class Lines
    {
        public static LineInfo CalculateLine(Point3d startPoint, Point3d endPoint)
        {
            LineInfo info = new LineInfo();
            if (startPoint == null || endPoint == null)
            {
                return info;
            }

            info.Azimuth = Math.Atan2(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y) * (180 / Math.PI);
            if (info.Azimuth < 0)
            {
                info.Azimuth += 360;
            }
            info.Length = Math.Sqrt(((endPoint.X - startPoint.X) * (endPoint.X - startPoint.X)) + ((endPoint.Y - startPoint.Y) * (endPoint.Y - startPoint.Y)));

            return info;
        }
    }
}
