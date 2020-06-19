using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class SettingTests
    {

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            TestCommon.Instance.Mocking = false;
        }

        //[TestMethod]
        //public void GetFeatures()
        //{
        //    TestCommon.Instance.Mocking = false;
        //    using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
        //    {
        //        IWeb web = context.Web.Get(p => p.Features);
        //        Assert.IsTrue(web.Features.Length > 0);
                
        //        //IFeatureCollection features = context.Web.Features;
        //        //Assert.IsTrue(features.Length > 0);

        //        //IContentType contentType = web.Features.FirstOrDefault(p => p.Name == "Item");
        //        //// Test a string property
        //        //Assert.AreEqual(contentType.Name, "Item");
        //        //// Test a boolean property
        //        //Assert.IsFalse(contentType.Hidden);
        //    }
        //}

        [TestMethod]
        public async Task GetFeaturesAsync()
        {
            TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                IWeb web = await context.Web.GetAsync(p => p.Features);
                Assert.IsTrue(web.Features.Length > 0);

            }
        }

        [TestMethod]
        public async Task GetFeaturesByIdAsync()
        {
            TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                IWeb web = await context.Web.GetAsync(p => p.Features);

                var id = new Guid("b6917cb1-93a0-4b97-a84d-7cf49975d4ec"); //SitePages
                IFeature feature = await web.Features.EnableAsync(id);

                Assert.IsNotNull(feature);
                Assert.IsNotNull(feature.DefinitionId);
                Assert.IsTrue(feature.DefinitionId != Guid.Empty);

            }
        }

    }
}
