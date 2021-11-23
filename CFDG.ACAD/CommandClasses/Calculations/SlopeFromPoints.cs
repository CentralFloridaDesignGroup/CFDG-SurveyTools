using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using CFDG.ACAD.Common;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace CFDG.ACAD.CommandClasses.Calculations
{
    public class SlopeFromPoints : ICommandMethod
    {
        [CommandMethod("SlopeFromPoints", CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public void InitialCommand()
        {
            Document doc = AcApplication.DocumentManager.MdiActiveDocument;
            Point3d startPnt = UserInput.SelectPointInDoc("Select your start point: ");
            if (startPnt == new Point3d(-1, -1, -1))
            {
                return;
            }
            Point3d endPnt = UserInput.SelectPointInDoc("Select your end point: ", startPnt);

            if (startPnt == new Point3d(-1, -1, -1) || endPnt == new Point3d(-1, -1, -1))
            {
                return;
            }

            if (DistanceWindow == null)
            {
                UI.SlopeDistance distanceWin = new UI.SlopeDistance(startPnt, endPnt);
                DistanceWindow = distanceWin;
                AcApplication.ShowModelessWindow(distanceWin);
                DistanceWindow.Closed += DistanceWindow_Closed;
            }
            DistanceWindow.Calculate(startPnt, endPnt);
        }

        #region Private Properties
        private static UI.SlopeDistance DistanceWindow;
        #endregion

        #region Private Methods
        private void DistanceWindow_Closed(object sender, EventArgs e)
        {
            DistanceWindow = null;
        }
        #endregion
    }
}
