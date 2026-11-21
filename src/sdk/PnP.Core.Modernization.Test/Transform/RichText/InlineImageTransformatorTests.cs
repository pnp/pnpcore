using AngleSharp.Html.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PnP.Core.Modernization;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Functions;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;


namespace PnP.Core.Modernization.Test.Transform.RichText
{
    [TestClass]
    [TestCategory("Offline")]
    public class InlineImageTransformatorTests
    {
        private const string SiteUrl = "https://contoso.sharepoint.com/sites/source";
        private const string ImageUrl = "/sites/source/SiteAssets/image.png";
        private const string TargetImageUrl = "/sites/target/SiteAssets/SitePages/page/image.png";
        private static readonly Uri SourcePageUrl = new Uri(SiteUrl + "/Pages/page.aspx");
        private PnPContext coreContext;

        [TestCleanup]
        public void DisposeCoreContext()
        {
            coreContext?.Dispose();
        }

        [DataTestMethod]
        [DataRow("47494638376110000800", 16, 8)]
        [DataRow("47494638396110000800", 16, 8)]
        [DataRow("89504E470D0A1A0A0000000D4948445200000140000000A0", 320, 160)]
        [DataRow("FFD8FFE000040000FFC00008080008001001FFD9", 16, 8)]
        [DataRow("FFD8FFFFC20008080100020001FFD9", 512, 256)]
        public void SystemImageSize_RecognizesImageDimensions(string hex, int width, int height)
        {
            var header = Convert.FromHexString(hex);
            var result = SystemImageSize.Read(header, header.Length);
            Assert.AreEqual(width, result.Width);
            Assert.AreEqual(height, result.Height);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("47494638396110")]
        [DataRow("47494638396100000800")]
        [DataRow("89504E470D0A1A0A0000000D494844528000000000000001")]
        [DataRow("FFD8FFE0FFFF")]
        [DataRow("FFD8FFC00002080008001001")]
        [DataRow("FFD8FFC00008080000001001")]
        [DataRow("FFD8FFDA0008000000000000")]
        [DataRow("3C68746D6C3E")]
        public void SystemImageSize_RejectsInvalidOrUnknownHeaders(string hex)
        {
            var header = Convert.FromHexString(hex);
            Assert.ThrowsException<InvalidDataException>(() => SystemImageSize.Read(header, header.Length));
        }

        [DataTestMethod]
        [DataRow("/_layouts/images/icgen.gif")]
        [DataRow("/_layouts/15/images/icgen.gif")]
        [DataRow("/sites/source/_layouts/14/images/icgen.gif")]
        [DataRow("/sites/source/_LAYOUTS/15/IMAGES/icgen.gif?version=1#icon")]
        [DataRow("https://contoso.sharepoint.com/_layouts/images/icgen.gif")]
        [DataRow("../_layouts/images/icgen.gif")]
        public void Transform_SystemImagesUseNativeImagesWithoutFileLookup(string source)
        {
            var page = CreatePage();
            var observer = new Mock<ILogObserver>();
            var transformator = new InlineImageTransformator(page.Object, SourcePageUrl,
                _ => throw new AssertFailedException("System images must not be transferred"), value => value,
                new[] { observer.Object },
                imageLookup: _ => throw new AssertFailedException("System images have no library metadata"),
                importExternalImage: _ => throw new AssertFailedException("Internal system images must not be imported"),
                systemImageSizeLookup: _ => throw new AssertFailedException("Explicit widths must not require a download"));
            var html = $"<p>before<a href='/details'><img class='ms-asset-icon ms-rtePosition-4' src='{source}' alt='A &amp; B' width='16' height='16'></a>after</p>";

            var result = transformator.Transform(html);
            Assert.AreEqual(result, transformator.Transform(result, externalImagesOnly: true));
            using (var document = new HtmlParser().ParseDocument(result))
            {
                var image = document.QuerySelector("body > .imagePlugin");
                Assert.IsNotNull(image);
                Assert.AreEqual(new Uri(SourcePageUrl, source).AbsoluteUri, image.GetAttribute("data-imageurl"));
                Assert.AreEqual("/details", image.GetAttribute("data-linkurl"));
                Assert.AreEqual("A & B", image.GetAttribute("data-alttext"));
                Assert.AreEqual("16", image.GetAttribute("data-width"));
                Assert.AreEqual("16", image.GetAttribute("data-height"));
                Assert.IsFalse(image.HasAttribute("data-widthpercentage"));
                foreach (var attribute in new[] { "data-siteid", "data-webid", "data-listid", "data-uniqueid" })
                {
                    Assert.IsFalse(image.HasAttribute(attribute));
                }
                Assert.AreEqual(0, document.QuerySelectorAll("img").Length);
                Assert.AreEqual("beforeafter", document.Body.TextContent);
            }

            page.Verify(p => p.GetInlineImage(It.IsAny<IPageText>(), It.IsAny<string>(), It.IsAny<PageImageOptions>()), Times.Never);
            page.Verify(p => p.NewWebPart(It.IsAny<IPageComponent>()), Times.Never);
            observer.Verify(log => log.Info(It.Is<LogEntry>(entry =>
                entry.Message.Contains("Keeping SharePoint system image") &&
                !entry.Message.Contains("?") && !entry.Message.Contains("#"))), Times.Once);
            observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Never);
            observer.Verify(log => log.Error(It.IsAny<LogEntry>()), Times.Never);
        }

        [DataTestMethod]
        [DataRow("", "16", "8")]
        [DataRow("height='4'", "8", "4")]
        public void Transform_SystemImagesCompleteMissingWidthFromResource(string attributes, string width, string height)
        {
            var transformator = new InlineImageTransformator(CreatePage().Object, SourcePageUrl,
                _ => throw new AssertFailedException("System images must not be transferred"), value => value,
                systemImageSizeLookup: _ => (16, 8));
            using (var document = new HtmlParser().ParseDocument(
                transformator.Transform($"<img src='/_layouts/images/icgen.gif' {attributes}>")))
            {
                var image = document.QuerySelector(".imagePlugin");
                Assert.AreEqual(width, image.GetAttribute("data-width"));
                Assert.AreEqual(height, image.GetAttribute("data-height"));
                Assert.IsFalse(image.HasAttribute("data-widthpercentage"));
            }
        }

