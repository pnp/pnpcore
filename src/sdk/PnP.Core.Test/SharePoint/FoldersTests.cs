using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.QueryModel;
using System.Collections.Generic;
using System;
using System.IO;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class FoldersTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetWebFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder folder = await context.Web.Folders.FirstOrDefaultAsync(f => f.Name == "SiteAssets");
                Assert.IsNotNull(folder);
            }
        }

        [TestMethod]
        public async Task GetFolderByServerRelativeUrlTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string sharedDocumentsFolderServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                string sitePagesFolderServerRelativeUrl = $"{context.Uri.PathAndQuery}/SitePages";

                IFolder sharedDocumentsfolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderServerRelativeUrl);
                IFolder sitePagesfolder = await context.Web.GetFolderByServerRelativeUrlAsync(sitePagesFolderServerRelativeUrl);

                Assert.IsNotNull(sharedDocumentsfolder);
                Assert.AreEqual("Shared Documents", sharedDocumentsfolder.Name);
                Assert.IsNotNull(sitePagesfolder);
                Assert.AreEqual("SitePages", sitePagesfolder.Name);
            }
        }

        [TestMethod]
        public async Task GetFolderByServerRelativeUrlBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string sharedDocumentsFolderServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                string sitePagesFolderServerRelativeUrl = $"{context.Uri.PathAndQuery}/SitePages";

                IFolder sharedDocumentsfolder = await context.Web.GetFolderByServerRelativeUrlBatchAsync(sharedDocumentsFolderServerRelativeUrl);
                IFolder sitePagesfolder = await context.Web.GetFolderByServerRelativeUrlBatchAsync(sitePagesFolderServerRelativeUrl);

                // Execute the requests in the batch
                await context.ExecuteAsync();

                Assert.IsNotNull(sharedDocumentsfolder);
                Assert.AreEqual("Shared Documents", sharedDocumentsfolder.Name);
                Assert.IsNotNull(sitePagesfolder);
                Assert.AreEqual("SitePages", sitePagesfolder.Name);
            }
        }

        [TestMethod]
        public async Task GetFolderStorageMetricsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder mockFolder = await parentFolder.Folders.AddAsync("TEST_STORAGE_METRICS");
                string testStorageMetricsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents/TEST_STORAGE_METRICS";

                IFolder folderWithStorageMetrics = await context.Web.GetFolderByServerRelativeUrlAsync(testStorageMetricsFolderUrl, f => f.StorageMetrics);

                Assert.IsNotNull(folderWithStorageMetrics.StorageMetrics);
                Assert.AreNotEqual(default, folderWithStorageMetrics.StorageMetrics.LastModified);
                Assert.AreEqual(0, folderWithStorageMetrics.StorageMetrics.TotalFileCount);
                Assert.AreEqual(152, folderWithStorageMetrics.StorageMetrics.TotalSize);
                Assert.AreEqual(0, folderWithStorageMetrics.StorageMetrics.TotalFileStreamSize);

                await mockFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetFolderPropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string sharedDocumentsServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/";

                IFolder folderWithProperties = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsServerRelativeUrl, f => f.Properties);

                Assert.IsNotNull(folderWithProperties.Properties);
                Assert.AreEqual("true", folderWithProperties.Properties["vti_x005f_isbrowsable"]);
                Assert.AreEqual("true", (object)folderWithProperties.Properties.AsDynamic().vti_x005f_isbrowsable);
                Assert.AreEqual(1, folderWithProperties.Properties["vti_x005f_level"]);
                Assert.AreEqual(1, (object)folderWithProperties.Properties.AsDynamic().vti_x005f_level);
            }
        }

        [TestMethod]
        public async Task AddFolderFromWebFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = await context.Web.Folders.FirstOrDefaultAsync(f => f.Name == "SiteAssets");

                IFolder newFolder = await parentFolder.Folders.AddAsync("TEST");

                // Test the created object
                Assert.IsNotNull(newFolder);
                Assert.AreEqual("TEST", newFolder.Name);
                Assert.AreNotEqual(default, newFolder.UniqueId);

                await newFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetListRootFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder folder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();

                Assert.IsNotNull(folder);
                Assert.AreEqual("Shared Documents", folder.Name);
                Assert.AreNotEqual(default, folder.UniqueId);
            }
        }

        [TestMethod]
        public async Task GetListSubFoldersTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder mockFolder = await parentFolder.Folders.AddAsync("TEST");

                List<IFolder> folders = await context.Web.Lists.GetByTitle("Documents").RootFolder.Folders.ToListAsync();

                Assert.AreNotEqual(0, folders.Count);

                await mockFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task QueryListSubFolderTest()
        {
            //TestCommon.Instance.Mocking = false;

            await AddMockFolderToSharedDocuments(0, "TEST_QUERY");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // NOTE: 
                // Currently linq query on folders (with the fluent syntax below) is working only if the RootFolder is previously loaded
                IFolder sharedDocsRootFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder foundFolder = await sharedDocsRootFolder.Folders.FirstOrDefaultAsync(f => f.Name == "TEST_QUERY");

                Assert.IsNotNull(foundFolder);
                Assert.AreNotEqual(default, foundFolder.UniqueId);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_QUERY");
        }

        #region Folder Tests

        [TestMethod]
        public async Task AddListFolderAsyncTest()
        {
            TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // NOTE: To be truly fluent, this should work but UniqueId of RootFolder is not populated
                //IFolder newFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.Folders.AddAsync("TEST");

                // This works
                IFolder parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder newFolder = await parentFolder.Folders.AddAsync("TEST");

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // NOTE: To be truly fluent, this should work but UniqueId of RootFolder is not populated
                //IFolder newFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.Folders.AddAsync("TEST");

                // This works
                IFolder parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder newFolder = parentFolder.Folders.Add("TEST");

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFolderBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // NOTE: To be truly fluent, this should work but UniqueId of RootFolder is not populated
                //IFolder newFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.Folders.AddAsync("TEST");

                // This works
                IFolder parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder newFolder = await parentFolder.Folders.AddBatchAsync("TEST");
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFolderBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // NOTE: To be truly fluent, this should work but UniqueId of RootFolder is not populated
                //IFolder newFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.Folders.AddAsync("TEST");

                // This works
                IFolder parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder newFolder = parentFolder.Folders.AddBatch("TEST");
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFolderSpecificBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // NOTE: To be truly fluent, this should work but UniqueId of RootFolder is not populated
                //IFolder newFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.Folders.AddAsync("TEST");

                var newBatch = context.NewBatch();

                // This works
                IFolder parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder newFolder = await parentFolder.Folders.AddBatchAsync(newBatch, "TEST");
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFolderSpecificBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // NOTE: To be truly fluent, this should work but UniqueId of RootFolder is not populated
                //IFolder newFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.Folders.AddAsync("TEST");

                var newBatch = context.NewBatch();

                // This works
                IFolder parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder newFolder = parentFolder.Folders.AddBatch(newBatch, "TEST");
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }

        #endregion

        [TestMethod]
        public async Task AddListSubFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // NOTE: To be truly fluent, this should work but UniqueId of RootFolder is not populated
                //IFolder newFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.AddSubFolderAsync("TEST");

                // This works
                IFolder parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder newFolder = await parentFolder.AddFolderAsync("TEST");

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }


        [TestMethod]
        public async Task UpdateFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder folderToUpdate = await parentFolder.Folders.AddAsync("TEST");

                // NOTE: WelcomePage is currently the only supported updatable property of Folder
                folderToUpdate.WelcomePage = "NewWelcomePage.aspx";

                await folderToUpdate.UpdateAsync();

                Assert.AreEqual("NewWelcomePage.aspx", folderToUpdate.WelcomePage);

                await folderToUpdate.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task DeleteFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder folderToDelete = await parentFolder.Folders.AddAsync("TO DELETE FOLDER");

                // Test if the folder is created
                Assert.IsNotNull(folderToDelete);

                await folderToDelete.DeleteAsync();

                // Test if the folder is still found
                IFolder folderToFind = await (from ct in context.Web.Lists.GetByTitle("Documents").RootFolder.Folders
                                              where ct.Name == "TO DELETE FOLDER"
                                              select ct).FirstOrDefaultAsync();

                Assert.IsNull(folderToFind);
            }
        }


        [TestMethod]
        public async Task CopyFolderWithoutOptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_COPY_NO_OPTIONS");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFolder folderToCopy = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/TEST_COPIED_NO_OPTIONS";
                await folderToCopy.CopyToAsync(destinationServerRelativeUrl);

                IFolder foundCopiedFolder = await context.Web.GetFolderByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundCopiedFolder);
                Assert.AreEqual("TEST_COPIED_NO_OPTIONS", foundCopiedFolder.Name);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_COPY_NO_OPTIONS");
            await CleanupMockFolderFromSharedDocuments(2, "TEST_COPIED_NO_OPTIONS");
        }

        [TestMethod]
        public async Task CopyFolderBatchWithoutOptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_COPY_BATCH_NO_OPTIONS");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFolder folderToCopy = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/TEST_COPIED_BATCH_NO_OPTIONS";
                await folderToCopy.CopyToBatchAsync(destinationServerRelativeUrl);
                await context.ExecuteAsync();

                IFolder foundCopiedFolder = await context.Web.GetFolderByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundCopiedFolder);
                Assert.AreEqual("TEST_COPIED_BATCH_NO_OPTIONS", foundCopiedFolder.Name);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_COPY_BATCH_NO_OPTIONS");
            await CleanupMockFolderFromSharedDocuments(2, "TEST_COPIED_BATCH_NO_OPTIONS");
        }

        [TestMethod]
        public async Task CopyFolderWithOptionsTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_COPY_OPTIONS");
            string copyDestUrl = await AddMockFolderToSharedDocuments(0, "TEST_COPY2_OPTIONS");

            string folderToFindName = "TEST_COPY2_OPTIONS1"; // With KeepBoth option, a numeric suffix is added to the copied folder's name

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFolder folderToCopy = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);

                await folderToCopy.CopyToAsync(copyDestUrl, new MoveCopyOptions()
                {
                    KeepBoth = true,
                    ResetAuthorAndCreatedOnCopy = true,
                    ShouldBypassSharedLocks = false, // NOTE no easy way to test this
                });

                // NOTE: 
                // Currently linq query on folders (with the fluent syntax below) is working only if the RootFolder is previously loaded
                IFolder sharedDocsRootFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder foundCopiedFolder = await sharedDocsRootFolder.Folders.FirstOrDefaultAsync(f => f.Name == folderToFindName);
                Assert.IsNotNull(foundCopiedFolder);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_COPY_OPTIONS");
            await CleanupMockFolderFromSharedDocuments(2, folderToFindName);
        }

        [TestMethod]
        public async Task CopyFolderBatchWithOptionsTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_COPY_BATCH_OPTIONS");
            string copyDestUrl = await AddMockFolderToSharedDocuments(0, "TEST_COPY2_BATCH_OPTIONS");

            string folderToFindName = "TEST_COPY2_BATCH_OPTIONS1"; // With KeepBoth option, a numeric suffix is added to the copied folder's name

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFolder folderToCopy = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);
                await folderToCopy.CopyToBatchAsync(copyDestUrl, new MoveCopyOptions()
                {
                    KeepBoth = true,
                    ResetAuthorAndCreatedOnCopy = true,
                    ShouldBypassSharedLocks = false, // NOTE no easy way to test this
                });
                await context.ExecuteAsync();

                // NOTE: 
                // Currently linq query on folders (with the fluent syntax below) is working only if the RootFolder is previously loaded
                IFolder sharedDocsRootFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder foundCopiedFolder = await sharedDocsRootFolder.Folders.FirstOrDefaultAsync(f => f.Name == folderToFindName);
                Assert.IsNotNull(foundCopiedFolder);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_COPY_BATCH_OPTIONS");
            await CleanupMockFolderFromSharedDocuments(2, "TEST_COPY2_BATCH_OPTIONS");
            await CleanupMockFolderFromSharedDocuments(2, folderToFindName);
        }

        [TestMethod]
        public async Task MoveFolderWithoutOptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_MOVE_NO_OPTIONS");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFolder folderToMove = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/TEST_MOVED_FOLDER_NO_OPTIONS";
                await folderToMove.MoveToAsync(destinationServerRelativeUrl);

                IFolder foundMovedFolder = await context.Web.GetFolderByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundMovedFolder);
                Assert.AreEqual("TEST_MOVED_FOLDER_NO_OPTIONS", foundMovedFolder.Name);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_MOVED_FOLDER_NO_OPTIONS");
        }

        [TestMethod]
        public async Task MoveFolderBatchWithoutOptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_MOVE_BATCH_NO_OPTIONS");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFolder folderToMove = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/TEST_MOVED_FOLDER_BATCH_NO_OPTIONS";
                await folderToMove.MoveToBatchAsync(destinationServerRelativeUrl);
                await context.ExecuteAsync();

                IFolder foundMovedFolder = await context.Web.GetFolderByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundMovedFolder);
                Assert.AreEqual("TEST_MOVED_FOLDER_BATCH_NO_OPTIONS", foundMovedFolder.Name);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_MOVED_FOLDER_BATCH_NO_OPTIONS");
        }

        [TestMethod]
        public async Task MoveFolderWithOptionsTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_MOVE_OPTIONS");
            string existingFolderUrl = await AddMockFolderToSharedDocuments(0, "TEST_MOVE_EXISTING_OPTIONS");
            string folderToFindName = "TEST_MOVE_EXISTING_OPTIONS1"; // With KeepBoth option, a numeric suffix is added to the copied folder's name

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFolder folderToMove = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/TEST_MOVE_EXISTING_OPTIONS";
                await folderToMove.MoveToAsync(destinationServerRelativeUrl, new MoveCopyOptions()
                {
                    KeepBoth = true,
                    ResetAuthorAndCreatedOnCopy = true,
                    //ShouldBypassSharedLocks = false
                });

                // NOTE: 
                // Currently linq query on folders (with the fluent syntax below) is working only if the RootFolder is previously loaded
                IFolder sharedDocsRootFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder foundMovedFolder = await sharedDocsRootFolder.Folders.FirstOrDefaultAsync(f => f.Name == folderToFindName);
                Assert.IsNotNull(foundMovedFolder);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_MOVE_EXISTING_OPTIONS");
            await CleanupMockFolderFromSharedDocuments(2, folderToFindName);
        }

        [TestMethod]
        public async Task MoveFolderBatchWithOptionsTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_MOVE_BATCH_OPTIONS");
            string existingFolderUrl = await AddMockFolderToSharedDocuments(0, "TEST_MOVE_EXISTING_BATCH_OPTIONS");
            string folderToFindName = "TEST_MOVE_EXISTING_BATCH_OPTIONS1"; // With KeepBoth option, a numeric suffix is added to the copied folder's name

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFolder folderToMove = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/TEST_MOVE_EXISTING_BATCH_OPTIONS";
                await folderToMove.MoveToBatchAsync(destinationServerRelativeUrl, new MoveCopyOptions()
                {
                    KeepBoth = true,
                    ResetAuthorAndCreatedOnCopy = true,
                    //ShouldBypassSharedLocks = false
                });
                await context.ExecuteAsync();

                // NOTE: 
                // Currently linq query on folders (with the fluent syntax below) is working only if the RootFolder is previously loaded
                IFolder sharedDocsRootFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder foundMovedFolder = await sharedDocsRootFolder.Folders.FirstOrDefaultAsync(f => f.Name == folderToFindName);
                Assert.IsNotNull(foundMovedFolder);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_MOVE_EXISTING_BATCH_OPTIONS");
            await CleanupMockFolderFromSharedDocuments(2, folderToFindName);
        }

        #region Mock folder
        private async Task<string> AddMockFolderToSharedDocuments(int contextId, string folderName, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, contextId, testName))
            {
                IFolder folder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder subFolder = await folder.AddFolderAsync(folderName);
                return subFolder.ServerRelativeUrl;
            }
        }

        private async Task CleanupMockFolderFromSharedDocuments(int contextId, string folderName, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, contextId, testName))
            {
                string mockFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents/{folderName}";
                IFolder mockFolder = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderUrl);
                await mockFolder.DeleteAsync();
            }
        }
        #endregion
    }
}
