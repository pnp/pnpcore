using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System.Threading.Tasks;
using PnP.Core.QueryModel;
using PnP.Core.Model;
using System;
using System.IO;

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

        [TestMethod]
        public async Task QueryFileInFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            await AddMockDocumentToSharedDocuments(0, "test_query.docx");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // Get the default document library root folder
                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == "test_query.docx");

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual("test_query.docx", documentToFind.Name);
                Assert.IsTrue(documentToFind.ServerRelativeUrl.EndsWith("/test_query.docx"));
            }

            await CleanupMockDocumentFromSharedDocuments(2, "test_query.docx");
        }

        [TestMethod]
        public async Task GetFileByServerRelativeUrlTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToSharedDocuments(0, "test_get.docx");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync($"{context.Uri.PathAndQuery}/Shared Documents/test_get.docx");

                Assert.IsNotNull(testDocument);
                Assert.AreEqual("test_get.docx", testDocument.Name);
                Assert.IsTrue(testDocument.ServerRelativeUrl.EndsWith("/test_get.docx"));
            }

            await CleanupMockDocumentFromSharedDocuments(2, "test_get.docx");
        }

        [TestMethod]
        public async Task GetFileByServerRelativeUrlBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToSharedDocuments(0, "test_get_with_batch.docx");


            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_get_with_batch.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlBatchAsync(testDocumentServerRelativeUrl);

                // Execute the requests in the batch
                await context.ExecuteAsync();

                Assert.IsNotNull(testDocument);
                Assert.AreEqual("test_get_with_batch.docx", testDocument.Name);
                Assert.IsTrue(testDocument.ServerRelativeUrl.EndsWith("/test_get_with_batch.docx"));
            }

            await CleanupMockDocumentFromSharedDocuments(2, "test_get_with_batch.docx");
        }

        [TestMethod]
        public async Task PublishFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToMinorVersioningEnabledLibrary(0, "test_publish", "test_publish.docx");

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_publish/test_publish.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishAsync("TEST PUBLISH");
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_publish/test_publish.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);


                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion+1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3, "test_publish");
        }

        [TestMethod]
        public async Task PublishFileWithBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToMinorVersioningEnabledLibrary(0, "test_publish_with_batch", "test_publish_with_batch.docx");

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_publish_with_batch/test_publish_with_batch.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishBatchAsync("TEST PUBLISH");
                await context.ExecuteAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_publish_with_batch/test_publish_with_batch.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);


                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3, "test_publish_with_batch");
        }

        [TestMethod]
        public async Task UnpublishFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToMinorVersioningEnabledLibrary(0, "test_publish", "test_publish.docx");

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_publish/test_publish.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishAsync("TEST PUBLISH");
                await testDocument.UnpublishAsync("TEST UNPUBLISHED");
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_publish/test_publish.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3, "test_publish");
        }

        [TestMethod]
        public async Task UnpublishFileWithBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToMinorVersioningEnabledLibrary(0, "test_publish", "test_publish.docx");

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_publish/test_publish.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.PublishAsync("TEST PUBLISH");
                await testDocument.UnpublishBatchAsync("TEST UNPUBLISHED");
                await context.ExecuteAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_publish/test_publish.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3, "test_publish");
        }

        [TestMethod]
        public async Task CheckoutFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToMinorVersioningEnabledLibrary(0, "test_checkout", "test_checkout.docx");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_checkout/test_checkout.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                await testDocument.CheckoutAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_checkout/test_checkout.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }

            await CleanupMockMinorVersioningEnabledLibrary(3, "test_checkout");
        }

        [TestMethod]
        public async Task CheckoutFileWithBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToMinorVersioningEnabledLibrary(0, "test_checkout_with_batch", "test_checkout_with_batch.docx");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_checkout_with_batch/test_checkout_with_batch.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                await testDocument.CheckoutBatchAsync();
                await context.ExecuteAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_checkout_with_batch/test_checkout_with_batch.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }

            await CleanupMockMinorVersioningEnabledLibrary(3, "test_checkout_with_batch");
        }

        [TestMethod]
        public async Task UndoCheckoutFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToMinorVersioningEnabledLibrary(0, "test_undo_checkout", "test_undo_checkout.docx");

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_undo_checkout/test_undo_checkout.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.UndoCheckoutAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_undo_checkout/test_undo_checkout.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3, "test_undo_checkout");
        }

        [TestMethod]
        public async Task UndoCheckoutFileWithBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToMinorVersioningEnabledLibrary(0, "test_undo_checkout_with_batch", "test_undo_checkout_with_batch.docx");

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_undo_checkout_with_batch/test_undo_checkout_with_batch.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.UndoCheckoutBatchAsync();
                await context.ExecuteAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_undo_checkout_with_batch/test_undo_checkout_with_batch.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3, "test_undo_checkout_with_batch");
        }

        [TestMethod]
        public async Task CheckinFileMajorVersionTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToMinorVersioningEnabledLibrary(0, "test_checkin_major", "test_checkin_major.docx");

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_checkin_major/test_checkin_major.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync("TEST CHECK IN", CheckinType.MajorCheckIn);
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_checkin_major/test_checkin_major.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);
                Assert.IsTrue(testDocument.MajorVersion == initialMajorVersion + 1);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3, "test_checkin_major");
        }

        [TestMethod]
        public async Task CheckinFileMinorVersionTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToMinorVersioningEnabledLibrary(0, "test_checkin_minor", "test_checkin_minor.docx");

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_checkin_minor/test_checkin_minor.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync("TEST CHECK IN", CheckinType.MinorCheckIn);
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_checkin_minor/test_checkin_minor.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.IsTrue(testDocument.MinorVersion == initialMinorVersion + 1);

            }

            await CleanupMockMinorVersioningEnabledLibrary(3, "test_checkin_minor");
        }

        [TestMethod]
        public async Task CheckinFileOverwriteVersionTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToMinorVersioningEnabledLibrary(0, "test_checkin_overwrite", "test_checkin_overwrite.docx");

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_checkin_overwrite/test_checkin_overwrite.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync("TEST CHECK IN", CheckinType.OverwriteCheckIn);
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_checkin_overwrite/test_checkin_overwrite.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);
                Assert.AreEqual(initialMajorVersion, testDocument.MajorVersion);
                Assert.AreEqual(initialMinorVersion, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3, "test_checkin_overwrite");
        }

        [TestMethod]
        public async Task CheckinFileWithBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToMinorVersioningEnabledLibrary(0, "test_checkin_with_batch", "test_checkin_with_batch.docx");

            int initialMajorVersion;
            int initialMinorVersion;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_checkin_with_batch/test_checkin_with_batch.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                initialMajorVersion = testDocument.MajorVersion;
                initialMinorVersion = testDocument.MinorVersion;
                await testDocument.CheckoutAsync();
                await testDocument.CheckinBatchAsync("TEST CHECK IN", CheckinType.MajorCheckIn);
                await context.ExecuteAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/test_checkin_with_batch/test_checkin_with_batch.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);
                Assert.IsTrue(testDocument.MajorVersion > initialMajorVersion);
                Assert.AreEqual(0, testDocument.MinorVersion);
            }

            await CleanupMockMinorVersioningEnabledLibrary(3, "test_checkin_with_batch");
        }

        [TestMethod]
        public async Task RecycleFileTest()
        {
            //TestCommon.Instance.Mocking = false;
            
            await AddMockDocumentToSharedDocuments(0, "test_recycle.docx");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_recycle.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Guid recycleBinId = await testDocument.RecycleAsync();

                Assert.AreNotEqual(Guid.Empty, recycleBinId);
            }

            // Use a second context to make sure the file is reloaded from SharePoint
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                try
                {
                    string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_recycle_with_batch.docx";
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        [TestMethod]
        public async Task RecycleFileWithBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            
            await AddMockDocumentToSharedDocuments(0, "test_recycle_with_batch.docx");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_recycle_with_batch.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                await testDocument.RecycleBatchAsync();
                await context.ExecuteAsync();
            }

            // Use a second context to make sure the file is reloaded from SharePoint
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                try
                {
                    string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_recycle_with_batch.docx";
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        [TestMethod]
        public async Task CopyFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToSharedDocuments(0, "test_copy.docx");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_copy.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_copied.docx";
                await testDocument.CopyToAsync(destinationServerRelativeUrl, true);

                IFile foundCopiedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundCopiedDocument);
                Assert.AreEqual("test_copied.docx", foundCopiedDocument.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(2, "test_copy.docx");
            await CleanupMockDocumentFromSharedDocuments(2, "test_copied.docx");
        }

        [TestMethod]
        public async Task CopyFileWithBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToSharedDocuments(0, "test_copy_with_batch.docx");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_copy_with_batch.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_copied_with_batch.docx";
                await testDocument.CopyToAsync(destinationServerRelativeUrl, true);

                IFile foundCopiedDocument = await context.Web.GetFileByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundCopiedDocument);
                Assert.AreEqual("test_copied_with_batch.docx", foundCopiedDocument.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(2, "test_copy_with_batch.docx");
            await CleanupMockDocumentFromSharedDocuments(2, "test_copied_with_batch.docx");
        }

        [TestMethod]
        public async Task MoveFileTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToSharedDocuments(0, "test_move.docx");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_move.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                string movedServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/moved_test_move.docx";
                await testDocument.MoveToAsync(movedServerRelativeUrl, MoveOperations.Overwrite);

                IFile foundMovedFile = await context.Web.GetFileByServerRelativeUrlAsync(movedServerRelativeUrl);
                Assert.IsNotNull(foundMovedFile);
                Assert.AreEqual("moved_test_move.docx", foundMovedFile.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(2, "moved_test_move.docx");
        }

        [TestMethod]
        public async Task MoveFileBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockDocumentToSharedDocuments(0, "test_move_with_batch.docx");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_move_with_batch.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                string movedServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/moved_test_move_with_batch.docx";
                await testDocument.MoveToBatchAsync(movedServerRelativeUrl, MoveOperations.Overwrite);
                await context.ExecuteAsync();

                IFile foundMovedFile = await context.Web.GetFileByServerRelativeUrlAsync(movedServerRelativeUrl);
                Assert.IsNotNull(foundMovedFile);
                Assert.AreEqual("moved_test_move_with_batch.docx", foundMovedFile.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(2, "moved_test_move_with_batch.docx");
        }

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


        [TestMethod]
        public async Task DeleteFileTest()
        {
            //TestCommon.Instance.Mocking = false;
            await AddMockDocumentToSharedDocuments(0, "test_delete.docx");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_delete.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                await testDocument.DeleteAsync();

                try
                {
                    IFile foundDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        [TestMethod]
        public async Task DeleteFileWithBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            await AddMockDocumentToSharedDocuments(0, "test_delete_with_batch.docx");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test_delete_with_batch.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                await testDocument.DeleteBatchAsync();
                await context.ExecuteAsync();

                try
                {
                    IFile foundDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }

        #region Test asset documents/libraries
        private async Task<string> AddMockDocumentToMinorVersioningEnabledLibrary(int contextId, string libraryName, string fileName, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, contextId, testName))
            {
                IList documentLibrary = await context.Web.Lists.AddAsync(libraryName, ListTemplateType.DocumentLibrary);
                documentLibrary.EnableVersioning = true;
                documentLibrary.EnableMinorVersions = true;
                await documentLibrary.UpdateAsync();
                IFolder folder = await documentLibrary.RootFolder.GetAsync();
                IFile mockDocument = await folder.Files.AddAsync(fileName, System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"));
                return mockDocument.ServerRelativeUrl;
            }
        }

        private async Task CleanupMockMinorVersioningEnabledLibrary(int contextId, string libraryName, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, contextId, testName))
            {
                IList documentLibrary = await context.Web.Lists.GetByTitleAsync(libraryName);
                await documentLibrary.DeleteAsync();
            }
        }

        private async Task<string> AddMockDocumentToSharedDocuments(int contextId, string fileName, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, contextId, testName))
            {
                IFolder folder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFile mockDocument = await folder.Files.AddAsync(fileName, System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"));
                return mockDocument.ServerRelativeUrl;
            }
        }

        private async Task CleanupMockDocumentFromSharedDocuments(int contextId, string fileName, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, contextId, testName))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/{fileName}";
                IFile mockDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                await mockDocument.DeleteAsync();
            }
        }
        #endregion
    }
}
