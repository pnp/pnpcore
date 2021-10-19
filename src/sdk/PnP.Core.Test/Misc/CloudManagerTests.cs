using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services;
using System;

namespace PnP.Core.Test.Misc
{
    [TestClass()]
    public class CloudManagerTests
    {
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
        [DataRow("login.microsoftonline.us", Microsoft365Environment.USGovernment)]
        [DataRow("login.microsoftonline.de", Microsoft365Environment.Germany)]
        [DataRow("login.chinacloudapi.cn", Microsoft365Environment.China)]
        [DataRow("login.microsoftonline.us", Microsoft365Environment.USGovernmentHigh)]
        [DataRow("login.microsoftonline.us", Microsoft365Environment.USGovernmentDoD)]
        public void Microsoft365EnvironmentToAzureADLogin(string azureADLogin, Microsoft365Environment env)
        {
            Assert.AreEqual(azureADLogin, CloudManager.GetAzureADLoginAuthority(env));
        }        
    }
}
