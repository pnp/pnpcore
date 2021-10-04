using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class PagingTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        #region Graph paging tests

        [TestMethod]
        public async Task GraphCollectionPaging()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {

                string newGroupName;
                if (!TestCommon.Instance.Mocking)
                {
                    newGroupName = $"PnP-{Guid.NewGuid()}";
                    Dictionary<string, string> properties = new()
                    {
                        { "GroupName", newGroupName }
                    };
                    TestManager.SaveProperties(context, properties);
                }
                else
                {
                    var properties = TestManager.GetProperties(context);
                    newGroupName = properties["GroupName"];
                }

                // Add new group
                var group = await context.TermStore.Groups.AddAsync(newGroupName);

                // Add term set
                var termSet = await group.Sets.AddAsync("PnPSet1", "Set description");

                // Add terms
                for (int i = 1; i <= 45; i++)
                {
                    await termSet.Terms.AddBatchAsync($"T1-{i}", $"Term {i} description in English");
                }
                // Send the terms to the server for adding
                context.Execute();

                termSet.Terms.Clear();

                // Load the terms again
                await termSet.LoadAsync(p => p.Terms);
                Assert.IsTrue(termSet.Terms.Length == 45);

                // Load the messages into a new termset instance
                var termSetForPaging = await termSet.GetAsync(p => p.Terms);
                Assert.IsTrue(termSetForPaging.Terms.Length == 45);

                var terms = await termSetForPaging.Terms.ToArrayAsync();
                Assert.IsTrue(terms.Length == 45);

                // Delete term set 
                await termSet.DeleteAsync();

                // Delete the group again
                await group.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GraphCollectionPages()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {

                string newGroupName;
                if (!TestCommon.Instance.Mocking)
                {
                    newGroupName = $"PnP-{Guid.NewGuid()}";
                    Dictionary<string, string> properties = new()
                    {
                        { "GroupName", newGroupName }
                    };
                    TestManager.SaveProperties(context, properties);
                }
                else
                {
                    var properties = TestManager.GetProperties(context);
                    newGroupName = properties["GroupName"];
                }

                // Add new group
                var group = await context.TermStore.Groups.AddAsync(newGroupName);

                // Add term set
                var termSet = await group.Sets.AddAsync("PnPSet1", "Set description");

                // Add terms
                for (int i = 1; i <= 45; i++)
                {
                    await termSet.Terms.AddBatchAsync($"T1-{i}", $"Term {i} description in English");
                }
                // Send the terms to the server for adding
                context.Execute();

                termSet.Terms.Clear();

                int pageSize = 10;

                // Read pages of terms
                var page = termSet.Terms.Take(pageSize).ToArray();
                Assert.IsTrue(page.Length == pageSize);

                page = termSet.Terms.Take(pageSize*2).ToArray();
                Assert.IsTrue(page.Length == pageSize*2);

                page = termSet.Terms.Take(pageSize * 3).ToArray();
                Assert.IsTrue(page.Length == pageSize * 3);

                page = termSet.Terms.Take(pageSize * 4).ToArray();
                Assert.IsTrue(page.Length == 40);

                page = termSet.Terms.Take(pageSize * 5).ToArray();
                Assert.IsTrue(page.Length == 45);

                // Delete term set 
                await termSet.DeleteAsync();

                // Delete the group again
                await group.DeleteAsync();
            }
        }

        #endregion

        #region REST paging

        [TestMethod]
        public async Task RESTListItemsNotSupported()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Force rest
                context.GraphFirst = false;

                var web = await context.Web.GetAsync(p => p.Lists);

                string listTitle = "RESTListItemPaging";
                var list = web.Lists.FirstOrDefault(p => p.Title == listTitle);

                if (list != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                else
                {
                    list = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                if (list != null)
                {
                    try
                    {
                        // Skip is not supported for items of a list
                        Assert.ThrowsException<InvalidOperationException>(() =>
                        {
                            list.Items.Skip(1).Take(2).ToArray();
                        });

                    }
                    finally
                    {
                        // Clean up
                        await list.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task RESTListItemPaging()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Force rest
                context.GraphFirst = false;

                var web = await context.Web.GetAsync(p => p.Lists);

                string listTitle = "RESTListItemPaging";
                var list = web.Lists.AsRequested().FirstOrDefault(p => p.Title == listTitle);

                if (list != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                else
                {
                    list = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                if (list != null)
                {
                    try
                    {
                        // Add items
                        for (int i = 0; i < 10; i++)
                        {
                            Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                            await list.Items.AddBatchAsync(values);
                        }
                        await context.ExecuteAsync();

                        var list2 = context.Web.Lists.FirstOrDefault(p => p.Id == list.Id);

                        var queryResult2 = list2.Items.Take(2).ToList();

                        // We should have loaded 1 list item
                        Assert.IsTrue(queryResult2.Count == 2);

                        await list2.Items.LoadAsync(i => i.Title);
                        // Do we have all items?
                        Assert.IsTrue(list2.Items.Length == 10);

                        // Check paging when starting from the middle, the skip + take combination results in a __next url that 
                        // has both the skiptoken and skip parameters, an invalid combination. Paging logic will handle this
                        var list3 = context.Web.Lists.Where(p => p.Id == list.Id).FirstOrDefault();

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                        {
                            var queryResult3 = list3.Items.Skip(4).Take(2).ToList();

                            // We should have loaded 1 list item
                            Assert.IsTrue(queryResult3.Count == 2);
                        });                        

                        await list3.Items.LoadAsync();
                        // Do we have all items?
                        Assert.IsTrue(list3.Items.Length == 10);
                    }
                    finally
                    {
                        // Clean up
                        await list.DeleteAsync();
                    }
                }
            }
        }


        [TestMethod]
        public async Task CamlListItemGetPagedAsyncPaging()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Force rest
                context.GraphFirst = false;

                var web = await context.Web.GetAsync(p => p.Lists);

                string listTitle = "CamlListItemGetPagedAsyncPaging";
                var list = web.Lists.FirstOrDefault(p => p.Title == listTitle);

                if (list != null)
                {
                    await list.DeleteAsync();
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                else
                {
                    list = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                if (list != null)
                {
                    // Add items
                    for (int i = 0; i < 100; i++)
                    {
                        Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                        await list.Items.AddBatchAsync(values);
                    }
                    await context.ExecuteAsync();

                    // Since we've already populated the model due to the add let's create a second context to perform a clean load again
                    using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                    {
                        // Force rest
                        context2.GraphFirst = false;

                        var list2 = context2.Web.Lists.Where(p => p.Id == list.Id).FirstOrDefault();

                        await list2.LoadItemsByCamlQueryAsync(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>20</RowLimit></View>"
                        });

                        Assert.IsTrue(list2.Items.Length == 20);

                        await list2.LoadItemsByCamlQueryAsync(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>20</RowLimit></View>",
                            PagingInfo = $"Paged=TRUE&p_ID={list2.Items.AsRequested().Last().Id}"
                        });

                        Assert.IsTrue(list2.Items.Length == 40);
                        Assert.IsTrue(list2.Items.AsRequested().ElementAt(21).Id == 22);

                        // delete the list
                        await list2.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task ListDataAsStreamListItemGetPagedAsyncPaging()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Force rest
                context.GraphFirst = false;

                var web = await context.Web.GetAsync(p => p.Lists);

                string listTitle = "ListDataAsStreamListItemGetPagedAsyncPaging";
                var list = web.Lists.AsRequested().FirstOrDefault(p => p.Title == listTitle);

                if (list != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                else
                {
                    list = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                if (list != null)
                {
                    try
                    {
                        // Add items
                        for (int i = 0; i < 100; i++)
                        {
                            Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", $"Item {i}" }
                        };

                            await list.Items.AddBatchAsync(values);
                        }
                        await context.ExecuteAsync();

                        var list2 = context.Web.Lists.Where(p => p.Id == list.Id).FirstOrDefault();

                        var result = await list2.LoadListDataAsStreamAsync(new RenderListDataOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit Paged='TRUE'>20</RowLimit></View>",
                            RenderOptions = RenderListDataOptionsFlags.ListData
                        });

                        Assert.IsTrue(list2.Items.Length == 20);

                        result = await list2.LoadListDataAsStreamAsync(new RenderListDataOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit Paged='TRUE'>20</RowLimit></View>",
                            RenderOptions = RenderListDataOptionsFlags.ListData,
                            Paging = result["NextHref"].ToString().Substring(1)
                        });

                        Assert.IsTrue(list2.Items.Length == 40);
                        Assert.IsTrue(list2.Items.AsRequested().ElementAt(21).Id == 22);
                    }
                    finally
                    {
                        // Clean up
                        await list.DeleteAsync();
                    }
                }
            }
        }

        #endregion


    }
}
