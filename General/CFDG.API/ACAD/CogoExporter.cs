using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;

namespace CFDG.API.ACAD
{
    public class CogoExporter
    {
        public List<CogoPoint> SelectedCogoPoints { get; set; }

        public CogoExporter()
        {
            Initalize(false);
        }

        public CogoExporter(bool reset)
        {
            Initalize(reset);
        }

        public void Initalize(bool reset)
        {
            if (SelectedCogoPoints != null && !reset)
            {
                return;
            }
            SelectedCogoPoints = new List<CogoPoint> { };
            return;
        }

        public bool AddPointRange(List<ObjectId> pointId)
        {
            try
            {
                foreach (ObjectId objectId in pointId)
                {
                    if (SelectedCogoPoints.Any(cp => cp.ObjectId == objectId))
                    {
                        SelectedCogoPoints.Add(ConvertObjectId(objectId));
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private CogoPoint ConvertObjectId(ObjectId objectId)
        {
            return GetCogoPoint.GetCogoByID(objectId);
        }
    }
}
