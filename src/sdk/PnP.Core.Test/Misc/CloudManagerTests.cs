using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Test.Misc
{
    [TestClass()]
    public class CloudManagerTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;

            // Configure the test cases to use application permissions instead of delegated permissions
            //TestCommon.Instance.UseApplicationPermissions = true;
        }

        [TestMethod()]
        [DataRow("https://bertonline.sharepoint.com/sites/prov-2", Microsoft365Environment.Production)]
        [DataRow("https://bertonline.sharepoint.de/sites/prov-2", Microsoft365Environment.Germany)]
        [DataRow("https://bertonline.sharepoint.cn/sites/prov-2", Microsoft365Environment.China)]
        [DataRow("https://bertonline.sharepoint.us/sites/prov-2", Microsoft365Environment.USGovernment)]
        [DataRow("https://bertonline.sharepoint.oops/sites/prov-2", Microsoft365Environment.Production)]
        public void UriToMicrosoft365Environment(string url, Microsoft365Environment expectedOutcome)
        {
            Assert.AreEqual(expectedOutcome, CloudManager.GetEnvironmentFromUri(new Uri(url)));
        }

        [TestMethod()]
        [DataRow("graph.microsoft.com", Microsoft365Environment.Production)]
        [DataRow("graph.microsoft.com", Microsoft365Environment.PreProduction)]
        [DataRow("graph.microsoft.com", Microsoft365Environment.USGovernment)]
        [DataRow("graph.microsoft.de", Microsoft365Environment.Germany)]
        [DataRow("microsoftgraph.chinacloudapi.cn", Microsoft365Environment.China)]
        [DataRow("graph.microsoft.us", Microsoft365Environment.USGovernmentHigh)]
        [DataRow("dod-graph.microsoft.us", Microsoft365Environment.USGovernmentDoD)]
        public void Microsoft365EnvironmentToGraph(string graph, Microsoft365Environment env)
        {
            Assert.AreEqual(graph, CloudManager.GetMicrosoftGraphAuthority(env));
        }

        [TestMethod()]
        [DataRow("login.microsoftonline.com", Microsoft365Environment.Production)]
        [DataRow("login.windows-ppe.net", Microsoft365Environment.PreProduction)]
        [DataRow("login.microsoftonline.com", Microsoft365Environment.USGovernment)]
        [DataRow("login.microsoftonline.de", Microsoft365Environment.Germany)]
        [DataRow("login.chinacloudapi.cn", Microsoft365Environment.China)]
        [DataRow("login.microsoftonline.us", Microsoft365Environment.USGovernmentHigh)]
        [DataRow("login.microsoftonline.us", Microsoft365Environment.USGovernmentDoD)]
        public void Microsoft365EnvironmentToAzureADLogin(string azureADLogin, Microsoft365Environment env)
        {
            Assert.AreEqual(azureADLogin, CloudManager.GetAzureADLoginAuthority(env));
        }


        [TestMethod()]
        public async Task Microsoft365EnvironmentToGraphUri()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.Environment = null;
                Assert.AreEqual(CloudManager.GetGraphBaseUri(context), new Uri($"https://graph.microsoft.com"));

                context.Environment = Microsoft365Environment.Production;
                Assert.AreEqual(CloudManager.GetGraphBaseUri(context), new Uri($"https://graph.microsoft.com"));

                context.Environment = Microsoft365Environment.PreProduction;
                Assert.AreEqual(CloudManager.GetGraphBaseUri(context), new Uri($"https://graph.microsoft.com"));

                context.Environment = Microsoft365Environment.USGovernment;
                Assert.AreEqual(CloudManager.GetGraphBaseUri(context), new Uri($"https://graph.microsoft.com"));

                context.Environment = Microsoft365Environment.Germany;
                Assert.AreEqual(CloudManager.GetGraphBaseUri(context), new Uri($"https://graph.microsoft.de"));

                context.Environment = Microsoft365Environment.China;
                Assert.AreEqual(CloudManager.GetGraphBaseUri(context), new Uri($"https://microsoftgraph.chinacloudapi.cn"));

                context.Environment = Microsoft365Environment.USGovernmentHigh;
                Assert.AreEqual(CloudManager.GetGraphBaseUri(context), new Uri($"https://graph.microsoft.us"));

                context.Environment = Microsoft365Environment.USGovernmentDoD;
                Assert.AreEqual(CloudManager.GetGraphBaseUri(context), new Uri($"https://dod-graph.microsoft.us"));

                context.Environment = Microsoft365Environment.Custom;
                context.MicrosoftGraphAuthority = "graph.microsoft.be";
                Assert.AreEqual(CloudManager.GetGraphBaseUri(context), new Uri($"https://graph.microsoft.be"));
            }
        }
    }
}
