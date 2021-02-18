using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.QueryModel;
using PnP.Core.Model;
using PnP.Core.Model.Security;

namespace PnP.Core.Test.Security
{
    [TestClass]
    public class SharePointGroupTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetSharePointGroups()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.SiteGroups);

                Assert.IsTrue(web.SiteGroups.Length > 0);
            }
        }

        [TestMethod]
        public async Task GetSharePointGroupRoleDefinitions()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var groupName = TestCommon.GetPnPSdkTestAssetName("TestGroup");
                var roleDefName = TestCommon.GetPnPSdkTestAssetName("TestRoleDef");

                // Create roledefinition to add to group
                var roleDefinition = await context.Web.RoleDefinitions.AddAsync(roleDefName, Model.SharePoint.RoleType.Reader, new Model.SharePoint.PermissionKind[] { Model.SharePoint.PermissionKind.Open });

                var siteGroup = await context.Web.SiteGroups.AddAsync(groupName);

                await siteGroup.AddRoleDefinitionsAsync(roleDefName);

                try
                {
                    var roleDefinitions = await siteGroup.GetRoleDefinitionsAsync();

                    Assert.IsTrue(roleDefinitions.Length > 0);
                }
                finally
                {
                    // delete role def
                    await roleDefinition.DeleteAsync();

                    // delete group
                    await siteGroup.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task AddSharePointGroupRoleDefinition()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var groupName = TestCommon.GetPnPSdkTestAssetName("TestGroup");
                var roleDefName = TestCommon.GetPnPSdkTestAssetName("TestRoleDef");

                // Create roledefinition to add to group
                var roleDefinition = await context.Web.RoleDefinitions.AddAsync(roleDefName, Model.SharePoint.RoleType.Reader, new Model.SharePoint.PermissionKind[] { Model.SharePoint.PermissionKind.Open });

                var siteGroup = await context.Web.SiteGroups.AddAsync(groupName);

                await siteGroup.AddRoleDefinitionsAsync(roleDefName);

                try
                {
                    var roleDefinitions = await siteGroup.GetRoleDefinitionsAsync();

                    var foundRole = roleDefinitions.AsEnumerable().FirstOrDefault(d => d.Name == roleDefName);
                    Assert.IsNotNull(foundRole);
                }
                finally
                {
                    await siteGroup.RemoveRoleDefinitionsAsync(roleDefName);

                    await siteGroup.DeleteAsync();

                    await roleDefinition.DeleteAsync();
                }
            }
        }


        [TestMethod]
        public async Task RemoveSharePointGroupRoleDefinition()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var groupName = TestCommon.GetPnPSdkTestAssetName("TestGroup");
                var roleDefName = TestCommon.GetPnPSdkTestAssetName("TestRoleDef");

                // Create roledefinition to add to group
                var roleDefinition = await context.Web.RoleDefinitions.AddAsync(roleDefName, Model.SharePoint.RoleType.Reader, new Model.SharePoint.PermissionKind[] { Model.SharePoint.PermissionKind.Open });

                var siteGroup = await context.Web.SiteGroups.AddAsync(groupName);

                try
                {
                    await siteGroup.AddRoleDefinitionsAsync(roleDefName);

                    await siteGroup.RemoveRoleDefinitionsAsync(roleDefName);

                    var roleDefinitions = await siteGroup.GetRoleDefinitionsAsync();

                    Assert.IsNull(roleDefinitions);
                }
                finally
                {
                    await siteGroup.DeleteAsync();

                    await roleDefinition.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task AddSharePointGroup()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var groupName = TestCommon.GetPnPSdkTestAssetName("TestGroup");
                ISharePointGroup siteGroup = null;

                try
                {
                    await context.Web.SiteGroups.AddAsync(groupName);

                    siteGroup = await context.Web.SiteGroups.FirstOrDefaultAsync(g => g.Title == groupName);

                    Assert.IsTrue(siteGroup.Requested);
                    Assert.AreEqual(siteGroup.Title, groupName);
                }
                finally
                {
                    if (siteGroup != null)
                    {
                        await siteGroup.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task UpdateSharePointGroup()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var groupName = TestCommon.GetPnPSdkTestAssetName("TestGroup");
                var groupNameRenamed = TestCommon.GetPnPSdkTestAssetName("TestGroup-Renamed");
                ISharePointGroup updatedSiteGroup = null;

                try
                {
                    await context.Web.SiteGroups.AddAsync(groupName);

                    var siteGroup = await context.Web.SiteGroups.FirstOrDefaultAsync(g => g.Title == groupName);

                    siteGroup.Title = groupNameRenamed;

                    await siteGroup.UpdateAsync();

                    updatedSiteGroup = await context.Web.SiteGroups.FirstOrDefaultAsync(g => g.Title == groupNameRenamed);

                    Assert.IsTrue(updatedSiteGroup.Requested);
                }
                finally
                {
                    if (updatedSiteGroup != null)
                    {
                        await updatedSiteGroup.DeleteAsync();
                    }
                }
            }
        }

    }
}
