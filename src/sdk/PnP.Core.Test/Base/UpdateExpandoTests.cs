using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Tests that focus on updating data via REST or Microsoft Graph - used to test the core expando data add logic of the engine.
    /// Specific domain model testing will be implemented in the domain model tests
    /// </summary>
    [TestClass]
    public class UpdateExpandoTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        #region Tests that use REST to hit SharePoint

        [TestMethod]
        public async Task UpdateValuesPropertyViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string listTitle = "UpdateValuesPropertyViaRest";
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
                        await myList.LoadAsync(p => p.Items);

                        // grab first item
                        var firstItem = myList.Items.AsRequested().FirstOrDefault();
                        if (firstItem != null)
                        {
                            firstItem.Values["Title"] = "No";
                            // The values property should have changed
                            Assert.IsTrue(firstItem.HasChanged("Values"));
                            // Did the transientdictionary list changes
                            Assert.IsTrue(firstItem.Values.HasChanges);

                            await firstItem.UpdateAsync();

                            // get items again from the list
                            await myList.LoadAsync(p => p.Items);
                            firstItem = myList.Items.AsRequested().FirstOrDefault();

                            Assert.IsTrue(firstItem.Values["Title"].ToString() == "No");
                            Assert.IsFalse(firstItem.HasChanged("Values"));
                            Assert.IsFalse(firstItem.Values.HasChanges);

                            // reset the item for the next test run via the expando syntax
                            dynamic dynamicFirstItem = firstItem;
                            dynamicFirstItem.Title = "Yes";
                            await dynamicFirstItem.UpdateAsync();
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
        public async Task UpdateValuesPropertyViaBatchRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string listTitle = "UpdateValuesPropertyViaBatchRest";
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

                if (myList != null)
                {
                    try
                    {
                        // get items from the list
                        await myList.LoadBatchAsync(p => p.Items);
                        await context.ExecuteAsync();

                        // grab first item
                        var firstItem = myList.Items.AsRequested().FirstOrDefault();
                        if (firstItem != null)
                        {
                            firstItem.Values["Title"] = "No";
                            // The values property should have changed
                            Assert.IsTrue(firstItem.HasChanged("Values"));
                            // Did the transientdictionary list changes
                            Assert.IsTrue(firstItem.Values.HasChanges);

                            await firstItem.UpdateBatchAsync();
                            await context.ExecuteAsync();

                            // get items again from the list
                            await myList.LoadBatchAsync(p => p.Items);
                            await context.ExecuteAsync();

                            firstItem = myList.Items.AsRequested().FirstOrDefault();
                            Assert.IsTrue(firstItem.Values["Title"].ToString() == "No");
                            Assert.IsFalse(firstItem.HasChanged("Values"));
                            Assert.IsFalse(firstItem.Values.HasChanges);

                            // reset the item for the next test run via the expando syntax
                            dynamic dynamicFirstItem = firstItem;
                            dynamicFirstItem.Title = "Yes";
                            await dynamicFirstItem.UpdateAsync();
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
    }
}
