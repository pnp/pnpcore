using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PnP.Core.Modernization.Tests.Transform
{
    [TestClass]
    [TestCategory("Integration")]
    public class ContextTests
    {
        [TestMethod]
        public void ContextSPO_BasicTest()
        {
            using (var clientContext = TestCommon.CreateClientContext())
            {
                var web = clientContext.Web;
                var title = web.EnsureProperty(o => o.Title);
                Console.WriteLine(title);
                Assert.IsTrue(!string.IsNullOrEmpty(title)); 
            }
        }
    }
}
