using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class StringExtensionTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public void ETagExtensionTest()
        {
            string input = "\"{91E65772-7707-4C71-8C37-F2EF8F83AA5B},1\"";
            string expectedOutput = "91E65772-7707-4C71-8C37-F2EF8F83AA5B";
            string result = input.AsGraphEtag();

            Assert.IsTrue(!string.IsNullOrEmpty(result));
            Assert.AreEqual(expectedOutput, result);
            Assert.IsFalse(result.Contains(','));
            Assert.IsFalse(result.Contains('{'));
            Assert.IsFalse(result.Contains('}'));
        }

        [TestMethod]
        public void ETagExtensionDirtTest()
        {
            string input = "This is a bad string";
            string result = input.AsGraphEtag();

            Assert.IsTrue(!string.IsNullOrEmpty(result));
            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void ETagExtensionEmptyTest()
        {
            string input = "";
            string result = input.AsGraphEtag();

            Assert.IsTrue(string.IsNullOrEmpty(result));
        }
    }
}
