using System;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using CFDG.ACAD.Common;
using CFDG.API;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace CFDG.ACAD.CommandClasses.Calculations
{
    public class Footprint : ICommandMethod
    {
        #region Internal Properties

        public static Point3d CurrentPoint { get; set; }

        public static Point3d StartPoint { get; set; }
        public static double CurrentAngle { get; set; }


        #endregion


        [CommandMethod("Footprint", CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public void InitialCommand()
        {
            try
            {
                CurrentPoint = UserInput.SelectPointInDoc("Select a start point: ");
                StartPoint = CurrentPoint;
                CurrentAngle = UserInput.SelectAngleInDoc("Select a start angle: ", CurrentPoint);
                if (!EstablishLine(CurrentPoint, CurrentAngle))
                {
                    return;
                }
                while (true)
                {
                    if (!AddSide()) //If I don't add a side, consider the command to want to exit.
                    {
                        break;
                    }
                }

                Logging.Info($"\nMisclosure: {Math.Round(CurrentPoint.DistanceTo(StartPoint), 3)}\'\t" +
                    $"Error N-S: {Math.Round(CurrentPoint.Y - StartPoint.Y, 3)}\'\t" +
                    $"Error W-E: {Math.Round(CurrentPoint.X - StartPoint.X, 3)}\'\n");
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        private static bool EstablishLine(Point3d start, double angle)
        {
            var distanceStr = UserInput.GetStringFromUser("Enter the distance: ");
            if (!double.TryParse(distanceStr, out double distance))
            {
                if (!(distance > 0))
                {
                    return false;
                }
            }
            Triangle triangle = new Triangle(distance, angle);
            Point3d endPoint = new Point3d(start.X + triangle.SideA, start.Y + triangle.SideB, start.Z);
            CreateLine(start, endPoint);
            CurrentPoint = endPoint;
            return true;
        }

        private static bool AddSide()
        {
            double angle, distance;

            string distanceStr = UserInput.GetStringFromUser("Enter a distance for the side: ");
            if (string.IsNullOrEmpty(distanceStr))
            {
                return false;
            }
            Match match = Regex.Match(distanceStr, @"^(-?)(\+?)\d+(\.\d+)?(@\d+)?$");
            if (!match.Success)
            {
                Logging.Warning($"\nThe value \"{distanceStr}\" is not a valid input\n");
                return true; //Return true to not exit the command
            }
            if (distanceStr.Contains('@'))
            {
                string[] parts = distanceStr.Split('@');
                angle = double.Parse(parts[0]) > 0 ? double.Parse(parts[1]) * -1 : double.Parse(parts[1]);
                distance = Math.Abs(double.Parse(parts[0]));
            }
            else
            {
                angle = double.Parse(distanceStr) > 0 ? -90 : 90;
                distance = Math.Abs(double.Parse(distanceStr));
            }
            CurrentAngle += angle;
            Triangle triangle = new Triangle(distance, CurrentAngle);
            Point3d endPoint = new Point3d(CurrentPoint.X + triangle.SideA, CurrentPoint.Y + triangle.SideB, 0);
            CreateLine(CurrentPoint, endPoint);
            CurrentPoint = endPoint;
            return true;
        }

        private static void CreateLine(Point3d start, Point3d end)
        {
            Line line = new Line(start,end);
            UserInput.AddObjectToDrawing(line);
        }
    }
}
