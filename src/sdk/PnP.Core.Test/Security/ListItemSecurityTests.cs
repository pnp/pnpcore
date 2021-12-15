using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Security
{
    [TestClass]
    public class ListItemSecurityTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;            
        }

        [TestMethod]
        public async Task ResetRoleInheritanceTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listName = TestCommon.GetPnPSdkTestAssetName("ResetRoleInheritanceTest");
                await InitializeTestListAsync(context, listName);
                var myList = await context.Web.Lists.GetByTitleAsync(listName, p => p.Title, p => p.Items);

                // get first item and do a reset role inheritance
                var first = myList.Items.First();

                await first.ResetRoleInheritanceBatchAsync();
                await context.ExecuteAsync();

                // Delete the list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task BreakRoleInheritanceTestNoCopyAssignmentsNoClearSubscopes()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listName = TestCommon.GetPnPSdkTestAssetName("BreakRoleInheritanceTestNoCopyAssignmentsNoClearSubscopes");
                await InitializeTestListAsync(context, listName);
                var myList = await context.Web.Lists.GetByTitleAsync(listName, p => p.Title, p => p.Items);


                // get first item and do a break role inheritance
                var first = myList.Items.First();

                await first.BreakRoleInheritanceBatchAsync(false, false);
                await context.ExecuteAsync();

                // Delete the list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task BreakRoleInheritanceOnFolder()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Add library with file to site
                var libraryName = TestCommon.GetPnPSdkTestAssetName("BreakRoleInheritanceOnFolder");
                var testLibrary = await context.Web.Lists.AddAsync(libraryName, ListTemplateType.DocumentLibrary);
                await testLibrary.EnsurePropertiesAsync(p => p.RootFolder);
                IFile testDocument = await testLibrary.RootFolder.Files.AddAsync("ClassifyAndExtractFile.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                IFolder folder = await testLibrary.RootFolder.AddFolderAsync("folder");
                var fileInFolder = await folder.Files.AddAsync("ClassifyAndExtractFile1.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);

                var source = await context.Web.GetFolderByServerRelativeUrlAsync(folder.ServerRelativeUrl, x => x.ItemCount, x => x.Folders, x => x.Files, x => x.ListItemAllFields);

                await source.ListItemAllFields.BreakRoleInheritanceAsync(false, true);

                // Delete the list
                await testLibrary.DeleteAsync();

            }
        }

        [TestMethod]
        public async Task GetRoleAssignmentsTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listName = TestCommon.GetPnPSdkTestAssetName("GetRoleAssignmentsTest");
                await InitializeTestListAsync(context, listName);
                var myList = await context.Web.Lists.GetByTitleAsync(listName, p => p.Title, p => p.Items);

                // get first item
                var first = myList.Items.First();

                // Break role inheritance, add a role assignment
                var firstUser = context.Web.SiteUsers.FirstOrDefault(u => u.IsSiteAdmin);

                await first.ResetRoleInheritanceAsync();
                await first.BreakRoleInheritanceAsync(false, false);

                // fetch the roleassignment collection
                await first.LoadAsync(i => i.RoleAssignments);

                // Check role assignment. Should be 1, because breaking the inheritence added the current user
                Assert.AreEqual(1, first.RoleAssignments.Length);

                // Delete the list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddItemRoleAssignmentTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new list
                string listName = TestCommon.GetPnPSdkTestAssetName("AddItemRoleAssignmentTest");
                await InitializeTestListAsync(context, listName);
                var myList = await context.Web.Lists.GetByTitleAsync(listName, p => p.Title, p => p.Items.QueryProperties(p => p.All, p => p.HasUniqueRoleAssignments));

                // get first item and fetch the role assignments
                var first = myList.Items.First();
                await first.GetAsync(i => i.RoleAssignments);

                // Reset the role inheritance first
                await first.ResetRoleInheritanceAsync();

                // Break role inheritance
                await first.BreakRoleInheritanceAsync(false, false);

                // Fetch a user
                // The below query sometimes brings back "Everyone"
                var firstUser = context.Web.SiteUsers.FirstOrDefault(u => u.IsSiteAdmin);

                await first.AddRoleDefinitionsAsync(firstUser.Id, new string[] { "Full Control" });

                // re-fetch the roleassignment collection
                await first.LoadAsync(i => i.RoleAssignments);

                // Check role assignment. Should be 1 or more
                Assert.IsTrue(first.RoleAssignments.Length > 0);

                var last = myList.Items.AsRequested().Last();
                await last.GetAsync(i => i.RoleAssignments);
                Assert.IsFalse(last.HasUniqueRoleAssignments);

                // Get the current user
                var currentUser = await context.Web.GetCurrentUserAsync();

                // Get role definition to add
                var roleDefinition = await context.Web.RoleDefinitions.FirstOrDefaultAsync(d => d.Name == "Full Control");

                // Batching approach to break role inheritance and apply custom roles for given users
                await last.BreakRoleInheritanceBatchAsync(false, false);
                await last.AddRoleDefinitionBatchAsync(currentUser.Id, roleDefinition);

                // Fire batch that breaks role inheritance and adds role
                await context.ExecuteAsync();

                // re-fetch the roleassignment collection
                await last.LoadAsync(i => i.RoleAssignments, i => i.HasUniqueRoleAssignments);

                // Check role assignment. Should be 2, because breaking the inheritence added the current user, and the user we added above
                Assert.AreEqual(1, last.RoleAssignments.Length);
                Assert.IsTrue(last.HasUniqueRoleAssignments);

                // Delete the list
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task RemoveItemRoleAssignmentTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                // Create a new list
                string listName = TestCommon.GetPnPSdkTestAssetName("RemoveItemRoleAssignmentTest");
                await InitializeTestListAsync(context, listName);
                var myList = await context.Web.Lists.GetByTitleAsync(listName, p => p.Title, p => p.Items);

                // get first item and fetch the role assignments
                var first = myList.Items.First();
                await first.GetAsync(i => i.RoleAssignments);

                // Fetch a user
                // The below query sometimes brings back "Everyone"
                var firstUser = context.Web.SiteUsers.FirstOrDefault(u => u.IsSiteAdmin);

                // Clean out any existing permissions from other tests.
                await first.ResetRoleInheritanceAsync();
                await first.BreakRoleInheritanceAsync(false, false);

                // First add a Role Assignment so we can delete it
                await first.AddRoleDefinitionsAsync(firstUser.Id, new string[] { "Full Control" });

                await first.RemoveRoleDefinitionsAsync(firstUser.Id, new string[] { "Full Control" });

                await first.LoadAsync(f => f.RoleAssignments);

                Assert.AreEqual(0, first.RoleAssignments.Length);

                // Delete the list
                await myList.DeleteAsync();
            }
        }

        private async Task InitializeTestListAsync(PnPContext context, string listName)
        {
            // Create a new list
            var myList = await context.Web.Lists.AddAsync(listName, ListTemplateType.GenericList);
            // Enable versioning
            myList.EnableVersioning = true;
            await myList.UpdateAsync();

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
