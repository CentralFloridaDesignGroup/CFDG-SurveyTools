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
using System.Windows.Navigation;
using System.IO;
using System.Globalization;
using System.Diagnostics;

namespace CFDG.ACAD.Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string DllFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\CFDG.ACAD.dll";
        public MainWindow()
        {
            InitializeComponent();
            DllFile = DllFile.Replace('\\', '/');
            UpdateList();
        }

        private void UpdateList()
        {
            LstVersions.Items.Clear();
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string searchCriteria = Path.Combine(programFiles, "Autodesk");
            string[] Subdirectiores = Directory.GetDirectories(searchCriteria, "AutoCAD 20*");
            string ci = CultureInfo.CurrentUICulture.Name;
            foreach (string subDirectory in Subdirectiores)
            {
                string autoCad = Path.GetFileName(subDirectory);
                string supportFolder = $@"{subDirectory}\support";
                string version = autoCad.Substring(autoCad.Length - 4);
                string supportFile = Path.Combine(supportFolder, $"acad{version}.lsp");
                if (File.Exists(supportFile))
                {
                    string content = File.ReadAllText(supportFile);
                    if (content.Contains($"(command \"_netload\" \"{ DllFile }\")"))
                    {
                        LstVersions.Items.Add(autoCad + " (Already Installed) | " + supportFile);
                    }
                    else
                    {
                        LstVersions.Items.Add(autoCad + " (Not Installed) | " + supportFile);
                        //File.Create(supportFile);
                        //File.WriteAllText(supportFile, $"(command \"_netload\" \"{ dllFile }\")");
                    }
                }
                else
                {
                    LstVersions.Items.Add(autoCad + " (Not Installed) | " + supportFile);

                }
            }
        }

        private void CmdCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CmdSubmit_Click(object sender, RoutedEventArgs e)
        {
            var selection = LstVersions.SelectedItem;
            if (selection == null)
            {
                MessageBox.Show("Nothing was selected");
                return;
            }

            var versionstr = LstVersions.SelectedValue.ToString();

            if (versionstr.Contains("Already"))
            {
                MessageBox.Show("Required files already present and correct.");
                return;
            }

            var versionSplit = versionstr.Split('|');
            var path = versionSplit[1].Remove(0, 1);
            File.AppendAllText(path, $"{Environment.NewLine}(command \"_netload\" \"{ DllFile }\")");
            MessageBox.Show("Required files added and loaded. Good to go!");
            UpdateList();
        }
    }
}
