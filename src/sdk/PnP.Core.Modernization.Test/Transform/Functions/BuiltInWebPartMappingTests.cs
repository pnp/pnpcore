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
using System.Reflection;
using System.Xml.Serialization;

namespace PnP.Core.Modernization.Test.Transform.Functions
{
    [TestClass]
    [TestCategory("Offline")]
    public class BuiltInWebPartMappingTests
    {
        private const string SourceUrl = "https://contoso.sharepoint.com/sites/source";
        private const string TargetUrl = "https://contoso.sharepoint.com/sites/target";
        private const string LibraryUrl = "/sites/target/Pictures";
        private const string PageViewer = "Microsoft.SharePoint.WebPartPages.PageViewerWebPart";
        private const string Slideshow = "Microsoft.SharePoint.WebPartPages.PictureLibrarySlideshowWebPart";
        private static readonly Guid SourceLibraryId = Guid.Parse("8f0bd5a1-7466-49b1-b210-07d49cbdd91a");
        private static readonly Guid TargetLibraryId = Guid.Parse("00b93254-042b-4815-8c62-2b16c7269c51");

        [DataTestMethod]
        [DataRow("https://example.org/embed?p=1&other=2#anchor", "4", "https://example.org/embed?p=1&other=2#anchor")]
        [DataRow("/sites/source/Pages/test.aspx", "8", SourceUrl + "/Pages/test.aspx")]
        [DataRow("Pages/test.aspx?x=1#section", "8", SourceUrl + "/Pages/test.aspx?x=1#section")]
        [DataRow("//example.org/embed", "8", "https://example.org/embed")]
        public void PageViewerMapping_ResolvesUrls(string link, string sourceType, string expected)
        {
            using (var context = CreateContext(SourceUrl))
            {
                var data = Transform(context, context, PageViewer, new Dictionary<string, string>
                {
                    ["ContentLink"] = link, ["SourceType"] = sourceType
                });
                AssertEmbed(data, expected);
            }
        }

        [DataTestMethod]
        [DataRow(false, TargetUrl + "/sitepages/test.aspx?x=1#section")]
        [DataRow(true, SourceUrl + "/SitePages/test.aspx?x=1#section")]
        public void PageViewerMapping_HonorsCrossSiteUrlRewrite(bool skipRewrite, string expected)
        {
            using (var source = CreateContext(SourceUrl))
            using (var target = CreateContext(TargetUrl))
            {
                var data = Transform(source, target, PageViewer, new Dictionary<string, string>
                {
                    ["ContentLink"] = "SitePages/test.aspx?x=1#section", ["SourceType"] = "8"
                }, skipRewrite: skipRewrite);
                AssertEmbed(data, expected);
            }
        }

