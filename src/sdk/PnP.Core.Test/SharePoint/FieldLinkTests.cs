using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class FieldLinkTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetSiteContentTypeFieldLinksTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Get existing content type
                IContentType contentType = (from ct in context.Web.ContentTypes
                                            where ct.Name == "Document"
                                            select ct)
                                            .Load(ct => ct.FieldLinks)
                                            .FirstOrDefault();

                Assert.IsTrue(contentType.FieldLinks.Count() > 0);
                Assert.IsNotNull(contentType);

                IFieldLink titleFieldLink = contentType.FieldLinks.FirstOrDefault(fl => fl.Name == "Title");

                Assert.IsNotNull(titleFieldLink);
                Assert.AreEqual("Title", titleFieldLink.Name);
                Assert.AreEqual(new Guid("fa564e0f-0c70-4ab9-b863-0177e6ddd247"), titleFieldLink.Id);
            }
        }

        [TestMethod]
        public async Task GetListContentTypeFieldLinksTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Get existing content type
                IContentType contentType = (from ct in context.Web.Lists.GetByTitle("Documents").ContentTypes
                                            where ct.Name == "Document"
                                            select ct)
                                            .Load(ct => ct.FieldLinks)
                                            .FirstOrDefault();

                Assert.IsTrue(contentType.FieldLinks.Count() > 0);
                Assert.IsNotNull(contentType);

                IFieldLink titleFieldLink = contentType.FieldLinks.FirstOrDefault(fl => fl.Name == "Title");

                Assert.IsNotNull(titleFieldLink);
                Assert.AreEqual("Title", titleFieldLink.Name);
                Assert.AreEqual(new Guid("fa564e0f-0c70-4ab9-b863-0177e6ddd247"), titleFieldLink.Id);
            }
        }

        [TestMethod]
        public async Task AddSiteContentTypeFieldLinkTest()
        {
            // The FieldLinkCollection Add REST endpoint has many limitations
            // https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-visio/jj245869%28v%3doffice.15%29#rest-resource-endpoint
            // It ONLY works with Site Columns already present in the parent content content type or  with list columns if they are already added to the list
            // TODO: Probably worthy to recommend not using this endpoint for adding fieldlinks... What alternative ?

            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Create a new test content type
                IContentType newContentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST ADD", "TESTING", "TESTING");
                Assert.IsNotNull(newContentType);

                // Add a new field link between newContentType and Title
                IFieldLink newFieldLink = await newContentType.FieldLinks.AddAsync("Title", "My Title");

                Assert.IsNotNull(newFieldLink);
                Assert.AreEqual("Title", newFieldLink.Name);
                Assert.AreEqual("My Title", newFieldLink.DisplayName);
                Assert.AreEqual(new Guid("fa564e0f-0c70-4ab9-b863-0177e6ddd247"), newFieldLink.Id);

                await newContentType.DeleteAsync();
            }
        }

        // TODO Handle List Content Types as the FieldLinkParent => A review in the core should be done
        //[TestMethod]
        //public async Task AddListContentTypeFieldLinkTest()
        //{
        //    // The FieldLinkCollection Add REST endpoint has many limitations
        //    // https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-visio/jj245869%28v%3doffice.15%29#rest-resource-endpoint
        //    // It ONLY works with Site Columns already present in the parent content content type or  with list columns if they are already added to the list
        //    // TODO: Probably worthy to recommend not using this endpoint for adding fieldlinks... What alternative ?

        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
        //    {
        //        // Create a new test content type
        //        IContentType newSiteContentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST ADD", "TESTING", "TESTING");
        //        IContentType newListContentType = await context.Web.Lists.GetByTitle("Documents").ContentTypes.AddAvailableContentTypeAsync(newSiteContentType.Id);
        //        Assert.IsNotNull(newListContentType);

        //        // Add a new field link between newContentType and Title
        //        IFieldLink newFieldLink = await newListContentType.FieldLinks.AddAsync("Title", "My Title");

        //        Assert.IsNotNull(newFieldLink);
        //        Assert.AreEqual("Title", newFieldLink.Name);
        //        Assert.AreEqual("My Title", newFieldLink.DisplayName);
        //        Assert.AreEqual(new Guid("fa564e0f-0c70-4ab9-b863-0177e6ddd247"), newFieldLink.Id);

        //        await newListContentType.DeleteAsync();
        //        await newSiteContentType.DeleteAsync();
        //    }
        //}

        // NOTE Delete and Update are no supported from regular REST endpoint
        //{"error":{"code":"-1, Microsoft.SharePoint.Client.InvalidClientQueryException","message":{"lang":"en-US","value":"The type SP.FieldLinkCollection does not support HTTP DELETE method."}}}
        //{"error":{"code":"-1, Microsoft.SharePoint.Client.InvalidClientQueryException","message":{"lang":"en-US","value":"The type SP.FieldLinkCollection does not support HTTP PATCH method."}}}
    }
}
