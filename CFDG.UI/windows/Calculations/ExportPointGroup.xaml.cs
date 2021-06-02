﻿using System;
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

namespace CFDG.UI.windows.Calculations
{
    /// <summary>
    /// Interaction logic for ExportPointGroup.xaml
    /// </summary>
    public partial class ExportPointGroup : Window
    {
        public Models.ExportPointGroupModel PointGroupModel { get; set; }

        public ExportPointGroup(List<string> pointGroups, string path)
        {
            InitializeComponent();
            PointGroupModel = new Models.ExportPointGroupModel();
            LbPointGroups.Items.Add("!All Points");
            LbPointGroups.Items.Add("!Comp Points");
            foreach (string group in pointGroups)
            {
                LbPointGroups.Items.Add(group);
            }
            LbPointGroups.Items.IsLiveSorting = true;
            string jobNumber = API.JobNumber.Parse(path, API.JobNumberFormats.ShortHyphan);
            this.Title = $"Export Point Groups - {jobNumber}";

            PointGroupModel.FileName = API.JobNumber.GetPath(jobNumber);
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
            InitializeComponent();
            /*this.pointGroups = pointGroups;
            foreach (ObjectId pointGroupId in pointGroups)
            {
                PointGroup group = (PointGroup)pointGroupId.GetObject(OpenMode.ForRead);
                if (true)
                {
                    LbPointGroups.Items.Add(group.Name);
                }
            }
            this.DialogResult = false;*/
        }
    }
}
