using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Test.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Base
{
    /// <summary>
    /// Tests that focus on validating the dynamic configuration for the various Authentication Providers
    /// </summary>
    [TestClass]
    public class AuthenticationConfigurationTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // NOOP so far
        }

        [TestMethod]
        public async Task TestUsernamePasswordWithGraph()
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteUsernamePassword))
            {
                await CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestUsernamePasswordWithSPO()
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteUsernamePassword))
            {
                await CheckAccessToTargetResource(context, false);
            }
        }

        [TestMethod]
        public async Task TestCredentialManagerWithGraph()
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteCredentialManager))
            {
                await CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestCredentialManagerWithSPO()
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteCredentialManager))
            {
                await CheckAccessToTargetResource(context, false);
            }
        }

        //[TestMethod]
        //public async Task TestOnBehalfOfWithGraph()
        //{
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteOnBehalfOf))
        //    {
        //        await CheckAccessToTargetResource(context);
        //    }
        //}

        //[TestMethod]
        //public async Task TestOnBehalfOfWithSPO()
        //{
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteOnBehalfOf))
        //    {
        //        await CheckAccessToTargetResource(context, false);
        //    }
        //}

        //[TestMethod]
        //public async Task TestAspNetCoreWithGraph()
        //{
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteAspNetCore))
        //    {
        //        await CheckAccessToTargetResource(context);
        //    }
        //}

        //[TestMethod]
        //public async Task TestAspNetCoreWithSPO()
        //{
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteAspNetCore))
        //    {
        //        await CheckAccessToTargetResource(context, false);
        //    }
        //}

        [TestMethod]
        public async Task TestX509CertificateWithGraph()
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteX509Certificate))
            {
                await CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestX509CertificateWithSPO()
        {
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteX509Certificate))
            {
                await CheckAccessToTargetResource(context, false);
            }
        }

        private static async Task CheckAccessToTargetResource(PnPContext context, bool graphFirst = true)
        {
            context.GraphFirst = graphFirst;
            var web = await context.Web.GetAsync(w => w.Title);

            Assert.IsNotNull(web.Title);
            Assert.AreEqual("pnpcoresdkdemo", web.Title);
        }
    }
}
