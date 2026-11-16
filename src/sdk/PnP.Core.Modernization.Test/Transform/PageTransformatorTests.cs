using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;

namespace PnP.Core.Modernization.Tests.Transform
{
    [TestClass]
    [TestCategory("Offline")]
    public class PageTransformatorTests
    {
        [DataTestMethod]
        [DataRow("https://contoso.sharepoint.com/sites/source", "https://contoso.sharepoint.com/sites/source", false)]
        [DataRow("https://contoso.sharepoint.com/sites/source", "https://contoso.sharepoint.com/sites/source/", true)]
        [DataRow("https://contoso.sharepoint.com/sites/source/subweb", "https://contoso.sharepoint.com/sites/source/subweb/", true)]
        public void NormalizeSameSiteTargetContext_UsesSourceOnlyModeAndPreservesTargetFolder(string sourceUrl, string targetUrl, bool initiallyCrossSite)
        {
            using (var sourceContext = new ClientContext(sourceUrl))
            using (var targetContext = new ClientContext(targetUrl))
            {
                CacheManager.Instance.SetSharePointVersion(new Uri(sourceContext.Url), SPVersion.SPO);
                CacheManager.Instance.SetSharePointVersion(new Uri(targetContext.Url), SPVersion.SPO);
                var transformationInformation = new PageTransformationInformation(null)
                {
                    TargetPageFolder = "Modernized",
                    TargetPageFolderOverridesDefaultFolder = true,
                    TargetPageName = "Converted.aspx"
                };
                var transformator = new PageTransformator(sourceContext, targetContext, new PageTransformation());
                transformator.CrossFarmTransformationValidation(transformationInformation);
                Assert.AreEqual(initiallyCrossSite, transformationInformation.IsCrossSiteTransformation);

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
            using (var sourceContext = new ClientContext(sourceUrl))
            using (var targetContext = new ClientContext(targetUrl))
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
            using (var sourceContext = new ClientContext("https://contoso.sharepoint.com/sites/source"))
            using (var targetContext = new ClientContext("https://fabrikam.sharepoint.com/sites/target"))
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
