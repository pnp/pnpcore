using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.QueryModel;
using PnP.Core.Model.SharePoint.Core.Public;

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
        public async Task GetItemsByCAMLQuery()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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

                    myList.Items.Add(values);
                }
                await context.ExecuteAsync();

                using (var context2 = TestCommon.Instance.GetContext(TestCommon.TestSite, 1))
                {
                    var list2 = context2.Web.Lists.GetByTitle(listTitle);
                    if (list2 != null)
                    {
                        var items = await list2.GetItemsByCamlQueryAsync("<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>");
                        Assert.IsTrue(items.Count() == 5);
                    }
                }

                using (var context3 = TestCommon.Instance.GetContext(TestCommon.TestSite, 2))
                {
                    var list3 = context3.Web.Lists.GetByTitle(listTitle);
                    if (list3 != null)
                    {
                        var items = await list3.GetItemsByCamlQueryAsync(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields></View>",
                            DatesInUtc = true
                        }) ;
                        Assert.IsTrue(items.Count() == 10);
                    }
                }

                // Batch testing
                using (var context4 = TestCommon.Instance.GetContext(TestCommon.TestSite, 3))
                {
                    var list4 = context4.Web.Lists.GetByTitle(listTitle);
                    if (list4 != null)
                    {
                        // Perform 2 queries, the first one limited to 5 items, the second one without limits. Total should be 10 items
                        var items = list4.GetItemsByCamlQuery(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields><RowLimit>5</RowLimit></View>",
                        });
                        list4.GetItemsByCamlQuery(new CamlQueryOptions()
                        {
                            ViewXml = "<View><ViewFields><FieldRef Name='Title' /></ViewFields></View>",
                        });
                        await context4.ExecuteAsync();

                        Assert.IsTrue(items.Count() == 10);
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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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

    }
}
