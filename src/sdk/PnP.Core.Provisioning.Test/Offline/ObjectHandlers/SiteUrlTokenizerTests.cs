using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using System;

namespace PnP.Core.Provisioning.Test.Offline.ObjectHandlers
{
    /// <summary>
    /// Pins how an extracted data row value is made to point at the site the template is applied to.
    /// </summary>
    [TestClass]
    [TestCategory("Offline")]
    public class SiteUrlTokenizerTests
    {
        private static readonly Guid WebId = new Guid("0f3c1d9e-8f7a-4b2c-9d6e-5a4b3c2d1e0f");
        private static readonly Guid SiteId = new Guid("7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d");

        /// <summary>
        /// A sub site, so the web and its site collection differ.
        /// </summary>
        private static SiteUrlTokenizer SubSite()
        {
            return new SiteUrlTokenizer(
                new Uri("https://contoso.sharepoint.com/sites/team/projects"), "/sites/team/projects", WebId,
                new Uri("https://contoso.sharepoint.com/sites/team"), "/sites/team", SiteId);
        }

        [TestMethod]
        public void AnAbsoluteUrlKeepsPointingAtAHost()
        {
            SiteUrlTokenizer tokenizer = SubSite();

            Assert.AreEqual("{hosturl}{site}/Lists/Tasks/AllItems.aspx",
                tokenizer.Tokenize("https://contoso.sharepoint.com/sites/team/projects/Lists/Tasks/AllItems.aspx"));

            Assert.AreEqual("{hosturl}{sitecollection}/SitePages/Home.aspx,Home",
                tokenizer.Tokenize("https://contoso.sharepoint.com/sites/team/SitePages/Home.aspx,Home"),
                "A url column is rendered as 'url,description'; the comma ends the url.");

            Assert.AreEqual("{hosturl}{site}", tokenizer.Tokenize("HTTPS://CONTOSO.SHAREPOINT.COM/sites/TEAM/projects"),
                "Urls compare without regard to case.");
        }

        [TestMethod]
        public void AServerRelativePathBecomesTheSiteToken()
        {
            SiteUrlTokenizer tokenizer = SubSite();

            Assert.AreEqual("{site}/Shared Documents/Plan.docx", tokenizer.Tokenize("/sites/team/projects/Shared Documents/Plan.docx"));
            Assert.AreEqual("See <a href=\"{sitecollection}/SitePages/Home.aspx\">home</a>",
                tokenizer.Tokenize("See <a href=\"/sites/team/SitePages/Home.aspx\">home</a>"));
        }

        [TestMethod]
        public void OnlyWholePathsAreReplaced()
        {
            SiteUrlTokenizer tokenizer = SubSite();

            Assert.AreEqual("https://contoso.sharepoint.com/sites/teamwork/Lists/Tasks",
                tokenizer.Tokenize("https://contoso.sharepoint.com/sites/teamwork/Lists/Tasks"),
                "A site whose url merely starts with this one's was tokenized.");

            Assert.AreEqual("/sites/teamwork", tokenizer.Tokenize("/sites/teamwork"));

            Assert.AreEqual("https://fabrikam.sharepoint.com/sites/team/SitePages/Home.aspx",
                tokenizer.Tokenize("https://fabrikam.sharepoint.com/sites/team/SitePages/Home.aspx"),
                "The same path on another host is not this site.");
        }

        [TestMethod]
        public void TheIdsBecomeTheirTokens()
        {
            SiteUrlTokenizer tokenizer = SubSite();

            Assert.AreEqual("web {siteid}, site {sitecollectionid}",
                tokenizer.Tokenize($"web {WebId.ToString().ToUpperInvariant()}, site {SiteId}"));
        }

        [TestMethod]
        public void TheRootSiteCollectionsPathIsLeftAlone()
        {
            var tokenizer = new SiteUrlTokenizer(
                new Uri("https://contoso.sharepoint.com/"), "/", WebId,
                new Uri("https://contoso.sharepoint.com/"), "/", SiteId);

            Assert.AreEqual("{hosturl}{site}/SitePages/Home.aspx",
                tokenizer.Tokenize("https://contoso.sharepoint.com/SitePages/Home.aspx"));

            Assert.AreEqual("/SitePages/Home.aspx", tokenizer.Tokenize("/SitePages/Home.aspx"),
                "The root site's path is '/', which is part of every path - it cannot be tokenized.");
        }

        [TestMethod]
        public void ValuesWithoutUrlsAreUntouched()
        {
            SiteUrlTokenizer tokenizer = SubSite();

            Assert.AreEqual("Not started", tokenizer.Tokenize("Not started"));
            Assert.AreEqual(string.Empty, tokenizer.Tokenize(string.Empty));
            Assert.IsNull(tokenizer.Tokenize(null));
        }
    }
}
