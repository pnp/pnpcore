using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Security;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.Security
{
    [TestClass]
    public class PermissionTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            TestCommon.Instance.Mocking = false;            
        }

        [TestMethod]
        public async Task GetShareLinks()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/pnpcoresdktestgroup/shared documents/PNP_SDK_TEST_GetSharingPermissionsAsyncTest.docx");
                var permissions = file.GetShareLinks();

                Assert.IsNotNull(permissions);
            }
        }

        [TestMethod]
        public async Task GetShareInvites()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/pnpcoresdktestgroup/shared documents/PNP_SDK_TEST_GetSharingPermissionsAsyncTest.docx");
                var permissions = file.GetShareInvites();

                Assert.IsNotNull(permissions);
            }
        }


        [TestMethod]
        public async Task DeleteShareLinks()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/pnpcoresdktestgroup/shared documents/PNP_SDK_TEST_GetSharingPermissionsAsyncTest.docx");
                
                file.DeleteShareLinks();

                var permissions = file.GetShareLinks();
                Assert.AreEqual(0, permissions.Count);
            }
        }

        [TestMethod]
        public async Task DeleteShareInvites()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/pnpcoresdktestgroup/shared documents/PNP_SDK_TEST_GetSharingPermissionsAsyncTest.docx");

                file.DeleteShareInvites();

                var permissions = file.GetShareInvites();
                Assert.AreEqual(0, permissions.Count);
            }
        }

        [TestMethod]
        public async Task ShareFileUsingLinkAsync()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/pnpcoresdktestgroup/shared documents/PNP_SDK_TEST_GetSharingPermissionsAsyncTest.docx");

                await file.DeleteShareLinksAsync();

                var originalSharingLinks = await file.GetShareLinksAsync();

                var shareLinkRequestOptions = new ShareLinkRequestOptions()
                { 
                    Scope = ShareScope.Organization,
                    Type = ShareType.Edit
                };
                var permission = await file.CreateSharingLinkAsync(shareLinkRequestOptions);
                
                Assert.IsNotNull(permission.Id);
                
                var newSharingLinks = await file.GetShareLinksAsync();

                Assert.AreEqual(newSharingLinks.Count, originalSharingLinks.Count + 1);

                //await permission.DeletePermissionAsync();
            }
        }

        [TestMethod]
        public async Task ShareFileUsingInvitationAsync()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/pnpcoresdktestgroup/shared documents/PNP_SDK_TEST_GetSharingPermissionsAsyncTest.docx");

                await file.DeleteShareInvitesAsync();

                var shareRequestOptions = new ShareInviteRequestOptions()
                {
                    Message = "I'd like to share this file with you",
                    RequireSignIn = true,
                    SendInvitation = true,
                    Recipients = new List<IDriveRecipient>
                    {
                        new DriveRecipient
                        {
                            Email = "mv@mathijsdev2.onmicrosoft.com"
                        }
                    },
                    Roles = new List<PermissionRole> { PermissionRole.Read }
                };

                var permission = await file.CreateSharingInviteAsync(shareRequestOptions);
                Assert.IsTrue(permission.Roles.Contains(PermissionRole.Read));
                Assert.IsTrue(permission.Invitation.SignInRequired);


                //Using Invitation cannot be immediately deleted as the ID is not being returned by the API call (however the documentation says that it should be)

            }
        }
    }
}
