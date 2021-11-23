using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
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
            //TODO: Change PlotEngine creation and use to the PlotHandler class (easier implimentation later on).
            PlotEngine plotEngine = PlotFactory.CreatePreviewEngine((int)PreviewEngineFlags.Plot);
            using (plotEngine)
            {
                plotStatus = PlotHandler.Preview(plotEngine, layout);
                if (plotStatus != PreviewEndPlotStatus.Plot)
                {
                    return;
                }
            }


            UI.windows.Export.OpenFileDialog selectFileDialog = new UI.windows.Export.OpenFileDialog(acVariables.Document.Name, new List<string> { ".pdf" });
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(selectFileDialog);

            if (!(bool)selectFileDialog.DialogResult)
            {
                Logging.Info("File not selected.");
                return;
            }

            Logging.Debug($"{selectFileDialog.Directory} -> {selectFileDialog.FileName}");

            plotEngine = PlotFactory.CreatePublishEngine();
            using (plotEngine)
            {
                plotStatus = PlotHandler.Plot(plotEngine, layout, Path.Combine(selectFileDialog.Directory, selectFileDialog.FileName));
                Logging.Info("Plot created successfully.");
                if (selectFileDialog.OpenAfterCreation)
                {
                    Process.Start(Path.Combine(selectFileDialog.Directory, selectFileDialog.FileName));
                }
            }
        }
    }
}
