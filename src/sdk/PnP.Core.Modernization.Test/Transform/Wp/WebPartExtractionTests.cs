using AngleSharp.Html.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Pages;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PnP.Core.Modernization.Tests.Transform.Wp
{
    [TestClass]
    [TestCategory("Offline")]
    public class WebPartExtractionTests
    {
        private const string FirstId = "aca825e5-31bc-4779-9359-1443c735159a";
        private const string SecondId = "c6d53810-e48b-471a-ad8e-561f2b642ca6";
        private const string PageUrl = "/sites/example/SitePages/test.aspx";
        private const string Links = "<h2>Quick links</h2><ul>" +
            "<li><a href='/home.aspx'>Home</a></li><li><a href='/services.aspx'>Services</a></li>" +
            "<li><a href='https://example.org/signing?a=1&amp;b=2'>Signing</a></li>" +
            "<li><a href='https://example.org/feedback' target='_blank'>Feedback</a></li></ul>";

        private readonly Mock<ILogObserver> observer = new Mock<ILogObserver>();
        private readonly List<WebPartEntity> parts = new List<WebPartEntity>();
        private readonly List<BasePage.WebPartPlaceHolder> pending = new List<BasePage.WebPartPlaceHolder>();
        private PnPContext context;
        private WikiPage page;

        [TestInitialize]
        public void Initialize()
        {
            context = CoreTestContext.Create("https://contoso.sharepoint.com/sites/example");
            var item = CoreTestContext.Item(context);
            item.Values[Constants.FileRefField] = PageUrl;
            page = new WikiPage(item, BasePageTransformator.LoadDefaultWebPartMapping(), new[] { observer.Object });
        }

        [TestCleanup]
        public void Cleanup()
        {
            context.Dispose();
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("id='div_not-a-guid'")]
        [DataRow("class='" + FirstId + "'")]
        [DataRow("data-sp-cs-webpartid='g_aca825e5_31bc_4779_9359_1443c735159a'")]
        public void MissingControlIdPreservesRenderedHtml(string attributes)
        {
            Analyze("<p>Before</p>" + Box(Links, attributes) + "<p>After</p>");

            Assert.AreEqual(0, pending.Count);
            CollectionAssert.AreEqual(new[] { 6, 7, 8 }, parts.Select(part => part.Order).ToArray());
            using (var expected = new HtmlParser().ParseDocument(Box(Links, attributes)))
            {
                Assert.AreEqual(expected.QuerySelector(".ms-rte-wpbox").InnerHtml, parts[1].Properties["Text"]);
            }
            VerifyWarning("preserved");
        }

        [TestMethod]
        public void NestedBoxesPreserveTextOrderWithoutDuplicatingRegisteredContent()
        {
            Analyze("<div style='color:red'><p>Before</p>" +
                Box("First registered rendering", "id='div_" + FirstId + "'") + "<p>Between</p>" +
                Box(Links) + "<p>More text</p>" +
                Box("Second registered rendering", "id='div_" + SecondId + "'") + "<p>After</p></div>");

            CollectionAssert.AreEqual(new[] { FirstId, SecondId }, pending.Select(part => part.Id).ToArray());
            CollectionAssert.AreEqual(new[] { 7, 11 }, pending.Select(part => part.Order).ToArray());
            CollectionAssert.AreEqual(new[] { 6, 8, 9, 10, 12 }, parts.Select(part => part.Order).ToArray());
            using (var result = new HtmlParser().ParseDocument(string.Concat(parts.Select(part => part.Properties["Text"]))))
            {
                Assert.AreEqual("BeforeBetweenQuick linksHomeServicesSigningFeedbackMore textAfter", result.Body.TextContent);
                Assert.AreEqual(4, result.QuerySelectorAll("a").Length);
                Assert.AreEqual(4, result.QuerySelectorAll("div[style='color:red']").Length);
            }
            VerifyWarning("preserved");
        }

        [TestMethod]
        public void EmptyUnresolvableBoxWarnsWithoutAddingText()
        {
            Analyze("<div class='ms-rte-wpbox'></div>");

            Assert.AreEqual(0, parts.Count);
            Assert.AreEqual(0, pending.Count);
            VerifyWarning("no rendered HTML");
        }

        [DataTestMethod]
        [DataRow("pre", "text-decoration:line-through", false)]
        [DataRow("pre", "text-decoration:underline line-through", true)]
        [DataRow("section", "text-decoration-line:line-through", false)]
        [DataRow("article", "text-decoration:line-through solid red", false)]
        public void UnsupportedBlockReplacementPreservesDecoration(string tag, string style, bool underline)
        {
            Analyze($"<{tag} style='{style}'><code>obsolete --option</code>" +
                $"<p><a href='/guide'>Retired guidance</a></p></{tag}><p>Current guidance</p>");

            var html = string.Concat(parts.Select(part => part.Properties["Text"]));
            using (var result = new HtmlParser().ParseDocument(new HtmlTransformator().Transform(html, false)))
            {
                Assert.AreEqual(0, result.QuerySelectorAll(tag).Length);
                Assert.AreEqual("obsolete --option", result.QuerySelector("code s")?.TextContent);
                Assert.AreEqual("Retired guidance", result.QuerySelector("a s")?.TextContent);
                Assert.AreEqual("/guide", result.QuerySelector("a").GetAttribute("href"));
                Assert.AreEqual(underline, result.QuerySelector("code u") != null);
                Assert.IsTrue(result.QuerySelectorAll("s").All(element => element.TextContent != "Current guidance"));
                Assert.AreEqual("obsolete --optionRetired guidanceCurrent guidance",
                    result.Body.TextContent.Replace("\n", "").Replace("\r", ""));
            }
        }

        [TestMethod]
        public void MissingRegistrationPreservesRenderedHtml()
        {
            ConfigureLookup(null);
            CacheManager.Instance.SetSharePointVersion(context.Uri, SPVersion.SPO);
            Analyze("<p>Before</p>" + Box(Links, "id='div_" + FirstId + "'") + "<p>After</p>");
            var placeholder = pending.Single();
            observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Never);

            var file = context.Web.GetFileByServerRelativeUrl(PageUrl);
            page.LoadWebPartsInWikiContentFromServer(parts, file, pending);

            Assert.AreEqual(3, parts.Count);
            var fallback = parts.Single(part => part.Order == placeholder.Order);
            Assert.AreEqual((placeholder.Row, placeholder.Column), (fallback.Row, fallback.Column));
            Assert.AreEqual(placeholder.RenderedHtml, fallback.Properties["Text"]);
            VerifyWarning("preserved");
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RegisteredUnsupportedWebPartDoesNotUseHtmlFallback(bool hidden)
        {
            ConfigureLookup(new Dictionary<string, object>
            {
                ["Id"] = FirstId, ["ZoneId"] = "",
                ["WebPart"] = new Dictionary<string, object>
                {
                    ["Title"] = "Custom part",
                    ["ExportMode"] = 0, ["ZoneIndex"] = 0,
                    ["Hidden"] = hidden, ["IsClosed"] = hidden,
                    ["Properties"] = new Dictionary<string, object>()
                }
            });
            Analyze(Box(Links, "id='div_" + FirstId + "'"));
            Assert.AreEqual(0, parts.Count);
            Assert.AreEqual("g_" + FirstId.Replace("-", "_"), pending.Single().ControlId);
            observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Never);

            page.LoadWebPartsInWikiContentFromServer(parts, context.Web.GetFileByServerRelativeUrl(PageUrl), pending);

            var part = parts.Single();
            Assert.AreEqual(new Guid(FirstId), part.Id);
            Assert.AreEqual("Unsupported Web Part Type", part.Type);
            Assert.AreEqual("Custom part", part.Title);
            Assert.AreEqual(hidden, part.Hidden);
            Assert.AreEqual(hidden, part.IsClosed);
            Assert.IsFalse(part.Properties.ContainsKey("Text"));
            observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Once);
            observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                entry.Message == LogStrings.Warning_NotSupportedWebPart)), Times.Once);
        }

        private void Analyze(string html)
        {
            using (var document = new HtmlParser().ParseDocument(html))
            {
                page.AnalyzeWikiContentBlock(parts, document, pending, 2, 3, 5, document.Body);
            }
            Assert.IsTrue(parts.All(part => part.Row == 2 && part.Column == 3));
            Assert.IsTrue(pending.All(part => part.Row == 2 && part.Column == 3));
        }

        private void ConfigureLookup(object definition)
        {
            CoreTestContext.SetResponse(context, (request, _) =>
            {
                var path = request.RequestUri.AbsolutePath;
                if (path.EndsWith("/WebParts", StringComparison.OrdinalIgnoreCase))
                    return CoreTestContext.Json(new { value = definition == null ? Array.Empty<object>() : new[] { definition } });
                if (path.Contains("/GetFileBy", StringComparison.OrdinalIgnoreCase))
                    return CoreTestContext.Json(new { Name = "test.aspx", ServerRelativeUrl = PageUrl, Exists = true });
                throw new InvalidOperationException($"Unexpected web-part request: {request.RequestUri}");
            });
        }

        private static string Box(string html, string attributes = "")
        {
            return "<div class='ms-rtestate-read ms-rte-wpbox'><div " + attributes + ">" + html + "</div></div>";
        }

        private void VerifyWarning(string action)
        {
            observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                entry.Message.Contains("registered web part") && entry.Message.Contains(action))), Times.Once);
            observer.Verify(log => log.Error(It.IsAny<LogEntry>()), Times.Never);
        }
    }
}
