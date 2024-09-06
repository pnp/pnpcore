using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Test.Common.Utilities;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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

        #region Add and update list items, including folders

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
                var folderForRootItem = await rootItem.GetParentFolderAsync().ConfigureAwait(false);
                Assert.IsFalse(await rootItem.IsFolderAsync());
                Assert.IsFalse(await rootItem.IsFileAsync());

                var folderItem = await list.AddListFolderAsync("Test");
                var folderForFolderItem = await folderItem.GetParentFolderAsync().ConfigureAwait(false);
                Assert.IsTrue(folderForFolderItem != null);

                var item = list.Items.Add(new Dictionary<string, object> { { "Title", "blabla" } }, "Test");
                var newFolderItem = await list.Items.GetByIdAsync(folderItem.Id);
                Assert.IsTrue(newFolderItem.IsFolder());
                Assert.IsFalse(newFolderItem.IsFile());

                var folder = await item.GetParentFolderAsync().ConfigureAwait(false);
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

                var batchFolder1 = list.AddListFolderBatch("Folder1");
                var batchFolder2 = list.AddListFolderBatch("Folder2");

                await context.ExecuteAsync();

                var newbatchFolder1 = await list.Items.GetByIdAsync(batchFolder1.Id);
                var newbatchFolder2 = await list.Items.GetByIdAsync(batchFolder2.Id);
                Assert.IsTrue(newbatchFolder1["ContentTypeId"].ToString().StartsWith("0x0120"));
                Assert.IsTrue(newbatchFolder2["ContentTypeId"].ToString().StartsWith("0x0120"));

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
        public async Task GetLibraryFolderViaItemTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("GetLibraryFolderViaItemTest");
                IList list = null;
                try
                {
                    list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                    var folderItem = await list.AddListFolderAsync("Test");

                    // Load folder directly from list item 
                    using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                    {
                        var list2 = await context2.Web.Lists.GetByTitleAsync(listTitle);
                        var listItem = await list2.Items.GetByIdAsync(folderItem.Id);
                        var folder = await listItem.Folder.GetAsync(f => f.ServerRelativeUrl, f => f.Name, f => f.TimeLastModified);

                        Assert.IsTrue(folder != null);
                        Assert.IsTrue(folder.Name == "Test");
                    }
                }
                finally
                {
                    await list.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task BulkAddListItemsWithBadFieldNameTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("BulkAddListItemsWithBadFieldNameTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);

                IField myField = await list.Fields.AddTextAsync("MetaInfo", new FieldTextOptions()
                {
                    Group = "Custom Fields",
                    AddToDefaultView = true,
                });

                Assert.IsTrue(myField.InternalName != "MetaInfo");

                List<Dictionary<string, object>> propertiesToUpdate = new List<Dictionary<string, object>>();

                var prop = new Dictionary<string, object>
                {
                    { "Title", "My title 1" },
                    { "MetaInfo", "abc" }
                };
                propertiesToUpdate.Add(prop);

                var prop2 = new Dictionary<string, object>
                {
                    { "Title", "My title 2" },
                    { "MetaInfo", "abc" }
                };
                propertiesToUpdate.Add(prop2);

                foreach (var propItem in propertiesToUpdate)
                {
                    await list.Items.AddBatchAsync(propItem);
                }

                await Assert.ThrowsExceptionAsync<SharePointRestServiceException>(async () =>
                {
                    await context.ExecuteAsync();
                });

                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task BulkAddListItemsWithBadFieldNameTestDontThrowOnError()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("BulkAddListItemsWithBadFieldNameTestDontThrowOnError");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                try
                {
                    IField myField = await list.Fields.AddTextAsync("MetaInfo", new FieldTextOptions()
                    {
                        Group = "Custom Fields",
                        AddToDefaultView = true,
                    });

                    Assert.IsTrue(myField.InternalName != "MetaInfo");

                    List<Dictionary<string, object>> propertiesToUpdate = new List<Dictionary<string, object>>();

                    var prop = new Dictionary<string, object>
                    {
                        { "Title", "My title 1" },
                        { "MetaInfo", "abc" }
                    };
                    propertiesToUpdate.Add(prop);

                    var prop2 = new Dictionary<string, object>
                    {
                        { "Title", "My title 2" },
                        { "MetaInfo", "abc" }
                    };
                    propertiesToUpdate.Add(prop2);

                    foreach (var propItem in propertiesToUpdate)
                    {
                        await list.Items.AddBatchAsync(propItem);
                    }

                    var errors = await context.ExecuteAsync(false);

                    Assert.IsTrue(errors.Count == 2);
                }
                finally
                {
                    await list.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task BulkAddListItemsWithBadFieldNameTestDontThrowOnErrorNamedBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("BulkAddListItemsWithBadFieldNameTestDontThrowOnErrorNamedBatch");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);

                try
                {
                    IField myField = await list.Fields.AddTextAsync("MetaInfo", new FieldTextOptions()
                    {
                        Group = "Custom Fields",
                        AddToDefaultView = true,
                    });

                    Assert.IsTrue(myField.InternalName != "MetaInfo");

                    var batch = context.NewBatch();

                    List<Dictionary<string, object>> propertiesToUpdate = new List<Dictionary<string, object>>();

                    var prop = new Dictionary<string, object>
                    {
                        { "Title", "My title 1" },
                        { "MetaInfo", "abc" }
                    };
                    propertiesToUpdate.Add(prop);

                    var prop2 = new Dictionary<string, object>
                    {
                        { "Title", "My title 2" },
                        { "MetaInfo", "abc" }
                    };
                    propertiesToUpdate.Add(prop2);

                    foreach (var propItem in propertiesToUpdate)
                    {
                        await list.Items.AddBatchAsync(batch, propItem);
                    }

                    var errors = await context.ExecuteAsync(batch, false);

                    Assert.IsTrue(errors.Count == 2);
                }
                finally
                {
                    await list.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task UpdateWithWrongValuesInteractive()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("UpdateWithWrongValuesInteractive");
                IList myList = null;

                try
                {
                    myList = context.Web.Lists.FirstOrDefault(p => p.Title == listTitle);

                    if (myList == null)
                    {
                        // Create the list
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);

                        IField myDateField = await myList.Fields.AddDateTimeAsync("MyDateField", new FieldDateTimeOptions
                        {
                            Group = "Custom Fields",
                            AddToDefaultView = true,                            
                        });

                        // Add a list item to this list
                        // Add a list item
                        Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", "Yes" },
                            { "MyDateField", DateTime.Now }
                        };
                        await myList.Items.AddAsync(values);
                    }
                    else
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList != null)
                    {
                        // get items from the list
                        await myList.LoadAsync(p => p.Items);

                        // grab first item
                        var firstItem = myList.Items.AsRequested().FirstOrDefault();
                        if (firstItem != null)
                        {
                            firstItem.Values["Title"] = "No";
                            firstItem.Values["MyDateField"] = "2024-33-33";

                            await Assert.ThrowsExceptionAsync<SharePointRestServiceException>(async () =>
                            {
                                await firstItem.UpdateAsync();
                            });
                        }
                    }
                }
                finally
                {
                    // Clean up
                    await myList.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task UpdateWithWrongValuesInteractiveBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("UpdateWithWrongValuesInteractiveBatch");
                IList myList = null;

                try
                {
                    myList = context.Web.Lists.FirstOrDefault(p => p.Title == listTitle);

                    if (myList == null)
                    {
                        // Create the list
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);

                        IField myDateField = await myList.Fields.AddDateTimeAsync("MyDateField", new FieldDateTimeOptions
                        {
                            Group = "Custom Fields",
                            AddToDefaultView = true,
                        });

                        // Add a list item to this list
                        // Add a list item
                        Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", "Yes" },
                            { "MyDateField", DateTime.Now }
                        };
                        await myList.Items.AddAsync(values);
                    }
                    else
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList != null)
                    {
                        // get items from the list
                        await myList.LoadAsync(p => p.Items);

                        // grab first item
                        var firstItem = myList.Items.AsRequested().FirstOrDefault();
                        if (firstItem != null)
                        {
                            firstItem.Values["Title"] = "No";
                            firstItem.Values["MyDateField"] = "2024-33-33";
                            
                            var batch = context.NewBatch();
                            await firstItem.UpdateBatchAsync(batch);

                            var batchResults = await context.ExecuteAsync(batch, false);

                            Assert.IsTrue(batchResults.Count == 1);
                        }

                        // get items from the list
                        await myList.LoadAsync(p => p.Items);

                        // grab first item
                        firstItem = myList.Items.AsRequested().FirstOrDefault();
                        if (firstItem != null)
                        {
                            firstItem.Values["Title"] = "No";
                            firstItem.Values["MyDateField"] = "2024-33-33";

                            var batch = context.NewBatch();
                            await firstItem.UpdateBatchAsync(batch);

                            await Assert.ThrowsExceptionAsync<SharePointRestServiceException>(async () =>
                            {
                                var batchResults = await context.ExecuteAsync(batch);
                            });
                        }

                    }
                }
                finally
                {
                    // Clean up
                    await myList.DeleteAsync();
                }
            }
        }


        [TestMethod]
        public async Task VerifyUserFieldsInRepetiveUpdates()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("VerifyUserFieldsInRepetiveUpdates");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);

                IField myUserField = await list.Fields.AddUserAsync("User1", new FieldUserOptions()
                {
                    Group = "Custom Fields",
                    AddToDefaultView = true,
                });

                // load the current user
                var currentUser = await context.Web.GetCurrentUserAsync();

                var addedListItem = await list.Items.AddAsync(new Dictionary<string, object>
                {
                    { "Title", "My title 1" },
                    { "User1", myUserField.NewFieldUserValue(currentUser) }
                });

                Assert.IsTrue((addedListItem["User1"] as IFieldUserValue).LookupId == currentUser.Id);

                // Update approach 1
                // Update only the list item title
                var firstListItem = list.Items.AsRequested().First();
                firstListItem.Title = "updated title 1";
                await firstListItem.UpdateAsync();

                Assert.IsTrue((firstListItem["User1"] as IFieldUserValue).LookupId == currentUser.Id);

                // Update approach 2
                // retrieve the list + items again
                list = context.Web.Lists.GetByTitle(listTitle, p => p.Title, p => p.Items,
                                                     p => p.Fields.QueryProperties(p => p.InternalName,
                                                                                   p => p.FieldTypeKind,
                                                                                   p => p.TypeAsString,
                                                                                   p => p.Title));
                firstListItem = list.Items.AsRequested().First();

                Assert.IsTrue((firstListItem["User1"] as IFieldUserValue).LookupId == currentUser.Id);

                firstListItem.Title = "updated title 2";

                // This update cleared out the user value, fixed now by ensuring a freshly loaded fieldvalue object has no pending changes
                await firstListItem.UpdateAsync();

                // Update approach 3
                var listItemToUpdate = await list.Items.GetByIdAsync(firstListItem.Id);
                Assert.IsTrue((listItemToUpdate["User1"] as IFieldUserValue).LookupId == currentUser.Id);

                listItemToUpdate.Title = "updated title 2";
                await listItemToUpdate.UpdateAsync();

                // Retrieve via ListDataAsStream method
                list.Items.Clear();
                var result = await list.LoadListDataAsStreamAsync(new RenderListDataOptions() { ViewXml = "<View><ViewFields><FieldRef Name='Title' /><FieldRef Name='User1' /></ViewFields><RowLimit>5</RowLimit></View>", RenderOptions = RenderListDataOptionsFlags.ListData });
                firstListItem = list.Items.AsRequested().First();

                Assert.IsTrue((firstListItem["User1"] as IFieldUserValue).LookupId == currentUser.Id);

                firstListItem.Title = "updated title 3";

                await firstListItem.UpdateAsync();

                listItemToUpdate = await list.Items.GetByIdAsync(firstListItem.Id);
                Assert.IsTrue((listItemToUpdate["User1"] as IFieldUserValue).LookupId == currentUser.Id);

                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task SetUserViaIdOnly()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("SetUserViaIdOnly");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);

                IField myUserField = await list.Fields.AddUserAsync("User1", new FieldUserOptions()
                {
                    Group = "Custom Fields",
                    AddToDefaultView = true,
                    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
                });

                // load the current user
                var currentUser = await context.Web.GetCurrentUserAsync();

                await Assert.ThrowsExceptionAsync<ClientException>(async () =>
                {
                    var addedListItem = await list.Items.AddAsync(new Dictionary<string, object>
                    {
                        { "Title", "My title 1" },
                        { "User1", myUserField.NewFieldUserValue(currentUser.Id) }
                    });
                });

                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdateListItemModifiedCreatedBy()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string listTitle = TestCommon.GetPnPSdkTestAssetName("UpdateListItemModifiedCreatedBy");
                var myList = context.Web.Lists.FirstOrDefault(p => p.Title == listTitle);

                if (myList == null)
                {
                    // Create the list
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);


                    // Add a list item to this list
                    // Add a list item
                    Dictionary<string, object> values = new Dictionary<string, object>
                    {
                        { "Title", "Yes" }
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
                        await myList.LoadAsync(p => p.Items, p => p.Fields);

                        // grab first item
                        var firstItem = myList.Items.AsRequested().FirstOrDefault();
                        if (firstItem != null)
                        {
                            Assert.IsTrue(firstItem.Values["Title"].ToString() == "Yes");

                            // Load the Author and Editor fields
                            var author = myList.Fields.AsRequested().FirstOrDefault(p => p.InternalName == "Author");
                            var editor = myList.Fields.AsRequested().FirstOrDefault(p => p.InternalName == "Editor");

                            // load the current user
                            var currentUser = await context.Web.GetCurrentUserAsync();
                            var newDate = new DateTime(2020, 10, 20);

                            firstItem.Values["Author"] = author.NewFieldUserValue(currentUser);
                            firstItem.Values["Editor"] = editor.NewFieldUserValue(currentUser);
                            firstItem.Values["Created"] = newDate;
                            firstItem.Values["Modified"] = newDate;

                            await firstItem.UpdateOverwriteVersionAsync();

                            // get items again from the list
                            await myList.LoadAsync(p => p.Items);
                            firstItem = myList.Items.AsRequested().FirstOrDefault();

                            if (!TestCommon.RunningInGitHubWorkflow())
                            {
                                Assert.IsTrue(((DateTime)firstItem.Values["Created"]).Year == newDate.Year);
                                Assert.IsTrue(((DateTime)firstItem.Values["Created"]).Month == newDate.Month);
                                Assert.IsTrue(((DateTime)firstItem.Values["Modified"]).Year == newDate.Year);
                                Assert.IsTrue(((DateTime)firstItem.Values["Modified"]).Month == newDate.Month);
                            }
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
        public async Task ParseListItemSpecialIssue519()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string listTitle = TestCommon.GetPnPSdkTestAssetName("ParseListItemSpecialIssue519");
                var myList = context.Web.Lists.FirstOrDefault(p => p.Title == listTitle);

                string fldText1 = "SpecialField";
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
                        { fldText1, "1.1" }
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

                        // grab first item via list load
                        var firstItem = myList.Items.AsRequested().FirstOrDefault();
                        if (firstItem != null)
                        {
                            Assert.IsTrue(firstItem.Values["Title"].ToString() == "Yes");
                            Assert.IsTrue(firstItem.Values[fldText1].ToString() == "1.1");
                        }

                        // grab first item via GetById
                        var firstItem2 = await myList.Items.GetByIdAsync(1);
                        if (firstItem2 != null)
                        {
                            Assert.IsTrue(firstItem2.Values["Title"].ToString() == "Yes");
                            Assert.IsTrue(firstItem2.Values[fldText1].ToString() == "1.1");
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

        #endregion

        #region MoveTo()

        [TestMethod]
        public void ListItemMoveToNestedFolder()
        {
            // TestCommon.Instance.Mocking = false;
            using (PnPContext context = TestCommon.Instance.GetContext(TestCommonBase.TestSite))
            {
                string listTitle = TestCommonBase.GetPnPSdkTestAssetName("ListItemMoveToNestedTest");
                IList list = context.Web.Lists.Add(listTitle, ListTemplateType.GenericList);
                try
                {
                    list.ContentTypesEnabled = true;
                    list.EnableFolderCreation = true;
                    list.Update();

                    // Create path 'sub1/sub2/sub3/sub4'
                    string path = new[] {"sub1", "sub2", "sub3", "sub4"}.Aggregate(
                        "",
                        (aggregate, element) =>
                        {
                            IListItem addedFolder = list.AddListFolder(element, aggregate);
                            Assert.IsTrue(addedFolder != null);

                            return $"{aggregate}/{element}";
                        }
                    );

                    // Add item to root of the list
                    IListItem rootItem = list.Items.Add(new Dictionary<string, object> {{"Title", "root"}});
                    Assert.IsFalse(rootItem.IsFolder());
                    Assert.IsFalse(rootItem.IsFile());

                    // Move item to folder 'sub1/sub2/sub3/sub4'
                    rootItem.MoveTo(path);

                    // Retrieve moved item
                    IListItem movedItem = list.Items.GetById(rootItem.Id);
                    // Retrieve parent folder of moved item
                    IFolder movedFolder = movedItem.GetParentFolder();

                    Assert.IsTrue(movedFolder.Name == "sub4");
                }
                finally
                {
                    list.Delete();
                }
            }
        }

        [DataRow("Test")]
        [DataRow("/Test")]
        [DataRow("Test/")]
        [DataRow("/Test/")]
        [TestMethod]
        public async Task ListItemMoveTo_Async(string folderPath)
        {
            // TestCommon.Instance.Mocking = false;
            using (PnPContext context = await TestCommon.Instance.GetContextAsync(TestCommonBase.TestSite))
            {
                string listTitle = TestCommonBase.GetPnPSdkTestAssetName("ListItemMoveToTest");
                IList list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);

                try
                {
                    list.ContentTypesEnabled = true;
                    list.EnableFolderCreation = true;
                    await list.UpdateAsync();

                    // Add item to root of the list
                    IListItem rootItem = list.Items.Add(new Dictionary<string, object> {{"Title", "root"}});
                    Assert.IsFalse(await rootItem.IsFolderAsync());
                    Assert.IsFalse(await rootItem.IsFileAsync());

                    // Add folder 'Test'
                    IListItem folderItem = await list.AddListFolderAsync("Test");
                    IFolder folderForFolderItem = await folderItem.GetParentFolderAsync();
                    Assert.IsTrue(folderForFolderItem != null);

                    IListItem newFolderItem = await list.Items.GetByIdAsync(folderItem.Id);
                    Assert.IsTrue(await newFolderItem.IsFolderAsync());
                    Assert.IsFalse(await newFolderItem.IsFileAsync());

                    // Move item to folder 'Test'
                    await rootItem.MoveToAsync(folderPath);

                    // Retrieve moved item
                    IListItem movedItem = await list.Items.GetByIdAsync(rootItem.Id);
                    // Retrieve parent folder of moved item
                    IFolder movedFolder = await movedItem.GetParentFolderAsync();

                    Assert.IsTrue(movedFolder.Name == "Test");
                }
                finally
                {
                    await list.DeleteAsync();
                }
            }
        }

        #endregion

        #region Recycle tests

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

        #endregion

        #region SystemUpdate tests

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
                // CSOM is used under the covers
                await first.WithResponseHeaders((responseHeaders) => {
                    Assert.IsTrue(!string.IsNullOrEmpty(responseHeaders["SPRequestGuid"]));
                }).UpdateOverwriteVersionBatchAsync(batch).ConfigureAwait(false);
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

        [TestMethod]
        public async Task SystemUpdateMetaInfo()
        {
            //TestCommon.Instance.Mocking = false;
            string listTitle = "SystemUpdateMetaInfo";

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

                    foreach (var item in myList.Items.AsRequested())
                    {
                        item["MetaInfo"] = "{DFC8691F-2432-4741-B780-3CAE3235A612}:SW|MyStringWithXmlValues";
                        item["Title"] = "okido";
                        // Both work
                        await item.SystemUpdateBatchAsync();
                        //await item.UpdateOverwriteVersionBatchAsync();
                    }
                    await context.ExecuteAsync();

                }

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var myList2 = context2.Web.Lists.FirstOrDefault(p => p.Title == listTitle);
                    await myList2.LoadAsync(p => p.Items.QueryProperties(p => p.Title, p => p.FieldValuesAsText));

                    foreach (var item2 in myList2.Items.AsRequested())
                    {
                        Assert.IsTrue(item2.Title == "okido");
                        Assert.IsTrue(item2.FieldValuesAsText.Values["MetaInfo"].ToString().Contains("{DFC8691F-2432-4741-B780-3CAE3235A612}:SW|MyStringWithXmlValues"));
                    }
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
        public async Task SystemUpdateFromFolderAndFileLoadedViaWeb()
        {
            //TestCommon.Instance.Mocking = false;
            string listTitle = "SystemUpdateFromFolderAndFileLoadedViaWeb";

            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    // Create a new list
                    var documentLibrary = context.Web.Lists.FirstOrDefault(p => p.Title == listTitle);

                    if (TestCommon.Instance.Mocking && documentLibrary != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (documentLibrary == null)
                    {
                        documentLibrary = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                        IField myField = await documentLibrary.Fields.AddTextAsync("CustomField", new FieldTextOptions()
                        {
                            Group = "Custom Fields",
                            AddToDefaultView = true,
                        });
                    }

                    // Get the root folder of the library
                    IFolder folder = await documentLibrary.RootFolder.GetAsync();

                    // Add a folder and a file
                    var demoFolder = await folder.AddFolderAsync("Demo");
                    IFile mockDocument = await demoFolder.Files.AddAsync("test.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"));

                    // Load the folder which we want to update again 
                    var folderToUpdate = await context.Web.GetFolderByServerRelativeUrlAsync(demoFolder.ServerRelativeUrl, p => p.ListItemAllFields);

                    // Call system update on the folder
                    folderToUpdate.ListItemAllFields["CustomField"] = "blabla";
                    await folderToUpdate.ListItemAllFields.SystemUpdateAsync();

                    // Load the file we want to update again
                    var fileToUpdate = await context.Web.GetFileByServerRelativeUrlAsync(mockDocument.ServerRelativeUrl, p => p.ListItemAllFields);

                    // Call system update on the folder
                    fileToUpdate.ListItemAllFields["CustomField"] = "blabla";
                    await fileToUpdate.ListItemAllFields.SystemUpdateAsync();
                }
            }
            finally
            {
                using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var documentLibrary = contextFinal.Web.Lists.FirstOrDefault(p => p.Title == listTitle);

                    // Cleanup the created list
                    await documentLibrary.DeleteAsync();
                }
            }
        }

        #endregion

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
        public async Task ImageFieldTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {

                //==========================================================
                // Step 1: Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("ImageAndLocationFieldTest");

                IList myList = null;

                try
                {
                    myList = await context.Web.Lists.GetByTitleAsync(listTitle);

                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    }

                    //==========================================================
                    string fldImage1 = "Image1";
                    IField addedImageField1 = await myList.Fields.AddFieldAsXmlAsync($"<Field DisplayName='{fldImage1}' Format='Thumbnail' IsModern='TRUE' Name='{fldImage1}' Title='{fldImage1}' Type='Thumbnail'></Field>", addToDefaultView: true);

                    // Upload an item 
                    var addedItem = await myList.Items.AddAsync(new Dictionary<string, object>
                    {
                        { "Title", "Item1" }                        
                    });

                    // Read the item again to get a reference to the image column
                    myList = context.Web.Lists.GetByTitle(listTitle, p => p.Title, p => p.Items, p => p.Fields.QueryProperties(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title));

                    // Set the image for the added item
                    addedItem = myList.Items.AsRequested().First();
                    // Important to add the extension in the image name as that's being checked to be a valid image extension
                    await (addedItem[$"{fldImage1}"] as IFieldThumbnailValue).UploadImageAsync(addedItem, "parker-ms-300.png", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}parker-ms-300.png"));

                    // Read the item again to get a reference to the image column which now should be populated
                    myList = context.Web.Lists.GetByTitle(listTitle, p => p.Title, p => p.Items, p => p.Fields.QueryProperties(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title));
                    addedItem = myList.Items.AsRequested().First();

                    Assert.IsTrue(addedItem[$"{fldImage1}"] is IFieldThumbnailValue);
                    Assert.IsTrue((addedItem[$"{fldImage1}"] as IFieldThumbnailValue).FileName.Contains("parker-ms-300"));

                    // Read the item using LoadListDataAsStreamAsync
                    myList.Items.Clear();
                    await myList.LoadListDataAsStreamAsync(new RenderListDataOptions 
                    { 
                        
                    });

                    addedItem = myList.Items.AsRequested().First();
                    Assert.IsTrue(addedItem[$"{fldImage1}"] is IFieldThumbnailValue);
                    Assert.IsTrue((addedItem[$"{fldImage1}"] as IFieldThumbnailValue).FileName.Contains("parker-ms-300"));
                }
                finally
                {
                    await myList.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task LocationFieldTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {

                //==========================================================
                // Step 1: Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("LocationFieldTest");

                IList myList = null;

                try
                {
                    myList = await context.Web.Lists.GetByTitleAsync(listTitle);

                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    }

                    //==========================================================
                    string fldLocation1 = "Location1";
                    IField addedLocationField1 = await myList.Fields.AddFieldAsXmlAsync($"<Field DisplayName='{fldLocation1}' Format='Dropdown' IsModern='TRUE' Name='{fldLocation1}' Title='{fldLocation1}' Type='Location'></Field>", addToDefaultView: true);

                    // Upload an item 
                    var addedItem = await myList.Items.AddAsync(new Dictionary<string, object>
                    {
                        { "Title", "Item1" },
                        { fldLocation1, addedLocationField1.NewFieldLocationValue("Somewhere", 50.6354, 3.06998) }
                    });

                    // Read the item again to get a reference to the image column which now should be populated
                    myList = context.Web.Lists.GetByTitle(listTitle, p => p.Title, p => p.Items, p => p.Fields.QueryProperties(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title));
                    addedItem = myList.Items.AsRequested().First();

                    Assert.IsTrue((addedItem[$"{fldLocation1}"] as IFieldLocationValue).DisplayName == "Somewhere");
                    Assert.IsTrue((addedItem[$"{fldLocation1}"] as IFieldLocationValue).Latitude == 50.6354);
                    Assert.IsTrue((addedItem[$"{fldLocation1}"] as IFieldLocationValue).Longitude == 3.06998);

                    // Read the item using LoadListDataAsStreamAsync
                    myList.Items.Clear();
                    await myList.LoadListDataAsStreamAsync(new RenderListDataOptions
                    {

                    });

                    addedItem = myList.Items.AsRequested().First();
                    Assert.IsTrue((addedItem[$"{fldLocation1}"] as IFieldLocationValue).DisplayName == "Somewhere");
                    Assert.IsTrue((addedItem[$"{fldLocation1}"] as IFieldLocationValue).Latitude == 50.6354);
                    Assert.IsTrue((addedItem[$"{fldLocation1}"] as IFieldLocationValue).Longitude == 3.06998);
                }
                finally
                {
                    await myList.DeleteAsync();
                }
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

                try
                {
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
                }
                finally
                {
                    // Cleanup the created list
                    await myList.DeleteAsync();
                }
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
                        // Don't check these as due to time zone settings of the use sites this may differ
                        //Assert.IsTrue(server.Day == expected.Day);
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
        public async Task SpecialFieldRestUpdateAlternativeTest()
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
                string listTitle = TestCommon.GetPnPSdkTestAssetName("SpecialFieldRestUpdateAlternativeTest");

                IList myList = null;
                try
                {
                    myList = await context.Web.Lists.GetByTitleAsync(listTitle);

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
                    item.Add(fldUrl1, new FieldUrlValue(fieldData[fldUrl1].Properties["Url"].ToString(), fieldData[fldUrl1].Properties["Description"].ToString()));

                    // URL field 2 -  no description value set on create
                    fieldData[fldUrl2].Properties.Add("Url", "https://pnp.com");
                    // set the expected data equal to the url field as that's what we expect
                    fieldData[fldUrl2].Properties.Add("Description", fieldData[fldUrl2].Properties["Url"]);
                    item.Add(fldUrl2, new FieldUrlValue(fieldData[fldUrl2].Properties["Url"].ToString()));

                    // User single field 1
                    fieldData[fldUserSingle1].Properties.Add("Principal", currentUser);
                    item.Add(fldUserSingle1, new FieldUserValue(currentUser));

                    // User multi field 1
                    var userCollection = new FieldValueCollection();
                    userCollection.Values.Add(new FieldUserValue(currentUser));
                    fieldData[fldUserMulti1].Properties.Add("Collection", userCollection);
                    item.Add(fldUserMulti1, userCollection);

                    // Taxonomy single field 1
                    fieldData[fldTaxonomy1].Properties.Add("TermStore", termStore);
                    fieldData[fldTaxonomy1].Properties.Add("TermSet", termSet);
                    fieldData[fldTaxonomy1].Properties.Add("Term1", term1);
                    fieldData[fldTaxonomy1].Properties.Add("Label1", label1);
                    item.Add(fldTaxonomy1, new FieldTaxonomyValue((Guid)fieldData[fldTaxonomy1].Properties["Term1"], fieldData[fldTaxonomy1].Properties["Label1"].ToString()));

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
                        new FieldTaxonomyValue((Guid)fieldData[fldTaxonomyMulti1].Properties["Term1"], fieldData[fldTaxonomyMulti1].Properties["Label1"].ToString()),
                        new FieldTaxonomyValue((Guid)fieldData[fldTaxonomyMulti1].Properties["Term2"], fieldData[fldTaxonomyMulti1].Properties["Label2"].ToString())
                    };
                    var termCollection = new FieldValueCollection(taxonomyValues);
                    fieldData[fldTaxonomyMulti1].Properties.Add("Collection", termCollection);
                    item.Add(fldTaxonomyMulti1, termCollection);

                    // Lookup single field 1
                    fieldData[fldLookupSingle1].Properties.Add("LookupId", 1);
                    item.Add(fldLookupSingle1, new FieldLookupValue((int)fieldData[fldLookupSingle1].Properties["LookupId"]));

                    // Lookup multi field 1
                    fieldData[fldLookupMulti1].Properties.Add("LookupId", 1);
                    var lookupCollection = new FieldValueCollection();
                    lookupCollection.Values.Add(new FieldLookupValue((int)fieldData[fldLookupMulti1].Properties["LookupId"]));
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
                    // Step 7: Update item using REST update 

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
                    (fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection).Values.Add(new FieldUserValue(userTwo));
                    addedItem[fldUserMulti1] = fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection;

                    // Taxonomy single field 1
                    fieldData[fldTaxonomy1].Properties["Term1"] = term2;
                    fieldData[fldTaxonomy1].Properties["Labe1"] = label2;
                    (addedItem[fldTaxonomy1] as IFieldTaxonomyValue).TermId = (Guid)fieldData[fldTaxonomy1].Properties["Term1"];
                    (addedItem[fldTaxonomy1] as IFieldTaxonomyValue).Label = fieldData[fldTaxonomy1].Properties["Label1"].ToString();

                    // Taxonomy multi field 1
                    (fieldData[fldTaxonomyMulti1].Properties["Collection"] as IFieldValueCollection).Values.Add(new FieldTaxonomyValue((Guid)fieldData[fldTaxonomyMulti1].Properties["Term3"], fieldData[fldTaxonomyMulti1].Properties["Label3"].ToString()));
                    addedItem[fldTaxonomyMulti1] = fieldData[fldTaxonomyMulti1].Properties["Collection"] as IFieldValueCollection;

                    // Lookup single field 1
                    fieldData[fldLookupSingle1].Properties["LookupId"] = 1;
                    (addedItem[fldLookupSingle1] as IFieldLookupValue).LookupId = (int)fieldData[fldLookupSingle1].Properties["LookupId"];

                    // Lookup multi field 1
                    (fieldData[fldLookupMulti1].Properties["Collection"] as IFieldValueCollection).Values.Add(new FieldLookupValue(1));
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
                    // Step 10: Blank item using REST update

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

                }
                finally
                {
                    if (myList != null)
                    {
                        // Cleanup the created list
                        await myList.DeleteAsync();
                    }
                }
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
                        Assert.IsTrue((addedItem[field.Key] as IFieldUrlValue).Url == null);
                        Assert.IsTrue((addedItem[field.Key] as IFieldUrlValue).Description == null);
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
                        Assert.IsTrue((addedItem[field.Key] as IFieldTaxonomyValue).TermId == Guid.Empty);
                        Assert.IsTrue((addedItem[field.Key] as IFieldTaxonomyValue).Label == null);
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

        #region Properties
        [TestMethod]
        public async Task ListItemAsFilePropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string parentLibraryName, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);
            
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IFile file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl,
                        f => f.ListItemAllFields.QueryProperties(li => li.Id),
                        f => f.Length,
                        f => f.ServerRelativeUrl);

                    Assert.IsNotNull(file);
                    Assert.IsNotNull(file.ListItemAllFields);
                    Assert.IsTrue(file.ListItemAllFields.Id > 0);

                    IList list = await context.Web.Lists.GetByTitleAsync(parentLibraryName);

                    Assert.IsNotNull(list);

                    IListItem listItem = await list.Items.GetByIdAsync(file.ListItemAllFields.Id,
                        li => li.CommentsDisabled,
                        li => li.CommentsDisabledScope,
                        li => li.ContentType.QueryProperties(ct => ct.Name, ct => ct.ReadOnly, ct => ct.Sealed),
                        li => li.File.QueryProperties(f => f.Length, f => f.ServerRelativeUrl, f => f.Name),
                        li => li.FileSystemObjectType,
                        li => li.Folder.QueryProperties(f => f.Name, f => f.ServerRelativeUrl),
                        li => li.ParentList.QueryProperties(l => l.Title),
                        li => li.ServerRedirectedEmbedUri,
                        li => li.ServerRedirectedEmbedUrl,
                        li => li.UniqueId);

                    Assert.IsNotNull(listItem);
                    Assert.IsNotNull(listItem.ContentType);
                    Assert.IsNotNull(listItem.File);

                    Assert.AreEqual(file.ListItemAllFields.Id, listItem.Id);

                    Assert.IsFalse(listItem.CommentsDisabled);
                    Assert.AreEqual(CommentsDisabledScope.None, listItem.CommentsDisabledScope);
                    Assert.AreEqual(FileSystemObjectType.File, listItem.FileSystemObjectType);
                    Assert.AreNotEqual(Guid.Empty, listItem.UniqueId);
                    Assert.IsNotNull(listItem.ServerRedirectedEmbedUri);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(listItem.ServerRedirectedEmbedUrl));

                    Assert.IsFalse(listItem.ContentType.ReadOnly);
                    Assert.IsFalse(listItem.ContentType.Sealed);

                    Assert.ThrowsException<ClientException>(() => listItem.Folder.ServerRelativeUrl);

                    Assert.AreEqual(file.Length, listItem.File.Length);
                    Assert.AreEqual(file.ServerRelativeUrl, listItem.File.ServerRelativeUrl);

                    Assert.AreEqual(list.Id, listItem.ParentList.Id);
                    Assert.AreEqual(list.Title, listItem.ParentList.Title);
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(2);
            }
        }

        [TestMethod]
        public async Task ListItemAsFolderPropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;
            IFolder mockFolder = null;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                try
                {
                    IFolder parentFolder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;
                    mockFolder = await parentFolder.Folders.AddAsync(nameof(ListItemAsFolderPropertiesTest));
                    mockFolder = await context.Web.GetFolderByServerRelativeUrlAsync(mockFolder.ServerRelativeUrl,
                        f => f.Name,
                        f => f.ServerRelativeUrl,
                        f => f.ListItemAllFields.QueryProperties(li => li.Id));

                    IList list = await context.Web.Lists.GetByTitleAsync("Documents");

                    Assert.IsNotNull(list);

                    IListItem listItem = await list.Items.GetByIdAsync(mockFolder.ListItemAllFields.Id,
                        li => li.CommentsDisabled,
                        li => li.CommentsDisabledScope,
                        li => li.ContentType.QueryProperties(ct => ct.Name, ct => ct.ReadOnly, ct => ct.Sealed),
                        li => li.File.QueryProperties(f => f.Length, f => f.ServerRelativeUrl, f => f.Name),
                        li => li.FileSystemObjectType,
                        li => li.Folder.QueryProperties(f => f.Name, f => f.ServerRelativeUrl),
                        li => li.ParentList.QueryProperties(l => l.Title),
                        li => li.ServerRedirectedEmbedUri,
                        li => li.ServerRedirectedEmbedUrl,
                        li => li.UniqueId);

                    Assert.IsNotNull(listItem);
                    Assert.IsNotNull(listItem.ContentType);
                    Assert.IsNotNull(listItem.Folder);

                    Assert.AreEqual(mockFolder.ListItemAllFields.Id, listItem.Id);

                    Assert.IsFalse(listItem.CommentsDisabled);
                    Assert.AreEqual(CommentsDisabledScope.None, listItem.CommentsDisabledScope);
                    Assert.AreEqual(FileSystemObjectType.Folder, listItem.FileSystemObjectType);
                    Assert.AreNotEqual(Guid.Empty, listItem.UniqueId);
                    Assert.IsNull(listItem.ServerRedirectedEmbedUri);
                    Assert.IsTrue(string.IsNullOrWhiteSpace(listItem.ServerRedirectedEmbedUrl));

                    Assert.IsFalse(listItem.ContentType.ReadOnly);
                    Assert.IsTrue(listItem.ContentType.Sealed);

                    Assert.ThrowsException<ClientException>(() => listItem.File.ServerRelativeUrl);

                    Assert.AreEqual(mockFolder.Name, listItem.Folder.Name);
                    Assert.AreEqual(mockFolder.ServerRelativeUrl, listItem.Folder.ServerRelativeUrl);

                    Assert.AreEqual(list.Id, listItem.ParentList.Id);
                    Assert.AreEqual(list.Title, listItem.ParentList.Title);
                }
                finally
                {
                    if (mockFolder != null)
                    {
                        await mockFolder.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task ListItemFieldValuesTest()
        {
            //TestCommon.Instance.Mocking = false;
            try
            {
                (string parentListName, int itemId, _) = await TestAssets.CreateTestListItemAsync(0);

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IList list = await context.Web.Lists.GetByTitleAsync(parentListName);

                    Assert.IsNotNull(list);

                    IListItem listItem = await list.Items.GetByIdAsync(itemId,
                        li => li.FieldValuesAsHtml,
                        li => li.FieldValuesAsText,
                        li => li.FieldValuesForEdit);

                    Assert.IsNotNull(listItem);
                    Assert.AreNotEqual(0, listItem.FieldValuesAsHtml.Count);
                    Assert.AreNotEqual(0, listItem.FieldValuesAsText.Count);
                    Assert.AreNotEqual(0, listItem.FieldValuesForEdit.Count);
                }
            }
            finally
            {
                await TestAssets.CleanupTestDedicatedListAsync(2);
            }
        }

        [TestMethod]
        public async Task ListItemAsFileFromCamlQueryAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create new library with extra fields
                var listTitle = TestCommon.GetPnPSdkTestAssetName("ListItemAsFileFromCamlQueryAsyncTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);

                // Add the fields as one batch call
                string fieldGroup = "custom";
                IField addedTextField1 = await list.Fields.AddTextBatchAsync("TestStringField", new FieldTextOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                IField addedBoolField1 = await list.Fields.AddBooleanBatchAsync("TestBoolField", new FieldBooleanOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                IField addedNumberField1 = await list.Fields.AddNumberBatchAsync("TestNumberField", new FieldNumberOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true
                });
                await context.ExecuteAsync();

                // Add a file
                await list.EnsurePropertiesAsync(l => l.RootFolder);
                IFile testDocument = list.RootFolder.Files.Add("test.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);

                // Set the file metadata, first load the connected list item
                await testDocument.ListItemAllFields.LoadAsync();
                testDocument.ListItemAllFields["TestStringField"] = "This is my test";
                testDocument.ListItemAllFields["TestBoolField"] = true;
                testDocument.ListItemAllFields["TestNumberField"] = 10;
                await testDocument.ListItemAllFields.UpdateAsync();

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    // Query the document again
                    const string viewXml = @"<View Scope='Recursive'><RowLimit>1</RowLimit></View>";

                    var list2 = await context2.Web.Lists.GetByTitleAsync(listTitle);
                    Expression<Func<IListItem, object>>[] selectors =
                    {
                            li => li.All,
                            li => li.CommentsDisabled,
                            li => li.CommentsDisabledScope,
                            li => li.ContentType.QueryProperties(ct => ct.Name, ct => ct.Sealed),
                            li => li.UniqueId,
                            li => li.ServerRedirectedEmbedUri,
                            li => li.ServerRedirectedEmbedUrl
                        };

                    await list2.LoadItemsByCamlQueryAsync(new CamlQueryOptions()
                    {
                        ViewXml = viewXml,
                        DatesInUtc = true
                    }, selectors).ConfigureAwait(false);

                    IListItem listItem = list2.Items.AsRequested().FirstOrDefault();

                    Assert.IsNotNull(listItem);
                    Assert.IsFalse(listItem.CommentsDisabled);
                    Assert.AreEqual(CommentsDisabledScope.None, listItem.CommentsDisabledScope);
                    Assert.AreEqual(FileSystemObjectType.File, listItem.FileSystemObjectType);
                    Assert.AreNotEqual(Guid.Empty, listItem.UniqueId);
                    Assert.IsNotNull(listItem.ServerRedirectedEmbedUri);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(listItem.ServerRedirectedEmbedUrl));

                    Assert.IsFalse(string.IsNullOrWhiteSpace(listItem.ContentType.Name));
                    Assert.IsFalse(listItem.ContentType.Sealed);

                    Assert.AreEqual(10.0, listItem.Values["TestNumberField"]);
                    Assert.AreEqual(true, listItem.Values["TestBoolField"]);
                    Assert.AreEqual("This is my test", listItem.Values["TestStringField"]);
                }

                // Delete the library again
                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ListItemAsFileFromDataStreamTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create new library with extra fields
                var listTitle = TestCommon.GetPnPSdkTestAssetName("ListItemAsFileFromDataStreamTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                await list.EnsurePropertiesAsync(l => l.RootFolder);
                list.ContentTypesEnabled = true;
                list.EnableFolderCreation = true;
                await list.UpdateAsync();

                // Add the fields as one batch call
                string fieldGroup = "custom";
                IField addedTextField1 = await list.Fields.AddTextBatchAsync("TestStringField", new FieldTextOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                IField addedBoolField1 = await list.Fields.AddBooleanBatchAsync("TestBoolField", new FieldBooleanOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });

                IField addedNumberField1 = await list.Fields.AddNumberBatchAsync("TestNumberField", new FieldNumberOptions()
                {
                    Group = fieldGroup,
                    AddToDefaultView = true,
                });
                await context.ExecuteAsync();

                // Add a file                
                IFile testDocument = list.RootFolder.Files.Add("test.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);

                // Set the file metadata, first load the connected list item
                await testDocument.ListItemAllFields.LoadAsync();
                testDocument.ListItemAllFields["TestStringField"] = "This is my test";
                testDocument.ListItemAllFields["TestBoolField"] = true;
                testDocument.ListItemAllFields["TestNumberField"] = 10;
                await testDocument.ListItemAllFields.UpdateAsync();

                // Add file in folder
                IFolder testFolder = await list.RootFolder.AddFolderAsync("Test");
                IFile testDocument2 = testFolder.Files.Add("test2.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                // Set the file metadata, first load the connected list item
                await testDocument2.ListItemAllFields.LoadAsync();
                testDocument2.ListItemAllFields["TestStringField"] = "This is my test 2";
                testDocument2.ListItemAllFields["TestBoolField"] = false;
                testDocument2.ListItemAllFields["TestNumberField"] = 100;
                await testDocument2.ListItemAllFields.UpdateAsync();

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    const string viewXml = @"<View Scope='Recursive'><RowLimit Paged='TRUE'>5</RowLimit></View>";

                    IList list2 = await context2.Web.Lists.GetByTitleAsync(listTitle);

                    Assert.IsNotNull(list2);

                    var output = await list2.LoadListDataAsStreamAsync(new RenderListDataOptions()
                    {
                        ViewXml = viewXml,
                        RenderOptions = RenderListDataOptionsFlags.ListData
                    }).ConfigureAwait(false);

                    IListItem listItem = list2.Items.AsRequested().FirstOrDefault();

                    Assert.IsNotNull(listItem);

                    Assert.AreEqual(FileSystemObjectType.File, listItem.FileSystemObjectType);
                    Assert.AreNotEqual(Guid.Empty, listItem.UniqueId);
                    Assert.IsNotNull(listItem.ServerRedirectedEmbedUri);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(listItem.ServerRedirectedEmbedUrl));

                    Assert.IsFalse(string.IsNullOrWhiteSpace(listItem.ContentType.Name));

                    Assert.AreEqual(10, (double)listItem.Values["TestNumberField"]);
                    Assert.AreEqual(true, (bool)listItem.Values["TestBoolField"]);
                    Assert.AreEqual("This is my test", (string)listItem.Values["TestStringField"]);


                    IListItem lastListItem = list2.Items.AsRequested().LastOrDefault();

                    Assert.IsNotNull(lastListItem);

                    Assert.AreEqual(FileSystemObjectType.File, lastListItem.FileSystemObjectType);
                    Assert.AreNotEqual(Guid.Empty, lastListItem.UniqueId);
                    Assert.IsNotNull(lastListItem.ServerRedirectedEmbedUri);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(lastListItem.ServerRedirectedEmbedUrl));

                    Assert.IsFalse(string.IsNullOrWhiteSpace(lastListItem.ContentType.Name));

                    Assert.AreEqual(100, (double)lastListItem.Values["TestNumberField"]);
                    Assert.AreEqual(false, (bool)lastListItem.Values["TestBoolField"]);
                    Assert.AreEqual("This is my test 2", (string)lastListItem.Values["TestStringField"]);

                }

                // Delete the library again
                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ListItemSpecialFieldNamesTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create new library with extra fields
                var listTitle = TestCommon.GetPnPSdkTestAssetName("ListItemSpecialFieldNamesTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);

                try
                {
                    // Add the fields as one batch call
                    string fieldGroup = "custom";
                    IField addedTextField1 = await list.Fields.AddTextBatchAsync("With Space", new FieldTextOptions()
                    {
                        Group = fieldGroup,
                        AddToDefaultView = true,
                    });

                    IField addedBoolField1 = await list.Fields.AddTextBatchAsync("With_Underscore", new FieldTextOptions()
                    {
                        Group = fieldGroup,
                        AddToDefaultView = true,
                    });

                    IField addedNumberField1 = await list.Fields.AddTextBatchAsync("With SpaceAnd_Underscore", new FieldTextOptions()
                    {
                        Group = fieldGroup,
                        AddToDefaultView = true,
                    });
                    await context.ExecuteAsync();

                    // Add a list item
                    Dictionary<string, object> values = new Dictionary<string, object>
                    {
                        { "Title", "Yes" },
                        { "With_x0020_Space", "Yes" },
                        { "With_Underscore", "Yes" },
                        { "With_x0020_SpaceAnd_Underscore", "Yes" },
                    };

                    await list.Items.AddAsync(values);

                    // Verify that all read options return the fields using their internal names
                    using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                    {
                        // Query the document again
                        const string viewXml = @"<View Scope='Recursive'><RowLimit>1</RowLimit></View>";

                        var list2 = await context2.Web.Lists.GetByTitleAsync(listTitle);
                        Expression<Func<IListItem, object>>[] selectors =
                        {
                            li => li.All
                        };

                        // Use CAML Query
                        await list2.LoadItemsByCamlQueryAsync(new CamlQueryOptions()
                        {
                            ViewXml = viewXml,
                            DatesInUtc = true
                        }).ConfigureAwait(false);

                        IListItem listItem = list2.Items.AsRequested().FirstOrDefault();

                        Assert.IsNotNull(listItem);

                        Assert.AreEqual("Yes", listItem.Values["With_x0020_Space"]);
                        Assert.AreEqual("Yes", listItem.Values["With_Underscore"]);
                        Assert.AreEqual("Yes", listItem.Values["With_x0020_SpaceAnd_Underscore"]);

                        // ListDataAsStream loading
                        list2.Items.Clear();

                        var output = await list2.LoadListDataAsStreamAsync(new RenderListDataOptions()
                        {
                            ViewXml = viewXml,
                            RenderOptions = RenderListDataOptionsFlags.ListData
                        }).ConfigureAwait(false);

                        listItem = list2.Items.AsRequested().FirstOrDefault();

                        Assert.AreEqual("Yes", listItem.Values["With_x0020_Space"]);
                        Assert.AreEqual("Yes", listItem.Values["With_Underscore"]);
                        Assert.AreEqual("Yes", listItem.Values["With_x0020_SpaceAnd_Underscore"]);

                        // Use REST based item loading 
                        list2.Items.Clear();

                        listItem = await list2.Items.GetByIdAsync(1);

                        Assert.AreEqual("Yes", listItem.Values["With_x0020_Space"]);
                        Assert.AreEqual("Yes", listItem.Values["With_Underscore"]);
                        Assert.AreEqual("Yes", listItem.Values["With_x0020_SpaceAnd_Underscore"]);

                        // Verify updates: REST
                        listItem.Values["With_x0020_SpaceAnd_Underscore"] = "No";
                        await listItem.UpdateAsync();

                        // Read again to verify
                        list2.Items.Clear();
                        listItem = await list2.Items.GetByIdAsync(1);
                        Assert.AreEqual("No", listItem.Values["With_x0020_SpaceAnd_Underscore"]);

                        // Verify updates: CSOM
                        listItem.Values["With_x0020_SpaceAnd_Underscore"] = "Yes";
                        await listItem.SystemUpdateAsync();

                        // Read again to verify
                        list2.Items.Clear();
                        listItem = await list2.Items.GetByIdAsync(1);
                        Assert.AreEqual("Yes", listItem.Values["With_x0020_SpaceAnd_Underscore"]);
                    }

                }
                finally
                {
                    // Delete the library again
                    await list.DeleteAsync();
                }
            }
        }

        #endregion

        #region Load File

        [TestMethod]
        public async Task LoadFileFromListItemTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string parentLibraryName, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            try
            {
                int listItemID = -1;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IFile file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.ListItemAllFields.QueryProperties(li => li.Id));

                    listItemID = file.ListItemAllFields.Id;

                    Assert.IsTrue(listItemID > 0);
                }

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    IList list = await context.Web.Lists.GetByTitleAsync(parentLibraryName);
                    IListItem listItem = await list.Items.GetByIdAsync(listItemID);

                    Assert.IsNotNull(listItem);

                    await listItem.File.LoadAsync();

                    Assert.IsNotNull(listItem.File);
                    Assert.IsTrue(listItem.File.Length > 0);
                    Assert.IsTrue(!string.IsNullOrWhiteSpace(listItem.File.ServerRelativeUrl));
                    Assert.IsTrue(listItem.File.IsPropertyAvailable(f => f.CheckOutType));
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(3);
            }
        }

        [TestMethod]
        public async Task LoadFileWithPropertiesFromListItemTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string parentLibraryName, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            try
            {
                int listItemID = -1;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IFile file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.ListItemAllFields.QueryProperties(li => li.Id));

                    listItemID = file.ListItemAllFields.Id;

                    Assert.IsTrue(listItemID > 0);
                }

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    IList list = await context.Web.Lists.GetByTitleAsync(parentLibraryName);
                    IListItem listItem = await list.Items.GetByIdAsync(listItemID);

                    Assert.IsNotNull(listItem);

                    await listItem.File.LoadAsync(f => f.Length, f => f.Author.QueryProperties(u => u.UserPrincipalName));

                    Assert.IsNotNull(listItem.File);
                    Assert.IsTrue(listItem.File.Length > 0);
                    Assert.IsTrue(!string.IsNullOrWhiteSpace(listItem.File.Author.UserPrincipalName));
                    Assert.IsTrue(!listItem.File.IsPropertyAvailable(f => f.CheckOutType));
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(3);
            }
        }

        [TestMethod]
        public async Task LoadFileInBatchWithPropertiesFromListItemTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string parentLibraryName, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);
            (_, _, string documentUrl2) = await TestAssets.CreateTestDocumentAsync(1, fileName: $"{nameof(LoadFileInBatchWithPropertiesFromListItemTest)}2");

            try
            {
                int listItemID = -1;
                int listItemID2 = -1;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    IFile file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.ListItemAllFields.QueryProperties(li => li.Id));
                    IFile file2 = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl2, f => f.ListItemAllFields.QueryProperties(li => li.Id));

                    listItemID = file.ListItemAllFields.Id;
                    listItemID2 = file2.ListItemAllFields.Id;

                    Assert.IsTrue(listItemID > 0);
                    Assert.IsTrue(listItemID2 > 0);
                }

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
                {
                    IList list = await context.Web.Lists.GetByTitleAsync(parentLibraryName);
                    IListItem listItem = await list.Items.GetByIdAsync(listItemID);
                    IListItem listItem2 = await list.Items.GetByIdAsync(listItemID2);

                    Assert.IsNotNull(listItem);
                    Assert.IsNotNull(listItem2);

                    var batch = context.NewBatch();
                    await listItem.File.LoadBatchAsync(batch, f => f.Length, f => f.Author.QueryProperties(u => u.UserPrincipalName));
                    await listItem2.File.LoadBatchAsync(batch, f => f.Length, f => f.Author.QueryProperties(u => u.UserPrincipalName));
                    await context.ExecuteAsync(batch);

                    Assert.IsNotNull(listItem.File);
                    Assert.IsTrue(listItem.File.Length > 0);
                    Assert.IsTrue(!string.IsNullOrWhiteSpace(listItem.File.Author.UserPrincipalName));
                    Assert.IsTrue(!listItem.File.IsPropertyAvailable(f => f.CheckOutType));

                    Assert.IsNotNull(listItem2.File);
                    Assert.IsTrue(listItem2.File.Length > 0);
                    Assert.IsTrue(!string.IsNullOrWhiteSpace(listItem2.File.Author.UserPrincipalName));
                    Assert.IsTrue(!listItem2.File.IsPropertyAvailable(f => f.CheckOutType));
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(4);
            }
        }

        #endregion

        #region GetDisplayName
        [TestMethod]
        public async Task GetFileDisplayNameAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string parentLibraryName, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IFile file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.ListItemAllFields.QueryProperties(li => li.Id));

                    Assert.IsNotNull(file);
                    Assert.IsNotNull(file.ListItemAllFields);
                    Assert.IsTrue(file.ListItemAllFields.Id > 0);

                    IList list = await context.Web.Lists.GetByTitleAsync(parentLibraryName);

                    Assert.IsNotNull(list);

                    IListItem listItem = await list.Items.GetByIdAsync(file.ListItemAllFields.Id);

                    Assert.IsNotNull(listItem);

                    string displayName = await listItem.GetDisplayNameAsync();

                    Assert.IsFalse(string.IsNullOrWhiteSpace(displayName));
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(2);
            }
        }

        [TestMethod]
        public async Task GetFileDisplayNameTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string parentLibraryName, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IFile file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.ListItemAllFields.QueryProperties(li => li.Id));

                    Assert.IsNotNull(file);
                    Assert.IsNotNull(file.ListItemAllFields);
                    Assert.IsTrue(file.ListItemAllFields.Id > 0);

                    IList list = await context.Web.Lists.GetByTitleAsync(parentLibraryName);

                    Assert.IsNotNull(list);

                    IListItem listItem = await list.Items.GetByIdAsync(file.ListItemAllFields.Id);

                    Assert.IsNotNull(listItem);

                    string displayName = listItem.GetDisplayName();

                    Assert.IsFalse(string.IsNullOrWhiteSpace(displayName));
                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(2);
            }
        }

        [TestMethod]
        public async Task GetDisplayNameAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            try
            {
                (string parentListName, int itemId, _) = await TestAssets.CreateTestListItemAsync(0);

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IList list = await context.Web.Lists.GetByTitleAsync(parentListName);

                    Assert.IsNotNull(list);

                    IListItem listItem = await list.Items.GetByIdAsync(itemId);

                    Assert.IsNotNull(listItem);

                    string displayName = await listItem.GetDisplayNameAsync();

                    Assert.IsFalse(string.IsNullOrWhiteSpace(displayName));
                }
            }
            finally
            {
                await TestAssets.CleanupTestDedicatedListAsync(2);
            }
        }

        [TestMethod]
        public async Task GetDisplayNameTest()
        {
            //TestCommon.Instance.Mocking = false;
            try
            {
                (string parentListName, int itemId, _) = await TestAssets.CreateTestListItemAsync(0);

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IList list = await context.Web.Lists.GetByTitleAsync(parentListName);

                    Assert.IsNotNull(list);

                    IListItem listItem = await list.Items.GetByIdAsync(itemId);

                    Assert.IsNotNull(listItem);

                    string displayName = listItem.GetDisplayName();

                    Assert.IsFalse(string.IsNullOrWhiteSpace(displayName));
                }
            }
            finally
            {
                await TestAssets.CleanupTestDedicatedListAsync(2);
            }
        }
        #endregion

        #region Changes

        [TestMethod]
        public async Task GetListItemChangesAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            (string parentLibraryName, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    IFile file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.ListItemAllFields.QueryProperties(li => li.Id));

                    Assert.IsNotNull(file);
                    Assert.IsNotNull(file.ListItemAllFields);
                    Assert.IsTrue(file.ListItemAllFields.Id > 0);

                    IList list = await context.Web.Lists.GetByTitleAsync(parentLibraryName);

                    Assert.IsNotNull(list);

                    IListItem listItem = await list.Items.GetByIdAsync(file.ListItemAllFields.Id);

                    Assert.IsNotNull(listItem);

                    var changes = await listItem.GetChangesAsync(new ChangeQueryOptions(true, true)
                    {
                        FetchLimit = 5,
                    });

                    Assert.IsNotNull(changes);
                    Assert.IsTrue(changes.Count == 1);

                    var changeItem = changes[0] as IChangeItem;
                    Assert.IsNotNull(changeItem);
                    Assert.AreEqual(ChangeType.Add, changeItem.ChangeType);
                    Assert.IsTrue(changeItem.ItemId > 0);
                    Assert.IsTrue(!string.IsNullOrWhiteSpace(changeItem.Editor));
                    Assert.IsTrue(!string.IsNullOrWhiteSpace(changeItem.ServerRelativeUrl));
                    Assert.AreNotEqual(Guid.Empty, changeItem.UniqueId);
                    Assert.AreNotEqual(Guid.Empty, changeItem.WebId);
                    Assert.AreNotEqual(Guid.Empty, changeItem.SiteId);
                    Assert.AreNotEqual(Guid.Empty, changeItem.ListId);

                    var changes2 = listItem.GetChanges(new ChangeQueryOptions(true, true)
                    {
                        FetchLimit = 5,
                    });

                    Assert.IsNotNull(changes2);
                    Assert.IsTrue(changes2.Count == 1);

                    var changesBatch2 = listItem.GetChangesBatch(new ChangeQueryOptions(true, true)
                    {
                        FetchLimit = 5,
                    });

                    Assert.IsFalse(changesBatch2.IsAvailable);

                    context.Execute();

                    Assert.IsTrue(changesBatch2.IsAvailable);
                    Assert.IsTrue(changes2.Count == 1);

                }
            }
            finally
            {
                await TestAssets.CleanupTestDocumentAsync(2);
            }
        }

        #endregion

        #region Item versions

        [TestMethod]
        public async Task GetListItemVersionsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            string listTitle = "ListItemVersionsTest1";
            int firstId;
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
                for (int i = 0; i < 5; i++)
                {
                    var values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                    await myList.Items.AddBatchAsync(values);
                }
                await context.ExecuteAsync();

                var first = myList.Items.AsRequested().First();
                firstId = first.Id;
                first.Title = "blabla";

                // Use the batch update flow here
                var batch = context.NewBatch();
                await first.UpdateBatchAsync(batch).ConfigureAwait(false);
                await context.ExecuteAsync(batch);
            }

            int versionId;
            using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var myList2 = await context2.Web.Lists.GetByTitleAsync(listTitle);
                var first2 = await myList2.Items.GetByIdAsync(firstId, li => li.All, li => li.Versions);

                var lastVersion = first2.Versions.AsRequested().Last();
                versionId = lastVersion.Id;

                Assert.AreEqual("blabla", first2.Title);
                Assert.AreEqual("2.0", first2.Values["_UIVersionString"].ToString());

                Assert.AreEqual(2, first2.Versions.Length);
                Assert.AreEqual("Item 0", lastVersion.Values["Title"].ToString());
            }

            using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                var myList3 = await context3.Web.Lists.GetByTitleAsync(listTitle);
                var first3 = await myList3.Items.GetByIdAsync(firstId, li => li.Id);
                var firstVersion = await first3.Versions.GetByIdAsync(versionId, v => v.All, v => v.Fields.QueryProperties(f => f.InternalName));

                Assert.AreEqual(versionId, firstVersion.Id);
                Assert.IsFalse(firstVersion.IsCurrentVersion);
                Assert.AreEqual("1.0", firstVersion.VersionLabel);
                Assert.AreEqual("Item 0", firstVersion.Values["Title"].ToString());
                Assert.IsTrue(firstVersion.Fields.AsRequested().Any());
            }

            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
            {
                var myList = await contextFinal.Web.Lists.GetByTitleAsync(listTitle);

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task DeleteListItemVersionsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            string listTitle = "DeleteListItemVersionsTest";
            int firstId;
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
                for (int i = 0; i < 5; i++)
                {
                    var values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                    await myList.Items.AddBatchAsync(values);
                }
                await context.ExecuteAsync();

                var first = myList.Items.AsRequested().First();
                firstId = first.Id;
                first.Title = "blabla";

                // Use the batch update flow here
                var batch = context.NewBatch();
                await first.UpdateBatchAsync(batch).ConfigureAwait(false);
                await context.ExecuteAsync(batch);
            }

            int versionId;
            using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var myList2 = await context2.Web.Lists.GetByTitleAsync(listTitle);
                var first2 = await myList2.Items.GetByIdAsync(firstId, li => li.All, li => li.Versions);

                var lastVersion = first2.Versions.AsRequested().Last();
                versionId = lastVersion.Id;

                Assert.AreEqual("blabla", first2.Title);
                Assert.AreEqual("2.0", first2.Values["_UIVersionString"].ToString());

                Assert.AreEqual(2, first2.Versions.Length);
                Assert.AreEqual("Item 0", lastVersion.Values["Title"].ToString());

                // Delete the last version
                lastVersion.Delete();

                first2 = await myList2.Items.GetByIdAsync(firstId, li => li.All, li => li.Versions);
                lastVersion = first2.Versions.AsRequested().Last();
                Assert.AreEqual(1, first2.Versions.Length);
                Assert.AreEqual("blabla", lastVersion.Values["Title"].ToString());
            }

            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
            {
                var myList = await contextFinal.Web.Lists.GetByTitleAsync(listTitle);

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }


        [TestMethod]
        public async Task GetListItemVersionFileVersionContentAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            const string fileContent = "PnP Rocks !!!";

            int firstId;
            (string libraryTitle, _, _) = await TestAssets.CreateTestDocumentInDedicatedLibraryAsync(0, parentLibraryEnableVersioning: true);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var myLibrary = await context.Web.Lists.GetByTitleAsync(libraryTitle, l => l.RootFolder);

                var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
                var documentName = $"{nameof(GetListItemVersionFileVersionContentAsyncTest)}.txt";
                var testDocument = await myLibrary.RootFolder.Files.AddAsync(documentName, contentStream);
                testDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocument.ServerRelativeUrl, f => f.ListItemAllFields);

                var listItem = testDocument.ListItemAllFields;
                firstId = listItem.Id;
                listItem.Title = "blabla";

                // Use the batch update flow here
                var batch = context.NewBatch();
                await listItem.UpdateBatchAsync(batch).ConfigureAwait(false);
                await context.ExecuteAsync(batch);
            }

            int versionId;
            using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                var myList2 = await context2.Web.Lists.GetByTitleAsync(libraryTitle);
                var first2 = await myList2.Items.GetByIdAsync(firstId, li => li.All, li => li.Versions);

                var lastVersion = first2.Versions.AsRequested().Last();
                versionId = lastVersion.Id;

                Assert.AreEqual("blabla", first2.Title);
                Assert.AreEqual("2.0", first2.Values["_UIVersionString"].ToString());

                Assert.AreEqual(2, first2.Versions.Length);
            }

            using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
            {
                var myList3 = await context3.Web.Lists.GetByTitleAsync(libraryTitle);
                var first3 = await myList3.Items.GetByIdAsync(firstId, li => li.Id);
                var firstVersion = await first3.Versions.GetByIdAsync(versionId, v => v.FileVersion);

                Assert.AreEqual(versionId, firstVersion.Id);
                Assert.IsNotNull(firstVersion.FileVersion);

                /* TODO: Finish this*/
                // Download document version content
                Stream downloadedContentStream = await firstVersion.FileVersion.GetContentAsync();
                downloadedContentStream.Seek(0, SeekOrigin.Begin);
                // Get string from the content stream
                string downloadedContent = await new StreamReader(downloadedContentStream).ReadToEndAsync();

                Assert.IsTrue(!string.IsNullOrEmpty(downloadedContent));
                Assert.AreEqual(fileContent, downloadedContent);
                /**/
            }

            await TestAssets.CleanupTestDedicatedListAsync(4);
        }

        //[TestMethod]
        //public async Task FieldTypeReadUrl()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        /*
        //        var list = await context.Web.Lists.GetByTitleAsync("FieldTypes");
        #endregion

        #region Comments

        [TestMethod]
        public async Task ListItemCommentsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("ListItemCommentsTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                var item = await list.Items.AddAsync(new Dictionary<string, object> { { "Title", "Comment me" } });

                // get list item comments
                var comments = await item.GetCommentsAsync();
                Assert.IsTrue(comments.Length == 0);

                // add comment
                var comment = await comments.AddAsync("this is great");

                Assert.IsTrue(comment != null);
                Assert.IsTrue(comment.Id == "1");

                // get comments again
                var comments2 = item.GetComments();
                Assert.IsTrue(comments2.Length == 1);

                var firstComment = comments2.AsRequested().First();

                Assert.IsTrue(firstComment.CreatedDate < DateTime.Now);
                Assert.IsTrue(firstComment.Id == "1");
                Assert.IsTrue(firstComment.IsLikedByUser == false);
                Assert.IsTrue(firstComment.IsReply == false);
                Assert.IsTrue(firstComment.ItemId == 1);
                Assert.IsTrue(firstComment.LikeCount == 0);
                Assert.IsTrue(firstComment.ListId == list.Id);
                Assert.IsTrue(firstComment.ParentId == "0");
                Assert.IsTrue(firstComment.ReplyCount == 0);
                Assert.IsTrue(firstComment.Text == "this is great");

                var commentAuthor = firstComment.Author;

                Assert.IsTrue(!string.IsNullOrEmpty(commentAuthor.Mail));
                Assert.IsTrue(commentAuthor.Expiration == null);
                Assert.IsTrue(commentAuthor.Id > 0);
                Assert.IsTrue(commentAuthor.IsActive == true);
                Assert.IsTrue(commentAuthor.IsExternal == false);
                Assert.IsTrue(commentAuthor.JobTitle == null);
                Assert.IsTrue(!string.IsNullOrEmpty(commentAuthor.LoginName));
                Assert.IsTrue(!string.IsNullOrEmpty(commentAuthor.Name));
                Assert.IsTrue(commentAuthor.PrincipalType == PrincipalType.User);
                Assert.IsTrue(commentAuthor.UserPrincipalName == null);

                // Delete comment
                await firstComment.DeleteAsync();

                // get comments again
                var comments3 = await item.GetCommentsAsync();
                Assert.IsTrue(comments3.Length == 0);

                // Cleanup the created list
                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ListItemCommentsLikeUnLikeTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("ListItemCommentsLikeUnLikeTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                var item = await list.Items.AddAsync(new Dictionary<string, object> { { "Title", "Comment me" } });

                // get list item comments
                var comments = await item.GetCommentsAsync();
                Assert.IsTrue(comments.Length == 0);

                // add comment
                var comment = await comments.AddAsync("this is great");

                Assert.IsTrue(comment != null);
                Assert.IsTrue(comment.Id == "1");

                // Like the comment
                comment.Like();

                // get comments again
                var comments2 = item.GetComments();
                Assert.IsTrue(comments2.Length == 1);

                var firstComment = comments2.AsRequested().First();

                Assert.IsTrue(firstComment.IsLikedByUser == true);
                Assert.IsTrue(firstComment.Text == "this is great");

                await firstComment.LoadAsync(p => p.LikedBy);
                Assert.IsTrue(firstComment.LikedBy.Length == 1);
                var firstLikedByUser = firstComment.LikedBy.AsRequested().First();
                Assert.IsNotNull(firstLikedByUser.Name);
                Assert.IsTrue(firstLikedByUser.Id > 0);
                Assert.IsNotNull(firstLikedByUser.Mail);
                Assert.IsNotNull(firstLikedByUser.LoginName);

                // unlike the comment
                firstComment.Unlike();

                var comments3 = item.GetComments();
                Assert.IsTrue(comments3.Length == 1);

                firstComment = comments3.AsRequested().First();

                Assert.IsTrue(firstComment.IsLikedByUser == false);
                Assert.IsTrue(firstComment.Text == "this is great");

                // Delete comment
                await firstComment.DeleteAsync();

                // get comments again
                var comments4 = await item.GetCommentsAsync();
                Assert.IsTrue(comments4.Length == 0);

                // Cleanup the created list
                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ListItemCommentsBatchAddTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("ListItemCommentsBatchAddTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                var item = await list.Items.AddAsync(new Dictionary<string, object> { { "Title", "Comment me" } });

                // get list item comments
                var comments = await item.GetCommentsAsync();
                Assert.IsTrue(comments.Length == 0);

                // add comment
                var comment1 = comments.AddBatch("this is great 1");
                var comment2 = comments.AddBatch("this is great 2");
                var comment3 = await comments.AddBatchAsync("this is great 3");

                // Execute batch
                await context.ExecuteAsync();

                Assert.IsTrue(comment1 != null);
                Assert.IsTrue(comment1.Id == "1");
                Assert.IsTrue(comment2 != null);
                Assert.IsTrue(comment2.Id == "2");
                Assert.IsTrue(comment3 != null);
                Assert.IsTrue(comment3.Id == "3");

                // get comments again
                var comments2 = item.GetComments();
                Assert.IsTrue(comments2.Length == 3);

                var firstComment = comments2.AsRequested().First();

                Assert.IsTrue(firstComment.CreatedDate < DateTime.Now);
                Assert.IsTrue(firstComment.Id == "3");
                Assert.IsTrue(firstComment.IsLikedByUser == false);
                Assert.IsTrue(firstComment.IsReply == false);
                Assert.IsTrue(firstComment.ItemId == 1);
                Assert.IsTrue(firstComment.LikeCount == 0);
                Assert.IsTrue(firstComment.ListId == list.Id);
                Assert.IsTrue(firstComment.ParentId == "0");
                Assert.IsTrue(firstComment.ReplyCount == 0);
                Assert.IsTrue(firstComment.Text == "this is great 3");

                var commentAuthor = firstComment.Author;

                Assert.IsTrue(!string.IsNullOrEmpty(commentAuthor.Mail));
                Assert.IsTrue(commentAuthor.Expiration == null);
                Assert.IsTrue(commentAuthor.Id > 0);
                Assert.IsTrue(commentAuthor.IsActive == true);
                Assert.IsTrue(commentAuthor.IsExternal == false);
                Assert.IsTrue(commentAuthor.JobTitle == null);
                Assert.IsTrue(!string.IsNullOrEmpty(commentAuthor.LoginName));
                Assert.IsTrue(!string.IsNullOrEmpty(commentAuthor.Name));
                Assert.IsTrue(commentAuthor.PrincipalType == PrincipalType.User);
                Assert.IsTrue(commentAuthor.UserPrincipalName == null);

                // Delete all comment
                comments2.DeleteAll();
                Assert.IsTrue(comments2.Length == 0);

                // get comments again
                var comments3 = await item.GetCommentsAsync();
                Assert.IsTrue(comments3.Length == 0);

                // Cleanup the created list
                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ListItemCommentsReplyTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("ListItemCommentsReplyTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                var item = await list.Items.AddAsync(new Dictionary<string, object> { { "Title", "Comment me" } });

                // get list item comments
                var comments = await item.GetCommentsAsync();
                Assert.IsTrue(comments.Length == 0);

                // add comment
                var comment = await comments.AddAsync("this is great");

                Assert.IsTrue(comment != null);
                Assert.IsTrue(comment.Id == "1");

                // add a reply to the comment
                var reply = await comment.Replies.AddAsync("this is a reply");

                // Verify all reply comment properties are loaded
                Assert.IsTrue(reply != null);
                Assert.IsTrue(reply.CreatedDate < DateTime.Now);
                Assert.IsTrue(reply.Id == "2");
                Assert.IsTrue(reply.IsLikedByUser == false);
                Assert.IsTrue(reply.IsReply == true);
                Assert.IsTrue(reply.ItemId == 1);
                Assert.IsTrue(reply.LikeCount == 0);
                Assert.IsTrue(reply.ListId == list.Id);
                Assert.IsTrue(reply.ParentId == "1");
                Assert.IsTrue(reply.ReplyCount == 0);
                Assert.IsTrue(reply.Text == "this is a reply");

                var commentAuthor = reply.Author;

                Assert.IsTrue(!string.IsNullOrEmpty(commentAuthor.Mail));
                Assert.IsTrue(commentAuthor.Expiration == null);
                Assert.IsTrue(commentAuthor.Id > 0);
                Assert.IsTrue(commentAuthor.IsActive == true);
                Assert.IsTrue(commentAuthor.IsExternal == false);
                Assert.IsTrue(commentAuthor.JobTitle == null);
                Assert.IsTrue(!string.IsNullOrEmpty(commentAuthor.LoginName));
                Assert.IsTrue(!string.IsNullOrEmpty(commentAuthor.Name));
                Assert.IsTrue(commentAuthor.PrincipalType == PrincipalType.User);
                Assert.IsTrue(commentAuthor.UserPrincipalName != null);

                // Load the comments with replies and verify the reply collection is now populated
                comments = await item.GetCommentsAsync(p => p.Replies);

                var firstComment = comments.AsRequested().First();

                Assert.IsTrue(firstComment.Id == "1");
                Assert.IsTrue(firstComment.Replies.Length == 1);

                var firstCommentReply = firstComment.Replies.AsRequested().First();
                Assert.IsTrue(firstCommentReply.IsReply == true);
                Assert.IsTrue(firstCommentReply.ParentId == "1");

                // Delete the reply again
                await firstCommentReply.DeleteAsync();

                comments = await item.GetCommentsAsync(p => p.Replies);
                firstComment = comments.AsRequested().First();

                Assert.IsTrue(firstComment.Id == "1");
                Assert.IsTrue(firstComment.Replies.Length == 0);

                // Cleanup the created list
                await list.DeleteAsync();
            }
        }

        #endregion

        #region Attachments

        [TestMethod]
        public async Task ListItemAttachmentsTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("ListItemAttachmentsTest");
                var list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                var item = await list.Items.AddAsync(new Dictionary<string, object> { { "Title", "Attach files to me" } });

                // load the item with attachments again
                var itemLoaded = await list.Items.GetByIdAsync(item.Id, p => p.AttachmentFiles);
                Assert.IsTrue(itemLoaded.AttachmentFiles.Length == 0);

                string fileContent = "PnP Rocks !!!";
                var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
                string fileName = TestCommon.GetPnPSdkTestAssetName("test_added.txt");
                var addedAttachment = itemLoaded.AttachmentFiles.Add(fileName, contentStream);

                // load the item with attachments again
                itemLoaded = await list.Items.GetByIdAsync(item.Id, p => p.AttachmentFiles);
                Assert.IsTrue(itemLoaded.AttachmentFiles.Length == 1);

                // Get the content from the attachment
                Stream downloadedContentStream = itemLoaded.AttachmentFiles.AsRequested().First().GetContent();
                downloadedContentStream.Seek(0, SeekOrigin.Begin);
                // Get string from the content stream
                string downloadedContent = new StreamReader(downloadedContentStream).ReadToEnd();

                Assert.AreEqual(fileContent, downloadedContent);

                // remove the added attachment again
                itemLoaded.AttachmentFiles.AsRequested().First().Delete();

                itemLoaded = await list.Items.GetByIdAsync(item.Id, p => p.AttachmentFiles);
                Assert.IsTrue(itemLoaded.AttachmentFiles.Length == 0);

                // Add another attachment
                fileName = TestCommon.GetPnPSdkTestAssetName("test_added.docx");
                addedAttachment = itemLoaded.AttachmentFiles.Add(fileName, System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"));

                itemLoaded = await list.Items.GetByIdAsync(item.Id, p => p.AttachmentFiles);
                Assert.IsTrue(itemLoaded.AttachmentFiles.Length == 1);

                // recycle the attachment
                itemLoaded.AttachmentFiles.AsRequested().First().Recycle();

                itemLoaded = await list.Items.GetByIdAsync(item.Id, p => p.AttachmentFiles);
                Assert.IsTrue(itemLoaded.AttachmentFiles.Length == 0);

                // Cleanup the created list
                await list.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ListItemAttachmentsSpecialCharTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("ListItemAttachmentsSpecialCharTest");
                IList list = null;
                try
                {
                    list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    var item = await list.Items.AddAsync(new Dictionary<string, object> { { "Title", "Attach files to me" } });

                    // load the item with attachments again
                    var itemLoaded = await list.Items.GetByIdAsync(item.Id, p => p.AttachmentFiles);
                    Assert.IsTrue(itemLoaded.AttachmentFiles.Length == 0);

                    string fileContent = "PnP Rocks !!!";
                    var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
                    string fileName = "I'm special.txt";
                    var addedAttachment = itemLoaded.AttachmentFiles.Add(fileName, contentStream);

                    // load the item with attachments again
                    itemLoaded = await list.Items.GetByIdAsync(item.Id, p => p.AttachmentFiles);
                    Assert.IsTrue(itemLoaded.AttachmentFiles.Length == 1);

                    // Get the content from the attachment
                    Stream downloadedContentStream = itemLoaded.AttachmentFiles.AsRequested().First().GetContent();
                    downloadedContentStream.Seek(0, SeekOrigin.Begin);
                    // Get string from the content stream
                    string downloadedContent = new StreamReader(downloadedContentStream).ReadToEnd();

                    Assert.AreEqual(fileContent, downloadedContent);

                    // remove the added attachment again
                    itemLoaded.AttachmentFiles.AsRequested().First().Delete();

                    itemLoaded = await list.Items.GetByIdAsync(item.Id, p => p.AttachmentFiles);
                    Assert.IsTrue(itemLoaded.AttachmentFiles.Length == 0);

                }
                finally
                {
                    // Cleanup the created list
                    await list.DeleteAsync();
                }
            }
        }
        #endregion

        #region Compliance
        [TestMethod]
        public async Task ComplianceTagTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {

                // Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("ListItemComplianceTagTest");
                var myList = context.Web.Lists.GetByTitle(listTitle);

                try
                {
                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    }

                    // Add an item
                    var addedItem = await myList.Items.AddAsync(new Dictionary<string, object>() { { "Title", "Test" } });

                    // Add Compliance tag
                    addedItem.SetComplianceTag("Retain1Year", false, false, false, false);

                    // Read the set compliance tag
                    using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                    {
                        var list2 = context2.Web.Lists.GetByTitle(listTitle);
                        if (list2 != null)
                        {
                            await list2.LoadListDataAsStreamAsync(new RenderListDataOptions() { ViewXml = "<View><ViewFields><FieldRef Name='Title' /><FieldRef Name='_ComplianceTag' /></ViewFields><RowLimit>5</RowLimit></View>", RenderOptions = RenderListDataOptionsFlags.ListData });
                            Assert.IsTrue(list2.Items.Length == 1);
                            var firstItem = list2.Items.AsRequested().First();

                            Assert.IsTrue(firstItem.Values["_ComplianceTag"].ToString() == "Retain1Year");
                        }
                    }
                }
                finally
                {
                    await myList.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task ComplianceTagBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {

                // Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("ListItemComplianceTagBatchTest");
                var myList = context.Web.Lists.GetByTitle(listTitle);

                try
                {
                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    }

                    // Add an item
                    var addedItem = await myList.Items.AddAsync(new Dictionary<string, object>() { { "Title", "Test" } });

                    // Add Compliance tag
                    addedItem.SetComplianceTagBatch("Retain1Year", false, false, false, false);
                    // Add something else to force batch end point being used
                    myList.ContentTypesEnabled = true;
                    myList.UpdateBatch();

                    // Execute batch
                    await context.ExecuteAsync();

                    // Read the set compliance tag
                    using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                    {
                        var list2 = context2.Web.Lists.GetByTitle(listTitle);
                        if (list2 != null)
                        {
                            await list2.LoadListDataAsStreamAsync(new RenderListDataOptions() { ViewXml = "<View><ViewFields><FieldRef Name='Title' /><FieldRef Name='_ComplianceTag' /></ViewFields><RowLimit>5</RowLimit></View>", RenderOptions = RenderListDataOptionsFlags.ListData });
                            Assert.IsTrue(list2.Items.Length == 1);
                            var firstItem = list2.Items.AsRequested().First();

                            Assert.IsTrue(firstItem.Values["_ComplianceTag"].ToString() == "Retain1Year");
                        }
                    }
                }
                finally
                {
                    await myList.DeleteAsync();
                }
            }
        }
        #endregion

        #region Effective user permissions

        [TestMethod]
        public async Task GetEffectiveUserPermissionsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("GetEffectiveUserPermissionsAsyncTest");
                IList list = null;

                try
                {
                    list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    var item = await list.Items.AddAsync(new Dictionary<string, object> { { "Title", "This is an item" } });

                    var siteUser = await context.Web.SiteUsers.FirstOrDefaultAsync(y => y.PrincipalType == Model.Security.PrincipalType.User);

                    var basePermissions = await item.GetUserEffectivePermissionsAsync(siteUser.UserPrincipalName);

                    Assert.IsNotNull(basePermissions);
                }
                finally
                {
                    await list.DeleteAsync();

                }
            }
        }


        [TestMethod]
        public async Task CheckIfUserHasPermissionsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("CheckIfUserHasPermissionsAsyncTest");
                IList list = null;

                try
                {
                    list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    var item = await list.Items.AddAsync(new Dictionary<string, object> { { "Title", "This is an item" } });

                    var siteUser = await context.Web.SiteUsers.FirstOrDefaultAsync(y => y.PrincipalType == Model.Security.PrincipalType.User);

                    var hasPermissions = await item.CheckIfUserHasPermissionsAsync(siteUser.UserPrincipalName, PermissionKind.AddListItems);

                    Assert.IsNotNull(hasPermissions);
                }
                finally
                {
                    await list.DeleteAsync();
                }
            }
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public async Task CheckIfUserHasPermissionsExceptionAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var listTitle = TestCommon.GetPnPSdkTestAssetName("CheckIfUserHasPermissionsExceptionAsyncTest");
                IList list = null;

                try
                {
                    list = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    var item = await list.Items.AddAsync(new Dictionary<string, object> { { "Title", "This is an item" } });

                    var hasPermissions = await item.CheckIfUserHasPermissionsAsync(null, PermissionKind.AddListItems);
                }
                finally
                {
                    await list.DeleteAsync();
                }
            }
        }

        #endregion
    }
}
