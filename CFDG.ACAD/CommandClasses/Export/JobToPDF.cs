using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Runtime;
using CFDG.ACAD.Common;

namespace CFDG.ACAD.CommandClasses.Export
{
    public class JobToPDF : ICommandMethod
    {

        [CommandMethod("PrintToPDF", CommandFlags.Modal | CommandFlags.NoBlockEditor)]
        public void InitialCommand()
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            if (!UserInput.IsInLayout())
            {
                Logging.Error("Please be in a layout view, command does not work in model space.", true);
                return;
            }

            if (!UserInput.CheckForProjectFolder(acVariables.Document, out _)) {
                Logging.Error("Could not determine the project name or folder, see above for error.", true);
                return;
            }

            HandlePrint();

            Logging.Debug($"{Environment.NewLine}Command \"PrintToPDF\" exited successfully.");
        }

        private static void HandlePrint()
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            LayoutManager layoutMgr = LayoutManager.Current;
            string layout = layoutMgr.CurrentLayout;
            if (PlotFactory.ProcessPlotState != ProcessPlotState.NotPlotting)
            {
                if (!UserInput.CheckForProjectFolder(acVariables.Document, out _))
                {
                    Logging.Error("Cannot plot while another plot is running, please try again in a few seconds.");
                    return;
                }
            }

            PreviewEndPlotStatus plotStatus;
            plotStatus = PlotHandler.Preview(layout);
            if (plotStatus != PreviewEndPlotStatus.Plot)
            {
                return;
            }

            UI.windows.Export.OpenFileDialog selectFileDialog = new UI.windows.Export.OpenFileDialog(acVariables.Document.Name, new List<string> { ".pdf" });
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(selectFileDialog);

            if (!(bool)selectFileDialog.DialogResult)
            {
                Logging.Info("File not selected.");
                return;
            }

            Logging.Debug($"{selectFileDialog.Directory} -> {selectFileDialog.FileName}");

            PlotHandler.Plot(layout, Path.Combine(selectFileDialog.Directory, selectFileDialog.FileName));
            Logging.Info("Plot created successfully.");
            if (selectFileDialog.OpenAfterCreation)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                WaitandOpenFile(Path.Combine(selectFileDialog.Directory, selectFileDialog.FileName));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private async static Task WaitandOpenFile(string path)
        {
            await Task.Run(() =>
            {
                while (!File.Exists(path)) { }
                Process.Start(path);
            });
            Logging.Debug("File created, located, and opened.");
        }
    }
}
