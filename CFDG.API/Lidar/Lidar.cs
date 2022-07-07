using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            panel.open_reader(path, out bool local);
            var output = new PanelInfo(panel.header.max_x, panel.header.max_y, panel.header.min_x, panel.header.min_y)
            {
                Name = Path.GetFileNameWithoutExtension(path)
            };
            panel.close_reader();
            return output;
        }
    }
}
