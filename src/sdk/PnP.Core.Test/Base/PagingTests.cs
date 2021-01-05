using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
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
                // This rest requires beta api's, so bail out if that's not enabled
                if (!context.GraphCanUseBeta)
                {
                    Assert.Inconclusive("This test requires Graph beta to be allowed.");
                }

                var team = await context.Team.GetAsync(p => p.Channels);

                // Create a new channel and add enough messages to it
                string channelName = $"Paging test {new Random().Next()}";

                if (TestCommon.Instance.Mocking)
                {
                    var properties = TestManager.GetProperties(context);
                    channelName = properties["ChannelName"];
                }

                var channelForPaging = team.Channels.FirstOrDefault(p => p.DisplayName == channelName);
                if (channelForPaging == null)
                {
                    // Persist the created channel name as we need to have the same name when we run an offline test
                    if (!TestCommon.Instance.Mocking)
                    {
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "ChannelName", channelName }
                        };
                        TestManager.SaveProperties(context, properties);
                    }

                    channelForPaging = await team.Channels.AddAsync(channelName, "Test channel, will be deleted in 21 days");
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the channel available.");
                }

                // Add messages, not using batching to ensure reliability
                for (int i = 1; i <= 45; i++)
                {
                    await channelForPaging.Messages.AddAsync($"Test message{i}");
                }

                // Since we've not yet loaded the channel messages from the server paging is not yet allowed
                Assert.IsFalse(channelForPaging.Messages.CanPage);

                // Since we've already populated the model due to the add let's create a second context to perform a clean load again
                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    // Retrieve the already created channel
                    await context2.Team.GetAsync(p => p.Channels);
                    var channelForPaging2 = context2.Team.Channels.FirstOrDefault(p => p.DisplayName == channelName);

                    // Load the messages, this will populate the first batch of messages and will indicate paging is allowed
                    await channelForPaging2.GetAsync(p => p.Messages);

                    // Paging should now be allowed
                    Assert.IsTrue(channelForPaging2.Messages.CanPage);

                    // We should have messages loaded

                    int messageCount = channelForPaging2.Messages.Count();
                    Assert.IsTrue(messageCount > 0);

                    // Get the next page
                    await channelForPaging2.Messages.GetNextPageAsync();

                    // Seems like the Graph API does load all 45 at once since they where created too short ago
                    //Assert.IsTrue(channelForPaging2.Messages.Count() > messageCount);

                    // Trigger a load of the remaining pages via the GetAllPages call
                    await channelForPaging2.Messages.GetAllPagesAsync();

                    // We now should have the full amount of messages loaded
                    Assert.IsTrue(channelForPaging2.Messages.Count() == 45);

                    // Cleanup by deleting the channel
                    await channelForPaging2.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task GraphLinqTakeToPaging()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Issue a linq query, will be executed by Graph at this point
                var lists = context.Web.Lists.Take(2);
                var queryResult = lists.ToList();

                // We should have loaded 2 lists
                Assert.IsTrue(queryResult.Count == 2);

                // Since we only asked 2 lists Graph will return a nextLink odata property 
                if (context.Web.Lists.CanPage)
                {
                    await context.Web.Lists.GetNextPageAsync();
                    Assert.IsTrue(context.Web.Lists.Count() == 4);
                }
                else
                {
                    Assert.Fail("No @odata.nextLink property returned and paging is not possible");
                }

            }
        }

        [TestMethod]
        public async Task GraphListViaGetPagedAsyncPaging()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {

                await context.Web.Lists.GetPagedAsync(2);

                // We should have loaded 2 lists
                Assert.IsTrue(context.Web.Lists.Count() == 2);

                // Since we only asked 2 lists Graph will return a nextLink odata property 
                if (context.Web.Lists.CanPage)
                {
                    await context.Web.Lists.GetNextPageAsync();
                    Assert.IsTrue(context.Web.Lists.Count() == 4);

                    await context.Web.Lists.GetAllPagesAsync();
                    Assert.IsTrue(context.Web.Lists.Count() >= 4);

                    // Once we've loaded all lists we can't page anymore
                    Assert.IsFalse(context.Web.Lists.CanPage);
                }
                else
                {
                    Assert.Fail("No __next property returned and paging is not possible");
                }
            }
        }

        [TestMethod]
        public async Task GraphListViaGetPagedExpressionAsyncPaging()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {

                await context.Web.Lists.GetPagedAsync(2, p => p.Title);

                // We should have loaded 2 lists
                Assert.IsTrue(context.Web.Lists.Count() == 2);

                // We only request the Title property, verify its loaded while others are not loaded
                foreach (var list in context.Web.Lists)
                {
                    Assert.IsTrue(list.IsPropertyAvailable(p => p.Title));
                    Assert.IsTrue(!string.IsNullOrEmpty(list.Title));
                }

                // Since we only asked 2 lists Graph will return a nextLink odata property 
                if (context.Web.Lists.CanPage)
                {
                    await context.Web.Lists.GetNextPageAsync();
                    Assert.IsTrue(context.Web.Lists.Count() == 4);

                    // We only request the Title property, verify its loaded while others are not loaded
                    foreach (var list in context.Web.Lists)
                    {
                        Assert.IsTrue(list.IsPropertyAvailable(p => p.Title));
                        Assert.IsTrue(!string.IsNullOrEmpty(list.Title));
                    }

                    await context.Web.Lists.GetAllPagesAsync();
                    Assert.IsTrue(context.Web.Lists.Count() >= 4);

                    // Once we've loaded all lists we can't page anymore
                    Assert.IsFalse(context.Web.Lists.CanPage);
                }
                else
                {
                    Assert.Fail("No __next property returned and paging is not possible");
                }
            }
        }        
        #endregion

        #region REST paging

        [TestMethod]
        public async Task RESTListViaGetPagedAsyncPaging()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Force rest
                context.GraphFirst = false;

                await context.Web.Lists.GetPagedAsync(2);

                // We should have loaded 2 lists
                Assert.IsTrue(context.Web.Lists.Count() == 2);

                // Since we only asked 2 lists Graph will return a nextLink odata property 
                if (context.Web.Lists.CanPage)
                {
                    await context.Web.Lists.GetNextPageAsync();
                    Assert.IsTrue(context.Web.Lists.Count() == 4);

                    await context.Web.Lists.GetAllPagesAsync();
                    Assert.IsTrue(context.Web.Lists.Count() >= 4);

                    // Once we've loaded all lists we can't page anymore
                    Assert.IsFalse(context.Web.Lists.CanPage);
                }
                else
                {
                    Assert.Fail("No __next property returned and paging is not possible");
                }
            }
        }

        [TestMethod]
        public async Task RESTListViaGetPagedWithFilterAsyncPaging()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Force rest
                context.GraphFirst = false;

                await context.Web.Lists.GetPagedAsync(p=>p.TemplateType == ListTemplateType.GenericList, 2);

                // We should have loaded 2 lists
                Assert.IsTrue(context.Web.Lists.Count() == 2);

                // Since we only asked 2 lists Graph will return a nextLink odata property 
                if (context.Web.Lists.CanPage)
                {
                    await context.Web.Lists.GetNextPageAsync();
                    Assert.IsTrue(context.Web.Lists.Count() == 4);

                    await context.Web.Lists.GetAllPagesAsync();
                    Assert.IsTrue(context.Web.Lists.Count() >= 4);

                    // Once we've loaded all lists we can't page anymore
                    Assert.IsFalse(context.Web.Lists.CanPage);
                }
                else
                {
                    Assert.Fail("No __next property returned and paging is not possible");
                }
            }
        }

        [TestMethod]
        public async Task RESTListViaGetPagedWithFilterAndPropertiesAsyncPaging()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Force rest
                context.GraphFirst = false;

                var lists = await context.Web.Lists.GetPagedAsync(p => p.TemplateType == ListTemplateType.GenericList, 2,
                                                      p => p.Title, p => p.TemplateType,
                                                                    p => p.ContentTypes.LoadProperties(
                                                                         p => p.Name, p => p.FieldLinks.LoadProperties(p => p.Name)));

                // We should have loaded 2 lists
                Assert.IsTrue(context.Web.Lists.Count() == 2);
                Assert.IsTrue(lists.Count() == 2);

                // Since we only asked 2 lists Graph will return a nextLink odata property 
                if (context.Web.Lists.CanPage)
                {
                    var nextLists = await context.Web.Lists.GetNextPageAsync();
                    Assert.IsTrue(context.Web.Lists.Count() == 4);
                    Assert.IsTrue(nextLists.Count() == 2);                    

                    await context.Web.Lists.GetAllPagesAsync();
                    Assert.IsTrue(context.Web.Lists.Count() >= 4);

                    // Once we've loaded all lists we can't page anymore
                    Assert.IsFalse(context.Web.Lists.CanPage);
                }
                else
                {
                    Assert.Fail("No __next property returned and paging is not possible");
                }
            }
        }

        [TestMethod]
        public async Task RESTListViaGetPagedExpressionAsyncPaging()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Force rest
                context.GraphFirst = false;

                await context.Web.Lists.GetPagedAsync(2, p => p.Title);

                // We should have loaded 2 lists
                Assert.IsTrue(context.Web.Lists.Count() == 2);

                // We only request the Title property, verify its loaded while others are not loaded
                foreach (var list in context.Web.Lists)
                {
                    Assert.IsTrue(list.IsPropertyAvailable(p => p.Title));
                    Assert.IsTrue(!string.IsNullOrEmpty(list.Title));
                    Assert.IsFalse(list.IsPropertyAvailable(p => p.Description));
                }

                // Since we only asked 2 lists Graph will return a nextLink odata property 
                if (context.Web.Lists.CanPage)
                {
                    await context.Web.Lists.GetNextPageAsync();
                    Assert.IsTrue(context.Web.Lists.Count() == 4);

                    // We only request the Title property, verify its loaded while others are not loaded
                    foreach (var list in context.Web.Lists)
                    {
                        Assert.IsTrue(list.IsPropertyAvailable(p => p.Title));
                        Assert.IsTrue(!string.IsNullOrEmpty(list.Title));
                        Assert.IsFalse(list.IsPropertyAvailable(p => p.Description));
                    }

                    await context.Web.Lists.GetAllPagesAsync();
                    Assert.IsTrue(context.Web.Lists.Count() >= 4);

                    // Once we've loaded all lists we can't page anymore
                    Assert.IsFalse(context.Web.Lists.CanPage);
                }
                else
                {
                    Assert.Fail("No __next property returned and paging is not possible");
                }
            }
        }


        [TestMethod]
        public async Task RESTListPaging()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Force rest
                context.GraphFirst = false;

                // Issue a linq query, will be executed by Graph at this point
                var lists = context.Web.Lists.Take(2);
                var queryResult = lists.ToList();

                // We should have loaded 2 lists
                Assert.IsTrue(queryResult.Count == 2);

                // Since we only asked 2 lists Graph will return a nextLink odata property 
                if (context.Web.Lists.CanPage)
                {
                    await context.Web.Lists.GetNextPageAsync();
                    Assert.IsTrue(context.Web.Lists.Count() == 4);

                    await context.Web.Lists.GetAllPagesAsync();
                    Assert.IsTrue(context.Web.Lists.Count() >= 4);

                    // Once we've loaded all lists we can't page anymore
                    Assert.IsFalse(context.Web.Lists.CanPage);
                }
                else
                {
                    Assert.Fail("No __next property returned and paging is not possible");
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
                var list = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

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

                    // Since we've already populated the model due to the add let's create a second context to perform a clean load again
                    // Check paging when starting from the beginning
                    using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                    {
                        // Force rest
                        context2.GraphFirst = false;

                        var list2 = context2.Web.Lists.Where(p => p.Id == list.Id).FirstOrDefault();

                        var items = list2.Items.Take(2);
                        var queryResult = items.ToList();

                        // We should have loaded 1 list item
                        Assert.IsTrue(queryResult.Count == 2);

                        if (list2.Items.CanPage)
                        {
                            await list2.Items.GetAllPagesAsync();
                            // Once we've loaded all items we can't page anymore
                            Assert.IsFalse(list2.Items.CanPage);
                            // Do we have all items?
                            Assert.IsTrue(list2.Items.Count() == 10);
                        }
                        else
                        {
                            Assert.Fail("No __next property returned and paging is not possible");
                        }
                    }

                    // Since we've already populated the model due to the add let's create a second context to perform a clean load again
                    // Check paging when starting from the middle, the skip + take combination results in a __next url that 
                    // has both the skiptoken and skip parameters, an invalid combination. Paging logic will handle this
                    using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                    {
                        // Force rest
                        context3.GraphFirst = false;

                        var list3 = context3.Web.Lists.Where(p => p.Id == list.Id).FirstOrDefault();

                        var items = list3.Items.Skip(4).Take(2);
                        var queryResult = items.ToList();

                        // We should have loaded 1 list item
                        Assert.IsTrue(queryResult.Count == 2);

                        if (list3.Items.CanPage)
                        {
                            await list3.Items.GetAllPagesAsync();
                            // Once we've loaded all items we can't page anymore
                            Assert.IsFalse(list3.Items.CanPage);
                            // Do we have all items?
                            Assert.IsTrue(list3.Items.Count() == 10);
                        }
                        else
                        {
                            Assert.Fail("No __next property returned and paging is not possible");
                        }
                    }

                }
            }
        }

        [TestMethod]
        public async Task RESTListItemGetPagedAsyncPaging()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Force rest
                context.GraphFirst = false;

                var web = await context.Web.GetAsync(p => p.Lists);

                string listTitle = "RESTListItemGetPagedAsyncPaging";
                var list = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

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

                    // Since we've already populated the model due to the add let's create a second context to perform a clean load again
                    using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                    {
                        // Force rest
                        context2.GraphFirst = false;

                        var list2 = context2.Web.Lists.Where(p => p.Id == list.Id).FirstOrDefault();

                        await list2.Items.GetPagedAsync(2);

                        // We should have loaded 2 list items
                        Assert.IsTrue(list2.Items.Count() == 2);

                        if (list2.Items.CanPage)
                        {
                            await list2.Items.GetAllPagesAsync();
                            // Once we've loaded all items we can't page anymore
                            Assert.IsFalse(list2.Items.CanPage);
                            // Do we have all items?
                            Assert.IsTrue(list2.Items.Count() == 10);
                        }
                        else
                        {
                            Assert.Fail("No __next property returned and paging is not possible");
                        }
                    }
                }
            }
        }
        #endregion


    }
}
