using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint.Core;
using System;
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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Lists);

                string listTitle = "UpdateValuesPropertyViaRest";
                var myList = web.Lists.Where(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (myList == null)
                {
                    // Create the list
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
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
                await myList.GetAsync(p => p.Items);

                // grab first item
                var firstItem = myList.Items.FirstOrDefault();
                if (firstItem != null)
                {
                    firstItem.Values["Title"] = "No";
                    // The values property should have changed
                    Assert.IsTrue(firstItem.HasChanged("Values"));
                    // Did the transientdictionary list changes
                    Assert.IsTrue(firstItem.Values.HasChanges);

                    await firstItem.UpdateAsync();

                    // get items again from the list
                    await myList.GetAsync(p => p.Items);
                    firstItem = myList.Items.FirstOrDefault();

                    Assert.IsTrue(firstItem.Values["Title"].ToString() == "No");
                    Assert.IsFalse(firstItem.HasChanged("Values"));
                    Assert.IsFalse(firstItem.Values.HasChanges);

                    // reset the item for the next test run via the expando syntax
                    dynamic dynamicFirstItem = firstItem;
                    dynamicFirstItem.Title = "Yes";
                    await dynamicFirstItem.UpdateAsync();
                }             
            }
        }

        [TestMethod]
        public async Task UpdateValuesPropertyViaBatchRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var web = context.Web.Get(p => p.Lists);
                await context.ExecuteAsync();

                string listTitle = "UpdateValuesPropertyViaBatchRest";
                var myList = web.Lists.Where(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (myList == null)
                {
                    // Create the list
                    myList = web.Lists.Add(listTitle, ListTemplateType.GenericList);
                    await context.ExecuteAsync();
                    // Add a list item to this list
                    // Add a list item
                    Dictionary<string, object> values = new Dictionary<string, object>
                    {
                        { "Title", "Yes" }
                    };
                    myList.Items.Add(values);
                    await context.ExecuteAsync();
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                // get items from the list
                myList.Get(p => p.Items);
                await context.ExecuteAsync();

                // grab first item
                var firstItem = myList.Items.FirstOrDefault();
                if (firstItem != null)
                {
                    firstItem.Values["Title"] = "No";
                    // The values property should have changed
                    Assert.IsTrue(firstItem.HasChanged("Values"));
                    // Did the transientdictionary list changes
                    Assert.IsTrue(firstItem.Values.HasChanges);

                    firstItem.Update();
                    await context.ExecuteAsync();

                    // get items again from the list
                    myList.Get(p => p.Items);
                    await context.ExecuteAsync();
                    firstItem = myList.Items.FirstOrDefault();

                    Assert.IsTrue(firstItem.Values["Title"].ToString() == "No");
                    Assert.IsFalse(firstItem.HasChanged("Values"));
                    Assert.IsFalse(firstItem.Values.HasChanges);

                    // reset the item for the next test run via the expando syntax
                    dynamic dynamicFirstItem = firstItem;
                    dynamicFirstItem.Title = "Yes";
                    await dynamicFirstItem.UpdateAsync();
                }
            }
        }
        #endregion
    }
}
