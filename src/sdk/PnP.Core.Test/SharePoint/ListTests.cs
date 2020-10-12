using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class ListTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }
        
        [TestMethod]
        public async Task GetListDataByRenderListDataAsStream()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "GetListDataByRenderListDataAsStream";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
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

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        var result = await list2.GetListDataAsStreamAsync(new RenderListDataOptions() { ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>", RenderOptions = RenderListDataOptionsFlags.ListData });
                        Assert.IsTrue(list2.Items.Count() == 5);
                        Assert.IsTrue(result.ContainsKey("FirstRow"));
                        Assert.IsTrue(result.ContainsKey("LastRow"));
                        Assert.IsTrue(result.ContainsKey("RowLimit"));
                        Assert.IsTrue((int)result["RowLimit"] == 5);
                    }
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var list3 = context3.Web.Lists.GetByTitle(listTitle);
                    if (list3 != null)
                    {
                        var result = list3.GetListDataAsStream(new RenderListDataOptions() { ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>", RenderOptions = RenderListDataOptionsFlags.ListData });
                        Assert.IsTrue(list3.Items.Count() == 5);
                        Assert.IsTrue(result.ContainsKey("FirstRow"));
                        Assert.IsTrue(result.ContainsKey("LastRow"));
                        Assert.IsTrue(result.ContainsKey("RowLimit"));
                        Assert.IsTrue((int)result["RowLimit"] == 5);
                    }
                }

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }
        

        [TestMethod]
        public async Task ListLinqGetMethods()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "ListLinqGetMethods";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                var listGuid = myList.Id;

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        Assert.IsTrue(list2.Title == listTitle);
                        Assert.IsTrue(list2.Id == listGuid);
                    }
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    context3.GraphFirst = false;

                    var list3 = context3.Web.Lists.GetById(listGuid, p=>p.TemplateType, p=>p.Title);
                    if (list3 != null)
                    {
                        Assert.IsTrue(list3.Title == listTitle);
                        Assert.IsTrue(list3.Id == listGuid);
                    }
                }

                using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
                {
                    var list4 = context4.Web.Lists.GetByTitle(listTitle);
                    if (list4 != null)
                    {
                        var listCheck = context4.Web.Lists.GetById(list4.Id);

                        Assert.IsTrue(listCheck.Title == listTitle);
                        Assert.IsTrue(listCheck.Id == listGuid);
                    }
                }


                // Cleanup the created list
                await myList.DeleteAsync();
            } 
        }

        [TestMethod]
        public async Task ListLinqGetAsyncMethods()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "ListLinqGetAsyncMethods";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                var listGuid = myList.Id;

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        Assert.IsTrue(list2.Title == listTitle);
                        Assert.IsTrue(list2.Id == listGuid);
                    }
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    context3.GraphFirst = false;

                    var list3 = await context3.Web.Lists.GetByIdAsync(listGuid, p => p.TemplateType, p => p.Title);
                    if (list3 != null)
                    {
                        Assert.IsTrue(list3.Title == listTitle);
                        Assert.IsTrue(list3.Id == listGuid);
                    }
                }

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ListLinqGetExceptionMethods()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "ListLinqGetExceptionMethods";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                var listGuid = myList.Id;

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
                            IListCollection list = null;
                            await list.GetByIdAsync(listGuid);
                        });

                        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
                            await context2.Web.Lists.GetByIdAsync(Guid.Empty, p => p.TemplateType, p => p.Title);
                        });

                        Assert.ThrowsException<ArgumentNullException>(() => {
                            IListCollection list = null;
                            list.GetById(listGuid);
                        });

                        Assert.ThrowsException<ArgumentNullException>(() => {
                            context2.Web.Lists.GetById(Guid.Empty, p => p.TemplateType, p => p.Title);
                        });

                        Assert.ThrowsException<ArgumentNullException>(() => {
                            IListCollection list = null;
                            list.GetByTitle(listTitle);
                        });

                        Assert.ThrowsException<ArgumentNullException>(() => {
                            context2.Web.Lists.GetByTitle(null);
                        });

                        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
                            IListCollection list = null;
                            await list.GetByTitleAsync(listTitle);
                        });

                        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
                            IListCollection list = null;
                            await list.GetByTitleAsync(null);
                        });

                    }
                }
                 
                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetItemsByCAMLQuery()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "GetItemsByCAMLQuery";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
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

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        await list2.GetItemsByCamlQueryAsync("<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>");
                        Assert.IsTrue(list2.Items.Count() == 5);
                    }
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var list3 = context3.Web.Lists.GetByTitle(listTitle);
                    if (list3 != null)
                    {
                        await list3.GetItemsByCamlQueryAsync(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields></View>",
                            DatesInUtc = true
                        }) ;
                        Assert.IsTrue(list3.Items.Count() == 10);
                    }
                }

                // Batch testing
                using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
                {
                    var list4 = context4.Web.Lists.GetByTitle(listTitle);
                    if (list4 != null)
                    {
                        // Perform 2 queries, the first one limited to 5 items, the second one without limits. Total should be 10 items
                        await list4.GetItemsByCamlQueryBatchAsync(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>",
                        });
                        await list4.GetItemsByCamlQueryBatchAsync(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields></View>",
                        });
                        await context4.ExecuteAsync();

                        Assert.IsTrue(list4.Items.Count() == 10);
                    }
                }

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetItemsByCAMLQuerySimpleNonAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "GetItemsByCAMLQuerySimpleAsyncTest";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = context.Web.Lists.Add(listTitle, ListTemplateType.GenericList);
                }

                // Add items to the list
                for (int i = 0; i < 10; i++)
                {
                    Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                    myList.Items.AddBatch(values);
                }
                await context.ExecuteAsync();

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        list2.GetItemsByCamlQuery("<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>");
                        Assert.IsTrue(list2.Items.Count() == 5);
                    }
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var list3 = context3.Web.Lists.GetByTitle(listTitle);
                    if (list3 != null)
                    {
                        list3.GetItemsByCamlQuery(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields></View>",
                            DatesInUtc = true
                        });
                        Assert.IsTrue(list3.Items.Count() == 10);
                    }
                }

                // Batch testing
                using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
                {
                    var list4 = context4.Web.Lists.GetByTitle(listTitle);
                    if (list4 != null)
                    {
                        // Perform 2 queries, the first one limited to 5 items, the second one without limits. Total should be 10 items
                        list4.GetItemsByCamlQueryBatch(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>",
                        });
                        list4.GetItemsByCamlQueryBatch("<View><ViewFields><FieldRef Name='Title' /></ViewFields></View>");
                        await context4.ExecuteAsync();

                        Assert.IsTrue(list4.Items.Count() == 10);
                    }
                }

                // Batch testing
                using (var context5 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 4))
                {
                    var list5 = context5.Web.Lists.GetByTitle(listTitle);
                    if (list5 != null)
                    {
                        var newBatch = context5.NewBatch();
                        // Perform 2 queries, the first one limited to 5 items, the second one without limits. Total should be 10 items
                        list5.GetItemsByCamlQueryBatch(newBatch, new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>",
                        });
                        list5.GetItemsByCamlQueryBatch(newBatch, new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields></View>",
                        });
                        context5.ExecuteAsync(newBatch).GetAwaiter().GetResult(); 

                        Assert.IsTrue(list5.Items.Count() == 10);
                    }
                }

                using (var context6 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 5))
                {
                    var list6 = context6.Web.Lists.GetByTitle(listTitle);
                    if (list6 != null)
                    {
                        var newBatch = context6.NewBatch();
                        // Perform 2 queries, the first one limited to 5 items, the second one without limits. Total should be 10 items
                        list6.GetItemsByCamlQueryBatch(newBatch, "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>");
                        context6.ExecuteAsync(newBatch).GetAwaiter().GetResult();

                        Assert.IsTrue(list6.Items.Count() == 5);
                    }
                }

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task RecycleList()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists);

                int listCount = web.Lists.Count();

                string listTitle = "RecycleList";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // recycle the list
                var recycleBinItemId = await myList.RecycleAsync();
                // A valid recycle returns a recuycle bin item id
                Assert.IsTrue(recycleBinItemId != Guid.Empty);
                // The recycled list should have been deleted from the lists collection
                Assert.IsTrue(web.Lists.Count() == listCount);
                // Loading lists again should still result in the same original list count as the added list is in the recycle bin
                await context.Web.GetAsync(p => p.Lists);
                Assert.IsTrue(web.Lists.Count() == listCount);

            }
        }


        [TestMethod]
        public async Task GetListIRMSettingsTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "GetListIRMSettingsTest";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                }

                // Enable IRM on the library
                await myList.GetAsync();
                myList.IrmEnabled = true;
                await myList.UpdateAsync();

                // Load IRM settings
                await myList.InformationRightsManagementSettings.GetAsync();

                // Verify default IRM list settings are returned
                Assert.IsTrue(myList.InformationRightsManagementSettings.Requested);
                Assert.IsTrue(myList.InformationRightsManagementSettings.AllowPrint == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.AllowScript == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.AllowWriteCopy == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.DisableDocumentBrowserView == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.DocumentAccessExpireDays == 90);
                Assert.IsTrue(myList.InformationRightsManagementSettings.DocumentLibraryProtectionExpireDate > DateTime.Today);
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableDocumentAccessExpire == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableDocumentBrowserPublishingView == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableGroupProtection == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableLicenseCacheExpire == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.GroupName == "");
                Assert.IsTrue(myList.InformationRightsManagementSettings.LicenseCacheExpireDays == 30);
                Assert.IsTrue(myList.InformationRightsManagementSettings.PolicyDescription == "");
                Assert.IsTrue(myList.InformationRightsManagementSettings.PolicyTitle == "");
                Assert.IsTrue(myList.InformationRightsManagementSettings.TemplateId == "");

                // turn off IRM again
                myList.IrmEnabled = false;
                await myList.UpdateAsync();

                // delete the list
                await myList.DeleteAsync();

            }
        }

        [TestMethod]
        public async Task GetListIRMSettingsBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listTitle = "GetListIRMSettingsBatchTest";
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                }

                // Enable IRM on the library
                await myList.GetAsync();
                myList.IrmEnabled = true;
                await myList.UpdateAsync();

                // Load IRM settings
                await myList.InformationRightsManagementSettings.GetBatchAsync();
                await context.ExecuteAsync();

                // Verify default IRM list settings are returned
                Assert.IsTrue(myList.InformationRightsManagementSettings.Requested);
                Assert.IsTrue(myList.InformationRightsManagementSettings.AllowPrint == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.AllowScript == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.AllowWriteCopy == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.DisableDocumentBrowserView == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.DocumentAccessExpireDays == 90);
                Assert.IsTrue(myList.InformationRightsManagementSettings.DocumentLibraryProtectionExpireDate > DateTime.Today);
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableDocumentAccessExpire == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableDocumentBrowserPublishingView == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableGroupProtection == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.EnableLicenseCacheExpire == false);
                Assert.IsTrue(myList.InformationRightsManagementSettings.GroupName == "");
                Assert.IsTrue(myList.InformationRightsManagementSettings.LicenseCacheExpireDays == 30);
                Assert.IsTrue(myList.InformationRightsManagementSettings.PolicyDescription == "");
                Assert.IsTrue(myList.InformationRightsManagementSettings.PolicyTitle == "");
                Assert.IsTrue(myList.InformationRightsManagementSettings.TemplateId == "");

                // turn off IRM again
                myList.IrmEnabled = false;
                await myList.UpdateAsync();

                // delete the list
                await myList.DeleteAsync();

            }
        }

        [TestMethod]
        public async Task GetListByServerRelativeUrl()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByServerRelativeUrlAsync($"{context.Uri.LocalPath}/shared%20documents", p => p.Title, p => p.ListExperience);

                Assert.IsTrue(list.Requested);
                Assert.AreEqual(list.Title, "documents", true);

            }
        }

        [TestMethod]
        public async Task GetListByServerRelativeUrlSync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByServerRelativeUrl($"{context.Uri.LocalPath}/shared%20documents", p => p.Title, p => p.ListExperience);

                Assert.IsTrue(list.Requested);
                Assert.AreEqual(list.Title, "documents", true);

            }
        }

        [TestMethod]
        public async Task GetListByTitle()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience);

                Assert.IsTrue(list.Requested);
                Assert.AreEqual(list.Title, "documents", true);

            }
        }

        [TestMethod]
        public async Task GetListByTitleSync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = context.Web.Lists.GetByTitle("Documents", p => p.Title, p => p.ListExperience);

                Assert.IsTrue(list.Requested);
                Assert.AreEqual(list.Title, "documents", true);

            }
        }

        [TestMethod]
        public async Task GetListViewAsync()
        {
            TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Title, p => p.ListExperience, p => p.Views);

                Assert.IsNotNull(list.Views);

            }
        }

    }
}
