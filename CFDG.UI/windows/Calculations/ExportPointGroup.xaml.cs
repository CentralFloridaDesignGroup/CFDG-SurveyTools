using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autodesk.Civil.DatabaseServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.ApplicationServices;

namespace CFDG.UI.windows.Calculations
{
    /// <summary>
    /// Interaction logic for ExportPointGroup.xaml
    /// </summary>
    public partial class ExportPointGroup : Window
    {
        public Models.ExportPointGroupModel PointGroupModel { get; set; }

        private Document currentDoc { get; set; }

        public ExportPointGroup(List<string> pointGroups, Document document)
        {
            InitializeComponent();
            PointGroupModel = new Models.ExportPointGroupModel();
            CmbPointGroups.ItemsSource = pointGroups;
            foreach (string group in pointGroups)
            {
                //LbPointGroups.Items.Add(group);
            }
            //LbPointGroups.Items.IsLiveSorting = true;
            string jobNumber = API.JobNumber.Parse(document.Name, API.JobNumberFormats.ShortHyphan);
            this.Title = $"Export Point - {jobNumber}";

            currentDoc = document;
        }

        private void CustomEntryKeyPress(object sender, KeyEventArgs args)
        {
           /*TextBox tbox = (TextBox)sender;
            if (IsValidFileName(tbox.Text))
            {
                tbox.BorderBrush = Brushes.Black;
                return;
            }
            tbox.BorderBrush = Brushes.Red;*/
        }

        private void CmdCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CmdSelectPoints_Click(object sender, RoutedEventArgs e)
        {
            currentDoc.SendStringToExecute("._getcogopoint ", false, false, true);
        }
    }
}
