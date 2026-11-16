using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using PnP.Framework.Utilities.UnitTests.Model;
using PnP.Framework.Utilities.UnitTests.Web;

namespace PnP.Core.Modernization.Tests.Transform.Publishing
{
    [TestClass]
    [TestCategory("Offline")]
    public class PublishingPageInPlaceTransformationTests
    {
        private static readonly Guid SourceSiteId = Guid.Parse("c33f44c0-e1ce-4d87-a961-98669e76bca6");
        private static readonly Guid SourceWebId = Guid.Parse("a467c0f4-dcf4-49f3-8a08-e33de32d3202");

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ModelCreateInPlaceUsesSourceContextAsTarget()
        {
            using (var sourceContext = CreateContext("https://contoso.sharepoint.com/sites/enterprise-wiki"))
            {
                var transformator = PublishingPageTransformator.CreateInPlace(sourceContext, new PageTransformation(), null);

                Assert.AreSame(sourceContext, transformator.sourceClientContext);
                Assert.AreSame(sourceContext, transformator.targetClientContext);
            }
        }

        [TestMethod]
        public void FileMappingCreateInPlaceUsesSourceContextAsTarget()
        {
            Directory.CreateDirectory(TestContext.TestResultsDirectory);
            var mappingPath = Path.Combine(TestContext.TestResultsDirectory, $"webpartmapping-{Guid.NewGuid():N}.xml");
            System.IO.File.WriteAllText(mappingPath, BasePageTransformator.LoadDefaultWebPartMappingFile());

            try
            {
                using (var sourceContext = CreateContext("https://contoso.sharepoint.com/sites/enterprise-wiki"))
                {
                    var transformator = PublishingPageTransformator.CreateInPlace(sourceContext, mappingPath, null);

                    Assert.AreSame(sourceContext, transformator.sourceClientContext);
                    Assert.AreSame(sourceContext, transformator.targetClientContext);
                }
            }
            finally
            {
                System.IO.File.Delete(mappingPath);
            }
        }

        [TestMethod]
        public void ModelConstructorStillRejectsANullTargetContext()
        {
            using (var sourceContext = CreateContext("https://contoso.sharepoint.com/sites/enterprise-wiki"))
            {
                Assert.ThrowsException<ArgumentException>(() => CreateTransformator(sourceContext, null));
            }
        }

        [TestMethod]
        public void FileMappingConstructorStillRejectsANullTargetContext()
        {
            Directory.CreateDirectory(TestContext.TestResultsDirectory);
            var mappingPath = Path.Combine(TestContext.TestResultsDirectory, $"webpartmapping-{Guid.NewGuid():N}.xml");
            System.IO.File.WriteAllText(mappingPath, BasePageTransformator.LoadDefaultWebPartMappingFile());

            try
            {
                using (var sourceContext = CreateContext("https://contoso.sharepoint.com/sites/enterprise-wiki"))
                {
                    Assert.ThrowsException<ArgumentException>(
                        () => new PublishingPageTransformator(sourceContext, null, mappingPath, null));
                }
            }
            finally
            {
                System.IO.File.Delete(mappingPath);
            }
        }

        [TestMethod]
        public void SameWebTargetIsAcceptedWithoutAdditionalOptions()
        {
            var result = ValidateSameWeb(hasWritableSitePages: true);

            Assert.AreEqual(PublishingPageTransformationTarget.SameWeb, result);
        }

        [TestMethod]
        public void SameSiteCollectionDifferentWebFailsClosed()
        {
            var result = PublishingPageTransformationValidator.ValidateTarget(
                SourceSiteId,
                SourceSiteId,
                SourceWebId,
                Guid.Parse("7896708f-f5ea-4f22-aeb3-6223bf8897b0"),
                true);

            Assert.AreEqual(PublishingPageTransformationTarget.SameSiteCollectionDifferentWeb, result);
        }

        [TestMethod]
        public void ConstructorRetainsAnExplicitTargetContext()
        {
            using (var sourceContext = CreateContext("https://contoso.sharepoint.com/sites/enterprise-wiki"))
            using (var targetContext = CreateContext("https://contoso.sharepoint.com/sites/modern-target"))
            {
                var transformator = CreateTransformator(sourceContext, targetContext);

                Assert.AreSame(sourceContext, transformator.sourceClientContext);
                Assert.AreSame(targetContext, transformator.targetClientContext);
            }
        }

        [TestMethod]
        public void ConstructorStillRequiresSourceContext()
        {
            Assert.ThrowsException<ArgumentException>(() => CreateTransformator(null, null));
        }

