using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Services;
using PnP.Core.Modernization.Transform;
using System;

namespace PnP.Core.Modernization.Tests.Transform
{
    [TestClass]
    [TestCategory("Offline")]
    public class SharePointOnlineSupportTests
    {
        [DataTestMethod]
        [DataRow("https://portal.example.org/sites/wiki")]
        [DataRow("http://contoso.sharepoint.com/sites/wiki")]
        [DataRow("https://contoso.sharepoint.com.example.org/sites/wiki")]
        [DataRow("https://user:password@contoso.sharepoint.com/sites/wiki")]
        public void RejectsUnsupportedAndMisleadingSiteAddresses(string url)
        {
            Assert.ThrowsException<NotSupportedException>(() => SharePointOnlineSupport.Validate(new Uri(url)));
        }

        [TestMethod]
        public void TransformationRejectsOnPremisesSourceAndDestination()
        {
            using var online = CoreTestContext.Create();
            using var onPremises = CoreTestContext.Create("https://portal.example.org/sites/wiki");
            Assert.ThrowsException<NotSupportedException>(() => new PageTransformator(onPremises, online));
            Assert.ThrowsException<NotSupportedException>(() => new PageTransformator(online, onPremises));
        }

        [DataTestMethod]
        [DataRow("https://contoso.sharepoint.com/sites/legacy-2010")]
        [DataRow("https://contoso.sharepoint.us/sites/wiki")]
        [DataRow("https://contoso.sharepoint-mil.us/sites/wiki")]
        [DataRow("https://contoso.sharepoint.cn/sites/wiki")]
        public void AcceptsOnlineSitesIncludingLegacyLayoutNames(string url)
        {
            SharePointOnlineSupport.Validate(new Uri(url));
        }
    }
}
