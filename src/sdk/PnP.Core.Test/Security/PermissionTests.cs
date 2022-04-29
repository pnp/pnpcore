using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.Security
{
    [TestClass]
    public class PermissionTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            TestCommon.Instance.Mocking = false;            
        }

        [TestMethod]
        public async Task GetGraphPermissionsAsync()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/pnpcoresdktestgroup/shared documents/PNP_SDK_TEST_GetSharingPermissionsAsyncTest.docx");
                await file.LoadAsync(y => y.GraphPermissions);
            }
        }
    }
}
