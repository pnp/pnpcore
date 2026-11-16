using AngleSharp.Html.Parser;
using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Functions;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using PnP.Framework.Utilities.UnitTests.Model;
using PnP.Framework.Utilities.UnitTests.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using PnPCore = PnP.Core.Model.SharePoint;

namespace PnP.Core.Modernization.Test.Transform.Functions
{
    [TestClass]
    [TestCategory("Offline")]
    public class BuiltInSummaryLinksTests
    {
        private const string Host = "https://contoso.sharepoint.com";
        private const string SourceUrl = Host + "/sites/summary-source";
        private const string TargetUrl = Host + "/sites/summary-target";
        private const string Group = "<div title='_link'><div title='_title'>Group one</div><div title='_groupstyle'>DefaultHeader</div></div>";

        [DataTestMethod]
        [DataRow("")]
        [DataRow("<div title='_linkurl'></div>")]
        [DataRow("<div title='_linkurl'><a>no href</a></div>")]
        [DataRow("<div title='_linkurl'><a href=''></a></div>")]
        [DataRow("<div title='_linkurl'><a href=' &#9; '></a></div>")]
        public void GetLinks_IncompleteEntryIsSkippedWithDiagnostic(string urlField)
        {
            var observer = new Mock<ILogObserver>();
            var transformator = new SummaryLinksHtmlTransformator(new[] { observer.Object });

            var links = transformator.GetLinks(Entry("Incomplete", urlField));

            Assert.AreEqual(0, links.Count);
            VerifyWarning(observer, "Incomplete", "skipped");
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("<div title='_linkurl'></div>")]
        [DataRow("<div title='_linkurl'><a>no href</a></div>")]
        [DataRow("<div title='_linkurl'><a href=''></a></div>")]
        [DataRow("<div title='_linkurl'><a href=' &#9; '></a></div>")]
        public void TextTransform_IncompleteEntryRetainsEncodedTitleWithoutHyperlink(string urlField)
        {
            var observer = new Mock<ILogObserver>();
            var transformator = new SummaryLinksHtmlTransformator(new[] { observer.Object });

            using (var document = new HtmlParser().ParseDocument(
                transformator.Transform(Entry("A &amp; &lt;script&gt;", urlField), false)))
            {
                Assert.AreEqual("A & <script>", document.QuerySelector("li").TextContent);
                Assert.AreEqual(0, document.QuerySelectorAll("a, script").Length);
            }
            VerifyWarning(observer, "A & <script>", "plain text");
        }

        [TestMethod]
        public void GetLinks_PreservesValidUrlsAndMetadataWithoutTreatingGroupsAsMissingUrls()
        {
            var observer = new Mock<ILogObserver>();
            var transformator = new SummaryLinksHtmlTransformator(new[] { observer.Object });
            var html = Group + Link("Relative", "../Pages/relative.aspx") +
                Entry("Absolute", UrlField("https://example.org/page?a=1&amp;b=2") +
                    "<div title='_description'>Description</div><div title='_imageurl'><a href='/image.png'></a></div>" +
                    "<div title='_imageurlalttext'>Image</div><div title='_linktooltip'>Tooltip</div>" +
                    "<div title='_style'>Default</div><div title='_openinnewwindow'>true</div>");

            var links = transformator.GetLinks(html);

            CollectionAssert.AreEqual(new[] { "../Pages/relative.aspx", "https://example.org/page?a=1&b=2" },
                links.Select(link => link.Url).ToArray());
            Assert.AreEqual("Absolute", links[1].Title);
            Assert.AreEqual("Description", links[1].Description);
            Assert.AreEqual("/image.png", links[1].ImageUrl);
            Assert.AreEqual("Image", links[1].ImageAltText);
            Assert.AreEqual("Tooltip", links[1].ToolTip);
            Assert.AreEqual("Default", links[1].Style);
            Assert.IsTrue(links[1].OpenInNewWindow);
            observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Never);
        }

