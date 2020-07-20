using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [TestMethod]
        public async Task SystemUpdate()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists);

                int listCount = web.Lists.Count();

                string listTitle = "SystemUpdate";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
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
                var first = myList.Items.First();

                first.Title = "blabla";

                await first.SystemUpdateAsync();

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    await context2.Web.GetAsync(p => p.Lists);
                    var myList2 = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                    await myList2.GetAsync(p => p.Items);

                    var first2 = myList2.Items.First();
                    
                    // verify the list item was updated and that we're still at version 1.0
                    Assert.IsTrue(first2.Title == "blabla");
                    Assert.IsTrue(first2.Values["_UIVersionString"].ToString() == "1.0");

                    // do a regular update to bump the version again
                    first2.Title = "blabla2";
                    await first2.UpdateAsync();
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    await context3.Web.GetAsync(p => p.Lists);
                    var myList3 = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                    await myList3.GetAsync(p => p.Items);

                    var first3 = myList3.Items.First();

                    // verify the list item was updated and that we're still at version 1.0
                    Assert.IsTrue(first3.Title == "blabla2");
                    Assert.IsTrue(first3.Values["_UIVersionString"].ToString() == "2.0");

                    // do a regular update to bump the version again
                    first3.Title = "blabla3";
                    await first3.SystemUpdateAsync();
                }

                using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
                {
                    await context4.Web.GetAsync(p => p.Lists);
                    var myList4 = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                    await myList4.GetAsync(p => p.Items);

                    var first4 = myList4.Items.First();

                    // verify the list item was updated and that we're still at version 2.0
                    Assert.IsTrue(first4.Title == "blabla3");
                    Assert.IsTrue(first4.Values["_UIVersionString"].ToString() == "2.0");

                }

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdateOverwriteVersion()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists.Include(p => p.Title, p=>p.Items));

                int listCount = web.Lists.Count();

                string listTitle = "UpdateOverwriteVersion";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
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
                var first = myList.Items.First();

                first.Title = "blabla";

                // Use the batch update flow here
                var batch = context.NewBatch();
                await first.UpdateOverwriteVersionBatchAsync(batch).ConfigureAwait(false);
                await context.ExecuteAsync(batch);

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    await context2.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));
                    var myList2 = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                    await myList2.GetAsync(p => p.Items);

                    var first2 = myList2.Items.First();

                    // verify the list item was updated and that we're still at version 1.0
                    Assert.IsTrue(first2.Title == "blabla");
                    Assert.IsTrue(first2.Values["_UIVersionString"].ToString() == "1.0");
                }

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }


    }
}
