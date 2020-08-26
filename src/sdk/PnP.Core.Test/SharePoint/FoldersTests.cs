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
            TestCommon.Instance.Mocking = false;
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
            TestCommon.Instance.Mocking = false;
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
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IFolder parentFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFolder mockFolder = await parentFolder.Folders.AddAsync("TEST");

                IFolder foundFolder= await context.Web.Lists.GetByTitle("Documents").RootFolder.Folders.FirstOrDefaultAsync(f => f.Name == "TEST");

                Assert.IsNotNull(foundFolder);
                Assert.AreNotEqual(default, foundFolder.UniqueId);

                await mockFolder.DeleteAsync();
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
                IFolder newFolder = await parentFolder.Folders.AddAsync("TEST");

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
    }
}
