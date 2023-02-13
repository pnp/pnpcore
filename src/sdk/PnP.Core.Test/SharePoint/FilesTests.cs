using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class FilesTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        #region Query file in folder
        [TestMethod]
        public async Task QueryFileInFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // Get the default document library root folder
                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == documentName);

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual(documentName, documentToFind.Name);
                Assert.AreEqual(documentUrl, documentToFind.ServerRelativeUrl);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }
        #endregion

        #region Get File User Properties
        [TestMethod]
        public async Task GetFileUserPropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, w => w.Author, w => w.ModifiedBy);

                Assert.IsNotNull(testDocument);
                Assert.IsNotNull(testDocument.Author);
                Assert.AreNotEqual(0, testDocument.Author.Id);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }
        #endregion

        #region GetFileByServerRelativeUrl()
        [TestMethod]
        public async Task GetFileByServerRelativeUrlAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.IsNotNull(testDocument);
                Assert.AreEqual(documentName, testDocument.Name);
                Assert.AreEqual(documentUrl, testDocument.ServerRelativeUrl);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFileByServerRelativeUrlTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = context.Web.GetFileByServerRelativeUrl(documentUrl);

                Assert.IsNotNull(testDocument);
                Assert.AreEqual(documentName, testDocument.Name);
                Assert.AreEqual(documentUrl, testDocument.ServerRelativeUrl);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFileByServerRelativeUrlCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlBatchAsync(documentUrl);
                await context.ExecuteAsync();

                Assert.IsNotNull(testDocument);
                Assert.AreEqual(documentName, testDocument.Name);
                Assert.AreEqual(documentUrl, testDocument.ServerRelativeUrl);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFileByServerRelativeUrlCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = context.Web.GetFileByServerRelativeUrlBatch(documentUrl);
                await context.ExecuteAsync();

                Assert.IsNotNull(testDocument);
                Assert.AreEqual(documentName, testDocument.Name);
                Assert.AreEqual(documentUrl, testDocument.ServerRelativeUrl);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFileByServerRelativeUrlBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var batch = context.NewBatch();
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlBatchAsync(batch, documentUrl);
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(testDocument);
                Assert.AreEqual(documentName, testDocument.Name);
                Assert.AreEqual(documentUrl, testDocument.ServerRelativeUrl);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFileByServerRelativeUrlBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var batch = context.NewBatch();
                IFile testDocument = context.Web.GetFileByServerRelativeUrlBatch(batch, documentUrl);
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(testDocument);
                Assert.AreEqual(documentName, testDocument.Name);
                Assert.AreEqual(documentUrl, testDocument.ServerRelativeUrl);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetNonExistingFileByServerRelativeUrlAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                bool exceptionThrown = false;
                try
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync($"{TestCommon.Instance.TestUris[TestCommon.TestSite].LocalPath}/Shared%20Documents/IdontExist.pdf");

                    Assert.IsNotNull(testDocument);
                }
                catch (SharePointRestServiceException ex)
                {
                    var error = ex.Error as SharePointRestError;
                    // Indicates the file did not exist
                    if (error.HttpResponseCode == 404 && error.ServerErrorCode == -2130575338)
                    {
                        exceptionThrown = true;
                    }
                }

                Assert.IsTrue(exceptionThrown);
            }

        }

        [TestMethod]
        public async Task GetNonExistingFileByServerRelativeUrlOrDefaultAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = context.Web.GetFileByServerRelativeUrlOrDefault($"{TestCommon.Instance.TestUris[TestCommon.TestSite].LocalPath}/Shared%20Documents/IdontExist.pdf");
                Assert.IsTrue(testDocument == null);
            }
        }

        [TestMethod]
        public async Task GetExistingFileInOtherSiteByServerRelativeUrlAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                bool exceptionThrown = false;
                try
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync($"{TestCommon.Instance.TestUris[TestCommon.NoGroupTestSite].LocalPath}/Shared%20Documents/IdontExist.pdf");

                    Assert.IsNotNull(testDocument);
                }
                catch (SharePointRestServiceException ex)
                {
                    var error = ex.Error as SharePointRestError;
                    // Indicates the file check was for a file in another site collection
                    if (error.Message.Contains("SPWeb.ServerRelativeUrl"))
                    {
                        exceptionThrown = true;
                    }
                }

                Assert.IsTrue(exceptionThrown);
            }

        }
        #endregion

        #region GetFileById tests

        [TestMethod]
        public async Task GetFileByIdTest()
        {
            //TestCommon.Instance.Mocking = false;

            try
            {
                (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IFile testDocument = context.Web.GetFileByServerRelativeUrl(documentUrl);

                    IFile testDocumentLoadedById = context.Web.GetFileById(testDocument.UniqueId);

                    Assert.IsNotNull(testDocument.UniqueId == testDocumentLoadedById.UniqueId);
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(2);
            }
        }

        [TestMethod]
        public async Task GetFileByIdCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            try
            {
                (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IFile testDocument = context.Web.GetFileByServerRelativeUrlBatch(documentUrl);
                    await context.ExecuteAsync();

                    IFile testDocumentLoadedById = context.Web.GetFileByIdBatch(testDocument.UniqueId);
                    await context.ExecuteAsync();

                    Assert.IsNotNull(testDocument.UniqueId == testDocumentLoadedById.UniqueId);
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(2);
            }
        }

        [TestMethod]
        public async Task GetFileByIdBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            try
            {
                (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IFile testDocument = context.Web.GetFileByServerRelativeUrlBatch(documentUrl);
                    await context.ExecuteAsync();

                    var batch = context.NewBatch();
                    IFile testDocumentLoadedById = context.Web.GetFileByIdBatch(batch, testDocument.UniqueId);
                    await context.ExecuteAsync(batch);

                    Assert.IsNotNull(testDocument.UniqueId == testDocumentLoadedById.UniqueId);
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(2);
            }
        }
        #endregion

        #region GetFileByLink tests

        [TestMethod]
        public async Task GetFileByLinkTest()
        {
            //TestCommon.Instance.Mocking = false;

            try
            {
                (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IFile testDocument = context.Web.GetFileByServerRelativeUrl(documentUrl);

                    // Test regular link
                    IFile testDocumentLoadedByLink = context.Web.GetFileByLink($"https://{context.Uri.DnsSafeHost}{documentUrl}");
                    Assert.IsNotNull(testDocument.UniqueId == testDocumentLoadedByLink.UniqueId);

                    // Test sharing link
                    var link = testDocument.CreateOrganizationalSharingLink(new Model.Security.OrganizationalLinkOptions
                    {
                        Type = Model.Security.ShareType.View,
                    });

                    IFile testDocumentLoadedByLink2 = context.Web.GetFileByLink(link.Link.WebUrl);
                    Assert.IsNotNull(testDocument.UniqueId == testDocumentLoadedByLink2.UniqueId);
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(2);
            }
        }

        [TestMethod]
        public async Task GetFileByLinkOtherSiteCollectionTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // Take the first list item
                // For now hardcoded to https://bertonline.sharepoint.com/sites/prov-1/SiteAssets/donotdelete-sharingtest.mp4
                // https://bertonline.sharepoint.com/:v:/s/prov-1/Ed2UMAoNf0tJhAjSJLt94wYBUd9U-ZhCKOoOXZcGS2dLBQ?e=KobeE5                    

                // Test regular link
                IFile testDocumentLoadedByLink = context.Web.GetFileByLink($"https://bertonline.sharepoint.com/sites/prov-1/SiteAssets/donotdelete-sharingtest.mp4");

                // Test sharing link
                IFile testDocumentLoadedByLink2 = context.Web.GetFileByLink("https://bertonline.sharepoint.com/:v:/s/prov-1/Ed2UMAoNf0tJhAjSJLt94wYBUd9U-ZhCKOoOXZcGS2dLBQ?e=KobeE5");
                Assert.IsNotNull(testDocumentLoadedByLink.UniqueId == testDocumentLoadedByLink2.UniqueId);
            }
        }

        #endregion

        #region Publish() variants
        // TODO: Review to cover 6 variants of each File methods with this set of tests as example
        [TestMethod]
        public async Task PublishFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishAsync("TEST PUBLISH");

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsTrue(!string.IsNullOrEmpty(testDocument.ContentTag));
                Assert.AreEqual(CustomizedPageStatus.None, testDocument.CustomizedPageStatus);
                Assert.IsTrue(!string.IsNullOrEmpty(testDocument.ETag));
                Assert.IsFalse(testDocument.IrmEnabled);
                Assert.IsTrue(testDocument.TimeCreated != DateTime.MinValue);
                Assert.IsTrue(testDocument.TimeLastModified != DateTime.MinValue);
                Assert.IsTrue(testDocument.LinkingUri.Contains(".sharepoint.com"));
                Assert.IsTrue(testDocument.LinkingUrl.Contains(".sharepoint.com"));
                Assert.AreEqual("1.0", testDocument.UIVersionLabel);
                Assert.AreEqual(512, testDocument.UIVersion);
                Assert.AreEqual("", testDocument.Title);

                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task PublishFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                testDocument.Publish("TEST PUBLISH");

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task PublishFileCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishBatchAsync("TEST PUBLISH");
                await context.ExecuteAsync();

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task PublishFileCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                testDocument.PublishBatch("TEST PUBLISH");
                await context.ExecuteAsync();

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task PublishFileBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                var batch = context.NewBatch();
                await testDocument.PublishBatchAsync(batch, "TEST PUBLISH");
                await context.ExecuteAsync(batch);

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task PublishFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                var batch = context.NewBatch();
                testDocument.PublishBatch(batch, "TEST PUBLISH");
                await context.ExecuteAsync(batch);

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }
        #endregion

        #region Unpublish() variants
        [TestMethod]
        public async Task UnpublishFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishAsync("TEST PUBLISH");
                await testDocument.UnpublishAsync("TEST UNPUBLISHED");

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task UnpublishFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishAsync("TEST PUBLISH");
                testDocument.Unpublish("TEST UNPUBLISHED");

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task UnpublishFileCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishAsync("TEST PUBLISH");
                await testDocument.UnpublishBatchAsync("TEST UNPUBLISHED");
                await context.ExecuteAsync();

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task UnpublishFileCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishAsync("TEST PUBLISH");
                testDocument.UnpublishBatch("TEST UNPUBLISHED");
                await context.ExecuteAsync();

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task UnpublishFileBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishAsync("TEST PUBLISH");
                var batch = context.NewBatch();
                await testDocument.UnpublishBatchAsync(batch, "TEST UNPUBLISHED");
                await context.ExecuteAsync(batch);

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task UnpublishFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishAsync("TEST PUBLISH");
                var batch = context.NewBatch();
                testDocument.UnpublishBatch(batch, "TEST UNPUBLISHED");
                await context.ExecuteAsync(batch);

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }
        #endregion

        #region Checkout() variants
        [TestMethod]
        public async Task CheckoutFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                await testDocument.CheckoutAsync();

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.CheckOutType, f => f.CheckedOutByUser);
                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsNotNull(testDocument.CheckedOutByUser);
                Assert.AreNotEqual(0, testDocument.CheckedOutByUser.Id);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task CheckoutFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                testDocument.Checkout();

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.CheckOutType, f => f.CheckedOutByUser);
                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsNotNull(testDocument.CheckedOutByUser);
                Assert.AreNotEqual(0, testDocument.CheckedOutByUser.Id);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task CheckoutFileCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                await testDocument.CheckoutBatchAsync();
                await context.ExecuteAsync();

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.CheckOutType, f => f.CheckedOutByUser);
                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsNotNull(testDocument.CheckedOutByUser);
                Assert.AreNotEqual(0, testDocument.CheckedOutByUser.Id);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task CheckoutFileCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                testDocument.CheckoutBatch();
                await context.ExecuteAsync();

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.CheckOutType, f => f.CheckedOutByUser);
                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsNotNull(testDocument.CheckedOutByUser);
                Assert.AreNotEqual(0, testDocument.CheckedOutByUser.Id);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task CheckoutFileBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                var batch = context.NewBatch();
                await testDocument.CheckoutBatchAsync(batch);
                await context.ExecuteAsync(batch);

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.CheckOutType, f => f.CheckedOutByUser);
                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsNotNull(testDocument.CheckedOutByUser);
                Assert.AreNotEqual(0, testDocument.CheckedOutByUser.Id);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task CheckoutFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                var batch = context.NewBatch();
                testDocument.CheckoutBatch(batch);
                await context.ExecuteAsync(batch);

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.CheckOutType, f => f.CheckedOutByUser);
                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsNotNull(testDocument.CheckedOutByUser);
                Assert.AreNotEqual(0, testDocument.CheckedOutByUser.Id);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }
        #endregion

        #region UndoCheckout() variants
        [TestMethod]
        public async Task UndoCheckoutFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.UndoCheckoutAsync();

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task UndoCheckoutFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                testDocument.Checkout();
                testDocument.UndoCheckout();

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task UndoCheckoutFileCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.UndoCheckoutBatchAsync();
                await context.ExecuteAsync();

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task UndoCheckoutFileCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                testDocument.Checkout();
                testDocument.UndoCheckoutBatch();
                await context.ExecuteAsync();

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task UndoCheckoutFileBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                var batch = context.NewBatch();
                await testDocument.UndoCheckoutBatchAsync(batch);
                await context.ExecuteAsync(batch);

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task UndoCheckoutFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                testDocument.Checkout();
                var batch = context.NewBatch();
                testDocument.UndoCheckoutBatch(batch);
                await context.ExecuteAsync(batch);

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }
        #endregion

        #region Approve() variants
        [TestMethod]
        public async Task ApproveFileTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true, parentLibraryApprove: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync("TEST CHECK IN", CheckinType.MajorCheckIn);
                await testDocument.ApproveAsync("TEST APPROVE");

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.CheckOutType, f => f.CheckInComment, f => f.MajorVersion, f => f.MinorVersion, p => p.ListItemAllFields);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);

                Assert.AreEqual("0", testDocument.ListItemAllFields["_ModerationStatus"].ToString());
                Assert.AreEqual("TEST APPROVE", testDocument.ListItemAllFields["_ModerationComments"].ToString());
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task ApproveFileWithBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true, parentLibraryApprove: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.CheckinBatchAsync("TEST CHECK IN", CheckinType.MajorCheckIn);
                await testDocument.ApproveBatchAsync("TEST APPROVE");
                await context.ExecuteAsync();

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.CheckOutType, f => f.CheckInComment, f => f.MajorVersion, f => f.MinorVersion, p => p.ListItemAllFields);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);

                Assert.AreEqual("0", testDocument.ListItemAllFields["_ModerationStatus"].ToString());
                Assert.AreEqual("TEST APPROVE", testDocument.ListItemAllFields["_ModerationComments"].ToString());

                Assert.IsTrue(testDocument.MajorVersion > initialMajorVersion);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        #endregion

        #region Checkin() variants
        [TestMethod]
        public async Task CheckinFileMajorVersionTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync("TEST CHECK IN", CheckinType.MajorCheckIn);

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task CheckinFileMinorVersionTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync("TEST CHECK IN", CheckinType.MinorCheckIn);

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.IsTrue(testDocument.MinorVersion == initialMinorVersion + 1);

            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task CheckinFileOverwriteVersionTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync("TEST CHECK IN", CheckinType.OverwriteCheckIn);

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task CheckinFileWithBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.CheckinBatchAsync("TEST CHECK IN", CheckinType.MajorCheckIn);
                await context.ExecuteAsync();

                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);
                Assert.IsTrue(testDocument.MajorVersion > initialMajorVersion);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }
        #endregion

        #region Recycle() variants
        [TestMethod]
        public async Task RecycleFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Guid recycleBinId = await testDocument.RecycleAsync();

                Assert.AreNotEqual(Guid.Empty, recycleBinId);

                try
                {
                    testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        [TestMethod]
        public async Task RecycleFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Guid recycleBinId = testDocument.Recycle();

                Assert.AreNotEqual(Guid.Empty, recycleBinId);

                try
                {
                    testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        [TestMethod]
        public async Task RecycleFileCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var batchRecycle = await testDocument.RecycleBatchAsync();
                Assert.IsFalse(batchRecycle.IsAvailable);
                await context.ExecuteAsync();
                Assert.IsTrue(batchRecycle.IsAvailable);
                Assert.AreNotEqual(Guid.Empty, batchRecycle.Result.Value);

                try
                {
                    testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        [TestMethod]
        public async Task RecycleFileCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                testDocument.RecycleBatch();
                await context.ExecuteAsync();

                try
                {
                    testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        [TestMethod]
        public async Task RecycleFileBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var batch = context.NewBatch();
                await testDocument.RecycleBatchAsync(batch);
                await context.ExecuteAsync(batch);

                try
                {
                    testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        [TestMethod]
        public async Task RecycleFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var batch = context.NewBatch();
                testDocument.RecycleBatch(batch);
                await context.ExecuteAsync(batch);

                try
                {
                    testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }
        #endregion

        #region CopyTo() variants
        [TestMethod]
        public async Task CopyFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            string originalFileNameWithoutExt = Path.GetFileNameWithoutExtension(documentName);
            string copyFileName = $"{originalFileNameWithoutExt}_COPY{Path.GetExtension(documentName)}";
            string destinationServerRelativeUrl = documentUrl.Replace(documentName, copyFileName);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                await testDocument.CopyToAsync(destinationServerRelativeUrl, true);

                IFile foundCopiedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundCopiedDocument);
                Assert.AreEqual(copyFileName, foundCopiedDocument.Name);
            }

            await TestAssets.CleanupTestDocumentAsync(2, fileName: documentName);
            await TestAssets.CleanupTestDocumentAsync(2, fileName: copyFileName);
        }

        [TestMethod]
        public async Task CopyFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            string originalFileNameWithoutExt = Path.GetFileNameWithoutExtension(documentName);
            string copyFileName = $"{originalFileNameWithoutExt}_COPY{Path.GetExtension(documentName)}";
            string destinationServerRelativeUrl = documentUrl.Replace(documentName, copyFileName);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                testDocument.CopyTo(destinationServerRelativeUrl, true);

                IFile foundCopiedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundCopiedDocument);
                Assert.AreEqual(copyFileName, foundCopiedDocument.Name);
            }

            await TestAssets.CleanupTestDocumentAsync(2, fileName: documentName);
            await TestAssets.CleanupTestDocumentAsync(2, fileName: copyFileName);
        }

        [TestMethod]
        public async Task CopyFileCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            string originalFileNameWithoutExt = Path.GetFileNameWithoutExtension(documentName);
            string copyFileName = $"{originalFileNameWithoutExt}_COPY{Path.GetExtension(documentName)}";
            string destinationServerRelativeUrl = documentUrl.Replace(documentName, copyFileName);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                await testDocument.CopyToBatchAsync(destinationServerRelativeUrl, true);
                await context.ExecuteAsync();

                IFile foundCopiedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundCopiedDocument);
                Assert.AreEqual(copyFileName, foundCopiedDocument.Name);
            }

            await TestAssets.CleanupTestDocumentAsync(2, fileName: documentName);
            await TestAssets.CleanupTestDocumentAsync(2, fileName: copyFileName);
        }

        [TestMethod]
        public async Task CopyFileCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            string originalFileNameWithoutExt = Path.GetFileNameWithoutExtension(documentName);
            string copyFileName = $"{originalFileNameWithoutExt}_COPY{Path.GetExtension(documentName)}";
            string destinationServerRelativeUrl = documentUrl.Replace(documentName, copyFileName);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                testDocument.CopyToBatch(destinationServerRelativeUrl, true);
                await context.ExecuteAsync();

                IFile foundCopiedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundCopiedDocument);
                Assert.AreEqual(copyFileName, foundCopiedDocument.Name);
            }

            await TestAssets.CleanupTestDocumentAsync(2, fileName: documentName);
            await TestAssets.CleanupTestDocumentAsync(2, fileName: copyFileName);
        }

        [TestMethod]
        public async Task CopyFileBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            string originalFileNameWithoutExt = Path.GetFileNameWithoutExtension(documentName);
            string copyFileName = $"{originalFileNameWithoutExt}_COPY{Path.GetExtension(documentName)}";
            string destinationServerRelativeUrl = documentUrl.Replace(documentName, copyFileName);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var batch = context.NewBatch();
                await testDocument.CopyToBatchAsync(batch, destinationServerRelativeUrl, true);
                await context.ExecuteAsync(batch);

                IFile foundCopiedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundCopiedDocument);
                Assert.AreEqual(copyFileName, foundCopiedDocument.Name);
            }

            await TestAssets.CleanupTestDocumentAsync(2, fileName: documentName);
            await TestAssets.CleanupTestDocumentAsync(2, fileName: copyFileName);
        }

        [TestMethod]
        public async Task CopyFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            string originalFileNameWithoutExt = Path.GetFileNameWithoutExtension(documentName);
            string copyFileName = $"{originalFileNameWithoutExt}_COPY{Path.GetExtension(documentName)}";
            string destinationServerRelativeUrl = documentUrl.Replace(documentName, copyFileName);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var batch = context.NewBatch();
                testDocument.CopyToBatch(batch, destinationServerRelativeUrl, true);
                await context.ExecuteAsync(batch);

                IFile foundCopiedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundCopiedDocument);
                Assert.AreEqual(copyFileName, foundCopiedDocument.Name);
            }

            await TestAssets.CleanupTestDocumentAsync(2, fileName: documentName);
            await TestAssets.CleanupTestDocumentAsync(2, fileName: copyFileName);
        }

        [TestMethod]
        public async Task CopyFileCrossSiteTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                using (var otherSiteContext = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 2))
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    string destinationServerRelativeUrl = $"{otherSiteContext.Uri.PathAndQuery}/Shared Documents/{documentName}";
                    await testDocument.CopyToAsync(destinationServerRelativeUrl, true);

                    IFile foundCopiedDocument = await otherSiteContext.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                    Assert.IsNotNull(foundCopiedDocument);
                    Assert.AreEqual(documentName, foundCopiedDocument.Name);
                }
            }

            await TestAssets.CleanupTestDocumentAsync(3);
            await TestAssets.CleanupTestDocumentAsync(3, contextConfig: TestCommon.NoGroupTestSite);
        }

        [TestMethod]
        public async Task CopyFileCrossSiteAbsoluteURLsTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                using (var otherSiteContext = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 2))
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    string destinationServerRelativeUrl = $"{otherSiteContext.Uri.PathAndQuery}/Shared Documents/{documentName}";
                    string destinationAbsoluteUrl = $"{otherSiteContext.Uri}/Shared Documents/{documentName}";
                    await testDocument.CopyToAsync(destinationAbsoluteUrl, true);

                    IFile foundCopiedDocument = await otherSiteContext.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                    Assert.IsNotNull(foundCopiedDocument);
                    Assert.AreEqual(documentName, foundCopiedDocument.Name);
                }
            }

            await TestAssets.CleanupTestDocumentAsync(3);
            await TestAssets.CleanupTestDocumentAsync(3, contextConfig: TestCommon.NoGroupTestSite);
        }

        [TestMethod]
        public async Task CopyFileCrossSiteNoOverWriteTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                using (var otherSiteContext = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 2))
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    string destinationServerRelativeUrl = $"{otherSiteContext.Uri.PathAndQuery}/Shared Documents/{documentName}";
                    
                    // Perform first copy
                    await testDocument.CopyToAsync(destinationServerRelativeUrl, options: new MoveCopyOptions { KeepBoth = true });

                    // Perform second copy, this will not overwrite the original file but add a copy with a number suffix
                    await testDocument.CopyToAsync(destinationServerRelativeUrl, options: new MoveCopyOptions { KeepBoth = true });

                    IFile foundCopiedDocument = await otherSiteContext.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                    Assert.IsNotNull(foundCopiedDocument);
                    Assert.AreEqual(documentName, foundCopiedDocument.Name);
                }
            }

            await TestAssets.CleanupTestDocumentAsync(3);
            await TestAssets.CleanupTestDocumentAsync(3, contextConfig: TestCommon.NoGroupTestSite);
            // Delete the extra file since we used keepboth twice
            await TestAssets.CleanupTestDocumentAsync(3, contextConfig: TestCommon.NoGroupTestSite, fileName: $"{Path.GetFileNameWithoutExtension(documentName)}1.docx");
        }

        [TestMethod]
        public async Task CopyFileNoOverWriteTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            string originalFileNameWithoutExt = Path.GetFileNameWithoutExtension(documentName);
            string copyFileName = $"{originalFileNameWithoutExt}_COPY{Path.GetExtension(documentName)}";
            string destinationServerRelativeUrl = documentUrl.Replace(documentName, copyFileName);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                // Perform first copy
                await testDocument.CopyToAsync(destinationServerRelativeUrl, options: new MoveCopyOptions { KeepBoth = true });

                // Perform second copy, this will not overwrite the original file but add a copy with a number suffix
                await testDocument.CopyToAsync(destinationServerRelativeUrl, options: new MoveCopyOptions { KeepBoth = true });

                IFile foundCopiedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundCopiedDocument);
                Assert.AreEqual(copyFileName, foundCopiedDocument.Name);
            }

            await TestAssets.CleanupTestDocumentAsync(2, fileName: documentName);
            await TestAssets.CleanupTestDocumentAsync(2, fileName: copyFileName);
            await TestAssets.CleanupTestDocumentAsync(2, fileName: $"{Path.GetFileNameWithoutExtension(copyFileName)}1.docx");
        }

        #endregion

        #region MoveTo() variants
        [TestMethod]
        public async Task MoveFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            string originalFileNameWithoutExt = Path.GetFileNameWithoutExtension(documentName);
            string newFileName = $"{originalFileNameWithoutExt}_MOVE{Path.GetExtension(documentName)}";
            string destinationServerRelativeUrl = documentUrl.Replace(documentName, newFileName);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                await testDocument.MoveToAsync(destinationServerRelativeUrl, MoveOperations.Overwrite);

                IFile foundMovedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundMovedDocument);
                Assert.AreEqual(newFileName, foundMovedDocument.Name);
            }

            await TestAssets.CleanupTestDocumentAsync(2, fileName: newFileName);
        }

        [TestMethod]
        public async Task MoveFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            string originalFileNameWithoutExt = Path.GetFileNameWithoutExtension(documentName);
            string newFileName = $"{originalFileNameWithoutExt}_MOVE{Path.GetExtension(documentName)}";
            string destinationServerRelativeUrl = documentUrl.Replace(documentName, newFileName);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                testDocument.MoveTo(destinationServerRelativeUrl, MoveOperations.Overwrite);

                IFile foundMovedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundMovedDocument);
                Assert.AreEqual(newFileName, foundMovedDocument.Name);
            }

            await TestAssets.CleanupTestDocumentAsync(2, fileName: newFileName);
        }

        [TestMethod]
        public async Task MoveFileCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            string originalFileNameWithoutExt = Path.GetFileNameWithoutExtension(documentName);
            string newFileName = $"{originalFileNameWithoutExt}_MOVE{Path.GetExtension(documentName)}";
            string destinationServerRelativeUrl = documentUrl.Replace(documentName, newFileName);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                await testDocument.MoveToBatchAsync(destinationServerRelativeUrl, MoveOperations.Overwrite);
                await context.ExecuteAsync();

                IFile foundMovedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundMovedDocument);
                Assert.AreEqual(newFileName, foundMovedDocument.Name);
            }

            await TestAssets.CleanupTestDocumentAsync(2, fileName: newFileName);
        }

        [TestMethod]
        public async Task MoveFileCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            string originalFileNameWithoutExt = Path.GetFileNameWithoutExtension(documentName);
            string newFileName = $"{originalFileNameWithoutExt}_MOVE{Path.GetExtension(documentName)}";
            string destinationServerRelativeUrl = documentUrl.Replace(documentName, newFileName);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                testDocument.MoveToBatch(destinationServerRelativeUrl, MoveOperations.Overwrite);
                await context.ExecuteAsync();

                IFile foundMovedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundMovedDocument);
                Assert.AreEqual(newFileName, foundMovedDocument.Name);
            }

            await TestAssets.CleanupTestDocumentAsync(2, fileName: newFileName);
        }

        [TestMethod]
        public async Task MoveFileBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            string originalFileNameWithoutExt = Path.GetFileNameWithoutExtension(documentName);
            string newFileName = $"{originalFileNameWithoutExt}_MOVE{Path.GetExtension(documentName)}";
            string destinationServerRelativeUrl = documentUrl.Replace(documentName, newFileName);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var batch = context.NewBatch();
                await testDocument.MoveToBatchAsync(batch, destinationServerRelativeUrl, MoveOperations.Overwrite);
                await context.ExecuteAsync(batch);

                IFile foundMovedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundMovedDocument);
                Assert.AreEqual(newFileName, foundMovedDocument.Name);
            }

            await TestAssets.CleanupTestDocumentAsync(2, fileName: newFileName);
        }

        [TestMethod]
        public async Task MoveFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            string originalFileNameWithoutExt = Path.GetFileNameWithoutExtension(documentName);
            string newFileName = $"{originalFileNameWithoutExt}_MOVE{Path.GetExtension(documentName)}";
            string destinationServerRelativeUrl = documentUrl.Replace(documentName, newFileName);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var batch = context.NewBatch();
                testDocument.MoveToBatch(batch, destinationServerRelativeUrl, MoveOperations.Overwrite);
                await context.ExecuteAsync(batch);

                IFile foundMovedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundMovedDocument);
                Assert.AreEqual(newFileName, foundMovedDocument.Name);
            }

            await TestAssets.CleanupTestDocumentAsync(2, fileName: newFileName);
        }

        [TestMethod]
        public async Task MoveFileCrossSiteTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                using (var otherSiteContext = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 2))
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    string destinationServerRelativeUrl = $"{otherSiteContext.Uri.PathAndQuery}/Shared Documents/{documentName}";
                    await testDocument.MoveToAsync(destinationServerRelativeUrl, MoveOperations.Overwrite);

                    IFile foundMovedDocument = await otherSiteContext.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                    Assert.IsNotNull(foundMovedDocument);
                    Assert.AreEqual(documentName, foundMovedDocument.Name);
                }
            }

            await TestAssets.CleanupTestDocumentAsync(3, contextConfig: TestCommon.NoGroupTestSite);
        }

        [TestMethod]
        public async Task MoveFileCrossSiteAbsoluteURLsTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                using (var otherSiteContext = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 2))
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    string destinationServerRelativeUrl = $"{otherSiteContext.Uri.PathAndQuery}/Shared Documents/{documentName}";
                    string destinationAbsoluteUrl = $"{context.Uri}/Shared Documents/{documentName}";
                    await testDocument.MoveToAsync(destinationServerRelativeUrl, MoveOperations.Overwrite);

                    IFile foundMovedDocument = await otherSiteContext.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                    Assert.IsNotNull(foundMovedDocument);
                    Assert.AreEqual(documentName, foundMovedDocument.Name);
                }
            }

            await TestAssets.CleanupTestDocumentAsync(3, contextConfig: TestCommon.NoGroupTestSite);
        }
        #endregion

        #region Add file
        [TestMethod]
        public async Task AddFileToFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = await context.Web.Folders.FirstOrDefaultAsync(f => f.Name == "SiteAssets");

                string fileName = TestCommon.GetPnPSdkTestAssetName("test_added.docx");
                IFile addedFile = await parentFolder.Files.AddAsync(fileName, System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"));

                // Test the created object
                Assert.IsNotNull(addedFile);
                Assert.AreEqual(fileName, addedFile.Name);
                Assert.AreNotEqual(default, addedFile.UniqueId);

                await addedFile.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddChunkedFileToFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = await context.Web.Folders.FirstOrDefaultAsync(f => f.Name == "SiteAssets");

                string fileName = TestCommon.GetPnPSdkTestAssetName("testchunked_added.docx");

                IFile addedFile = await parentFolder.Files.AddAsync(fileName, System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}testchunked.docx"));

                // Test the created object
                Assert.IsNotNull(addedFile);
                Assert.AreEqual(fileName, addedFile.Name);
                Assert.AreNotEqual(default, addedFile.UniqueId);

                await addedFile.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddFileToLibraryFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var fileName = TestCommon.GetPnPSdkTestAssetName("test_added.docx");

                IFolder folder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFile addedFile = await folder.Files.AddAsync(fileName, System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"));

                Assert.IsNotNull(addedFile);
                Assert.AreEqual(fileName, addedFile.Name);
                Assert.AreNotEqual(default, addedFile.UniqueId);

                await addedFile.DeleteAsync();
            }
        }
        #endregion

        #region Get file content
        [TestMethod]
        public async Task GetFileContentAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder folder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                string fileContent = "PnP Rocks !!!";
                var contentStream = new MemoryStream(Encoding.UTF8.GetBytes("PnP Rocks !!!"));
                string fileName = $"{nameof(GetFileContentAsyncTest)}.txt";
                IFile testFile = await folder.Files.AddAsync(fileName, contentStream);

                // Test the created object
                Assert.IsNotNull(testFile);

                // Get the file to download 
                // TODO This is needed because the API URL tokens are not resolved on file from AddAsync(), it would be nice not to need to refetch the file
                string fileUrl = $"{sharedDocumentsFolderUrl}/{fileName}";
                IFile fileToDownload = await context.Web.GetFileByServerRelativeUrlAsync(fileUrl);

                // Download the content
                Stream downloadedContentStream = await fileToDownload.GetContentAsync();
                downloadedContentStream.Seek(0, SeekOrigin.Begin);
                // Get string from the content stream
                string downloadedContent = new StreamReader(downloadedContentStream).ReadToEnd();

                Assert.AreEqual(fileContent, downloadedContent);

                await fileToDownload.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetFileContentTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder folder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                string fileContent = "PnP Rocks !!!";
                var contentStream = new MemoryStream(Encoding.UTF8.GetBytes("PnP Rocks !!!"));
                string fileName = $"{nameof(GetFileContentTest)}.txt";
                IFile testFile = await folder.Files.AddAsync(fileName, contentStream);

                // Test the created object
                Assert.IsNotNull(testFile);

                // Get the file to download 
                // TODO This is needed because the API URL tokens are not resolved on file from AddAsync(), it would be nice not to need to refetch the file
                string fileUrl = $"{sharedDocumentsFolderUrl}/{fileName}";
                IFile fileToDownload = await context.Web.GetFileByServerRelativeUrlAsync(fileUrl);

                // Download the content
                Stream downloadedContentStream = fileToDownload.GetContent();
                downloadedContentStream.Seek(0, SeekOrigin.Begin);
                // Get string from the content stream
                string downloadedContent = new StreamReader(downloadedContentStream).ReadToEnd();

                Assert.AreEqual(fileContent, downloadedContent);

                await fileToDownload.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetFileContentBytesAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder folder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                string fileContent = "PnP Rocks !!!";
                var contentStream = new MemoryStream(Encoding.UTF8.GetBytes("PnP Rocks !!!"));
                string fileName = $"{nameof(GetFileContentBytesAsyncTest)}.txt";
                IFile testFile = await folder.Files.AddAsync(fileName, contentStream);

                // Test the created object
                Assert.IsNotNull(testFile);

                // Get the file to download 
                // TODO This is needed because the API URL tokens are not resolved on file from AddAsync(), it would be nice not to need to refetch the file
                string fileUrl = $"{sharedDocumentsFolderUrl}/{fileName}";
                IFile fileToDownload = await context.Web.GetFileByServerRelativeUrlAsync(fileUrl);

                // Download the content
                byte[] downloadedContentBytes = await fileToDownload.GetContentBytesAsync();
                // Get string from the content stream
                string downloadedContent = Encoding.UTF8.GetString(downloadedContentBytes);

                Assert.AreEqual(fileContent, downloadedContent);

                await fileToDownload.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetFileContentBytesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder folder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                string fileContent = "PnP Rocks !!!";
                var contentStream = new MemoryStream(Encoding.UTF8.GetBytes("PnP Rocks !!!"));
                string fileName = $"{nameof(GetFileContentBytesTest)}.txt";
                IFile testFile = await folder.Files.AddAsync(fileName, contentStream);

                // Test the created object
                Assert.IsNotNull(testFile);

                // Get the file to download 
                // TODO This is needed because the API URL tokens are not resolved on file from AddAsync(), it would be nice not to need to refetch the file
                string fileUrl = $"{sharedDocumentsFolderUrl}/{fileName}";
                IFile fileToDownload = await context.Web.GetFileByServerRelativeUrlAsync(fileUrl);

                // Download the content
                byte[] downloadedContentBytes = fileToDownload.GetContentBytes();
                // Get string from the content stream
                string downloadedContent = Encoding.UTF8.GetString(downloadedContentBytes);

                Assert.AreEqual(fileContent, downloadedContent);

                await fileToDownload.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetFileVersionContentAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string libraryName, _, _) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var list = await context.Web.Lists.GetByTitleAsync(libraryName, l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl));

                const string fileContent1 = "PnP Rocks !!!";
                const string fileContent2 = "BlahBlahBlah???";

                var contentStream1 = new MemoryStream(Encoding.UTF8.GetBytes(fileContent1));
                var contentStream2 = new MemoryStream(Encoding.UTF8.GetBytes(fileContent2));

                string documentName = $"{nameof(GetFileVersionContentAsyncTest)}.txt";
                IFile testDocument = await list.RootFolder.Files.AddAsync(documentName, contentStream1);

                // Create 2 additional minor versions
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync();
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync();

                // Create major version
                await testDocument.CheckoutAsync();
                testDocument = await list.RootFolder.Files.AddAsync(documentName, contentStream2, true);
                await testDocument.CheckinAsync("OVERWROTE A FILE", CheckinType.MajorCheckIn);

                // Create another major version
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync("TEST COMMENT", CheckinType.MajorCheckIn);

                IFile documentWithVersions = await context.Web.GetFileByServerRelativeUrlAsync(testDocument.ServerRelativeUrl,
                    f => f.Versions,
                    f => f.CheckInComment,
                    f => f.MajorVersion,
                    f => f.MinorVersion,
                    f => f.UIVersionLabel);

                Assert.AreEqual("TEST COMMENT", documentWithVersions.CheckInComment);
                Assert.AreEqual(2, documentWithVersions.MajorVersion);
                Assert.AreEqual(0, documentWithVersions.MinorVersion);
                Assert.AreEqual("2.0", documentWithVersions.UIVersionLabel);

                Assert.IsNotNull(documentWithVersions.Versions);

                var versions = documentWithVersions.Versions.AsRequested().ToList();

                // The versions history contains 2 versions
                Assert.AreEqual(4, versions.Count);

                Assert.AreEqual($"_vti_history/1/{libraryName}/{documentName}", versions.ElementAt(0).Url);

                Assert.AreEqual("0.1", versions.ElementAt(0).VersionLabel);
                Assert.AreEqual("0.2", versions.ElementAt(1).VersionLabel);
                Assert.AreEqual("0.3", versions.ElementAt(2).VersionLabel);
                Assert.AreEqual("1.0", versions.ElementAt(3).VersionLabel);

                Assert.AreEqual("OVERWROTE A FILE", versions.ElementAt(3).CheckInComment);

                Assert.AreEqual(3, versions.ElementAt(2).Id);
                Assert.IsTrue(versions.ElementAt(2).Created != DateTime.MinValue);

                // Download document version content
                Stream downloadedContentStream = await versions.ElementAt(2).GetContentAsync();
                downloadedContentStream.Seek(0, SeekOrigin.Begin);
                // Get string from the content stream
                string downloadedContent = await new StreamReader(downloadedContentStream).ReadToEndAsync();

                Assert.IsTrue(!string.IsNullOrEmpty(downloadedContent));
                Assert.AreEqual(fileContent1, downloadedContent);
                Assert.AreNotEqual(fileContent2, downloadedContent);

                // Get the last file version
                downloadedContentStream = await versions.ElementAt(3).GetContentAsync();
                downloadedContentStream.Seek(0, SeekOrigin.Begin);
                // Get string from the content stream
                downloadedContent = await new StreamReader(downloadedContentStream).ReadToEndAsync();

                Assert.IsTrue(!string.IsNullOrEmpty(downloadedContent));
                Assert.AreEqual(fileContent2, downloadedContent);
                Assert.AreNotEqual(fileContent1, downloadedContent);

                // Download the latest from the actual file
                downloadedContentStream = await documentWithVersions.GetContentAsync();
                downloadedContentStream.Seek(0, SeekOrigin.Begin);
                // Get string from the content stream
                downloadedContent = await new StreamReader(downloadedContentStream).ReadToEndAsync();

                Assert.IsTrue(!string.IsNullOrEmpty(downloadedContent));
                Assert.AreEqual(fileContent2, downloadedContent);
                Assert.AreNotEqual(fileContent1, downloadedContent);
            }

            await TestAssets.CleanupTestDedicatedListAsync(2);
        }

        #endregion

        #region Work with very large files
        //[TestMethod]
        //public async Task AddVeryLargeFileTest()
        //{
        //    TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        IFolder parentFolder = await context.Web.Folders.GetFirstOrDefaultAsync(f => f.Name == "SiteAssets");

        //        string fileName = TestCommon.GetPnPSdkTestAssetName("2gb.test");

        //        IFile addedFile = await parentFolder.Files.AddAsync(fileName, System.IO.File.OpenRead($"d:\\temp\\largetestfiles\\2gb.test"));
        //        // Test the created object
        //        Assert.IsNotNull(addedFile);
        //        Assert.AreEqual(fileName, addedFile.Name);
        //        Assert.AreNotEqual(default, addedFile.UniqueId);

        //        //await addedFile.DeleteAsync();
        //    }
        //}

        //[TestMethod]
        //public async Task DownloadVeryLargeFileTest()
        //{
        //    TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        IFolder parentFolder = await context.Web.Folders.GetFirstOrDefaultAsync(f => f.Name == "SiteAssets");

        //        string fileName = TestCommon.GetPnPSdkTestAssetName("2gb.test");

        //        string fileUrl = $"{parentFolder.ServerRelativeUrl}/{fileName}";
        //        IFile fileToDownload = await context.Web.GetFileByServerRelativeUrlAsync(fileUrl);

        //        // Download the content
        //        Stream downloadedContentStream = await fileToDownload.GetContentAsync(true);

        //        var bufferSize = 2 * 1024 * 1024;  // 2 MB buffer
        //        using (var content = System.IO.File.Create("d:\\temp\\largetestfiles\\2gb.test.downloaded"))
        //        {
        //            var buffer = new byte[bufferSize];
        //            int read;
        //            while ((read = await downloadedContentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
        //            {
        //                content.Write(buffer, 0, read);
        //            }
        //        }
        //    }
        //}
        #endregion

        #region Add Template file

        [TestMethod]
        public async Task AddTemplateFileTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Site Pages", p => p.RootFolder)).RootFolder;

                string fileName = TestCommon.GetPnPSdkTestAssetName("page1.aspx");
                IFile addedFile = await parentFolder.Files.AddTemplateFileAsync($"{parentFolder.ServerRelativeUrl}/{fileName}", TemplateFileType.ClientSidePage);

                // Test the created object
                Assert.IsNotNull(addedFile);
                Assert.AreEqual(fileName, addedFile.Name);
                Assert.AreNotEqual(default, addedFile.UniqueId);

                await addedFile.DeleteAsync();

                fileName = TestCommon.GetPnPSdkTestAssetName("Hi'there is &ok.aspx");
                addedFile = await parentFolder.Files.AddTemplateFileAsync($"{parentFolder.ServerRelativeUrl}/{fileName}", TemplateFileType.ClientSidePage);

                // Test the created object
                Assert.IsNotNull(addedFile);
                Assert.AreEqual(fileName, addedFile.Name);
                Assert.AreNotEqual(default, addedFile.UniqueId);

                await addedFile.DeleteAsync();
            }
        }
        #endregion

        // TODO Implement the update metadata test when the list item Graph identifier can be resolved from the file object
        //[TestMethod]
        //public async Task UpdateFileMetadataTest()
        //{
        //    //TestCommon.Instance.Mocking = false;

        //    await TestAssets.CreateTestDocument(0, "test_update.docx");

        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
        //    {
        //        string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_update.docx";
        //        IFile testDocument = await context.Web.GetFileByServerRelativeUrlBatchAsync(testDocumentServerRelativeUrl);

        //        testDocument.ListItemAllFields.Values["Title"] = "test_updated.docx";
        //        await testDocument.ListItemAllFields.UpdateAsync();
        //    }

        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
        //    {
        //        string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
        //        IFile testDocument = await context.Web.GetFileByServerRelativeUrlBatchAsync(testDocumentServerRelativeUrl);

        //        Assert.AreEqual("test_updated.docx", testDocument.ListItemAllFields.Title);

        //        testDocument.ListItemAllFields.Title = "test.docx";
        //        await testDocument.ListItemAllFields.UpdateAsync();
        //    }

        //    await TestAssets.CleanupTestDocument(2, "test_update.docx");
        //}

        #region Delete
        [TestMethod]
        public async Task DeleteFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                await testDocument.DeleteAsync();

                try
                {
                    IFile foundDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        [TestMethod]
        public async Task DeleteFileTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                testDocument.Delete();

                try
                {
                    IFile foundDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        [TestMethod]
        public async Task DeleteFileCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                await testDocument.DeleteBatchAsync();
                await context.ExecuteAsync();

                try
                {
                    IFile foundDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        [TestMethod]
        public async Task DeleteFileCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                testDocument.DeleteBatch();
                await context.ExecuteAsync();

                try
                {
                    IFile foundDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        [TestMethod]
        public async Task DeleteFileBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                var batch = context.NewBatch();
                await testDocument.DeleteBatchAsync(batch);
                await context.ExecuteAsync(batch);

                try
                {
                    IFile foundDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        [TestMethod]
        public async Task DeleteFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                var batch = context.NewBatch();
                testDocument.DeleteBatch(batch);
                await context.ExecuteAsync(batch);

                try
                {
                    IFile foundDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        #endregion

        #region Get File Properties
        [TestMethod]
        public async Task GetFilePropertiesAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithProperties = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.Properties);

                Assert.IsNotNull(documentWithProperties.Properties);
                Assert.IsTrue(documentWithProperties.Properties.AsDynamic().ContentTypeId.StartsWith("0x0101"));

                Assert.AreEqual(1, (int)documentWithProperties.Properties["vti_level"]);
                Assert.AreEqual("100", documentWithProperties.Properties["vti_genericcontenttempnextbsnalloc"]);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFilePropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithProperties = context.Web.GetFileByServerRelativeUrl(documentUrl, f => f.Properties);

                Assert.IsNotNull(documentWithProperties.Properties);
                Assert.IsTrue(documentWithProperties.Properties.AsDynamic().ContentTypeId.StartsWith("0x0101"));
                Assert.AreEqual(1, (int)documentWithProperties.Properties["vti_level"]);
                Assert.AreEqual("100", documentWithProperties.Properties["vti_genericcontenttempnextbsnalloc"]);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFilePropertiesCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithProperties = await context.Web.GetFileByServerRelativeUrlBatchAsync(documentUrl, f => f.Properties);
                await context.ExecuteAsync();

                Assert.IsNotNull(documentWithProperties.Properties);
                Assert.IsTrue(documentWithProperties.Properties.AsDynamic().ContentTypeId.StartsWith("0x0101"));
                Assert.AreEqual(1, (int)documentWithProperties.Properties["vti_level"]);
                Assert.AreEqual("100", documentWithProperties.Properties["vti_genericcontenttempnextbsnalloc"]);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFilePropertiesCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithProperties = context.Web.GetFileByServerRelativeUrlBatch(documentUrl, f => f.Properties);
                await context.ExecuteAsync();

                Assert.IsNotNull(documentWithProperties.Properties);
                Assert.IsTrue(documentWithProperties.Properties.AsDynamic().ContentTypeId.StartsWith("0x0101"));
                Assert.AreEqual(1, (int)documentWithProperties.Properties["vti_level"]);
                Assert.AreEqual("100", documentWithProperties.Properties["vti_genericcontenttempnextbsnalloc"]);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFilePropertiesBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var batch = context.NewBatch();
                IFile documentWithProperties = await context.Web.GetFileByServerRelativeUrlBatchAsync(batch, documentUrl, f => f.Properties);
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(documentWithProperties.Properties);
                Assert.IsTrue(documentWithProperties.Properties.AsDynamic().ContentTypeId.StartsWith("0x0101"));
                Assert.AreEqual(1, (int)documentWithProperties.Properties["vti_level"]);
                Assert.AreEqual("100", documentWithProperties.Properties["vti_genericcontenttempnextbsnalloc"]);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFilePropertiesBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var batch = context.NewBatch();
                IFile documentWithProperties = context.Web.GetFileByServerRelativeUrlBatch(batch, documentUrl, f => f.Properties);
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(documentWithProperties.Properties);
                Assert.IsTrue(documentWithProperties.Properties.AsDynamic().ContentTypeId.StartsWith("0x0101"));
                Assert.AreEqual(1, (int)documentWithProperties.Properties["vti_level"]);
                Assert.AreEqual("100", documentWithProperties.Properties["vti_genericcontenttempnextbsnalloc"]);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        #region Set File Properties
        [TestMethod]
        public async Task SetFilePropertiesAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithProperties = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.Properties, f => f.ServerRelativeUrl);
                var propertyKey = "SetFilePropertiesAsyncTest";
                var myProperty = documentWithProperties.Properties.GetBoolean(propertyKey, false);
                if (myProperty == false)
                {
                    documentWithProperties.Properties[propertyKey] = true;
                    await documentWithProperties.Properties.UpdateAsync();
                }

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    documentWithProperties = await context2.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.Properties, f => f.ServerRelativeUrl);
                    myProperty = documentWithProperties.Properties.GetBoolean(propertyKey, false); ;
                    Assert.IsTrue(myProperty == true);

                    documentWithProperties.Properties[propertyKey] = null;
                    await documentWithProperties.Properties.UpdateAsync();
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
                {
                    documentWithProperties = await context3.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.Properties, f => f.ServerRelativeUrl);
                    myProperty = documentWithProperties.Properties.GetBoolean(propertyKey, false);
                    Assert.IsTrue(myProperty == false);
                }
            }

            await TestAssets.CleanupTestDocumentAsync(4);
        }

        [TestMethod]
        public async Task SetFilePropertiesAsyncWithoutLoadingServerRelativeUrlTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            bool errorThrown = false;
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IFile documentWithProperties = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.Properties);
                    var propertyKey = "SetFilePropertiesAsyncTest";
                    var myProperty = documentWithProperties.Properties.GetBoolean(propertyKey, false);
                    if (myProperty == false)
                    {
                        documentWithProperties.Properties[propertyKey] = true;
                        await documentWithProperties.Properties.UpdateAsync();
                    }
                }
            }
            catch (ClientException ex)
            {
                if (ex.Error.Type == ErrorType.Unsupported)
                {
                    errorThrown = true;
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(2);
            }

            Assert.IsTrue(errorThrown);
        }

        #endregion

        #endregion

        #region Get IFile Properties
        [TestMethod]
        public async Task GetIFilePropertiesAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithProperties = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl,
                    f => f.HasAlternateContentStreams,
                    f => f.Length,
                    f => f.ServerRedirectedUrl,
                    f => f.VroomDriveID,
                    f => f.VroomItemID);

                Assert.IsNotNull(documentWithProperties);

                Assert.IsTrue(documentWithProperties.UniqueId != Guid.Empty);
                Assert.IsFalse(documentWithProperties.HasAlternateContentStreams);
                Assert.IsTrue(documentWithProperties.Length > 0);
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.ServerRedirectedUrl) && documentWithProperties.ServerRedirectedUrl.Contains("_layouts/15/Doc.aspx?sourcedoc=", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.VroomDriveID));
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.VroomItemID));
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetIFilePropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithProperties = context.Web.GetFileByServerRelativeUrl(documentUrl,
                    f => f.HasAlternateContentStreams,
                    f => f.Length,
                    f => f.ServerRedirectedUrl,
                    f => f.VroomDriveID,
                    f => f.VroomItemID);

                Assert.IsNotNull(documentWithProperties);
                Assert.IsTrue(documentWithProperties.UniqueId != Guid.Empty);
                Assert.IsFalse(documentWithProperties.HasAlternateContentStreams);
                Assert.IsTrue(documentWithProperties.Length > 0);
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.ServerRedirectedUrl) && documentWithProperties.ServerRedirectedUrl.Contains("_layouts/15/Doc.aspx?sourcedoc=", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.VroomDriveID));
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.VroomItemID));
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetIFilePropertiesCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithProperties = await context.Web.GetFileByServerRelativeUrlBatchAsync(documentUrl,
                    f => f.HasAlternateContentStreams,
                    f => f.Length,
                    f => f.ServerRedirectedUrl,
                    f => f.VroomDriveID,
                    f => f.VroomItemID);
                await context.ExecuteAsync();

                Assert.IsNotNull(documentWithProperties);
                Assert.IsTrue(documentWithProperties.UniqueId != Guid.Empty);
                Assert.IsFalse(documentWithProperties.HasAlternateContentStreams);
                Assert.IsTrue(documentWithProperties.Length > 0);
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.ServerRedirectedUrl) && documentWithProperties.ServerRedirectedUrl.Contains("_layouts/15/Doc.aspx?sourcedoc=", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.VroomDriveID));
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.VroomItemID));
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetIFilePropertiesCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithProperties = context.Web.GetFileByServerRelativeUrlBatch(documentUrl,
                    f => f.HasAlternateContentStreams,
                    f => f.Length,
                    f => f.ServerRedirectedUrl,
                    f => f.VroomDriveID,
                    f => f.VroomItemID);
                await context.ExecuteAsync();

                Assert.IsNotNull(documentWithProperties);
                Assert.IsTrue(documentWithProperties.UniqueId != Guid.Empty);
                Assert.IsFalse(documentWithProperties.HasAlternateContentStreams);
                Assert.IsTrue(documentWithProperties.Length > 0);
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.ServerRedirectedUrl) && documentWithProperties.ServerRedirectedUrl.Contains("_layouts/15/Doc.aspx?sourcedoc=", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.VroomDriveID));
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.VroomItemID));
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetIFilePropertiesBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var batch = context.NewBatch();
                IFile documentWithProperties = await context.Web.GetFileByServerRelativeUrlBatchAsync(batch, documentUrl,
                    f => f.HasAlternateContentStreams,
                    f => f.Length,
                    f => f.ServerRedirectedUrl,
                    f => f.VroomDriveID,
                    f => f.VroomItemID);
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(documentWithProperties);
                Assert.IsTrue(documentWithProperties.UniqueId != Guid.Empty);
                Assert.IsFalse(documentWithProperties.HasAlternateContentStreams);
                Assert.IsTrue(documentWithProperties.Length > 0);
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.ServerRedirectedUrl) && documentWithProperties.ServerRedirectedUrl.Contains("_layouts/15/Doc.aspx?sourcedoc=", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.VroomDriveID));
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.VroomItemID));
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetIFilePropertiesBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var batch = context.NewBatch();
                IFile documentWithProperties = context.Web.GetFileByServerRelativeUrlBatch(batch, documentUrl,
                    f => f.HasAlternateContentStreams,
                    f => f.Length,
                    f => f.ServerRedirectedUrl,
                    f => f.VroomDriveID,
                    f => f.VroomItemID);
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(documentWithProperties);
                Assert.IsTrue(documentWithProperties.UniqueId != Guid.Empty);
                Assert.IsFalse(documentWithProperties.HasAlternateContentStreams);
                Assert.IsTrue(documentWithProperties.Length > 0);
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.ServerRedirectedUrl) && documentWithProperties.ServerRedirectedUrl.Contains("_layouts/15/Doc.aspx?sourcedoc=", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.VroomDriveID));
                Assert.IsTrue(!string.IsNullOrEmpty(documentWithProperties.VroomItemID));
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }
        #endregion

        #region Get File IRM
        [TestMethod]
        public async Task GetFileIRMSettingsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithIrm = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.InformationRightsManagementSettings);

                // TODO The asserts below checks the IRM settings are properly returned. More advanced tests should be done on a IRM enabled and configured tenant
                Assert.IsNotNull(documentWithIrm.InformationRightsManagementSettings);
                Assert.IsFalse(documentWithIrm.InformationRightsManagementSettings.IrmEnabled);
                Assert.AreEqual(90, documentWithIrm.InformationRightsManagementSettings.DocumentAccessExpireDays);
                Assert.AreEqual(30, documentWithIrm.InformationRightsManagementSettings.LicenseCacheExpireDays);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFileIRMSettingsTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithIrm = context.Web.GetFileByServerRelativeUrl(documentUrl, f => f.InformationRightsManagementSettings);

                // TODO The asserts below checks the IRM settings are properly returned. More advanced tests should be done on a IRM enabled and configured tenant
                Assert.IsNotNull(documentWithIrm.InformationRightsManagementSettings);
                Assert.IsFalse(documentWithIrm.InformationRightsManagementSettings.IrmEnabled);
                Assert.AreEqual(90, documentWithIrm.InformationRightsManagementSettings.DocumentAccessExpireDays);
                Assert.AreEqual(30, documentWithIrm.InformationRightsManagementSettings.LicenseCacheExpireDays);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFileIRMSettingsCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithIrm = await context.Web.GetFileByServerRelativeUrlBatchAsync(documentUrl, f => f.InformationRightsManagementSettings);
                await context.ExecuteAsync();

                // TODO The asserts below checks the IRM settings are properly returned. More advanced tests should be done on a IRM enabled and configured tenant
                Assert.IsNotNull(documentWithIrm.InformationRightsManagementSettings);
                Assert.IsFalse(documentWithIrm.InformationRightsManagementSettings.IrmEnabled);
                Assert.AreEqual(90, documentWithIrm.InformationRightsManagementSettings.DocumentAccessExpireDays);
                Assert.AreEqual(30, documentWithIrm.InformationRightsManagementSettings.LicenseCacheExpireDays);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFileIRMSettingsCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithIrm = context.Web.GetFileByServerRelativeUrlBatch(documentUrl, f => f.InformationRightsManagementSettings);
                await context.ExecuteAsync();

                // TODO The asserts below checks the IRM settings are properly returned. More advanced tests should be done on a IRM enabled and configured tenant
                Assert.IsNotNull(documentWithIrm.InformationRightsManagementSettings);
                Assert.IsFalse(documentWithIrm.InformationRightsManagementSettings.IrmEnabled);
                Assert.AreEqual(90, documentWithIrm.InformationRightsManagementSettings.DocumentAccessExpireDays);
                Assert.AreEqual(30, documentWithIrm.InformationRightsManagementSettings.LicenseCacheExpireDays);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFileIRMSettingsBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var batch = context.NewBatch();
                IFile documentWithIrm = await context.Web.GetFileByServerRelativeUrlBatchAsync(batch, documentUrl, f => f.InformationRightsManagementSettings);
                await context.ExecuteAsync(batch);

                // TODO The asserts below checks the IRM settings are properly returned. More advanced tests should be done on a IRM enabled and configured tenant
                Assert.IsNotNull(documentWithIrm.InformationRightsManagementSettings);
                Assert.IsFalse(documentWithIrm.InformationRightsManagementSettings.IrmEnabled);
                Assert.AreEqual(90, documentWithIrm.InformationRightsManagementSettings.DocumentAccessExpireDays);
                Assert.AreEqual(30, documentWithIrm.InformationRightsManagementSettings.LicenseCacheExpireDays);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFileIRMSettingsBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var batch = context.NewBatch();
                IFile documentWithIrm = context.Web.GetFileByServerRelativeUrlBatch(batch, documentUrl, f => f.InformationRightsManagementSettings);
                await context.ExecuteAsync(batch);

                // TODO The asserts below checks the IRM settings are properly returned. More advanced tests should be done on a IRM enabled and configured tenant
                Assert.IsNotNull(documentWithIrm.InformationRightsManagementSettings);
                Assert.IsFalse(documentWithIrm.InformationRightsManagementSettings.IrmEnabled);
                Assert.AreEqual(90, documentWithIrm.InformationRightsManagementSettings.DocumentAccessExpireDays);
                Assert.AreEqual(30, documentWithIrm.InformationRightsManagementSettings.LicenseCacheExpireDays);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFileEffectiveIRMSettingsTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // Enable IRM on the library
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.IrmEnabled, p => p.InformationRightsManagementSettings);
                list.IrmEnabled = true;
                await list.UpdateAsync();

                IFile documentWithEffectiveIrm = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.EffectiveInformationRightsManagementSettings);

                // TODO The asserts below checks the effective IRM settings object is instantiated. More relevant tests should be done on a IRM enabled and configured tenant
                Assert.IsNotNull(documentWithEffectiveIrm.EffectiveInformationRightsManagementSettings);

                // turn off IRM again
                list.IrmEnabled = false;
                await list.UpdateAsync();

            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }
        #endregion

        #region Get file versions
        [TestMethod]
        public async Task GetFileVersionsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string libraryName, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                // Create 2 minor versions
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync();
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync();

                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync("TEST COMMENT", CheckinType.MajorCheckIn);

                IFile documentWithVersions = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.Versions);

                Assert.IsNotNull(documentWithVersions.Versions);

                var versions = documentWithVersions.Versions.AsRequested().ToList();

                // The versions history contains 2 versions
                Assert.AreEqual(3, versions.Count);
                Assert.AreEqual($"_vti_history/1/{libraryName}/{documentName}", versions.ElementAt(0).Url);
                Assert.AreEqual("0.1", versions.ElementAt(0).VersionLabel);
                Assert.AreEqual("0.2", versions.ElementAt(1).VersionLabel);

                Assert.AreEqual(3, versions.ElementAt(2).Id);
                Assert.IsTrue(versions.ElementAt(2).Created != DateTime.MinValue);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task GetFileVersionsTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string libraryName, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                // Create 2 minor versions
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync();
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync();

                IFile documentWithVersions = context.Web.GetFileByServerRelativeUrl(documentUrl, f => f.Versions);

                Assert.IsNotNull(documentWithVersions.Versions);

                var versions = documentWithVersions.Versions.AsRequested().ToList();

                // The versions history contains 2 versions
                Assert.AreEqual(2, versions.Count);
                Assert.AreEqual($"_vti_history/1/{libraryName}/{documentName}", versions.ElementAt(0).Url);
                Assert.AreEqual("0.1", versions.ElementAt(0).VersionLabel);
                Assert.AreEqual("0.2", versions.ElementAt(1).VersionLabel);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task GetFileVersionsCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string libraryName, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                // Create 2 minor versions
                await testDocument.CheckoutBatchAsync();
                await testDocument.CheckinBatchAsync();
                await testDocument.CheckoutBatchAsync();
                await testDocument.CheckinBatchAsync();
                await context.ExecuteAsync();

                IFile documentWithVersions = await context.Web.GetFileByServerRelativeUrlBatchAsync(documentUrl, f => f.Versions);
                await context.ExecuteAsync();

                Assert.IsNotNull(documentWithVersions.Versions);

                var versions = documentWithVersions.Versions.AsRequested().ToList();

                // The versions history contains 2 versions
                Assert.AreEqual(2, versions.Count);
                Assert.AreEqual($"_vti_history/1/{libraryName}/{documentName}", versions.ElementAt(0).Url);
                Assert.AreEqual("0.1", versions.ElementAt(0).VersionLabel);
                Assert.AreEqual("0.2", versions.ElementAt(1).VersionLabel);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task GetFileVersionsCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string libraryName, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                // Create 2 minor versions
                testDocument.CheckoutBatch();
                testDocument.CheckinBatch();
                testDocument.CheckoutBatch();
                testDocument.CheckinBatch();
                await context.ExecuteAsync();

                IFile documentWithVersions = context.Web.GetFileByServerRelativeUrlBatch(documentUrl, f => f.Versions);
                await context.ExecuteAsync();

                Assert.IsNotNull(documentWithVersions.Versions);

                var versions = documentWithVersions.Versions.AsRequested().ToList();

                // The versions history contains 2 versions
                Assert.AreEqual(2, versions.Count);
                Assert.AreEqual($"_vti_history/1/{libraryName}/{documentName}", versions.ElementAt(0).Url);
                Assert.AreEqual("0.1", versions.ElementAt(0).VersionLabel);
                Assert.AreEqual("0.2", versions.ElementAt(1).VersionLabel);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task GetFileVersionsBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string libraryName, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var newBatch = context.NewBatch();

                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                // Create 2 minor versions
                await testDocument.CheckoutBatchAsync(newBatch);
                await testDocument.CheckinBatchAsync(newBatch);
                await testDocument.CheckoutBatchAsync(newBatch);
                await testDocument.CheckinBatchAsync(newBatch);

                await context.ExecuteAsync(newBatch);

                var batch = context.NewBatch();
                IFile documentWithVersions = await context.Web.GetFileByServerRelativeUrlBatchAsync(batch, documentUrl, f => f.Versions);
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(documentWithVersions.Versions);

                var versions = documentWithVersions.Versions.AsRequested().ToList();

                // The versions history contains 2 versions
                Assert.AreEqual(2, versions.Count);
                Assert.AreEqual($"_vti_history/1/{libraryName}/{documentName}", versions.ElementAt(0).Url);
                Assert.AreEqual("0.1", versions.ElementAt(0).VersionLabel);
                Assert.AreEqual("0.2", versions.ElementAt(1).VersionLabel);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        [TestMethod]
        public async Task GetFileVersionsBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string libraryName, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var newBatch = context.NewBatch();

                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                // Create 2 minor versions
                testDocument.CheckoutBatch(newBatch);
                testDocument.CheckinBatch(newBatch);
                testDocument.CheckoutBatch(newBatch);
                testDocument.CheckinBatch(newBatch);
                await context.ExecuteAsync(newBatch);

                var batch = context.NewBatch();
                IFile documentWithVersions = context.Web.GetFileByServerRelativeUrlBatch(batch, documentUrl, f => f.Versions);
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(documentWithVersions.Versions);

                var versions = documentWithVersions.Versions.AsRequested().ToList();

                // The versions history contains 2 versions
                Assert.AreEqual(2, versions.Count);
                Assert.AreEqual($"_vti_history/1/{libraryName}/{documentName}", versions.ElementAt(0).Url);
                Assert.AreEqual("0.1", versions.ElementAt(0).VersionLabel);
                Assert.AreEqual("0.2", versions.ElementAt(1).VersionLabel);
            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }

        #endregion

        #region File <-> ListItem handling
        [TestMethod]
        public async Task GetFileByServerRelativeUrlListItemUpdatesTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, p => p.Name, p => p.ServerRelativeUrl, p => p.ListItemAllFields);

                Assert.IsNotNull(testDocument);
                Assert.AreEqual(documentName, testDocument.Name);
                Assert.AreEqual(documentUrl, testDocument.ServerRelativeUrl);

                // Ensure list item properties, since the ListItem model's parent is a File this is a special case
                await testDocument.ListItemAllFields.EnsurePropertiesAsync(p => p.Title, p => p.Id);
                Assert.IsTrue(testDocument.ListItemAllFields.IsPropertyAvailable(p => p.Id));

                // Update title
                testDocument.ListItemAllFields.Title = "NewTitle";
                await testDocument.ListItemAllFields.UpdateAsync();


                await testDocument.LoadAsync(p => p.ListId);

                Assert.IsTrue(testDocument.ListId != Guid.Empty);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        #endregion

        #region TESTS TO REVIEW - Get file versionevents
        [TestMethod]
        public async Task GetFileVersionEventsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string libraryName, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true, parentLibraryEnableMinorVersions: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                // Create 2 minor versions
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync();
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync();

                IFile documentWithVersionEvents = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.VersionEvents);

                Assert.IsNotNull(documentWithVersionEvents.VersionEvents);
                // TODO The VersionEvents property return an empty array, double-check what it's supposed to return
                Assert.AreEqual(0, documentWithVersionEvents.VersionEvents.Length);

            }

            await TestAssets.CleanupTestDedicatedListAsync(3);
        }
        #endregion

        #region testing asset documents/libraries helpers

        private async Task<Tuple<string, string, string>> AddMockDocumentToMinorVersioningEnabledLibrary(int contextId,
            [System.Runtime.CompilerServices.CallerMemberName] string libraryName = null,
            [System.Runtime.CompilerServices.CallerMemberName] string fileName = null,
            [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            if (!fileName.EndsWith(".docx"))
            {
                fileName += ".docx";
            }

            fileName = TestCommon.GetPnPSdkTestAssetName(fileName);
            libraryName = TestCommon.GetPnPSdkTestAssetName(libraryName);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, contextId, testName))
            {
                IList documentLibrary = await context.Web.Lists.AddAsync(libraryName, ListTemplateType.DocumentLibrary);
                documentLibrary.EnableVersioning = true;
                documentLibrary.EnableMinorVersions = true;
                await documentLibrary.UpdateAsync();
                IFolder folder = await documentLibrary.RootFolder.GetAsync();
                IFile mockDocument = await folder.Files.AddAsync(fileName, System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"));
                return new Tuple<string, string, string>(libraryName, mockDocument.Name, mockDocument.ServerRelativeUrl);
            }
        }

        #endregion

        #region Convert tests

        [TestMethod]
        public async Task ConvertFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);
            IFile pdfFile = null;

            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    IFolder folder = await context.Web.GetFolderByServerRelativeUrlAsync(documentUrl.Replace($"/{documentName}", string.Empty));

                    var pdfContent = await testDocument.ConvertToAsync(new ConvertToOptions { Format = ConvertToFormat.Pdf });

                    Assert.IsNotNull(pdfContent);

                    var targetFileName = documentName.Replace(".docx", ".pdf");

                    await folder.Files.AddAsync(targetFileName, pdfContent, true);

                    pdfFile = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl.Replace(".docx", ".pdf"));

                    Assert.IsNotNull(pdfFile);
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(2, fileName: documentName);
                if (pdfFile != null)
                {
                    await pdfFile.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task ConvertImageFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            IFile jpgFile = null;
            IFile testDocument = null;

            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    // Upload image file
                    string documentName = TestCommon.GetPnPSdkTestAssetName("ConvertImageFileAsyncTest.png");
                    var parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                    testDocument = await parentFolder.Files.AddAsync(documentName, System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}parker-ms-300.png"), true);
                    string documentUrl = testDocument.ServerRelativeUrl;

                    IFolder folder = await context.Web.GetFolderByServerRelativeUrlAsync(documentUrl.Replace($"/{documentName}", string.Empty));

                    var jpgContent = await testDocument.ConvertToAsync(new ConvertToOptions { Format = ConvertToFormat.Jpg, JpgFormatHeight = 100, JpgFormatWidth = 100 });

                    Assert.IsNotNull(jpgContent);

                    var targetFileName = documentName.Replace(".png", ".jpg");

                    await folder.Files.AddAsync(targetFileName, jpgContent, true);

                    jpgFile = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl.Replace(".png", ".jpg"));

                    Assert.IsNotNull(jpgFile);
                }
            }
            finally
            {
                if (testDocument != null)
                {
                    await testDocument.DeleteAsync();
                }
                
                if (jpgFile != null)
                {
                    await jpgFile.DeleteAsync();
                }
            }
        }

        [ExpectedException(typeof(ClientException))]
        [TestMethod]
        public async Task ConvertFileAsyncExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            IFile testDocument = null;
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    // Upload image file
                    string documentName = TestCommon.GetPnPSdkTestAssetName("ConvertImageFileAsyncTest.png");
                    var parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                    testDocument = await parentFolder.Files.AddAsync(documentName, System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}parker-ms-300.png"), true);
                    string documentUrl = testDocument.ServerRelativeUrl;

                    IFolder folder = await context.Web.GetFolderByServerRelativeUrlAsync(documentUrl.Replace($"/{documentName}", string.Empty));

                    // Try convert image to PDF...not supported
                    var jpgContent = await testDocument.ConvertToAsync(new ConvertToOptions { Format = ConvertToFormat.Pdf });
                }
            }
            finally
            {
                if (testDocument != null)
                {
                    await testDocument.DeleteAsync();
                }
            }
        }

        #endregion

        #region Preview

        [TestMethod]
        public async Task GetFilePreviewAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            string documentUrl = null;
            try
            {
                (_, _, documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IFile file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    Assert.IsNotNull(file);

                    var filePreview = await file.GetPreviewAsync();

                    Assert.IsNotNull(filePreview);
                    Assert.IsNotNull(filePreview.GetUrl);
                }
            }
            finally
            {
                if (documentUrl != null)
                {
                    await TestAssets.CleanupTestDocumentAsync(2);
                }
            }
        }

        [TestMethod]
        public async Task GetFilePreviewIncludingPageAndZoomAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            string documentUrl = null;
            try
            {
                (_, _, documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IFile file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    Assert.IsNotNull(file);

                    var filePreview = await file.GetPreviewAsync(new PreviewOptions { Page = "2", Zoom = 5 });

                    Assert.IsNotNull(filePreview);
                    Assert.IsNotNull(filePreview.GetUrl);
                }

            }
            finally
            {
                if (documentUrl != null)
                {
                    await TestAssets.CleanupTestDocumentAsync(2);
                }
            }
        }

        #endregion
    }
}
