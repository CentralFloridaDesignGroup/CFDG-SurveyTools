using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFDG.API.Calcs
{
    public struct LineInfo
    {
        public float Length { get; set; }
        public double Azimuth { get; set; }
        public string Bearing { get
            {
                return Angles.AzimuthToBearing(Azimuth);
            }
        }
    }

    public static class Lines
    {

    }
}
