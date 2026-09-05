using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.ObjectHandlers;

namespace PnP.Core.Provisioning.Test.Offline.ObjectHandlers
{
    /// <summary>
    /// Pins the web-url normalisation every url tokenizer depends on.
    /// </summary>
    [TestClass]
    [TestCategory("Offline")]
    public class ServerRelativePathTests
    {
        [TestMethod]
        public void AnAbsoluteUrlYieldsItsPath()
        {
            Assert.AreEqual("/sites/marketing",
                ObjectHandlerBase.ServerRelativePathOf("https://contoso.sharepoint.com/sites/marketing"));
        }

        [TestMethod]
        public void AServerRelativePathIsAlreadyWhatIsWanted()
        {
            Assert.AreEqual("/sites/marketing",
                ObjectHandlerBase.ServerRelativePathOf("/sites/marketing"));
        }

        [TestMethod]
        public void TheRootSiteIsASingleSlashEitherWay()
        {
            Assert.AreEqual("/", ObjectHandlerBase.ServerRelativePathOf("https://contoso.sharepoint.com"));
            Assert.AreEqual("/", ObjectHandlerBase.ServerRelativePathOf("https://contoso.sharepoint.com/"));
            Assert.AreEqual("/", ObjectHandlerBase.ServerRelativePathOf("/"));
        }

        [TestMethod]
        public void AnEncodedPathIsDecoded()
        {
            Assert.AreEqual("/sites/my site",
                ObjectHandlerBase.ServerRelativePathOf("https://contoso.sharepoint.com/sites/my%20site"));

            Assert.AreEqual("/sites/my site",
                ObjectHandlerBase.ServerRelativePathOf("/sites/my%20site"));
        }

        [TestMethod]
        public void SomethingThatIsNeitherIsRefused()
        {
            Assert.IsNull(ObjectHandlerBase.ServerRelativePathOf("sites/marketing"));
            Assert.IsNull(ObjectHandlerBase.ServerRelativePathOf("marketing"));
        }

        [TestMethod]
        public void NothingIsRefused()
        {
            Assert.IsNull(ObjectHandlerBase.ServerRelativePathOf(null));
            Assert.IsNull(ObjectHandlerBase.ServerRelativePathOf(string.Empty));
            Assert.IsNull(ObjectHandlerBase.ServerRelativePathOf("   "));
        }

        [TestMethod]
        public void ASubSitePathIsKeptWhole()
        {
            Assert.AreEqual("/sites/marketing/team",
                ObjectHandlerBase.ServerRelativePathOf("https://contoso.sharepoint.com/sites/marketing/team"));

            Assert.AreEqual("/sites/marketing/team",
                ObjectHandlerBase.ServerRelativePathOf("/sites/marketing/team"));
        }
    }
}
