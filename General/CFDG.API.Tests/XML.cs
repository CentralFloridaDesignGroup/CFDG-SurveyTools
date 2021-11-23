using NUnit.Framework;

namespace CFDG.API.Tests
{
    [TestFixture]
    public class XML
    {
        [Theory]
        [TestCase("general", "companyabbreviation", "CFDG")]
        [TestCase("api", "testint", 12)]
        [TestCase("autocad", "enableosnapz", true)]
        [TestCase("autocad", "novalue", null)]
        public void GetValues(string category, string key, object expected)
        {
            dynamic value = API.XML.ReadValue(category, key);
            Assert.AreEqual(value, expected);
        }
    }
}