        [TestMethod]
        public void PageViewerMapping_PreservesExternalUrlAndEscaping()
        {
            using (var source = CreateContext(SourceUrl))
            using (var target = CreateContext(TargetUrl))
            {
                const string link = "https://example.org/sites/source/embed?name=a&copy=1&literal=&quot;&quoted=\"hello\"&token={Title}";
                var observer = new Mock<ILogObserver>();
                var data = Transform(source, target, PageViewer, new Dictionary<string, string>
                {
                    ["ContentLink"] = link, ["SourceType"] = "4", ["Title"] = "Do not substitute"
                }, observer.Object);
                AssertEmbed(data, new Uri(link).AbsoluteUri);
                observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                    entry.Message.Contains("HTML Field Security") && entry.Message.Contains("framing"))), Times.Once);
            }
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("javascript:alert(1)")]
        [DataRow("file:///C:/test.html")]
        [DataRow(@"https:\example.org\test")]
        [DataRow("java\nscript:alert(1)")]
        [DataRow("https://user:password@example.org/")]
        [DataRow("https://")]
        public void PageViewerMapping_RejectsInvalidContentLinks(string link)
        {
            using (var context = CreateContext(SourceUrl))
            {
                var exception = Assert.ThrowsException<TargetInvocationException>(() =>
                    Transform(context, context, PageViewer, new Dictionary<string, string>
                    {
                        ["ContentLink"] = link, ["SourceType"] = "4"
                    }));
                Assert.IsInstanceOfType(exception.InnerException, typeof(NotAvailableAtTargetException));
            }
        }

        [DataTestMethod]
        [DataRow("1")]
        [DataRow("2")]
        public void PageViewerMapping_SkipsFileAndFolderWithoutResolvingContent(string sourceType)
        {
            using (var context = CreateContext(SourceUrl))
            {
                var observer = new Mock<ILogObserver>();
                Assert.IsNull(Transform(context, context, PageViewer, new Dictionary<string, string>
                {
                    ["ContentLink"] = @"\\server\share", ["SourceType"] = sourceType
                }, observer.Object));
                observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                    entry.Message.Contains("source type") && entry.Message.Contains("skipped"))), Times.Once);
            }
        }

        [DataTestMethod]
        [DataRow(3, 3, "2", true, 3, true)]
        [DataRow(1, 2, "TitleBelow", false, 0, true)]
        [DataRow(15, 10, "0", false, 2, false)]
        [DataRow(0, 5, "NoTitleDesc", true, 4, false)]
        public void SlideshowMapping_PreservesSettingsAndDestinationLibrary(
            int speed, int expectedSpeed, string layout, bool toolbar, int chromeType, bool showTitle)
        {
            using (var source = CreateContext(SourceUrl))
            using (var target = CreateContext(TargetUrl))
            {
                var observer = new Mock<ILogObserver>();
                var values = SlideshowProperties();
                const string title = "Pictures \"Summer\" &quot; <tag> {Title} $0";
                values["Speed"] = speed.ToString(System.Globalization.CultureInfo.InvariantCulture);
                values["Layout"] = layout;
                values["ShowToolbar"] = toolbar.ToString();
                values["Title"] = title;
                values["ChromeType"] = chromeType.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var data = Transform(source, target, Slideshow, values, observer.Object);
                var properties = data["properties"];
                var carousel = properties["carouselSettings"];
                var metadata = layout != "0" && layout != "NoTitleDesc";
                Assert.AreEqual(expectedSpeed, (int)carousel["autoplaySpeed"]);
                Assert.AreEqual(toolbar, (bool)carousel["dots"]);
                Assert.AreEqual(metadata, (bool)carousel["metadata"]);
                Assert.IsTrue((bool)carousel["autoplay"]);
                Assert.AreEqual(TargetLibraryId.ToString(), (string)properties["listId"]);
                Assert.AreEqual(LibraryUrl, (string)properties["listUrl"]);
                Assert.AreEqual(LibraryUrl, (string)data["serverProcessedContent"]["links"]["listUrl"]);
                Assert.AreEqual(title, (string)properties["title"]);
                Assert.AreEqual(title, (string)data["serverProcessedContent"]["searchablePlainTexts"]["title"]);
                Assert.AreEqual(showTitle, (bool)properties["isTitleEnabled"]);
                Assert.IsTrue((bool)properties["hasDynamicModeEnabled"]);
                Assert.IsTrue((bool)properties["canAlwaysSelectDocLibAsSource"]);
                Assert.AreEqual(2, (int)properties["imageSourceType"]);
                observer.Verify(log => log.Warning(It.Is<LogEntry>(entry => entry.Message.Contains("speed"))),
                    speed == expectedSpeed ? Times.Never() : Times.Once());
                observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                    entry.Message.Contains("NoTitleDesc") && entry.Message.Contains("caption visibility"))),
                    metadata ? Times.Never() : Times.Once());
            }
        }

        [TestMethod]
        public void SlideshowMapping_ReportsViewAndRandomOrderLimitations()
        {
            using (var source = CreateContext(SourceUrl))
            using (var target = CreateContext(TargetUrl))
            {
                var values = SlideshowProperties();
                values["Mode"] = "1";
                values["ViewGuid"] = "67c64919-ab2f-4c2e-80b0-8debb0927da9";
                var observer = new Mock<ILogObserver>();
                Transform(source, target, Slideshow, values, observer.Object);
                observer.Verify(log => log.Warning(It.Is<LogEntry>(entry => entry.Message.Contains("view") &&
                    entry.Message.Contains("filtering"))), Times.Once);
                observer.Verify(log => log.Warning(It.Is<LogEntry>(entry => entry.Message.Contains("random ordering"))), Times.Once);
            }
        }

        [TestMethod]
        public void SlideshowMapping_UnconfiguredLibraryIsSkipped()
        {
            using (var context = CreateContext(SourceUrl))
            {
                var values = SlideshowProperties();
                values["LibraryGuid"] = Guid.Empty.ToString();
                var exception = Assert.ThrowsException<TargetInvocationException>(() =>
                    Transform(context, context, Slideshow, values));
                Assert.IsInstanceOfType(exception.InnerException, typeof(NotAvailableAtTargetException));
            }
        }

        private static void AssertEmbed(JObject data, string expected)
        {
            var properties = data["properties"];
            var markup = (string)properties["embedCode"];
            Assert.AreEqual(markup, (string)properties["cachedEmbedCode"]);
            using (var document = new HtmlParser().ParseDocument(markup))
            {
                var iframe = document.QuerySelector("iframe");
                Assert.IsNotNull(iframe);
                Assert.AreEqual(expected, iframe.GetAttribute("src"));
                Assert.AreEqual(1, document.Body.Children.Length);
            }
        }

        private static Dictionary<string, string> SlideshowProperties()
        {
            return new Dictionary<string, string>
            {
                ["LibraryGuid"] = SourceLibraryId.ToString(), ["Layout"] = "TitleBelow",
                ["Speed"] = "7", ["ShowToolbar"] = "true", ["ViewGuid"] = Guid.Empty.ToString(),
                ["Mode"] = "Sequential", ["Title"] = "Pictures", ["ChromeType"] = "0"
            };
        }

        private static JObject Transform(ClientContext source, ClientContext target, string type,
            Dictionary<string, string> values, ILogObserver observer = null, bool skipRewrite = false)
        {
            var assembly = typeof(BuiltIn).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .Single(name => name.EndsWith("Modernization.webpartmapping.xml", StringComparison.Ordinal));
            PageTransformation transformation;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                transformation = (PageTransformation)new XmlSerializer(typeof(PageTransformation)).Deserialize(stream);
            }
            var mapping = transformation.WebParts.Single(part => part.Type.Split(',')[0] == type);
            mapping.Properties = mapping.Properties.Concat(transformation.BaseWebPart.Properties
                .Where(property => mapping.Properties.All(existing => existing.Name != property.Name))).ToArray();
            var webPart = new WebPartEntity { Type = mapping.Type };
            foreach (var property in mapping.Properties)
            {
                webPart.Properties[property.Name] = values.TryGetValue(property.Name, out var value) ? value : "";
            }
            var information = new PageTransformationInformation(null) { SkipTelemetry = true, SkipDefaultUrlRewrite = skipRewrite };
            var processor = new FunctionProcessor(source, target, null, transformation, information,
                observer == null ? null : new[] { observer });
            var selected = processor.Process(ref mapping, webPart);
            var result = mapping.Mappings.Mapping.Single(item => selected == null ? item.Default : item.Name == selected);
            processor.ProcessMappingFunctions(ref mapping, webPart, result);
            if (result.ClientSideWebPart == null)
            {
                Assert.IsNull(result.ClientSideText);
                return null;
            }
            Assert.AreEqual(type == Slideshow ? ClientSideWebPartType.ImageGallery : ClientSideWebPartType.ContentEmbed,
                result.ClientSideWebPart.Single().Type);
            return JObject.Parse(WebUtility.HtmlDecode(TokenParser.ReplaceTokens(
                result.ClientSideWebPart.Single().JsonControlData, webPart)));
        }

        private static ClientContext CreateContext(string url)
        {
            CacheManager.Instance.SetSharePointVersion(new Uri(url), SPVersion.SPO);
            var provider = new MockEntryResponseProvider();
            foreach (var property in new[] { "Site", "Web" })
            {
                provider.ResponseEntries.Add(new MockResponseEntry<object>
                {
                    Url = url, PropertyName = property,
                    ReturnValue = new Dictionary<string, object>
                    {
                        ["_ObjectType_"] = "SP." + property,
                        ["ServerRelativeUrl"] = new Uri(url).AbsolutePath, ["Url"] = url
                    }
                });
            }
            var library = new Dictionary<string, object>
            {
                ["_ObjectType_"] = "SP.List",
                ["Id"] = $"/Guid({(url == SourceUrl ? SourceLibraryId : TargetLibraryId)})/",
                ["Title"] = "Pictures",
                ["RootFolder"] = new Dictionary<string, object>
                {
                    ["_ObjectType_"] = "SP.Folder", ["ServerRelativeUrl"] = LibraryUrl
                }
            };
            provider.ResponseEntries.Add(new MockResponseEntry<object>
            {
                Url = url, Method = "GetListByTitle", ReturnValue = library
            });
            provider.ResponseEntries.Add(new MockResponseEntry<object>
            {
                Url = url, PropertyName = "Lists",
                ReturnValue = new Dictionary<string, object>
                {
                    ["_ObjectType_"] = "SP.ListCollection",
                    ["_Child_Items_"] = new[] { library }
                }
            });
            return new ClientContext(url) { WebRequestExecutorFactory = new MockWebRequestExecutorFactory(provider) };
        }
    }
}
