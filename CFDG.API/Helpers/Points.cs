using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace CFDG.API.Helpers
{
    public static class Points
    {
        public static readonly Point2d Base2dPoint = new Point2d(0, 0);
        public static readonly Point2d Null2dPoint = new Point2d(-1, -1);
        public static readonly Point3d Base3dPoint = new Point3d(0, 0, 0);
        public static readonly Point3d Null3dPoint = new Point3d(-1, -1, -1);
    }
}
