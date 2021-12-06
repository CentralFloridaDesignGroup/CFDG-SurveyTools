using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFDG.UI.Models
{
    public class ExportPointGroupModel
    {
        public string FileName { get; set; }

        public List<string> PointGroups { get; set; }
    }
}
