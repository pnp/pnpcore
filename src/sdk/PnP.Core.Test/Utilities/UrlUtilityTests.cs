using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PnP.Core.Utilities.Tests
{
    [TestClass()]
    public class UrlUtilityTests
    {
        [TestMethod()]
        public void CombineUriPathWithPartsTest()
        {
            string expected = "https://contoso.sharepoint.com/sites/foo";
            Uri actual = UrlUtility.Combine(new Uri("https://contoso.sharepoint.com"), "sites", "foo");
            Assert.AreEqual(expected, actual.ToString());
        }

        [TestMethod()]
        public void CombineWithPartsTest()
        {
            string expected = "https://contoso.sharepoint.com/sites/foo";
            Uri actual = UrlUtility.Combine("https://contoso.sharepoint.com", "sites", "foo");
            Assert.AreEqual(expected, actual.ToString());
        }

        [TestMethod()]
        public void CombineWithRelativePathTest()
        {
            string expected = "https://contoso.sharepoint.com/sites/foo";
            Uri actual = UrlUtility.Combine("https://contoso.sharepoint.com", "sites/foo");
            Assert.AreEqual(expected, actual.ToString());
        }

        [TestMethod()]
        public void CombineWithNullRelativePathTest()
        {
            string expected = "https://contoso.sharepoint.com/";
            Uri actual = UrlUtility.Combine("https://contoso.sharepoint.com", (string)null);
            Assert.AreEqual(expected, actual.ToString());
        }

        [TestMethod()]
        public void CombineWithNullPathTest()
        {
            Assert.ThrowsException<UriFormatException>(() =>
            {
                UrlUtility.Combine((string)null, "sites/foo");
            });
        }

        [TestMethod()]
        public void MakeAbsoluteUrlTest()
        {
            Uri webUrl = new Uri("https://contoso.sharepoint.com/sites/foo");
            string relativeUrl = "/sites/foo/library/doc.docx";
            string expected = "https://contoso.sharepoint.com/sites/foo/library/doc.docx";
            Uri actual = UrlUtility.MakeAbsoluteUrl(webUrl, relativeUrl);
            Assert.AreEqual(expected, actual.ToString());
        }

        [TestMethod()]
        public void MakeAbsoluteUrlWithNullWebUrlTest()
        {
            Uri webUrl = null;
            string relativeUrl = "/sites/foo/library/doc.docx";
            string expected = null;
            Uri actual = UrlUtility.MakeAbsoluteUrl(webUrl, relativeUrl);
            Assert.AreEqual(expected, actual?.ToString());
        }

        [TestMethod()]
        public void EnsureAbsoluteUrlWithRelativeResourceUrlTest()
        {
            Uri webUrl = new Uri("https://contoso.sharepoint.com/sites/foo");
            string resourceUrl = "/sites/foo/library/doc.docx";
            string expected = "https://contoso.sharepoint.com/sites/foo/library/doc.docx";
            Uri actual = UrlUtility.EnsureAbsoluteUrl(webUrl, resourceUrl);
            Assert.AreEqual(expected, actual?.ToString());
        }

        [TestMethod()]
        public void EnsureAbsoluteUrlWithValidAbsoluteResourceUrlTest()
        {
            Uri webUrl = new Uri("https://contoso.sharepoint.com/sites/foo");
            string resourceUrl = "https://contoso.sharepoint.com/sites/foo/library/doc.docx";
            string expected = "https://contoso.sharepoint.com/sites/foo/library/doc.docx";
            Uri actual = UrlUtility.EnsureAbsoluteUrl(webUrl, resourceUrl);
            Assert.AreEqual(expected, actual?.ToString());
        }

        [TestMethod()]
        public void EnsureAbsoluteUrlWithInvalidAbsoluteResourceUrlTest()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                Uri webUrl = new Uri("https://contoso.sharepoint.com/sites/foo");
                string resourceUrl = "https://contoso.sharepoint.com/sites/bar/library/doc.docx";
                UrlUtility.EnsureAbsoluteUrl(webUrl, resourceUrl, true);
            });
        }

        [TestMethod()]
        public void EnsureAbsoluteUrlWithNoCrossSiteCheckUrlTest()
        {
            Uri webUrl = new Uri("https://contoso.sharepoint.com/sites/foo");
            string resourceUrl = "https://contoso.sharepoint.com/sites/bar/library/doc.docx";
            string expected = "https://contoso.sharepoint.com/sites/bar/library/doc.docx";
            Uri actual = UrlUtility.EnsureAbsoluteUrl(webUrl, resourceUrl);
            Assert.AreEqual(expected, actual?.ToString());
        }

        [TestMethod()]
        public void IsSameSiteRelativeUrlTest()
        {
            Uri webUrl = new Uri("https://contoso.sharepoint.com/sites/foo");
            string resourceUrl = "/sites/foo/library/doc.docx";
            bool actual = UrlUtility.IsSameSite(webUrl, resourceUrl);
            Assert.IsTrue(actual);

            webUrl = new Uri("https://contoso.sharepoint.com/sites/foo");
            resourceUrl = "/sites/bar/library/doc.docx";
            actual = UrlUtility.IsSameSite(webUrl, resourceUrl);
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void IsSameSiteAbsoluteUrlTest()
        {
            Uri webUrl = new Uri("https://contoso.sharepoint.com/sites/foo");
            string resourceUrl = "https://contoso.sharepoint.com/sites/foo/library/doc.docx";
            bool actual = UrlUtility.IsSameSite(webUrl, resourceUrl);
            Assert.IsTrue(actual);

            webUrl = new Uri("https://contoso.sharepoint.com/sites/foo");
            resourceUrl = "https://contoso.sharepoint.com/sites/bar/library/doc.docx";
            actual = UrlUtility.IsSameSite(webUrl, resourceUrl);
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void EnsureTrailingSlashUrlTest()
        {
            string url = "https://foo";
            string actual = UrlUtility.EnsureTrailingSlash(url);
            Assert.IsTrue(actual.EndsWith("/"));
        }

        [TestMethod()]
        public void EnsureTrailingSlashUriTest()
        {
            Uri actual = new Uri("https://foo").EnsureTrailingSlash();
            Assert.IsTrue(actual.ToString().EndsWith("/"));
        }

        [TestMethod()]
        public void MergeUrlParametersTest()
        {
            var mergedUrl = UrlUtility.CombineRelativeUrlWithUrlParameters("teams/{Site.GroupId}/channels", "");
            Assert.IsTrue(mergedUrl.Equals("teams/{Site.GroupId}/channels", StringComparison.InvariantCultureIgnoreCase));

            mergedUrl = UrlUtility.CombineRelativeUrlWithUrlParameters("teams/{Site.GroupId}/channels", "$select=displayName,id");
            Assert.IsTrue(mergedUrl.Equals("teams/{Site.GroupId}/channels?$select=displayName,id", StringComparison.InvariantCultureIgnoreCase));

            mergedUrl = UrlUtility.CombineRelativeUrlWithUrlParameters("teams/{Site.GroupId}/channels?$select=displayName,id", "");
            Assert.IsTrue(mergedUrl.Equals("teams/{Site.GroupId}/channels?$select=displayName,id", StringComparison.InvariantCultureIgnoreCase));

            mergedUrl = UrlUtility.CombineRelativeUrlWithUrlParameters("teams/{Site.GroupId}/installedapps?$select=bert&$expand=TeamsApp", "$select=displayName,id");
            Assert.IsTrue(mergedUrl.Equals("teams/{Site.GroupId}/installedapps?$select=bert,displayName,id&$expand=TeamsApp", StringComparison.InvariantCultureIgnoreCase));

            mergedUrl = UrlUtility.CombineRelativeUrlWithUrlParameters("teams/{Site.GroupId}/installedapps?$expand=TeamsApp", "$select=displayName,id");
            Assert.IsTrue(mergedUrl.Equals("teams/{Site.GroupId}/installedapps?$expand=TeamsApp&$select=displayname,id", StringComparison.InvariantCultureIgnoreCase));

            // Case sensitive test on list name!
            mergedUrl = UrlUtility.CombineRelativeUrlWithUrlParameters("https://bertonline.sharepoint.com/sites/prov-1/_api/web/lists", "$select=Title,ListExperienceOptions&$filter=Title%20eq%20%27Documents%27&$top=1");
            Assert.IsTrue(mergedUrl.Equals("bertonline.sharepoint.com/sites/prov-1/_api/web/lists?$select=Title,ListExperienceOptions&$filter=Title+eq+%27Documents%27&$top=1"));
        }

    }
}