        [TestMethod]
        public void Transform_SystemImageDimensionFailureRetainsHtmlAndWarns()
        {
            var observer = new Mock<ILogObserver>();
            var transformator = new InlineImageTransformator(CreatePage().Object, SourcePageUrl,
                _ => throw new AssertFailedException("System images must not be transferred"), value => value,
                new[] { observer.Object },
                systemImageSizeLookup: _ => throw new InvalidDataException("Invalid image header"));
            const string html = "<img src='/_layouts/images/icgen.gif'>";

            Assert.AreEqual(html, transformator.Transform(html));
            observer.Verify(log => log.Warning(It.Is<LogEntry>(entry => entry.Message.Contains("Invalid image header"))), Times.Once);
        }

        [DataTestMethod]
        [DataRow("/sites/source/SiteAssets/images/icgen.gif")]
        [DataRow("/sites/source/not_layouts/images/icgen.gif")]
        [DataRow("/_layouts/15/getpreview.ashx")]
        [DataRow("/_layouts/15/images-other/icgen.gif")]
        public void Transform_NonSystemImagesStillUseFileBackedConversion(string source)
        {
            var page = CreatePage();
            var result = CreateTransformator(page).Transform($"<img src='{source}'>");

            Assert.IsTrue(result.Contains("imagePlugin"));
            page.Verify(p => p.GetInlineImage(null, TargetImageUrl, It.IsAny<PageImageOptions>()), Times.Once);
        }

        [TestMethod]
        public void Transform_ExternalLayoutsImageStillUsesImporter()
        {
            const string source = "https://images.example/_layouts/images/icgen.gif";
            Uri imported = null;
            var page = CreatePage();
            var transformator = new InlineImageTransformator(page.Object, SourcePageUrl,
                _ => throw new AssertFailedException("Unexpected internal transfer"), value => value,
                importExternalImage: url => { imported = url; return TargetImageUrl; });

            var result = transformator.Transform($"<img src='{source}'>", externalImagesOnly: true);

            Assert.AreEqual(source, imported.AbsoluteUri);
            Assert.IsTrue(result.Contains("imagePlugin"));
            page.Verify(p => p.GetInlineImage(null, TargetImageUrl, It.IsAny<PageImageOptions>()), Times.Once);
        }

        [DataTestMethod]
        [DataRow("<p>before<img src='" + ImageUrl + "'>after</p>", "body > .imagePlugin")]
        [DataRow("<table><tbody><tr><td><p>before<img src='" + ImageUrl + "'>after</p></td></tr></tbody></table>", "td > .imagePlugin")]
        [DataRow("<ol><li><ul><li><p><strong><img src='" + ImageUrl + "'></strong></p></li></ul></li></ol>", "ol > li > ul > li > .imagePlugin")]
        public void Transform_ConvertsInPlaceAndPreservesContainer(string html, string selector)
        {
            var page = CreatePage();
            var converted = CreateTransformator(page).Transform(html);

            using (var document = new HtmlParser().ParseDocument(converted))
            {
                Assert.AreEqual(1, document.QuerySelectorAll(selector).Length);
                Assert.AreEqual(0, document.QuerySelectorAll("img").Length);
                Assert.AreEqual(TargetImageUrl, document.QuerySelector(".imagePlugin").GetAttribute("data-imageurl"));
                Assert.AreEqual(0, document.QuerySelectorAll("p .imagePlugin, strong .imagePlugin").Length);
            }
            page.Verify(p => p.GetInlineImage(null, TargetImageUrl, It.IsAny<PageImageOptions>()), Times.Once);
            page.Verify(p => p.NewWebPart(It.IsAny<IPageComponent>()), Times.Never);
        }

        [TestMethod]
        public void Transform_PreservesLinkedSiblingTextAndImageOrder()
        {
            var html = $"<p id='original'>before<a href='/sites/target/Pages/details.aspx'>left<strong><img src='{ImageUrl}'></strong>middle<img src='{ImageUrl}'>right</a>after</p>";
            using (var document = new HtmlParser().ParseDocument(CreateTransformator(CreatePage()).Transform(html)))
            {
                var images = document.QuerySelectorAll(".imagePlugin");
                Assert.AreEqual(2, images.Length);
                Assert.IsTrue(images.All(image => image.GetAttribute("data-linkurl") == "/sites/target/Pages/details.aspx"));
                Assert.AreEqual("beforeleftmiddlerightafter", document.Body.TextContent);
                CollectionAssert.AreEqual(new[] { "P", "DIV", "P", "DIV", "P" },
                    document.Body.Children.Select(element => element.TagName).ToArray());
                Assert.AreEqual("beforeleft", document.Body.Children[0].TextContent);
                Assert.AreEqual("middle", document.Body.Children[2].TextContent);
                Assert.AreEqual("rightafter", document.Body.Children[4].TextContent);
                Assert.AreEqual(1, document.QuerySelectorAll("#original").Length);
                Assert.IsTrue(document.QuerySelectorAll("a").All(anchor => anchor.GetAttribute("href") == "/sites/target/Pages/details.aspx"));
            }
        }

