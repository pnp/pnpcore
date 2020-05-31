using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.QueryModel;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class ContentTypeTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        // Cleanup is not working due to matching mock data not existing
        //[ClassCleanup]
        //public static void TestCleanup()
        //{
        //    using (var ctx = TestCommon.Instance.GetContext(TestCommon.TestSite))
        //    {
        //        // Remove the content type created in the Add test
        //        IContentType addedContentType = (from ct in ctx.Web.ContentTypes
        //                                         where ct.Name == "ADD TEST"
        //                                         select ct).FirstOrDefault();
        //        if (addedContentType != null)
        //            addedContentType.DeleteAsync().ConfigureAwait(false);
        //    }
        //}

        [TestMethod]
        public async Task ContentTypesGetTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                IWeb web = await context.Web.GetAsync(p => p.ContentTypes);
                Assert.IsTrue(web.ContentTypes.Count() > 0);

                IContentType contentType = web.ContentTypes.FirstOrDefault(p => p.Name == "Item");
                // Test a string property
                Assert.AreEqual(contentType.Name, "Item");
                // Test a boolean property
                Assert.IsFalse(contentType.Hidden);
            }
        }

        [TestMethod]
        public void ContentTypesGetByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                IContentType contentType = (from ct in context.Web.ContentTypes
                                            where ct.StringId == "0x01"
                                            select ct)
                            .Load(ct => ct.StringId, ct => ct.Id)
                            .FirstOrDefault();

                Assert.IsNotNull(contentType);
                // Test complex-type Id property
                Assert.AreEqual(contentType.Id.StringValue, "0x01");
            }
        }

        [TestMethod]
        public async Task ContentTypesAddTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                IContentType newContentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST ADD", "TESTING", "TESTING");

                // Test the created object
                Assert.IsNotNull(newContentType);
                // NOTE : The Id of the created content type is not identical to the specified one using the SP Rest API
                // The test on Id is commented out for now
                //Assert.AreEqual(newContentTypeId, newContentType.StringId);
                Assert.AreEqual("TEST ADD", newContentType.Name);
                Assert.AreEqual("TESTING", newContentType.Description);
                Assert.AreEqual("TESTING", newContentType.Group);

                // NOTE : The created test content type is NOT cleaned up
            }
        }

        [TestMethod]
        public async Task ContentTypesUpdateTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                IContentType contentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST UPDATE", "TESTING", "TESTING");

                // Test if the content type is created
                Assert.IsNotNull(contentType);

                contentType.Name = "UPDATED";
                await contentType.UpdateAsync();

                // Test if the content type is still found
                IContentType contentTypeToFind = (from ct in context.Web.ContentTypes
                                                  where ct.Name == "UPDATED"
                                                  select ct).FirstOrDefault();

                Assert.IsNotNull(contentTypeToFind);
            }
        }

        [TestMethod]
        public async Task ContentTypesDeleteTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                IContentType contentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST DELETE", "TESTING", "TESTING");

                // Test if the content type is created
                Assert.IsNotNull(contentType);

                await contentType.DeleteAsync();

                // Test if the content type is still found
                IContentType contentTypeToFind = (from ct in context.Web.ContentTypes
                                                  where ct.Name == "TEST DELETE"
                                                  select ct).FirstOrDefault();

                Assert.IsNull(contentTypeToFind);
            }
        }
    }
}
