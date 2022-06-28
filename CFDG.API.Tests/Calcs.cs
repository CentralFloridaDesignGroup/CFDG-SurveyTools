using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using CFDG.API.Calcs;
using NUnit.Framework;

namespace CFDG.API.Tests
{
    [TestFixture]
    public class Calcs
    {
        [Theory]
        [TestCase(0.000, "N00°00'00\"E")]
        [TestCase(30.5, "N30°30'00\"E")]
        [TestCase(200.5, "S20°30'00\"W")]
        [TestCase(180, "S00°00'00\"E")]
        [TestCase(170.25, "S09°45'00\"E")]
        [TestCase(270, "N90°00'00\"W")]
        [TestCase(314.5181, "N45°28'55\"W")]
        [TestCase(360, "N00°00'00\"W")]
        public void GetBearingFromAzimuth(double azimuth, string expected)
        {
            string result = API.Calcs.Angles.AzimuthToBearing(azimuth);
            Assert.AreEqual(expected, result);
        }

        [Theory]
        [TestCase("N00.00.00E", 0)]
        [TestCase("N15.15.00E", 15.25)]
        [TestCase("S46.00.00E", 134)]
        [TestCase("S46.00.00W", 226)]
        [TestCase("N45.00.00W", 315)]
        public void GetAzimuthFromBearing(string bearing, double expected)
        {
            double result = API.Calcs.Angles.BearingToAzimuth(bearing);
            Assert.AreEqual(expected, result);
        }

        [Theory]
        [TestCase("N45°05'02\"E", "N45.05.02E")]
        [TestCase("N45D05'02\"E", "N45.05.02E")]
        [TestCase("N45.0502E", "N45.05.02E")]
        [TestCase("S45.0502W", "S45.05.02W")]
        [TestCase("S45.0000E", "S45.00.00E")]
        [TestCase("N15°06'59\"W", "N15.06.59W")]
        public void ConvertBearing(string bearing, string expected)
        {
            string result = API.Calcs.Angles.ConvertBearing(bearing);
            Assert.AreEqual(expected, result);
        }
    }

}