        [TestMethod]
        public void Transform_PreservesDimensionsAndEncodesAltTextAndLinks()
        {
            PageImageOptions options = null;
            var page = CreatePage();
            page.Setup(p => p.GetInlineImage(null, TargetImageUrl, It.IsAny<PageImageOptions>()))
                .Callback<IPageText, string, PageImageOptions>((_, __, value) => options = value)
                .Returns<IPageText, string, PageImageOptions>((_, url, value) => ImageSnippet(url, value));
            var html = $"<a href='https://example.org/?a=1&amp;b=&quot;two&quot;'><img src='{ImageUrl}' alt='A &quot;quote&quot; &amp; &lt;tag&gt;' width='100' height='50' style='width: 240px; height: 120px; float: right'></a>";
            using (var document = new HtmlParser().ParseDocument(CreateTransformator(page).Transform(html)))
            {
                Assert.AreEqual(240, options.Width);
                Assert.AreEqual(120, options.Height);
                Assert.AreEqual(PageImageAlignment.Right, options.Alignment);
                var image = document.QuerySelector(".imagePlugin");
                Assert.AreEqual("A \"quote\" & <tag>", image.GetAttribute("data-alttext"));
                Assert.AreEqual("https://example.org/?a=1&b=\"two\"", image.GetAttribute("data-linkurl"));
                Assert.AreEqual(1, document.Body.Children.Length);
            }
        }

        [TestMethod]
        public void Transform_NormalizesLinkAroundBlockContainerWithoutLosingText()
        {
            var html = $"<a href='/details'><div>before<span><img src='{ImageUrl}'></span>after</div></a>";
            using (var document = new HtmlParser().ParseDocument(CreateTransformator(CreatePage()).Transform(html)))
            {
                Assert.AreEqual(1, document.QuerySelectorAll("body > .imagePlugin").Length);
                Assert.AreEqual("/details", document.QuerySelector(".imagePlugin").GetAttribute("data-linkurl"));
                CollectionAssert.AreEqual(new[] { "before", "after" },
                    document.QuerySelectorAll("a[href='/details']").Select(anchor => anchor.TextContent).ToArray());
            }
        }

        [DataTestMethod]
        [DataRow("", null)]
        [DataRow("style='width:50%'", "50")]
        public void Transform_DoesNotStretchPixelImagesToSdkDefaultPercentage(string style, string expectedPercentage)
        {
            var page = CreatePage();
            page.Setup(p => p.GetInlineImage(null, TargetImageUrl, It.IsAny<PageImageOptions>()))
                .Returns<IPageText, string, PageImageOptions>((_, url, options) =>
                {
                    options.WidthPercentage ??= 100;
                    return ImageSnippet(url, options);
                });
            var result = CreateTransformator(page).Transform($"<img src='{ImageUrl}' width='160' height='80' {style}>");
            using (var document = new HtmlParser().ParseDocument(result))
            {
                var image = document.QuerySelector(".imagePlugin");
                Assert.AreEqual("160", image.GetAttribute("data-width"));
                Assert.AreEqual("80", image.GetAttribute("data-height"));
                Assert.AreEqual(expectedPercentage, image.GetAttribute("data-widthpercentage"));
            }
        }

        [DataTestMethod]
        [DataRow("", 320, 160)]
        [DataRow("width='160'", 160, 80)]
        [DataRow("height='80'", 160, 80)]
        public void Transform_CompletesDimensionsFromImageMetadata(string attributes, int width, int height)
        {
            var page = CreatePage();
            var transformator = new InlineImageTransformator(page.Object, SourcePageUrl, _ => TargetImageUrl, value => value,
                imageLookup: _ => new Dictionary<string, string> { ["ImageWidth"] = "320", ["ImageHeight"] = "160" });
            var result = transformator.Transform($"<img src='{ImageUrl}' {attributes}>");
            page.Verify(p => p.GetInlineImage(null, TargetImageUrl,
                It.Is<PageImageOptions>(options => options.Width == width && options.Height == height)), Times.Once);
            using (var document = new HtmlParser().ParseDocument(result))
            {
                Assert.IsFalse(document.QuerySelector(".imagePlugin").HasAttribute("data-widthpercentage"));
            }
        }

        [TestMethod]
        public void Transform_UnknownAspectRatioRetainsHeightOnlyImageAndWarns()
        {
            var page = CreatePage();
            var observer = new Mock<ILogObserver>();
            var transformator = new InlineImageTransformator(page.Object, SourcePageUrl, _ => TargetImageUrl, value => value,
                new[] { observer.Object }, _ => new Dictionary<string, string> { ["ImageWidth"] = "-1", ["ImageHeight"] = "-1" });
            var html = $"<img src='{ImageUrl}' height='80'>";
            Assert.AreEqual(html, transformator.Transform(html));
            observer.Verify(log => log.Warning(It.Is<LogEntry>(entry => entry.Message.Contains("specified height"))), Times.Once);
            page.Verify(p => p.GetInlineImage(It.IsAny<IPageText>(), It.IsAny<string>(), It.IsAny<PageImageOptions>()), Times.Never);
        }

        [TestMethod]
        public void Transform_RewritesImageSelfLinkToCopiedAsset()
        {
            var transformator = new InlineImageTransformator(CreatePage().Object, SourcePageUrl,
                _ => TargetImageUrl, value => value.Replace("/sites/source/", "/sites/target/"));
            var result = transformator.Transform($"<a href='/sites/target/SiteAssets/image.png'><img src='{ImageUrl}'></a>");
            using (var document = new HtmlParser().ParseDocument(result))
            {
                Assert.AreEqual(TargetImageUrl, document.QuerySelector(".imagePlugin").GetAttribute("data-linkurl"));
            }
        }

