using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System.Threading.Tasks;
using PnP.Core.QueryModel;
using System;

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
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Get the default document library root folder
                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == "test.docx");

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual("test.docx", documentToFind.Name);
                Assert.AreEqual($"{sharedDocumentsFolderUrl}/test.docx", documentToFind.ServerRelativeUrl);
            }
        }

        [TestMethod]
        public async Task GetFileByServerRelativeUrlTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";

                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.IsNotNull(testDocument);
                Assert.AreEqual("test.docx", testDocument.Name);
                Assert.AreEqual(testDocumentServerRelativeUrl, testDocument.ServerRelativeUrl);
            }
        }

        [TestMethod]
        public async Task GetFileByServerRelativeUrlBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlBatchAsync(testDocumentServerRelativeUrl);

                // Execute the requests in the batch
                await context.ExecuteAsync();

                Assert.IsNotNull(testDocument);
                Assert.AreEqual("test.docx", testDocument.Name);
                Assert.AreEqual(testDocumentServerRelativeUrl, testDocument.ServerRelativeUrl);
            }
        }

        [TestMethod]
        public async Task PublishFileTest()
        {
            // TODO Test the major version value when capable of dealing with a dedicated library and create a dedicated file
            // TODO Enable/Disable the versioning capabilities makes the versions value not reliable
            //int currentVersion;
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Enable minor versioning
                IList sharedDocs = await context.Web.Lists.GetByTitleAsync("Documents", l => l.EnableMinorVersions);
                sharedDocs.EnableMinorVersions = true;
                await sharedDocs.UpdateAsync();

                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                //currentVersion = testDocument.MajorVersion;

                await testDocument.PublishAsync("TEST PUBLISH");
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH", testDocument.CheckInComment);

                // disbale minor versioning
                IList sharedDocs = await context.Web.Lists.GetByTitleAsync("Documents", l => l.EnableMinorVersions);
                sharedDocs.EnableMinorVersions = false;
                await sharedDocs.UpdateAsync();
            }
        }

        [TestMethod]
        public async Task PublishFileWithBatchTest()
        {
            // TODO Test the major version value when capable of dealing with a dedicated library and create a dedicated file
            // TODO Enable/Disable the versioning capabilities makes the versions value not reliable
            //int currentVersion;
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Enable minor versioning
                IList sharedDocs = await context.Web.Lists.GetByTitleAsync("Documents", l => l.EnableMinorVersions);
                sharedDocs.EnableMinorVersions = true;
                await sharedDocs.UpdateAsync();

                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                // The test document should be loaded (and its Id should be known) before continuing with Batch request
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                //currentVersion = testDocument.MajorVersion;

                await testDocument.PublishBatchAsync("TEST PUBLISH WITH BATCH");
                await context.ExecuteAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST PUBLISH WITH BATCH", testDocument.CheckInComment);

                // disbale minor versioning
                IList sharedDocs = await context.Web.Lists.GetByTitleAsync("Documents", l => l.EnableMinorVersions);
                sharedDocs.EnableMinorVersions = false;
                await sharedDocs.UpdateAsync();
            }
        }

        [TestMethod]
        public async Task UnpublishFileTest()
        {
            // TODO Test the major version value when capable of dealing with a dedicated library and create a dedicated file
            // TODO Enable/Disable the versioning capabilities makes the versions value not reliable
            //int currentVersion;
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Enable minor versioning
                IList sharedDocs = await context.Web.Lists.GetByTitleAsync("Documents", l => l.EnableMinorVersions);
                sharedDocs.EnableMinorVersions = true;
                await sharedDocs.UpdateAsync();

                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                //currentVersion = testDocument.MajorVersion;

                await testDocument.PublishAsync("TEST PUBLISH");
                await testDocument.UnpublishAsync("TEST UNPUBLISHED");
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST UNPUBLISHED", testDocument.CheckInComment);

                // disbale minor versioning
                IList sharedDocs = await context.Web.Lists.GetByTitleAsync("Documents", l => l.EnableMinorVersions);
                sharedDocs.EnableMinorVersions = false;
                await sharedDocs.UpdateAsync();
            }
        }

        [TestMethod]
        public async Task UnpublishFileWithBatchTest()
        {
            // TODO Test the major version value when capable of dealing with a dedicated library and create a dedicated file
            // TODO Enable/Disable the versioning capabilities makes the versions value not reliable
            //int currentVersion;
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Enable minor versioning
                IList sharedDocs = await context.Web.Lists.GetByTitleAsync("Documents", l => l.EnableMinorVersions);
                sharedDocs.EnableMinorVersions = true;
                await sharedDocs.UpdateAsync();

                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                //currentVersion = testDocument.MajorVersion;

                await testDocument.PublishAsync("TEST PUBLISH");

                await testDocument.UnpublishBatchAsync("TEST UNPUBLISHED WITH BATCH");
                await context.ExecuteAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                //Assert.AreEqual(currentVersion + 1, testDocument.MajorVersion);
                Assert.AreEqual("TEST UNPUBLISHED WITH BATCH", testDocument.CheckInComment);

                // disbale minor versioning
                IList sharedDocs = await context.Web.Lists.GetByTitleAsync("Documents", l => l.EnableMinorVersions);
                sharedDocs.EnableMinorVersions = false;
                await sharedDocs.UpdateAsync();
            }
        }

        [TestMethod]
        public async Task CheckoutFileTest()
        {
            // TODO Test the major version value when capable of dealing with a dedicated library and create a dedicated file
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                await testDocument.CheckoutAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }
        }

        [TestMethod]
        public async Task CheckoutFileWithBatchTest()
        {
            // TODO Test the major version value when capable of dealing with a dedicated library and create a dedicated file
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                await testDocument.CheckoutBatchAsync();
                await context.ExecuteAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreNotEqual(CheckOutType.None, testDocument.CheckOutType);

                // Undo checkout of the file
                await testDocument.UndoCheckoutAsync();
            }
        }

        [TestMethod]
        public async Task UndoCheckoutFileTest()
        {
            // TODO Test the major version value when capable of dealing with a dedicated library and create a dedicated file
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                await testDocument.CheckoutAsync();
                await testDocument.UndoCheckoutAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }
        }

        [TestMethod]
        public async Task UndoCheckoutFileWithBatchTest()
        {
            // TODO Test the major version value when capable of dealing with a dedicated library and create a dedicated file
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                await testDocument.CheckoutAsync();
                await testDocument.UndoCheckoutBatchAsync();
                await context.ExecuteAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
            }
        }

        [TestMethod]
        public async Task CheckinFileTest()
        {
            // TODO Test the major version value when capable of dealing with a dedicated library and create a dedicated file
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                await testDocument.CheckoutAsync();
                await testDocument.CheckinAsync("TEST CHECK IN", CheckinType.MajorCheckIn);
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN", testDocument.CheckInComment);
                // TODO Test version of checked in file
            }
        }

        [TestMethod]
        public async Task CheckinFileWithBatchTest()
        {
            // TODO Test the major version value when capable of dealing with a dedicated library and create a dedicated file
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                await testDocument.CheckoutAsync();
                await testDocument.CheckinBatchAsync("TEST CHECK IN WITH BATCH", CheckinType.MajorCheckIn);
                await context.ExecuteAsync();
            }

            // Use a different context to make sure the file is reloaded
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Assert.AreEqual(CheckOutType.None, testDocument.CheckOutType);
                Assert.AreEqual("TEST CHECK IN WITH BATCH", testDocument.CheckInComment);
                // TODO Test version of checked in file
            }
        }

        [TestMethod]
        public async Task RecycleFileTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // TODO : When the capability is added, add a mock document to recycle

                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                Guid recycleBinId = await testDocument.RecycleAsync();

                Assert.AreNotEqual(Guid.Empty, recycleBinId);
            }
        }

        [TestMethod]
        public async Task RecycleFileWithBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // TODO : When the capability is added, add a mock document to recycle

                string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);

                await testDocument.RecycleBatchAsync();
                await context.ExecuteAsync();
            }

            // Use a second context to make sure the file is reloaded from SharePoint
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                try
                {
                    string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                    Assert.Fail("The document was found and should not");
                }
                catch (SharePointRestServiceException serviceException)
                {
                    Assert.AreEqual(404, ((SharePointRestError)serviceException.Error).HttpResponseCode);
                }
            }
        }



        // TODO Uncomment when the AddAsync is properly implemented without batch call
        //[TestMethod]
        //public async Task AddFileToFolderTest()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        IFolder parentFolder = await context.Web.Folders.FirstOrDefaultAsync(f => f.Name == "SiteAssets");

        //        IFile addedFile = await parentFolder.Files.AddAsync("test_added.docx", System.IO.File.OpenRead(".\\TestAssets\\test.docx"));


        //        // Test the created object
        //        Assert.IsNotNull(addedFile);
        //        Assert.AreEqual("test_added.docx", addedFile.Name);
        //        Assert.AreNotEqual(default, addedFile.UniqueId);

        //        await addedFile.DeleteAsync();
        //    }
        //}

        // TODO Uncomment when the AddAsync is properly implemented without batch call
        //[TestMethod]
        //public async Task AddFileToLibraryFolderTest()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        IFolder folder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
        //        IFile addedFile = await folder.Files.AddAsync("test_added.docx", System.IO.File.OpenRead(".\\TestAssets\\test.docx"));

        //        Assert.IsNotNull(addedFile);
        //        Assert.AreEqual("test_added.docx", addedFile.Name);
        //        Assert.AreNotEqual(default, addedFile.UniqueId);

        //        await addedFile.DeleteAsync();
        //    }
        //}

        // TODO Implement the update metadata test when the list item Graph identifier can be resolved from the file object
        //[TestMethod]
        //public async Task UpdateFileMetadataTest()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
        //        IFile testDocument = await context.Web.GetFileByServerRelativeUrlBatchAsync(testDocumentServerRelativeUrl);

        //        testDocument.ListItemAllFields.Values["Title"]= "test_updated.docx";
        //        await testDocument.ListItemAllFields.UpdateAsync();
        //    }

        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        string testDocumentServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/test.docx";
        //        IFile testDocument = await context.Web.GetFileByServerRelativeUrlBatchAsync(testDocumentServerRelativeUrl);

        //        Assert.AreEqual("test_updated.docx", testDocument.ListItemAllFields.Title);

        //        testDocument.ListItemAllFields.Title = "test.docx";
        //        await testDocument.ListItemAllFields.UpdateAsync();
        //    }
        //}

        // TODO Implement the Delete file test when
        //[TestMethod]
        //public async Task DeleteFileTest()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {

        //    }
        //}
    }
}
