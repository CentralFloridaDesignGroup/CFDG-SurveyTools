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
using CFDG.ACAD.Functions;

namespace CFDG.ACAD.CommandClasses.Export
{
    public class JobToPDF
    {

        [CommandMethod("PrintToPDF", CommandFlags.Modal | CommandFlags.NoBlockEditor)]
        public static void PrintSinglePDF()
        {
            (Document AcDoc, Editor AcEditor) = UserInput.GetCurrentDocSpace();
            if (!UserInput.IsInLayout())
            {
                UserInput.NoteError("Please be in a layout view, command does not work in model space.", true);
                return;
            }

            if (!UserInput.CheckForProjectFolder(AcDoc, out string jobPath)) { 
                UserInput.NoteError("Could not determine the project name or folder, see above for error.", true);
                return;
            }

            HandlePrint();

            AcEditor.WriteMessage($"{Environment.NewLine}Command \"PrintToPDF\" exited successfully.");
        }

        private static void HandlePrint()
        {
            (Document AcDoc, Editor AcEditor) = UserInput.GetCurrentDocSpace();
            LayoutManager layoutMgr = LayoutManager.Current;
            string layout = layoutMgr.CurrentLayout;
            if (PlotFactory.ProcessPlotState != ProcessPlotState.NotPlotting)
            {
                if (!UserInput.CheckForProjectFolder(AcDoc, out string jobPath))
                {
                    UserInput.NoteError("Cannot plot while another plot is running, please try again in a few seconds.");
                    return;
                }
            }

            PreviewEndPlotStatus plotStatus;
            PlotEngine plotEngine = PlotFactory.CreatePreviewEngine((int)PreviewEngineFlags.Plot);
            using (plotEngine)
            {
                plotStatus = PlotHandler.Preview(plotEngine, layout);
                AcEditor.WriteMessage($"{Environment.NewLine}Preview return: {plotStatus}");
                if (plotStatus == PreviewEndPlotStatus.Plot)
                {

                }
            }
        }
    }
}
