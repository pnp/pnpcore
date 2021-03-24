using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Security
{
    [TestClass]
    public class ListItemSecurityTests
    {
        private string listTitle = "PNP_SDK_TEST_ItemSecurityTests";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            // TestCommon.Instance.Mocking = false;            
        }

        [TestInitialize]
        public void Init()
        {
            this.InitializeTestListAsync().Wait();
        }

        [TestMethod]
        public async Task ResetRoleInheritanceTest()
        {
            // TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists.QueryProperties(p => p.Title, p => p.Items));
                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // get first item and do a reset role inheritance
                var first = myList.Items.First();

                await first.ResetRoleInheritanceAsync();
            }
        }

        [TestMethod]
        public async Task BreakRoleInheritanceTestNoCopyAssignmentsNoClearSubscopes()
        {
            // TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists.QueryProperties(p => p.Title, p => p.Items));
                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));


                // get first item and do a break role inheritance
                var first = myList.Items.First();

                await first.BreakRoleInheritanceAsync(false, false);
            }
        }

        [TestMethod]
        public async Task GetRoleAssignmentsTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists.QueryProperties(p => p.Title, p => p.Items));
                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // get first item
                var first = myList.Items.First();

                // Break role inheritance, add a role assignment
                var firstUser = web.SiteUsers.FirstOrDefault(u => u.IsSiteAdmin);

                await first.ResetRoleInheritanceAsync();
                await first.BreakRoleInheritanceAsync(false, false);

                // fetch the roleassignment collection
                await first.LoadAsync(i => i.RoleAssignments);

                // Check role assignment. Should be 1, because breaking the inheritence added the current user
                Assert.AreEqual(1, first.RoleAssignments.Length);
            }
        }

        [TestMethod]
        public async Task AddItemRoleAssignmentTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists.QueryProperties(p => p.Title, p => p.Items), p => p.SiteUsers);
                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // get first item and fetch the role assignments
                var first = myList.Items.First();
                await first.GetAsync(i => i.RoleAssignments);

                // Reset the role inheritance first
                await first.ResetRoleInheritanceAsync();

                // Break role inheritance
                await first.BreakRoleInheritanceAsync(false, false);

                // Fetch a user
                // The below query sometimes brings back "Everyone"
                var firstUser = web.SiteUsers.FirstOrDefault(u => u.IsSiteAdmin);

                await first.AddRoleDefinitionsAsync(firstUser.Id, new string[] { "Full Control" });

                // re-fetch the roleassignment collection
                await first.LoadAsync(i => i.RoleAssignments);

                // Check role assignment. Should be 2, because breaking the inheritence added the current user, and the user we added above
                Assert.AreEqual(2, first.RoleAssignments.Length);
            }
        }

        [TestMethod]
        public async Task RemoveItemRoleAssignmentTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists.QueryProperties(p => p.Title, p => p.Items), w => w.SiteUsers);
                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // get first item and fetch the role assignments
                var first = myList.Items.First();
                await first.GetAsync(i => i.RoleAssignments);

                // Fetch a user
                // The below query sometimes brings back "Everyone"
                var firstUser = web.SiteUsers.FirstOrDefault(u => u.IsSiteAdmin);

                // Clean out any existing permissions from other tests.
                await first.ResetRoleInheritanceAsync();
                await first.BreakRoleInheritanceAsync(false, false);

                // First add a Role Assignment so we can delete it
                await first.AddRoleDefinitionsAsync(firstUser.Id, new string[] { "Full Control" });

                await first.RemoveRoleDefinitionsAsync(firstUser.Id, new string[] { "Full Control" });

                await first.LoadAsync(f => f.RoleAssignments);

                // We should only have one role assignment left
                Assert.AreEqual(1, first.RoleAssignments.Length);
            }
        }

        private async Task InitializeTestListAsync()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists.QueryProperties(p => p.Title, p => p.Items));
                var myList = web.Lists.AsRequested().FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    // Enable versioning
                    myList.EnableVersioning = true;
                    await myList.UpdateAsync();
                }

                await myList.LoadItemsByCamlQueryAsync("<View></View>");

                if (myList.Items.Length == 0)
                {
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
                }
            }
        }
    }
}
