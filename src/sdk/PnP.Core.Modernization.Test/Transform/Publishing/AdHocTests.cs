using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Publishing;

namespace PnP.Core.Modernization.Tests.Transform.Publishing
{
    [TestClass]
    [TestCategory("Integration")]
    public class AdHocTests
    {

        [TestMethod]
        public void TestMethod1()
        {
            using (ClientContext cc = TestCommon.CreateClientContext())
            {
                PageLayoutManager m = new PageLayoutManager(null);
                var result = m.LoadPageLayoutMappingFile(@"..\..\..\PnP.Core.Modernization\Publishing\pagelayoutmapping_sample.xml");
            }
        }

    }
}
