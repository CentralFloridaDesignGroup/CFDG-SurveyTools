using System;
using System.Collections.Generic;
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
    public class JobToPDF
    {

        //[CommandMethod("PrintToPDF", CommandFlags.Modal | CommandFlags.NoBlockEditor)]
        public static void PrintSinglePDF()
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
            PlotEngine plotEngine = PlotFactory.CreatePreviewEngine((int)PreviewEngineFlags.Plot);
            using (plotEngine)
            {
                plotStatus = PlotHandler.Preview(plotEngine, layout);
                Logging.Debug($"{Environment.NewLine}Preview return: {plotStatus}");
                if (plotStatus == PreviewEndPlotStatus.Plot)
                {

                }
            }
        }
    }
}
