﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class ListItemTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task UpdateListItemWithUnderScoreField()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string listTitle = TestCommon.GetPnPSdkTestAssetName("UpdateListItemWithUnderScoreField");
                var myList = context.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                
                string fldText1 = "_SpecialField";
                if (myList == null)
                {
                    // Create the list
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);

                    // Text field 1                    
                    IField addedTextField1 = await myList.Fields.AddTextAsync(fldText1, new FieldTextOptions()
                    {
                        Group = "TEST GROUP",
                        AddToDefaultView = true,
                    });

                    // Add a list item to this list
                    // Add a list item
                    Dictionary<string, object> values = new Dictionary<string, object>
                    {
                        { "Title", "Yes" },
                        { fldText1, "No" }
                    };

                    await myList.Items.AddAsync(values);
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList != null)
                {
                    try
                    {
                        // get items from the list
                        await myList.LoadAsync(p => p.Items);

                        // grab first item
                        var firstItem = myList.Items.AsRequested().FirstOrDefault();
                        if (firstItem != null)
                        {
                            Assert.IsTrue(firstItem.Values["Title"].ToString() == "Yes");
                            Assert.IsTrue(firstItem.Values[fldText1].ToString() == "No");
                                                                                   
                            firstItem.Values["Title"] = "No";
                            firstItem.Values[fldText1] = "Noo";
                            // The values property should have changed
                            Assert.IsTrue(firstItem.HasChanged("Values"));
                            // Did the transientdictionary list changes
                            Assert.IsTrue(firstItem.Values.HasChanges);

                            await firstItem.UpdateAsync();

                            // get items again from the list
                            await myList.LoadAsync(p => p.Items);
                            firstItem = myList.Items.AsRequested().FirstOrDefault();

                            Assert.IsTrue(firstItem.Values["Title"].ToString() == "No");
                            Assert.IsTrue(firstItem.Values[fldText1].ToString() == "Noo");
                            Assert.IsFalse(firstItem.HasChanged("Values"));
                            Assert.IsFalse(firstItem.Values.HasChanges);
                        }
                    }
                    finally
                    {
                        // Clean up
                        await myList.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task AddListItemWithFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("AddListItemWithFolderTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                await list.EnsurePropertiesAsync(l => l.RootFolder);
                list.ContentTypesEnabled = true;
                list.EnableFolderCreation = true;
                list.Update();

                // Add item to root of the list
                var rootItem = list.Items.Add(new Dictionary<string, object> { { "Title", "root" } });
                var folderForRootItem = await rootItem.GetFolderAsync().ConfigureAwait(false);
                Assert.IsFalse(await rootItem.IsFolderAsync());
                Assert.IsFalse(await rootItem.IsFileAsync());

                var folderItem = await list.AddListFolderAsync("Test");
                var folderForFolderItem = await folderItem.GetFolderAsync().ConfigureAwait(false);
                Assert.IsTrue(folderForFolderItem != null);

                var item = list.Items.Add(new Dictionary<string, object> { { "Title", "blabla" } }, "Test");
                var newFolderItem = await list.Items.GetByIdAsync(folderItem.Id);
                Assert.IsTrue(newFolderItem.IsFolder());
                Assert.IsFalse(newFolderItem.IsFile());

                var folder = await item.GetFolderAsync().ConfigureAwait(false);
                Assert.IsTrue(folder.Name == "Test");

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var newList = await context.Web.Lists.GetByTitleAsync(listTitle, l => l.RootFolder);

                    var result = await newList.LoadListDataAsStreamAsync(new RenderListDataOptions() { ViewXml = "<View><ViewFields><FieldRef Name='Title' /><FieldRef Name='FileDirRef' /></ViewFields><RowLimit>1</RowLimit></View>", RenderOptions = RenderListDataOptionsFlags.ListData, FolderServerRelativeUrl = $"{newList.RootFolder.ServerRelativeUrl}/Test" });


                    Assert.IsTrue(newList.Items.AsRequested().FirstOrDefault() != null);

                }
                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("AddListWithFolderTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                await list.EnsurePropertiesAsync(l => l.RootFolder);
                list.ContentTypesEnabled = true;
                list.EnableFolderCreation = true;
                list.Update();

                var folderItem = await list.AddListFolderAsync("Test");
                var newFolderItem = await list.Items.GetByIdAsync(folderItem.Id);
                Assert.IsTrue(newFolderItem["ContentTypeId"].ToString().StartsWith("0x0120"));

                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFolder2Test()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("AddListWithFolder2Test");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                await list.EnsurePropertiesAsync(l => l.RootFolder);
                list.ContentTypesEnabled = true;
                list.EnableFolderCreation = true;
                list.Update();

                IListItem folderItem = list.Items.Add(new Dictionary<string, object>() { { "Title", "Folder" } }, string.Empty, FileSystemObjectType.Folder);
                var newFolderItem = await list.Items.GetByIdAsync(folderItem.Id);
                Assert.IsTrue(newFolderItem["ContentTypeId"].ToString().StartsWith("0x0120"));

                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddLibraryFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("AddLibraryFolderTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                await list.EnsurePropertiesAsync(l => l.RootFolder);
                list.ContentTypesEnabled = true;
                list.EnableFolderCreation = true;
                list.Update();

                var folderItem = await list.AddListFolderAsync("Test");
                var newFolderItem = await list.Items.GetByIdAsync(folderItem.Id);
                Assert.IsTrue(newFolderItem["ContentTypeId"].ToString().StartsWith("0x0120"));
                Assert.IsTrue(newFolderItem.IsFolder());

                var folder = await context.Web.GetFolderByServerRelativeUrlAsync($"{context.Uri.PathAndQuery}/{listTitle}/Test");

                // Add file into folder
                IFile testDocument = folder.Files.Add("test.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                // Load the connected listitem
                testDocument.Load(p => p.ListItemAllFields);
                Assert.IsTrue(testDocument.ListItemAllFields.IsFile());

                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task RecycleListItemTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("RecycleListItemTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                var item = await list.Items.AddAsync(new Dictionary<string, object> { { "Title", "Recycle me" } });
                var recycledGuid = await item.RecycleAsync();

                Assert.IsTrue(recycledGuid != Guid.Empty);
                var recycleBinItem = context.Web.RecycleBin.GetById(recycledGuid);
                Assert.IsNotNull(recycleBinItem);
                Assert.IsTrue(recycleBinItem.Requested);

                await recycleBinItem.DeleteAsync();
                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task RecycleListItemBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("RecycleListItemBatchTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                var item = await list.Items.AddAsync(new Dictionary<string, object> { { "Title", "Recycle me" } });
                
                var recycleBatchResponse = await item.RecycleBatchAsync();
                Assert.IsFalse(recycleBatchResponse.IsAvailable);
                await context.ExecuteAsync();
                Assert.IsTrue(recycleBatchResponse.IsAvailable);
                Assert.AreNotEqual(Guid.Empty, recycleBatchResponse.Result.Value);

                // Verify the item was removed from the list item collection
                Assert.IsTrue((item as ListItem).Deleted);
                Assert.IsTrue(list.Items.Length == 0);

                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task RecycleListItemCollectionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("RecycleListItemCollectionTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                var item = await list.Items.AddAsync(new Dictionary<string, object> { { "Title", "Recycle me" } });

                var recycledGuid = await list.Items.RecycleByIdAsync(item.Id);

                // Verify the item was removed from the list item collection
                Assert.IsTrue((item as ListItem).Deleted);
                Assert.IsTrue(list.Items.Length == 0);

                Assert.IsTrue(recycledGuid != Guid.Empty);
                var recycleBinItem = context.Web.RecycleBin.GetById(recycledGuid);
                Assert.IsNotNull(recycleBinItem);
                Assert.IsTrue(recycleBinItem.Requested);

                await recycleBinItem.DeleteAsync();
                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task RecycleListItemCollectionNotLoadedTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("RecycleListItemCollectionNotLoadedTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                var item = await list.Items.AddAsync(new Dictionary<string, object> { { "Title", "Recycle me" } });

                Guid recycledGuid = Guid.Empty;
                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = await context2.Web.Lists.GetByTitleAsync(listTitle);
                    recycledGuid = await list2.Items.RecycleByIdAsync(item.Id);
                }

                Assert.IsTrue(recycledGuid != Guid.Empty);
                var recycleBinItem = context.Web.RecycleBin.GetById(recycledGuid);
                Assert.IsNotNull(recycleBinItem);
                Assert.IsTrue(recycleBinItem.Requested);

                await recycleBinItem.DeleteAsync();
                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task RecycleListItemBatchCollectionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("RecycleListItemBatchCollectionTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                var item = await list.Items.AddAsync(new Dictionary<string, object> { { "Title", "Recycle me" } });

                await list.Items.RecycleByIdBatchAsync(item.Id);
                await context.ExecuteAsync();

                // Verify the item was removed from the list item collection
                Assert.IsTrue((item as ListItem).Deleted);
                Assert.IsTrue(list.Items.Length == 0);

                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task SystemUpdate()
        {
            //TestCommon.Instance.Mocking = false;
            string listTitle = "SystemUpdate";

            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    // Create a new list
                    var myList = context.Web.Lists.FirstOrDefault(p => p.Title == listTitle);

                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                        // Enable versioning
                        myList.EnableVersioning = true;
                        await myList.UpdateAsync();
                    }

                    // Add items to the list
                    for (int i = 0; i < 10; i++)
                    {
                        Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                        await myList.Items.AddBatchAsync(values);
                    }
                    await context.ExecuteAsync();

                    // get first item and do a system update
                    var first = myList.Items.AsRequested().First();

                    first.Title = "blabla";

                    await first.SystemUpdateAsync();
                }

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var myList2 = context2.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                    await myList2.LoadAsync(p => p.Items);

                    var first2 = myList2.Items.AsRequested().First();

                    // verify the list item was updated and that we're still at version 1.0
                    Assert.IsTrue(first2.Title == "blabla");
                    Assert.IsTrue(first2.Values["_UIVersionString"].ToString() == "1.0");

                    // do a regular update to bump the version again
                    first2.Title = "blabla2";
                    await first2.UpdateAsync();
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var myList3 = context3.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                    await myList3.LoadAsync(p => p.Items);

                    var first3 = myList3.Items.AsRequested().First();

                    // verify the list item was updated and that we're still at version 1.0
                    Assert.IsTrue(first3.Title == "blabla2");
                    Assert.IsTrue(first3.Values["_UIVersionString"].ToString() == "2.0");

                    // do a regular update to bump the version again
                    first3.Title = "blabla3";
                    await first3.SystemUpdateAsync();
                }

                using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
                {
                    var myList4 = context4.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                    await myList4.LoadAsync(p => p.Items);

                    var first4 = myList4.Items.AsRequested().First();

                    // verify the list item was updated and that we're still at version 2.0
                    Assert.IsTrue(first4.Title == "blabla3");
                    Assert.IsTrue(first4.Values["_UIVersionString"].ToString() == "2.0");
                }
            }
            finally
            {
                using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 4))
                {
                    var myList = contextFinal.Web.Lists.FirstOrDefault(p => p.Title == listTitle);

                    // Cleanup the created list
                    await myList.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task SystemUpdateBatchTests()
        {
            //TestCommon.Instance.Mocking = false;
            string listTitle = "SystemUpdateBatchTests";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var myList = context.Web.Lists.FirstOrDefault(p => p.Title == listTitle);

                #region Test Setup
                if (!TestCommon.Instance.Mocking && myList != null)
                {
                    // Cleanup the created list possibly from a previous run
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    // Enable versioning
                    myList.EnableVersioning = true;
                    await myList.UpdateAsync();
                }

                // Add items to the list
                for (int i = 0; i < 10; i++)
                {
                    Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                    await myList.Items.AddBatchAsync(values);
                }
                await context.ExecuteAsync();

                #endregion

                // get first item and do a system update
                var first = myList.Items.AsRequested().First();

                first.Title = "blabla";

                await first.SystemUpdateBatchAsync();
                await context.ExecuteAsync();
            }

            using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                context2.BatchClient.EnsureBatch();

                var myList2 = context2.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                await myList2.LoadAsync(p => p.Items);
                var first2 = myList2.Items.AsRequested().First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first2.Title == "blabla");
                Assert.IsTrue(first2.Values["_UIVersionString"].ToString() == "1.0");

                // do a regular update to bump the version again
                first2.Title = "blabla2";
                await first2.UpdateAsync();
            }

            using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                var myList3 = context3.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                await myList3.LoadAsync(p => p.Items);
                var first3 = myList3.Items.AsRequested().First();

                // verify the list item was updated and that we're still at version 2.0
                Assert.IsTrue(first3.Title == "blabla2");
                Assert.IsTrue(first3.Values["_UIVersionString"].ToString() == "2.0");

                // do a regular update to bump the version again
                first3.Title = "blabla3";
                first3.SystemUpdate();
                await context3.ExecuteAsync();
            }

            using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
            {
                var myList4 = context4.Web.Lists.FirstOrDefault(p => p.Title == listTitle); 
                await myList4.LoadAsync(p => p.Items);
                var first4 = myList4.Items.AsRequested().First();

                // verify the list item was updated and that we're still at version 2.0
                Assert.IsTrue(first4.Title == "blabla3");
                Assert.IsTrue(first4.Values["_UIVersionString"].ToString() == "2.0");

                // do a regular update to bump the version again
                first4.Title = "blabla4";
                var newBatch = context4.NewBatch();
                first4.SystemUpdateBatch(newBatch);
                await context4.ExecuteAsync(newBatch);
            }

            using (var context5 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 4))
            {
                var myList5 = context5.Web.Lists.FirstOrDefault(p => p.Title == listTitle); 
                myList5.Load(p => p.Items);
                var first5 = myList5.Items.AsRequested().First();

                // verify the list item was updated and that we're still at version 2.0
                Assert.IsTrue(first5.Title == "blabla4");
                Assert.IsTrue(first5.Values["_UIVersionString"].ToString() == "2.0");

                // do a regular update to bump the version again
                first5.Title = "blabla5";
                first5.SystemUpdateBatch();
                await context5.ExecuteAsync();

            }

            using (var context6 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 5))
            {
                var myList6 = context6.Web.Lists.FirstOrDefault(p => p.Title == listTitle); 
                myList6.Load(p => p.Items);
                var first6 = myList6.Items.AsRequested().First();

                // verify the list item was updated and that we're still at version 2.0
                Assert.IsTrue(first6.Title == "blabla5");
                Assert.IsTrue(first6.Values["_UIVersionString"].ToString() == "2.0");
            }

            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 6))
            {
                var web = await contextFinal.Web.GetAsync(p => p.Lists);
                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task SystemUpdateAsyncNoDataChangeTests()
        {
            //TestCommon.Instance.Mocking = false;
            string listTitle = "SystemUpdateAsyncNoDataChangeTests";

            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    #region Test Setup

                    var myList = context.Web.Lists.FirstOrDefault(p => p.Title == listTitle);

                    if (!TestCommon.Instance.Mocking && myList != null)
                    {
                        // Cleanup the created list possibly from a previous run
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                        // Enable versioning
                        myList.EnableVersioning = true;
                        await myList.UpdateAsync();
                    }

                    // Add items to the list
                    for (int i = 0; i < 10; i++)
                    {
                        Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                        await myList.Items.AddBatchAsync(values);
                    }
                    await context.ExecuteAsync();

                    #endregion

                    // get first item and do a system update
                    var first = myList.Items.AsRequested().First();

                    first.Title = "blabla";

                    await first.SystemUpdateBatchAsync();
                    await context.ExecuteAsync();

                }

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var myList2 = context2.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                    await myList2.LoadAsync(p => p.Items);

                    var first2 = myList2.Items.AsRequested().First();

                    // verify the list item was updated and that we're still at version 1.0
                    Assert.IsTrue(first2.Title == "blabla");
                    Assert.IsTrue(first2.Values["_UIVersionString"].ToString() == "1.0");

                    // do a regular update to bump the version again
                    first2.Title = "blabla2";
                    await first2.UpdateAsync();
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var myList3 = context3.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                    await myList3.LoadAsync(p => p.Items);

                    var first3 = myList3.Items.AsRequested().First();

                    // verify the list item was updated and that we're still at version 2.0
                    Assert.IsTrue(first3.Title == "blabla2");
                    Assert.IsTrue(first3.Values["_UIVersionString"].ToString() == "2.0");

                    // do a regular update to bump the version again
                    await first3.SystemUpdateAsync();
                }

                using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
                {
                    var myList4 = context4.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                    await myList4.LoadAsync(p => p.Items);

                    var first4 = myList4.Items.AsRequested().First();

                    // verify the list item was updated and that we're still at version 2.0
                    Assert.IsTrue(first4.Title == "blabla2");
                    Assert.IsTrue(first4.Values["_UIVersionString"].ToString() == "2.0");

                    // do a regular update to bump the version again
                    await first4.SystemUpdateBatchAsync();
                    await context4.ExecuteAsync();

                }

                using (var context5 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 4))
                {
                    var myList5 = context5.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                    await myList5.LoadAsync(p => p.Items);

                    var first5 = myList5.Items.AsRequested().First();

                    // verify the list item was updated and that we're still at version 2.0
                    Assert.IsTrue(first5.Title == "blabla2");
                    Assert.IsTrue(first5.Values["_UIVersionString"].ToString() == "2.0");
                }
            }
            finally
            {
                using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 5))
                {
                    var myList = contextFinal.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                    // Cleanup the created list
                    await myList.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task UpdateOverwriteVersion()
        {
            //TestCommon.Instance.Mocking = false;

            string listTitle = "UpdateOverwriteVersion";
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var myList = context.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    // Enable versioning
                    myList.EnableVersioning = true;
                    await myList.UpdateAsync();
                }

                // Add items to the list
                for (int i = 0; i < 10; i++)
                {
                    Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                    await myList.Items.AddBatchAsync(values);
                }
                await context.ExecuteAsync();

                // get first item and do a system update
                var first = myList.Items.AsRequested().First();

                first.Title = "blabla";

                // Use the batch update flow here
                var batch = context.NewBatch();
                await first.UpdateOverwriteVersionBatchAsync(batch).ConfigureAwait(false);
                await context.ExecuteAsync(batch);
            }
            using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var myList2 = context2.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                await myList2.LoadAsync(p => p.Items);

                var first2 = myList2.Items.AsRequested().First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first2.Title == "blabla");
                Assert.IsTrue(first2.Values["_UIVersionString"].ToString() == "1.0");
            }
            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                var myList = contextFinal.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                
                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdateOverwriteVersionBatchTests()
        {
            //TestCommon.Instance.Mocking = false;

            string listTitle = "UpdateOverwriteVersionBatchTests";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                #region Test Setup

                // Create a new list
                var myList = await context.Web.Lists.FirstOrDefaultAsync(p => p.Title == listTitle);

                if (!TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    // Enable versioning
                    myList.EnableVersioning = true;
                    await myList.UpdateAsync();
                }

                // Add items to the list
                for (int i = 0; i < 10; i++)
                {
                    Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                    await myList.Items.AddBatchAsync(values);
                }
                await context.ExecuteAsync();

                #endregion

                // get first item and do a system update
                var first = myList.Items.AsRequested().First();

                first.Title = "blabla";

                // Use the batch update flow here
                var batch = context.NewBatch();
                first.UpdateOverwriteVersionBatch(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var myList2 = await context2.Web.Lists.FirstOrDefaultAsync(p => p.Title == listTitle);
                await myList2.LoadAsync(p => p.Items);

                var first2 = myList2.Items.AsRequested().First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first2.Title == "blabla");
                Assert.IsTrue(first2.Values["_UIVersionString"].ToString() == "1.0");

                first2.Title = "blabla2";
                first2.UpdateOverwriteVersion();
            }

            using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                var myList3 = await context3.Web.Lists.FirstOrDefaultAsync(p => p.Title == listTitle);
                await myList3.LoadAsync(p => p.Items);

                var first3 = myList3.Items.AsRequested().First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first3.Title == "blabla2");
                Assert.IsTrue(first3.Values["_UIVersionString"].ToString() == "1.0");

                var batch3 = context3.NewBatch();
                first3.Title = "blabla3";
                first3.UpdateOverwriteVersionBatch(batch3);
                await context3.ExecuteAsync(batch3);
            }

            using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
            {
                var myList4 = await context4.Web.Lists.FirstOrDefaultAsync(p => p.Title == listTitle);
                await myList4.LoadAsync(p => p.Items);

                var first4 = myList4.Items.AsRequested().First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first4.Title == "blabla3");
                Assert.IsTrue(first4.Values["_UIVersionString"].ToString() == "1.0");

                first4.Title = "blabla4";
                first4.UpdateOverwriteVersionBatch();
                await context4.ExecuteAsync();
            }

            using (var context5 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 4))
            {
                var myList5 = await context5.Web.Lists.FirstOrDefaultAsync(p => p.Title == listTitle);
                await myList5.LoadAsync(p => p.Items);

                var first5 = myList5.Items.AsRequested().First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first5.Title == "blabla4");
                Assert.IsTrue(first5.Values["_UIVersionString"].ToString() == "1.0");
            }

            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 5))
            {
                var myList = await contextFinal.Web.Lists.FirstOrDefaultAsync(p => p.Title == listTitle);

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdateOverwriteVersionNoDataTests()
        {
            //TestCommon.Instance.Mocking = false;
            string listTitle = "UpdateOverwriteVersionNoDataTests";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                #region Test Setup

                var myList = await context.Web.Lists.FirstOrDefaultAsync(p => p.Title == listTitle);

                if (!TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    // Enable versioning
                    myList.EnableVersioning = true;
                    await myList.UpdateAsync();
                }

                // Add items to the list
                for (int i = 0; i < 10; i++)
                {
                    Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                    await myList.Items.AddBatchAsync(values);
                }
                await context.ExecuteAsync();

                #endregion

                // get first item and do a system update
                var first = myList.Items.AsRequested().First();

                first.Title = "blabla";

                // Use the batch update flow here
                var batch = context.NewBatch();
                first.UpdateOverwriteVersionBatch(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var myList2 = await context2.Web.Lists.FirstOrDefaultAsync(p => p.Title == listTitle);
                await myList2.LoadAsync(p => p.Items);

                var first2 = myList2.Items.AsRequested().First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first2.Title == "blabla");
                Assert.IsTrue(first2.Values["_UIVersionString"].ToString() == "1.0");

                await first2.UpdateOverwriteVersionAsync();
            }

            using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                var myList3 = await context3.Web.Lists.FirstOrDefaultAsync(p => p.Title == listTitle);
                await myList3.LoadAsync(p => p.Items);

                var first3 = myList3.Items.AsRequested().First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first3.Title == "blabla");
                Assert.IsTrue(first3.Values["_UIVersionString"].ToString() == "1.0");

                await first3.UpdateOverwriteVersionBatchAsync();
                await context3.ExecuteAsync();
            }

            using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
            {
                var myList4 = await context4.Web.Lists.FirstOrDefaultAsync(p => p.Title == listTitle);
                await myList4.LoadAsync(p => p.Items);

                var first4 = myList4.Items.AsRequested().First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first4.Title == "blabla");
                Assert.IsTrue(first4.Values["_UIVersionString"].ToString() == "1.0");
            }

            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 4))
            {
                // Create a new list
                var myList = await contextFinal.Web.Lists.FirstOrDefaultAsync(p => p.Title == listTitle);

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        #region Field type read/update tests

        internal class FieldData
        {
            internal FieldData(string type)
            {
                FieldType = type;
                Properties = new Dictionary<string, object>();
            }

            internal string FieldType { get; set; }
            internal Dictionary<string, object> Properties { get; set; }
        }

        [TestMethod]
        public async Task RegularRestUpdateTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Step 0: Data needed for the test run

                //==========================================================
                // Step 1: Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("RegularRestUpdateTest");
                var myList = await context.Web.Lists.GetByTitleAsync(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                //==========================================================
                // Step 2: Add special fields
                string fieldGroup = "TEST GROUP";

                // Text field 1
                string fldText1 = "Text1";
                IField addedTextField1 = await myList.Fields.AddTextAsync(fldText1, new FieldTextOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                // MultilineText field 1
                string fldMultilineText1 = "MultilineText1";
                IField addedMultilineTextField1 = await myList.Fields.AddMultilineTextAsync(fldMultilineText1, new FieldMultilineTextOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                // Boolean field 1
                string fldBool1 = "Bool1";
                IField addedBoolField1 = await myList.Fields.AddBooleanAsync(fldBool1, new FieldBooleanOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                // Number field 1
                string fldNumber1 = "Number1";
                IField addedNumberField1 = await myList.Fields.AddNumberAsync(fldNumber1, new FieldNumberOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                // DateTime field 1
                string fldDateTime1 = "DateTime1";
                IField addedDateTimeField1 = await myList.Fields.AddDateTimeAsync(fldDateTime1, new FieldDateTimeOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                // Currency field 1
                string fldCurrency1 = "Currency1";
                IField addedCurrencyField1 = await myList.Fields.AddCurrencyAsync(fldCurrency1, new FieldCurrencyOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                // Calculated field 1
                string fldCalculated1 = "Calculated1";
                IField addedCalculatedField1 = await myList.Fields.AddCalculatedAsync(fldCalculated1, new FieldCalculatedOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    Formula = @"=1-0.5",
                    OutputType = FieldType.Number,
                    ShowAsPercentage = true,
                });

                // Choice single field 1
                string fldChoiceSingle1 = "ChoiceSingle1";
                IField addedChoiceSingleField1 = await myList.Fields.AddChoiceAsync(fldChoiceSingle1, new FieldChoiceOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    Choices = new List<string>() { "Option A", "Option B", "Option C" }.ToArray(),
                    DefaultChoice = "Option B"
                });

                // Choice multi field 1
                string fldChoiceMulti1 = "ChoiceMulti1";
                IField addChoiceMultiField1 = await myList.Fields.AddChoiceMultiAsync(fldChoiceMulti1, new FieldChoiceOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    Choices = new List<string>() { "Option A", "Option B", "Option C", "Option D", "Option E" }.ToArray(),
                    DefaultChoice = "Option B"
                });

                //==========================================================
                // Step 3: Add a list item
                Dictionary<string, object> item = new Dictionary<string, object>()
                {
                    { "Title", "Item1" }
                };

                Dictionary<string, FieldData> fieldData = new Dictionary<string, FieldData>
                {
                    { fldText1, new FieldData("Text") },
                    { fldMultilineText1, new FieldData("MultilineText") },
                    { fldNumber1, new FieldData("Number") },
                    { fldBool1, new FieldData("Boolean") },
                    { fldDateTime1, new FieldData("DateTime") },
                    { fldCurrency1, new FieldData("Currency") },
                    { fldCalculated1, new FieldData("Calculated") },
                    { fldChoiceSingle1, new FieldData("Choice") },
                    { fldChoiceMulti1, new FieldData("ChoiceMulti") },
                };

                fieldData[fldText1].Properties.Add("Text", "PnP Rocks");
                item.Add(fldText1, fieldData[fldText1].Properties["Text"]);

                fieldData[fldMultilineText1].Properties.Add("Text", "PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks");
                item.Add(fldMultilineText1, fieldData[fldMultilineText1].Properties["Text"]);

                fieldData[fldNumber1].Properties.Add("Number", 67687);
                item.Add(fldNumber1, fieldData[fldNumber1].Properties["Number"]);

                fieldData[fldBool1].Properties.Add("Boolean", true);
                item.Add(fldBool1, fieldData[fldBool1].Properties["Boolean"]);

                DateTime baseDate = new DateTime(2020, 12, 6, 15, 25, 47);
                fieldData[fldDateTime1].Properties.Add("DateTime", baseDate);
                item.Add(fldDateTime1, fieldData[fldDateTime1].Properties["DateTime"]);

                fieldData[fldCurrency1].Properties.Add("Currency", 67.67);
                item.Add(fldCurrency1, fieldData[fldCurrency1].Properties["Currency"]);

                fieldData[fldChoiceSingle1].Properties.Add("Choice", "Option A");
                item.Add(fldChoiceSingle1, fieldData[fldChoiceSingle1].Properties["Choice"]);

                fieldData[fldChoiceMulti1].Properties.Add("Choice1", "Option A");
                fieldData[fldChoiceMulti1].Properties.Add("Choice2", "Option B");
                var choices = new List<string> { fieldData[fldChoiceMulti1].Properties["Choice1"].ToString(), fieldData[fldChoiceMulti1].Properties["Choice2"].ToString() };
                fieldData[fldChoiceMulti1].Properties.Add("Choices", choices);
                item.Add(fldChoiceMulti1, choices);

                // Add the configured list item
                var addedItem = await myList.Items.AddAsync(item);

                //==========================================================
                // Step 4: validate returned list item
                Assert.IsTrue(addedItem.Requested);
                Assert.IsTrue(addedItem["Title"].ToString() == "Item1");

                Assert.IsTrue(addedItem[fldText1] is string);
                Assert.IsTrue(addedItem[fldText1] == fieldData[fldText1].Properties["Text"]);

                Assert.IsTrue(addedItem[fldMultilineText1] is string);
                Assert.IsTrue(addedItem[fldMultilineText1] == fieldData[fldMultilineText1].Properties["Text"]);

                Assert.IsTrue(addedItem[fldNumber1] is int);
                Assert.IsTrue(addedItem[fldNumber1] == fieldData[fldNumber1].Properties["Number"]);

                Assert.IsTrue(addedItem[fldBool1] is bool);
                Assert.IsTrue(addedItem[fldBool1] == fieldData[fldBool1].Properties["Boolean"]);

                Assert.IsTrue(addedItem[fldDateTime1] is DateTime);
                Assert.IsTrue(addedItem[fldDateTime1] == fieldData[fldDateTime1].Properties["DateTime"]);

                Assert.IsTrue(addedItem[fldCurrency1] is double);
                Assert.IsTrue(addedItem[fldCurrency1] == fieldData[fldCurrency1].Properties["Currency"]);

                Assert.IsTrue(addedItem[fldChoiceSingle1] is string);
                Assert.IsTrue(addedItem[fldChoiceSingle1] == fieldData[fldChoiceSingle1].Properties["Choice"]);

                Assert.IsTrue(addedItem[fldChoiceMulti1] is List<string>);
                Assert.IsTrue(addedItem[fldChoiceMulti1] == fieldData[fldChoiceMulti1].Properties["Choices"]);

                //==========================================================
                // Step 5: Read list item using GetAsync approach and verify data was written correctly
                await VerifyRegularListItemViaGetAsync(2, listTitle, fieldData);

                //==========================================================
                // Step 6: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyRegularListItemViaGetListDataAsStreamAsync(3, listTitle, fieldData);

                //==========================================================
                // Step 7: Update item using CSOM UpdateOverwriteVersionAsync 

                fieldData[fldText1].Properties["Text"] = "22 PnP Rocks";
                addedItem[fldText1] = fieldData[fldText1].Properties["Text"];

                fieldData[fldMultilineText1].Properties["Text"] = "2222 PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks";
                addedItem[fldMultilineText1] = fieldData[fldMultilineText1].Properties["Text"];

                fieldData[fldNumber1].Properties["Number"] = 22222;
                addedItem[fldNumber1] = fieldData[fldNumber1].Properties["Number"];

                fieldData[fldBool1].Properties["Boolean"] = false;
                addedItem[fldBool1] = fieldData[fldBool1].Properties["Boolean"];

                fieldData[fldDateTime1].Properties["DateTime"] = baseDate.Subtract(new TimeSpan(10, 0, 0, 0));
                addedItem[fldDateTime1] = fieldData[fldDateTime1].Properties["DateTime"];

                fieldData[fldCurrency1].Properties["Currency"] = 22.22;
                addedItem[fldCurrency1] = fieldData[fldCurrency1].Properties["Currency"];

                fieldData[fldChoiceSingle1].Properties["Choice"] = "Option B";
                addedItem[fldChoiceSingle1] = fieldData[fldChoiceSingle1].Properties["Choice"];

                fieldData[fldChoiceMulti1].Properties.Add("Choice3", "Option C");
                var choices2 = new List<string> { fieldData[fldChoiceMulti1].Properties["Choice1"].ToString(), fieldData[fldChoiceMulti1].Properties["Choice2"].ToString(), fieldData[fldChoiceMulti1].Properties["Choice3"].ToString() };
                fieldData[fldChoiceMulti1].Properties["Choices"] = choices2;
                addedItem[fldChoiceMulti1] = fieldData[fldChoiceMulti1].Properties["Choices"];

                // Update list item
                await addedItem.UpdateAsync();

                //==========================================================
                // Step 8: Read list item using GetAsync approach and verify data was written correctly
                await VerifyRegularListItemViaGetAsync(4, listTitle, fieldData);

                //==========================================================
                // Step 9: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyRegularListItemViaGetListDataAsStreamAsync(5, listTitle, fieldData);

                //==========================================================
                // Step 10: Blank item using CSOM UpdateOverwriteVersionAsync 

                fieldData[fldText1].Properties["Text"] = "";
                addedItem[fldText1] = fieldData[fldText1].Properties["Text"];

                fieldData[fldMultilineText1].Properties["Text"] = "";
                addedItem[fldMultilineText1] = fieldData[fldMultilineText1].Properties["Text"];

                fieldData[fldNumber1].Properties["Number"] = 0;
                addedItem[fldNumber1] = fieldData[fldNumber1].Properties["Number"];

                fieldData[fldBool1].Properties["Boolean"] = false;
                addedItem[fldBool1] = fieldData[fldBool1].Properties["Boolean"];

                fieldData[fldDateTime1].Properties["DateTime"] = null;
                addedItem[fldDateTime1] = fieldData[fldDateTime1].Properties["DateTime"];

                fieldData[fldCurrency1].Properties["Currency"] = 0;
                addedItem[fldCurrency1] = fieldData[fldCurrency1].Properties["Currency"];

                fieldData[fldChoiceSingle1].Properties["Choice"] = "";
                addedItem[fldChoiceSingle1] = fieldData[fldChoiceSingle1].Properties["Choice"];

                var choices3 = new List<string>();
                fieldData[fldChoiceMulti1].Properties["Choices"] = choices3;
                addedItem[fldChoiceMulti1] = fieldData[fldChoiceMulti1].Properties["Choices"];

                // Update list item
                await addedItem.UpdateAsync();

                //==========================================================
                // Step 8: Read list item using GetAsync approach and verify data was written correctly
                await VerifyRegularListItemViaGetAsync(6, listTitle, fieldData);

                //==========================================================
                // Step 9: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyRegularListItemViaGetListDataAsStreamAsync(7, listTitle, fieldData);

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task RegularFieldCsomTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Step 0: Data needed for the test run

                //==========================================================
                // Step 1: Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("RegularFieldCsomTest");
                var myList = await context.Web.Lists.GetByTitleAsync(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                //==========================================================
                // Step 2: Add special fields
                string fieldGroup = "TEST GROUP";

                // Text field 1
                string fldText1 = "Text1";
                IField addedTextField1 = await myList.Fields.AddTextAsync(fldText1, new FieldTextOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                // MultilineText field 1
                string fldMultilineText1 = "MultilineText1";
                IField addedMultilineTextField1 = await myList.Fields.AddMultilineTextAsync(fldMultilineText1, new FieldMultilineTextOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                // Boolean field 1
                string fldBool1 = "Bool1";
                IField addedBoolField1 = await myList.Fields.AddBooleanAsync(fldBool1, new FieldBooleanOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                // Number field 1
                string fldNumber1 = "Number1";
                IField addedNumberField1 = await myList.Fields.AddNumberAsync(fldNumber1, new FieldNumberOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                // DateTime field 1
                string fldDateTime1 = "DateTime1";
                IField addedDateTimeField1 = await myList.Fields.AddDateTimeAsync(fldDateTime1, new FieldDateTimeOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                // Currency field 1
                string fldCurrency1 = "Currency1";
                IField addedCurrencyField1 = await myList.Fields.AddCurrencyAsync(fldCurrency1, new FieldCurrencyOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                // Calculated field 1
                string fldCalculated1 = "Calculated1";
                IField addedCalculatedField1 = await myList.Fields.AddCalculatedAsync(fldCalculated1, new FieldCalculatedOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    Formula = @"=1-0.5",
                    OutputType = FieldType.Number,
                    ShowAsPercentage = true,
                });

                // Choice single field 1
                string fldChoiceSingle1 = "ChoiceSingle1";
                IField addedChoiceSingleField1 = await myList.Fields.AddChoiceAsync(fldChoiceSingle1, new FieldChoiceOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    Choices = new List<string>() { "Option A", "Option B", "Option C" }.ToArray(),
                    DefaultChoice = "Option B"
                });

                // Choice multi field 1
                string fldChoiceMulti1 = "ChoiceMulti1";
                IField addChoiceMultiField1 = await myList.Fields.AddChoiceMultiAsync(fldChoiceMulti1, new FieldChoiceOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    Choices = new List<string>() { "Option A", "Option B", "Option C", "Option D", "Option E" }.ToArray(),
                    DefaultChoice = "Option B"
                });

                //==========================================================
                // Step 3: Add a list item
                Dictionary<string, object> item = new Dictionary<string, object>()
                {
                    { "Title", "Item1" }
                };

                Dictionary<string, FieldData> fieldData = new Dictionary<string, FieldData>
                {
                    { fldText1, new FieldData("Text") },
                    { fldMultilineText1, new FieldData("MultilineText") },
                    { fldNumber1, new FieldData("Number") },
                    { fldBool1, new FieldData("Boolean") },
                    { fldDateTime1, new FieldData("DateTime") },
                    { fldCurrency1, new FieldData("Currency") },
                    { fldCalculated1, new FieldData("Calculated") },
                    { fldChoiceSingle1, new FieldData("Choice") },
                    { fldChoiceMulti1, new FieldData("ChoiceMulti") },
                };

                fieldData[fldText1].Properties.Add("Text", "PnP Rocks");
                item.Add(fldText1, fieldData[fldText1].Properties["Text"]);

                fieldData[fldMultilineText1].Properties.Add("Text", "PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks");
                item.Add(fldMultilineText1, fieldData[fldMultilineText1].Properties["Text"]);

                fieldData[fldNumber1].Properties.Add("Number", 67687);
                item.Add(fldNumber1, fieldData[fldNumber1].Properties["Number"]);

                fieldData[fldBool1].Properties.Add("Boolean", true);
                item.Add(fldBool1, fieldData[fldBool1].Properties["Boolean"]);

                DateTime baseDate = new DateTime(2020, 12, 6, 8, 25, 47);
                fieldData[fldDateTime1].Properties.Add("DateTime", baseDate);
                item.Add(fldDateTime1, fieldData[fldDateTime1].Properties["DateTime"]);

                fieldData[fldCurrency1].Properties.Add("Currency", 67.67);
                item.Add(fldCurrency1, fieldData[fldCurrency1].Properties["Currency"]);

                fieldData[fldChoiceSingle1].Properties.Add("Choice", "Option A");
                item.Add(fldChoiceSingle1, fieldData[fldChoiceSingle1].Properties["Choice"]);

                fieldData[fldChoiceMulti1].Properties.Add("Choice1", "Option A");
                fieldData[fldChoiceMulti1].Properties.Add("Choice2", "Option B");
                var choices = new List<string> { fieldData[fldChoiceMulti1].Properties["Choice1"].ToString(), fieldData[fldChoiceMulti1].Properties["Choice2"].ToString() };
                fieldData[fldChoiceMulti1].Properties.Add("Choices", choices);
                item.Add(fldChoiceMulti1, choices);

                // Add the configured list item
                var addedItem = await myList.Items.AddAsync(item);

                //==========================================================
                // Step 4: validate returned list item
                Assert.IsTrue(addedItem.Requested);
                Assert.IsTrue(addedItem["Title"].ToString() == "Item1");

                Assert.IsTrue(addedItem[fldText1] is string);
                Assert.IsTrue(addedItem[fldText1] == fieldData[fldText1].Properties["Text"]);

                Assert.IsTrue(addedItem[fldMultilineText1] is string);
                Assert.IsTrue(addedItem[fldMultilineText1] == fieldData[fldMultilineText1].Properties["Text"]);

                Assert.IsTrue(addedItem[fldNumber1] is int);
                Assert.IsTrue(addedItem[fldNumber1] == fieldData[fldNumber1].Properties["Number"]);

                Assert.IsTrue(addedItem[fldBool1] is bool);
                Assert.IsTrue(addedItem[fldBool1] == fieldData[fldBool1].Properties["Boolean"]);

                Assert.IsTrue(addedItem[fldDateTime1] is DateTime);
                Assert.IsTrue(addedItem[fldDateTime1] == fieldData[fldDateTime1].Properties["DateTime"]);

                Assert.IsTrue(addedItem[fldCurrency1] is double);
                Assert.IsTrue(addedItem[fldCurrency1] == fieldData[fldCurrency1].Properties["Currency"]);

                Assert.IsTrue(addedItem[fldChoiceSingle1] is string);
                Assert.IsTrue(addedItem[fldChoiceSingle1] == fieldData[fldChoiceSingle1].Properties["Choice"]);

                Assert.IsTrue(addedItem[fldChoiceMulti1] is List<string>);
                Assert.IsTrue(addedItem[fldChoiceMulti1] == fieldData[fldChoiceMulti1].Properties["Choices"]);

                //==========================================================
                // Step 5: Read list item using GetAsync approach and verify data was written correctly
                await VerifyRegularListItemViaGetAsync(2, listTitle, fieldData);

                //==========================================================
                // Step 6: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyRegularListItemViaGetListDataAsStreamAsync(3, listTitle, fieldData);

                //==========================================================
                // Step 7: Update item using CSOM UpdateOverwriteVersionAsync 

                fieldData[fldText1].Properties["Text"] = "22 PnP Rocks";
                addedItem[fldText1] = fieldData[fldText1].Properties["Text"];

                fieldData[fldMultilineText1].Properties["Text"] = "2222 PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks...PnP Rocks";
                addedItem[fldMultilineText1] = fieldData[fldMultilineText1].Properties["Text"];

                fieldData[fldNumber1].Properties["Number"] = 22222;
                addedItem[fldNumber1] = fieldData[fldNumber1].Properties["Number"];

                fieldData[fldBool1].Properties["Boolean"] = false;
                addedItem[fldBool1] = fieldData[fldBool1].Properties["Boolean"];

                fieldData[fldDateTime1].Properties["DateTime"] = baseDate.Subtract(new TimeSpan(10, 0, 0, 0));
                addedItem[fldDateTime1] = fieldData[fldDateTime1].Properties["DateTime"];

                fieldData[fldCurrency1].Properties["Currency"] = 22.22;
                addedItem[fldCurrency1] = fieldData[fldCurrency1].Properties["Currency"];

                fieldData[fldChoiceSingle1].Properties["Choice"] = "Option B";
                addedItem[fldChoiceSingle1] = fieldData[fldChoiceSingle1].Properties["Choice"];

                fieldData[fldChoiceMulti1].Properties.Add("Choice3", "Option C");
                var choices2 = new List<string> { fieldData[fldChoiceMulti1].Properties["Choice1"].ToString(), fieldData[fldChoiceMulti1].Properties["Choice2"].ToString(), fieldData[fldChoiceMulti1].Properties["Choice3"].ToString() };
                fieldData[fldChoiceMulti1].Properties["Choices"] = choices2;
                addedItem[fldChoiceMulti1] = fieldData[fldChoiceMulti1].Properties["Choices"];

                // Update list item
                await addedItem.UpdateOverwriteVersionAsync();

                //==========================================================
                // Step 8: Read list item using GetAsync approach and verify data was written correctly
                await VerifyRegularListItemViaGetAsync(4, listTitle, fieldData);

                //==========================================================
                // Step 9: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyRegularListItemViaGetListDataAsStreamAsync(5, listTitle, fieldData);

                //==========================================================
                // Step 10: Blank item using CSOM UpdateOverwriteVersionAsync 

                fieldData[fldText1].Properties["Text"] = "";
                addedItem[fldText1] = fieldData[fldText1].Properties["Text"];

                fieldData[fldMultilineText1].Properties["Text"] = "";
                addedItem[fldMultilineText1] = fieldData[fldMultilineText1].Properties["Text"];

                fieldData[fldNumber1].Properties["Number"] = 0;
                addedItem[fldNumber1] = fieldData[fldNumber1].Properties["Number"];

                fieldData[fldBool1].Properties["Boolean"] = false;
                addedItem[fldBool1] = fieldData[fldBool1].Properties["Boolean"];

                fieldData[fldDateTime1].Properties["DateTime"] = null;
                addedItem[fldDateTime1] = fieldData[fldDateTime1].Properties["DateTime"];

                fieldData[fldCurrency1].Properties["Currency"] = 0;
                addedItem[fldCurrency1] = fieldData[fldCurrency1].Properties["Currency"];

                fieldData[fldChoiceSingle1].Properties["Choice"] = "";
                addedItem[fldChoiceSingle1] = fieldData[fldChoiceSingle1].Properties["Choice"];

                var choices3 = new List<string>();
                fieldData[fldChoiceMulti1].Properties["Choices"] = choices3;
                addedItem[fldChoiceMulti1] = fieldData[fldChoiceMulti1].Properties["Choices"];

                // Update list item
                await addedItem.UpdateOverwriteVersionAsync();

                //==========================================================
                // Step 8: Read list item using GetAsync approach and verify data was written correctly
                await VerifyRegularListItemViaGetAsync(6, listTitle, fieldData);

                //==========================================================
                // Step 9: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyRegularListItemViaGetListDataAsStreamAsync(7, listTitle, fieldData);

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        private static async Task<IListItem> VerifyRegularListItemViaGetListDataAsStreamAsync(int id, string listTitle, Dictionary<string, FieldData> fieldData, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, id, testName))
            {
                var myList = context.Web.Lists.GetByTitle(listTitle);                

                var listDataOptions = new RenderListDataOptions()
                {
                    RenderOptions = RenderListDataOptionsFlags.ListData,
                };

                var fieldsToLoad = new List<string>() { "Title" };
                foreach (var field in fieldData)
                {
                    fieldsToLoad.Add(field.Key);
                }

                listDataOptions.SetViewXmlFromFields(fieldsToLoad);

                await myList.LoadListDataAsStreamAsync(listDataOptions).ConfigureAwait(false);
                var addedItem = myList.Items.AsRequested().First();

                AssertRegularListItemProperties(fieldData, addedItem);

                return addedItem;
            }
        }

        private static async Task<IListItem> VerifyRegularListItemViaGetAsync(int id, string listTitle, Dictionary<string, FieldData> fieldData, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, id, testName))
            {
                context.GraphFirst = false;
                var myList = context.Web.Lists.GetByTitle(listTitle, p => p.Title, p => p.Items, p => p.Fields.QueryProperties(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title));
                var addedItem = myList.Items.AsRequested().FirstOrDefault(p => p.Title == "Item1");

                AssertRegularListItemProperties(fieldData, addedItem);

                return addedItem;
            }
        }

        private static void AssertRegularListItemProperties(Dictionary<string, FieldData> fieldData, IListItem addedItem)
        {
            Assert.IsTrue(addedItem.Requested);
            Assert.IsTrue(addedItem["Title"].ToString() == "Item1");

            foreach (var field in fieldData)
            {
                if (field.Value.FieldType == "Text")
                {
                    if (addedItem[field.Key] != null)
                    {
                        Assert.IsTrue(addedItem[field.Key] is string);
                        Assert.IsTrue(addedItem[field.Key].ToString() == field.Value.Properties["Text"].ToString());
                    }
                }
                else if (field.Value.FieldType == "MultilineText")
                {
                    if (addedItem[field.Key] != null)
                    {
                        Assert.IsTrue(addedItem[field.Key] is string);
                        Assert.IsTrue(addedItem[field.Key].ToString() == field.Value.Properties["Text"].ToString());
                    }
                }
                else if (field.Value.FieldType == "Number")
                {
                    if (addedItem[field.Key] is double)
                    {
                        Assert.IsTrue((double)addedItem[field.Key] == Convert.ToDouble(field.Value.Properties["Number"]));
                    }
                    else
                    {
                        Assert.IsTrue((int)addedItem[field.Key] == Convert.ToInt32(field.Value.Properties["Number"]));
                    }
                }
                else if (field.Value.FieldType == "Boolean")
                {
                    Assert.IsTrue(addedItem[field.Key] is bool);
                    Assert.IsTrue((bool)addedItem[field.Key] == (bool)field.Value.Properties["Boolean"]);
                }
                else if (field.Value.FieldType == "DateTime")
                {
                    if (addedItem[field.Key] != null)
                    {
                        DateTime server = ((DateTime)addedItem[field.Key]).ToUniversalTime();
                        DateTime expected = ((DateTime)field.Value.Properties["DateTime"]).ToUniversalTime();
                        Assert.IsTrue(server.Year == expected.Year);
                        Assert.IsTrue(server.Month == expected.Month);
                        Assert.IsTrue(server.Day == expected.Day);
                        //Assert.IsTrue(server.Hour == expected.Hour);
                        Assert.IsTrue(server.Minute == expected.Minute);
                        Assert.IsTrue(server.Second == expected.Second);
                    }
                }
                else if (field.Value.FieldType == "Currency")
                {
                    if (addedItem[field.Key] is double)
                    {
                        Assert.IsTrue((double)addedItem[field.Key] == Convert.ToDouble(field.Value.Properties["Currency"]));
                    }
                    else
                    {
                        Assert.IsTrue((int)addedItem[field.Key] == Convert.ToInt32(field.Value.Properties["Currency"]));
                    }
                }
                else if (field.Value.FieldType == "Calculated")
                {
                }
                else if (field.Value.FieldType == "Choice")
                {
                    if (addedItem[field.Key] != null)
                    {
                        Assert.IsTrue(addedItem[field.Key] is string);
                        Assert.IsTrue(addedItem[field.Key].ToString() == field.Value.Properties["Choice"].ToString());
                    }
                }
                else if (field.Value.FieldType == "ChoiceMulti")
                {
                    if (field.Value.Properties["Choices"] is List<string> choicesList && addedItem[field.Key] != null)
                    {
                        Assert.IsTrue(addedItem[field.Key] is List<string>);

                        foreach (var choice in choicesList)
                        {
                            Assert.IsTrue((addedItem[field.Key] as List<string>).Contains(choice));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task SpecialFieldRestUpdateTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Step 0: Data needed for the test run
                // Get current user
                var currentUser = await context.Web.GetCurrentUserAsync();
                // Get the principal representing two claims which are always available
                var userTwo = await context.Web.EnsureUserAsync("Everyone except external users");
                // Site pages library for lookup of the home page
                IList sitePages = await context.Web.Lists.GetByTitleAsync("Site Pages");
                // Taxonomy data ~ replace by creating term set once taxonomy APIs work again
                Guid termStore = new Guid("437b86fc-1258-45a9-85ea-87a29156ce3c");
                Guid termSet = new Guid("d50ec969-cb27-4a49-839f-3c25d1d607d5");
                Guid term1 = new Guid("108b34b1-87af-452d-be13-881a29477965");
                string label1 = "Dutch";
                Guid term2 = new Guid("8246e3c1-19ea-4b22-8ae3-df9cbc150a74");
                string label2 = "English";
                Guid term3 = new Guid("3f773e87-24c3-4d0d-a07f-96eb0c1e905e");
                string label3 = "French";

                //==========================================================
                // Step 1: Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("SpecialFieldRestUpdateTest");
                var myList = await context.Web.Lists.GetByTitleAsync(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                //==========================================================
                // Step 2: Add special fields
                string fieldGroup = "TEST GROUP";

                // URL field 1
                string fldUrl1 = "URLField1";
                IField addedUrlField1 = await myList.Fields.AddUrlAsync(fldUrl1, new FieldUrlOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    DisplayFormat = UrlFieldFormatType.Hyperlink
                });

                // URL field 2
                string fldUrl2 = "URLField2";
                IField addedUrlField2 = await myList.Fields.AddUrlAsync(fldUrl2, new FieldUrlOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    DisplayFormat = UrlFieldFormatType.Hyperlink
                });

                // User Single field 1
                string fldUserSingle1 = "UserSingleField1";
                IField addedUserSingleField1 = await myList.Fields.AddUserAsync(fldUserSingle1, new FieldUserOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
                });

                // User Multi field 1
                string fldUserMulti1 = "UserMultiField1";
                IField addedUserMultiField1 = await myList.Fields.AddUserMultiAsync(fldUserMulti1, new FieldUserOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
                });

                // Taxonomy field 1
                string fldTaxonomy1 = "TaxonomyField1";
                IField addedTaxonomyField1 = await myList.Fields.AddTaxonomyAsync(fldTaxonomy1, new FieldTaxonomyOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    TermStoreId = new Guid("437b86fc-1258-45a9-85ea-87a29156ce3c"),
                    TermSetId = new Guid("d50ec969-cb27-4a49-839f-3c25d1d607d5")
                });

                // Taxonomy Multi field 1
                string fldTaxonomyMulti1 = "TaxonomyMultiField1";
                IField addedTaxonomyMultiField1 = await myList.Fields.AddTaxonomyMultiAsync(fldTaxonomyMulti1, new FieldTaxonomyOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    TermStoreId = new Guid("437b86fc-1258-45a9-85ea-87a29156ce3c"),
                    TermSetId = new Guid("d50ec969-cb27-4a49-839f-3c25d1d607d5")
                });

                // Choice single field 1
                string fldChoiceSingle1 = "ChoiceSingle1";
                IField addChoiceSingleField1 = await myList.Fields.AddChoiceAsync(fldChoiceSingle1, new FieldChoiceOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    Choices = (new List<string>() { "Option A", "Option B", "Option C" }).ToArray(),
                    DefaultChoice = "Option B"
                });

                // Choice multi field 1
                string fldChoiceMulti1 = "ChoiceMulti1";
                IField addChoiceMultiField1 = await myList.Fields.AddChoiceMultiAsync(fldChoiceMulti1, new FieldChoiceOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    Choices = (new List<string>() { "Option A", "Option B", "Option C", "Option D", "Option E" }).ToArray(),
                    DefaultChoice = "Option B"
                });

                // Lookup single field 1
                string fldLookupSingle1 = "LookupSingleField1";
                IField addedLookupSingleField1 = await myList.Fields.AddLookupAsync(fldLookupSingle1, new FieldLookupOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    LookupListId = sitePages.Id,
                    LookupFieldName = "Title",
                });

                string fldLookupMulti1 = "LookupMultiField1";
                IField addedLookupMultiField1 = await myList.Fields.AddLookupMultiAsync(fldLookupMulti1, new FieldLookupOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    LookupListId = sitePages.Id,
                    LookupFieldName = "Title",
                });

                //==========================================================
                // Step 3: Add a list item
                Dictionary<string, object> item = new Dictionary<string, object>()
                {
                    { "Title", "Item1" }
                };

                Dictionary<string, FieldData> fieldData = new Dictionary<string, FieldData>
                {
                    // URL field 1
                    { fldUrl1, new FieldData("URL") },
                    // URL field 2
                    { fldUrl2, new FieldData("URL") },
                    // User single field 1
                    { fldUserSingle1, new FieldData("UserSingle") },
                    // User multi field 1
                    { fldUserMulti1, new FieldData("UserMulti") },
                    // Taxonomy single field 1
                    { fldTaxonomy1, new FieldData("TaxonomySingle") },
                    // Taxonomy multi field 1
                    { fldTaxonomyMulti1, new FieldData("TaxonomyMulti") },
                    // Lookup single field 1
                    { fldLookupSingle1, new FieldData("LookupSingle") },
                    // Lookup multi field 1
                    { fldLookupMulti1, new FieldData("LookupMulti") },
                };

                // URL field 1
                fieldData[fldUrl1].Properties.Add("Url", "https://pnp.com");
                fieldData[fldUrl1].Properties.Add("Description", "PnP Rocks");
                item.Add(fldUrl1, addedUrlField1.NewFieldUrlValue(fieldData[fldUrl1].Properties["Url"].ToString(), fieldData[fldUrl1].Properties["Description"].ToString()));

                // URL field 2 -  no description value set on create
                fieldData[fldUrl2].Properties.Add("Url", "https://pnp.com");
                // set the expected data equal to the url field as that's what we expect
                fieldData[fldUrl2].Properties.Add("Description", fieldData[fldUrl2].Properties["Url"]);
                item.Add(fldUrl2, addedUrlField2.NewFieldUrlValue(fieldData[fldUrl2].Properties["Url"].ToString()));

                // User single field 1
                fieldData[fldUserSingle1].Properties.Add("Principal", currentUser);
                item.Add(fldUserSingle1, addedUserSingleField1.NewFieldUserValue(currentUser));

                // User multi field 1
                var userCollection = addedUserMultiField1.NewFieldValueCollection();
                userCollection.Values.Add(addedUserMultiField1.NewFieldUserValue(currentUser));
                fieldData[fldUserMulti1].Properties.Add("Collection", userCollection);
                item.Add(fldUserMulti1, userCollection);

                // Taxonomy single field 1
                fieldData[fldTaxonomy1].Properties.Add("TermStore", termStore);
                fieldData[fldTaxonomy1].Properties.Add("TermSet", termSet);
                fieldData[fldTaxonomy1].Properties.Add("Term1", term1);
                fieldData[fldTaxonomy1].Properties.Add("Label1", label1);
                item.Add(fldTaxonomy1, addedTaxonomyField1.NewFieldTaxonomyValue((Guid)fieldData[fldTaxonomy1].Properties["Term1"], fieldData[fldTaxonomy1].Properties["Label1"].ToString()));

                // Taxonomy multi field 1
                fieldData[fldTaxonomyMulti1].Properties.Add("TermStore", termStore);
                fieldData[fldTaxonomyMulti1].Properties.Add("TermSet", termSet);
                fieldData[fldTaxonomyMulti1].Properties.Add("Term1", term1);
                fieldData[fldTaxonomyMulti1].Properties.Add("Label1", label1);
                fieldData[fldTaxonomyMulti1].Properties.Add("Term2", term2);
                fieldData[fldTaxonomyMulti1].Properties.Add("Label2", label2);
                fieldData[fldTaxonomyMulti1].Properties.Add("Term3", term3);
                fieldData[fldTaxonomyMulti1].Properties.Add("Label3", label3);

                // Use the option to specify a list of values in the constructor
                List<IFieldTaxonomyValue> taxonomyValues = new List<IFieldTaxonomyValue>
                {
                    addedTaxonomyMultiField1.NewFieldTaxonomyValue((Guid)fieldData[fldTaxonomyMulti1].Properties["Term1"], fieldData[fldTaxonomyMulti1].Properties["Label1"].ToString()),
                    addedTaxonomyMultiField1.NewFieldTaxonomyValue((Guid)fieldData[fldTaxonomyMulti1].Properties["Term2"], fieldData[fldTaxonomyMulti1].Properties["Label2"].ToString())
                };
                var termCollection = addedTaxonomyMultiField1.NewFieldValueCollection(taxonomyValues);
                fieldData[fldTaxonomyMulti1].Properties.Add("Collection", termCollection);
                item.Add(fldTaxonomyMulti1, termCollection);

                // Lookup single field 1
                fieldData[fldLookupSingle1].Properties.Add("LookupId", 1);
                item.Add(fldLookupSingle1, addedLookupSingleField1.NewFieldLookupValue((int)fieldData[fldLookupSingle1].Properties["LookupId"]));

                // Lookup multi field 1
                fieldData[fldLookupMulti1].Properties.Add("LookupId", 1);
                var lookupCollection = addedLookupMultiField1.NewFieldValueCollection();
                lookupCollection.Values.Add(addedLookupMultiField1.NewFieldLookupValue((int)fieldData[fldLookupMulti1].Properties["LookupId"]));
                fieldData[fldLookupMulti1].Properties.Add("Collection", lookupCollection);
                item.Add(fldLookupMulti1, lookupCollection);

                // Add the configured list item
                var addedItem = await myList.Items.AddAsync(item);

                //==========================================================
                // Step 4: validate returned list item
                Assert.IsTrue(addedItem.Requested);
                Assert.IsTrue(addedItem["Title"].ToString() == "Item1");

                // URL field 1
                Assert.IsTrue(addedItem[fldUrl1] is IFieldUrlValue);
                Assert.IsTrue((addedItem[fldUrl1] as IFieldUrlValue).Url == fieldData[fldUrl1].Properties["Url"].ToString());
                Assert.IsTrue((addedItem[fldUrl1] as IFieldUrlValue).Description == fieldData[fldUrl1].Properties["Description"].ToString());

                // URL field 2
                Assert.IsTrue(addedItem[fldUrl2] is IFieldUrlValue);
                Assert.IsTrue((addedItem[fldUrl2] as IFieldUrlValue).Url == fieldData[fldUrl2].Properties["Url"].ToString());
                Assert.IsTrue((addedItem[fldUrl2] as IFieldUrlValue).Description == fieldData[fldUrl2].Properties["Description"].ToString());

                // User single field 1
                Assert.IsTrue(addedItem[fldUserSingle1] is IFieldUserValue);
                Assert.IsTrue((addedItem[fldUserSingle1] as IFieldUserValue).LookupId == (fieldData[fldUserSingle1].Properties["Principal"] as ISharePointPrincipal).Id);

                // User multi field 1
                Assert.IsTrue(addedItem[fldUserMulti1] is IFieldValueCollection);
                Assert.IsTrue((addedItem[fldUserMulti1] as IFieldValueCollection).Values[0] == (fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection).Values[0]);

                // Taxonomy single field 1
                Assert.IsTrue(addedItem[fldTaxonomy1] is IFieldTaxonomyValue);
                Assert.IsTrue((addedItem[fldTaxonomy1] as IFieldTaxonomyValue).TermId == (Guid)fieldData[fldTaxonomy1].Properties["Term1"]);
                Assert.IsTrue((addedItem[fldTaxonomy1] as IFieldTaxonomyValue).Label == fieldData[fldTaxonomy1].Properties["Label1"].ToString());

                // Taxonomy multi field 1
                Assert.IsTrue(addedItem[fldTaxonomyMulti1] is IFieldValueCollection);
                Assert.IsTrue((addedItem[fldTaxonomyMulti1] as IFieldValueCollection).Values[0] == (fieldData[fldTaxonomyMulti1].Properties["Collection"] as IFieldValueCollection).Values[0]);
                Assert.IsTrue((addedItem[fldTaxonomyMulti1] as IFieldValueCollection).Values[1] == (fieldData[fldTaxonomyMulti1].Properties["Collection"] as IFieldValueCollection).Values[1]);

                // Lookup single field 1
                Assert.IsTrue(addedItem[fldLookupSingle1] is IFieldLookupValue);
                Assert.IsTrue((addedItem[fldLookupSingle1] as IFieldLookupValue).LookupId == (int)fieldData[fldLookupSingle1].Properties["LookupId"]);

                // Lookup multi field 1
                Assert.IsTrue(addedItem[fldLookupMulti1] is IFieldValueCollection);
                Assert.IsTrue((addedItem[fldLookupMulti1] as IFieldValueCollection).Values[0] == (fieldData[fldLookupMulti1].Properties["Collection"] as IFieldValueCollection).Values[0]);

                //==========================================================
                // Step 5: Read list item using GetAsync approach and verify data was written correctly
                await VerifyListItemViaGetAsync(2, listTitle, fieldData);

                //==========================================================
                // Step 6: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyListItemViaGetListDataAsStreamAsync(3, listTitle, fieldData);

                //==========================================================
                // Step 7: Update item using CSOM UpdateOverwriteVersionAsync 

                // URL field 1
                fieldData[fldUrl1].Properties["Url"] = $"{fieldData[fldUrl1].Properties["Url"]}/rocks";
                fieldData[fldUrl1].Properties["Description"] = $"{fieldData[fldUrl1].Properties["Description"]}A";
                (addedItem[fldUrl1] as IFieldUrlValue).Url = fieldData[fldUrl1].Properties["Url"].ToString();
                (addedItem[fldUrl1] as IFieldUrlValue).Description = fieldData[fldUrl1].Properties["Description"].ToString();

                // URL field 2
                fieldData[fldUrl2].Properties["Url"] = $"{fieldData[fldUrl2].Properties["Url"]}/rocks";
                (addedItem[fldUrl2] as IFieldUrlValue).Url = fieldData[fldUrl2].Properties["Url"].ToString();
                (addedItem[fldUrl2] as IFieldUrlValue).Description = fieldData[fldUrl2].Properties["Description"].ToString();

                // User single field 1
                fieldData[fldUserSingle1].Properties["Principal"] = userTwo;
                (addedItem[fldUserSingle1] as IFieldUserValue).Principal = fieldData[fldUserSingle1].Properties["Principal"] as ISharePointPrincipal;

                // User multi field2
                (fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection).Values.Add(addedUserMultiField1.NewFieldUserValue(userTwo));
                addedItem[fldUserMulti1] = fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection;

                // Taxonomy single field 1
                fieldData[fldTaxonomy1].Properties["Term1"] = term2;
                fieldData[fldTaxonomy1].Properties["Labe1"] = label2;
                (addedItem[fldTaxonomy1] as IFieldTaxonomyValue).TermId = (Guid)fieldData[fldTaxonomy1].Properties["Term1"];
                (addedItem[fldTaxonomy1] as IFieldTaxonomyValue).Label = fieldData[fldTaxonomy1].Properties["Label1"].ToString();

                // Taxonomy multi field 1
                (fieldData[fldTaxonomyMulti1].Properties["Collection"] as IFieldValueCollection).Values.Add(addedTaxonomyMultiField1.NewFieldTaxonomyValue((Guid)fieldData[fldTaxonomyMulti1].Properties["Term3"], fieldData[fldTaxonomyMulti1].Properties["Label3"].ToString()));
                addedItem[fldTaxonomyMulti1] = fieldData[fldTaxonomyMulti1].Properties["Collection"] as IFieldValueCollection;

                // Lookup single field 1
                fieldData[fldLookupSingle1].Properties["LookupId"] = 1;
                (addedItem[fldLookupSingle1] as IFieldLookupValue).LookupId = (int)fieldData[fldLookupSingle1].Properties["LookupId"];

                // Lookup multi field 1
                (fieldData[fldLookupMulti1].Properties["Collection"] as IFieldValueCollection).Values.Add(addedLookupMultiField1.NewFieldLookupValue(1));
                addedItem[fldLookupMulti1] = fieldData[fldLookupMulti1].Properties["Collection"] as IFieldValueCollection;

                // Update list item
                await addedItem.UpdateAsync();

                //==========================================================
                // Step 8: Read list item using GetAsync approach and verify data was written correctly
                await VerifyListItemViaGetAsync(4, listTitle, fieldData);

                //==========================================================
                // Step 9: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyListItemViaGetListDataAsStreamAsync(5, listTitle, fieldData);

                //==========================================================
                // Step 10: Blank item using CSOM UpdateOverwriteVersionAsync 

                // URL field 1
                fieldData[fldUrl1].Properties["Url"] = "";
                fieldData[fldUrl1].Properties["Description"] = "";
                (addedItem[fldUrl1] as IFieldUrlValue).Url = fieldData[fldUrl1].Properties["Url"].ToString();
                (addedItem[fldUrl1] as IFieldUrlValue).Description = fieldData[fldUrl1].Properties["Description"].ToString();

                // URL field 2
                fieldData[fldUrl2].Properties["Url"] = "";
                fieldData[fldUrl2].Properties["Description"] = "";
                (addedItem[fldUrl2] as IFieldUrlValue).Url = fieldData[fldUrl2].Properties["Url"].ToString();
                (addedItem[fldUrl2] as IFieldUrlValue).Description = fieldData[fldUrl2].Properties["Description"].ToString();

                // User single field 1
                fieldData[fldUserSingle1].Properties["Principal"] = null;
                (addedItem[fldUserSingle1] as IFieldUserValue).Principal = fieldData[fldUserSingle1].Properties["Principal"] as ISharePointPrincipal;

                // User multi field2
                (fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection).Values.Clear();
                addedItem[fldUserMulti1] = fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection;

                // Taxonomy single field 1
                fieldData[fldTaxonomy1].Properties["Term1"] = Guid.Empty;
                addedItem[fldTaxonomy1] = null;

                // Taxonomy multi field 1
                (fieldData[fldTaxonomyMulti1].Properties["Collection"] as IFieldValueCollection).Values.Clear();
                addedItem[fldTaxonomyMulti1] = fieldData[fldTaxonomyMulti1].Properties["Collection"] as IFieldValueCollection;

                // Lookup single field 1
                fieldData[fldLookupSingle1].Properties["LookupId"] = null;
                addedItem[fldLookupSingle1] = null;

                // Lookup multi field 1
                (fieldData[fldLookupMulti1].Properties["Collection"] as IFieldValueCollection).Values.Clear();
                addedItem[fldLookupMulti1] = fieldData[fldLookupMulti1].Properties["Collection"] as IFieldValueCollection;

                // Update list item
                await addedItem.UpdateAsync();

                //==========================================================
                // Step 8: Read list item using GetAsync approach and verify data was written correctly
                await VerifyListItemViaGetAsync(6, listTitle, fieldData);

                //==========================================================
                // Step 9: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyListItemViaGetListDataAsStreamAsync(7, listTitle, fieldData);

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task SpecialFieldCsomTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Step 0: Data needed for the test run
                // Get current user
                var currentUser = await context.Web.GetCurrentUserAsync();
                // Get the principal representing two claims which are always available
                var userTwo = await context.Web.EnsureUserAsync("Everyone except external users");
                // Site pages library for lookup of the home page
                IList sitePages = await context.Web.Lists.GetByTitleAsync("Site Pages");
                // Taxonomy data ~ replace by creating term set once taxonomy APIs work again
                Guid termStore = new Guid("437b86fc-1258-45a9-85ea-87a29156ce3c");
                Guid termSet = new Guid("d50ec969-cb27-4a49-839f-3c25d1d607d5");
                Guid term1 = new Guid("108b34b1-87af-452d-be13-881a29477965");
                string label1 = "Dutch";
                Guid term2 = new Guid("8246e3c1-19ea-4b22-8ae3-df9cbc150a74");
                string label2 = "English";
                Guid term3 = new Guid("3f773e87-24c3-4d0d-a07f-96eb0c1e905e");
                string label3 = "French";

                //==========================================================
                // Step 1: Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("SpecialFieldCsomTest");
                var myList = await context.Web.Lists.GetByTitleAsync(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                //==========================================================
                // Step 2: Add special fields
                string fieldGroup = "TEST GROUP";

                // URL field 1
                string fldUrl1 = "URLField1";
                IField addedUrlField1 = await myList.Fields.AddUrlAsync(fldUrl1, new FieldUrlOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    DisplayFormat = UrlFieldFormatType.Hyperlink
                });

                // URL field 2
                string fldUrl2 = "URLField2";
                IField addedUrlField2 = await myList.Fields.AddUrlAsync(fldUrl2, new FieldUrlOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    DisplayFormat = UrlFieldFormatType.Hyperlink
                });

                // User Single field 1
                string fldUserSingle1 = "UserSingleField1";
                IField addedUserSingleField1 = await myList.Fields.AddUserAsync(fldUserSingle1, new FieldUserOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
                });

                // User Multi field 1
                string fldUserMulti1 = "UserMultiField1";
                IField addedUserMultiField1 = await myList.Fields.AddUserMultiAsync(fldUserMulti1, new FieldUserOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
                });

                // Taxonomy field 1
                string fldTaxonomy1 = "TaxonomyField1";
                IField addedTaxonomyField1 = await myList.Fields.AddTaxonomyAsync(fldTaxonomy1, new FieldTaxonomyOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    TermStoreId = new Guid("437b86fc-1258-45a9-85ea-87a29156ce3c"),
                    TermSetId = new Guid("d50ec969-cb27-4a49-839f-3c25d1d607d5")
                });

                // Taxonomy Multi field 1
                string fldTaxonomyMulti1 = "TaxonomyMultiField1";
                IField addedTaxonomyMultiField1 = await myList.Fields.AddTaxonomyMultiAsync(fldTaxonomyMulti1, new FieldTaxonomyOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    TermStoreId = new Guid("437b86fc-1258-45a9-85ea-87a29156ce3c"),
                    TermSetId = new Guid("d50ec969-cb27-4a49-839f-3c25d1d607d5")
                });

                // Choice single field 1
                string fldChoiceSingle1 = "ChoiceSingle1";
                IField addChoiceSingleField1 = await myList.Fields.AddChoiceAsync(fldChoiceSingle1, new FieldChoiceOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    Choices = (new List<string>() { "Option A", "Option B", "Option C" }).ToArray(),
                    DefaultChoice = "Option B"
                });

                // Choice multi field 1
                string fldChoiceMulti1 = "ChoiceMulti1";
                IField addChoiceMultiField1 = await myList.Fields.AddChoiceMultiAsync(fldChoiceMulti1, new FieldChoiceOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    Choices = (new List<string>() { "Option A", "Option B", "Option C", "Option D", "Option E" }).ToArray(),
                    DefaultChoice = "Option B"
                });

                // Lookup single field 1
                string fldLookupSingle1 = "LookupSingleField1";
                IField addedLookupSingleField1 = await myList.Fields.AddLookupAsync(fldLookupSingle1, new FieldLookupOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    LookupListId = sitePages.Id,
                    LookupFieldName = "Title",
                });

                string fldLookupMulti1 = "LookupMultiField1";
                IField addedLookupMultiField1 = await myList.Fields.AddLookupMultiAsync(fldLookupMulti1, new FieldLookupOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                    LookupListId = sitePages.Id,
                    LookupFieldName = "Title",
                });

                //==========================================================
                // Step 3: Add a list item
                Dictionary<string, object> item = new Dictionary<string, object>()
                {
                    { "Title", "Item1" }
                };

                Dictionary<string, FieldData> fieldData = new Dictionary<string, FieldData>
                {
                    // URL field 1
                    { fldUrl1, new FieldData("URL") },
                    // URL field 2
                    { fldUrl2, new FieldData("URL") },
                    // User single field 1
                    { fldUserSingle1, new FieldData("UserSingle") },
                    // User multi field 1
                    { fldUserMulti1, new FieldData("UserMulti") },
                    // Taxonomy single field 1
                    { fldTaxonomy1, new FieldData("TaxonomySingle") },
                    // Taxonomy multi field 1
                    { fldTaxonomyMulti1, new FieldData("TaxonomyMulti") },
                    // Lookup single field 1
                    { fldLookupSingle1, new FieldData("LookupSingle") },
                    // Lookup multi field 1
                    { fldLookupMulti1, new FieldData("LookupMulti") },
                };

                // URL field 1
                fieldData[fldUrl1].Properties.Add("Url", "https://pnp.com");
                fieldData[fldUrl1].Properties.Add("Description", "PnP Rocks");
                item.Add(fldUrl1, addedUrlField1.NewFieldUrlValue(fieldData[fldUrl1].Properties["Url"].ToString(), fieldData[fldUrl1].Properties["Description"].ToString()));

                // URL field 2 -  no description value set on create
                fieldData[fldUrl2].Properties.Add("Url", "https://pnp.com");
                // set the expected data equal to the url field as that's what we expect
                fieldData[fldUrl2].Properties.Add("Description", fieldData[fldUrl2].Properties["Url"]);
                item.Add(fldUrl2, addedUrlField2.NewFieldUrlValue(fieldData[fldUrl2].Properties["Url"].ToString()));

                // User single field 1
                fieldData[fldUserSingle1].Properties.Add("Principal", currentUser);
                item.Add(fldUserSingle1, addedUserSingleField1.NewFieldUserValue(currentUser));

                // User multi field 1                
                var userCollection = addedUserMultiField1.NewFieldValueCollection();
                userCollection.Values.Add(addedUserMultiField1.NewFieldUserValue(currentUser));
                fieldData[fldUserMulti1].Properties.Add("Collection", userCollection);
                item.Add(fldUserMulti1, userCollection);

                // Taxonomy single field 1
                fieldData[fldTaxonomy1].Properties.Add("TermStore", termStore);
                fieldData[fldTaxonomy1].Properties.Add("TermSet", termSet);
                fieldData[fldTaxonomy1].Properties.Add("Term1", term1);
                fieldData[fldTaxonomy1].Properties.Add("Label1", label1);
                item.Add(fldTaxonomy1, addedTaxonomyField1.NewFieldTaxonomyValue((Guid)fieldData[fldTaxonomy1].Properties["Term1"], fieldData[fldTaxonomy1].Properties["Label1"].ToString()));

                // Taxonomy multi field 1
                fieldData[fldTaxonomyMulti1].Properties.Add("TermStore", termStore);
                fieldData[fldTaxonomyMulti1].Properties.Add("TermSet", termSet);
                fieldData[fldTaxonomyMulti1].Properties.Add("Term1", term1);
                fieldData[fldTaxonomyMulti1].Properties.Add("Label1", label1);
                fieldData[fldTaxonomyMulti1].Properties.Add("Term2", term2);
                fieldData[fldTaxonomyMulti1].Properties.Add("Label2", label2);
                fieldData[fldTaxonomyMulti1].Properties.Add("Term3", term3);
                fieldData[fldTaxonomyMulti1].Properties.Add("Label3", label3);

                List<KeyValuePair<Guid, string>> terms = new List<KeyValuePair<Guid, string>>
                {
                    new KeyValuePair<Guid, string>((Guid)fieldData[fldTaxonomyMulti1].Properties["Term1"], fieldData[fldTaxonomyMulti1].Properties["Label1"].ToString()),
                    new KeyValuePair<Guid, string>((Guid)fieldData[fldTaxonomyMulti1].Properties["Term2"], fieldData[fldTaxonomyMulti1].Properties["Label2"].ToString())
                };

                // Use the special constructor to get it covered
                var termCollection = addedTaxonomyMultiField1.NewFieldValueCollection(terms);
                //termCollection.Values.Add(addedTaxonomyMultiField1.NewFieldTaxonomyValue((Guid)fieldData[fldTaxonomyMulti1].Properties["Term1"], fieldData[fldTaxonomyMulti1].Properties["Label1"].ToString()));
                //termCollection.Values.Add(addedTaxonomyMultiField1.NewFieldTaxonomyValue((Guid)fieldData[fldTaxonomyMulti1].Properties["Term2"], fieldData[fldTaxonomyMulti1].Properties["Label2"].ToString()));
                fieldData[fldTaxonomyMulti1].Properties.Add("Collection", termCollection);
                item.Add(fldTaxonomyMulti1, termCollection);

                // Lookup single field 1
                fieldData[fldLookupSingle1].Properties.Add("LookupId", 1);
                item.Add(fldLookupSingle1, addedLookupSingleField1.NewFieldLookupValue((int)fieldData[fldLookupSingle1].Properties["LookupId"]));

                // Lookup multi field 1
                fieldData[fldLookupMulti1].Properties.Add("LookupId", 1);
                var lookupCollection = addedLookupMultiField1.NewFieldValueCollection();
                lookupCollection.Values.Add(addedLookupMultiField1.NewFieldLookupValue((int)fieldData[fldLookupMulti1].Properties["LookupId"]));
                fieldData[fldLookupMulti1].Properties.Add("Collection", lookupCollection);
                item.Add(fldLookupMulti1, lookupCollection);

                // Add the configured list item
                var addedItem = await myList.Items.AddAsync(item);

                //==========================================================
                // Step 4: validate returned list item
                Assert.IsTrue(addedItem.Requested);
                Assert.IsTrue(addedItem["Title"].ToString() == "Item1");

                // URL field 1
                Assert.IsTrue(addedItem[fldUrl1] is IFieldUrlValue);
                Assert.IsTrue((addedItem[fldUrl1] as IFieldUrlValue).Url == fieldData[fldUrl1].Properties["Url"].ToString());
                Assert.IsTrue((addedItem[fldUrl1] as IFieldUrlValue).Description == fieldData[fldUrl1].Properties["Description"].ToString());

                // URL field 2
                Assert.IsTrue(addedItem[fldUrl2] is IFieldUrlValue);
                Assert.IsTrue((addedItem[fldUrl2] as IFieldUrlValue).Url == fieldData[fldUrl2].Properties["Url"].ToString());
                Assert.IsTrue((addedItem[fldUrl2] as IFieldUrlValue).Description == fieldData[fldUrl2].Properties["Description"].ToString());

                // User single field 1
                Assert.IsTrue(addedItem[fldUserSingle1] is IFieldUserValue);
                Assert.IsTrue((addedItem[fldUserSingle1] as IFieldUserValue).LookupId == (fieldData[fldUserSingle1].Properties["Principal"] as ISharePointPrincipal).Id);

                // User multi field 1
                Assert.IsTrue(addedItem[fldUserMulti1] is IFieldValueCollection);
                Assert.IsTrue((addedItem[fldUserMulti1] as IFieldValueCollection).Values[0] == (fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection).Values[0]);

                // Taxonomy single field 1
                Assert.IsTrue(addedItem[fldTaxonomy1] is IFieldTaxonomyValue);
                Assert.IsTrue((addedItem[fldTaxonomy1] as IFieldTaxonomyValue).TermId == (Guid)fieldData[fldTaxonomy1].Properties["Term1"]);
                Assert.IsTrue((addedItem[fldTaxonomy1] as IFieldTaxonomyValue).Label == fieldData[fldTaxonomy1].Properties["Label1"].ToString());

                // Taxonomy multi field 1
                Assert.IsTrue(addedItem[fldTaxonomyMulti1] is IFieldValueCollection);
                Assert.IsTrue((addedItem[fldTaxonomyMulti1] as IFieldValueCollection).Values[0] == (fieldData[fldTaxonomyMulti1].Properties["Collection"] as IFieldValueCollection).Values[0]);
                Assert.IsTrue((addedItem[fldTaxonomyMulti1] as IFieldValueCollection).Values[1] == (fieldData[fldTaxonomyMulti1].Properties["Collection"] as IFieldValueCollection).Values[1]);

                // Lookup single field 1
                Assert.IsTrue(addedItem[fldLookupSingle1] is IFieldLookupValue);
                Assert.IsTrue((addedItem[fldLookupSingle1] as IFieldLookupValue).LookupId == (int)fieldData[fldLookupSingle1].Properties["LookupId"]);

                // Lookup multi field 1
                Assert.IsTrue(addedItem[fldLookupMulti1] is IFieldValueCollection);
                Assert.IsTrue((addedItem[fldLookupMulti1] as IFieldValueCollection).Values[0] == (fieldData[fldLookupMulti1].Properties["Collection"] as IFieldValueCollection).Values[0]);

                //==========================================================
                // Step 5: Read list item using GetAsync approach and verify data was written correctly
                await VerifyListItemViaGetAsync(2, listTitle, fieldData);

                //==========================================================
                // Step 6: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyListItemViaGetListDataAsStreamAsync(3, listTitle, fieldData);

                //==========================================================
                // Step 7: Update item using CSOM UpdateOverwriteVersionAsync 

                // URL field 1
                fieldData[fldUrl1].Properties["Url"] = $"{fieldData[fldUrl1].Properties["Url"]}/rocks";
                fieldData[fldUrl1].Properties["Description"] = $"{fieldData[fldUrl1].Properties["Description"]}A";
                (addedItem[fldUrl1] as IFieldUrlValue).Url = fieldData[fldUrl1].Properties["Url"].ToString();
                (addedItem[fldUrl1] as IFieldUrlValue).Description = fieldData[fldUrl1].Properties["Description"].ToString();

                // URL field 2
                fieldData[fldUrl2].Properties["Url"] = $"{fieldData[fldUrl2].Properties["Url"]}/rocks";
                (addedItem[fldUrl2] as IFieldUrlValue).Url = fieldData[fldUrl2].Properties["Url"].ToString();
                (addedItem[fldUrl2] as IFieldUrlValue).Description = fieldData[fldUrl2].Properties["Description"].ToString();

                // User single field 1
                fieldData[fldUserSingle1].Properties["Principal"] = userTwo;
                (addedItem[fldUserSingle1] as IFieldUserValue).Principal = fieldData[fldUserSingle1].Properties["Principal"] as ISharePointPrincipal;

                // User multi field2
                // Load via just user ID to test this constructor
                (fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection).Values.Add(addedUserMultiField1.NewFieldUserValue(userTwo.Id));
                addedItem[fldUserMulti1] = fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection;

                // Taxonomy single field 1
                fieldData[fldTaxonomy1].Properties["Term1"] = term2;
                fieldData[fldTaxonomy1].Properties["Labe1"] = label2;
                (addedItem[fldTaxonomy1] as IFieldTaxonomyValue).TermId = (Guid)fieldData[fldTaxonomy1].Properties["Term1"];
                (addedItem[fldTaxonomy1] as IFieldTaxonomyValue).Label = fieldData[fldTaxonomy1].Properties["Label1"].ToString();

                // Taxonomy multi field 1
                (fieldData[fldTaxonomyMulti1].Properties["Collection"] as IFieldValueCollection).Values.Add(addedTaxonomyMultiField1.NewFieldTaxonomyValue((Guid)fieldData[fldTaxonomyMulti1].Properties["Term3"], fieldData[fldTaxonomyMulti1].Properties["Label3"].ToString()));
                addedItem[fldTaxonomyMulti1] = fieldData[fldTaxonomyMulti1].Properties["Collection"] as IFieldValueCollection;

                // Lookup single field 1
                fieldData[fldLookupSingle1].Properties["LookupId"] = 1;
                (addedItem[fldLookupSingle1] as IFieldLookupValue).LookupId = (int)fieldData[fldLookupSingle1].Properties["LookupId"];

                // Lookup multi field 1
                (fieldData[fldLookupMulti1].Properties["Collection"] as IFieldValueCollection).Values.Add(addedLookupMultiField1.NewFieldLookupValue(1));
                addedItem[fldLookupMulti1] = fieldData[fldLookupMulti1].Properties["Collection"] as IFieldValueCollection;

                // Update list item
                await addedItem.UpdateOverwriteVersionAsync();

                //==========================================================
                // Step 8: Read list item using GetAsync approach and verify data was written correctly
                await VerifyListItemViaGetAsync(4, listTitle, fieldData);

                //==========================================================
                // Step 9: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyListItemViaGetListDataAsStreamAsync(5, listTitle, fieldData);

                //==========================================================
                // Step 10: Blank item using CSOM UpdateOverwriteVersionAsync 

                // URL field 1
                fieldData[fldUrl1].Properties["Url"] = "";
                fieldData[fldUrl1].Properties["Description"] = "";
                (addedItem[fldUrl1] as IFieldUrlValue).Url = fieldData[fldUrl1].Properties["Url"].ToString();
                (addedItem[fldUrl1] as IFieldUrlValue).Description = fieldData[fldUrl1].Properties["Description"].ToString();

                // URL field 2
                fieldData[fldUrl2].Properties["Url"] = "";
                fieldData[fldUrl2].Properties["Description"] = "";
                (addedItem[fldUrl2] as IFieldUrlValue).Url = fieldData[fldUrl2].Properties["Url"].ToString();
                (addedItem[fldUrl2] as IFieldUrlValue).Description = fieldData[fldUrl2].Properties["Description"].ToString();

                // User single field 1
                fieldData[fldUserSingle1].Properties["Principal"] = null;
                (addedItem[fldUserSingle1] as IFieldUserValue).Principal = fieldData[fldUserSingle1].Properties["Principal"] as ISharePointPrincipal;

                // User multi field2
                (fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection).Values.Clear();
                addedItem[fldUserMulti1] = fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection;

                // Taxonomy single field 1
                fieldData[fldTaxonomy1].Properties["Term1"] = Guid.Empty;
                addedItem[fldTaxonomy1] = null;

                // Taxonomy multi field 1
                (fieldData[fldTaxonomyMulti1].Properties["Collection"] as IFieldValueCollection).Values.Clear();
                addedItem[fldTaxonomyMulti1] = fieldData[fldTaxonomyMulti1].Properties["Collection"] as IFieldValueCollection;

                // Lookup single field 1
                fieldData[fldLookupSingle1].Properties["LookupId"] = null;
                addedItem[fldLookupSingle1] = null;

                // Lookup multi field 1
                (fieldData[fldLookupMulti1].Properties["Collection"] as IFieldValueCollection).Values.Clear();
                addedItem[fldLookupMulti1] = fieldData[fldLookupMulti1].Properties["Collection"] as IFieldValueCollection;

                // Update list item
                await addedItem.UpdateOverwriteVersionAsync();

                //==========================================================
                // Step 8: Read list item using GetAsync approach and verify data was written correctly
                await VerifyListItemViaGetAsync(6, listTitle, fieldData);

                //==========================================================
                // Step 9: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyListItemViaGetListDataAsStreamAsync(7, listTitle, fieldData);

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        private static void AssertListItemProperties(Dictionary<string, FieldData> fieldData, IListItem addedItem)
        {
            Assert.IsTrue(addedItem.Requested);
            Assert.IsTrue(addedItem["Title"].ToString() == "Item1");

            foreach (var field in fieldData)
            {
                if (field.Value.FieldType == "URL")
                {
                    if (field.Value.Properties["Url"].ToString() == "")
                    {
                        Assert.IsTrue(addedItem[field.Key] == null);
                    }
                    else
                    {
                        Assert.IsTrue(addedItem[field.Key] is IFieldUrlValue);
                        Assert.IsTrue((addedItem[field.Key] as IFieldUrlValue).Url == field.Value.Properties["Url"].ToString());
                        Assert.IsTrue((addedItem[field.Key] as IFieldUrlValue).Description == field.Value.Properties["Description"].ToString());
                    }
                }
                else if (field.Value.FieldType == "UserSingle")
                {
                    Assert.IsTrue(addedItem[field.Key] is IFieldUserValue);
                    int idToCheck;
                    if (field.Value.Properties.ContainsKey("Principal"))
                    {
                        if ((field.Value.Properties["Principal"] as ISharePointPrincipal) == null)
                        {
                            idToCheck = -1;
                        }
                        else
                        {
                            idToCheck = (field.Value.Properties["Principal"] as ISharePointPrincipal).Id;
                        }
                    }
                    else
                    {
                        idToCheck = (int)field.Value.Properties["UserId"];
                    }
                    Assert.IsTrue((addedItem[field.Key] as IFieldUserValue).LookupId == idToCheck);
                }
                else if (field.Value.FieldType == "UserMulti")
                {
                    Assert.IsTrue(addedItem[field.Key] is IFieldValueCollection);

                    var expectedUsers = (field.Value.Properties["Collection"] as IFieldValueCollection).Values.Cast<IFieldUserValue>();

                    foreach (var user in (addedItem[field.Key] as IFieldValueCollection).Values)
                    {
                        Assert.IsTrue(user is IFieldUserValue);
                        // is this user in the list of expected users
                        var expectedUser = expectedUsers.FirstOrDefault(p => p.LookupId == (user as IFieldUserValue).LookupId);
                        Assert.IsTrue(expectedUser != null);
                    }
                }
                else if (field.Value.FieldType == "TaxonomySingle")
                {
                    if ((Guid)field.Value.Properties["Term1"] == Guid.Empty)
                    {
                        Assert.IsTrue(addedItem[field.Key] == null);
                    }
                    else
                    {
                        Assert.IsTrue(addedItem[field.Key] is IFieldTaxonomyValue);
                        Assert.IsTrue((addedItem[field.Key] as IFieldTaxonomyValue).TermId == (Guid)field.Value.Properties["Term1"]);
                        // Label value is not returned as ID to the hidden tax list when returning a single value taxonomy field using a regular get()
                        //Assert.IsTrue((addedItem[field.Key] as IFieldTaxonomyValue).Label == field.Value.Properties["Label1"].ToString());
                    }
                }
                else if (field.Value.FieldType == "TaxonomyMulti")
                {
                    Assert.IsTrue(addedItem[field.Key] is IFieldValueCollection);

                    var expectedTerms = (field.Value.Properties["Collection"] as IFieldValueCollection).Values.Cast<IFieldTaxonomyValue>();

                    foreach (var term in (addedItem[field.Key] as IFieldValueCollection).Values)
                    {
                        Assert.IsTrue(term is IFieldTaxonomyValue);
                        // is this term in the list of expected terms
                        var expectedTerm = expectedTerms.FirstOrDefault(p => p.TermId == (term as IFieldTaxonomyValue).TermId);
                        Assert.IsTrue(expectedTerm != null);
                    }
                }
                else if (field.Value.FieldType == "LookupSingle")
                {
                    int idToCheck;
                    if (field.Value.Properties.ContainsKey("LookupId"))
                    {
                        if (field.Value.Properties["LookupId"] == null)
                        {
                            idToCheck = -1;
                        }
                        else
                        {
                            idToCheck = (int)field.Value.Properties["LookupId"];
                        }
                    }
                    else
                    {
                        idToCheck = (int)field.Value.Properties["LookupId"];
                    }

                    if (idToCheck == -1)
                    {
                        Assert.IsTrue(addedItem[field.Key] == null);
                    }
                    else
                    {
                        Assert.IsTrue(addedItem[field.Key] is IFieldLookupValue);
                        Assert.IsTrue((addedItem[field.Key] as IFieldLookupValue).LookupId == idToCheck);
                    }
                }
                else if (field.Value.FieldType == "LookupMulti")
                {
                    Assert.IsTrue(addedItem[field.Key] is IFieldValueCollection);

                    var expectedLookups = (field.Value.Properties["Collection"] as IFieldValueCollection).Values.Cast<IFieldLookupValue>();

                    foreach (var lookup in (addedItem[field.Key] as IFieldValueCollection).Values)
                    {
                        Assert.IsTrue(lookup is IFieldLookupValue);
                        // is this user in the list of expected users
                        var expectedLookup = expectedLookups.FirstOrDefault(p => p.LookupId == (lookup as IFieldLookupValue).LookupId);
                        Assert.IsTrue(expectedLookup != null);
                    }
                }
            }
        }

        private static async Task<IListItem> VerifyListItemViaGetListDataAsStreamAsync(int id, string listTitle, Dictionary<string, FieldData> fieldData, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, id, testName))
            {
                var myList = context.Web.Lists.GetByTitle(listTitle);
                var itemViaGetAsync = myList.Items.FirstOrDefault(p => p.Title == "Item1");

                var listDataOptions = new RenderListDataOptions()
                {
                    RenderOptions = RenderListDataOptionsFlags.ListData,
                };

                var fieldsToLoad = new List<string>() { "Title" };
                foreach (var field in fieldData)
                {
                    fieldsToLoad.Add(field.Key);
                }

                listDataOptions.SetViewXmlFromFields(fieldsToLoad);

                await myList.LoadListDataAsStreamAsync(listDataOptions).ConfigureAwait(false);
                var addedItem = myList.Items.AsRequested().First();

                AssertListItemProperties(fieldData, addedItem);

                return addedItem;
            }
        }

        private static async Task<IListItem> VerifyListItemViaGetAsync(int id, string listTitle, Dictionary<string, FieldData> fieldData, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, id, testName))
            {
                var myList = context.Web.Lists.GetByTitle(listTitle, p => p.Title, p => p.Items, p => p.Fields.QueryProperties(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title));
                var addedItem = myList.Items.AsRequested().FirstOrDefault(p => p.Title == "Item1");

                AssertListItemProperties(fieldData, addedItem);

                return addedItem;
            }
        }
        #endregion


        //[TestMethod]
        //public async Task FieldTypeReadUrl()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        /*
        //        var list = await context.Web.Lists.GetByTitleAsync("FieldTypes");

        //        var listDataOptions = new RenderListDataOptions()
        //        {
        //            RenderOptions = RenderListDataOptionsFlags.ListData,
        //        };

        //        listDataOptions.SetViewXmlFromFields(new List<string>() { "Title", "Url", "PersonSingle", "PersonMultiple", 
        //                                                                  "MMSingle", "MMMultiple", "LookupSingle", "LookupMultiple", 
        //                                                                  "Location", "Bool", "Number", "DateTime", "ChoiceSingle", "ChoiceMultiple" });

        //        await list.GetListDataAsStreamAsync(listDataOptions).ConfigureAwait(false);

        //        var item = list.Items.First();
        //        */


        //        var list = await context.Web.Lists.GetByTitleAsync("FieldTypes", p => p.Title, p => p.Items, p => p.Fields.Load(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title));

        //        var item = list.Items.FirstOrDefault(p => p.Title == "Item1");


        //        //Assert.IsTrue(item != null);
        //        //Assert.IsTrue(item["Url"] != null);
        //        //Assert.IsTrue(item["Url"] is IFieldUrlValue);
        //        //Assert.IsTrue(item["PersonSingle"] is IFieldUserValue);
        //        //Assert.IsTrue(item["PersonMultiple"] is IFieldValueCollection);
        //        //Assert.IsTrue(item["MMSingle"] is IFieldTaxonomyValue);
        //        //Assert.IsTrue(item["MMMultiple"] is IFieldValueCollection);
        //        //Assert.IsTrue(item["LookupSingle"] is IFieldValueCollection);
        //        //Assert.IsTrue(item["LookupMultiple"] is IFieldValueCollection);
        //        //Assert.IsTrue(item["Location"] is IFieldLocationValue);
        //        //Assert.IsTrue(item["ChoiceSingle"] is string);
        //        //Assert.IsTrue(item["ChoiceMultiple"] is List<string>);

        //        // Clear user field testing
        //        //var urlField = list.Fields.First(p => p.InternalName == "Url");
        //        //item["Url"] = item.NewFieldUrlValue(urlField, "");

        //        //// Update url field
        //        //var urlField = list.Fields.First(p => p.InternalName == "Url");

        //        //(item["Url"] as IFieldUrlValue).Url = "https://pnp.com/3";
        //        //(item["Url"] as IFieldUrlValue).Description = "something3";

        //        // clear person field testing
        //        //(item["PersonSingle"] as IFieldUserValue).LookupId = -1;
        //        //(item["PersonMultiple"] as IFieldValueCollection).Values.Clear();

        //        //// Update user fields
        //        //var personMultipleField = list.Fields.First(p => p.InternalName == "PersonMultiple");
        //        //(item["PersonSingle"] as IFieldUserValue).LookupId = 6;
        //        //(item["PersonMultiple"] as IFieldValueCollection).Values.Clear();
        //        //(item["PersonMultiple"] as IFieldValueCollection).Values.Add(item.NewFieldUserValue(personMultipleField, 6));
        //        //(item["PersonMultiple"] as IFieldValueCollection).Values.Add(item.NewFieldUserValue(personMultipleField, 14));

        //        // Clear lookup field testing
        //        //(item["LookupSingle"] as IFieldValueCollection).Values.Clear();
        //        //(item["LookupMultiple"] as IFieldValueCollection).Values.Clear();

        //        //// Update lookup fields
        //        //var lookupSingleField = list.Fields.First(p => p.InternalName == "LookupSingle");
        //        //var lookupMultipleField = list.Fields.First(p => p.InternalName == "LookupMultiple");

        //        //(item["LookupSingle"] as IFieldValueCollection).Values.Clear();
        //        //(item["LookupSingle"] as IFieldValueCollection).Values.Add(item.NewFieldLookupValue(lookupSingleField, 122));
        //        //(item["LookupMultiple"] as IFieldValueCollection).Values.Clear();
        //        //(item["LookupMultiple"] as IFieldValueCollection).Values.Add(item.NewFieldLookupValue(lookupMultipleField, 1));
        //        //(item["LookupMultiple"] as IFieldValueCollection).Values.Add(item.NewFieldLookupValue(lookupMultipleField, 71));
        //        //(item["LookupMultiple"] as IFieldValueCollection).Values.Add(item.NewFieldLookupValue(lookupMultipleField, 122));

        //        // Clear taxonomy fields
        //        //item["MMSingle"] = null;
        //        //(item["MMMultiple"] as IFieldValueCollection).Values.Clear();

        //        //// Update taxonomy fields
        //        //var mmSingleField = list.Fields.First(p => p.InternalName == "MMSingle");

        //        //item["MMSingle"] = item.NewFieldTaxonomyValue(mmSingleField, Guid.Parse("0b709a34-a74e-4d07-b493-48041424a917"), "HBI");

        //        //(item["MMMultiple"] as IFieldValueCollection).RemoveTaxonomyFieldValue(Guid.Parse("1824510b-00e1-40ac-8294-528b1c9421e0"));
        //        //var mmMultipleField = list.Fields.First(p => p.InternalName == "MMMultiple");
        //        //var taxCollection = item.NewFieldValueCollection(mmMultipleField, item.Values);
        //        //taxCollection.Values.Add(item.NewFieldTaxonomyValue(mmMultipleField, Guid.Parse("ed5449ec-4a4f-4102-8f07-5a207c438571"), "LBI"));
        //        //taxCollection.Values.Add(item.NewFieldTaxonomyValue(mmMultipleField, Guid.Parse("1824510b-00e1-40ac-8294-528b1c9421e0"), "MBI"));
        //        //item["MMMultiple"] = taxCollection;

        //        // Clear choice fields
        //        //item["ChoiceSingle"] = null;
        //        //item["ChoiceMultiple"] = new List<string>();

        //        // Update Choice field
        //        //item["ChoiceSingle"] = "Choice 1";

        //        //item["ChoiceMultiple"] = new List<string>() { "Choice 2", "Choice 3", "Choice 4" };



        //        // save update back
        //        await item.UpdateAsync();

        //        //await item.UpdateOverwriteVersionAsync();

        //    }
        //}


    }
}
