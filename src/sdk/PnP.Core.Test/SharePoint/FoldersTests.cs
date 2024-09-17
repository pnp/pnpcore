using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task GetWebFolderDetailsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder folder = await context.Web.Folders.FirstOrDefaultAsync(f => f.Name == "SiteAssets");
                Assert.IsNotNull(folder);
                Assert.IsTrue(folder.Exists);
                Assert.IsFalse(folder.IsWOPIEnabled);
                Assert.IsNull(folder.ProgID); //Ok this is a bit of a bad test
                Assert.AreNotEqual(DateTime.MinValue, folder.TimeCreated);
                Assert.AreNotEqual(DateTime.MinValue, folder.TimeLastModified);
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
        public async Task GetFolderByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string sharedDocumentsFolderServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                string sitePagesFolderServerRelativeUrl = $"{context.Uri.PathAndQuery}/SitePages";

                IFolder sitePagesfolder = await context.Web.GetFolderByServerRelativeUrlAsync(sitePagesFolderServerRelativeUrl);

                IFolder sitePagesFolder2 = await context.Web.GetFolderByIdAsync(sitePagesfolder.UniqueId);

                Assert.IsNotNull(sitePagesFolder2);
                Assert.AreEqual("SitePages", sitePagesFolder2.Name);
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
        public async Task GetFolderByIdBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string sitePagesFolderServerRelativeUrl = $"{context.Uri.PathAndQuery}/SitePages";

                IFolder sitePagesfolder = await context.Web.GetFolderByServerRelativeUrlAsync(sitePagesFolderServerRelativeUrl);

                IFolder sitePagesFolder2 = await context.Web.GetFolderByIdBatchAsync(sitePagesfolder.UniqueId);
                // Execute the requests in the batch
                await context.ExecuteAsync();

                Assert.IsNotNull(sitePagesFolder2);
                Assert.AreEqual("SitePages", sitePagesFolder2.Name);
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
                string sharedDocumentsServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents";

                IFolder folderWithProperties = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsServerRelativeUrl, f => f.Properties);

                Assert.IsNotNull(folderWithProperties.Properties);
                Assert.AreEqual("true", folderWithProperties.Properties["vti_isbrowsable"]);
                Assert.AreEqual("true", (object)folderWithProperties.Properties.AsDynamic().vti_isbrowsable);
                Assert.AreEqual(1, folderWithProperties.Properties["vti_level"]);
                Assert.AreEqual(1, (object)folderWithProperties.Properties.AsDynamic().vti_level);
            }
        }

        [TestMethod]
        public async Task SetFolderPropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByTitle("Site Pages", p => p.Title, p => p.RootFolder.QueryProperties(p => p.Properties));
                var properties = list.RootFolder.Properties;

                var propertyKey = "ListPropertiesTest123";
                var myProperty = properties.GetString(propertyKey, null);
                if (myProperty == null)
                {
                    properties.Values[propertyKey] = "test123";
                    await properties.UpdateAsync();
                }

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    list = context2.Web.Lists.GetByTitle("Site Pages", p => p.Title, p => p.RootFolder);
                    properties = (await list.RootFolder.GetAsync(p => p.Properties)).Properties;
                    myProperty = properties.GetString(propertyKey, null);
                    Assert.IsTrue(myProperty == "test123");

                    properties.Values[propertyKey] = null;
                    await properties.UpdateAsync();
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
                {
                    list = context3.Web.Lists.GetByTitle("Site Pages", p => p.Title, p => p.RootFolder);
                    properties = (await list.RootFolder.GetAsync(p => p.Properties)).Properties;
                    myProperty = properties.GetString(propertyKey, null);
                    Assert.IsTrue(myProperty == null);
                }
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
                //IFolder folder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder folder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;

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
                //IFolder parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
                IFolder mockFolder = await parentFolder.Folders.AddAsync("TEST");

                List<IFolder> folders = await context.Web.Lists.GetByTitle("Documents", p => p.RootFolder).RootFolder.Folders.ToListAsync();

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
                //IFolder sharedDocsRootFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder sharedDocsRootFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;

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
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
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
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
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
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
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
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
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
                var newBatch = context.NewBatch();

                // This works
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
                IFolder newFolder = await parentFolder.Folders.AddBatchAsync(newBatch, "TEST");
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFolderExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();

                // This works
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IFolder newFolder = await parentFolder.Folders.AddBatchAsync(newBatch, string.Empty);
                    await context.ExecuteAsync(newBatch);
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IFolder newFolder = await parentFolder.Folders.AddAsync(string.Empty);
                });
            }
        }

        [TestMethod]
        public async Task AddListFolderSpecificBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();

                // This works
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
                IFolder newFolder = parentFolder.Folders.AddBatch(newBatch, "TEST");
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }

        #endregion

        #region AddListSubFolder Tests

        [TestMethod]
        public async Task AddListSubFolderAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // This works
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
                IFolder newFolder = await parentFolder.AddFolderAsync("TEST");

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListSubFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // This works
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
                IFolder newFolder = parentFolder.AddFolder("TEST");

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListSubFolderBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // This works
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
                IFolder newFolder = await parentFolder.AddFolderBatchAsync("TEST");
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListSubFolderBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // This works
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
                IFolder newFolder = parentFolder.AddFolderBatch("TEST");
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListSubFolderSpecificBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();

                // This works
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
                IFolder newFolder = await parentFolder.AddFolderBatchAsync(newBatch, "TEST");
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListSubFolderSpecificBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();

                // This works
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
                IFolder newFolder = parentFolder.AddFolderBatch(newBatch, "TEST");
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(newFolder);

                await newFolder.DeleteAsync();
            }
        }

        #endregion

        #region EnsureFolder tests
        [TestMethod]
        public async Task EnsureListFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Site Pages", p => p.RootFolder)).RootFolder;

                var addedFolder = await parentFolder.EnsureFolderAsync("sub1/sub2");
                Assert.IsTrue(addedFolder != null);
                Assert.IsTrue(addedFolder.Name == "sub2");

                var folderToDelete = await parentFolder.EnsureFolderAsync("sub1");
                Assert.IsTrue(folderToDelete != null);
                Assert.IsTrue(folderToDelete.Name == "sub1");

                await folderToDelete.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task EnsureListFolderSingleLevelTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Site Pages", p => p.RootFolder)).RootFolder;

                var addedFolder = await parentFolder.EnsureFolderAsync("sub1");
                Assert.IsTrue(addedFolder != null);
                Assert.IsTrue(addedFolder.Name == "sub1");

                await addedFolder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task EnsureListFolderDeepTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Site Pages", p => p.RootFolder)).RootFolder;

                var addedFolder = await parentFolder.EnsureFolderAsync("sub1/sub2/sub3/sub4/sub5");
                Assert.IsTrue(addedFolder != null);
                Assert.IsTrue(addedFolder.Name == "sub5");

                var folderToDelete = await parentFolder.EnsureFolderAsync("sub1");
                Assert.IsTrue(folderToDelete != null);
                Assert.IsTrue(folderToDelete.Name == "sub1");

                await folderToDelete.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task EnsureListFolderInExistingHiarchyTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Site Pages", p => p.RootFolder)).RootFolder;

                var addedFolder = await parentFolder.EnsureFolderAsync("sub1/sub2/sub3a/sub4a");
                Assert.IsTrue(addedFolder != null);
                Assert.IsTrue(addedFolder.Name == "sub4a");

                var addedFolder2 = await parentFolder.EnsureFolderAsync("sub1/sub2/sub3b/sub4b");
                Assert.IsTrue(addedFolder2 != null);
                Assert.IsTrue(addedFolder2.Name == "sub4b");


                var folderToDelete = await parentFolder.EnsureFolderAsync("sub1");
                Assert.IsTrue(folderToDelete != null);
                Assert.IsTrue(folderToDelete.Name == "sub1");
                Assert.IsFalse(folderToDelete.IsPropertyAvailable(p => p.StorageMetrics));

                var folderToLoadWithExpression = await parentFolder.EnsureFolderAsync("sub1/sub2", p => p.Name, p => p.StorageMetrics); 
                Assert.IsTrue(folderToLoadWithExpression != null);
                Assert.IsTrue(folderToLoadWithExpression.Name == "sub2");
                Assert.IsTrue(folderToLoadWithExpression.IsPropertyAvailable(p => p.StorageMetrics));

                await folderToDelete.DeleteAsync();
            }
        }
        #endregion

        [TestMethod]
        public async Task RenameFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder addedFolder = null;
                try
                {
                    IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Site Pages", p => p.RootFolder)).RootFolder;

                    addedFolder = await parentFolder.EnsureFolderAsync("sub1");
                    Assert.IsTrue(addedFolder != null);
                    Assert.IsTrue(addedFolder.Name == "sub1");

                    // rename the added folder
                    addedFolder.Rename("newsub1");

                    Assert.IsTrue(addedFolder.Name == "newsub1");

                    // Get the folder again
                    IFolder addedFolder2 = await context.Web.GetFolderByIdAsync(addedFolder.UniqueId);

                    Assert.IsTrue(addedFolder2.Name == addedFolder.Name);
                }
                finally
                {
                    await addedFolder.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task UpdateFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
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
                IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
                IFolder folderToDelete = await parentFolder.Folders.AddAsync("TO DELETE FOLDER");

                // Test if the folder is created
                Assert.IsNotNull(folderToDelete);

                await folderToDelete.DeleteAsync();

                // Test if the folder is still found
                IFolder folderToFind = await context.Web.Lists.GetByTitle("Documents", p => p.RootFolder).RootFolder.Folders
                                        .FirstOrDefaultAsync(ct => ct.Name == "TO DELETE FOLDER");

                Assert.IsNull(folderToFind);
            }
        }


        [TestMethod]
        public async Task CopyFolderWithoutOptionAsyncTest()
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
        public async Task CopyFolderWithoutOptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_COPY_NO_OPTIONS");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFolder folderToCopy = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/TEST_COPIED_NO_OPTIONS";
                folderToCopy.CopyTo(destinationServerRelativeUrl);

                IFolder foundCopiedFolder = await context.Web.GetFolderByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundCopiedFolder);
                Assert.AreEqual("TEST_COPIED_NO_OPTIONS", foundCopiedFolder.Name);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_COPY_NO_OPTIONS");
            await CleanupMockFolderFromSharedDocuments(2, "TEST_COPIED_NO_OPTIONS");
        }

        [TestMethod]
        public async Task CopyFolderBatchWithoutOptionAsyncTest()
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
        public async Task CopyFolderBatchWithoutOptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_COPY_BATCH_NO_OPTIONS");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFolder folderToCopy = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/TEST_COPIED_BATCH_NO_OPTIONS";
                folderToCopy.CopyToBatch(destinationServerRelativeUrl);
                await context.ExecuteAsync();

                IFolder foundCopiedFolder = await context.Web.GetFolderByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundCopiedFolder);
                Assert.AreEqual("TEST_COPIED_BATCH_NO_OPTIONS", foundCopiedFolder.Name);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_COPY_BATCH_NO_OPTIONS");
            await CleanupMockFolderFromSharedDocuments(2, "TEST_COPIED_BATCH_NO_OPTIONS");
        }

        [TestMethod]
        public async Task CopyFolderSpecificBatchWithoutOptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_COPY_BATCH_NO_OPTIONS");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var newBatch = context.NewBatch();
                IFolder folderToCopy = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/TEST_COPIED_BATCH_NO_OPTIONS";
                folderToCopy.CopyToBatch(newBatch, destinationServerRelativeUrl);
                await context.ExecuteAsync(newBatch);

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
                IFolder sharedDocsRootFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
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
                IFolder sharedDocsRootFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
                IFolder foundCopiedFolder = await sharedDocsRootFolder.Folders.FirstOrDefaultAsync(f => f.Name == folderToFindName);
                Assert.IsNotNull(foundCopiedFolder);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_COPY_BATCH_OPTIONS");
            await CleanupMockFolderFromSharedDocuments(2, "TEST_COPY2_BATCH_OPTIONS");
            await CleanupMockFolderFromSharedDocuments(2, folderToFindName);
        }

        [TestMethod]
        public async Task MoveFolderWithoutOptionAsyncTest()
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
        public async Task MoveFolderWithoutOptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_MOVE_NO_OPTIONS");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFolder folderToMove = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/TEST_MOVED_FOLDER_NO_OPTIONS";
                folderToMove.MoveTo(destinationServerRelativeUrl);

                IFolder foundMovedFolder = await context.Web.GetFolderByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundMovedFolder);
                Assert.AreEqual("TEST_MOVED_FOLDER_NO_OPTIONS", foundMovedFolder.Name);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_MOVED_FOLDER_NO_OPTIONS");
        }

        [TestMethod]
        public async Task MoveFolderBatchAsyncWithoutOptionTest()
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
        public async Task MoveFolderBatchWithoutOptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_MOVE_BATCH_NO_OPTIONS");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFolder folderToMove = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/TEST_MOVED_FOLDER_BATCH_NO_OPTIONS";
                folderToMove.MoveToBatch(destinationServerRelativeUrl);
                await context.ExecuteAsync();

                IFolder foundMovedFolder = await context.Web.GetFolderByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundMovedFolder);
                Assert.AreEqual("TEST_MOVED_FOLDER_BATCH_NO_OPTIONS", foundMovedFolder.Name);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_MOVED_FOLDER_BATCH_NO_OPTIONS");
        }

        [TestMethod]
        public async Task MoveFolderSpecificBatchAsyncWithoutOptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_MOVE_BATCH_NO_OPTIONS");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFolder folderToMove = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);
                var newBatch = context.NewBatch();

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/TEST_MOVED_FOLDER_BATCH_NO_OPTIONS";
                await folderToMove.MoveToBatchAsync(newBatch, destinationServerRelativeUrl);
                await context.ExecuteAsync(newBatch);

                IFolder foundMovedFolder = await context.Web.GetFolderByServerRelativeUrlAsync(destinationServerRelativeUrl);
                Assert.IsNotNull(foundMovedFolder);
                Assert.AreEqual("TEST_MOVED_FOLDER_BATCH_NO_OPTIONS", foundMovedFolder.Name);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_MOVED_FOLDER_BATCH_NO_OPTIONS");
        }

        [TestMethod]
        public async Task MoveFolderSpecificBatchAWithoutOptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            string mockFolderServerRelativeUrl = await AddMockFolderToSharedDocuments(0, "TEST_MOVE_BATCH_NO_OPTIONS");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IFolder folderToMove = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolderServerRelativeUrl);
                var newBatch = context.NewBatch();

                string destinationServerRelativeUrl = $"{context.Uri.PathAndQuery}/Shared Documents/TEST_MOVED_FOLDER_BATCH_NO_OPTIONS";
                folderToMove.MoveToBatch(newBatch, destinationServerRelativeUrl);
                await context.ExecuteAsync(newBatch);

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

                IFolder sharedDocsRootFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
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

                IFolder sharedDocsRootFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
                IFolder foundMovedFolder = await sharedDocsRootFolder.Folders.FirstOrDefaultAsync(f => f.Name == folderToFindName);
                Assert.IsNotNull(foundMovedFolder);
            }

            await CleanupMockFolderFromSharedDocuments(2, "TEST_MOVE_EXISTING_BATCH_OPTIONS");
            await CleanupMockFolderFromSharedDocuments(2, folderToFindName);
        }

        [TestMethod]
        public async Task GetFolderChangesAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var folder = await context.Web.Folders.FirstOrDefaultAsync(f => f.Name == "SiteAssets");
                var changes = await folder.GetChangesAsync(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5,
                });

                Assert.IsNotNull(changes);
                Assert.IsTrue(changes.Count > 0);
            }
        }

        [TestMethod]
        public void GetFolderChangesTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var folder = context.Web.Folders.FirstOrDefault(f => f.Name == "SiteAssets");
                var changes = folder.GetChanges(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5,
                });

                Assert.IsNotNull(changes);
                Assert.IsTrue(changes.Count > 0);

                var changesBatch = folder.GetChangesBatch(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5,
                });

                Assert.IsFalse(changesBatch.IsAvailable);

                context.Execute();

                Assert.IsTrue(changesBatch.IsAvailable);
                Assert.IsTrue(changesBatch.Count > 0);
            }
        }

        [TestMethod]
        public async Task GetGraphIdsFromFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Get folder from web
                IFolder folder = await context.Web.Folders.FirstOrDefaultAsync(f => f.Name == "SiteAssets");
                Assert.IsNotNull(folder);

                (string driveId, string driveItemId) = await (folder as Folder).GetGraphIdsAsync();

                Assert.IsFalse(string.IsNullOrEmpty(driveId));
                Assert.IsFalse(string.IsNullOrEmpty(driveItemId));

                // Get folder from web with properties loaded
                await folder.LoadAsync(p => p.Properties);

                (driveId, driveItemId) = await (folder as Folder).GetGraphIdsAsync();

                Assert.IsFalse(string.IsNullOrEmpty(driveId));
                Assert.IsFalse(string.IsNullOrEmpty(driveItemId));

                // Get folder from list
                var list = await context.Web.Lists.GetByTitleAsync("Site Assets", p => p.RootFolder);

                (driveId, driveItemId) = await (list.RootFolder as Folder).GetGraphIdsAsync();

                Assert.IsFalse(string.IsNullOrEmpty(driveId));
                Assert.IsFalse(string.IsNullOrEmpty(driveItemId));

                // Sub folder from list root folder
                await list.RootFolder.LoadAsync(p => p.Folders);

                (driveId, driveItemId) = await (list.RootFolder.Folders.AsRequested().First() as Folder).GetGraphIdsAsync();

                Assert.IsFalse(string.IsNullOrEmpty(driveId));
                Assert.IsFalse(string.IsNullOrEmpty(driveItemId));
            }
        }

        #region Mock folder
        private async Task<string> AddMockFolderToSharedDocuments(int contextId, string folderName, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, contextId, testName))
            {
                IFolder folder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
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
