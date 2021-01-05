using Microsoft.VisualStudio.TestTools.UnitTesting;
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
                context.Web.Get(w => w.RoleDefinitions);
                Assert.IsTrue(context.Web.RoleDefinitions.Count() > 0);
            }
        }

        [TestMethod]
        public async Task CreateWebRoleDefinition()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.Web.Get(w => w.RoleDefinitions);

                // get existing role def
                var adminRoleDef = context.Web.RoleDefinitions.Where(r => r.Name == "Administrator");
                var roleDefinition = await context.Web.RoleDefinitions.AddAsync("Test RoleDef 2", Model.SharePoint.RoleType.Administrator, new Model.SharePoint.PermissionKind[] { Model.SharePoint.PermissionKind.AddAndCustomizePages }, "", false, 0);
                Assert.IsTrue(roleDefinition.Requested);
                await roleDefinition.DeleteAsync();
            }
        }
    }
}
