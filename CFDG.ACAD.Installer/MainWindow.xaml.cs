using System;
using System.IO;
using System.Windows;

namespace CFDG.ACAD.Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string DllFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\CFDG.ACAD.dll";

        //TODO: Change storage location of path to file from not displaying.
        public MainWindow()
        {
            InitializeComponent();
            DllFile = DllFile.Replace('\\', '/'); //Because why use the standard delineator for your file paths.
            UpdateList();
        }

        private void UpdateList()
        {
            LstVersions.Items.Clear();

            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string searchCriteria = Path.Combine(programFiles, "Autodesk");
            string[] Subdirectiores = Directory.GetDirectories(searchCriteria, "AutoCAD 20*");

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
                    }
                }
                else
                {
                    LstVersions.Items.Add(autoCad + " (File not found) | " + supportFile);
                }
            }
        }

        private void CmdCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CmdSubmit_Click(object sender, RoutedEventArgs e)
        {
            object selection = LstVersions.SelectedItem;
            if (selection == null)
            {
                MessageBox.Show("Nothing was selected");
                return;
            }

            string versionstr = LstVersions.SelectedValue.ToString();

            if (versionstr.Contains("Already"))
            {
                MessageBox.Show("Required files already present and correct.");
                return;
            }

            string[] versionSplit = versionstr.Split('|');
            string path = versionSplit[1].Remove(0, 1);
            File.AppendAllText(path, $"{Environment.NewLine}(command \"_netload\" \"{ DllFile }\")");
            MessageBox.Show("Required files added and loaded. Good to go!");
            UpdateList();
        }
    }
}
