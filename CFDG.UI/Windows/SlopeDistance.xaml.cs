using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Autodesk.AutoCAD.Geometry;

namespace CFDG.UI
{
    /// <summary>
    /// Interaction logic for SlopeDistance.xaml
    /// </summary>
    public partial class SlopeDistance : Window
    {
        public SlopeDistance(Point3d startPoint, Point3d endPoint)
        {
            InitializeComponent();
            Calculate(startPoint, endPoint);
        }

        public void Calculate(Point3d startPoint, Point3d endPoint)
        {
            PntAEasting.Text = string.Format("{0:0.00}", startPoint.X);
            PntANorthing.Text = string.Format("{0:0.00}", startPoint.Y);
            PntAElevation.Text = string.Format("{0:0.00}", startPoint.Z);

            PntBEasting.Text = string.Format("{0:0.00}", endPoint.X);
            PntBNorthing.Text = string.Format("{0:0.00}", endPoint.Y);
            PntBElevation.Text = string.Format("{0:0.00}", endPoint.Z);
            //CalculateValues();
        }

        private void OnTextboxChange(object sender, TextChangedEventArgs e)
        {
            if (Double.TryParse(((TextBox)sender).Text, out _))
            {
                CalculateValues();
                Distance.Foreground = Brushes.Black;
                Slope.Foreground = Brushes.Black;
                SlopeAct.Foreground = Brushes.Black;
            }
            else
            {
                Distance.Foreground = Brushes.Red;
                Slope.Foreground = Brushes.Red;
                SlopeAct.Foreground = Brushes.Red;
            }
        }

        private void CalculateValues()
        {
            double deltaX = Math.Abs(Convert.ToDouble(PntBEasting.Text) - Convert.ToDouble(PntAEasting.Text));
            double deltaY = Math.Abs(Convert.ToDouble(PntBNorthing.Text) - Convert.ToDouble(PntANorthing.Text));
            double deltaZ = Convert.ToDouble(PntAElevation.Text) - Convert.ToDouble(PntBElevation.Text);

            double distance = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
            double slope = deltaZ / distance;

            Distance.Text = string.Format("±{0:0.0}LF", Math.Round(distance, 1));
            Slope.Text = string.Format("{0:0.00}%", Math.Round(slope * 100, 2));
            SlopeAct.Text = string.Format("{0:0.00000}", Math.Round(slope, 5));
        }
    }
}
