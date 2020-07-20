using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Test cases for the BatchClient class
    /// </summary>
    [TestClass]
    public class CSOMTests
    {
        private static readonly string WebTitleCsom = "<Request AddExpandoFieldTypeSuffix=\"true\" SchemaVersion=\"15.0.0.0\" LibraryVersion=\"16.0.0.0\" ApplicationName=\".NET Library\" xmlns=\"http://schemas.microsoft.com/sharepoint/clientquery/2009\"><Actions><ObjectPath Id=\"2\" ObjectPathId=\"1\" /><ObjectPath Id=\"4\" ObjectPathId=\"3\" /><Query Id=\"5\" ObjectPathId=\"3\"><Query SelectAllProperties=\"false\"><Properties><Property Name=\"Title\" ScalarProperty=\"true\" /></Properties></Query></Query></Actions><ObjectPaths><StaticProperty Id=\"1\" TypeId=\"{3747adcd-a3c3-41b9-bfab-4a64dd2f1e0a}\" Name=\"Current\" /><Property Id=\"3\" ParentId=\"1\" Name=\"Web\" /></ObjectPaths></Request>";
        private static readonly string WebDescriptionCsom = "<Request AddExpandoFieldTypeSuffix=\"true\" SchemaVersion=\"15.0.0.0\" LibraryVersion=\"16.0.0.0\" ApplicationName=\".NET Library\" xmlns=\"http://schemas.microsoft.com/sharepoint/clientquery/2009\"><Actions><ObjectPath Id=\"2\" ObjectPathId=\"1\" /><ObjectPath Id=\"4\" ObjectPathId=\"3\" /><Query Id=\"5\" ObjectPathId=\"3\"><Query SelectAllProperties=\"false\"><Properties><Property Name=\"Description\" ScalarProperty=\"true\" /></Properties></Query></Query></Actions><ObjectPaths><StaticProperty Id=\"1\" TypeId=\"{3747adcd-a3c3-41b9-bfab-4a64dd2f1e0a}\" Name=\"Current\" /><Property Id=\"3\" ParentId=\"1\" Name=\"Web\" /></ObjectPaths></Request>";


        [ClassInitialize]
        public static void TestFixtureSetup(TestContext testContext)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
            //TestCommon.Instance.GenerateMockingDebugFiles = true;
        }

        [TestMethod]
        public async Task SimplePropertyRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = context.Web;

                // Get the title value via non CSOM
                await web.EnsurePropertiesAsync(p => p.Title);

                var apiCall = new ApiCall(WebTitleCsom);

                var response = await (web as Web).RawRequestAsync(apiCall, HttpMethod.Post);

                Assert.IsTrue(response.CsomResponseJson.Count > 0);
                Assert.IsTrue(response.CsomResponseJson[5].GetProperty("Title").GetString() == web.Title);
            }
        }

        [TestMethod]
        public async Task MultipleSimplePropertyRequests()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = context.Web;

                // Get the title value via non CSOM
                await web.EnsurePropertiesAsync(p => p.Title, p => p.Description);

                var apiCall1 = new ApiCall(WebTitleCsom);
                var apiCall2 = new ApiCall(WebDescriptionCsom);

                var batch = context.BatchClient.EnsureBatch();
                await (web as Web).RawRequestBatchAsync(batch, apiCall1, HttpMethod.Post);
                await (web as Web).RawRequestBatchAsync(batch, apiCall2, HttpMethod.Post);
                await context.ExecuteAsync(batch);

                var response1 = batch.Requests.First().Value;
                var response2 = batch.Requests.Last().Value;

                Assert.IsTrue(response1.CsomResponseJson.Count > 0);
                Assert.IsTrue(response2.CsomResponseJson.Count > 0);
                Assert.IsTrue(response1.CsomResponseJson[5].GetProperty("Title").GetString() == web.Title);
                Assert.IsTrue(response2.CsomResponseJson[5].GetProperty("Description").GetString() == web.Description);
            }
        }

    }
}
