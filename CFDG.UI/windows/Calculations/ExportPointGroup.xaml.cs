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
using System.Diagnostics;
using Autodesk.AutoCAD.EditorInput;

namespace CFDG.UI.windows.Calculations
{
    /// <summary>
    /// Interaction logic for ExportPointGroup.xaml
    /// </summary>
    public partial class ExportPointGroup : Window
    {
        public Models.ExportPointGroupModel PointGroupModel { get; set; }
        public API.ACAD.CogoExporter Exporter { get; set; }

        private Document currentDoc { get; set; }
        private static List<ObjectId> PointsToExport { get; set; }

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
            var filename = System.IO.Path.GetFileNameWithoutExtension(document.Name);
            string jobNumber = API.JobNumber.Parse(filename, API.JobNumberFormats.ShortHyphan);
            this.Title = $"Export Point - {jobNumber}";

            currentDoc = document;
            Exporter = new API.ACAD.CogoExporter();
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
            var objectIds = GetPoint(true);
            if (objectIds.Count == 1 && objectIds[0] == ObjectId.Null)
            {
                LblPointsSelected.Content = "0 points selected.";
                return;
            }
            PointsToExport = objectIds;
            LblPointsSelected.Content = $"{PointsToExport.Count} point{(PointsToExport.Count == 1 ? '\0' : 's')} selected";
            Exporter.AddPointRange(objectIds);
        }

        public List<ObjectId> GetPoint(bool multipleSelections)
        {
            Document acDocument = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor acEditor = acDocument.Editor;
            List<ObjectId> pointIds = new List<ObjectId> { };

            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();

            using (acEditor.StartUserInteraction(this))
            using (acDocument.LockDocument())
            {
                var pointIdList = API.ACAD.GetCogoPoint.SelectPoint(true);
                foreach (var id in pointIdList)
                {
                    if (!pointIds.Contains(id))
                    {
                        pointIds.Add(id);
                    }
                }
            }

            return pointIds;
        }
    }
}
