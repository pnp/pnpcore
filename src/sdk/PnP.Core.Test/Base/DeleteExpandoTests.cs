using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
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
    /// Tests that focus on adding data via REST or Microsoft Graph - used to test the core data add logic of the engine.
    /// Specific domain model testing will be implemented in the domain model tests
    /// </summary>
    [TestClass]
    public class DeleteExpandoTests
    {

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        #region Tests that use REST to hit SharePoint

        [TestMethod]
        public async Task DeleteListItemViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string listTitle = "DeleteListItemViaRest";
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

                // get items from the list
                await myList.LoadAsync(p => p.Items);

                try
                {
                    // grab first item
                    var firstItem = myList.Items.AsRequested().FirstOrDefault();
                    if (firstItem != null)
                    {
                        // get original item count
                        int itemCount = myList.Items.Length;

                        await firstItem.DeleteAsync();

                        // Using the deleted item should result in an error
                        bool exceptionThrown = false;
                        try
                        {
                            var deletedItemTitle = firstItem.Values["Title"];
                        }
                        catch (Exception)
                        {
                            exceptionThrown = true;
                        }
                        Assert.IsTrue(exceptionThrown);

                        exceptionThrown = false;
                        dynamic dynamicFirstItem = firstItem;
                        try
                        {
                            var deletedItemTitle = dynamicFirstItem.Title;
                        }
                        catch (Exception)
                        {
                            exceptionThrown = true;
                        }
                        Assert.IsTrue(exceptionThrown);

                        // get items from the list
                        await myList.LoadAsync(p => p.Items);

                        Assert.IsTrue(myList.Items.Length == itemCount - 1);
                    }
                }
                finally
                {
                    // Cleanup
                    await myList.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task DeleteListItemViaBatchRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string listTitle = "DeleteListItemViaBatchRest";
                var myList = context.Web.Lists.FirstOrDefault(p => p.Title == listTitle);

                if (myList == null)
                {
                    // Create the list
                    myList = await context.Web.Lists.AddBatchAsync(listTitle, ListTemplateType.GenericList);
                    await context.ExecuteAsync();
                    // Add a list item to this list
                    // Add a list item
                    Dictionary<string, object> values = new Dictionary<string, object>
                        {
                            { "Title", "Yes" }
                        };
                    await myList.Items.AddBatchAsync(values);
                    await context.ExecuteAsync();
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                // get items from the list
                await myList.LoadBatchAsync(p => p.Items);
                await context.ExecuteAsync();

                try
                {
                    // grab first item
                    var firstItem = myList.Items.AsRequested().FirstOrDefault();
                    if (firstItem != null)
                    {
                        // get original item count
                        int itemCount = myList.Items.Length;

                        await firstItem.DeleteBatchAsync();
                        await context.ExecuteAsync();

                        // Using the deleted item should result in an error
                        bool exceptionThrown = false;
                        try
                        {
                            var deletedItemTitle = firstItem.Values["Title"];
                        }
                        catch (Exception)
                        {
                            exceptionThrown = true;
                        }
                        Assert.IsTrue(exceptionThrown);

                        exceptionThrown = false;
                        dynamic dynamicFirstItem = firstItem;
                        try
                        {
                            var deletedItemTitle = dynamicFirstItem.Title;
                        }
                        catch (Exception)
                        {
                            exceptionThrown = true;
                        }
                        Assert.IsTrue(exceptionThrown);

                        // get items from the list
                        await myList.LoadBatchAsync(p => p.Items);
                        await context.ExecuteAsync();

                        Assert.IsTrue(myList.Items.Length == itemCount - 1);
                    }
                }
                finally
                {
                    // Cleanup
                    await myList.DeleteAsync();
                }
            }
        }
        #endregion

    }
}
