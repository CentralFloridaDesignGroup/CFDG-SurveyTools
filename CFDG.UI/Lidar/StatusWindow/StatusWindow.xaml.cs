using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autodesk.AutoCAD.Geometry;

namespace CFDG.UI.Lidar
{
    /// <summary>
    /// Interaction logic for StatusWindow.xaml
    /// </summary>
    public partial class StatusWindow : Window
    {
        public List<string> ValidFiles;

        private static Point2d _minPoint;
        private static Point2d _maxPoint;
        private static readonly string indexFile = API.XML.ReadValue("lidar", "indexFile");
        private bool _methodOkay;

        public StatusWindow(Point2d min, Point2d max)
        {
            InitializeComponent();
            _minPoint = min;
            _maxPoint = max;
            _methodOkay = true;
            ValidFiles = new List<string>();
        }

        public void BeginProcessing()
        {
            if (!File.Exists(indexFile))
            {
                this.DialogResult = false;
                this.Close();
            }

            txtStatus.Text = "Finding files";

            BackgroundWorker worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.DoWork += GatherIndexFiles;
            if (!worker.IsBusy)
            {
                worker.RunWorkerAsync();
            }

        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                throw e.Error;
            }
            if (e.Cancelled)
            {
                this.DialogResult = false;
                this.Close();
            }
            txtStatus.Text = $"Files found - {ValidFiles.Count}";
            GatherPoints();
        }

        private void GatherPoints()
        {
            BackgroundWorker worker2 = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            worker2.ProgressChanged += Worker2_ProgressChanged;
            worker2.RunWorkerCompleted += Worker2_RunWorkerCompleted;
            worker2.DoWork += GatherPointsWork();
            if (!worker2.IsBusy)
            {
                worker2.RunWorkerAsync();
            }
        }

        private void Worker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Worker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private DoWorkEventHandler GatherPointsWork()
        {
            throw new NotImplementedException();
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage; 
            txtStatus.Text = $"Finding files [{e.ProgressPercentage}%]";
        }

        private void GatherIndexFiles(object sender, DoWorkEventArgs e)
        {
            string[] files = File.ReadAllLines(indexFile);
            BackgroundWorker worker = sender as BackgroundWorker;
            Point2d[] corners = new Point2d[4] { _minPoint, _maxPoint, new Point2d(_minPoint.X, _maxPoint.Y), new Point2d(_maxPoint.X, _minPoint.Y) };
            for (int i = 0; i < files.Length; i++)
            {
                string file = CheckFileBoundary(corners, files[i]);
                if (!string.IsNullOrEmpty(file))
                {
                    ValidFiles.Add(file);
                }
                int progress = (int)Math.Ceiling((double)(i + 1) / files.Length);
                worker.ReportProgress(progress);
            }
            return;
        }

        private string CheckFileBoundary(Point2d[] corners, string entry)
        {
            string file = entry.Split(',')[0];
            bool isValid = false;
            API.Lidar lidar = new API.Lidar(file);
            
            foreach (Point2d point in corners)
            {
                if ((lidar.Meta.WestBound <= point.Y) && (point.Y <= lidar.Meta.EastBound))
                {
                    if ((lidar.Meta.SouthBound <= point.X) && (point.X <= lidar.Meta.NorthBound))
                    {
                        isValid = true;
                    }
                }
            }
            if (isValid)
            {
                return file;
            }
            return "";
        }

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            BeginProcessing();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
