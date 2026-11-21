using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Transform;
using PnP.Core.Modernization.Cache;
using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace PnP.Core.Modernization.Tests.Transform
{
    [TestClass]
    [TestCategory("Offline")]
    public class AssetTransferCoreTests
    {
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void DownloadsFromOwningWebAndRejectsIncorrectPayloadLength(bool incorrectPayload)
        {
            const string root = "https://contoso.sharepoint.com/sites/source";
            const string owner = root + "/wiki";
            const string destination = "https://contoso.sharepoint.com/sites/target";
            const string sourcePath = "/sites/source/wiki/SiteAssets/nested/image.png";
            const string targetFolder = "/sites/target/SiteAssets/page";
            var fileId = Guid.NewGuid();
            var content = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
            byte[] uploaded = null;
            Uri downloadedFrom = null;
            CacheManager.Instance.SetSharePointVersion(new Uri(root), SPVersion.SPO);
            using var source = CoreTestContext.Create(root, (request, _) =>
            {
                var path = request.RequestUri.AbsolutePath;
                if (path.Contains("GetWebUrlFromPageUrl", StringComparison.OrdinalIgnoreCase))
                    return CoreTestContext.Json(new { value = owner });
                if (path.EndsWith("/_api/web", StringComparison.OrdinalIgnoreCase))
                    return CoreTestContext.Json(new { Id = fileId, Url = owner, ServerRelativeUrl = "/sites/source/wiki", WebTemplate = "STS" });
                if (path.EndsWith("/_api/site", StringComparison.OrdinalIgnoreCase))
                    return CoreTestContext.Json(new { Id = fileId, Url = root, ServerRelativeUrl = "/sites/source" });
                if (path.Contains("GetFileBy", StringComparison.OrdinalIgnoreCase))
                    return CoreTestContext.Json(new { UniqueId = fileId, Name = "image.png", ServerRelativeUrl = sourcePath, Length = content.Length });
                if (path.EndsWith("/download.aspx", StringComparison.OrdinalIgnoreCase))
                {
                    downloadedFrom = request.RequestUri;
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(incorrectPayload ? Encoding.UTF8.GetBytes("<html>error page</html>") : content)
                    };
                }
                throw new InvalidOperationException("Unexpected source request: " + request.RequestUri);
            });
            using var target = CoreTestContext.Create(destination, (request, _) =>
            {
                if (request.Method == HttpMethod.Post)
                {
                    uploaded = request.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    return CoreTestContext.Json(new { UniqueId = fileId, Name = "image.png", ServerRelativeUrl = targetFolder + "/image.png" });
                }
                if (request.RequestUri.AbsolutePath.Contains("GetFolderBy", StringComparison.OrdinalIgnoreCase))
                    return CoreTestContext.Json(new { UniqueId = fileId, ServerRelativeUrl = targetFolder });
                throw new InvalidOperationException("Unexpected target request: " + request.RequestUri);
            });

            var transfer = new AssetTransfer(source, target);
            if (incorrectPayload)
            {
                StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() =>
                    transfer.CopyAssetToTargetLocation(sourcePath, targetFolder)).Message, "length");
                Assert.IsNull(uploaded);
            }
            else
            {
                Assert.AreEqual(targetFolder + "/image.png", transfer.CopyAssetToTargetLocation(sourcePath, targetFolder));
                CollectionAssert.AreEqual(content, uploaded);
            }
            Assert.AreEqual(owner + "/_layouts/15/download.aspx", downloadedFrom.GetLeftPart(UriPartial.Path));
        }
    }

    [TestClass]
    [TestCategory("Integration")]
    public class AssetTransferTests
    {

        /// <summary>
        /// This test validates with SharePoint the entire operation
        /// </summary>
        [TestMethod]
        public void AssetTransfer_CopyAssetToTarget_SmallFileTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevTeamSiteUrl")))
                {
                    AssetTransfer assetTransfer = new AssetTransfer(sourceClientContext, targetClientContext);

                    var targetWebUrl = targetClientContext.Web.EnsureProperty(o => o.ServerRelativeUrl);
                    var sourceWebUrl = sourceClientContext.Web.EnsureProperty(o => o.ServerRelativeUrl);

                    // Very crude test - ensure the site is setup for this ahead of the test
                    var sourceFileServerRelativeUrl = $"{sourceWebUrl}/SiteImages/extra8_500x500.jpg";
                    var targetLocation = $"{targetWebUrl}/Shared%20Documents"; //Shared Documents for example, Site Assets may not exist on vanilla sites

                    assetTransfer.CopyAssetToTargetLocation(sourceFileServerRelativeUrl, targetLocation);
                }
            }

            Assert.Inconclusive(TestCommon.InconclusiveNoAutomatedChecksMessage);
        }

        /// <summary>
        /// This test validates with SharePoint the entire operation
        /// </summary>
        [TestMethod]
        public void AssetTransfer_CopyAssetToTarget_LargeFileTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevTeamSiteUrl")))
                {
                    AssetTransfer assetTransfer = new AssetTransfer(sourceClientContext, targetClientContext);

                    // Very crude test - ensure the site is setup for this ahead of the test
                    // Note this file is not included in this project assets due to its licensing. Pls find a > 3MB file to use as a test.
                    var targetWebUrl = targetClientContext.Web.EnsureProperty(o => o.ServerRelativeUrl);
                    var sourceWebUrl = sourceClientContext.Web.EnsureProperty(o => o.ServerRelativeUrl);

                    var sourceFileServerRelativeUrl = $"{sourceWebUrl}/SiteImages/bigstock-Html-Web-Code-57446159.jpg";
                    var targetLocation = $"{targetWebUrl}/Shared%20Documents"; //Shared Documents for example, Site Assets may not exist on vanilla sites

                    assetTransfer.CopyAssetToTargetLocation(sourceFileServerRelativeUrl, targetLocation);
                }
            }

            Assert.Inconclusive(TestCommon.InconclusiveNoAutomatedChecksMessage);
        }

        /// <summary>
        /// This test validates with SharePoint the entire operation
        /// </summary>
        [TestMethod]
        public void AssetTransfer_CopyAssetToTarget_PagesWithImageWebPartTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevTeamSiteUrl")))
                {
                    var pageTransformator = new PageTransformator(sourceClientContext, targetClientContext);

                    var pages = WebExtensions.GetPages(sourceClientContext.Web,"WPP_Image-Asset");

                    foreach (IListItem page in pages)
                    {
                        PageTransformationInformation pti = new PageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,

                            // Don't log test runs
                            SkipTelemetry = true,

                            // Replace embedded images and iframes with a placeholder and add respective images and video web parts at the bottom of the page
                            //HandleWikiImagesAndVideos = false,
                        };

                        pageTransformator.Transform(pti);
                    }
                }
            }

            Assert.Inconclusive(TestCommon.InconclusiveNoAutomatedChecksMessage);
        }

        /// <summary>
        /// This test validates with SharePoint the entire operation
        /// </summary>
        [TestMethod]
        public void AssetTransfer_ValidateSupportedLocation_AspxRejectTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext())
                {
                    // Needs valid client contexts as they are part of the checks.
                    AssetTransfer assetTransfer = new AssetTransfer(sourceClientContext, targetClientContext);

                    var webUrl = sourceClientContext.Web.EnsureProperty(o => o.ServerRelativeUrl);
                    var result = assetTransfer.ValidateAssetInSupportedLocation($"{webUrl}/siteassets/wrongfile.aspx");
                    var expected = false;

                    Assert.AreEqual(expected, result);
                }
            }
        }

        /// <summary>
        /// This test validates with SharePoint the entire operation
        /// </summary>
        [TestMethod]
        public void AssetTransfer_ValidateSupportedLocation_OtherTenantRejectTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext())
                {
                    // Needs valid client contexts as they are part of the checks.
                    AssetTransfer assetTransfer = new AssetTransfer(sourceClientContext, targetClientContext);
                    var result = assetTransfer.ValidateAssetInSupportedLocation("https://faketenant.sharepoint.com/sites/fakesitecollection/images/afakeimage.jpg");
                    var expected = false;

                    Assert.AreEqual(expected, result);
                }
            }
        }

        /// <summary>
        /// This test validates with SharePoint the entire operation
        /// </summary>
        [TestMethod]
        public void AssetTransfer_ValidateSupportedLocation_OtherSiteCollectionRelativeRejectTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext())
                {
                    // Needs valid client contexts as they are part of the checks.
                    AssetTransfer assetTransfer = new AssetTransfer(sourceClientContext, targetClientContext);
                    var result = assetTransfer.ValidateAssetInSupportedLocation($"/sites/fakesitecollection/images/afakeimage.jpg");
                    var expected = false;

                    Assert.AreEqual(expected, result);
                }
            }
        }

        [TestMethod]
        public void AssetTransfer_ValidateSupportedLocation_ImageOnSubsiteAcceptTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext())
                {
                    // Needs valid client contexts as they are part of the checks.
                    AssetTransfer assetTransfer = new AssetTransfer(sourceClientContext, targetClientContext);

                    var webUrl = sourceClientContext.Web.EnsureProperty(o => o.ServerRelativeUrl);
                    
                    var result = assetTransfer.ValidateAssetInSupportedLocation($"{webUrl}/subsite/siteassets/afakeimage.jpg");
                    var expected = true;

                    Assert.AreEqual(expected, result);
                }
            }
        }

        /// <summary>
        /// This test validates with SharePoint the entire operation
        /// </summary>
        [TestMethod]
        public void AssetTransfer_ValiateSupportedLocation_SameCtxRejectTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext())
            {
                using (var sourceClientContext = TestCommon.CreateClientContext())
                {
                    // Needs valid client contexts as they are part of the checks.
                    AssetTransfer assetTransfer = new AssetTransfer(sourceClientContext, targetClientContext);
                    var webUrl = sourceClientContext.Web.EnsureProperty(o => o.ServerRelativeUrl);
                    var result = assetTransfer.ValidateAssetInSupportedLocation($"{webUrl}/siteassets/rightfile.jpg");
                    var expected = false;

                    Assert.AreEqual(expected, result);
                }
            }
        }

        /// <summary>
        /// This test validates with SharePoint the entire operation
        /// </summary>
        [TestMethod]
        public void AssetTransfer_ValiateSupportedLocation_AcceptTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext())
                {
                    // Needs valid client contexts as they are part of the checks.
                    AssetTransfer assetTransfer = new AssetTransfer(sourceClientContext, targetClientContext);

                    var webUrl = sourceClientContext.Web.EnsureProperty(o => o.ServerRelativeUrl);
                    var result = assetTransfer.ValidateAssetInSupportedLocation($"{webUrl}/siteassets/rightfile.jpg");
                    var expected = true;

                    Assert.AreEqual(expected, result);
                }
            }
        }

        /// <summary>
        /// This test validates with SharePoint the entire operation
        /// </summary>
        [TestMethod]
        public void AssetTransfer_CopyAssetToTarget_WithCacheSameFileTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevTeamSiteUrl")))
                {
                    var pageTransformator = new PageTransformator(sourceClientContext, targetClientContext);

                    var pages = WebExtensions.GetPages(sourceClientContext.Web,"WPP_Image-Asset-MultipleImages-Test");

                    foreach (IListItem page in pages)
                    {
                        PageTransformationInformation pti = new PageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,

                            // Don't log test runs
                            SkipTelemetry = true,

                            // Replace embedded images and iframes with a placeholder and add respective images and video web parts at the bottom of the page
                            //HandleWikiImagesAndVideos = false,
                        };

                        pageTransformator.Transform(pti);
                    }
                }
            }

            Assert.Inconclusive(TestCommon.InconclusiveNoAutomatedChecksMessage);
        }

        /// <summary>
        /// This test validates with SharePoint the entire operation
        /// </summary>
        [TestMethod]
        public void AssetTransfer_CopyAssetToTarget_WithCacheMultipleFileTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevTeamSiteUrl")))
                {
                    var pageTransformator = new PageTransformator(sourceClientContext, targetClientContext);

                    var pages = WebExtensions.GetPages(sourceClientContext.Web,"WPP_Image-Asset");

                    foreach (IListItem page in pages)
                    {
                        PageTransformationInformation pti = new PageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,

                            // Don't log test runs
                            SkipTelemetry = true,

                            // Replace embedded images and iframes with a placeholder and add respective images and video web parts at the bottom of the page
                            //HandleWikiImagesAndVideos = false,
                        };

                        pageTransformator.Transform(pti);
                    }
                }
            }

            Assert.Inconclusive(TestCommon.InconclusiveNoAutomatedChecksMessage);
        }

        /// <summary>
        /// This test validates with SharePoint the entire operation
        /// </summary>
        [TestMethod]
        public void AssetTransfer_CopyAssetToTarget_WithFullTransformTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevTeamSiteUrl")))
                {
                    var pageTransformator = new PageTransformator(sourceClientContext, targetClientContext);

                    var pages = WebExtensions.GetPages(sourceClientContext.Web,"p");

                    foreach (IListItem page in pages)
                    {
                        PageTransformationInformation pti = new PageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,

                            // Don't log test runs
                            SkipTelemetry = true,

                            // Replace embedded images and iframes with a placeholder and add respective images and video web parts at the bottom of the page
                            //HandleWikiImagesAndVideos = false,
                        };

                        pageTransformator.Transform(pti);
                    }
                }
            }

            Assert.Inconclusive(TestCommon.InconclusiveNoAutomatedChecksMessage);
        }

        /// <summary>
        /// This test validates with SharePoint the entire operation
        /// </summary>
        [TestMethod]
        public void AssetTransfer_EnsureSiteAssets_Test()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext())
                {
                    AssetTransfer assetTransfer = new AssetTransfer(sourceClientContext, targetClientContext);
                    assetTransfer.EnsureSiteAssetsLibrary();

                    // Validate the site assets works
                    var siteAssetsExist = targetClientContext.Web.Lists.GetByTitle("Site Assets") != null;
                    Assert.IsTrue(siteAssetsExist);
                    // Clean up the test target site

                }
            }
        }

        /// <summary>
        /// This test validates with SharePoint the entire operation
        /// </summary>
        [TestMethod]
        public void AssetTransfer_EnsureFolderLocaton_Test()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext())
                {
                    // Needs valid client contexts as they are part of the checks.
                    AssetTransfer assetTransfer = new AssetTransfer(sourceClientContext, targetClientContext);
                    var result = assetTransfer.EnsureDestination("WPP_Image-Asset-Test.aspx");

                    var webUrl = targetClientContext.Web.EnsureProperty(o => o.ServerRelativeUrl);
                    var expected = $"{webUrl}/SiteAssets/SitePages/WPP_Image-Asset-Test";

                    Assert.AreEqual(expected, result);
                }
            }
        }

        /// <summary>
        /// This test validates with SharePoint the entire operation
        /// </summary>
        [TestMethod]
        public void AssetTransfer_TransferAsset_Test()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevTeamSiteUrl")))
                {
                    // Needs valid client contexts as they are part of the checks.
                    AssetTransfer assetTransfer = new AssetTransfer(sourceClientContext, targetClientContext);
                    
                    var targetWebUrl = targetClientContext.Web.EnsureProperty(o => o.ServerRelativeUrl);
                    var sourceWebUrl = sourceClientContext.Web.EnsureProperty(o => o.ServerRelativeUrl);

                    // Very crude test - ensure the site is setup for this ahead of the test
                    var sourceFileServerRelativeUrl = $"{sourceWebUrl}/SiteImages/extra8_500x500.jpg";
                    var targetLocation = $"{targetWebUrl}/Shared%20Documents"; //Shared Documents for example, Site Assets may not exist on vanilla sites

                    var result = assetTransfer.TransferAsset(sourceFileServerRelativeUrl, "This is a unit test page.aspx"); //Page shouldnt need to exist at this point
                    var expected = $"{targetWebUrl}/SiteAssets/SitePages/This-is-a-unit-test-page/extra8_500x500.jpg";

                    Assert.AreEqual(expected, result);
                }
            }
        }

        [TestMethod]
        public void AssetTransfer_TransferAsset_ContextSiteImageOnSubSiteAcceptTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint
            // Scenario here is when the source context is a subsite and the image asset is on the Site Collection

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPORootDevSiteUrl")))
                {
                    // Needs valid client contexts as they are part of the checks.
                    AssetTransfer assetTransfer = new AssetTransfer(sourceClientContext, targetClientContext);

                    var siteUrl = sourceClientContext.Site.EnsureProperty(o => o.ServerRelativeUrl);
                    var target = $"{TestCommon.AppSetting("SPOTargetSiteUrl")}/SiteAssets/";

                    var sourceImageLocation = $"{siteUrl}/en/PublishingImages/extra.jpg";

                    var result = assetTransfer.CopyAssetToTargetLocation(sourceImageLocation, target);
                    Assert.IsTrue(!string.IsNullOrEmpty(result));
                }
            }
        }

        [TestMethod]
        public void AssetTransfer_TransferAsset_EnsureContextRootSiteTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint
            // Scenario here is when the source context is a subsite and the image asset is on the Site Collection

            AssetTransferOfAsset("SiteCollectionImages/extra.jpg");

        }

        [TestMethod]
        public void AssetTransfer_TransferAsset_EnsureContextSubSiteTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint
            // Scenario here is when the source context is a subsite and the image asset is on the Site Collection

            AssetTransferOfAsset("en/PublishingImages/extra.jpg");

        }

        [TestMethod]
        public void AssetTransfer_TransferAsset_EnsureContextDeepTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint
            // Scenario here is when the source context is a subsite and the image asset is on the Site Collection

            AssetTransferOfAsset("en/subsite/PublishingImages/extra.jpg");

        }

        [TestMethod]
        public void AssetTransfer_TransferAsset_EnsureContextDeepWithFolderTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint
            // Scenario here is when the source context is a subsite and the image asset is on the Site Collection

            AssetTransferOfAsset("en/subsite/PublishingImages/Example-Folder/extra.jpg");

        }

        [TestMethod]
        public void AssetTransfer_TransferAsset_EnsureContextRootTest()
        {
            //Note: This is more of a system test rather than unit given its dependency on SharePoint
            // Scenario here is when the source context is a subsite and the image asset is on the Site Collection


            AssetTransferOfAsset("SiteAssets/extra.jpg", TestCommon.AppSetting("SPOTenantRootSiteUrl"));

        }


        /// <summary>
        /// Note: this test will only work if the files are present at the parts of the sharepoint site
        /// and the source site originates from a sub site
        /// </summary>
        /// <param name="url"></param>
        private void AssetTransferOfAsset(string url)
        {
            AssetTransferOfAsset(url, TestCommon.AppSetting("SPODevSiteUrl"));
        }

        /// <summary>
        /// Note: this test will only work if the files are present at the parts of the sharepoint site
        /// and the source site originates from a sub site
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sourceContextUrl"></param>
        private void AssetTransferOfAsset(string url, string sourceContextUrl)
        {
            if (string.IsNullOrEmpty(url))
            {
                Assert.Inconclusive("You havent provided a URL");
            }

            url = url.TrimStart('/'); //Remove slash at the beginning

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(sourceContextUrl))
                {
                    // Needs valid client contexts as they are part of the checks.
                    AssetTransfer assetTransfer = new AssetTransfer(sourceClientContext, targetClientContext);
                    assetTransfer.RegisterObserver(new UnitTestLogObserver());

                    var siteUrl = sourceClientContext.Site.EnsureProperty(o => o.ServerRelativeUrl);
                    var target = $"{TestCommon.AppSetting("SPOTargetSiteUrl")}/SiteAssets/";

                    var sourceImageLocation = $"{siteUrl.TrimEnd('/')}/{url}";
                    Console.WriteLine("Source Image Location: {0}", sourceImageLocation);

                    assetTransfer.EnsureAssetContextIfRequired(sourceClientContext, sourceImageLocation);

                    var result = assetTransfer.CopyAssetToTargetLocation(sourceImageLocation, target);

                    Assert.IsTrue(!string.IsNullOrEmpty(result));
                }
            }
        }


    }
}
