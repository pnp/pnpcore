using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class WebEnumerationTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;

            // Configure the test cases to use application permissions instead of delegated permissions
            //TestCommon.Instance.UseApplicationPermissions = true;
        }

        [TestMethod]
        public async Task EnumerateWebsViaDelegatedPermissionsViaSitesApi()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var webs = context.GetSiteCollectionManager().GetSiteCollectionWebsWithDetails();

                VerifySite(webs, context);
            }
        }

        [TestMethod]
        public async Task EnumerateWebsViaApplicationPermissionsViaSitesApi()
        {
            //TestCommon.Instance.Mocking = false;
            try
            {
                TestCommon.Instance.UseApplicationPermissions = true;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    var webs = context.GetSiteCollectionManager().GetSiteCollectionWebsWithDetails();

                    VerifySite(webs, context);
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
            }
        }


        private void VerifySite(List<IWebWithDetails> webs, PnPContext context)
        {
            Assert.IsTrue(webs.Count > 0);

            foreach(var web in webs)
            {
                Assert.IsTrue(web.Id != Guid.Empty);
                Assert.IsTrue(!string.IsNullOrEmpty(web.ServerRelativeUrl));
                Assert.IsTrue(web.Url != null);
                Assert.IsTrue(!string.IsNullOrEmpty(web.WebTemplate));
                Assert.IsTrue(!string.IsNullOrEmpty(web.WebTemplateConfiguration));
                Assert.IsTrue(web.TimeCreated > DateTime.MinValue);
                Assert.IsTrue(web.LastItemModifiedDate > DateTime.MinValue);
                Assert.IsTrue(web.LastItemUserModifiedDate > DateTime.MinValue);
                Assert.IsTrue(((int)web.Language) > 0);
                Assert.IsTrue(web.Title == null || web.Title != null);
                Assert.IsTrue(web.Description == null || web.Description != null);
            }

        }
    }
}
