using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class ContentTypeHubTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            TestCommon.Instance.Mocking = false;            
        }

        [TestMethod]
        public async Task GetContentTypesFromHubAsync()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.ContentTypeHub.LoadAsync(y => y.ContentTypes);

                Assert.IsNotNull(context.ContentTypeHub.ContentTypes);
            }
        }

        [TestMethod]
        public async Task AddContentTypeToHubAsync()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST ADD", "TESTING", "TESTING");

                // Test the created object
                Assert.IsNotNull(newContentType);
                Assert.AreEqual("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", newContentType.StringId);
                Assert.AreEqual("TEST ADD", newContentType.Name);
                Assert.AreEqual("TESTING", newContentType.Description);
                Assert.AreEqual("TESTING", newContentType.Group);

                await newContentType.DeleteAsync();
            }
        }

    }
}
