using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Copilot.Public.DTO;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.Copilot
{
    [TestClass]
    public class RetrievalTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }
        [TestMethod]
        public async Task RetriveTest()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var response = await context.Copilot.Retrive(new RetrievalRequest
                {
                    DataSource = "sharePoint",
                    QueryString = "Find me documents about test project",
                });
                Assert.AreEqual(response.RetrievalHits.Count, 4);
            }
        }
    }
}
