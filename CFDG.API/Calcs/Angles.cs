﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                    output += start + ConvertDDToDMS(azimuth) + end;
                    return output;
                }
                case "SW":
                {
                    azimuth -= 180;
                    output += start + ConvertDDToDMS(azimuth) + end;
                    return output;
                }
                case "SE":
                {
                    azimuth = 180 - azimuth;
                    output += start + ConvertDDToDMS(azimuth) + end;
                    return output;
                }
                case "NW":
                {
                    azimuth = 360 - azimuth;
                    output += start + ConvertDDToDMS(azimuth) + end;
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
            string quadrant = $"{bearing[0]}{bearing[bearing.Length - 1]}";
            switch (quadrant)
            {
                case "NE": return rawAngle;
                case "SW": return rawAngle + 180;
                case "SE": return 180 - rawAngle;
                case "NW": return 360 - rawAngle;
                default: return -1;
            }
        }

        public static string ConvertBearing(string bearing)
        {
            bearing = bearing.ToUpper();
            Regex regex = new Regex("(N|S)(\\d{2})(\\.|D|°)(\\d{2})('?)(\\d{2})(\"?)(W|E)");
            if (!regex.IsMatch(bearing))
            {
                return "";
            }
            bearing = bearing.Replace("°", ".")
                .Replace("D", ".")
                .Replace("'", ".")
                .Replace("\"", "");
            if (bearing.Length == 9)
            {
                bearing = bearing.Insert(6, ".");
            }
            return bearing;
        }

        private static double DivideAngle(double angle)
        {
            angle -= Math.Floor(angle);
            return angle * 60;
        }

        private static string ConvertDDToDMS(double azimuth)
        {
            int degree;
            int minute;
            int second;
            double remainder;

            degree = Convert.ToInt32(Math.Floor(azimuth));
            remainder = DivideAngle(azimuth);
            minute = Convert.ToInt32(Math.Floor(remainder));
            remainder = DivideAngle(remainder);
            second = Convert.ToInt32(Math.Round(remainder));
            if (second == 60)
            {
                minute += 1;
                second = 0;
            }
            if (minute == 60)
            {
                degree += 1;
                minute = 0;
            }
            return $"{degree:00}°{minute:00}'{second:00}\"";
        }
    }
}