        [TestMethod]
        public void SameWebSupportsGeneralPublishingPagesAndRequiresWritableSitePages()
        {
            Assert.AreEqual(
                PublishingPageTransformationTarget.SameWebRequiresWritableSitePages,
                ValidateSameWeb(false));
            Assert.AreEqual(
                PublishingPageTransformationTarget.SameWeb,
                ValidateSameWeb(true));
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void SitePagesFeaturePreflightRejectsMissingFeatureWithActionableError(bool hasUnrelatedFeature)
        {
            var fixture = new SitePagesFeatureFixture();
            if (hasUnrelatedFeature)
            {
                fixture.AddFeature(Guid.Parse("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb"));
            }

            using (var context = fixture.CreateContext())
            {
                var transformator = CreateTransformator(context, context);
                var exception = Assert.ThrowsException<InvalidOperationException>(() =>
                    transformator.EnsureSitePagesFeatureEnabled(PublishingPageTransformationTarget.SameWeb));

                StringAssert.Contains(exception.Message, LogStrings.Error_InPlacePublishingPageSitePagesFeatureNotEnabled);
                StringAssert.Contains(exception.Message, context.Url);
                StringAssert.Contains(exception.Message, Modernization.Constants.FeatureId_Web_ModernPage.ToString());
                StringAssert.Contains(exception.Message, "Manage site features");
                Assert.AreEqual(1, fixture.RequestCount);
                Assert.IsFalse(context.HasPendingRequest);
            }
        }

        [TestMethod]
        public void SitePagesFeaturePreflightAcceptsEnabledFeatureWithoutWrites()
        {
            var fixture = new SitePagesFeatureFixture();
            fixture.AddFeature(Modernization.Constants.FeatureId_Web_ModernPage);

            using (var context = fixture.CreateContext())
            {
                CreateTransformator(context, context).EnsureSitePagesFeatureEnabled(PublishingPageTransformationTarget.SameWeb);

                Assert.AreEqual(1, fixture.RequestCount);
                Assert.IsFalse(context.HasPendingRequest);
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void SitePagesFeaturePreflightRefreshesAReusedContext(bool initiallyEnabled)
        {
            var fixture = new SitePagesFeatureFixture();
            if (initiallyEnabled)
            {
                fixture.AddFeature(Modernization.Constants.FeatureId_Web_ModernPage);
            }

            using (var context = fixture.CreateContext())
            {
                var transformator = CreateTransformator(context, context);
                Action preflight = () => transformator.EnsureSitePagesFeatureEnabled(PublishingPageTransformationTarget.SameWeb);
                if (initiallyEnabled)
                {
                    preflight();
                    fixture.Features.Clear();
                    Assert.ThrowsException<InvalidOperationException>(preflight);
                }
                else
                {
                    Assert.ThrowsException<InvalidOperationException>(preflight);
                    fixture.AddFeature(Modernization.Constants.FeatureId_Web_ModernPage);
                    preflight();
                }

                Assert.AreEqual(2, fixture.RequestCount);
            }
        }

        [TestMethod]
        public void SitePagesFeaturePreflightDoesNotQueryCrossSiteTargets()
        {
            var fixture = new SitePagesFeatureFixture();
            using (var context = fixture.CreateContext())
            {
                CreateTransformator(context, context).EnsureSitePagesFeatureEnabled(PublishingPageTransformationTarget.CrossSiteCollection);

                Assert.AreEqual(0, fixture.RequestCount);
                Assert.IsFalse(context.HasPendingRequest);
            }
        }

        [TestMethod]
        public void SitePagesFeaturePreflightPreservesQueryFailures()
        {
            var fixture = new SitePagesFeatureFixture { FailRequests = true };
            using (var context = fixture.CreateContext())
            {
                var exception = Assert.ThrowsException<ClientRequestException>(() =>
                    CreateTransformator(context, context).EnsureSitePagesFeatureEnabled(PublishingPageTransformationTarget.SameWeb));

                StringAssert.Contains(exception.Message, "InternalServerError");
                Assert.IsFalse(exception.ToString().Contains(LogStrings.Error_InPlacePublishingPageSitePagesFeatureNotEnabled));
                Assert.AreEqual(1, fixture.RequestCount);
            }
        }

        [TestMethod]
        public void CrossSiteCollectionKeepsExistingTargetPolicyAndPermissionSemantics()
        {
            var result = PublishingPageTransformationValidator.ValidateTarget(
                SourceSiteId,
                Guid.Parse("2e33ad34-f43a-4d75-af55-e0deba981252"),
                SourceWebId,
                Guid.Empty,
                false);

            Assert.AreEqual(PublishingPageTransformationTarget.CrossSiteCollection, result);
            Assert.IsTrue(PublishingPageTransformationValidator.CanOverwriteTarget(result, true));
            Assert.IsTrue(PublishingPageTransformationValidator.UsesCrossSitePermissionSemantics(result));
        }

        [TestMethod]
        public void SameWebNeverOverwritesAndUsesInPlacePermissionSemantics()
        {
            var result = ValidateSameWeb(true);

            Assert.IsFalse(PublishingPageTransformationValidator.CanOverwriteTarget(result, true));
            Assert.IsFalse(PublishingPageTransformationValidator.UsesCrossSitePermissionSemantics(result));
        }

        [TestMethod]
        public void SameWebUrlTransformationIsANoOp()
        {
            const string webUrl = "https://contoso.sharepoint.com/sites/enterprise-wiki";
            const string sourceUrl = "/sites/enterprise-wiki/Pages/source.aspx";

            using (var sourceContext = CreateContext(webUrl))
            using (var targetContext = CreateContext(webUrl))
            {
                var information = new PublishingPageTransformationInformation(null);
                var transformator = new UrlTransformator(information, sourceContext, targetContext);

                Assert.AreEqual(sourceUrl, transformator.Transform(sourceUrl));
            }
        }

        [TestMethod]
        public void SameWebAssetTransferIsANoOp()
        {
            const string webUrl = "https://contoso.sharepoint.com/sites/enterprise-wiki";
            const string sourceAssetUrl = "/sites/enterprise-wiki/PublishingImages/image.png";

            using (var sourceContext = CreateContext(webUrl))
            using (var targetContext = CreateContext(webUrl))
            {
                var transfer = new AssetTransfer(sourceContext, targetContext);

                Assert.AreEqual(sourceAssetUrl, transfer.TransferAsset(sourceAssetUrl, "target.aspx"));
            }
        }

        [TestMethod]
        public void TransformSeamKnownCollisionNeverInvokesAddAndKeepsSourceUnchanged()
        {
            using (var sourceContext = CreateContext("https://contoso.sharepoint.com/sites/enterprise-wiki"))
            using (var targetContext = CreateContext("https://contoso.sharepoint.com/sites/enterprise-wiki"))
            {
                var information = CreateInPlaceInformation(sourceContext, overwrite: true);
                var sourceBefore = SnapshotSource(information.SourcePage);
                var sourceHadPendingRequest = sourceContext.HasPendingRequest;
                var addCalls = 0;
                var transformator = CreateTransformator(sourceContext, targetContext);

                Assert.ThrowsException<ArgumentException>(() => transformator.CreateTargetPageAfterCollisionProbe(
                    PublishingPageTransformationTarget.SameWeb,
                    information,
                    () => null,
                    _ =>
                    {
                        addCalls++;
                        return null;
                    },
                    out _));

                Assert.AreEqual(0, addCalls, "AddClientSidePage seam must not run for a known in-place collision.");
                AssertSourceUnchanged(sourceBefore, information.SourcePage);
                Assert.AreEqual(sourceHadPendingRequest, sourceContext.HasPendingRequest, "The source context request state changed.");
            }
        }

        [TestMethod]
        public void TransformSeamRejectedInPlaceTargetsNeverProbeOrAddAndKeepSourceUnchanged()
        {
            using (var sourceContext = CreateContext("https://contoso.sharepoint.com/sites/enterprise-wiki"))
            using (var targetContext = CreateContext("https://contoso.sharepoint.com/sites/enterprise-wiki"))
            {
                var information = CreateInPlaceInformation(sourceContext, overwrite: true);
                var sourceBefore = SnapshotSource(information.SourcePage);
                var sourceHadPendingRequest = sourceContext.HasPendingRequest;
                var transformator = CreateTransformator(sourceContext, targetContext);

                foreach (var rejectedTarget in new[]
                {
                    PublishingPageTransformationTarget.SameSiteCollectionDifferentWeb,
                    PublishingPageTransformationTarget.SameWebRequiresWritableSitePages,
                })
                {
                    var probeCalls = 0;
                    var addCalls = 0;

                    var exception = Assert.ThrowsException<ArgumentException>(() => transformator.CreateTargetPageAfterCollisionProbe(
                        rejectedTarget,
                        information,
                        () =>
                        {
                            probeCalls++;
                            return null;
                        },
                        _ =>
                        {
                            addCalls++;
                            return null;
                        },
                        out _));

                    Assert.AreEqual(
                        rejectedTarget == PublishingPageTransformationTarget.SameSiteCollectionDifferentWeb
                            ? LogStrings.Error_SameSiteTransferDifferentWeb
                            : LogStrings.Error_InPlacePublishingPageRequiresWritableSitePages,
                        exception.Message);
                    Assert.AreEqual(0, probeCalls, $"Target probe ran for rejected target '{rejectedTarget}'.");
                    Assert.AreEqual(0, addCalls, $"AddClientSidePage seam ran for rejected target '{rejectedTarget}'.");
                    AssertSourceUnchanged(sourceBefore, information.SourcePage);
                    Assert.AreEqual(sourceHadPendingRequest, sourceContext.HasPendingRequest, "The source context request state changed.");
                }
            }
        }

        [TestMethod]
        public void TransformSeamInconclusiveProbeNeverInvokesAddAndKeepsSourceUnchanged()
        {
            using (var sourceContext = CreateContext("https://contoso.sharepoint.com/sites/enterprise-wiki"))
            using (var targetContext = CreateContext("https://contoso.sharepoint.com/sites/enterprise-wiki"))
            {
                var information = CreateInPlaceInformation(sourceContext, overwrite: true);
                var sourceBefore = SnapshotSource(information.SourcePage);
                var sourceHadPendingRequest = sourceContext.HasPendingRequest;
                var addCalls = 0;
                var transformator = CreateTransformator(sourceContext, targetContext);

                var exception = Assert.ThrowsException<InvalidOperationException>(() => transformator.CreateTargetPageAfterCollisionProbe(
                    PublishingPageTransformationTarget.SameWeb,
                    information,
                    () => throw new InvalidOperationException("target probe inconclusive"),
                    _ =>
                    {
                        addCalls++;
                        return null;
                    },
                    out _));

                Assert.AreEqual("target probe inconclusive", exception.Message);
                Assert.AreEqual(0, addCalls, "AddClientSidePage seam must not run after an inconclusive in-place probe.");
                AssertSourceUnchanged(sourceBefore, information.SourcePage);
                Assert.AreEqual(sourceHadPendingRequest, sourceContext.HasPendingRequest, "The source context request state changed.");
            }
        }

        [TestMethod]
        public void TransformSeamConfirmedMissingTargetInvokesAddOnce()
        {
            using (var sourceContext = CreateContext("https://contoso.sharepoint.com/sites/enterprise-wiki"))
            using (var targetContext = CreateContext("https://contoso.sharepoint.com/sites/enterprise-wiki"))
            {
                var information = CreateInPlaceInformation(sourceContext, overwrite: false);
                var addCalls = 0;
                var addedPageName = string.Empty;
                var transformator = CreateTransformator(sourceContext, targetContext);

                transformator.CreateTargetPageAfterCollisionProbe(
                    PublishingPageTransformationTarget.SameWeb,
                    information,
                    () => throw new PageDoesNotExistInWebException($"{information.TargetPageName} - {LogStrings.TransformPageDoesNotExistInWeb}"),
                    pageName =>
                    {
                        addCalls++;
                        addedPageName = pageName;
                        return null;
                    },
                    out _);

                Assert.AreEqual(1, addCalls);
                Assert.AreEqual("converted/source.aspx", addedPageName);
            }
        }

        [TestMethod]
        public void TargetPageNameWithoutExtensionIsCanonicalized()
        {
            // Saving appends .aspx, so an uncanonicalized name would probe and read back a different
            // file than the one written, letting an existing target page be overwritten unnoticed.
            Assert.AreEqual("existing.aspx", BasePageTransformator.EnsureAspxExtension("existing"));
        }

        [TestMethod]
        public void TargetPageNameKeepsAnExistingExtensionRegardlessOfCasing()
        {
            Assert.AreEqual("existing.aspx", BasePageTransformator.EnsureAspxExtension("existing.aspx"));
            Assert.AreEqual("existing.ASPX", BasePageTransformator.EnsureAspxExtension("existing.ASPX"));
        }

        [TestMethod]
        public void TargetPageNameCanonicalizationIsIdempotent()
        {
            var canonicalized = BasePageTransformator.EnsureAspxExtension("existing");

            Assert.AreEqual(canonicalized, BasePageTransformator.EnsureAspxExtension(canonicalized));
        }

        [TestMethod]
        public void TargetPageNameCanonicalizationKeepsEmbeddedDots()
        {
            Assert.AreEqual("report.v2.aspx", BasePageTransformator.EnsureAspxExtension("report.v2"));
        }

        [TestMethod]
        public void TargetPageNameCanonicalizationLeavesAnAbsentNameToTheSourcePageDefault()
        {
            Assert.IsNull(BasePageTransformator.EnsureAspxExtension(null));
            Assert.AreEqual(string.Empty, BasePageTransformator.EnsureAspxExtension(string.Empty));
        }

        private static PublishingPageTransformationTarget ValidateSameWeb(
            bool hasWritableSitePages)
        {
            return PublishingPageTransformationValidator.ValidateTarget(
                SourceSiteId,
                SourceSiteId,
                SourceWebId,
                SourceWebId,
                hasWritableSitePages);
        }

        private static PublishingPageTransformator CreateTransformator(ClientContext sourceContext, ClientContext targetContext)
        {
            return new PublishingPageTransformator(sourceContext, targetContext, new PageTransformation(), null);
        }

        private static PublishingPageTransformationInformation CreateInPlaceInformation(ClientContext sourceContext, bool overwrite)
        {
            var sourcePage = sourceContext.Web.Lists.GetByTitle("Pages").GetItemById(7);
            sourcePage["CCD_SourceIdentity"] = "Pages/7/v3";
            sourcePage["CCD_SourceDigest"] = "sha256:unchanged";

            return new PublishingPageTransformationInformation(sourcePage, overwrite)
            {
                Folder = "converted/",
                TargetPageName = "source.aspx",
            };
        }

        private static Dictionary<string, string> SnapshotSource(ListItem sourcePage)
        {
            return sourcePage.FieldValues.ToDictionary(
                pair => pair.Key,
                pair => pair.Value?.ToString());
        }

        private static void AssertSourceUnchanged(Dictionary<string, string> expected, ListItem actual)
        {
            var actualSnapshot = SnapshotSource(actual);
            CollectionAssert.AreEquivalent(expected.Keys.ToArray(), actualSnapshot.Keys.ToArray());

            foreach (var pair in expected)
            {
                Assert.AreEqual(pair.Value, actualSnapshot[pair.Key], $"Source field '{pair.Key}' changed.");
            }
        }

        private sealed class SitePagesFeatureFixture : IMockResponseProvider
        {
            private const string WebUrl = "https://contoso.sharepoint.com/sites/enterprise-wiki";
            private readonly MockEntryResponseProvider provider = new MockEntryResponseProvider();
            internal readonly List<object> Features = new List<object>();
            internal bool FailRequests { get; set; }
            internal int RequestCount { get; private set; }

            internal void AddFeature(Guid id)
            {
                Features.Add(new { _ObjectType_ = "SP.Feature", DefinitionId = $"/Guid({id})/" });
            }

            internal ClientContext CreateContext()
            {
                provider.ResponseEntries.Add(new MockResponseEntry<object>
                {
                    Url = WebUrl,
                    PropertyName = "Features",
                    ReturnValue = new { _Child_Items_ = Features },
                });
                var context = PublishingPageInPlaceTransformationTests.CreateContext(WebUrl, provider);
                context.WebRequestExecutorFactory = new MockWebRequestExecutorFactory(this);
                return context;
            }

            public string GetResponse(string url, string verb, string body)
            {
                RequestCount++;
                var request = XDocument.Parse(body);
                Assert.IsFalse(request.Descendants().Any(element =>
                    element.Name.LocalName == "Method" || element.Name.LocalName == "SetProperty"),
                    "The Site Pages preflight must only read the active feature collection.");
                if (FailRequests)
                {
                    throw new InvalidOperationException("Feature query failed");
                }
                return provider.GetResponse(url, verb, body);
            }
        }

        private static ClientContext CreateContext(string webUrl, MockEntryResponseProvider responseProvider = null)
        {
            responseProvider = responseProvider ?? new MockEntryResponseProvider();
            responseProvider.ResponseEntries.Add(new MockResponseEntry<object>
            {
                Url = webUrl,
                PropertyName = "Web",
                ReturnValue = new
                {
                    Url = webUrl,
                    ServerRelativeUrl = "/sites/enterprise-wiki",
                },
            });
            responseProvider.ResponseEntries.Add(new MockResponseEntry<object>
            {
                Url = webUrl,
                PropertyName = "Site",
                ReturnValue = new
                {
                    Url = webUrl,
                    ServerRelativeUrl = "/sites/enterprise-wiki",
                },
            });

            CacheManager.Instance.SetSharePointVersion(new Uri(webUrl), SPVersion.SPO);
            CacheManager.Instance.SetExactSharePointVersion(new Uri(webUrl), "16.0.0.26000");

            return new ClientContext(webUrl)
            {
                WebRequestExecutorFactory = new MockWebRequestExecutorFactory(responseProvider),
            };
        }
    }
}
