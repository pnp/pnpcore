using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class SiteManagerTests
    {

        [TestMethod]
        public async Task GetSiteCollectionProperties()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    await context.GetSiteCollectionManager().GetSiteCollectionPropertiesAsync(context.Uri);
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
            }
        }

    }
}
