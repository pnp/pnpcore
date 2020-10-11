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
    public class UserCustomActionTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetWebUserCustomActionsTest()
        {
            //TestCommon.Instance.Mocking = false;
            // TODO Add Test Custom action

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IWeb web = await context.Web.GetAsync(p => p.UserCustomActions);
                Assert.IsNotNull(web.UserCustomActions);
                Assert.IsTrue(web.UserCustomActions.Count() == 0);
            }

            // TODO Cleanup Test Custom action
        }

        [TestMethod]
        public async Task GetSiteUserCustomActionsTest()
        {
            //TestCommon.Instance.Mocking = false;
            // TODO Add Test Custom action

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                ISite site = await context.Site.GetAsync(p => p.UserCustomActions);
                Assert.IsNotNull(site.UserCustomActions);
                Assert.IsTrue(site.UserCustomActions.Count() == 0);
            }

            // TODO Cleanup Test Custom action
        }

        [TestMethod]
        public async Task AddWebUserCustomActionTest()
        {
            TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var customActionUrl = $"{context.Uri}/_layouts/MyPage.aspx";
                var customActionImageUrl = $"{context.Uri}/SiteAssets/SiteIcon.jpg";
                IUserCustomAction newUserCustomAction = await context.Web.UserCustomActions.AddAsync(new AddUserCustomActionOptions()
                {
                    Location = "ClientSideExtension.ApplicationCustomizer",
                    ClientSideComponentId = new Guid("a54612b1-e5cb-4a43-80ae-3b5fb6ce1e35"),
                    ClientSideComponentProperties = @"{""message"":""Added from Unit Test AddWebUserCustomActionTest""}",
                    Sequence = 100,
                    Name = "UCA_CustomHeader",
                    Title = "Custom Header",
                    RegistrationType = UserCustomActionRegistrationType.None,
                    Url = customActionUrl,
                    ImageUrl = customActionImageUrl,
                    Description = "TESTING",
                    Group = "TESTING"
                });

                // Test the created object
                Assert.IsNotNull(newUserCustomAction);
                Assert.AreEqual("ClientSideExtension.ApplicationCustomizer", newUserCustomAction.Location);
                Assert.AreEqual("Custom Header", newUserCustomAction.Title);
                Assert.AreEqual("UCA_CustomHeader", newUserCustomAction.Name);
                Assert.AreEqual("TESTING", newUserCustomAction.Group);
                Assert.AreEqual("TESTING", newUserCustomAction.Description);
                Assert.AreEqual(customActionUrl, newUserCustomAction.Url);
                Assert.AreEqual(customActionImageUrl, newUserCustomAction.ImageUrl);

                await newUserCustomAction.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ContentTypesUpdateTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType contentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST UPDATE", "TESTING", "TESTING");

                // Test if the content type is created
                Assert.IsNotNull(contentType);

                // Update the content type
                contentType.Name = "UPDATED";
                await contentType.UpdateAsync();

                // Test if the updated content type is still found
                IContentType contentTypeToFind = (from ct in context.Web.ContentTypes
                                                  where ct.Name == "UPDATED"
                                                  select ct).FirstOrDefault();

                Assert.IsNotNull(contentTypeToFind);

                await contentType.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ContentTypesDeleteTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IContentType contentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "TEST DELETE", "TESTING", "TESTING");

                // Test if the content type is created
                Assert.IsNotNull(contentType);

                // Delete the content type again
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
