using AngleSharp.Html.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Transform;
using System.Linq;

namespace PnP.Core.Modernization.Test.Transform.RichText
{
    [TestClass]
    [TestCategory("Offline")]
    public class HtmlTransformatorTests
    {
        private const string Indent = "<blockquote style='margin:0px 0px 0px 40px;border:none;padding:0px;'>";

        [DataTestMethod]
        [DataRow("A plain text answer.")]
        [DataRow("Remove KeyContainer from the .inf file.<br>The system creates a unique container.")]
        [DataRow("<strong>Signing error.</strong> Use an approved keycode <span>and retry</span>.")]
        [DataRow("Review <a href='https://example.org/docs?a=1&amp;b=2'>the documentation</a>: the certificate subject must match the manifest publisher.")]
        [DataRow("<p>Before.</p>Unwrapped answer.<div>After.</div>")]
        [DataRow("Keep &lt;KeyContainer&gt; &amp; &quot;container name&quot; as text.")]
        [DataRow("<!-- before -->Answer<!-- after -->")]
        [DataRow("<p>Existing paragraph.</p><div>Existing block.</div>")]
        public void Transform_PreservesAllIndentedTextInOrder(string content)
        {
            var html = Indent + content + "</blockquote>";
            using (var source = new HtmlParser().ParseDocument(html))
            using (var result = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                Assert.AreEqual(source.Body.TextContent, result.Body.TextContent);
                Assert.AreEqual(0, result.QuerySelectorAll("blockquote").Length);
                Assert.AreEqual(0, result.QuerySelectorAll("keycontainer").Length);
                CollectionAssert.AreEqual(
                    source.QuerySelectorAll("a").Select(anchor => anchor.GetAttribute("href")).ToArray(),
                    result.QuerySelectorAll("a").Select(anchor => anchor.GetAttribute("href")).ToArray());
                CollectionAssert.AreEqual(
                    source.QuerySelectorAll("strong").Select(element => element.TextContent).ToArray(),
                    result.QuerySelectorAll("strong").Select(element => element.TextContent).ToArray());
                Assert.AreEqual(source.QuerySelectorAll("br").Length, result.QuerySelectorAll("br").Length);
                Assert.IsTrue(result.QuerySelectorAll("p, div").All(element =>
                    element.GetAttribute("style").Contains("margin-left: 40px")));
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Transform_PreservesTroubleshootingAnswers(bool usePlaceHolder)
        {
            var html = "<h2>Troubleshooting</h2>" +
                Indent + "<span>Signing error:</span> The request uses a keycode that has not been approved. " +
                "Check the input JSON and application ID.<span> Retry after approval.</span></blockquote>" +
                Indent + "<strong>Package signing error.</strong> Review " +
                "<a href='https://example.org/troubleshooting'>the documentation</a>: " +
                "the certificate subject must match the AppxManifest.xml publisher name.<br></blockquote>" +
                "<p>Resolution:</p>" + Indent +
                "Do not specify KeyContainer in the .inf file.<br>The OS creates a unique container.</blockquote>";

            using (var source = new HtmlParser().ParseDocument(html))
            using (var result = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, usePlaceHolder)))
            {
                Assert.AreEqual(source.Body.TextContent, result.Body.TextContent);
                Assert.AreEqual(1, result.QuerySelectorAll("a").Length);
                Assert.AreEqual("https://example.org/troubleshooting", result.QuerySelector("a").GetAttribute("href"));
            }
        }

        [DataTestMethod]
        [DataRow("", null)]
        [DataRow("text-decoration:underline;", "u")]
        [DataRow("text-decoration:line-through;", "s")]
        public void Transform_PreservesNestedIndentationAndInheritedFormatting(string decoration, string tag)
        {
            var html = "<blockquote style='margin:0px 0px 0px 40px;" + decoration + "'>" +
                Indent + "Unwrapped nested answer.</blockquote></blockquote>";
            using (var result = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                Assert.AreEqual("Unwrapped nested answer.", result.Body.TextContent);
                Assert.AreEqual(0, result.QuerySelectorAll("blockquote").Length);
                StringAssert.Contains(result.QuerySelector("p").GetAttribute("style"), "margin-left: 80px");
                if (tag != null)
                {
                    Assert.AreEqual("Unwrapped nested answer.", result.QuerySelector(tag).TextContent);
                }
            }
        }
    }
}
