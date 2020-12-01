using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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
            string listTitle = "SystemUpdate";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists);

                int listCount = web.Lists.Count();

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
            }

            using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var web2 = await context2.Web.GetAsync(p => p.Lists);
                var myList2 = web2.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
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
                var web3 = await context3.Web.GetAsync(p => p.Lists);
                var myList3 = web3.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
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
                var web4 = await context4.Web.GetAsync(p => p.Lists);
                var myList4 = web4.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList4.GetAsync(p => p.Items);

                var first4 = myList4.Items.First();

                // verify the list item was updated and that we're still at version 2.0
                Assert.IsTrue(first4.Title == "blabla3");
                Assert.IsTrue(first4.Values["_UIVersionString"].ToString() == "2.0");
            }

            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 4))
            {
                var web = await contextFinal.Web.GetAsync(p => p.Lists);
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task SystemUpdateBatchTests()
        {
            //TestCommon.Instance.Mocking = false;
            string listTitle = "SystemUpdateBatchTests";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists);

                int listCount = web.Lists.Count();

                #region Test Setup

                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (!TestCommon.Instance.Mocking && myList != null)
                {
                    // Cleanup the created list possibly from a previous run
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

                #endregion

                // get first item and do a system update
                var first = myList.Items.First();

                first.Title = "blabla";

                await first.SystemUpdateBatchAsync();
                await context.ExecuteAsync();
            }

            using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                context2.BatchClient.EnsureBatch();

                var web2 = await context2.Web.GetAsync(p => p.Lists);
                var myList2 = web2.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
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
                var web3 = await context3.Web.GetAsync(p => p.Lists);
                var myList3 = web3.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList3.GetAsync(p => p.Items);

                var first3 = myList3.Items.First();

                // verify the list item was updated and that we're still at version 2.0
                Assert.IsTrue(first3.Title == "blabla2");
                Assert.IsTrue(first3.Values["_UIVersionString"].ToString() == "2.0");

                // do a regular update to bump the version again
                first3.Title = "blabla3";
                first3.SystemUpdate();
                await context3.ExecuteAsync();
            }

            using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
            {
                var web4 = await context4.Web.GetAsync(p => p.Lists);
                var myList4 = web4.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList4.GetAsync(p => p.Items);

                var first4 = myList4.Items.First();

                // verify the list item was updated and that we're still at version 2.0
                Assert.IsTrue(first4.Title == "blabla3");
                Assert.IsTrue(first4.Values["_UIVersionString"].ToString() == "2.0");

                // do a regular update to bump the version again
                first4.Title = "blabla4";
                var newBatch = context4.NewBatch();
                first4.SystemUpdateBatch(newBatch);
                await context4.ExecuteAsync(newBatch);
            }

            using (var context5 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 4))
            {
                var web5 = await context5.Web.GetAsync(p => p.Lists);
                var myList5 = web5.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                myList5.Get(p => p.Items);

                var first5 = myList5.Items.First();

                // verify the list item was updated and that we're still at version 2.0
                Assert.IsTrue(first5.Title == "blabla4");
                Assert.IsTrue(first5.Values["_UIVersionString"].ToString() == "2.0");

                // do a regular update to bump the version again
                first5.Title = "blabla5";
                first5.SystemUpdateBatch();
                await context5.ExecuteAsync();

            }

            using (var context6 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 5))
            {
                var web6 = await context6.Web.GetAsync(p => p.Lists);
                var myList6 = web6.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                myList6.Get(p => p.Items);

                var first6 = myList6.Items.First();

                // verify the list item was updated and that we're still at version 2.0
                Assert.IsTrue(first6.Title == "blabla5");
                Assert.IsTrue(first6.Values["_UIVersionString"].ToString() == "2.0");
            }

            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 6))
            {
                var web = await contextFinal.Web.GetAsync(p => p.Lists);
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task SystemUpdateAsyncNoDataChangeTests()
        {
            //TestCommon.Instance.Mocking = false;
            string listTitle = "SystemUpdateAsyncNoDataChangeTests";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists);

                int listCount = web.Lists.Count();

                #region Test Setup


                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (!TestCommon.Instance.Mocking && myList != null)
                {
                    // Cleanup the created list possibly from a previous run
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

                #endregion

                // get first item and do a system update
                var first = myList.Items.First();

                first.Title = "blabla";

                await first.SystemUpdateBatchAsync();
                await context.ExecuteAsync();

            }

            using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var web2 = await context2.Web.GetAsync(p => p.Lists);
                var myList2 = web2.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
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
                var web3 = await context3.Web.GetAsync(p => p.Lists);
                var myList3 = web3.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList3.GetAsync(p => p.Items);

                var first3 = myList3.Items.First();

                // verify the list item was updated and that we're still at version 2.0
                Assert.IsTrue(first3.Title == "blabla2");
                Assert.IsTrue(first3.Values["_UIVersionString"].ToString() == "2.0");

                // do a regular update to bump the version again
                await first3.SystemUpdateAsync();
            }

            using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
            {
                var web4 = await context4.Web.GetAsync(p => p.Lists);
                var myList4 = web4.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList4.GetAsync(p => p.Items);

                var first4 = myList4.Items.First();

                // verify the list item was updated and that we're still at version 2.0
                Assert.IsTrue(first4.Title == "blabla2");
                Assert.IsTrue(first4.Values["_UIVersionString"].ToString() == "2.0");

                // do a regular update to bump the version again
                await first4.SystemUpdateBatchAsync();
                await context4.ExecuteAsync();

            }

            using (var context5 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 4))
            {
                var web5 = await context5.Web.GetAsync(p => p.Lists);
                var myList5 = web5.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList5.GetAsync(p => p.Items);

                var first5 = myList5.Items.First();

                // verify the list item was updated and that we're still at version 2.0
                Assert.IsTrue(first5.Title == "blabla2");
                Assert.IsTrue(first5.Values["_UIVersionString"].ToString() == "2.0");
            }

            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 5))
            {
                var web = await contextFinal.Web.GetAsync(p => p.Lists);
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdateOverwriteVersion()
        {
            //TestCommon.Instance.Mocking = false;

            string listTitle = "UpdateOverwriteVersion";
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));

                int listCount = web.Lists.Count();

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
            }
            using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var web2 = await context2.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList2 = web2.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList2.GetAsync(p => p.Items);

                var first2 = myList2.Items.First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first2.Title == "blabla");
                Assert.IsTrue(first2.Values["_UIVersionString"].ToString() == "1.0");
            }
            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                var web = await contextFinal.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdateOverwriteVersionBatchTests()
        {
            //TestCommon.Instance.Mocking = false;

            string listTitle = "UpdateOverwriteVersionBatchTests";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));

                int listCount = web.Lists.Count();

                #region Test Setup

                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (!TestCommon.Instance.Mocking && myList != null)
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

                #endregion

                // get first item and do a system update
                var first = myList.Items.First();

                first.Title = "blabla";

                // Use the batch update flow here
                var batch = context.NewBatch();
                first.UpdateOverwriteVersionBatch(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var web2 = await context2.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList2 = web2.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList2.GetAsync(p => p.Items);

                var first2 = myList2.Items.First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first2.Title == "blabla");
                Assert.IsTrue(first2.Values["_UIVersionString"].ToString() == "1.0");

                first2.Title = "blabla2";
                first2.UpdateOverwriteVersion();
            }

            using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                var web3 = await context3.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList3 = web3.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList3.GetAsync(p => p.Items);

                var first3 = myList3.Items.First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first3.Title == "blabla2");
                Assert.IsTrue(first3.Values["_UIVersionString"].ToString() == "1.0");

                var batch3 = context3.NewBatch();
                first3.Title = "blabla3";
                first3.UpdateOverwriteVersionBatch(batch3);
                await context3.ExecuteAsync(batch3);
            }

            using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
            {
                var web4 = await context4.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList4 = web4.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList4.GetAsync(p => p.Items);

                var first4 = myList4.Items.First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first4.Title == "blabla3");
                Assert.IsTrue(first4.Values["_UIVersionString"].ToString() == "1.0");

                first4.Title = "blabla4";
                first4.UpdateOverwriteVersionBatch();
                await context4.ExecuteAsync();
            }

            using (var context5 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 4))
            {
                var web5 = await context5.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList5 = web5.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList5.GetAsync(p => p.Items);

                var first5 = myList5.Items.First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first5.Title == "blabla4");
                Assert.IsTrue(first5.Values["_UIVersionString"].ToString() == "1.0");
            }

            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 5))
            {
                var web = await contextFinal.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdateOverwriteVersionNoDataTests()
        {
            //TestCommon.Instance.Mocking = false;
            string listTitle = "UpdateOverwriteVersionNoDataTests";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));

                int listCount = web.Lists.Count();

                #region Test Setup

                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (!TestCommon.Instance.Mocking && myList != null)
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

                #endregion

                // get first item and do a system update
                var first = myList.Items.First();

                first.Title = "blabla";

                // Use the batch update flow here
                var batch = context.NewBatch();
                first.UpdateOverwriteVersionBatch(batch);
                await context.ExecuteAsync(batch);
            }

            using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var web2 = await context2.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList2 = web2.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList2.GetAsync(p => p.Items);

                var first2 = myList2.Items.First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first2.Title == "blabla");
                Assert.IsTrue(first2.Values["_UIVersionString"].ToString() == "1.0");

                await first2.UpdateOverwriteVersionAsync();
            }

            using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                var web3 = await context3.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList3 = web3.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList3.GetAsync(p => p.Items);

                var first3 = myList3.Items.First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first3.Title == "blabla");
                Assert.IsTrue(first3.Values["_UIVersionString"].ToString() == "1.0");

                await first3.UpdateOverwriteVersionBatchAsync();
                await context3.ExecuteAsync();
            }

            using (var context4 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
            {
                var web4 = await context4.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList4 = web4.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList4.GetAsync(p => p.Items);

                var first4 = myList4.Items.First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first4.Title == "blabla");
                Assert.IsTrue(first4.Values["_UIVersionString"].ToString() == "1.0");
            }

            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 4))
            {
                // Create a new list
                var web = await contextFinal.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        #region URL field type tests

        internal class FieldData
        {
            internal FieldData(string type)
            {
                FieldType = type;
                Properties = new Dictionary<string, object>();
            }

            internal string FieldType { get; set; }
            internal Dictionary<string, object> Properties { get; set; }
        }

        [TestMethod]
        public async Task SpecialFieldCsomTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Step 0: Data needed for the test run
                // Get current user
                var currentUser = await context.Web.GetCurrentUserAsync();
                // Get the principal representing two claims which are always available
                var userTwo = await context.Web.EnsureUserAsync("Everyone except external users");

                //==========================================================
                // Step 1: Create a new list
                string listTitle = TestCommon.GetPnPSdkTestAssetName("SpecialFieldCsomTest");
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                //==========================================================
                // Step 2: Add special fields
                string fieldGroup = "TEST GROUP";

                // URL field 1
                string fldUrl1 = "URLField1";
                IField addedUrlField1 = await myList.Fields.AddUrlAsync(fldUrl1, new FieldUrlOptions()
                {
                    Group = fieldGroup,
                    DisplayFormat = UrlFieldFormatType.Hyperlink
                });

                // URL field 2
                string fldUrl2 = "URLField2";
                IField addedUrlField2 = await myList.Fields.AddUrlAsync(fldUrl2, new FieldUrlOptions()
                {
                    Group = fieldGroup,
                    DisplayFormat = UrlFieldFormatType.Hyperlink
                });

                // User Single field 1
                string fldUserSingle1 = "UserSingleField1";
                IField addedUserSingleField1 = await myList.Fields.AddUserAsync(fldUserSingle1, new FieldUserOptions()
                {
                    Group = fieldGroup,
                    AllowMultipleValues = false, 
                    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
                });

                // User Multi field 1
                string fldUserMulti1 = "UserMultiField1";
                IField addedUserMultiField1 = await myList.Fields.AddUserMultiAsync(fldUserMulti1, new FieldUserOptions()
                {
                    Group = fieldGroup,
                    AllowMultipleValues = true,
                    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
                });

                //==========================================================
                // Step 3: Add a list item
                Dictionary<string, object> item = new Dictionary<string, object>()
                {
                    { "Title", "Item1" }
                };

                Dictionary<string, FieldData> fieldData = new Dictionary<string, FieldData>
                {
                    // URL field 1
                    { fldUrl1, new FieldData("URL") },
                    // URL field 2
                    { fldUrl2, new FieldData("URL") },
                    // User single field 1
                    { fldUserSingle1, new FieldData("UserSingle") },
                    // User multi field 1
                    { fldUserMulti1, new FieldData("UserMulti") },
                };

                // URL field 1
                fieldData[fldUrl1].Properties.Add("Url", "https://pnp.com");
                fieldData[fldUrl1].Properties.Add("Description", "PnP Rocks");
                item.Add(fldUrl1, addedUrlField1.NewFieldUrlValue(fieldData[fldUrl1].Properties["Url"].ToString(), fieldData[fldUrl1].Properties["Description"].ToString()));

                // URL field 2 -  no description value set on create
                fieldData[fldUrl2].Properties.Add("Url", "https://pnp.com");
                // set the expected data equal to the url field as that's what we expect
                fieldData[fldUrl2].Properties.Add("Description", fieldData[fldUrl2].Properties["Url"]);
                item.Add(fldUrl2, addedUrlField2.NewFieldUrlValue(fieldData[fldUrl2].Properties["Url"].ToString()));

                // User single field 1
                fieldData[fldUserSingle1].Properties.Add("Principal", currentUser);
                item.Add(fldUserSingle1, addedUserSingleField1.NewFieldUserValue(currentUser));

                // User multi field 1
                var userCollection = addedUserMultiField1.NewFieldValueCollection();
                userCollection.Values.Add(addedUserMultiField1.NewFieldUserValue(currentUser));
                fieldData[fldUserMulti1].Properties.Add("Collection", userCollection);
                item.Add(fldUserMulti1, userCollection);

                var addedItem = await myList.Items.AddAsync(item);

                //==========================================================
                // Step 4: validate returned list item
                Assert.IsTrue(addedItem.Requested);
                Assert.IsTrue(addedItem["Title"].ToString() == "Item1");

                // URL field 1
                Assert.IsTrue(addedItem[fldUrl1] is IFieldUrlValue);
                Assert.IsTrue((addedItem[fldUrl1] as IFieldUrlValue).Url == fieldData[fldUrl1].Properties["Url"].ToString());
                Assert.IsTrue((addedItem[fldUrl1] as IFieldUrlValue).Description == fieldData[fldUrl1].Properties["Description"].ToString());

                // URL field 2
                Assert.IsTrue(addedItem[fldUrl2] is IFieldUrlValue);
                Assert.IsTrue((addedItem[fldUrl2] as IFieldUrlValue).Url == fieldData[fldUrl2].Properties["Url"].ToString());
                Assert.IsTrue((addedItem[fldUrl2] as IFieldUrlValue).Description == fieldData[fldUrl2].Properties["Description"].ToString());

                // User single field 1
                Assert.IsTrue(addedItem[fldUserSingle1] is IFieldUserValue);
                Assert.IsTrue((addedItem[fldUserSingle1] as IFieldUserValue).LookupId == (fieldData[fldUserSingle1].Properties["Principal"] as ISharePointPrincipal).Id);

                // User multi field 1
                Assert.IsTrue(addedItem[fldUserMulti1] is IFieldValueCollection);
                Assert.IsTrue((addedItem[fldUserMulti1] as IFieldValueCollection).Values[0] == (fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection).Values[0]);

                //==========================================================
                // Step 5: Read list item using GetAsync approach and verify data was written correctly
                await VerifyListItemViaUpdateAsync(2, listTitle, fieldData);

                //==========================================================
                // Step 6: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyListItemViaGetListDataAsStreamAsync(3, listTitle, fieldData);

                //==========================================================
                // Step 7: Update item using CSOM UpdateOverwriteVersionAsync 

                // URL field 1
                fieldData[fldUrl1].Properties["Url"] = $"{fieldData[fldUrl1].Properties["Url"]}/rocks";
                fieldData[fldUrl1].Properties["Description"] = $"{fieldData[fldUrl1].Properties["Description"]}A";
                (addedItem[fldUrl1] as IFieldUrlValue).Url = fieldData[fldUrl1].Properties["Url"].ToString();
                (addedItem[fldUrl1] as IFieldUrlValue).Description = fieldData[fldUrl1].Properties["Description"].ToString();

                // URL field 2
                fieldData[fldUrl2].Properties["Url"] = $"{fieldData[fldUrl2].Properties["Url"]}/rocks";
                (addedItem[fldUrl2] as IFieldUrlValue).Url = fieldData[fldUrl2].Properties["Url"].ToString();
                (addedItem[fldUrl2] as IFieldUrlValue).Description = fieldData[fldUrl2].Properties["Description"].ToString();

                // User single field 1
                fieldData[fldUserSingle1].Properties["Principal"] = userTwo;
                (addedItem[fldUserSingle1] as IFieldUserValue).Principal = fieldData[fldUserSingle1].Properties["Principal"] as ISharePointPrincipal;

                // User multi field2
                (fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection).Values.Add(addedUserMultiField1.NewFieldUserValue(userTwo));
                addedItem[fldUserMulti1] = fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection;

                await addedItem.UpdateOverwriteVersionAsync();

                //==========================================================
                // Step 8: Read list item using GetAsync approach and verify data was written correctly
                await VerifyListItemViaUpdateAsync(4, listTitle, fieldData);

                //==========================================================
                // Step 9: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyListItemViaGetListDataAsStreamAsync(5, listTitle, fieldData);

                //==========================================================
                // Step 10: Blank item using CSOM UpdateOverwriteVersionAsync 

                // URL field 1
                fieldData[fldUrl1].Properties["Url"] = "";
                fieldData[fldUrl1].Properties["Description"] = "";
                (addedItem[fldUrl1] as IFieldUrlValue).Url = fieldData[fldUrl1].Properties["Url"].ToString();
                (addedItem[fldUrl1] as IFieldUrlValue).Description = fieldData[fldUrl1].Properties["Description"].ToString();

                // URL field 2
                fieldData[fldUrl2].Properties["Url"] = "";
                fieldData[fldUrl2].Properties["Description"] = "";
                (addedItem[fldUrl2] as IFieldUrlValue).Url = fieldData[fldUrl2].Properties["Url"].ToString();
                (addedItem[fldUrl2] as IFieldUrlValue).Description = fieldData[fldUrl2].Properties["Description"].ToString();

                // User single field 1
                fieldData[fldUserSingle1].Properties["Principal"] = null;
                (addedItem[fldUserSingle1] as IFieldUserValue).Principal = fieldData[fldUserSingle1].Properties["Principal"] as ISharePointPrincipal;

                // User multi field2
                (fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection).Values.Clear();
                addedItem[fldUserMulti1] = fieldData[fldUserMulti1].Properties["Collection"] as IFieldValueCollection;

                await addedItem.UpdateOverwriteVersionAsync();

                //==========================================================
                // Step 8: Read list item using GetAsync approach and verify data was written correctly
                await VerifyListItemViaUpdateAsync(6, listTitle, fieldData);

                //==========================================================
                // Step 9: Read list item using GetListDataAsStreamAsync approach and verify data was written correctly
                await VerifyListItemViaGetListDataAsStreamAsync(7, listTitle, fieldData);

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        private static void AssertListItemProperties(Dictionary<string, FieldData> fieldData, IListItem addedItem)
        {
            Assert.IsTrue(addedItem.Requested);
            Assert.IsTrue(addedItem["Title"].ToString() == "Item1");

            foreach (var field in fieldData)
            {
                if (field.Value.FieldType == "URL")
                {
                    if (field.Value.Properties["Url"].ToString() == "")
                    {
                        Assert.IsTrue(addedItem[field.Key] == null);
                    }
                    else
                    {
                        Assert.IsTrue(addedItem[field.Key] is IFieldUrlValue);
                        Assert.IsTrue((addedItem[field.Key] as IFieldUrlValue).Url == field.Value.Properties["Url"].ToString());
                        Assert.IsTrue((addedItem[field.Key] as IFieldUrlValue).Description == field.Value.Properties["Description"].ToString());
                    }
                }
                else if (field.Value.FieldType == "UserSingle")
                {
                    Assert.IsTrue(addedItem[field.Key] is IFieldUserValue);
                    int idToCheck;
                    if (field.Value.Properties.ContainsKey("Principal"))
                    {
                        if ((field.Value.Properties["Principal"] as ISharePointPrincipal) == null)
                        {
                            idToCheck = -1;
                        }
                        else
                        {
                            idToCheck = (field.Value.Properties["Principal"] as ISharePointPrincipal).Id;
                        }
                    }
                    else
                    {
                        idToCheck = (int)field.Value.Properties["UserId"];
                    }
                    Assert.IsTrue((addedItem[field.Key] as IFieldUserValue).LookupId == idToCheck);
                }
                else if (field.Value.FieldType == "UserMulti")
                {
                    Assert.IsTrue(addedItem[field.Key] is IFieldValueCollection);

                    var expectedUsers = (field.Value.Properties["Collection"] as IFieldValueCollection).Values.Cast<IFieldUserValue>();

                    foreach (var user in (addedItem[field.Key] as IFieldValueCollection).Values)
                    {
                        Assert.IsTrue(user is IFieldUserValue);
                        // is this user in the list of expected users
                        var expectedUser = expectedUsers.FirstOrDefault(p => p.LookupId == (user as IFieldUserValue).LookupId);
                        Assert.IsTrue(expectedUser != null);
                    }

                }
            }
        }

        private static async Task<IListItem> VerifyListItemViaGetListDataAsStreamAsync(int id, string listTitle, Dictionary<string, FieldData> fieldData, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, id, testName))
            {
                var myList = context.Web.Lists.GetByTitle(listTitle);
                var itemViaGetAsync = myList.Items.FirstOrDefault(p => p.Title == "Item1");

                var listDataOptions = new RenderListDataOptions()
                {
                    RenderOptions = RenderListDataOptionsFlags.ListData,
                };

                var fieldsToLoad = new List<string>() { "Title" };
                foreach (var field in fieldData)
                {
                    fieldsToLoad.Add(field.Key);
                }

                listDataOptions.SetViewXmlFromFields(fieldsToLoad);

                await myList.GetListDataAsStreamAsync(listDataOptions).ConfigureAwait(false);
                var addedItem = myList.Items.First();

                AssertListItemProperties(fieldData, addedItem);

                return addedItem;
            }
        }

        private static async Task<IListItem> VerifyListItemViaUpdateAsync(int id, string listTitle, Dictionary<string, FieldData> fieldData, [System.Runtime.CompilerServices.CallerMemberName] string testName = null)
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, id, testName))
            {
                var myList = context.Web.Lists.GetByTitle(listTitle, p => p.Title, p => p.Items, p => p.Fields.LoadProperties(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title));
                var addedItem = myList.Items.FirstOrDefault(p => p.Title == "Item1");

                AssertListItemProperties(fieldData, addedItem);

                return addedItem;
            }
        }
        #endregion


        //[TestMethod]
        //public async Task FieldTypeReadUrl()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        /*
        //        var list = await context.Web.Lists.GetByTitleAsync("FieldTypes");

        //        var listDataOptions = new RenderListDataOptions()
        //        {
        //            RenderOptions = RenderListDataOptionsFlags.ListData,
        //        };

        //        listDataOptions.SetViewXmlFromFields(new List<string>() { "Title", "Url", "PersonSingle", "PersonMultiple", 
        //                                                                  "MMSingle", "MMMultiple", "LookupSingle", "LookupMultiple", 
        //                                                                  "Location", "Bool", "Number", "DateTime", "ChoiceSingle", "ChoiceMultiple" });

        //        await list.GetListDataAsStreamAsync(listDataOptions).ConfigureAwait(false);

        //        var item = list.Items.First();
        //        */


        //        var list = await context.Web.Lists.GetByTitleAsync("FieldTypes", p => p.Title, p => p.Items, p => p.Fields.LoadProperties(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title));

        //        var item = list.Items.FirstOrDefault(p => p.Title == "Item1");


        //        //Assert.IsTrue(item != null);
        //        //Assert.IsTrue(item["Url"] != null);
        //        //Assert.IsTrue(item["Url"] is IFieldUrlValue);
        //        //Assert.IsTrue(item["PersonSingle"] is IFieldUserValue);
        //        //Assert.IsTrue(item["PersonMultiple"] is IFieldValueCollection);
        //        //Assert.IsTrue(item["MMSingle"] is IFieldTaxonomyValue);
        //        //Assert.IsTrue(item["MMMultiple"] is IFieldValueCollection);
        //        //Assert.IsTrue(item["LookupSingle"] is IFieldValueCollection);
        //        //Assert.IsTrue(item["LookupMultiple"] is IFieldValueCollection);
        //        //Assert.IsTrue(item["Location"] is IFieldLocationValue);
        //        //Assert.IsTrue(item["ChoiceSingle"] is string);
        //        //Assert.IsTrue(item["ChoiceMultiple"] is List<string>);

        //        // Clear user field testing
        //        //var urlField = list.Fields.First(p => p.InternalName == "Url");
        //        //item["Url"] = item.NewFieldUrlValue(urlField, "");

        //        //// Update url field
        //        //var urlField = list.Fields.First(p => p.InternalName == "Url");

        //        //(item["Url"] as IFieldUrlValue).Url = "https://pnp.com/3";
        //        //(item["Url"] as IFieldUrlValue).Description = "something3";

        //        // clear person field testing
        //        //(item["PersonSingle"] as IFieldUserValue).LookupId = -1;
        //        //(item["PersonMultiple"] as IFieldValueCollection).Values.Clear();

        //        //// Update user fields
        //        //var personMultipleField = list.Fields.First(p => p.InternalName == "PersonMultiple");
        //        //(item["PersonSingle"] as IFieldUserValue).LookupId = 6;
        //        //(item["PersonMultiple"] as IFieldValueCollection).Values.Clear();
        //        //(item["PersonMultiple"] as IFieldValueCollection).Values.Add(item.NewFieldUserValue(personMultipleField, 6));
        //        //(item["PersonMultiple"] as IFieldValueCollection).Values.Add(item.NewFieldUserValue(personMultipleField, 14));

        //        // Clear lookup field testing
        //        //(item["LookupSingle"] as IFieldValueCollection).Values.Clear();
        //        //(item["LookupMultiple"] as IFieldValueCollection).Values.Clear();

        //        //// Update lookup fields
        //        //var lookupSingleField = list.Fields.First(p => p.InternalName == "LookupSingle");
        //        //var lookupMultipleField = list.Fields.First(p => p.InternalName == "LookupMultiple");

        //        //(item["LookupSingle"] as IFieldValueCollection).Values.Clear();
        //        //(item["LookupSingle"] as IFieldValueCollection).Values.Add(item.NewFieldLookupValue(lookupSingleField, 122));
        //        //(item["LookupMultiple"] as IFieldValueCollection).Values.Clear();
        //        //(item["LookupMultiple"] as IFieldValueCollection).Values.Add(item.NewFieldLookupValue(lookupMultipleField, 1));
        //        //(item["LookupMultiple"] as IFieldValueCollection).Values.Add(item.NewFieldLookupValue(lookupMultipleField, 71));
        //        //(item["LookupMultiple"] as IFieldValueCollection).Values.Add(item.NewFieldLookupValue(lookupMultipleField, 122));

        //        // Clear taxonomy fields
        //        //item["MMSingle"] = null;
        //        //(item["MMMultiple"] as IFieldValueCollection).Values.Clear();

        //        //// Update taxonomy fields
        //        //var mmSingleField = list.Fields.First(p => p.InternalName == "MMSingle");

        //        //item["MMSingle"] = item.NewFieldTaxonomyValue(mmSingleField, Guid.Parse("0b709a34-a74e-4d07-b493-48041424a917"), "HBI");

        //        //(item["MMMultiple"] as IFieldValueCollection).RemoveTaxonomyFieldValue(Guid.Parse("1824510b-00e1-40ac-8294-528b1c9421e0"));
        //        //var mmMultipleField = list.Fields.First(p => p.InternalName == "MMMultiple");
        //        //var taxCollection = item.NewFieldValueCollection(mmMultipleField, item.Values);
        //        //taxCollection.Values.Add(item.NewFieldTaxonomyValue(mmMultipleField, Guid.Parse("ed5449ec-4a4f-4102-8f07-5a207c438571"), "LBI"));
        //        //taxCollection.Values.Add(item.NewFieldTaxonomyValue(mmMultipleField, Guid.Parse("1824510b-00e1-40ac-8294-528b1c9421e0"), "MBI"));
        //        //item["MMMultiple"] = taxCollection;

        //        // Clear choice fields
        //        //item["ChoiceSingle"] = null;
        //        //item["ChoiceMultiple"] = new List<string>();

        //        // Update Choice field
        //        //item["ChoiceSingle"] = "Choice 1";

        //        //item["ChoiceMultiple"] = new List<string>() { "Choice 2", "Choice 3", "Choice 4" };



        //        // save update back
        //        await item.UpdateAsync();

        //        //await item.UpdateOverwriteVersionAsync();

        //    }
        //}


    }
}
