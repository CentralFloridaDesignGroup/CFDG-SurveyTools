using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [TestCase("S45.30.00E", 136.5)]
        [TestCase("N00.00.00E", 0)]
        [TestCase("N00.00.00E", 0)]
        public void GetAzimuthFromBearing(string bearing, double expected)
        {
            double result = API.Calcs.Angles.BearingToAzimuth(bearing);
            Assert.AreEqual(expected, result);
        }
    }
}