        [DataTestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public void QuickLinksMapping_MixedEntriesProducesOnlyValidContiguousItems(bool crossSite, bool skipUrlRewrite)
        {
            using (var source = CreateContext(SourceUrl))
            using (var target = CreateContext(crossSite ? TargetUrl : SourceUrl))
            {
                var observer = new Mock<ILogObserver>();
                var html = Group + Entry("Missing", "<div title='_imageurl'><a href='/sites/summary-source/SiteAssets/unused.png'></a></div>") +
                    Link("Relative", "../Pages/relative.aspx") +
                    Entry("Empty", UrlField("")) + Link("Server relative", "/sites/summary-source/SitePages/local.aspx") +
                    Entry("Whitespace", UrlField(" &#9; ")) + Link("Absolute", "https://example.org/page.aspx") +
                    Link("Source absolute", SourceUrl + "/SitePages/absolute.aspx");

                var output = ProcessMapping(source, target, observer, html, true, skipUrlRewrite);
                var json = JObject.Parse(output);
                var properties = (JObject)json["properties"];
                var processed = json["serverProcessedContent"];
                var links = (JObject)processed["links"];
                var texts = (JObject)processed["searchablePlainTexts"];

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, properties["items"].Select(item => (int)item["id"]).ToArray());
                Assert.AreEqual(5, links.Count);
                Assert.AreEqual(13, texts.Count);
                Assert.AreEqual("../Pages/relative.aspx", (string)links["items[0].sourceItem.url"]);
                Assert.AreEqual(crossSite && !skipUrlRewrite ? "/sites/summary-target/sitepages/local.aspx" : "/sites/summary-source/SitePages/local.aspx",
                    (string)links["items[1].sourceItem.url"]);
                Assert.AreEqual("https://example.org/page.aspx", (string)links["items[2].sourceItem.url"]);
                var expectedAbsolute = crossSite
                    ? (skipUrlRewrite ? "/sites/summary-source/SitePages/absolute.aspx" : "/sites/summary-target/sitepages/absolute.aspx")
                    : SourceUrl + "/SitePages/absolute.aspx";
                Assert.AreEqual(expectedAbsolute, (string)links["items[3].sourceItem.url"]);
                CollectionAssert.AreEqual(new[] { "Relative", "Server relative", "Absolute", "Source absolute" },
                    Enumerable.Range(0, 4).Select(index => (string)texts[$"items[{index}].title"]).ToArray());
                Assert.AreEqual(0, ((JObject)processed["imageSources"]).Count);
                Assert.IsFalse(output.Contains("{JsonProperties}"));
                VerifyWarning(observer, "Missing", "skipped");
                VerifyWarning(observer, "Empty", "skipped");
                VerifyWarning(observer, "Whitespace", "skipped");
                observer.Verify(log => log.Warning(It.Is<LogEntry>(entry => entry.Message.Contains("Summary link"))), Times.Exactly(3));
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void QuickLinksMapping_AllIncompleteEntriesProducesNoBlankItemsAndIsHidden(bool skipUrlRewrite)
        {
            using (var source = CreateContext(SourceUrl))
            using (var target = CreateContext(TargetUrl))
            {
                var observer = new Mock<ILogObserver>();
                var html = Entry("PB3-summary-link-one", "") + Entry("PB3-summary-link-two", UrlField(""));
                var json = JObject.Parse(ProcessMapping(source, target, observer, html, true, skipUrlRewrite));

                Assert.AreEqual(0, ((JArray)json["properties"]["items"]).Count);
                Assert.IsTrue((bool)json["properties"]["hideWebPartWhenEmpty"]);
                Assert.AreEqual(1, ((JObject)json["serverProcessedContent"]["links"]).Count);
                Assert.AreEqual(1, ((JObject)json["serverProcessedContent"]["searchablePlainTexts"]).Count);
                Assert.AreEqual(0, ((JObject)json["serverProcessedContent"]["imageSources"]).Count);
                VerifyWarning(observer, "PB3-summary-link-one", "skipped");
                VerifyWarning(observer, "PB3-summary-link-two", "skipped");
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void TextMapping_MixedEntriesPreservesGroupsTitlesAndValidLinks(bool skipUrlRewrite)
        {
            using (var source = CreateContext(SourceUrl))
            using (var target = CreateContext(TargetUrl))
            {
                var observer = new Mock<ILogObserver>();
                var html = Group + Entry("Missing", "") +
                    Entry("Valid", UrlField("/sites/summary-source/SitePages/local.aspx") + "<div title='_openinnewwindow'>true</div>") +
                    Group.Replace("Group one", "Group two") + Entry("Empty", UrlField("")) +
                    Link("Relative", "../Pages/relative.aspx") + Link("Absolute", "https://example.org/page.aspx");

                using (var document = new HtmlParser().ParseDocument(ProcessMapping(source, target, observer, html, false, skipUrlRewrite)))
                {
                    Assert.AreEqual("Summary links", document.QuerySelector("h3").TextContent);
                    CollectionAssert.AreEqual(new[] { "Group one", "Group two" },
                        document.QuerySelectorAll("strong").Select(element => element.TextContent).ToArray());
                    CollectionAssert.AreEqual(new[] { "MissingValid", "EmptyRelativeAbsolute" },
                        document.QuerySelectorAll("ul").Select(element => element.TextContent).ToArray());
                    Assert.AreEqual(5, document.QuerySelectorAll("li").Length);
                    var anchors = document.QuerySelectorAll("a");
                    Assert.AreEqual(3, anchors.Length);
                    Assert.AreEqual(skipUrlRewrite ? "/sites/summary-source/SitePages/local.aspx" : "/sites/summary-target/sitepages/local.aspx",
                        anchors[0].GetAttribute("href"));
                    Assert.AreEqual("_blank", anchors[0].GetAttribute("target"));
                    Assert.AreEqual("off", anchors[0].GetAttribute("data-interception"));
                    Assert.AreEqual("../Pages/relative.aspx", anchors[1].GetAttribute("href"));
                    Assert.AreEqual("https://example.org/page.aspx", anchors[2].GetAttribute("href"));
                }
                VerifyWarning(observer, "Missing", "plain text");
                VerifyWarning(observer, "Empty", "plain text");
                observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Exactly(2));
            }
        }

        private static string ProcessMapping(ClientContext source, ClientContext target, Mock<ILogObserver> observer,
            string html, bool useQuickLinks, bool skipUrlRewrite)
        {
            var transformation = BasePageTransformator.LoadDefaultWebPartMapping();
            var mapping = transformation.WebParts.Single(part => part.Type == WebParts.SummaryLink);
            var webPart = new WebPartEntity { Type = mapping.Type };
            foreach (var property in mapping.Properties)
            {
                webPart.Properties[property.Name] = string.Empty;
            }
            var additionalProperties = new Dictionary<string, string>
            {
                ["SummaryLinkStore"] = html, ["ChromeType"] = "1", ["Title"] = "Summary links",
                ["Host"] = Host, ["Web"] = new Uri(target.Url).AbsolutePath,
                ["SummaryLinksToQuickLinks"] = useQuickLinks.ToString()
            };
            foreach (var property in additionalProperties)
            {
                webPart.Properties[property.Key] = property.Value;
                if (!mapping.Properties.Any(value => value.Name == property.Key))
                {
                    mapping.Properties = mapping.Properties.Concat(new[]
                    {
                        new Property { Name = property.Key, Type = PropertyType.@string }
                    }).ToArray();
                }
            }
            var page = new Mock<PnPCore.IPage>();
            page.SetupGet(value => value.PageTitle).Returns("summary-links-test");
            var information = new PageTransformationInformation(null) { SkipTelemetry = true, SkipUrlRewrite = skipUrlRewrite };
            var processor = new FunctionProcessor(source, target, page.Object, transformation, information, new[] { observer.Object });
            var selected = processor.Process(ref mapping, webPart);
            Assert.AreEqual(useQuickLinks ? "UseQuickLinks" : "UseText", selected);
            var selectedMapping = mapping.Mappings.Mapping.Single(value => value.Name == selected);
            processor.ProcessMappingFunctions(ref mapping, webPart, selectedMapping);
            observer.Verify(log => log.Error(It.IsAny<LogEntry>()), Times.Never);

            return useQuickLinks
                ? WebUtility.HtmlDecode(TokenParser.ReplaceTokens(selectedMapping.ClientSideWebPart.Single().JsonControlData, webPart))
                : TokenParser.ReplaceTokens(selectedMapping.ClientSideText.Single().Text, webPart);
        }

        private static ClientContext CreateContext(string siteUrl)
        {
            CacheManager.Instance.SetSharePointVersion(new Uri(siteUrl), SPVersion.SPO);
            var provider = new MockEntryResponseProvider();
            var web = new Dictionary<string, object>
            {
                ["_ObjectType_"] = "SP.Web", ["Url"] = siteUrl, ["ServerRelativeUrl"] = new Uri(siteUrl).AbsolutePath,
                ["Id"] = "/Guid(9d03e0aa-cb94-4463-b885-15a55d03ed52)/"
            };
            provider.ResponseEntries.Add(new MockResponseEntry<object> { Url = siteUrl, PropertyName = "Web", ReturnValue = web });
            provider.ResponseEntries.Add(new MockResponseEntry<object> { Url = siteUrl, PropertyName = "RootWeb", ReturnValue = web });
            provider.ResponseEntries.Add(new MockResponseEntry<object>
            {
                Url = siteUrl, PropertyName = "Site",
                ReturnValue = new Dictionary<string, object>
                {
                    ["_ObjectType_"] = "SP.Site", ["Url"] = siteUrl, ["ServerRelativeUrl"] = new Uri(siteUrl).AbsolutePath,
                    ["Id"] = "/Guid(c6815669-f27c-4ff6-b937-0825b2cdfa93)/", ["RootWeb"] = web
                }
            });
            provider.ResponseEntries.Add(new MockResponseEntry<object>
            {
                Url = siteUrl, Method = "GetFileByServerRelativeUrl",
                ReturnValue = new Dictionary<string, object>
                {
                    ["_ObjectType_"] = "SP.File",
                    ["ListId"] = "/Guid(f266cd97-f155-4360-b802-47bb9f0d9b55)/",
                    ["UniqueId"] = "/Guid(a9b5b53e-2014-47d9-b26c-fc8d52047d1e)/"
                }
            });
            return new ClientContext(siteUrl) { WebRequestExecutorFactory = new MockWebRequestExecutorFactory(provider) };
        }

        private static void VerifyWarning(Mock<ILogObserver> observer, string title, string action)
        {
            observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                entry.Message.Contains(title) && entry.Message.Contains("URL") && entry.Message.Contains(action))), Times.Once);
            observer.Verify(log => log.Error(It.IsAny<LogEntry>()), Times.Never);
        }

        private static string Entry(string title, string fields)
        {
            return $"<div title='_link'><div title='_title'>{title}</div>{fields}</div>";
        }

        private static string UrlField(string url)
        {
            return $"<div title='_linkurl'><a href='{url}'></a></div>";
        }

        private static string Link(string title, string url)
        {
            return Entry(title, UrlField(url));
        }
    }
}
