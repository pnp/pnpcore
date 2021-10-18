using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.Security;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Security
{
    [TestClass]
    public class RoleDefinitionTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetWebRoleDefinitions()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.Web.Load(w => w.RoleDefinitions);
                Assert.IsTrue(context.Web.RoleDefinitions.Length > 0);
            }
        }

        [TestMethod]
        public async Task CreateWebRoleDefinition()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.Web.Load(w => w.RoleDefinitions);

                IRoleDefinition roleDefinition = null;

                try
                {
                    // get existing role def
                    var adminRoleDef = context.Web.RoleDefinitions.AsEnumerable().Where(r => r.Name == "Administrator");
                    Assert.IsNotNull(adminRoleDef);

                    roleDefinition = await context.Web.RoleDefinitions.AddAsync("Test RoleDef 2", Model.SharePoint.RoleType.Administrator, new Model.SharePoint.PermissionKind[] { Model.SharePoint.PermissionKind.AddAndCustomizePages }, "", false, 0);
                    Assert.IsTrue(roleDefinition.Requested);
                }
                finally
                {
                    if (roleDefinition != null)
                    {
                        await roleDefinition.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task CreateUpdateDeleteWebRoleDefinition()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.Web.Load(w => w.RoleDefinitions);

                IRoleDefinition roleDefinition = null;

                try
                {
                    // get existing role def
                    var adminRoleDef = context.Web.RoleDefinitions.AsEnumerable().Where(r => r.Name == "Administrator");
                    Assert.IsNotNull(adminRoleDef);

                    // Add new one
                    roleDefinition = await context.Web.RoleDefinitions.AddAsync("Test RoleDef 2", Model.SharePoint.RoleType.Administrator, new Model.SharePoint.PermissionKind[] { Model.SharePoint.PermissionKind.AddAndCustomizePages }, "", false, 0);
                    Assert.IsTrue(roleDefinition.Requested);

                    // grab added role def again from server
                    var addedRoleDefinition = await context.Web.RoleDefinitions.FirstOrDefaultAsync(d => d.Name == "Test RoleDef 2");

                    // Remove AddAndCustomizePages role, add other + set description
                    addedRoleDefinition.BasePermissions.Clear(Model.SharePoint.PermissionKind.AddAndCustomizePages);
                    addedRoleDefinition.BasePermissions.Set(Model.SharePoint.PermissionKind.AddListItems);
                    addedRoleDefinition.Description = "hi new role";
                    await addedRoleDefinition.UpdateAsync();

                    // read again from server
                    addedRoleDefinition = await context.Web.RoleDefinitions.FirstOrDefaultAsync(d => d.Name == "Test RoleDef 2");

                    // Verify
                    Assert.IsTrue(addedRoleDefinition.Description == "hi new role");
                    Assert.IsTrue(addedRoleDefinition.BasePermissions.Has(Model.SharePoint.PermissionKind.AddListItems));
                    Assert.IsFalse(addedRoleDefinition.BasePermissions.Has(Model.SharePoint.PermissionKind.AddAndCustomizePages));
                }
                finally
                {
                    if (roleDefinition != null)
                    {
                        await roleDefinition.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task CreateUpdateDeleteWebRoleDefinitionBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.Web.Load(w => w.RoleDefinitions);

                IRoleDefinition roleDefinition = null;

                try
                {
                    // get existing role def
                    var adminRoleDef = context.Web.RoleDefinitions.AsEnumerable().Where(r => r.Name == "Administrator");
                    Assert.IsNotNull(adminRoleDef);

                    // Add new one
                    roleDefinition = await context.Web.RoleDefinitions.AddBatchAsync("Test RoleDef 2", Model.SharePoint.RoleType.Administrator, new Model.SharePoint.PermissionKind[] { Model.SharePoint.PermissionKind.AddAndCustomizePages }, "", false, 0);
                    await context.ExecuteAsync();

                    Assert.IsTrue(roleDefinition.Requested);

                    // grab added role def again from server
                    var addedRoleDefinition = await context.Web.RoleDefinitions.FirstOrDefaultAsync(d => d.Name == "Test RoleDef 2");

                    // Remove AddAndCustomizePages role, add other + set description
                    addedRoleDefinition.BasePermissions.Clear(Model.SharePoint.PermissionKind.AddAndCustomizePages);
                    addedRoleDefinition.BasePermissions.Set(Model.SharePoint.PermissionKind.AddListItems);
                    addedRoleDefinition.Description = "hi new role";
                    await addedRoleDefinition.UpdateBatchAsync();
                    await context.ExecuteAsync();

                    // read again from server
                    addedRoleDefinition = await context.Web.RoleDefinitions.FirstOrDefaultAsync(d => d.Name == "Test RoleDef 2");

                    // Verify
                    Assert.IsTrue(addedRoleDefinition.Description == "hi new role");
                    Assert.IsTrue(addedRoleDefinition.BasePermissions.Has(Model.SharePoint.PermissionKind.AddListItems));
                    Assert.IsFalse(addedRoleDefinition.BasePermissions.Has(Model.SharePoint.PermissionKind.AddAndCustomizePages));
                }
                finally
                {
                    if (roleDefinition != null)
                    {
                        await roleDefinition.DeleteBatchAsync();
                        await context.ExecuteAsync();
                    }
                }
            }
        }
    }
}
