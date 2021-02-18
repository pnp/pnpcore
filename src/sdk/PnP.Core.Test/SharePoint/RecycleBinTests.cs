using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.QueryModel;
using PnP.Core.Model;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class RecycleBinTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetWebRecycleBinItemTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // Load the recycle bin
                await context.Web.LoadAsync(w => w.RecycleBin);

                // Still convinced the FirstOrDefaultAsync should load the RecycleBin without the need to load it previously...
                IRecycleBinItem recycleBinItem = context.Web.RecycleBin.AsEnumerable().FirstOrDefault(item => item.Id == recycleBinItemId);

                Assert.IsNotNull(recycleBinItem);
                Assert.AreEqual(recycleBinItemId, recycleBinItem.Id);
                Assert.AreEqual(fileName, recycleBinItem.LeafName);
                Assert.AreEqual(fileName, recycleBinItem.Title);
                Assert.AreEqual(RecycleBinItemState.FirstStageRecycleBin, recycleBinItem.ItemState);
                Assert.AreEqual(RecycleBinItemType.File, recycleBinItem.ItemType);
                Assert.AreNotEqual("", recycleBinItem.AuthorEmail);
                Assert.AreNotEqual("", recycleBinItem.AuthorName);
                Assert.AreNotEqual("", recycleBinItem.DeletedByEmail);
                Assert.AreNotEqual("", recycleBinItem.DeletedByName);
                Assert.AreNotEqual(default, recycleBinItem.DeletedDate);
                Assert.AreNotEqual("", recycleBinItem.DeletedDateLocalFormatted);
                Assert.IsTrue(recycleBinItem.DirName.EndsWith("/Shared Documents"));
                Assert.AreNotEqual(0, recycleBinItem.Size);
            }

            await CleanupWebRecycleBinItem(2, recycleBinItemId);
        }

        [TestMethod]
        public async Task GetWebRecycleBinItemWithAuthorTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // Load the recycle bin
                await context.Web.LoadAsync(w => w.RecycleBin.QueryProperties(p => p.Author, p => p.Id, p => p.Title));
                IRecycleBinItem recycleBinItem = context.Web.RecycleBin.AsEnumerable().FirstOrDefault(item => item.Id == recycleBinItemId);

                // Still convinced the FirstOrDefaultAsync should load the RecycleBin without the need to load it previously...
                //IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.QueryProperties(p => p.Author, p => p.Id, p => p.Title).FirstOrDefaultAsync(item => item.Id == recycleBinItemId);

                Assert.IsNotNull(recycleBinItem);
                Assert.AreEqual(recycleBinItemId, recycleBinItem.Id);
                Assert.AreEqual(fileName, recycleBinItem.Title);
                Assert.IsTrue(recycleBinItem.Author.Requested);
                Assert.IsTrue(recycleBinItem.Author.Id > 0);
            }

            await CleanupWebRecycleBinItem(2, recycleBinItemId);
        }

        [TestMethod]
        public async Task GetRecycleBinItemByIdAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.GetByIdAsync(recycleBinItemId, r => r.LeafName);

                Assert.AreEqual(fileName, recycleBinItem.LeafName);
            }

            await CleanupSiteRecycleBinItem(2, recycleBinItemId);
        }

        #region Restore()
        [TestMethod]
        public async Task RestoreAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.FirstOrDefaultAsync(item => item.Id == recycleBinItemId);

                await recycleBinItem.RestoreAsync();

                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == fileName);

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual(fileName, documentToFind.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(2, fileName);
        }

        [TestMethod]
        public async Task RestoreTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.FirstOrDefaultAsync(item => item.Id == recycleBinItemId);

                recycleBinItem.Restore();

                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == fileName);

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual(fileName, documentToFind.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(2, fileName);
        }

        [TestMethod]
        public async Task RestoreBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.FirstOrDefaultAsync(item => item.Id == recycleBinItemId);

                var batch = context.NewBatch();
                await recycleBinItem.RestoreBatchAsync(batch);
                await context.ExecuteAsync(batch);

                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == fileName);

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual(fileName, documentToFind.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(2, fileName);
        }

        [TestMethod]
        public async Task RestoreBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.FirstOrDefaultAsync(item => item.Id == recycleBinItemId);

                var batch = context.NewBatch();
                recycleBinItem.RestoreBatch(batch);
                await context.ExecuteAsync(batch);

                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == fileName);

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual(fileName, documentToFind.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(2, fileName);
        }

        [TestMethod]
        public async Task RestoreCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.FirstOrDefaultAsync(item => item.Id == recycleBinItemId);

                await recycleBinItem.RestoreBatchAsync();
                await context.ExecuteAsync();

                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == fileName);

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual(fileName, documentToFind.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(2, fileName);
        }

        [TestMethod]
        public async Task RestoreCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.FirstOrDefaultAsync(item => item.Id == recycleBinItemId);

                recycleBinItem.RestoreBatch();
                await context.ExecuteAsync();

                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == fileName);

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual(fileName, documentToFind.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(2, fileName);
        }
        #endregion

        #region MoveToSecondStage()
        [TestMethod]
        public async Task MoveToSecondStageAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.FirstOrDefaultAsync(item => item.Id == recycleBinItemId);

                Assert.AreEqual(RecycleBinItemState.FirstStageRecycleBin, recycleBinItem.ItemState);

                // TODO Should the method set the ItemState property by itself or reload the item in order to prevent the need to reload or do nothing ?
                await recycleBinItem.MoveToSecondStageAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // Load the site collection recycle bin
                await context.Site.LoadAsync(w => w.RecycleBin.QueryProperties(p => p.Id, p => p.DeletedBy, p => p.ItemState));
                IRecycleBinItem recycleBinItem = context.Site.RecycleBin.AsEnumerable().FirstOrDefault(item => item.Id == recycleBinItemId);

                Assert.AreEqual(RecycleBinItemState.SecondStageRecycleBin, recycleBinItem.ItemState);
                Assert.IsTrue(recycleBinItem.DeletedBy.Requested);
                Assert.IsTrue(recycleBinItem.DeletedBy.Id > 0);
            }

            await CleanupSiteRecycleBinItem(3, recycleBinItemId);
        }

        [TestMethod]
        public async Task MoveToSecondStageTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.FirstOrDefaultAsync(item => item.Id == recycleBinItemId);

                Assert.AreEqual(RecycleBinItemState.FirstStageRecycleBin, recycleBinItem.ItemState);

                // TODO Should the method set the ItemState property by itself or reload the item in order to prevent the need to reload or do nothing ?
                recycleBinItem.MoveToSecondStage();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // Load the site collection recycle bin
                await context.Site.LoadAsync(w => w.RecycleBin);
                IRecycleBinItem recycleBinItem = context.Site.RecycleBin.AsEnumerable().FirstOrDefault(item => item.Id == recycleBinItemId);

                Assert.AreEqual(RecycleBinItemState.SecondStageRecycleBin, recycleBinItem.ItemState);
            }

            await CleanupSiteRecycleBinItem(3, recycleBinItemId);
        }

        [TestMethod]
        public async Task MoveToSecondStageBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.FirstOrDefaultAsync(item => item.Id == recycleBinItemId);

                Assert.AreEqual(RecycleBinItemState.FirstStageRecycleBin, recycleBinItem.ItemState);

                var batch = context.NewBatch();
                // TODO Should the method set the ItemState property by itself or reload the item in order to prevent the need to reload or do nothing ?
                await recycleBinItem.MoveToSecondStageBatchAsync(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // Load the site collection recycle bin
                await context.Site.LoadAsync(w => w.RecycleBin);
                IRecycleBinItem recycleBinItem = context.Site.RecycleBin.AsEnumerable().FirstOrDefault(item => item.Id == recycleBinItemId);

                Assert.AreEqual(RecycleBinItemState.SecondStageRecycleBin, recycleBinItem.ItemState);
            }

            await CleanupSiteRecycleBinItem(3, recycleBinItemId);
        }

        [TestMethod]
        public async Task MoveToSecondStageBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.FirstOrDefaultAsync(item => item.Id == recycleBinItemId);

                Assert.AreEqual(RecycleBinItemState.FirstStageRecycleBin, recycleBinItem.ItemState);

                var batch = context.NewBatch();
                // TODO Should the method set the ItemState property by itself or reload the item in order to prevent the need to reload or do nothing ?
                recycleBinItem.MoveToSecondStageBatch(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // Load the site collection recycle bin
                await context.Site.LoadAsync(w => w.RecycleBin);
                IRecycleBinItem recycleBinItem = context.Site.RecycleBin.AsEnumerable().FirstOrDefault(item => item.Id == recycleBinItemId);

                Assert.AreEqual(RecycleBinItemState.SecondStageRecycleBin, recycleBinItem.ItemState);
            }

            await CleanupSiteRecycleBinItem(3, recycleBinItemId);
        }

        [TestMethod]
        public async Task MoveToSecondStageCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.FirstOrDefaultAsync(item => item.Id == recycleBinItemId);

                Assert.AreEqual(RecycleBinItemState.FirstStageRecycleBin, recycleBinItem.ItemState);

                // TODO Should the method set the ItemState property by itself or reload the item in order to prevent the need to reload or do nothing ?
                await recycleBinItem.MoveToSecondStageBatchAsync();
                await context.ExecuteAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // Load the site collection recycle bin
                await context.Site.LoadAsync(w => w.RecycleBin);
                IRecycleBinItem recycleBinItem = context.Site.RecycleBin.AsEnumerable().FirstOrDefault(item => item.Id == recycleBinItemId);

                Assert.AreEqual(RecycleBinItemState.SecondStageRecycleBin, recycleBinItem.ItemState);
            }

            await CleanupSiteRecycleBinItem(3, recycleBinItemId);
        }

        [TestMethod]
        public async Task MoveToSecondStageCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.FirstOrDefaultAsync(item => item.Id == recycleBinItemId);

                Assert.AreEqual(RecycleBinItemState.FirstStageRecycleBin, recycleBinItem.ItemState);

                // TODO Should the method set the ItemState property by itself or reload the item in order to prevent the need to reload or do nothing ?
                recycleBinItem.MoveToSecondStageBatch();
                await context.ExecuteAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // Load the site collection recycle bin
                await context.Site.LoadAsync(w => w.RecycleBin);
                IRecycleBinItem recycleBinItem = context.Site.RecycleBin.AsEnumerable().FirstOrDefault(item => item.Id == recycleBinItemId);

                Assert.AreEqual(RecycleBinItemState.SecondStageRecycleBin, recycleBinItem.ItemState);
            }

            await CleanupSiteRecycleBinItem(3, recycleBinItemId);
        }
        #endregion

        #region DeleteAll()
        [TestMethod]
        public async Task DeleteAllAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Web.RecycleBin.Length > 0);

                await context.Web.RecycleBin.DeleteAllAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Web.RecycleBin.Length);
            }
        }

        [TestMethod]
        public async Task DeleteAllTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Web.RecycleBin.Length > 0);

                context.Web.RecycleBin.DeleteAll();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Web.RecycleBin.Length);
            }
        }

        [TestMethod]
        public async Task DeleteAllBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Web.RecycleBin.Length > 0);

                var batch = context.NewBatch();
                await context.Web.RecycleBin.DeleteAllBatchAsync(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Web.RecycleBin.Length);
            }
        }

        [TestMethod]
        public async Task DeleteAllBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Web.RecycleBin.Length > 0);

                var batch = context.NewBatch();
                context.Web.RecycleBin.DeleteAllBatch(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Web.RecycleBin.Length);
            }
        }

        [TestMethod]
        public async Task DeleteAllCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Web.RecycleBin.Length > 0);

                await context.Web.RecycleBin.DeleteAllBatchAsync();
                await context.ExecuteAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Web.RecycleBin.Length);
            }
        }

        [TestMethod]
        public async Task DeleteAllCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Web.RecycleBin.Length > 0);

                var batch = context.NewBatch();
                context.Web.RecycleBin.DeleteAllBatch(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Web.RecycleBin.Length);
            }
        }
        #endregion

        #region DeleteAllSecondStageItems()
        [TestMethod]
        public async Task DeleteAllSecondStageItemsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledToSecondStageDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // There is at least 1 item in second stage recycle bin
                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin) > 0);

                await context.Site.RecycleBin.DeleteAllSecondStageItemsAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // There is no more second stage recycle bin items
                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin));
            }
        }

        [TestMethod]
        public async Task DeleteAllSecondStageItemsTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledToSecondStageDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // There is at least 1 item in second stage recycle bin
                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin) > 0);

                context.Site.RecycleBin.DeleteAllSecondStageItems();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // There is no more second stage recycle bin items
                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin));
            }
        }

        [TestMethod]
        public async Task DeleteAllSecondStageItemBatchsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledToSecondStageDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // There is at least 1 item in second stage recycle bin
                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin) > 0);

                var batch = context.NewBatch();
                await context.Site.RecycleBin.DeleteAllSecondStageItemsBatchAsync(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // There is no more second stage recycle bin items
                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin));
            }
        }

        [TestMethod]
        public async Task DeleteAllSecondStageItemsBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledToSecondStageDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // There is at least 1 item in second stage recycle bin
                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin) > 0);

                var batch = context.NewBatch();
                context.Site.RecycleBin.DeleteAllSecondStageItemsBatch(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // There is no more second stage recycle bin items
                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin));
            }
        }

        [TestMethod]
        public async Task DeleteAllSecondStageItemCurrentBatchsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledToSecondStageDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // There is at least 1 item in second stage recycle bin
                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin) > 0);

                await context.Site.RecycleBin.DeleteAllSecondStageItemsBatchAsync();
                await context.ExecuteAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // There is no more second stage recycle bin items
                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin));
            }
        }

        [TestMethod]
        public async Task DeleteAllSecondStageItemsCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledToSecondStageDocument(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // There is at least 1 item in second stage recycle bin
                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin) > 0);

                context.Site.RecycleBin.DeleteAllSecondStageItemsBatch();
                await context.ExecuteAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // There is no more second stage recycle bin items
                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin));
            }
        }
        #endregion

        #region MoveAllToSecondStage

        [TestMethod]
        public async Task MoveAllToSecondStageAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0, clearFirst: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // There is at least 1 item in first stage recycle bin
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Web.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.FirstStageRecycleBin) > 0);

                await context.Web.RecycleBin.MoveAllToSecondStageAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // There is no more first stage recycle bin items
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Web.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.FirstStageRecycleBin));

                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                // The second stage recycle bin now contains items
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin) > 0);
            }

            await CleanupSiteRecycleBinItem(3, recycleBinItemId);
        }

        [TestMethod]
        public async Task MoveAllToSecondStageTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0, clearFirst: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // There is at least 1 item in first stage recycle bin
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Web.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.FirstStageRecycleBin) > 0);

                context.Web.RecycleBin.MoveAllToSecondStage();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // There is no more first stage recycle bin items
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Web.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.FirstStageRecycleBin));

                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                // The second stage recycle bin now contains items
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin) > 0);
            }

            await CleanupSiteRecycleBinItem(3, recycleBinItemId);
        }

        [TestMethod]
        public async Task MoveAllToSecondStageBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0, clearFirst: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // There is at least 1 item in first stage recycle bin
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Web.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.FirstStageRecycleBin) > 0);

                var batch = context.NewBatch();
                await context.Web.RecycleBin.MoveAllToSecondStageBatchAsync(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // There is no more first stage recycle bin items
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Web.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.FirstStageRecycleBin));

                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                // The second stage recycle bin now contains items
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin) > 0);
            }

            await CleanupSiteRecycleBinItem(3, recycleBinItemId);
        }

        [TestMethod]
        public async Task MoveAllToSecondStageBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0, clearFirst: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // There is at least 1 item in first stage recycle bin
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Web.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.FirstStageRecycleBin) > 0);

                var batch = context.NewBatch();
                context.Web.RecycleBin.MoveAllToSecondStageBatch(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // There is no more first stage recycle bin items
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Web.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.FirstStageRecycleBin));

                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                // The second stage recycle bin now contains items
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin) > 0);
            }

            await CleanupSiteRecycleBinItem(3, recycleBinItemId);
        }

        [TestMethod]
        public async Task MoveAllToSecondStageCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0, clearFirst: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // There is at least 1 item in first stage recycle bin
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Web.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.FirstStageRecycleBin) > 0);

                await context.Web.RecycleBin.MoveAllToSecondStageBatchAsync();
                await context.ExecuteAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // There is no more first stage recycle bin items
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Web.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.FirstStageRecycleBin));

                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                // The second stage recycle bin now contains items
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin) > 0);
            }

            await CleanupSiteRecycleBinItem(3, recycleBinItemId);
        }

        [TestMethod]
        public async Task MoveAllToSecondStageCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(0, clearFirst: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                // There is at least 1 item in first stage recycle bin
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Web.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.FirstStageRecycleBin) > 0);

                context.Web.RecycleBin.MoveAllToSecondStageBatch();
                await context.ExecuteAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                // There is no more first stage recycle bin items
                await context.Web.LoadAsync(w => w.RecycleBin);
                Assert.AreEqual(0, context.Web.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.FirstStageRecycleBin));

                // CAUTION, The second stage recycle bin is at the SITE COLLECTION LEVEL
                // The second stage recycle bin now contains items
                await context.Site.LoadAsync(w => w.RecycleBin);
                Assert.IsTrue(context.Site.RecycleBin.AsEnumerable().Count(r => r.ItemState == RecycleBinItemState.SecondStageRecycleBin) > 0);
            }

            await CleanupSiteRecycleBinItem(3, recycleBinItemId);
        }
        #endregion

        #region RestoreAll()
        [TestMethod]
        public async Task RestoreAllAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(1, clearFirst: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                await context.Web.RecycleBin.RestoreAllAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                // The recycle bin is empty
                Assert.AreEqual(0, context.Web.RecycleBin.Length);

                // The mock file has been restored
                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == fileName);

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual(fileName, documentToFind.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(3, fileName);
        }

        [TestMethod]
        public async Task RestoreAllTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(1, clearFirst: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                context.Web.RecycleBin.RestoreAll();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                // The recycle bin is empty
                Assert.AreEqual(0, context.Web.RecycleBin.Length);

                // The mock file has been restored
                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == fileName);

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual(fileName, documentToFind.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(3, fileName);
        }

        [TestMethod]
        public async Task RestoreAllBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(1, clearFirst: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var batch = context.NewBatch();
                await context.Web.RecycleBin.RestoreAllBatchAsync(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                // The recycle bin is empty
                Assert.AreEqual(0, context.Web.RecycleBin.Length);

                // The mock file has been restored
                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == fileName);

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual(fileName, documentToFind.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(3, fileName);
        }

        [TestMethod]
        public async Task RestoreAllBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(1, clearFirst: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var batch = context.NewBatch();
                context.Web.RecycleBin.RestoreAllBatch(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                // The recycle bin is empty
                Assert.AreEqual(0, context.Web.RecycleBin.Length);

                // The mock file has been restored
                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == fileName);

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual(fileName, documentToFind.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(3, fileName);
        }

        [TestMethod]
        public async Task RestoreAllCurrentBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(1, clearFirst: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                await context.Web.RecycleBin.RestoreAllBatchAsync();
                await context.ExecuteAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                // The recycle bin is empty
                Assert.AreEqual(0, context.Web.RecycleBin.Length);

                // The mock file has been restored
                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == fileName);

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual(fileName, documentToFind.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(3, fileName);
        }

        [TestMethod]
        public async Task RestoreAllCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            (Guid recycleBinItemId, string fileName) = await AddMockRecycledDocument(1, clearFirst: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                context.Web.RecycleBin.RestoreAllBatch();
                await context.ExecuteAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                await context.Web.LoadAsync(w => w.RecycleBin);
                // The recycle bin is empty
                Assert.AreEqual(0, context.Web.RecycleBin.Length);

                // The mock file has been restored
                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                IFile documentToFind = await sharedDocumentsFolder.Files.FirstOrDefaultAsync(f => f.Name == fileName);

                Assert.IsNotNull(documentToFind);
                Assert.AreEqual(fileName, documentToFind.Name);
            }

            await CleanupMockDocumentFromSharedDocuments(3, fileName);
        }

        #endregion

        #region testing asset documents/libraries helpers

        private async Task<Tuple<Guid, string>> AddMockRecycledDocument(int contextId,
            bool clearFirst = false,
              [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, contextId, testName))
            {
                context.GraphFirst = false;

                if (clearFirst)
                {
                    await context.Web.RecycleBin.DeleteAllAsync();
                }

                string fileName = $"{TestCommon.PnPCoreSDKTestPrefix}{testName}.docx";
                IFolder folder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();

                //var list = await context.Web.Lists.GetByTitleAsync("Documents");
                //IFolder folder = await list.RootFolder.GetAsync();

                IFile mockDocument = await folder.Files.AddAsync(fileName, System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                Guid id = await mockDocument.RecycleAsync();

                return new Tuple<Guid, string>(id, fileName);
            }
        }

        private async Task<Tuple<Guid, string>> AddMockRecycledToSecondStageDocument(int contextId,
             [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, contextId, testName))
            {
                string fileName = $"{TestCommon.PnPCoreSDKTestPrefix}{testName}.docx";
                IFolder folder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                IFile mockDocument = await folder.Files.AddAsync(fileName, System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                Guid id = await mockDocument.RecycleAsync();

                var webWithRecycleBin = await context.Web.GetAsync(w => w.RecycleBin);
                var recycleBinItem = webWithRecycleBin.RecycleBin.FirstOrDefault(r => r.Id == id);
                await recycleBinItem.MoveToSecondStageAsync();

                return new Tuple<Guid, string>(id, fileName);
            }
        }

        private async Task CleanupWebRecycleBinItem(int contextId, Guid recycleBinItemId, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, contextId, testName))
            {
                IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.FirstOrDefaultAsync(item => item.Id == recycleBinItemId);
                await recycleBinItem.DeleteAsync();
            }
        }

        private async Task CleanupSiteRecycleBinItem(int contextId, Guid recycleBinItemId, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, contextId, testName))
            {
                IRecycleBinItem recycleBinItem = await context.Site.RecycleBin.FirstOrDefaultAsync(item => item.Id == recycleBinItemId);
                await recycleBinItem.DeleteAsync();
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

            fileName = fileName.StartsWith($"{TestCommon.PnPCoreSDKTestPrefix}") ? fileName : $"{TestCommon.PnPCoreSDKTestPrefix}{fileName}";
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
