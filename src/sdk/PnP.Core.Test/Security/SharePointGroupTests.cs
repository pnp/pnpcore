using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Security;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System.Linq;
using System.Threading.Tasks;

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

                    var foundRole = roleDefinitions.AsRequested().FirstOrDefault(d => d.Name == roleDefName);
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
        public async Task AddSharePointGroupWithComplexDescription()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var groupName = TestCommon.GetPnPSdkTestAssetName("TestGroupDescription");
                ISharePointGroup siteGroup = null;

                try
                {
                    siteGroup = await context.Web.SiteGroups.AddAsync(groupName);

                    siteGroup.Description = "<title>GitHub - pnp/pnpcore: The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API&#39;s being called. GitHub - pnp/pnpcore: The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API&#39;s being called. GitHub - pnp/pnpcore: The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API&#39;s being called</title><meta name=\"description\" content=\"The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API&#39;s being called - GitHub - pnp/pnpcore: The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API&#39;s being called\"><link rel=\"search\" type=\"application/opensearchdescription+xml\" href=\"/opensearch.xml\" title=\"GitHub\"><link rel=\"fluid-icon\" href=\"https://github.com/fluidicon.png\" title=\"GitHub\">";
                    await siteGroup.UpdateAsync();

                    siteGroup = await context.Web.SiteGroups.FirstOrDefaultAsync(g => g.Title == groupName);

                    Assert.IsTrue(siteGroup.Requested);
                    Assert.AreEqual(siteGroup.Description.Length, 511);
                    Assert.AreEqual(siteGroup.Description, "GitHub - pnp/pnpcore: The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API's being called. GitHub - pnp/pnpcore: The PnP Core SDK is a modern .NET SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API's being called. GitHub - pnp/pnpcore: The PnP Core SDK is a mod");
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
        public async Task AddSharePointGroupWithOptions()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var groupName = TestCommon.GetPnPSdkTestAssetName("TestGroupOptions");
                ISharePointGroup addedGroup = null;

                try
                {
                    addedGroup = await context.Web.SiteGroups.AddAsync(groupName);

                    await addedGroup.AddRoleDefinitionsAsync("Full Control");

                    addedGroup.Title = "test group";
                    addedGroup.Description = "just a sample test group";
                    addedGroup.IsHiddenInUI = false;
                    addedGroup.AllowMembersEditMembership = true;
                    addedGroup.AllowRequestToJoinLeave = true;
                    addedGroup.OnlyAllowMembersViewMembership = true;
                    addedGroup.AutoAcceptRequestToJoinLeave = true;
                    addedGroup.CanCurrentUserEditMembership = true;
                    addedGroup.CanCurrentUserManageGroup = true;
                    addedGroup.CanCurrentUserViewMembership = true;
                    addedGroup.RequestToJoinLeaveEmailSetting = "joe@contoso.sharepoint.com";
                    await addedGroup.UpdateAsync();

                    addedGroup = await context.Web.SiteGroups.QueryProperties(p => p.All, p => p.CanCurrentUserEditMembership,
                        p => p.CanCurrentUserManageGroup, p => p.CanCurrentUserViewMembership).FirstOrDefaultAsync(g => g.Title == "test group");

                    Assert.IsTrue(addedGroup.Requested);
                    Assert.AreEqual(addedGroup.Title, "test group");
                    Assert.AreEqual(addedGroup.Description, "just a sample test group");
                    Assert.AreEqual(addedGroup.IsHiddenInUI, false);
                    Assert.AreEqual(addedGroup.AllowMembersEditMembership, true);
                    Assert.AreEqual(addedGroup.AllowRequestToJoinLeave, true);
                    Assert.AreEqual(addedGroup.OnlyAllowMembersViewMembership, true);
                    Assert.AreEqual(addedGroup.AutoAcceptRequestToJoinLeave, true);
                    Assert.AreEqual(addedGroup.CanCurrentUserEditMembership, true);
                    Assert.AreEqual(addedGroup.CanCurrentUserManageGroup, true);
                    Assert.AreEqual(addedGroup.CanCurrentUserViewMembership, true);
                    Assert.IsTrue(!string.IsNullOrEmpty(addedGroup.OwnerTitle));
                    Assert.AreEqual(addedGroup.RequestToJoinLeaveEmailSetting, "joe@contoso.sharepoint.com");

                }
                finally
                {
                    if (addedGroup != null)
                    {
                        await addedGroup.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task AddRemoveSharePointGroupUsers()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var groupName = TestCommon.GetPnPSdkTestAssetName("TestGroupUsers");
                ISharePointGroup addedGroup = null;

                try
                {
                    addedGroup = await context.Web.SiteGroups.AddAsync(groupName);

                    await addedGroup.AddRoleDefinitionsAsync("Full Control");

                    // Get current user
                    var currentUser = await context.Web.GetCurrentUserAsync();

                    await addedGroup.AddUserAsync(currentUser.LoginName);

                    addedGroup = await context.Web.SiteGroups.QueryProperties(p => p.Users).FirstOrDefaultAsync(g => g.Title == groupName);

                    Assert.IsTrue(addedGroup.Requested);
                    Assert.IsNotNull(addedGroup.Users.AsRequested().FirstOrDefault(p => p.LoginName == currentUser.LoginName));

                    // remove the user again
                    await addedGroup.RemoveUserAsync(currentUser.Id);
                    addedGroup = await context.Web.SiteGroups.QueryProperties(p => p.Users).FirstOrDefaultAsync(g => g.Title == groupName);

                    Assert.IsTrue(addedGroup.Requested);
                    Assert.IsNull(addedGroup.Users.AsRequested().FirstOrDefault(p => p.LoginName == currentUser.LoginName));
                }
                finally
                {
                    if (addedGroup != null)
                    {
                        await addedGroup.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task AddRemoveSharePointGroupUsersOption2()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var groupName = TestCommon.GetPnPSdkTestAssetName("TestGroupUsers");
                ISharePointGroup addedGroup = null;

                try
                {
                    addedGroup = await context.Web.SiteGroups.AddAsync(groupName);

                    await addedGroup.AddRoleDefinitionsAsync("Full Control");

                    // Get current user
                    var currentUser = await context.Web.GetCurrentUserAsync();

                    addedGroup.Users.Add(currentUser.LoginName);

                    // Query the group again
                    addedGroup = await context.Web.SiteGroups.QueryProperties(p => p.Users).FirstOrDefaultAsync(g => g.Title == groupName);

                    Assert.IsTrue(addedGroup.Requested);
                    Assert.IsNotNull(addedGroup.Users.AsRequested().FirstOrDefault(p => p.LoginName == currentUser.LoginName));

                    // remove the user again
                    addedGroup.Users.AsRequested().FirstOrDefault(p => p.LoginName == currentUser.LoginName).Delete();

                    addedGroup = await context.Web.SiteGroups.QueryProperties(p => p.Users).FirstOrDefaultAsync(g => g.Title == groupName);

                    Assert.IsTrue(addedGroup.Requested);
                    Assert.IsNull(addedGroup.Users.AsRequested().FirstOrDefault(p => p.LoginName == currentUser.LoginName));
                }
                finally
                {
                    if (addedGroup != null)
                    {
                        await addedGroup.DeleteAsync();
                    }
                }
            }
        }

        [TestMethod]
        public async Task AddRemoveSharePointGroupUsersOption2Batch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var groupName = TestCommon.GetPnPSdkTestAssetName("TestGroupUsersBatch");
                ISharePointGroup addedGroup = null;

                try
                {
                    addedGroup = await context.Web.SiteGroups.AddAsync(groupName);

                    await addedGroup.AddRoleDefinitionsAsync("Full Control");

                    // Get current user
                    var currentUser = await context.Web.GetCurrentUserAsync();

                    addedGroup.Users.AddBatch(currentUser.LoginName);

                    context.Execute();

                    // Query the group again
                    addedGroup = await context.Web.SiteGroups.QueryProperties(p => p.Users).FirstOrDefaultAsync(g => g.Title == groupName);

                    Assert.IsTrue(addedGroup.Requested);
                    Assert.IsNotNull(addedGroup.Users.AsRequested().FirstOrDefault(p => p.LoginName == currentUser.LoginName));

                    // remove the user again
                    addedGroup.Users.AsRequested().FirstOrDefault(p => p.LoginName == currentUser.LoginName).DeleteBatch();
                    context.Execute();

                    addedGroup = await context.Web.SiteGroups.QueryProperties(p => p.Users).FirstOrDefaultAsync(g => g.Title == groupName);

                    Assert.IsTrue(addedGroup.Requested);
                    Assert.IsNull(addedGroup.Users.AsRequested().FirstOrDefault(p => p.LoginName == currentUser.LoginName));
                }
                finally
                {
                    if (addedGroup != null)
                    {
                        await addedGroup.DeleteAsync();
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
