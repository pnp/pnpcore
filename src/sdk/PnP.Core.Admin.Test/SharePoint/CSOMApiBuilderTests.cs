using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant;
using PnP.Core.Services.Core.CSOM;
using System;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class CSOMApiBuilderTests
    {
        [TestMethod]
        public void CSOMApiCallBuilder_Test_BuildUpdateItemApiCall()
        {
            RemoveSiteRequest request = new RemoveSiteRequest(new Uri("https://bertonline.sharepoint.com/sites/removeme"));

            CSOMApiCallBuilder builder = new CSOMApiCallBuilder();
            builder.AddRequest(request);
            string xmlBody = builder.SerializeCSOMRequests();

            Assert.AreEqual(xmlBody, "<Request AddExpandoFieldTypeSuffix=\"true\" SchemaVersion=\"15.0.0.0\" LibraryVersion=\"16.0.0.0\" ApplicationName=\"pnp core sdk\" xmlns=\"http://schemas.microsoft.com/sharepoint/clientquery/2009\"><Actions><Query Id=\"3\" ObjectPathId=\"1\" ><Query SelectAllProperties=\"false\"><Properties><Property Name=\"PollingInterval\" ScalarProperty=\"true\" /><Property Name=\"IsComplete\" ScalarProperty=\"true\" /></Properties></Query></Query></Actions><ObjectPaths><Constructor Id=\"2\" TypeId=\"{268004ae-ef6b-4e9b-8425-127220d84719}\" /><Method Id=\"1\" ParentId=\"2\" Name=\"RemoveSite\"><Parameters><Parameter Type=\"String\">https://bertonline.sharepoint.com/sites/removeme</Parameter></Parameters></Method></ObjectPaths></Request>");
        }       
    }
}