        [DataTestMethod]
        [DataRow("", false)]
        [DataRow("src=''", false)]
        [DataRow("src=' \t\r\n '", false)]
        [DataRow("src='&nbsp;'", false)]
        [DataRow("", true)]
        [DataRow("src=''", true)]
        [DataRow("src=' \t\r\n '", true)]
        public void Transform_ImageWithoutSourceIsRemovedAndWarns(string attributes, bool externalImagesOnly)
        {
            var page = CreatePage();
            var observer = new Mock<ILogObserver>();
            var transformator = new InlineImageTransformator(page.Object, SourcePageUrl,
                _ => throw new AssertFailedException("An image without a source must not be transferred"),
                _ => throw new AssertFailedException("An image without a source must not be rewritten"),
                new[] { observer.Object },
                imageLookup: _ => throw new AssertFailedException("An image without a source must not be looked up"),
                importExternalImage: _ => throw new AssertFailedException("An image without a source must not be imported"));
            var html = $"<p>before<a href=\"/details\">link<img {attributes} alt=\"missing\" style=\"width: 42px; height: 21px\"></a>after</p>";
            var result = transformator.Transform(html, externalImagesOnly);

            Assert.AreEqual("<p>before<a href=\"/details\">link</a>after</p>", result);
            Assert.AreEqual(result, transformator.Transform(result, externalImagesOnly: true));
            observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                entry.Message.Contains("Removed") && entry.Message.Contains("missing, empty or whitespace-only src") &&
                entry.Message.Contains("source HTML"))), Times.Once);
            observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Once);
            page.Verify(p => p.GetInlineImage(It.IsAny<IPageText>(), It.IsAny<string>(), It.IsAny<PageImageOptions>()), Times.Never);
        }

        [DataTestMethod]
        [DataRow("data:image/png;base64,AA==")]
        [DataRow("ftp://images.example/photo.png")]
        [DataRow("https://user:password@images.example/photo.png")]
        [DataRow("http://[")]
        public void Transform_UnsupportedNonblankSourceRetainsOriginalAndWarns(string source)
        {
            var page = CreatePage();
            var observer = new Mock<ILogObserver>();
            var html = $"<p><img src='{source}' alt='keep'></p>";

            Assert.AreEqual(html, CreateTransformator(page, observer.Object).Transform(html));

            observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                entry.Message.Contains("Retaining the original image HTML"))), Times.Once);
            page.Verify(p => p.GetInlineImage(It.IsAny<IPageText>(), It.IsAny<string>(), It.IsAny<PageImageOptions>()), Times.Never);
        }

        [TestMethod]
        public void Transform_ImportsExternalImageAndUpdatesItsSelfLink()
        {
            const string external = "https://images.example/photo.png?version=1&signature=test";
            Uri imported = null;
            var page = CreatePage();
            var transformator = new InlineImageTransformator(page.Object, SourcePageUrl,
                _ => throw new AssertFailedException("Unexpected internal transfer"), value => value,
                importExternalImage: url => { imported = url; return TargetImageUrl; });
            var html = "<p><a href='https://images.example/photo.png?version=1&amp;signature=test'>" +
                "<img src='https://images.example/photo.png?version=1&amp;signature=test' alt='Imported' width='160' height='80'></a></p>";
            using (var document = new HtmlParser().ParseDocument(transformator.Transform(html, externalImagesOnly: true)))
            {
                Assert.AreEqual(external, imported.AbsoluteUri);
                var image = document.QuerySelector("body > .imagePlugin");
                Assert.IsNotNull(image);
                Assert.AreEqual(TargetImageUrl, image.GetAttribute("data-imageurl"));
                Assert.AreEqual(TargetImageUrl, image.GetAttribute("data-linkurl"));
                Assert.AreEqual("Imported", image.GetAttribute("data-alttext"));
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Transform_FailedExternalImportRetainsNativeImageWithoutFakeFileMetadata(bool invalidResponse)
        {
            const string external = "https://images.example/photo.png?signature=secret-token&version=2";
            var page = CreatePage();
            var observer = new Mock<ILogObserver>();
            var transformator = new InlineImageTransformator(page.Object, SourcePageUrl, _ => "", value => value,
                new[] { observer.Object }, importExternalImage: _ =>
                {
                    if (invalidResponse) throw new InvalidDataException(external);
                    throw new IOException(external);
                });
            var html = "<table><tr><td><p><a href='/details'>Before <img src='https://images.example/photo.png?signature=secret-token&amp;version=2' " +
                "alt='A &quot;quote&quot; &amp; &lt;tag&gt;' width='160' height='80'> After</a></p></td></tr></table>";
            using (var document = new HtmlParser().ParseDocument(transformator.Transform(html, externalImagesOnly: true)))
            {
                var image = document.QuerySelector("td > .imagePlugin");
                Assert.IsNotNull(image);
                Assert.AreEqual(external, image.GetAttribute("data-imageurl"));
                Assert.AreEqual("/details", image.GetAttribute("data-linkurl"));
                Assert.AreEqual("0", image.GetAttribute("data-uploading"));
                Assert.AreEqual("160", image.GetAttribute("data-width"));
                Assert.AreEqual("80", image.GetAttribute("data-height"));
                Assert.AreEqual("A \"quote\" & <tag>", image.GetAttribute("data-alttext"));
                Assert.IsFalse(image.HasAttribute("data-siteid"));
                Assert.IsFalse(image.HasAttribute("data-webid"));
                Assert.IsFalse(image.HasAttribute("data-listid"));
                Assert.IsFalse(image.HasAttribute("data-uniqueid"));
                Assert.IsFalse(image.HasAttribute("data-widthpercentage"));
                Assert.AreEqual(0, document.QuerySelectorAll("img").Length);
                Assert.AreEqual("Before  After", document.Body.TextContent);
            }
            page.Verify(p => p.GetInlineImage(It.IsAny<IPageText>(), It.IsAny<string>(), It.IsAny<PageImageOptions>()), Times.Never);
            observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                entry.Message.Contains("native external CK5 image") && !entry.Message.Contains("secret-token"))), Times.Once);
        }

        [TestMethod]
        public void Transform_StockImagesStayExternalWithoutImport()
        {
            const string source = "https://cdn.hubblecontent.osi.office.net/image.png";
            var transformator = new InlineImageTransformator(CreatePage().Object, SourcePageUrl, _ => "", value => value,
                importExternalImage: _ => throw new AssertFailedException("Stock images must not be ingested"));
            using (var document = new HtmlParser().ParseDocument(transformator.Transform($"<img src='{source}'>", externalImagesOnly: true)))
            {
                Assert.AreEqual(source, document.QuerySelector(".imagePlugin").GetAttribute("data-imageurl"));
            }
        }

        [TestMethod]
        public void Transform_CK4TargetFailsBeforeTransferringAssets()
        {
            var page = CreatePage();
            page.SetupGet(p => p.EditorType).Returns(EditorType.CK4);
            var observer = new Mock<ILogObserver>();
            var transformator = new InlineImageTransformator(page.Object, SourcePageUrl,
                _ => throw new AssertFailedException("No asset transfer should occur"), value => value, new[] { observer.Object });

            Assert.ThrowsException<NotSupportedException>(() => transformator.Transform($"<img src='{ImageUrl}'>"));
            observer.Verify(log => log.Error(It.Is<LogEntry>(entry => entry.Message.Contains("CK5"))), Times.Once);
        }

        [TestMethod]
        public void TextCleanup_PreservesImagesInAllTableSectionsAndHeaderRows()
        {
            var html = $"<table><thead><tr><th><img src='{ImageUrl}' alt='header1'></th></tr><tr><th><img src='{ImageUrl}' alt='header2'></th></tr></thead><tbody><tr><td><img src='{ImageUrl}' alt='body1'></td></tr></tbody><tbody><tr><td><img src='{ImageUrl}' alt='body2'></td></tr></tbody><tfoot><tr><td><img src='{ImageUrl}' alt='footer'></td></tr></tfoot></table>";
            var result = InlineImageTransformator.PreserveImages(html, text => new HtmlTransformator().Transform(text, false));
            using (var document = new HtmlParser().ParseDocument(CreateTransformator(CreatePage()).Transform(result)))
            {
                CollectionAssert.AreEqual(new[] { "header1", "header2", "body1", "body2", "footer" },
                    document.QuerySelectorAll("td .imagePlugin").Select(image => image.GetAttribute("data-alttext")).ToArray());
                Assert.AreEqual(5, document.QuerySelectorAll("tr").Length);
            }
        }

        [DataTestMethod]
        [DataRow(true, true)]
        [DataRow(false, false)]
        public void WikiSplitter_NeverSplitsOrExtractsImages(bool handleImagesAndVideos, bool addTableListImages)
        {
            using (var context = CreateContext())
            {
                var original = WikiText($"<p>before<img src='{ImageUrl}'>after</p><table><tr><td><img src='{ImageUrl}'></td></tr></table><ul><li><a href='/link'><img src='{ImageUrl}'></a></li></ul>");
                var result = new WikiHtmlTransformator(context, context, CreatePage().Object, new PageTransformationInformation(null))
                    .TransformPlusSplit(new List<WebPartEntity> { original }, handleImagesAndVideos, addTableListImages);

                Assert.AreEqual(1, result.Count);
                Assert.AreSame(original, result[0]);
                Assert.AreEqual(3, new HtmlParser().ParseDocument(result[0].Properties["Text"]).QuerySelectorAll("img").Length);
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void WikiSplitter_PreservesIframeBehaviorWithImages(bool splitVideo)
        {
            using (var context = CreateContext())
            {
                var result = new WikiHtmlTransformator(context, context, CreatePage().Object, new PageTransformationInformation(null))
                    .TransformPlusSplit(new List<WebPartEntity>
                    {
                        WikiText($"<p>before<img src='{ImageUrl}'></p><iframe src='https://video.example/embed'></iframe><p>after</p>")
                    }, splitVideo, true);

                Assert.AreEqual(1, result.Count(part => part.Type == WebParts.WikiVideo));
                Assert.AreEqual(0, result.Count(part => part.Type == WebParts.WikiImage));
                Assert.AreEqual(splitVideo ? 2 : 1, result.Count(part => part.Type == WebParts.WikiText));
                Assert.AreEqual(1, result.Where(part => part.Type == WebParts.WikiText)
                    .Sum(part => new HtmlParser().ParseDocument(part.Properties["Text"]).QuerySelectorAll("img").Length));
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void ContentMapping_ProducesOneTextControlWithNativeImages(bool contentEditor)
        {
            using (var context = CreateContext())
            {
                var page = CreatePage();
                var controls = ConfigureCanvas(page);
                var information = new PageTransformationInformation(null);
                var source = WikiText($"<p>before<img src='{ImageUrl}' alt='inline'>after</p>");
                if (contentEditor)
                {
                    source.Type = WebParts.ContentEditor;
                    source.Properties = new Dictionary<string, string>
                    {
                        ["Content"] = source.Properties["Text"], ["ContentLink"] = "", ["ChromeType"] = "2"
                    };
                }
                var parts = new WikiHtmlTransformator(context, context, page.Object, information)
                    .TransformPlusSplit(new List<WebPartEntity> { source }, true, true);
                new ContentTransformator(context, context, page.Object, LoadMapping(), information).Transform(parts);

                Assert.AreEqual(1, controls.Count);
                var text = controls.Single() as IPageText;
                Assert.IsNotNull(text);
                using (var document = new HtmlParser().ParseDocument(text.Text))
                {
                    Assert.AreEqual(1, document.QuerySelectorAll(".imagePlugin").Length);
                    Assert.AreEqual(0, document.QuerySelectorAll("img").Length);
                    Assert.AreEqual("beforeafter", document.Body.TextContent);
                    Assert.AreEqual(ImageUrl, document.QuerySelector(".imagePlugin").GetAttribute("data-imageurl"));
                }
                page.Verify(p => p.GetInlineImage(null, ImageUrl, It.IsAny<PageImageOptions>()), Times.Once);
            }
        }

        [TestMethod]
        public void ContentMapping_ScriptContentEditorDoesNotConvertOrTransferImages()
        {
            using (var context = CreateContext())
            {
                var page = CreatePage();
                var controls = ConfigureCanvas(page);
                var information = new PageTransformationInformation(null);
                var source = new WebPartEntity
                {
                    Type = WebParts.ContentEditor, Row = 1, Column = 1, Order = 1,
                    Properties = new Dictionary<string, string>
                    {
                        ["Content"] = $"<script>alert(1)</script><img src='{ImageUrl}'>",
                        ["ContentLink"] = "", ["ChromeType"] = "2"
                    }
                };
                var parts = new WikiHtmlTransformator(context, context, page.Object, information)
                    .TransformPlusSplit(new List<WebPartEntity> { source }, true, true);
                new ContentTransformator(context, context, page.Object, LoadMapping(), information).Transform(parts);

                Assert.AreEqual("Content editor contains script and could not be loaded.", ((IPageText)controls.Single()).Text);
                page.Verify(p => p.GetInlineImage(It.IsAny<IPageText>(), It.IsAny<string>(), It.IsAny<PageImageOptions>()), Times.Never);
            }
        }

        [TestMethod]
        public void ContentMapping_RemovesFourSourcelessImagesAndConvertsFourValidImages()
        {
            using (var context = CreateContext())
            {
                var page = CreatePage();
                var controls = ConfigureCanvas(page);
                var observer = new Mock<ILogObserver>();
                var information = new PageTransformationInformation(null);
                var html = $"<p>before<img src='{ImageUrl}' alt='one' width='160' height='80'><img></p>" +
                    $"<table><tr><td><img src=''><img src='{ImageUrl}' alt='two' width='160' height='80'></td></tr></table>" +
                    $"<ul><li><img src=' \t '><img src='{ImageUrl}' alt='three' width='160' height='80'></li></ul>" +
                    $"<p><img style='width:1072px'><img src='{ImageUrl}' alt='four' width='160' height='80'>after</p>";

                new ContentTransformator(context, context, page.Object, LoadMapping(), information,
                    new[] { observer.Object }).Transform(new List<WebPartEntity> { WikiText(html) });
                var transformator = new TestPageTransformator { sourceClientContext = context };
                transformator.RegisterObserver(observer.Object);
                transformator.SavePage(page.Object, "target.aspx", information, context,
                    _ => throw new AssertFailedException("No external import should occur"));

                Assert.AreEqual(1, controls.Count);
                using (var document = new HtmlParser().ParseDocument(((IPageText)controls.Single()).Text))
                {
                    Assert.AreEqual(0, document.QuerySelectorAll("img").Length);
                    CollectionAssert.AreEqual(new[] { "one", "two", "three", "four" },
                        document.QuerySelectorAll(".imagePlugin").Select(image => image.GetAttribute("data-alttext")).ToArray());
                    Assert.AreEqual(1, document.QuerySelectorAll("td > .imagePlugin").Length);
                    Assert.AreEqual(1, document.QuerySelectorAll("li > .imagePlugin").Length);
                    Assert.AreEqual("beforeafter", document.Body.TextContent);
                }
                page.Verify(p => p.GetInlineImage(null, ImageUrl, It.IsAny<PageImageOptions>()), Times.Exactly(4));
                observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                    entry.Message.Contains("missing, empty or whitespace-only src"))), Times.Exactly(4));
                observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Exactly(4));
            }
        }

        [TestMethod]
        public void ContentMapping_MissingImagePreservesHtmlAndContinues()
        {
            using (var context = CreateContext())
            {
                var page = CreatePage();
                var controls = ConfigureCanvas(page);
                var observer = new Mock<ILogObserver>();
                page.SetupSequence(p => p.GetInlineImage(null, ImageUrl, It.IsAny<PageImageOptions>()))
                    .Throws(new FileNotFoundException("Missing image"))
                    .Returns(ImageSnippet(ImageUrl, new PageImageOptions { Width = 160, Height = 80 }));
                var source = WikiText($"<p>before<img src='{ImageUrl}' alt='keep' style='height: 42px; width: 84px'>" +
                    $"<img src='{ImageUrl}' alt='present' width='160' height='80'>after</p>");
                var information = new PageTransformationInformation(null);
                new ContentTransformator(context, context, page.Object, LoadMapping(), information,
                    new[] { observer.Object }).Transform(new List<WebPartEntity> { source });
                var transformator = new TestPageTransformator { sourceClientContext = context };
                transformator.RegisterObserver(observer.Object);
                transformator.SavePage(page.Object, "target.aspx", information, context,
                    _ => throw new AssertFailedException("Internal images must not be retried as external images"));

                using (var document = new HtmlParser().ParseDocument(((IPageText)controls.Single()).Text))
                {
                    Assert.AreEqual(ImageUrl, document.QuerySelector("p > img").GetAttribute("src"));
                    Assert.AreEqual("height: 42px; width: 84px", document.QuerySelector("img").GetAttribute("style"));
                    Assert.AreEqual("beforeafter", document.Body.TextContent);
                    Assert.AreEqual(1, document.QuerySelectorAll(".imagePlugin").Length);
                    Assert.AreEqual("present", document.QuerySelector(".imagePlugin").GetAttribute("data-alttext"));
                }
                page.Verify(p => p.GetInlineImage(null, ImageUrl, It.IsAny<PageImageOptions>()), Times.Exactly(2));
                observer.Verify(log => log.Warning(It.Is<LogEntry>(entry => entry.Message.Contains("Missing image"))), Times.Once);
            }
        }

        [TestMethod]
        public void ContentMapping_SystemIconStaysInLabeledLinkAlongsideFileBackedImage()
        {
            using (var context = CreateContext())
            {
                var page = CreatePage();
                var controls = ConfigureCanvas(page);
                var observer = new Mock<ILogObserver>();
                var information = new PageTransformationInformation(null);
                var source = WikiText("<p><a href='/details'><img class='ms-asset-icon ms-rtePosition-4' " +
                    "src='/_layouts/images/icgen.gif' alt='' width='16' height='16' style='margin:5px;'>Download</a></p>" +
                    $"<p><img src='{ImageUrl}' width='160' height='80'></p>");
                new ContentTransformator(context, context, page.Object, LoadMapping(), information,
                    new[] { observer.Object }).Transform(new List<WebPartEntity> { source });
                var transformator = new TestPageTransformator { sourceClientContext = context };
                transformator.RegisterObserver(observer.Object);
                transformator.SavePage(page.Object, "target.aspx", information, context,
                    _ => throw new AssertFailedException("Internal images must not be imported"));

                using (var document = new HtmlParser().ParseDocument(((IPageText)controls.Single()).Text))
                {
                    var link = document.QuerySelector("p > a[href='/details']");
                    Assert.IsNotNull(link);
                    var icon = link.QuerySelector("img");
                    Assert.IsNotNull(icon);
                    Assert.AreEqual("https://contoso.sharepoint.com/_layouts/images/icgen.gif", icon.GetAttribute("src"));
                    Assert.AreEqual("", icon.GetAttribute("alt"));
                    Assert.AreEqual("16", icon.GetAttribute("width"));
                    Assert.AreEqual("16", icon.GetAttribute("height"));
                    Assert.AreEqual("margin:5px;", icon.GetAttribute("style"));
                    Assert.AreEqual("Download", link.TextContent);
                    Assert.AreEqual(1, document.QuerySelectorAll("a[href='/details']").Length);
                    Assert.AreEqual(1, document.QuerySelectorAll(".imagePlugin").Length);
                    Assert.AreEqual(1, document.QuerySelectorAll("img").Length);
                }
                page.Verify(p => p.GetInlineImage(null, "/_layouts/images/icgen.gif", It.IsAny<PageImageOptions>()), Times.Never);
                page.Verify(p => p.GetInlineImage(null, ImageUrl, It.IsAny<PageImageOptions>()), Times.Once);
                observer.Verify(log => log.Info(It.Is<LogEntry>(entry => entry.Message.Contains("Keeping SharePoint system icon"))), Times.Once);
                observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Never);
                observer.Verify(log => log.Error(It.IsAny<LogEntry>()), Times.Never);
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void BuiltInCleanup_PreservesImagesThroughTableAndUrlRewriting(bool skipUrlRewrite)
        {
            using (var source = CreateContext())
            using (var target = CreateContext("https://contoso.sharepoint.com/sites/target"))
            {
                var information = new PageTransformationInformation(null) { SkipUrlRewrite = skipUrlRewrite };
                var builtIn = new BuiltIn(information, target, source, CreatePage().Object);
                var html = $"<table><thead><tr><th><a href='/sites/source/Pages/details.aspx'><img src='{ImageUrl}' style='height: 42px; width: 84px' alt='A &amp; B'></a></th></tr></thead>" +
                    $"<tbody><tr><td><ul><li><img src='{ImageUrl}'></li></ul></td></tr></tbody></table>";
                var result = builtIn.TextCleanup(html, "true");
                using (var document = new HtmlParser().ParseDocument(result))
                {
                    Assert.AreEqual(2, document.QuerySelectorAll("img").Length);
                    Assert.IsTrue(document.QuerySelectorAll("img").All(image => image.GetAttribute("src") == ImageUrl));
                    Assert.AreEqual("height: 42px; width: 84px", document.QuerySelector("img").GetAttribute("style"));
                    Assert.AreEqual("A & B", document.QuerySelector("img").GetAttribute("alt"));
                    Assert.AreEqual(skipUrlRewrite ? "/sites/source/Pages/details.aspx" : "/sites/target/Pages/details.aspx",
                        document.QuerySelector("a").GetAttribute("href"));
                    Assert.IsFalse(result.Contains("placeholder"));
                    Assert.IsFalse(result.Contains("pnpimage"));
                }
                using (var document = new HtmlParser().ParseDocument(CreateTransformator(CreatePage()).Transform(result)))
                {
                    Assert.AreEqual(2, document.QuerySelectorAll("td .imagePlugin").Length);
                    Assert.AreEqual(1, document.QuerySelectorAll("li > .imagePlugin").Length);
                }
            }
        }

        [TestMethod]
        public void BuiltInConversion_ResolvesRelativeImagesAgainstSourcePageFileRef()
        {
            using (var context = CreateContext())
            {
                var sourcePage = CoreTestContext.Item(context);
                sourcePage[Constants.FileRefField] = "/sites/source/Pages/folder/page.aspx";
                var page = CreatePage();
                var builtIn = new BuiltIn(new PageTransformationInformation(sourcePage), context, context, page.Object);

                builtIn.TransformInlineImages("<img src='../../SiteAssets/image.png' width='160' height='80'>");

                page.Verify(p => p.GetInlineImage(null, ImageUrl, It.IsAny<PageImageOptions>()), Times.Once);
            }
        }

        [TestMethod]
        public void SavePage_ImportsExternalImagesBetweenInitialAndFinalSave()
        {
            using (var context = CreateContext())
            {
                var events = new List<string>();
                var page = CreatePage();
                var controls = ConfigureCanvas(page);
                var information = new PageTransformationInformation(null);
                new ContentTransformator(context, context, page.Object, LoadMapping(), information)
                    .Transform(new List<WebPartEntity>
                    {
                        WikiText("<p><img src='https://images.example/photo.png' width='160' height='80'></p>")
                    });
                page.Setup(value => value.Save("target.aspx")).Callback(() => events.Add("save")).Returns("target.aspx");
                var transformator = new TestPageTransformator { sourceClientContext = context };

                transformator.SavePage(page.Object, "target.aspx", information, context,
                    _ => { events.Add("import"); return TargetImageUrl; });

                CollectionAssert.AreEqual(new[] { "save", "import", "save" }, events);
                var text = (IPageText)controls.Single();
                Assert.IsTrue(text.Text.Contains("imagePlugin"));
                Assert.IsTrue(text.Text.Contains(TargetImageUrl));
            }
        }

        [TestMethod]
        public void BuiltInConversion_DoesNotCopyTargetHostImagesFromADifferentSourceHost()
        {
            using (var source = CreateContext("https://legacy.sharepoint.com/sites/source"))
            using (var target = CreateContext("https://contoso.sharepoint.com/sites/target"))
            {
                var page = CreatePage();
                var observer = new Mock<ILogObserver>();
                var builtIn = new BuiltIn(new PageTransformationInformation(null), target, source, page.Object, new[] { observer.Object });
                const string path = "/sites/target/SiteAssets/image.png";
                var result = builtIn.TransformInlineImages($"<img src='https://contoso.sharepoint.com{path}' width='160' height='80'>");

                page.Verify(value => value.GetInlineImage(null, path, It.IsAny<PageImageOptions>()), Times.Once);
                Assert.IsTrue(result.Contains("imagePlugin"));
                observer.Verify(value => value.Warning(It.IsAny<LogEntry>()), Times.Never);
                observer.Verify(value => value.Error(It.IsAny<LogEntry>()), Times.Never);
            }
        }

        [DataTestMethod]
        [DataRow("<img src='https://images.example/photo.png'>", 1)]
        [DataRow("<div class='imagePlugin' data-imageurl='/image.png'></div>", 1)]
        [DataRow("<p><br></p>", 0)]
        public void EmptyTextCleanup_RemovesOnlyEmptyControls(string html, int expectedCount)
        {
            var page = CreatePage();
            var text = new Mock<IPageText>();
            text.SetupGet(value => value.Text).Returns(html);
            var controls = new List<ICanvasControl> { text.Object };
            page.SetupGet(value => value.Controls).Returns(controls);

            new TestPageTransformator().RemoveEmptyTextParts(page.Object);

            Assert.AreEqual(expectedCount, controls.Count);
        }

        private sealed class TestPageTransformator : BasePageTransformator
        {
        }

        private static InlineImageTransformator CreateTransformator(Mock<IPage> page, ILogObserver observer = null)
        {
            return new InlineImageTransformator(page.Object, SourcePageUrl, _ => TargetImageUrl, value => value,
                observer == null ? null : new[] { observer });
        }

        private static Mock<IPage> CreatePage()
        {
            var page = new Mock<IPage>();
            page.SetupGet(p => p.EditorType).Returns(EditorType.CK5);
            page.SetupGet(p => p.PageTitle).Returns("page");
            page.Setup(p => p.GetInlineImage(null, It.IsAny<string>(), It.IsAny<PageImageOptions>()))
                .Returns<IPageText, string, PageImageOptions>((_, url, options) => ImageSnippet(url, options));
            return page;
        }

        private static string ImageSnippet(string url, PageImageOptions options)
        {
            return $"<div class=\"imagePlugin\" style=\"background-color:transparent;position:relative;\" data-alignment=\"{options.Alignment}\" data-imageurl=\"{url}\" data-uploading=\"0\" data-height=\"{options.Height.GetValueOrDefault()}\" data-width=\"{options.Width.GetValueOrDefault()}\" data-widthpercentage=\"{options.WidthPercentage ?? 100}\" data-alttext=\"\" data-linkurl=\"\"></div>";
        }

        private List<ICanvasControl> ConfigureCanvas(Mock<IPage> page)
        {
            if (coreContext == null)
            {
                coreContext = CoreTestContext.Create(SiteUrl);
            }
            page.SetupGet(p => p.PnPContext).Returns(coreContext);
            var controls = new List<ICanvasControl>();
            page.SetupGet(p => p.Controls).Returns(controls);
            var column = new Mock<ICanvasColumn>();
            column.SetupGet(c => c.Controls).Returns(controls);
            var section = new Mock<ICanvasSection>();
            section.SetupGet(s => s.Columns).Returns(new List<ICanvasColumn> { column.Object });
            page.SetupGet(p => p.Sections).Returns(new List<ICanvasSection> { section.Object });
            page.Setup(p => p.AvailablePageComponentsAsync(null)).ReturnsAsync(new List<IPageComponent>());
            page.Setup(p => p.NewTextPart(It.IsAny<string>())).Returns(() =>
            {
                var text = new Mock<IPageText>();
                text.SetupAllProperties();
                return text.Object;
            });
            page.Setup(p => p.AddControl(It.IsAny<ICanvasControl>(), column.Object, It.IsAny<int>()))
                .Callback<ICanvasControl, ICanvasColumn, int, ControlFlexLayoutPosition>((control, _, order, position) => controls.Add(control));
            return controls;
        }

        private static WebPartEntity WikiText(string html)
        {
            return new WebPartEntity
            {
                Type = WebParts.WikiText, Row = 1, Column = 1, Order = 1,
                Properties = new Dictionary<string, string> { ["Text"] = html }
            };
        }

        private static PageTransformation LoadMapping()
        {
            using (var reader = new StringReader(BasePageTransformator.LoadDefaultWebPartMappingFile()))
            {
                return (PageTransformation)new XmlSerializer(typeof(PageTransformation)).Deserialize(reader);
            }
        }

        private static PnPContext CreateContext(string siteUrl = SiteUrl)
        {
            CacheManager.Instance.SetSharePointVersion(new Uri(siteUrl), SPVersion.SPO);
            var web = new Dictionary<string, object>
            {
                ["Url"] = siteUrl, ["ServerRelativeUrl"] = new Uri(siteUrl).AbsolutePath,
                ["Id"] = "413af984-d496-45ef-82d6-a2835dca7d49"
            };
            return CoreTestContext.WithFile(siteUrl, new
            {
                ListId = "80e43d5d-5fd0-48f3-86ea-db933db5e21b",
                UniqueId = "296c36c4-8b73-4fc7-9800-64855534d108",
                Properties = new Dictionary<string, object>
                {
                    ["vti_lastwidth"] = "320", ["vti_lastheight"] = "160"
                }
            }, web, new Dictionary<string, object>
            {
                ["Id"] = "e7f8587b-0764-4b08-a5dd-bd33683daa4e", ["RootWeb"] = web
            });
        }
    }
}
