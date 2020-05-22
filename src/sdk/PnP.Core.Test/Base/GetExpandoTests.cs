using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Disable graph first as we're testing the REST path here
                context.GraphFirst = false;

                var web = context.Web.Get(p => p.Lists);
                await context.ExecuteAsync();

                string listTitle = "GetListAndListItemViaRest";
                if (!SetupList(context, listTitle))
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    // Get items from the list
                    await myList.GetAsync(p => p.Items);
                    // There should be 3 items in the list
                    Assert.IsTrue(myList.Items.Count() == 3);
                    // Can we get the value of list item field
                    var firstItem = myList.Items.First();
                    Assert.IsNotNull(firstItem.Title);
                    Assert.AreEqual(ItemTitleValue, firstItem.Title);
                    // Test the dynamic list item data reading
                    var dynamicFirstItem = firstItem.ToDynamic();
                    Assert.AreEqual(ItemTitleValue, dynamicFirstItem.Title);
                    Assert.AreEqual(ItemTitleValue, dynamicFirstItem["Title"]);
                    // handling of standard field in the list item
                    Assert.AreEqual(firstItem.Id, dynamicFirstItem.Id);
                    Assert.AreEqual(firstItem.Id, dynamicFirstItem["Id"]);
                }
            }
        }

        [TestMethod]
        public async Task GetListPropertiesAndListItemViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Disable graph first as we're testing the REST path here
                context.GraphFirst = false;

                var web = context.Web.Get(p => p.Lists);
                await context.ExecuteAsync();

                string listTitle = "GetListPropertiesAndListItemViaRest";
                if (!SetupList(context, listTitle))
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    // Get items from the list + custom properties
                    await myList.GetAsync(p => p.Items, p => p.Title, p => p.NoCrawl);
                    // There should be 3 items in the list
                    Assert.IsTrue(myList.Items.Count() == 3);
                    // Can we get the value of list item field
                    var firstItem = myList.Items.First();
                    Assert.IsNotNull(firstItem.Title);
                    Assert.AreEqual(ItemTitleValue, firstItem.Title);
                    // Test the dynamic list item data reading
                    var dynamicFirstItem = firstItem.ToDynamic();
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
            }
        }

        #endregion

        #region Tests that use Graph to hit SharePoint

        [TestMethod]
        public async Task GetListAndListItemViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var web = context.Web.Get(p => p.Lists);
                await context.ExecuteAsync();

                string listTitle = "GetListAndListItemViaGraph";
                if (!SetupList(context, listTitle))
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    // Get items from the list, load NoCrawl to ensure retrieval via REST
                    await myList.GetAsync(p => p.Items);
                    // There should be 3 items in the list
                    Assert.IsTrue(myList.Items.Count() == 3);
                    // Can we get the value of list item field
                    var firstItem = myList.Items.First();
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
            }
        }

        [TestMethod]
        public async Task GetListPropertiesAndListItemViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var web = context.Web.Get(p => p.Lists);
                await context.ExecuteAsync();

                string listTitle = "GetListPropertiesAndListItemViaGraph";
                if (!SetupList(context, listTitle))
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    // Get items from the list + custom properties
                    await myList.GetAsync(p => p.Items, p => p.ContentTypesEnabled, p => p.Hidden);
                    // There should be 3 items in the list
                    Assert.IsTrue(myList.Items.Count() == 3);
                    // Can we get the value of list item field
                    var firstItem = myList.Items.First();
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
            }
        }
        #endregion

        private bool SetupList(PnPContext context, string listTitle)
        {
            // Disable graph first as we're testing the REST path here
            context.GraphFirst = false;

            var web = context.Web.Get(p => p.Lists);
            context.ExecuteAsync().Wait();

            var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

            if (myList == null)
            {
                // Add a new list
                myList = web.Lists.AddAsync(listTitle, ListTemplateType.GenericList).GetAwaiter().GetResult();

                // Add a number of list items and fetch them again in a single server call
                Dictionary<string, object> values = new Dictionary<string, object>
                    {
                        { "Title", ItemTitleValue }
                    };

                myList.Items.Add(values);
                myList.Items.Add(values);
                myList.Items.Add(values);
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
