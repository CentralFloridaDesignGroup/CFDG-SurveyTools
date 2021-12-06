using System;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using ACApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace CFDG.ACAD
{
    public class RibbonButtonHandler : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }
#pragma warning disable CS0067 //command is never used
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

        public void Execute(object parameter)

        {
            // Grab the command associated with the button
            RibbonButton cmd = parameter as RibbonButton;

            Document dwg = ACApplication.DocumentManager.MdiActiveDocument;

            // Send the command to the application in the current document
            dwg.SendStringToExecute(cmd.CommandParameter as string, true, false, true);

        }
    }
}