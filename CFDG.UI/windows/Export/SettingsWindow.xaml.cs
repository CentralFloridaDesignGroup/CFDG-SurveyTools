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

namespace CFDG.UI.windows.Export
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ChkAutoAddDate.IsChecked = SaveDialogSettings.Default.AutoAddDate;
            ChkAutomaticName.IsChecked = SaveDialogSettings.Default.AutomaticName;
            ChkOpenOnSave.IsChecked = SaveDialogSettings.Default.OpenAfterSave;
        }

        private void CmdSave_Click(object sender, RoutedEventArgs e)
        {
            SaveDialogSettings.Default.AutoAddDate = (bool)ChkAutoAddDate.IsChecked;
            SaveDialogSettings.Default.AutomaticName = (bool)ChkAutomaticName.IsChecked;
            SaveDialogSettings.Default.OpenAfterSave = (bool)ChkOpenOnSave.IsChecked;
            SaveDialogSettings.Default.Save();
            DialogResult = true;
            this.Close();
        }

        private void CmdCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
