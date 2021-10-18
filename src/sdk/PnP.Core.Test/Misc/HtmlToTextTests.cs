using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Utilities;

namespace PnP.Core.Test.Misc
{
    [TestClass()]
    public class HtmlToTextTests
    {
        [TestMethod()]
        public void SimpleText()
        {
            string input = "this is not html";
            string expected = "this is not html";
            var result = HtmlToText.ConvertSimpleHtmlToText(input, 1000);
            Assert.AreEqual(expected, result);
        }

        [TestMethod()]
        public void SimpleHtml()
        {
            string input = "this <B>is</B> not <Span><Span>html</Span></span>";
            string expected = "this is not html";
            var result = HtmlToText.ConvertSimpleHtmlToText(input, 1000);
            Assert.AreEqual(expected, result);
        }

        [TestMethod()]
        public void SpecialCharHtml()
        {
            string input = "<meta name=\"analytics-location\" content=\"/&lt;user-name&gt;/&lt;repo-name&gt;\" data-pjax-transient=\"true\">This &quot; &quot; &amp; &#39; &lt; &gt; &#160; is text</meta>";
            string expected = "This \" \" & ' < >   is text";
            var result = HtmlToText.ConvertSimpleHtmlToText(input, 1000);
            Assert.AreEqual(expected, result);
        }

        [TestMethod()]
        public void ComplexHtmlWithLengtLimit()
        {
            string input = "<title>GitHub - pnp/pnpcore: The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API&#39;s being called</title><meta name=\"description\" content=\"The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API&#39;s being called - GitHub - pnp/pnpcore: The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API&#39;s being called\"><link rel=\"search\" type=\"application/opensearchdescription+xml\" href=\"/opensearch.xml\" title=\"GitHub\"><link rel=\"fluid-icon\" href=\"https://github.com/fluidicon.png\" title=\"GitHub\">";
            string expected = "GitHub - pnp/pnpcore: The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with Sha";
            var result = HtmlToText.ConvertSimpleHtmlToText(input, 151);
            Assert.AreEqual(expected, result);
            Assert.IsTrue(expected.Length == 151);
        }

        //[TestMethod()]
        //public void ComplexHtmlWithLengtLimit2()
        //{
        //    string input = "<title>GitHub - pnp/pnpcore: The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API&#39;s being called. GitHub - pnp/pnpcore: The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API&#39;s being called. GitHub - pnp/pnpcore: The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API&#39;s being called</title><meta name=\"description\" content=\"The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API&#39;s being called - GitHub - pnp/pnpcore: The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API&#39;s being called\"><link rel=\"search\" type=\"application/opensearchdescription+xml\" href=\"/opensearch.xml\" title=\"GitHub\"><link rel=\"fluid-icon\" href=\"https://github.com/fluidicon.png\" title=\"GitHub\">";
        //    string expected = "GitHub - pnp/pnpcore: The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with Sha";
        //    var result = HtmlToText.ConvertSimpleHtmlToText(input, 511);
        //    Assert.AreEqual(expected, result);
        //    Assert.IsTrue(expected.Length == 511);
        //}
    }
}
