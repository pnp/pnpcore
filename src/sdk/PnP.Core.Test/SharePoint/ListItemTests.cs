using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
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
            Assert.Inconclusive("Possible Issue in underlying batch update");

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
            Assert.Inconclusive("Possible Issue in underlying batch update");

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
            Assert.Inconclusive("Possible Issue in underlying batch update");

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
                var web = await context.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));

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
                var web2 = await context2.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));
                var myList2 = web2.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList2.GetAsync(p => p.Items);

                var first2 = myList2.Items.First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first2.Title == "blabla");
                Assert.IsTrue(first2.Values["_UIVersionString"].ToString() == "1.0");
            } 
            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                var web = await contextFinal.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));
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
                var web = await context.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));

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
                var web2 = await context2.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));
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
                var web3 = await context3.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));
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
                var web4 = await context4.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));
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
                var web5 = await context5.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));
                var myList5 = web5.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList5.GetAsync(p => p.Items);

                var first5 = myList5.Items.First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first5.Title == "blabla4");
                Assert.IsTrue(first5.Values["_UIVersionString"].ToString() == "1.0");
            }

            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 5))
            {
                var web = await contextFinal.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdateOverwriteVersionNoDataTests()
        {
            //TestCommon.Instance.Mocking = false;

            Assert.Inconclusive("Disabled - Mocking generates an error");

            string listTitle = "UpdateOverwriteVersionNoDataTests";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));

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
                var web2 = await context2.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));
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
                var web3 = await context3.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));
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
                var web4 = await context4.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));
                var myList4 = web4.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                await myList4.GetAsync(p => p.Items);

                var first4 = myList4.Items.First();

                // verify the list item was updated and that we're still at version 1.0
                Assert.IsTrue(first4.Title == "blabla");
                Assert.IsTrue(first4.Values["_UIVersionString"].ToString() == "1.0");
            }
                        
            using (var contextFinal = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await contextFinal.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Items));
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // Cleanup the created list
                await myList.DeleteAsync();
            }
        }
    }
}
