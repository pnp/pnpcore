using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
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
            //TestCommon.Instance.Mocking = false;            
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
                var web = await context.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

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
                var web = await context.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));


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
                var web = await context.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // get first item and fetch the role assignments
                var first = myList.Items.First();
                await first.GetAsync(i => i.RoleAssignments);

                Assert.IsTrue(first.RoleAssignments.Count() > 0);
            }
        }

        [TestMethod]
        public async Task AddItemRoleAssignmentTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // get first item and fetch the role assignments
                var first = myList.Items.First();
                await first.GetAsync(i => i.RoleAssignments);

                // Reset the role inheritance first
                await first.ResetRoleInheritanceAsync();

                // Break role inheritance
                await first.BreakRoleInheritanceAsync(false, false);

                // Fetch a user
                // The below query sometimes brings back "Everyone"
                var firstUser = await context.Web.SiteUsers.GetFirstOrDefaultAsync(u => u.IsSiteAdmin);

                // fetch a role definition
                var roleDefs = await context.Web.RoleDefinitions.GetAsync();
                var roleDef = roleDefs.FirstOrDefault(r => r.Name == "Full Control");

                await first.AddRoleAssignmentAsync(firstUser.Id, roleDef.Id);
            }
        }

        [TestMethod]
        public async Task RemoveItemRoleAssignmentTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // get first item and fetch the role assignments
                var first = myList.Items.First();
                await first.GetAsync(i => i.RoleAssignments);

                // Fetch a user
                // The below query sometimes brings back "Everyone"
                var firstUser = await context.Web.SiteUsers.GetFirstOrDefaultAsync(u => u.IsSiteAdmin);

                // fetch a role definition
                var roleDefs = await context.Web.RoleDefinitions.GetAsync();
                var roleDef = roleDefs.FirstOrDefault(r => r.Name == "Full Control");

                await first.RemoveRoleAssignmentAsync(firstUser.Id, roleDef.Id);
            }
        }

        private async Task InitializeTestListAsync()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                var web = await context.Web.GetAsync(p => p.Lists.LoadProperties(p => p.Title, p => p.Items));
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));
                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    // Enable versioning
                    myList.EnableVersioning = true;
                    await myList.UpdateAsync();
                }

                var items = await myList.Items.GetAsync();
                if (items.Count() == 0)
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
