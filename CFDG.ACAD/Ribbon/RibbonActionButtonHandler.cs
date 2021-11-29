using System;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace CFDG.ACAD
{
    public class RibbonActionButtonHandler : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            RibbonActionButton actionButton = parameter as RibbonActionButton;

            if (actionButton != null)
            {
                ActionExecute(actionButton.CommandAction, actionButton.ReferenceTextBox.TextValue);
                actionButton.ReferenceTextBox.TextValue = "";
            }
        }

        internal void ActionExecute(Action<string> commandMethod, string param)
        {
            commandMethod(param);
        }
    }
}
