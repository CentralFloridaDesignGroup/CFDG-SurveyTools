using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFDG.API.Calcs
{
    public static class Angles
    {
        public static string AzimuthToBearing(double azimuth)
        {
            if (azimuth < 0)
            {
                return "";
            }
            if (azimuth > 360)
            {
                azimuth %= 360;
            }

            string output = "";

            string start = ((azimuth >= 0 && azimuth <= 90) || (azimuth >= 270 && azimuth <= 360)) ? "N" : "S";
            string end = (azimuth >= 0 && azimuth <= 180) ? "E" : "W";
            switch ($"{start}{end}")
            {
                case "NE":
                {
                    output += start + Convert.ToInt32(Math.Floor(azimuth)).ToString("00") + "°";
                    double nextVal = DivideAngle(azimuth);
                    output += Convert.ToInt32(Math.Floor(nextVal)).ToString("00") + "'";
                    nextVal = DivideAngle(nextVal);
                    output += Convert.ToInt32(Math.Round(nextVal)).ToString("00") + "\"" + end;
                    return output;
                }
                case "SW":
                {
                    azimuth -= 180;
                    output += start + Convert.ToInt32(Math.Floor(azimuth)).ToString("00") + "°";
                    double nextVal = DivideAngle(azimuth);
                    output += Convert.ToInt32(Math.Floor(nextVal)).ToString("00") + "'";
                    nextVal = DivideAngle(nextVal);
                    output += Convert.ToInt32(Math.Round(nextVal)).ToString("00") + "\"" + end;
                    return output;
                }
                case "SE":
                {
                    azimuth = 180 - azimuth;
                    output += start + Convert.ToInt32(Math.Floor(azimuth)).ToString("00") + "°";
                    double nextVal = DivideAngle(azimuth);
                    output += Convert.ToInt32(Math.Floor(nextVal)).ToString("00") + "'";
                    nextVal = DivideAngle(nextVal);
                    output += Convert.ToInt32(Math.Round(nextVal)).ToString("00") + "\"" + end;
                    return output;
                }
                case "NW":
                {
                    azimuth = 360 - azimuth;
                    output += start + Convert.ToInt32(Math.Floor(azimuth)).ToString("00") + "°";
                    double nextVal = DivideAngle(azimuth);
                    output += Convert.ToInt32(Math.Floor(nextVal)).ToString("00") + "'";
                    nextVal = DivideAngle(nextVal);
                    output += Convert.ToInt32(Math.Round(nextVal)).ToString("00") + "\"" + end;
                    return output;
                }

                default:
                {
                    return "";
                }
            }
        }

        public static double BearingToAzimuth(string bearing)
        {
            if (string.IsNullOrEmpty(bearing))
            {
                return -1;
            }
            //allowed passage: Ndd.mm.ssE
            string angle = bearing.Substring(1, bearing.Length - 2);
            string[] angleParts = angle.Split('.');
            double rawAngle = Convert.ToDouble(angleParts[0]) + (Convert.ToDouble(angleParts[1]) / 60) + (Convert.ToDouble(angleParts[2]) / 3600);
            string quadrant = $"{angle[0]}{angle[angle.Length - 1]}";
            switch (quadrant)
            {
                case "NE": return rawAngle;
                case "SW": return rawAngle + 180;
                case "SE": return 180 - rawAngle;
                case "NW": return 360 - rawAngle;
                default: return -1;
            }
        }

        internal static double DivideAngle(double angle)
        {
            angle -= Math.Floor(angle);
            return angle * 60;
        }
    }
}
