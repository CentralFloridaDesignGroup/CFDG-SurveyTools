using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;

namespace CFDG.UI
{
    /// <summary>
    /// Interaction logic for PlotPageDialog.xaml
    /// </summary>
    public partial class PlotPageDialog : Window
    {


        public PlotPageDialog()
        {
            InitializeComponent();
        }

        public PlotPageDialog(IEnumerable<Layout> layouts)
        {
            InitializeComponent();
            IOrderedEnumerable<Layout> list = layouts.OrderBy(l => l.TabOrder);
            foreach (Layout lo in list)
            {
                if (lo.LayoutName.ToLower() != "model")
                {
                    ViewsAvalible.Items.Add(lo.LayoutName);
                }
            }

            PlotDate.Text = DateTime.Now.ToString("MM/dd/yyyy");

        }

        private void Cancel_form(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void Accept_form(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
