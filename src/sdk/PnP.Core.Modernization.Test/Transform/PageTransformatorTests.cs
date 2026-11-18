using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;
using System.Linq;
using System.Net.Http;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Tests.Transform
{
    [TestClass]
    [TestCategory("Offline")]
    public class PageTransformatorTests
    {
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void SummaryLinksUsesCoreProtocolOnlyWhenLosslessFieldDataIsNeeded(bool populated)
        {
            using var context = CoreTestContext.Create(respond: (_, _) =>
                CoreTestContext.Json(new { SummaryLinks = populated ? "<div>Link label</div>" : "" }));
            var rawItem = CoreTestContext.Item(context);
            var field = new Mock<IField>();
            field.SetupGet(value => value.InternalName).Returns("SummaryLinks");
            field.SetupGet(value => value.EntityPropertyName).Returns("SummaryLinks");
            field.SetupGet(value => value.TypeAsString).Returns("SummaryLinks");
            var fields = new Mock<IFieldCollection>();
            fields.SetupGet(value => value.RequestedItems).Returns(new[] { field.Object });
            fields.Setup(value => value.LoadAsync(It.IsAny<Expression<Func<IField, object>>[]>()))
                .Returns(Task.CompletedTask);
            var result = new Mock<ICamlQueryCsomResult>();
            result.SetupGet(value => value.Items).Returns(new[] { rawItem });
            var list = new Mock<IList>();
            list.SetupGet(value => value.Id).Returns(Guid.NewGuid());
            list.SetupGet(value => value.Fields).Returns(fields.Object);
            list.Setup(value => value.LoadItemsByCamlQueryViaCsom(It.IsAny<CamlQueryOptions>())).Returns(result.Object);
            var page = new Mock<IListItem>();
            page.SetupGet(value => value.ParentList).Returns(list.Object);
            page.SetupGet(value => value.PnPContext).Returns(context);
            page.SetupGet(value => value.Id).Returns(1);
            page.SetupGet(value => value.Values).Returns(rawItem.Values);

            var loaded = BasePageTransformator.LoadSourcePageFields(page.Object);

            Assert.AreSame(populated ? rawItem : page.Object, loaded);
            list.Verify(value => value.LoadItemsByCamlQueryViaCsom(It.Is<CamlQueryOptions>(
                query => query.DatesInUtc == true && query.ViewXml.Contains("<RowLimit>1</RowLimit>"))),
                populated ? Times.Once() : Times.Never());
        }

        [TestMethod]
        public void PublishingLayoutIdentifiesPublishingPagesWhenRestOmitsAudience()
        {
            using var context = CoreTestContext.Create();
            var item = CoreTestContext.Item(context);
            item[Constants.PublishingPageLayoutField] = Mock.Of<IFieldUrlValue>(
                value => value.Url == "https://contoso.sharepoint.com/_catalogs/masterpage/ArticleLeft.aspx");

            Assert.AreEqual("PublishingPage", item.PageType());
            item[Constants.ClientSideApplicationIdField] = Constants.FeatureId_Web_ModernPage.ToString();
            Assert.AreEqual("ClientSidePage", item.PageType());
        }

        [TestMethod]
        public void SourceFieldsLoadThroughCoreWithoutBecomingPendingMetadataWrites()
        {
            using var context = CoreTestContext.Create();
            var item = CoreTestContext.Item(context);
            item["Title"] = "Source title";
            var names = new[] { "Title", "FileRef", "FileLeafRef", "FileDirRef", "HTML_x0020_File_x0020_Type", "PublishingPageImage", "PublishingImageCaption" };
            var fields = names.Select(name => new
            {
                Id = Guid.NewGuid(), InternalName = name, EntityPropertyName = name, Title = name,
                TypeAsString = name == "PublishingPageImage" ? "Image" : "Text", FieldTypeKind = 2
            }).ToArray();
            var requests = 0;
            CoreTestContext.SetResponse(context, (request, body) =>
            {
                if (request.Method == HttpMethod.Get)
                {
                    if (request.RequestUri.AbsolutePath.EndsWith("/items", StringComparison.OrdinalIgnoreCase))
                        return CoreTestContext.Json(new { value = new[] { new { Id = 1, Title = "Source title" } } });
                    if (request.RequestUri.AbsolutePath.Contains("/items(", StringComparison.OrdinalIgnoreCase))
                        return CoreTestContext.Json(new
                        {
                            Id = 1, Title = "Source title", PublishingImageCaption = "<p>Raw caption</p>",
                            ParentList = new { Id = item.ParentList.Id, Fields = fields }
                        });
                    return request.RequestUri.AbsolutePath.EndsWith("/fields", StringComparison.OrdinalIgnoreCase)
                        ? CoreTestContext.Json(new { value = fields })
                        : CoreTestContext.Json(new { Id = item.ParentList.Id, Fields = fields });
                }
                requests++;
                Assert.AreEqual(HttpMethod.Post, request.Method);
                StringAssert.Contains(body, "FileRef");
                StringAssert.Contains(body, "FileLeafRef");
                StringAssert.Contains(body, "FileDirRef");
                StringAssert.Contains(body, "HTML_x0020_File_x0020_Type");
                StringAssert.Contains(body, "PublishingPageImage");
                return CoreTestContext.Json(new
                {
                    Row = new[]
                    {
                        new
                        {
                            ID = "1", Title = "Source title", FileRef = "/sites/test/SitePages/source.aspx",
                            FileLeafRef = "source.aspx", FileDirRef = "/sites/test/SitePages",
                            HTML_x0020_File_x0020_Type = "SharePoint.WebPartPage.Document",
                            PublishingPageImage = "<img alt='Original image' src='/image.png'>",
                            PublishingImageCaption = "<div class=\"ms-rtestate-field\"><p>Raw caption</p></div>"
                        }
                    }
                });
            });

            var loaded = BasePageTransformator.LoadSourcePageFields(item);
            BasePageTransformator.LoadSourcePageFields(loaded);

            Assert.AreEqual("source.aspx", loaded["FileLeafRef"]);
            Assert.AreEqual("Source title", loaded["Title"]);
            Assert.AreEqual("WebPartPage", loaded.PageType());
            Assert.AreEqual("<p>Raw caption</p>", loaded["PublishingImageCaption"]);
            Assert.IsFalse(loaded.Values.HasChanges);
            Assert.AreEqual(1, requests);
        }

        [DataTestMethod]
        [DataRow("https://contoso.sharepoint.com/sites/source", "https://contoso.sharepoint.com/sites/source")]
        [DataRow("https://contoso.sharepoint.com/sites/source", "https://contoso.sharepoint.com/sites/source/")]
        [DataRow("https://contoso.sharepoint.com/sites/source/subweb", "https://contoso.sharepoint.com/sites/source/subweb/")]
        public void NormalizeSameSiteTargetContext_UsesSourceOnlyModeAndPreservesTargetFolder(string sourceUrl, string targetUrl)
        {
            using (var sourceContext = CoreTestContext.Create(sourceUrl))
            using (var targetContext = CoreTestContext.Create(targetUrl))
            {
                CacheManager.Instance.SetSharePointVersion(new Uri(sourceContext.Uri.AbsoluteUri), SPVersion.SPO);
                CacheManager.Instance.SetSharePointVersion(new Uri(targetContext.Uri.AbsoluteUri), SPVersion.SPO);
                var transformationInformation = new PageTransformationInformation(null)
                {
                    TargetPageFolder = "Modernized",
                    TargetPageFolderOverridesDefaultFolder = true,
                    TargetPageName = "Converted.aspx"
                };
                var transformator = new PageTransformator(sourceContext, targetContext, new PageTransformation());
                transformator.CrossFarmTransformationValidation(transformationInformation);
                Assert.IsFalse(transformationInformation.IsCrossSiteTransformation);

                var siteId = Guid.NewGuid();
                var webId = Guid.NewGuid();
                var normalized = transformator.NormalizeSameSiteTargetContext(siteId, siteId, webId, webId, transformationInformation);

                Assert.IsTrue(normalized);
                Assert.IsNull(transformator.targetClientContext);
                Assert.AreSame(sourceContext, transformator.sourceClientContext);
                Assert.AreEqual("Modernized", transformationInformation.TargetPageFolder);
                Assert.IsTrue(transformationInformation.TargetPageFolderOverridesDefaultFolder);
                Assert.AreEqual("Converted.aspx", transformationInformation.TargetPageName);
                Assert.IsFalse(transformationInformation.TargetPageTakesSourcePageName);
                Assert.IsFalse(transformationInformation.IsCrossSiteTransformation);
                Assert.IsFalse(transformationInformation.IsCrossFarmTransformation);
                Assert.AreEqual(SPVersion.SPO, transformationInformation.TargetVersion);
                Assert.AreEqual(transformationInformation.SourceVersionNumber, transformationInformation.TargetVersionNumber);
            }
        }

        [DataTestMethod]
        [DataRow("https://contoso.sharepoint.com/sites/source", "https://contoso.sharepoint.com/sites/source/subweb", false)]
        [DataRow("https://contoso.sharepoint.com/sites/source/subweb", "https://contoso.sharepoint.com/sites/source", true)]
        [DataRow("https://contoso.sharepoint.com/sites/source/first", "https://contoso.sharepoint.com/sites/source/second", false)]
        public void NormalizeSameSiteTargetContext_DifferentWebInSameSiteRejectsWithoutChangingState(
            string sourceUrl, string targetUrl, bool overwrite)
        {
            using (var sourceContext = CoreTestContext.Create(sourceUrl))
            using (var targetContext = CoreTestContext.Create(targetUrl))
            {
                var information = new PageTransformationInformation(null)
                {
                    IsCrossSiteTransformation = true,
                    SourceVersion = SPVersion.SPO,
                    SourceVersionNumber = "16.0.0.0",
                    TargetVersion = SPVersion.SPO,
                    TargetVersionNumber = "16.0.0.0",
                    TargetPageName = "Converted.aspx",
                    TargetPageFolder = "Modernized",
                    TargetPageFolderOverridesDefaultFolder = true,
                    Overwrite = overwrite
                };
                var observer = new Mock<ILogObserver>();
                var transformator = new PageTransformator(sourceContext, targetContext, new PageTransformation());
                transformator.RegisterObserver(observer.Object);
                var siteId = Guid.NewGuid();

                var exception = Assert.ThrowsException<ArgumentException>(() =>
                    transformator.NormalizeSameSiteTargetContext(siteId, siteId, Guid.NewGuid(), Guid.NewGuid(), information));

                Assert.AreEqual(LogStrings.Error_SameSiteTransferDifferentWeb, exception.Message);
                observer.Verify(log => log.Error(It.Is<LogEntry>(entry =>
                    entry.Message == LogStrings.Error_SameSiteTransferDifferentWeb &&
                    entry.Heading == LogStrings.Heading_SharePointConnection)), Times.Once);
                Assert.AreSame(sourceContext, transformator.sourceClientContext);
                Assert.AreSame(targetContext, transformator.targetClientContext);
                Assert.IsTrue(information.IsCrossSiteTransformation);
                Assert.IsFalse(information.IsCrossFarmTransformation);
                Assert.AreEqual(SPVersion.SPO, information.TargetVersion);
                Assert.AreEqual("16.0.0.0", information.TargetVersionNumber);
                Assert.AreEqual("Converted.aspx", information.TargetPageName);
                Assert.AreEqual("Modernized", information.TargetPageFolder);
                Assert.IsTrue(information.TargetPageFolderOverridesDefaultFolder);
                Assert.IsFalse(information.TargetPageTakesSourcePageName);
                Assert.AreEqual(overwrite, information.Overwrite);
            }
        }

        [TestMethod]
        public void NormalizeSameSiteTargetContext_DifferentSitePreservesTargetSettings()
        {
            using (var sourceContext = CoreTestContext.Create("https://contoso.sharepoint.com/sites/source"))
            using (var targetContext = CoreTestContext.Create("https://fabrikam.sharepoint.com/sites/target"))
            {
                var information = new PageTransformationInformation(null)
                {
                    IsCrossSiteTransformation = true,
                    IsCrossFarmTransformation = true,
                    SourceVersion = SPVersion.SP2019,
                    TargetVersion = SPVersion.SPO,
                    TargetVersionNumber = "16.0.0.0",
                    TargetPageFolder = "Modernized"
                };
                var transformator = new PageTransformator(sourceContext, targetContext, new PageTransformation());

                Assert.IsFalse(transformator.NormalizeSameSiteTargetContext(
                    Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), information));
                Assert.AreSame(targetContext, transformator.targetClientContext);
                Assert.IsTrue(information.IsCrossSiteTransformation);
                Assert.IsTrue(information.IsCrossFarmTransformation);
                Assert.AreEqual(SPVersion.SPO, information.TargetVersion);
                Assert.AreEqual("16.0.0.0", information.TargetVersionNumber);
                Assert.AreEqual("Modernized", information.TargetPageFolder);
            }
        }
    }
}
