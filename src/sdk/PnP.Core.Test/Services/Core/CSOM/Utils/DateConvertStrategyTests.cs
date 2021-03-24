using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services.Core.CSOM.Utils.DateHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.Services.Core.CSOM.Utils
{
    [TestClass]
    public class DateConvertStrategyTests
    {
        [TestMethod]
        public void TestParsingDateFromConstructorLikeString()
        {
            DateConstuctorStrategy strategy = new DateConstuctorStrategy();
            string inputString = "/Date(2019,11,5,17,7,26,0)/";

            DateTime expectedDate = new DateTime(2019, 11, 5, 17, 7, 26, 0);
            DateTime? actualDate = strategy.ConverDate(inputString);

            Assert.AreEqual(expectedDate, actualDate);
        }
        [TestMethod]
        public void TestParsingDateFromConstructorLikeString_Negative()
        {
            DateConstuctorStrategy strategy = new DateConstuctorStrategy();
            string inputString = "/Date(1612534319000)/";

            DateTime? actualDate = strategy.ConverDate(inputString);

            Assert.IsFalse(actualDate.HasValue);
        }
        [TestMethod]
        public void TestParsingDateFromMiliseconds()
        {
            FromMilisecondsConversionStrategy strategy = new FromMilisecondsConversionStrategy();
            string inputString = "/Date(1612534319000)/";

            DateTime expectedDate = new DateTime(2021, 2, 5, 14, 11, 59, 0);
            DateTime? actualDate = strategy.ConverDate(inputString);

            Assert.AreEqual(expectedDate, actualDate);
        }
        [TestMethod]
        public void TestParsingDateFromMiliseconds_Negative()
        {
            FromMilisecondsConversionStrategy strategy = new FromMilisecondsConversionStrategy();
            string inputString = "/Date(2019,11,5,17,7,26,0)/";

            DateTime? actualDate = strategy.ConverDate(inputString);

            Assert.IsFalse(actualDate.HasValue);
        }
        [TestMethod]
        public void CSOMDateConverter_Test_FromConstructor()
        {
            CSOMDateConverter converter = new CSOMDateConverter();
            string inputString = "/Date(2019,11,5,17,7,26,0)/";

            DateTime expectedDate = new DateTime(2019, 11, 5, 17, 7, 26, 0);
            DateTime? actualDate = converter.ConverDate(inputString);

            Assert.AreEqual(expectedDate, actualDate);
        }
        [TestMethod]
        public void CSOMDateConverter_Test_FromMiliseconds()
        {
            CSOMDateConverter converter = new CSOMDateConverter();
            string inputString = "/Date(1612534319000)/";

            DateTime expectedDate = new DateTime(2021, 2, 5, 14, 11, 59, 0);
            DateTime? actualDate = converter.ConverDate(inputString);

            Assert.AreEqual(expectedDate, actualDate);
        }
    }
}
