using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Security
{
    [TestClass]
    public class RoleAssignmentsTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetWebRoleAssignments()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.Web.Load(w => w.RoleAssignments);
                Assert.IsTrue(context.Web.RoleAssignments.Length > 0);
            }
        }


        [TestMethod]
        public async Task WebWithCustomPermissions()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string webTitle = "permissionweb";

                var addedWeb = await context.Web.Webs.AddAsync(new WebOptions { Title = webTitle, Url = webTitle });

                try
                {
                    using (var context2 = await TestCommon.Instance.CloneAsync(context, addedWeb.Url, 1))
                    {
                        // break permission inheritance
                        context2.Web.BreakRoleInheritance(false, true);

                        // get current user
                        var currentUser = await context2.Web.GetCurrentUserAsync();

                        // Assign current user "Full Control"
                        context2.Web.AddRoleDefinitions(currentUser.Id, new string[] { "Full Control", "Edit" });

                        // reload web
                        await context2.Web.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                        Assert.IsTrue(context2.Web.HasUniqueRoleAssignments);
                        Assert.IsTrue(context2.Web.RoleAssignments.Length == 1);
                        Assert.IsTrue(context2.Web.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 2);

                        // remove the editor role
                        context2.Web.RemoveRoleDefinitions(currentUser.Id, new string[] { "Edit" });

                        // reload web
                        await context2.Web.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                        Assert.IsTrue(context2.Web.HasUniqueRoleAssignments);
                        Assert.IsTrue(context2.Web.RoleAssignments.Length == 1);
                        Assert.IsTrue(context2.Web.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 1);

                        // reset permission inheritance
                        context2.Web.ResetRoleInheritance();
                        // reload web
                        await context2.Web.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                        Assert.IsFalse(context2.Web.HasUniqueRoleAssignments);
                        Assert.IsTrue(context2.Web.RoleAssignments.Length > 1);
                    }
                }
                finally
                {
                    // Delete the created web again
                    await addedWeb.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task WebWithCustomPermissionsBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string webTitle = "permissionwebbatch";

                var addedWeb = await context.Web.Webs.AddAsync(new WebOptions { Title = webTitle, Url = webTitle });

                try
                {
                    using (var context2 = await TestCommon.Instance.CloneAsync(context, addedWeb.Url, 1))
                    {
                        // get current user
                        var currentUser = await context2.Web.GetCurrentUserAsync();
                        var roleDefinitions = (await context2.Web.GetAsync(p => p.RoleDefinitions)).RoleDefinitions;
                        var fullControlRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Full Control");
                        var editRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Edit");

                        var batch = context2.NewBatch();

                        // break permission inheritance
                        context2.Web.BreakRoleInheritanceBatch(batch, false, true);

                        // Assign current user "Full Control"
                        context2.Web.AddRoleDefinitionBatch(batch, currentUser.Id, fullControlRole);
                        context2.Web.AddRoleDefinitionBatch(batch, currentUser.Id, editRole);

                        // Execute batch
                        context2.Execute(batch);

                        // reload web
                        await context2.Web.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                        Assert.IsTrue(context2.Web.HasUniqueRoleAssignments);
                        Assert.IsTrue(context2.Web.RoleAssignments.Length == 1);
                        Assert.IsTrue(context2.Web.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 2);

                        // remove the editor role
                        batch = context2.NewBatch();
                        context2.Web.RemoveRoleDefinitionBatch(batch, currentUser.Id, editRole);

                        // Execute batch
                        context2.Execute(batch);

                        // reload web
                        await context2.Web.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                        Assert.IsTrue(context2.Web.HasUniqueRoleAssignments);
                        Assert.IsTrue(context2.Web.RoleAssignments.Length == 1);
                        Assert.IsTrue(context2.Web.RoleAssignments.AsRequested().First().RoleDefinitions.Length == 1);

                        // reset permission inheritance
                        batch = context2.NewBatch();
                        context2.Web.ResetRoleInheritanceBatch(batch);
                        // Execute batch
                        context2.Execute(batch);

                        // reload web
                        await context2.Web.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions), w => w.HasUniqueRoleAssignments);

                        Assert.IsFalse(context2.Web.HasUniqueRoleAssignments);
                        Assert.IsTrue(context2.Web.RoleAssignments.Length > 1);
                    }
                }
                finally
                {
                    // Delete the created web again
                    await addedWeb.DeleteAsync();
                }
            }
        }

    }
}
