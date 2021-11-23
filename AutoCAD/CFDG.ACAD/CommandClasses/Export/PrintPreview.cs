using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.PlottingServices;
using CFDG.ACAD.Common;

namespace CFDG.ACAD.CommandClasses.Export
{
    public class PlotHandler
    {
        public static PreviewEndPlotStatus Preview(PlotEngine pe, string layout)
        {
            return PlotHandle(pe, layout, "");
        }

        public static PreviewEndPlotStatus Plot(PlotEngine pe, string layout, string filePath)
        {
            return PlotHandle(pe, layout, filePath);
        }

        private static PreviewEndPlotStatus PlotHandle(PlotEngine pe, string layout, string saveDir)
        {
            AcVariablesStruct acVariables = UserInput.GetCurrentDocSpace();
            bool isPlot = !string.IsNullOrEmpty(saveDir); //string is empty -> preview; string isn't empty -> plot.

            PreviewEndPlotStatus returnValue;
            using (Transaction AcTransaction = acVariables.Database.TransactionManager.StartTransaction())
            {
                Layout layoutObject = (Layout)AcTransaction.GetObject(LayoutManager.Current.GetLayoutId(layout), OpenMode.ForRead);

                // We'll be plotting the current layout
                BlockTableRecord btr = (BlockTableRecord)AcTransaction.GetObject(layoutObject.BlockTableRecordId, OpenMode.ForRead);
                Layout lo = (Layout)AcTransaction.GetObject(btr.LayoutId, OpenMode.ForRead);

                PlotSettings ps = new PlotSettings(lo.ModelType);
                ps.CopyFrom(lo);

                PlotInfo pi = new PlotInfo
                {
                    Layout = btr.LayoutId
                };

                // Make the layout we're plotting current
                LayoutManager.Current.CurrentLayout = lo.LayoutName;
                // We need to link the PlotInfo to the PlotSettings and then validate it
                pi.OverrideSettings = ps;
                PlotInfoValidator piv = new PlotInfoValidator
                {
                    MediaMatchingPolicy = MatchingPolicy.MatchEnabled
                };

                piv.Validate(pi);

                // Create a Progress Dialog to provide info and allow thej user to cancel
                PlotProgressDialog ppd = new PlotProgressDialog(!isPlot, 1, true);
                using (ppd)
                {
                    ppd.set_PlotMsgString(PlotMessageIndex.DialogTitle, isPlot ? "Print Progress" : "Preview Progress");
                    ppd.set_PlotMsgString(PlotMessageIndex.SheetName, acVariables.Document.Name.Substring(acVariables.Document.Name.LastIndexOf("\\") + 1));
                    ppd.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");
                    ppd.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Cancel Sheet");
                    ppd.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
                    ppd.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Sheet Progress");
                    ppd.LowerPlotProgressRange = 0;
                    ppd.UpperPlotProgressRange = 100;
                    ppd.PlotProgressPos = 0;

                    // Let's start the plot/preview, at last
                    ppd.OnBeginPlot();
                    ppd.IsVisible = true;
                    pe.BeginPlot(ppd, null);

                    // We'll be plotting/previewing a single document
                    pe.BeginDocument(pi, acVariables.Document.Name, null, 1, isPlot, saveDir);

                    // Which contains a single sheet
                    ppd.OnBeginSheet();
                    ppd.LowerSheetProgressRange = 0;
                    ppd.UpperSheetProgressRange = 100;
                    ppd.SheetProgressPos = 0;
                    PlotPageInfo ppi = new PlotPageInfo();
                    pe.BeginPage(ppi, pi, true, null);
                    pe.BeginGenerateGraphics(null);
                    ppd.SheetProgressPos = 50;
                    pe.EndGenerateGraphics(null);

                    // Finish the sheet
                    PreviewEndPlotInfo pepi = new PreviewEndPlotInfo();
                    pe.EndPage(pepi);
                    returnValue = pepi.Status;
                    ppd.SheetProgressPos = 100;
                    ppd.OnEndSheet();

                    // Finish the document
                    pe.EndDocument(null);

                    // And finish the plot
                    ppd.PlotProgressPos = 100;
                    ppd.OnEndPlot();
                    pe.EndPlot(null);
                }
                // Committing is cheaper than aborting
                AcTransaction.Commit();
            }
            return returnValue;
        }
    }
}
