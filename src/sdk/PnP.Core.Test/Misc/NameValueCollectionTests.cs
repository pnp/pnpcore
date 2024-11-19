using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using System.Web;

namespace PnP.Core.Test.Misc
{
    [TestClass()]
    public class NameValueCollectionTests
    {
        [TestMethod()]
        public void CombineUriPathWithPartsTest()
        {
            string inputValue = "Fullständig Behörighet,+=";
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            // Add key and value, which will be automatically URL-encoded, if needed
            queryString.Add("filter", inputValue);

            string expected = queryString.ToString();
            // In .NET Framework the .ToString() returns data wrongly encoded, hence we're
            // using our own ToEncodedString() method
            Assert.AreEqual(expected, queryString.ToEncodedString(), true);
        }
    }
}
