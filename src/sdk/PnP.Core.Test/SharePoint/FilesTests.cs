using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System.Threading.Tasks;
using PnP.Core.QueryModel;
using System;
using System.IO;
using System.Linq;

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
            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2);
        }
        #endregion

        #region GetFileByServerRelativeUrl()
        [TestMethod]
        public async Task GetFileByServerRelativeUrlAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.IsNotNull(testDocument);
                Assert.AreEqual(documentName, testDocument.Name);
                Assert.AreEqual(documentUrl, testDocument.ServerRelativeUrl);
            }

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFileByServerRelativeUrlTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = context.Web.GetFileByServerRelativeUrl(documentUrl);

                Assert.IsNotNull(testDocument);
                Assert.AreEqual(documentName, testDocument.Name);
                Assert.AreEqual(documentUrl, testDocument.ServerRelativeUrl);
            }

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFileByServerRelativeUrlCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlBatchAsync(documentUrl);
                await context.ExecuteAsync();

                Assert.IsNotNull(testDocument);
                Assert.AreEqual(documentName, testDocument.Name);
                Assert.AreEqual(documentUrl, testDocument.ServerRelativeUrl);
            }

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFileByServerRelativeUrlCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = context.Web.GetFileByServerRelativeUrlBatch(documentUrl);
                await context.ExecuteAsync();

                Assert.IsNotNull(testDocument);
                Assert.AreEqual(documentName, testDocument.Name);
                Assert.AreEqual(documentUrl, testDocument.ServerRelativeUrl);
            }

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFileByServerRelativeUrlBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var batch = context.NewBatch();
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlBatchAsync(batch, documentUrl);
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(testDocument);
                Assert.AreEqual(documentName, testDocument.Name);
                Assert.AreEqual(documentUrl, testDocument.ServerRelativeUrl);
            }

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFileByServerRelativeUrlBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var batch = context.NewBatch();
                IFile testDocument = context.Web.GetFileByServerRelativeUrlBatch(batch, documentUrl);
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(testDocument);
                Assert.AreEqual(documentName, testDocument.Name);
                Assert.AreEqual(documentUrl, testDocument.ServerRelativeUrl);
            }

            await CleanupMockDocumentFromSharedDocuments(2);
        }
        #endregion

        #region Publish() variants
        // TODO: Review to cover 6 variants of each File methods with this set of tests as example
        [TestMethod]
        public async Task PublishFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishAsync("TEST PUBLISH");
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsTrue(!string.IsNullOrEmpty(testDocument.ContentTag));
                Assert.AreEqual(CustomizedPageStatus.None, testDocument.CustomizedPageStatus);
                Assert.IsTrue(!string.IsNullOrEmpty(testDocument.ETag));
                Assert.IsTrue(testDocument.Exists);
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

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task PublishFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                testDocument.Publish("TEST PUBLISH");
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task PublishFileCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishBatchAsync("TEST PUBLISH");
                await context.ExecuteAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task PublishFileCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                testDocument.PublishBatch("TEST PUBLISH");
                await context.ExecuteAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task PublishFileBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

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
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task PublishFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

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
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }
        #endregion

        #region Unpublish() variants
        [TestMethod]
        public async Task UnpublishFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishAsync("TEST PUBLISH");
                await testDocument.UnpublishAsync("TEST UNPUBLISHED");
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task UnpublishFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishAsync("TEST PUBLISH");
                testDocument.Unpublish("TEST UNPUBLISHED");
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task UnpublishFileCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

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
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task UnpublishFileCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

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
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task UnpublishFileBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

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
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task UnpublishFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

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
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }
        #endregion

        #region Checkout() variants
        [TestMethod]
        public async Task CheckoutFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                await testDocument.CheckoutAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task CheckoutFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                testDocument.Checkout();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task CheckoutFileCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                await testDocument.CheckoutBatchAsync();
                await context.ExecuteAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task CheckoutFileCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                testDocument.CheckoutBatch();
                await context.ExecuteAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task CheckoutFileBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                var batch = context.NewBatch();
                await testDocument.CheckoutBatchAsync(batch);
                await context.ExecuteAsync(batch);
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task CheckoutFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                var batch = context.NewBatch();
                testDocument.CheckoutBatch(batch);
                await context.ExecuteAsync(batch);
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }
        #endregion

        #region UndoCheckout() variants
        [TestMethod]
        public async Task UndoCheckoutFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.UndoCheckoutAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task UndoCheckoutFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                testDocument.Checkout();
                testDocument.UndoCheckout();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task UndoCheckoutFileCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

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
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task UndoCheckoutFileCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

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
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task UndoCheckoutFileBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

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
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task UndoCheckoutFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

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
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }
        #endregion

        #region TESTS TO REVIEW - Checkin() variants
        [TestMethod]
        public async Task CheckinFileMajorVersionTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync("TEST CHECK IN", CheckinType.MajorCheckIn);
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task CheckinFileMinorVersionTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync("TEST CHECK IN", CheckinType.MinorCheckIn);
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.IsTrue(testDocument.MinorVersion == initialMinorVersion + 1);

            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task CheckinFileOverwriteVersionTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync("TEST CHECK IN", CheckinType.OverwriteCheckIn);
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task CheckinFileWithBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

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
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);
                Assert.IsTrue(testDocument.MajorVersion > initialMajorVersion);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }
        #endregion

        #region Recycle() variants
        [TestMethod]
        public async Task RecycleFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Guid recycleBinId = await testDocument.RecycleAsync();

                Assert.AreNotEqual(Guid.Empty, recycleBinId);
            }

            // Use a second context to make sure the file is reloaded from SharePoint
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                try
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
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

            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                Guid recycleBinId = testDocument.Recycle();

                Assert.AreNotEqual(Guid.Empty, recycleBinId);
            }

            // Use a second context to make sure the file is reloaded from SharePoint
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                try
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
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

            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                await testDocument.RecycleBatchAsync();
                await context.ExecuteAsync();
            }

            // Use a second context to make sure the file is reloaded from SharePoint
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                try
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
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

            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                testDocument.RecycleBatch();
                await context.ExecuteAsync();
            }

            // Use a second context to make sure the file is reloaded from SharePoint
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                try
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
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

            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var batch = context.NewBatch();
                await testDocument.RecycleBatchAsync(batch);
                await context.ExecuteAsync(batch);
            }

            // Use a second context to make sure the file is reloaded from SharePoint
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                try
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
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

            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var batch = context.NewBatch();
                testDocument.RecycleBatch(batch);
                await context.ExecuteAsync(batch);
            }

            // Use a second context to make sure the file is reloaded from SharePoint
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                try
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
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

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2, documentName);
            await CleanupMockDocumentFromSharedDocuments(2, copyFileName);
        }

        [TestMethod]
        public async Task CopyFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2, documentName);
            await CleanupMockDocumentFromSharedDocuments(2, copyFileName);
        }

        [TestMethod]
        public async Task CopyFileCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2, documentName);
            await CleanupMockDocumentFromSharedDocuments(2, copyFileName);
        }

        [TestMethod]
        public async Task CopyFileCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2, documentName);
            await CleanupMockDocumentFromSharedDocuments(2, copyFileName);
        }

        [TestMethod]
        public async Task CopyFileBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2, documentName);
            await CleanupMockDocumentFromSharedDocuments(2, copyFileName);
        }

        [TestMethod]
        public async Task CopyFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2, documentName);
            await CleanupMockDocumentFromSharedDocuments(2, copyFileName);
        }

        [TestMethod]
        public async Task CopyFileCrossSiteTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(3);
            await CleanupMockDocumentFromSharedDocuments(3, contextConfig: TestCommon.NoGroupTestSite);
        }

        [TestMethod]
        public async Task CopyFileCrossSiteAbsoluteURLsTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(3);
            await CleanupMockDocumentFromSharedDocuments(3, contextConfig: TestCommon.NoGroupTestSite);
        }

        #endregion

        #region MoveTo() variants
        [TestMethod]
        public async Task MoveFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2, newFileName);
        }

        [TestMethod]
        public async Task MoveFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2, newFileName);
        }

        [TestMethod]
        public async Task MoveFileCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2, newFileName);
        }

        [TestMethod]
        public async Task MoveFileCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2, newFileName);
        }

        [TestMethod]
        public async Task MoveFileBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2, newFileName);
        }

        [TestMethod]
        public async Task MoveFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2, newFileName);
        }

        [TestMethod]
        public async Task MoveFileCrossSiteTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(3, contextConfig: TestCommon.NoGroupTestSite);
        }

        [TestMethod]
        public async Task MoveFileCrossSiteAbsoluteURLsTest()
        {
            //TestCommon.Instance.Mocking = false;

            (string documentName, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(3, contextConfig: TestCommon.NoGroupTestSite);
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

                IFile addedFile = await parentFolder.Files.AddAsync("test_added.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"));

                // Test the created object
                Assert.IsNotNull(addedFile);
                Assert.AreEqual("test_added.docx", addedFile.Name);
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

                IFile addedFile = await parentFolder.Files.AddAsync("testchunked_added.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}testchunked.docx"));

                // Test the created object
                Assert.IsNotNull(addedFile);
                Assert.AreEqual("testchunked_added.docx", addedFile.Name);
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
                IFolder folder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFile addedFile = await folder.Files.AddAsync("test_added.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"));

                Assert.IsNotNull(addedFile);
                Assert.AreEqual("test_added.docx", addedFile.Name);
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

        //    await AddMockDocumentToSharedDocuments(0, "test_update.docx");

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

        //    await CleanupMockDocumentFromSharedDocuments(2, "test_update.docx");
        //}

        #region Delete
        [TestMethod]
        public async Task DeleteFileAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithProperties = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.Properties);

                Assert.IsNotNull(documentWithProperties.Properties);
                Assert.IsTrue(documentWithProperties.Properties.AsDynamic().ContentTypeId.StartsWith("0x0101"));
                Assert.AreEqual(1, (int)documentWithProperties.Properties["vti_x005f_level"]);
                Assert.AreEqual("100", documentWithProperties.Properties["vti_x005f_genericcontenttempnextbsnalloc"]);
            }

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFilePropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithProperties = context.Web.GetFileByServerRelativeUrl(documentUrl, f => f.Properties);

                Assert.IsNotNull(documentWithProperties.Properties);
                Assert.IsTrue(documentWithProperties.Properties.AsDynamic().ContentTypeId.StartsWith("0x0101"));
                Assert.AreEqual(1, (int)documentWithProperties.Properties["vti_x005f_level"]);
                Assert.AreEqual("100", documentWithProperties.Properties["vti_x005f_genericcontenttempnextbsnalloc"]);
            }

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFilePropertiesCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithProperties = await context.Web.GetFileByServerRelativeUrlBatchAsync(documentUrl, f => f.Properties);
                await context.ExecuteAsync();

                Assert.IsNotNull(documentWithProperties.Properties);
                Assert.IsTrue(documentWithProperties.Properties.AsDynamic().ContentTypeId.StartsWith("0x0101"));
                Assert.AreEqual(1, (int)documentWithProperties.Properties["vti_x005f_level"]);
                Assert.AreEqual("100", documentWithProperties.Properties["vti_x005f_genericcontenttempnextbsnalloc"]);
            }

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFilePropertiesCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithProperties = context.Web.GetFileByServerRelativeUrlBatch(documentUrl, f => f.Properties);
                await context.ExecuteAsync();

                Assert.IsNotNull(documentWithProperties.Properties);
                Assert.IsTrue(documentWithProperties.Properties.AsDynamic().ContentTypeId.StartsWith("0x0101"));
                Assert.AreEqual(1, (int)documentWithProperties.Properties["vti_x005f_level"]);
                Assert.AreEqual("100", documentWithProperties.Properties["vti_x005f_genericcontenttempnextbsnalloc"]);
            }

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFilePropertiesBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var batch = context.NewBatch();
                IFile documentWithProperties = await context.Web.GetFileByServerRelativeUrlBatchAsync(batch, documentUrl, f => f.Properties);
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(documentWithProperties.Properties);
                Assert.IsTrue(documentWithProperties.Properties.AsDynamic().ContentTypeId.StartsWith("0x0101"));
                Assert.AreEqual(1, (int)documentWithProperties.Properties["vti_x005f_level"]);
                Assert.AreEqual("100", documentWithProperties.Properties["vti_x005f_genericcontenttempnextbsnalloc"]);
            }

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFilePropertiesBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var batch = context.NewBatch();
                IFile documentWithProperties = context.Web.GetFileByServerRelativeUrlBatch(batch, documentUrl, f => f.Properties);
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(documentWithProperties.Properties);
                Assert.IsTrue(documentWithProperties.Properties.AsDynamic().ContentTypeId.StartsWith("0x0101"));
                Assert.AreEqual(1, (int)documentWithProperties.Properties["vti_x005f_level"]);
                Assert.AreEqual("100", documentWithProperties.Properties["vti_x005f_genericcontenttempnextbsnalloc"]);
            }

            await CleanupMockDocumentFromSharedDocuments(2);
        }
        #endregion

        #region TODO: TESTS TO REVIEW WITH IRM ENABLED TENANT -  Get File IRM
        [TestMethod]
        public async Task GetFileIRMSettingsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithIrm = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.InformationRightsManagementSettings);

                // TODO The asserts below checks the IRM settings are properly returned. More advanced tests should be done on a IRM enabled and configured tenant
                Assert.IsNotNull(documentWithIrm.InformationRightsManagementSettings);
                Assert.IsFalse(documentWithIrm.InformationRightsManagementSettings.IrmEnabled);
                Assert.AreEqual(90, documentWithIrm.InformationRightsManagementSettings.DocumentAccessExpireDays);
                Assert.AreEqual(30, documentWithIrm.InformationRightsManagementSettings.LicenseCacheExpireDays);
            }

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFileIRMSettingsTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile documentWithIrm = context.Web.GetFileByServerRelativeUrl(documentUrl, f => f.InformationRightsManagementSettings);

                // TODO The asserts below checks the IRM settings are properly returned. More advanced tests should be done on a IRM enabled and configured tenant
                Assert.IsNotNull(documentWithIrm.InformationRightsManagementSettings);
                Assert.IsFalse(documentWithIrm.InformationRightsManagementSettings.IrmEnabled);
                Assert.AreEqual(90, documentWithIrm.InformationRightsManagementSettings.DocumentAccessExpireDays);
                Assert.AreEqual(30, documentWithIrm.InformationRightsManagementSettings.LicenseCacheExpireDays);
            }

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFileIRMSettingsCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFileIRMSettingsCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFileIRMSettingsBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2);
        }

        [TestMethod]
        public async Task GetFileIRMSettingsBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, string documentUrl) = await AddMockDocumentToSharedDocuments(0);

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

            await CleanupMockDocumentFromSharedDocuments(2);
        }
        #endregion

        // TODO: Uncomment this test with live test on IRM enabled tenant
        //[TestMethod]
        //public async Task GetFileEffectiveIRMSettingsTest()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    await AddMockDocumentToSharedDocuments(0, "test_irm_effective_settings.docx");

        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
        //    {
        //        string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_irm_effective_settings.docx";
        //        IFile documentWithEffectiveIrm = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.EffectiveInformationRightsManagementSettings);

        //        // TODO The asserts below checks the effective IRM settings object is instantiated. More relevant tests should be done on a IRM enabled and configured tenant
        //        Assert.IsNotNull(documentWithEffectiveIrm.EffectiveInformationRightsManagementSettings);
        //    }

        //    await CleanupMockDocumentFromSharedDocuments(2, "test_irm_effective_settings.docx");
        //}

        #region Get file versions
        [TestMethod]
        public async Task GetFileVersionsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string libraryName, string documentName, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

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
            }

            // New context to ensure reload the file
            // TODO: Shouldn't we implement a mechanism to ensure reload the properties after action
            // (e.g. Publish() => refreshes the versions collection
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile documentWithVersions = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.Versions);

                Assert.IsNotNull(documentWithVersions.Versions);
                // The versions history contains 2 versions
                Assert.AreEqual(3, documentWithVersions.Versions.Count());
                Assert.AreEqual($"_vti_history/1/{libraryName}/{documentName}", documentWithVersions.Versions[0].Url);
                Assert.AreEqual("0.1", documentWithVersions.Versions[0].VersionLabel);
                Assert.AreEqual("0.2", documentWithVersions.Versions[1].VersionLabel);

                Assert.AreEqual(3, documentWithVersions.Versions[2].ID);
                Assert.IsTrue(documentWithVersions.Versions[2].Created != DateTime.MinValue);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task GetFileVersionsTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string libraryName, string documentName, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                // Create 2 minor versions
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync();
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync();
            }

            // New context to ensure reload the file
            // TODO: Shouldn't we implement a mechanism to ensure reload the properties after action
            // (e.g. Publish() => refreshes the versions collection
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile documentWithVersions = context.Web.GetFileByServerRelativeUrl(documentUrl, f => f.Versions);

                Assert.IsNotNull(documentWithVersions.Versions);
                // The versions history contains 2 versions
                Assert.AreEqual(2, documentWithVersions.Versions.Count());
                Assert.AreEqual($"_vti_history/1/{libraryName}/{documentName}", documentWithVersions.Versions[0].Url);
                Assert.AreEqual("0.1", documentWithVersions.Versions[0].VersionLabel);
                Assert.AreEqual("0.2", documentWithVersions.Versions[1].VersionLabel);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task GetFileVersionsCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string libraryName, string documentName, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                // Create 2 minor versions
                await testDocument.CheckoutBatchAsync();
                await testDocument.CheckinBatchAsync();
                await testDocument.CheckoutBatchAsync();
                await testDocument.CheckinBatchAsync();
                await context.ExecuteAsync();
            }

            // New context to ensure reload the file
            // TODO: Shouldn't we implement a mechanism to ensure reload the properties after action
            // (e.g. Publish() => refreshes the versions collection
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile documentWithVersions = await context.Web.GetFileByServerRelativeUrlBatchAsync(documentUrl, f => f.Versions);
                await context.ExecuteAsync();

                Assert.IsNotNull(documentWithVersions.Versions);
                // The versions history contains 2 versions
                Assert.AreEqual(2, documentWithVersions.Versions.Count());
                Assert.AreEqual($"_vti_history/1/{libraryName}/{documentName}", documentWithVersions.Versions[0].Url);
                Assert.AreEqual("0.1", documentWithVersions.Versions[0].VersionLabel);
                Assert.AreEqual("0.2", documentWithVersions.Versions[1].VersionLabel);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task GetFileVersionsCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string libraryName, string documentName, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                // Create 2 minor versions
                testDocument.CheckoutBatch();
                testDocument.CheckinBatch();
                testDocument.CheckoutBatch();
                testDocument.CheckinBatch();
                await context.ExecuteAsync();
            }

            // New context to ensure reload the file
            // TODO: Shouldn't we implement a mechanism to ensure reload the properties after action
            // (e.g. Publish() => refreshes the versions collection
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile documentWithVersions = context.Web.GetFileByServerRelativeUrlBatch(documentUrl, f => f.Versions);
                await context.ExecuteAsync();

                Assert.IsNotNull(documentWithVersions.Versions);
                // The versions history contains 2 versions
                Assert.AreEqual(2, documentWithVersions.Versions.Count());
                Assert.AreEqual($"_vti_history/1/{libraryName}/{documentName}", documentWithVersions.Versions[0].Url);
                Assert.AreEqual("0.1", documentWithVersions.Versions[0].VersionLabel);
                Assert.AreEqual("0.2", documentWithVersions.Versions[1].VersionLabel);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task GetFileVersionsBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string libraryName, string documentName, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

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
            }

            // New context to ensure reload the file
            // TODO: Shouldn't we implement a mechanism to ensure reload the properties after action
            // (e.g. Publish() => refreshes the versions collection
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                var batch = context.NewBatch();
                IFile documentWithVersions = await context.Web.GetFileByServerRelativeUrlBatchAsync(batch, documentUrl, f => f.Versions);
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(documentWithVersions.Versions);
                // The versions history contains 2 versions
                Assert.AreEqual(2, documentWithVersions.Versions.Count());
                Assert.AreEqual($"_vti_history/1/{libraryName}/{documentName}", documentWithVersions.Versions[0].Url);
                Assert.AreEqual("0.1", documentWithVersions.Versions[0].VersionLabel);
                Assert.AreEqual("0.2", documentWithVersions.Versions[1].VersionLabel);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        [TestMethod]
        public async Task GetFileVersionsBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string libraryName, string documentName, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

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
            }

            // New context to ensure reload the file
            // TODO: Shouldn't we implement a mechanism to ensure reload the properties after action
            // (e.g. Publish() => refreshes the versions collection
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                var batch = context.NewBatch();
                IFile documentWithVersions = context.Web.GetFileByServerRelativeUrlBatch(batch, documentUrl, f => f.Versions);
                await context.ExecuteAsync(batch);

                Assert.IsNotNull(documentWithVersions.Versions);
                // The versions history contains 2 versions
                Assert.AreEqual(2, documentWithVersions.Versions.Count());
                Assert.AreEqual($"_vti_history/1/{libraryName}/{documentName}", documentWithVersions.Versions[0].Url);
                Assert.AreEqual("0.1", documentWithVersions.Versions[0].VersionLabel);
                Assert.AreEqual("0.2", documentWithVersions.Versions[1].VersionLabel);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
        }

        #endregion

        #region TESTS TO REVIEW - Get file versionevents
        [TestMethod]
        public async Task GetFileVersionEventsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string libraryName, string documentName, string documentUrl) = await AddMockDocumentToMinorVersioningEnabledLibrary(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                // Create 2 minor versions
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync();
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync();
            }

            // New context to ensure reload the file
            // TODO: Shouldn't we implement a mechanism to ensure reload the properties after action
            // (e.g. Publish() => refreshes the versions collection
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IFile documentWithVersionEvents = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.VersionEvents);

                Assert.IsNotNull(documentWithVersionEvents.VersionEvents);
                // TODO The VersionEvents property return an empty array, double-check what it's supposed to return
                Assert.AreEqual(0, documentWithVersionEvents.VersionEvents.Count());

            }

            await CleanupMockMinorVersioningEnabledLibrary(3);
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

            fileName = $"PNP_SDK_TEST_{fileName}";
            libraryName = $"PNP_SDK_TEST_{libraryName}";

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

        private async Task CleanupMockMinorVersioningEnabledLibrary(int contextId,
            [System.Runtime.CompilerServices.CallerMemberName] string libraryName = null,
            [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            libraryName = $"PNP_SDK_TEST_{libraryName}";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, contextId, testName))
            {
                IList documentLibrary = await context.Web.Lists.GetByTitleAsync(libraryName);
                await documentLibrary.DeleteAsync();
            }
        }

        private async Task<Tuple<string, string>> AddMockDocumentToSharedDocuments(int contextId,
              [System.Runtime.CompilerServices.CallerMemberName] string fileName = null,
              [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            if (!fileName.EndsWith(".docx"))
            {
                fileName += ".docx";
            }

            fileName = fileName.StartsWith("PNP_SDK_TEST_") ? fileName : $"PNP_SDK_TEST_{fileName}";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, contextId, testName))
            {
                IFolder folder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFile mockDocument = await folder.Files.AddAsync(fileName, System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                return new Tuple<string, string>(mockDocument.Name, mockDocument.ServerRelativeUrl);
            }
        }

        private async Task CleanupMockDocumentFromSharedDocuments(int contextId,
            [System.Runtime.CompilerServices.CallerMemberName] string fileName = null,
            string contextConfig = null, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            contextConfig ??= TestCommon.TestSite;
            if (!fileName.EndsWith(".docx"))
            {
                fileName += ".docx";
            }

            fileName = fileName.StartsWith("PNP_SDK_TEST_") ? fileName : $"PNP_SDK_TEST_{fileName}";
            using (var context = await TestCommon.Instance.GetContextAsync(contextConfig, contextId, testName))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/{fileName}";
                IFile mockDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                await mockDocument.DeleteAsync();
            }
        }
        #endregion
    }
}
