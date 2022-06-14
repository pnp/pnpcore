using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task GetContentTypesFromHubAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.ContentTypeHub.LoadAsync(y => y.ContentTypes);

                Assert.IsNotNull(context.ContentTypeHub.ContentTypes);
            }
        }

        [TestMethod]
        public async Task AddContentTypeToHubAsyncTest()
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

                await context.ContentTypeHub.LoadAsync(y => y.ContentTypes);

                var matchingCt = context.ContentTypeHub.ContentTypes.AsRequested().FirstOrDefault(y => y.Id == newContentType.StringId);

                Assert.IsNotNull(matchingCt);

                await newContentType.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PublishContentTypeAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968B", "TEST ADD", "TESTING", "TESTING");

                var isPublished = await newContentType.IsPublishedAsync();

                Assert.IsFalse(isPublished);

                await newContentType.PublishAsync();

                isPublished = await newContentType.IsPublishedAsync();

                Assert.IsTrue(isPublished);

                await newContentType.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UnpublishContentTypeAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968D", "TEST ADD Unpublish", "TESTING", "TESTING");

                // First we have to publish the content type

                await newContentType.PublishAsync();

                var isPublished = await newContentType.IsPublishedAsync();

                Assert.IsTrue(isPublished);

                await newContentType.UnpublishAsync();

                isPublished = await newContentType.IsPublishedAsync();

                Assert.IsFalse(isPublished);

                await newContentType.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CheckIfContentTypeIsPublishedAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968C", "TEST ADD IsPublished", "TESTING", "TESTING");

                var isPublished = await newContentType.IsPublishedAsync();

                Assert.IsFalse(isPublished);

                await newContentType.PublishAsync();

                isPublished = await newContentType.IsPublishedAsync();

                Assert.IsTrue(isPublished);

                await newContentType.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UnpublishContentTypeAsyncExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968D", "TEST ADD Unpublish", "TESTING", "TESTING");

                await Assert.ThrowsExceptionAsync<Exception>(async () =>
                {
                    await newContentType.UnpublishAsync();
                });

                await newContentType.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PublishContentTypeAsyncTargetExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType newContentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968D", "TEST ADD Publish", "TESTING", "TESTING");

                await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                {
                    await newContentType.PublishAsync();
                });

                await newContentType.DeleteAsync();
            }
        }
    }
}
