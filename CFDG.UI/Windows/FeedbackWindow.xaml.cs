using System;
using System.Windows;

namespace CFDG.UI
{
    /// <summary>
    /// Interaction logic for FeedbackWindow.xaml
    /// </summary>
    public partial class FeedbackWindow : Window
    {
        public FeedbackWindow()
        {
            InitializeComponent();
        }

        public FeedbackWindow(int product, Version version)
        {
            InitializeComponent();
            Product.SelectedIndex = product;
            productVersion.Text = version.ToString();
        }

        private void CmdCancel_Click(object s, RoutedEventArgs e)
        {

        }

        private void CmdSend_Click(object s, RoutedEventArgs e)
        {

        }
    }
}
