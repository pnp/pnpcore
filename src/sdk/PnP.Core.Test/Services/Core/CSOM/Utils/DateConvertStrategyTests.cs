using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services.Core.CSOM.Utils.DateHelpers;
using PnP.Core.Test.Utilities;
using System;

namespace PnP.Core.Test.Services.Core.CSOM.Utils
{
    [TestClass]
    public class DateConvertStrategyTests
    {

        [TestMethod]
        public void TestParsingFromDateTimeStrategy1()
        {
            FromDateTimeStrategy strategy = new FromDateTimeStrategy();
            string inputString = "/Date(2019,11,5,17,7,26,0)/";

            DateTime expectedDate = new DateTime(2019, 12, 5, 17, 7, 26, 0);
            DateTime? actualDate = strategy.ConverDate(inputString);

            Assert.AreEqual(expectedDate, actualDate);
        }

        [TestMethod]
        public void TestParsingFromDateTimeStrategy2()
        {
            FromDateTimeStrategy strategy = new FromDateTimeStrategy();
            string inputString = "/Date(2022,0,5,17,7,26,0)/";

            DateTime expectedDate = new DateTime(2022, 1, 5, 17, 7, 26, 0);
            DateTime? actualDate = strategy.ConverDate(inputString);

            Assert.AreEqual(expectedDate, actualDate);
        }

        [TestMethod]
        public void TestParsingFromDateTimeStrategy3()
        {
            FromDateTimeStrategy strategy = new FromDateTimeStrategy();
            string inputString = "/Date(2022,2,29,9,40,58,397)/";

            DateTime expectedDate = new DateTime(2022, 3, 29, 9, 40, 58, 397);
            DateTime? actualDate = strategy.ConverDate(inputString);

            Assert.AreEqual(expectedDate, actualDate);
        }

        [TestMethod]
        public void TestParsingFromDateTimeStrategy4()
        {
            FromDateTimeStrategy strategy = new FromDateTimeStrategy();
            string inputString = "/Date(1612534319000)/";

            DateTime expectedDate = new DateTime(2021, 2, 5, 14, 11, 59, 0);
            DateTime? actualDate = strategy.ConverDate(inputString);

            Assert.AreEqual(expectedDate, actualDate);
        }

        [TestMethod]
        public void TestParsingFromDateTimeStrategy5()
        {
            FromDateTimeStrategy strategy = new FromDateTimeStrategy();
            string inputString = "/Date(-50827680000)/";

            DateTime expectedDate = new DateTime(1968, 5, 22, 17, 12, 0, 0);
            DateTime? actualDate = strategy.ConverDate(inputString);

            Assert.AreEqual(expectedDate, actualDate);
        }

        [TestMethod]
        public void TestParsingFromDateTimeStrategy6()
        {
            FromDateTimeStrategy strategy = new FromDateTimeStrategy();
            string inputString = "/Date(1243037520000-0700)/";

            DateTime expectedDate = new DateTime(2009, 5, 23, 0, 12, 0, 0, DateTimeKind.Utc).ToLocalTime();
            DateTime? actualDate = strategy.ConverDate(inputString);

            Assert.AreEqual(expectedDate, actualDate);
        }
        
        [TestMethod]
        public void TestParsingFromDateTimeStrategy7()
        {
            FromDateTimeStrategy strategy = new FromDateTimeStrategy();
            string inputString = "/Date(1243037520000+0300)/";

            DateTime expectedDate = new DateTime(2009, 5, 23, 0, 12, 0, 0, DateTimeKind.Utc).ToLocalTime();
            DateTime? actualDate = strategy.ConverDate(inputString);

            if (!TestCommon.RunningInGitHubWorkflow())
            {
                Assert.AreEqual(expectedDate, actualDate);
            }
        }

        [TestMethod]
        public void TestParsingFromDateTimeStrategy8()
        {
            FromDateTimeStrategy strategy = new FromDateTimeStrategy();
            string inputString = "/Date(1243037520000)/";

            DateTime expectedDate = new DateTime(2009, 5, 23, 0, 12, 0, 0, DateTimeKind.Utc);
            DateTime? actualDate = strategy.ConverDate(inputString);

            if (!TestCommon.RunningInGitHubWorkflow())
            {
                Assert.AreEqual(expectedDate, actualDate);
            }
        }

        [TestMethod]
        public void TestParsingFromDateTimeStrategyNegative1()
        {
            FromDateTimeStrategy strategy = new FromDateTimeStrategy();
            string inputString = "";

            DateTime? actualDate = strategy.ConverDate(inputString);

            Assert.IsFalse(actualDate.HasValue);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestParsingFromDateTimeStrategyNegative2()
        {
            FromDateTimeStrategy strategy = new FromDateTimeStrategy();
            string inputString = "/Date(2019,11,A,17,7,26,0)/";

            strategy.ConverDate(inputString);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestParsingFromDateTimeStrategyNegative3()
        {
            FromDateTimeStrategy strategy = new FromDateTimeStrategy();
            string inputString = "/Date(2019,11,5,17,7,26,0,888)/";

            strategy.ConverDate(inputString);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestParsingFromDateTimeStrategyNegative4()
        {
            FromDateTimeStrategy strategy = new FromDateTimeStrategy();
            string inputString = "/Date(2022,12,29,9,40,58,397)/";

            strategy.ConverDate(inputString);
        }
    }
}
