using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.QueryModel;
using System.Collections.Generic;

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
