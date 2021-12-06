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

namespace CFDG.UI.windows.Common
{
    /// <summary>
    /// Interaction logic for TextMessageBoxForm.xaml
    /// </summary>
    internal partial class TextMessageBoxForm : Window
    {
        public string UserInput;

        public TextMessageBoxResult Result;

        internal TextMessageBoxForm(string msg, string ttl, string presetVal)
        {
            InitializeComponent();
            Result = TextMessageBoxResult.Cancel;
            TxtMessage.Text = msg;
            Title = ttl;
            if (!string.IsNullOrEmpty(presetVal))
            {
                TxtInput.Text = presetVal;
            }
        }

        private void CmdSubmit_Click(object sender, RoutedEventArgs e)
        {
            UserInput = TxtInput.Text;
            Result = TextMessageBoxResult.OK;
            Close();
        }

        private void CmdCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TxtInput_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtInput.SelectAll();
        }
    }

    public static class TextMessageBox
    {
        public static TextMessageBoxResult Show(string message, out string result)
        {
            return ShowHandler(message, "Input Request", "", out result);
        }

        public static TextMessageBoxResult Show(string message, string title, out string result)
        {
            return ShowHandler(message, title, "", out result);
        }

        public static TextMessageBoxResult Show(string message, string title, string presetValue, out string result)
        {
            return ShowHandler(message, title, presetValue, out result);
        }

        private static TextMessageBoxResult ShowHandler(string message, string title, string presetVal, out string result)
        {
            TextMessageBoxForm form = new TextMessageBoxForm(message, title, presetVal)
            {
                ShowInTaskbar = false
            };
            form.ShowDialog();
            result = form.UserInput;
            return form.Result;
        }
    }
}
