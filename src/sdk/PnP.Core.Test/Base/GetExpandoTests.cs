using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Model;
using PnP.Core.QueryModel;

namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Tests that focus on getting ExpandoObject (e.g. ListItem) data via REST or Microsoft Graph - used to test the core data retrieval/mapping logic of the engine.
    /// Specific domain model testing will be implemented in the domain model tests
    /// </summary>
    [TestClass]
    public class GetExpandoTests
    {
        private const string ItemTitleValue = "Yesssss";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext testContext)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            // TestCommon.Instance.Mocking = false;
        }

        #region Tests that use REST to hit SharePoint

        [TestMethod]
        public async Task GetListAndListItemViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Disable graph first as we're testing the REST path here
                context.GraphFirst = false;

                var web = await context.Web.GetBatchAsync(p => p.Lists);
                await context.ExecuteAsync();

                string listTitle = "GetListAndListItemViaRest";
                if (!await SetupList(context, listTitle))
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                var myList = web.Result.Lists.FirstOrDefault(p => p.Title == listTitle);

                if (myList != null)
                {
                    try
                    {
                        // Get items from the list
                        await myList.LoadAsync(p => p.Items);
                        // There should be 3 items in the list
                        Assert.IsTrue(myList.Items.Length == 3);
                        // Can we get the value of list item field
                        var firstItem = myList.Items.AsRequested().First();
                        Assert.IsNotNull(firstItem.Title);
                        Assert.AreEqual(ItemTitleValue, firstItem.Title);
                        // Test the dynamic list item data reading
                        var dynamicFirstItem = firstItem.AsDynamic();
                        Assert.AreEqual(ItemTitleValue, dynamicFirstItem.Title);
                        Assert.AreEqual(ItemTitleValue, dynamicFirstItem["Title"]);
                        // handling of standard field in the list item
                        Assert.AreEqual(firstItem.Id, dynamicFirstItem.Id);
                        Assert.AreEqual(firstItem.Id, dynamicFirstItem["Id"]);
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
        public async Task GetListPropertiesAndListItemViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Disable graph first as we're testing the REST path here
                context.GraphFirst = false;

                var web = await context.Web.GetBatchAsync(p => p.Lists);
                await context.ExecuteAsync();

                string listTitle = "GetListPropertiesAndListItemViaRest";
                if (!await SetupList(context, listTitle))
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                var myList = web.Result.Lists.FirstOrDefault(p => p.Title == listTitle);

                if (myList != null)
                {
                    try
                    {
                        // Get items from the list + custom properties
                        await myList.LoadAsync(p => p.Items, p => p.Title, p => p.NoCrawl);
                        // There should be 3 items in the list
                        Assert.IsTrue(myList.Items.Length == 3);
                        // Can we get the value of list item field
                        var firstItem = myList.Items.AsRequested().First();
                        Assert.IsNotNull(firstItem.Title);
                        Assert.AreEqual(ItemTitleValue, firstItem.Title);
                        // Test the dynamic list item data reading
                        var dynamicFirstItem = firstItem.AsDynamic();
                        Assert.AreEqual(ItemTitleValue, dynamicFirstItem.Title);
                        Assert.AreEqual(ItemTitleValue, dynamicFirstItem["Title"]);
                        // handling of standard field in the list item
                        Assert.AreEqual(firstItem.Id, dynamicFirstItem.Id);
                        Assert.AreEqual(firstItem.Id, dynamicFirstItem["Id"]);
                        // Check if the custom list properties where also loaded
                        Assert.IsTrue(myList.IsPropertyAvailable(p => p.Title));
                        Assert.IsTrue(!string.IsNullOrEmpty(myList.Title));
                        Assert.IsTrue(myList.IsPropertyAvailable(p => p.NoCrawl));
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

        #region Tests that use Graph to hit SharePoint

        [TestMethod]
        public async Task GetListAndListItemViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.LoadBatchAsync(p => p.Lists);
                await context.ExecuteAsync();

                string listTitle = "GetListAndListItemViaGraph";
                if (!await SetupList(context, listTitle))
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                var myList = context.Web.Lists.FirstOrDefault(p => p.Title == listTitle);

                if (myList != null)
                {
                    try
                    {
                        // Get items from the list, load NoCrawl to ensure retrieval via REST
                        await myList.LoadAsync(p => p.Items);
                        // There should be 3 items in the list
                        Assert.IsTrue(myList.Items.Length == 3);
                        // Can we get the value of list item field
                        var firstItem = myList.Items.AsRequested().First();
                        Assert.IsNotNull(firstItem.Title);
                        Assert.AreEqual(ItemTitleValue, firstItem.Title);
                        // Test the dynamic list item data reading
                        dynamic dynamicFirstItem = firstItem;
                        Assert.AreEqual(ItemTitleValue, dynamicFirstItem.Title);
                        Assert.AreEqual(ItemTitleValue, dynamicFirstItem["Title"]);
                        // handling of standard field in the list item
                        Assert.AreEqual(firstItem.Id, dynamicFirstItem.Id);
                        Assert.AreEqual(firstItem.Id, dynamicFirstItem["Id"]);
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
        public async Task GetListPropertiesAndListItemViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetBatchAsync(p => p.Lists);
                await context.ExecuteAsync();

                string listTitle = "GetListPropertiesAndListItemViaGraph";
                if (!await SetupList(context, listTitle))
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                var myList = web.Result.Lists.FirstOrDefault(p => p.Title == listTitle);

                if (myList != null)
                {
                    try
                    {
                        // Get items from the list + custom properties
                        await myList.LoadAsync(p => p.Items, p => p.ContentTypesEnabled, p => p.Hidden);
                        // There should be 3 items in the list
                        Assert.IsTrue(myList.Items.Length == 3);
                        // Can we get the value of list item field
                        var firstItem = myList.Items.AsRequested().First();
                        Assert.IsNotNull(firstItem.Title);
                        Assert.AreEqual(ItemTitleValue, firstItem.Title);
                        // Test the dynamic list item data reading
                        dynamic dynamicFirstItem = firstItem;
                        Assert.AreEqual(ItemTitleValue, dynamicFirstItem.Title);
                        Assert.AreEqual(ItemTitleValue, dynamicFirstItem["Title"]);
                        // handling of standard field in the list item
                        Assert.AreEqual(firstItem.Id, dynamicFirstItem.Id);
                        Assert.AreEqual(firstItem.Id, dynamicFirstItem["Id"]);
                        // Check if the custom list properties where also loaded
                        Assert.IsTrue(myList.IsPropertyAvailable(p => p.Hidden));
                        Assert.IsTrue(myList.IsPropertyAvailable(p => p.ContentTypesEnabled));
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

        private async Task<bool> SetupList(PnPContext context, string listTitle)
        {
            // Disable graph first as we're testing the REST path here
            context.GraphFirst = false;

            await context.Web.LoadBatchAsync(p => p.Lists);
            context.ExecuteAsync().Wait();

            var myList = context.Web.Lists.FirstOrDefault(p => p.Title == listTitle);

            if (myList == null)
            {
                // Add a new list
                myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);

                // Add a number of list items and fetch them again in a single server call
                Dictionary<string, object> values = new Dictionary<string, object>
                    {
                        { "Title", ItemTitleValue }
                    };

                await myList.Items.AddBatchAsync(values);
                await myList.Items.AddBatchAsync(values);
                await myList.Items.AddBatchAsync(values);
                context.ExecuteAsync().Wait();
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
