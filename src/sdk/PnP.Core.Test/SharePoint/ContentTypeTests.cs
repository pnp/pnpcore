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
        public async Task ContentTypesGetByIdTest()
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
            TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                string newContentTypeId = "0x0100302EF0D1F1DB4C4EBF58251BCCF5968F";
                IContentType newContentType = await context.Web.ContentTypes.AddAsync(newContentTypeId, "Foo", "Foo Description", "Foo Group");

                // Test the created object
                Assert.IsNotNull(newContentType);
                Assert.AreEqual(newContentTypeId, newContentType.StringId);
                Assert.AreEqual("Foo", newContentType.Name);
                Assert.AreEqual("Foo Description", newContentType.Description);
                Assert.AreEqual("Foo Group", newContentType.Group);

                // Load the created content type and test its properties
                IContentType loadedContentType = (from ct in context.Web.ContentTypes
                                                  where ct.StringId == newContentTypeId
                                                  select ct)
                            .Load(ct => ct.StringId, ct => ct.Name, ct => ct.Description, ct => ct.Group)
                            .FirstOrDefault();

                Assert.IsNotNull(loadedContentType);
                Assert.AreEqual(newContentTypeId, loadedContentType.StringId);
                Assert.AreEqual("Foo", loadedContentType.Name);
                Assert.AreEqual("Foo Description", loadedContentType.Description);
                Assert.AreEqual("Foo Group", loadedContentType.Group);
            }
        }
    }
}
