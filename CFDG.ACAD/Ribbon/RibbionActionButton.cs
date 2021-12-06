using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Windows;

namespace CFDG.ACAD
{
    public partial class RibbonActionButton : RibbonButton
    {
        public Action<string> CommandAction { get; set; }

        public RibbonTextBox ReferenceTextBox { get; set; }
    }
}
