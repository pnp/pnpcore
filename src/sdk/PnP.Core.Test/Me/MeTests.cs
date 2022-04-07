using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.Me
{
    [TestClass]
    public class MeTest
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
             TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetMeTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var me = await context.Me.GetAsync();
                Assert.IsNotNull(me.Id);
            }
        }
    }


}
