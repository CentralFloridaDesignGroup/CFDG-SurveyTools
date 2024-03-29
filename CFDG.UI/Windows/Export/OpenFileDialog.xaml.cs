﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace CFDG.UI.windows.Export
{
    //URGENT: Databinding 
    /// <summary>
    /// Interaction logic for OpenFileDialog.xaml
    /// </summary>
    public partial class OpenFileDialog : Window
    {
        
        internal struct ListInfo
        {
            public BitmapImage icon { get; set; }
            public string name { get; set; }
            public DateTime editDate { get; set; }

            public string type { get; set; }
        }

        internal string ProjectNumber { get; set; }

        internal readonly string CurrentDate = DateTime.Now.ToString("MM-dd-yyyy");

        internal bool IncludeDate { get; set; }
        internal bool AutomaticName { get; set; }
        internal string Description { get; set; }
        public string FileName { get; internal set; }
        public string Directory { get; internal set; }
        public bool OpenAfterCreation { get; internal set; }

        internal ContextMenu menu { get; set; }

        internal List<string> Filters { get; set; }


        private void UpdateName()
        {
            FileName = $"{ProjectNumber}{(Description.Length > 0 ? " " + Description : "")}{(IncludeDate ? " " + CurrentDate : "")}.pdf";
            TxtFileName.Text = FileName;
        }

        public OpenFileDialog(string filename)
        {
            HandleInitialization("Open File", filename, null);
        }

        public OpenFileDialog(string filename, List<string> filters)
        {
            HandleInitialization("Open File", filename, filters);
        }

        private void HandleInitialization(string title, string filePath, List<string> filter)
        {
            InitializeComponent();

            string currentDir = Path.GetDirectoryName(filePath);
            TxtCurrentPath.Text = currentDir;
            Directory = currentDir;

            ProjectNumber = Path.GetFileNameWithoutExtension(filePath);

            UpdateSettings();

            Description = "";
            UpdateName();

            Filters = filter;

            ContextMenu cm = new ContextMenu();
            MenuItem addFolder = new MenuItem();
            addFolder.Click += new RoutedEventHandler(CreateDirectory);
            addFolder.Header = "Add Folder";
            cm.Items.Add(addFolder);
            MenuItem remove = new MenuItem();
            remove.Click += new RoutedEventHandler(Deleteitem);
            remove.Header = "Delete";
            cm.Items.Add(remove);
            menu = cm;

            PopulateDirectory();
        }

        private void UpdateSettings()
        {
            IncludeDate = SaveDialogSettings.Default.AutoAddDate;
            ChkIncludeDate.IsChecked = SaveDialogSettings.Default.AutoAddDate;
            AutomaticName = SaveDialogSettings.Default.AutomaticName;
            ChkAutoNameFile.IsChecked = SaveDialogSettings.Default.AutomaticName;
            OpenAfterCreation = SaveDialogSettings.Default.OpenAfterSave;
            ChkOpenAfterCreate.IsChecked = SaveDialogSettings.Default.OpenAfterSave;
        }

        private void PopulateDirectory()
        {
            string[] dirs = System.IO.Directory.GetDirectories(Directory);
            string[] files = System.IO.Directory.GetFiles(Directory);

            LstDirList.Items.Clear();

            foreach (string dir in dirs)
            {
                DirectoryInfo info = new DirectoryInfo(dir);
                LstDirList.Items.Add(new ListInfo { 
                    icon = API.Imaging.BitmapToImageSource(API.Imaging.FolderLarge), 
                    name = info.Name, 
                    editDate = info.CreationTime, 
                    type="folder" 
                });
            }
            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                if (!info.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    if ((Filters != null && Filters.Contains(info.Extension)) || Filters == null)
                    {
                        LstDirList.Items.Add(new ListInfo { 
                            icon = API.Imaging.BitmapToImageSource(System.Drawing.Icon.ExtractAssociatedIcon(info.FullName)), 
                            name = info.Name, 
                            editDate = info.LastWriteTime, 
                            type="file" 
                        });
                    }
                }
            }

            CmdHome.Visibility = Directory != API.JobNumber.GetPath(ProjectNumber) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CmdCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ChkIncludeDate_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) { return; }
            IncludeDate = (bool)ChkIncludeDate.IsChecked;
            if (AutomaticName) { UpdateName(); }
        }
        private void ChkAutomaticName_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) { return; }
            AutomaticName = (bool)ChkAutoNameFile.IsChecked;
            if (AutomaticName) { UpdateName(); }
        }

        private void TxtDescription_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!IsInitialized) { return; }
            Description = TxtDescription.Text;
            UpdateName();
        }

        private void LstDirList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListInfo item = (ListInfo)LstDirList.SelectedItem;
            if (item.type == "folder")
            {
                string newPath = Path.Combine(Directory, item.name);
                Directory = newPath;
                TxtCurrentPath.Text = Directory;
                PopulateDirectory();
                return;
            }

            SelectAndSaveFile(item.name);
            return;
        }

        private void CmdDirUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string parent = System.IO.Directory.GetParent(Directory).FullName;
                Directory = parent;
                TxtCurrentPath.Text = Directory;
                PopulateDirectory();
            }
            catch
            {

            }
        }

        private void CmdSave_Click(object sender, RoutedEventArgs e)
        {
            string file = TxtFileName.Text;

            if (!file.Contains(Filters[0]))
            {
                file += Filters[0];
            }
            SelectAndSaveFile(file);
        }

        private void SelectAndSaveFile(string file)
        {
            string fileToCheck = Path.Combine(Directory, file);
            if (File.Exists(fileToCheck))
            {
                var res = MessageBox.Show("Are you sure you want to override the file?", "Confirm Override", MessageBoxButton.YesNo);
                if (res != MessageBoxResult.Yes)
                {
                    return;
                }
                try
                {
                    using (FileStream fs = new FileStream(fileToCheck, FileMode.Open))
                    {
                        fs.Close();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("The file is open in another application. Please either close the file or select another one.");
                    return;
                }
                
            }
            FileName = file;
            DialogResult = true;
            OpenAfterCreation = (bool)ChkOpenAfterCreate.IsChecked;
            Close();
        }

        private void TxtCurrentPath_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtCurrentPath.Text))
            {
                return;
            }

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                string dir = TxtCurrentPath.Text;
                if (System.IO.Directory.Exists(dir))
                {
                    Directory = dir;
                    PopulateDirectory();
                }
            }
        }

        private void TxtJobNumber_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SwitchJob();
            }
        }

        private void SwitchJob()
        {

            if (API.JobNumber.TryParse(TxtJobNumber.Text, API.JobNumberFormats.ShortHyphan, out string jobNumber))
            {
                string newPath = API.JobNumber.GetPath(jobNumber);
                Directory = newPath;
                TxtCurrentPath.Text = Directory;
                PopulateDirectory();
            }
        }

        private void CmdOpenProject_Click(object sender, RoutedEventArgs e)
        {
            SwitchJob();
        }

        private void CmdHome_Click(object sender, RoutedEventArgs e)
        {
            Directory = API.JobNumber.GetPath(ProjectNumber);
            TxtCurrentPath.Text = Directory;
            PopulateDirectory();
        }

        private void TxtCurrentPath_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtCurrentPath.SelectAll();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e, string directory, string file)
        {
            FileSystemWatcher fsw = new FileSystemWatcher(directory)
            {
                Filter = file,
                NotifyFilter = NotifyFilters.Attributes
                    | NotifyFilters.CreationTime
                    | NotifyFilters.DirectoryName
                    | NotifyFilters.FileName
                    | NotifyFilters.LastAccess
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Security
                    | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            WaitForChangedResult fileCreated = fsw.WaitForChanged(WatcherChangeTypes.Created, 2 * 60 * 100);
        }

        private void LstDirList_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            object item = LstDirList.SelectedItem;

            if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
            {
                MenuItem deleteOption = (MenuItem)menu.Items[1];
                deleteOption.Visibility = item == null ? Visibility.Collapsed : Visibility.Visible;
                menu.PlacementTarget = LstDirList;
                menu.IsOpen = true;
                e.Handled = true;
            }
            else if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                if (item == null)
                {
                    e.Handled = true;
                    return;
                }

                var itemInfo = (ListInfo)item;
                if (itemInfo.type == "file")
                {
                    TxtFileName.Text = itemInfo.name;
                    AutomaticName = false;
                    ChkAutoNameFile.IsChecked = false;
                }
                e.Handled = true;
            }
        }

        private void CreateDirectory(object sender, RoutedEventArgs e)
        {
            TextMessageBoxResult res = Common.TextMessageBox.Show("Please enter a name for the folder.", out string dialogTextResult);
            if (res != TextMessageBoxResult.OK)
            {
                return;
            }
            System.IO.Directory.CreateDirectory(Path.Combine(Directory, dialogTextResult));
            PopulateDirectory();
        }

        private void Deleteitem(object s, RoutedEventArgs e)
        {
            ListInfo item = (ListInfo)LstDirList.SelectedItem;

            if(MessageBox.Show($"Are you sure you want to delete {item.name}? This cannot be undone.", "Confirm deletion", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }
            try
            {

                if (item.type == "folder")
                {
                    System.IO.Directory.Delete(Path.Combine(Directory, item.name), true);
                    PopulateDirectory();
                    return;
                }
                using (FileStream fs = new FileStream(Path.Combine(Directory, item.name), FileMode.Open))
                {
                    fs.Close();
                }
                File.Delete(Path.Combine(Directory, item.name));
                PopulateDirectory();
                return;
            }
            catch
            {
                MessageBox.Show("The file is open in another application. Please either close the file or select another one.");
                return;
            }
        }

        private void CmdSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow window = new SettingsWindow();
            var result = window.ShowDialog();
            if (result == true)
            {
                UpdateSettings();
            }
        }
    }
}
