using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using System;
using System.Threading.Tasks;
using PnP.Core.Model;
using PnP.Core.Services;
using System.Collections.Generic;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class PipelineTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task PipelinePerRequest()
        {
            TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                CustomHeadersRequestModule customHeadersRequestModule= new CustomHeadersRequestModule(new Dictionary<string, string>() { { "header1", "value1"} });
                var result = await context.Web.WithModule(customHeadersRequestModule).GetAsync( p=> p.All);
                Assert.IsTrue(result.IsPropertyAvailable(p=>p.MasterUrl));

                context.Web.WithModule(customHeadersRequestModule).Load(p => p.All);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.MasterUrl));



            }
        }

    }
}
