using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace CFDG.ACAD.Extensions
{
    public static class PointGroupExtensions
    {
        public static List<CogoPoint> GetCogoPoints(this PointGroup pointGroup)
        {
            CivilDocument civDoc = CivilApplication.ActiveDocument;
            return (from p in pointGroup.GetPointNumbers()
                    select civDoc.CogoPoints.GetPointByPointNumber(p).GetObject(OpenMode.ForRead) as CogoPoint).ToList();
        }
    }
}
