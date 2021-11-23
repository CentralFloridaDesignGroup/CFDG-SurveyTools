using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.AutoCAD.EditorInput;

namespace CFDG.ACAD.Common
{
    public class Logging
    {

        #region Public Methods
        /// <summary>
        /// Show information and log the action.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public static void Info(string message)
        {
            WriteAcMessage(ComposeMessage(message, MessageLevel.Info, false));
        }

        /// <summary>
        /// Show a warning and log the action.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public static void Warning(string message)
        {
            WriteAcMessage(ComposeMessage(message, MessageLevel.Warning, false));
        }

        /// <summary>
        /// Show an error and log the action.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public static void Error(string message)
        {
            Error(message, false);
        }

        /// <summary>
        /// Show an error and log the action and an optional modal dialog.
        /// </summary>
        /// <param name="message">Message to show.</param>
        /// <param name="showModalDialog">Show the modal dialog.</param>
        public static void Error(string message, bool showModalDialog)
        {
            WriteAcMessage(ComposeMessage(message, MessageLevel.Error, true));
            if (showModalDialog)
            {
                CreateModalDialog(message, MessageLevel.Error);
            }
        }

        /// <summary>
        /// Show a critial error and log the action.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public static void Critical(string message)
        {
            Critical(message, false);
        }

        /// <summary>
        /// Show a critical error and log the action and an optional modal dialog.
        /// </summary>
        /// <param name="message">Message to show.</param>
        /// <param name="showModalDialog">Show the modal dialog.</param>
        public static void Critical(string message, bool showModalDialog)
        {
            WriteAcMessage(ComposeMessage(message, MessageLevel.Critical, true));
            if (showModalDialog)
            {
                CreateModalDialog(message, MessageLevel.Critical);
            }
        }

        [Conditional("DEBUG")]
        /// <summary>
        /// Show a debug message and log the action.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public static void Debug(string message)
        {
            Critical(message, false);
        }

        [Conditional("DEBUG")]
        /// <summary>
        /// Show a debug message and log the action.
        /// </summary>
        /// <param name="message">Message to show.</param>
        /// <param name="showModalDialog">Show the modal dialog.</param>
        public static void Debug(string message, bool showModalDialog)
        {
            WriteAcMessage(ComposeMessage(message, MessageLevel.Debug, true));
            if (showModalDialog)
            {
                CreateModalDialog(message, MessageLevel.Debug);
            }
        }

        #endregion

        #region Private Methods and Enums


        /// <summary>
        /// Create an AutoCAD message for the editor console.
        /// </summary>
        /// <param name="message"></param>
        private static void WriteAcMessage(string message)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            acVariables.Editor.WriteMessage($"{Environment.NewLine}{message}");
        }

        /// <summary>
        /// Create a modal dialog with the specified <paramref name="level"/> and <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Message to show.</param>
        /// <param name="level">Level to show in the header.</param>
        private static void CreateModalDialog(string message, MessageLevel level)
        {
            MessageBox.Show(ComposeMessage(message, level, false), level.ToString(), MessageBoxButton.OK);
        }

        /// <summary>
        /// Composes the <paramref name="message"/> with the specified <paramref name="level"/> if opted to show.
        /// </summary>
        /// <param name="message">Message to show.</param>
        /// <param name="level">Level of the message.</param>
        /// <param name="showLevel">Show the level in the message.</param>
        /// <returns></returns>
        private static string ComposeMessage(string message, MessageLevel level, bool showLevel)
        {
            return $"{(showLevel ? $"[{level.ToString().ToUpper()}] " : "")}{message}";
        }

        /// <summary>
        /// Message level
        /// </summary>
        private enum MessageLevel
        {
            Info,
            Warning,
            Error,
            Critical,
            Debug
        }

        #endregion

    }
}
