using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Security;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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
            //TestCommon.Instance.Mocking = false;            
        }

        [TestMethod]
        public async Task GetShareLinksTest()
        {
            //TestCommon.Instance.Mocking = false;
            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var originalSharingLinks = await file.GetShareLinksAsync();

                var shareLinkRequestOptions = new OrganizationalLinkOptions()
                {
                    Type = ShareType.Edit
                };
                
                file.CreateOrganizationalSharingLink(shareLinkRequestOptions);

                var permissions = file.GetShareLinks();

                Assert.IsNotNull(permissions);
            }
            
            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task DeleteShareLinksTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var originalSharingLinks = await file.GetShareLinksAsync();

                var shareLinkRequestOptions = new OrganizationalLinkOptions()
                {
                    Type = ShareType.Edit
                };

                file.CreateOrganizationalSharingLink(shareLinkRequestOptions);

                file.DeleteShareLinks();

                var permissions = file.GetShareLinks();
                Assert.AreEqual(0, permissions.Count());
            }
            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task ShareFileUsingLinkOrganizationTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var originalSharingLinks = await file.GetShareLinksAsync();

                var shareLinkRequestOptions = new OrganizationalLinkOptions()
                { 
                    Type = ShareType.Edit
                };
                var permission = file.CreateOrganizationalSharingLink(shareLinkRequestOptions);
                
                Assert.IsNotNull(permission.Id);
                Assert.AreEqual(permission.Link.Scope, ShareScope.Organization);

                var newSharingLinks = await file.GetShareLinksAsync();

                Assert.AreEqual(newSharingLinks.Count(), originalSharingLinks.Count() + 1);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task ShareFileUsingLinkAnonymousWithEditPermissionsAndPasswordTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var originalSharingLinks = await file.GetShareLinksAsync();

                var shareLinkRequestOptions = new AnonymousLinkOptions()
                {
                    Type = ShareType.Edit,
                    Password = "PnP Rocks!"
                };
                var permission = file.CreateAnonymousSharingLink(shareLinkRequestOptions);

                Assert.IsNotNull(permission.Id);
                Assert.IsNotNull(permission.Link.WebUrl);

                Assert.AreEqual(permission.Link.Type, ShareType.Edit);
                Assert.AreEqual(permission.Link.Scope, ShareScope.Anonymous);
                Assert.AreEqual(permission.Link.PreventsDownload, false);
                Assert.AreEqual(permission.HasPassword, true);

                var newSharingLinks = await file.GetShareLinksAsync();

                Assert.AreEqual(newSharingLinks.Count(), originalSharingLinks.Count() + 1);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task ShareFileUsingLinkAnonymousWithReadPermissionsAndPreventingDownloadTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var originalSharingLinks = await file.GetShareLinksAsync();

                var shareLinkRequestOptions = new AnonymousLinkOptions()
                {
                    Type = ShareType.BlocksDownload
                };

                var permission = file.CreateAnonymousSharingLink(shareLinkRequestOptions);

                Assert.IsNotNull(permission.Id);
                Assert.IsNotNull(permission.Link.WebUrl);
                Assert.AreEqual(permission.Link.Type, ShareType.View);
                Assert.AreEqual(permission.Link.Scope, ShareScope.Anonymous);
                Assert.AreEqual(permission.HasPassword, false);
                Assert.AreEqual(permission.Link.PreventsDownload, true);

                var newSharingLinks = await file.GetShareLinksAsync();

                Assert.AreEqual(newSharingLinks.Count(), originalSharingLinks.Count() + 1);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task ShareFileUsingLinkAnonymousWithReadPermissionsAndAllowingDownloadIncludingPasswordTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var originalSharingLinks = await file.GetShareLinksAsync();

                // Schedule the page publishing
                DateTime expirationDate = DateTime.MinValue;
                if (!TestCommon.Instance.Mocking)
                {
                    expirationDate = DateTime.Now.AddDays(5);
                    Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "Ticks", expirationDate.Ticks.ToString() },
                        };
                    TestManager.SaveProperties(context, properties);
                }
                else
                {
                    var properties = TestManager.GetProperties(context);
                    expirationDate = new DateTime(long.Parse(properties["Ticks"]));
                }

                var shareLinkRequestOptions = new AnonymousLinkOptions()
                {
                    Type = ShareType.View,
                    Password = "PnP Rocks!",
                    ExpirationDateTime = expirationDate
                };                

                var permission = file.CreateAnonymousSharingLink(shareLinkRequestOptions);

                Assert.IsNotNull(permission.Id);
                Assert.IsNotNull(permission.Link.WebUrl);

                Assert.AreEqual(permission.Link.Type, ShareType.View);
                Assert.AreEqual(permission.Link.Scope, ShareScope.Anonymous);
                Assert.AreEqual(permission.Link.PreventsDownload, false);
                Assert.AreEqual(permission.HasPassword, true);
                Assert.AreEqual(permission.ExpirationDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"), shareLinkRequestOptions.ExpirationDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                var newSharingLinks = await file.GetShareLinksAsync();

                Assert.AreEqual(newSharingLinks.Count(), originalSharingLinks.Count() + 1);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task ShareFileUsingLinkAnonymousWithEditPermissionsAndReviewCapabilitiesTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var originalSharingLinks = await file.GetShareLinksAsync();

                var shareLinkRequestOptions = new AnonymousLinkOptions()
                {
                    Type = ShareType.Review
                };

                var permission = file.CreateAnonymousSharingLink(shareLinkRequestOptions);

                Assert.IsNotNull(permission.Id);
                Assert.IsNotNull(permission.Link.WebUrl);
                
                Assert.AreEqual(permission.Link.Type, ShareType.Review);
                Assert.AreEqual(permission.Link.Scope, ShareScope.Anonymous);
                Assert.AreEqual(permission.HasPassword, false);
                Assert.AreEqual(permission.Link.PreventsDownload, false);

                var newSharingLinks = await file.GetShareLinksAsync();

                Assert.AreEqual(newSharingLinks.Count(), originalSharingLinks.Count() + 1);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task ShareFileUsingLinkUsersReadPermissionsDownloadingBlockedTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var originalSharingLinks = await file.GetShareLinksAsync();

                var testUser = context.Web.SiteUsers.FirstOrDefault(p => p.PrincipalType == PrincipalType.User);

                var driveRecipients = new List<IDriveRecipient>()
                {
                    new DriveRecipient
                    {
                        Email = testUser.Mail
                    }
                };

                var shareLinkRequestOptions = new UserLinkOptions()
                {
                    Type = ShareType.BlocksDownload,
                    Recipients = driveRecipients
                };

                var permission = file.CreateUserSharingLink(shareLinkRequestOptions);

                Assert.IsNotNull(permission.Id);
                Assert.IsNotNull(permission.Link.WebUrl);

                Assert.AreEqual(permission.Link.Type, ShareType.View);
                Assert.AreEqual(permission.Link.Scope, ShareScope.Users);
                Assert.AreEqual(permission.Link.PreventsDownload, true);
                Assert.AreEqual(permission.HasPassword, false);
                Assert.AreEqual(permission.GrantedToIdentitiesV2.FirstOrDefault().SiteUser.Email, shareLinkRequestOptions.Recipients.First().Email);
                var newSharingLinks = await file.GetShareLinksAsync();

                Assert.AreEqual(newSharingLinks.Count(), originalSharingLinks.Count() + 1);
            }

            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task ShareFileUsingInvitationTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {

                var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                var testUser = context.Web.SiteUsers.FirstOrDefault(p => p.PrincipalType == PrincipalType.User);

                var driveRecipients = new List<IDriveRecipient>()
                {
                    new DriveRecipient
                    {
                        Email = testUser.Mail
                    }
                };

                var shareRequestOptions = new InviteOptions()
                {
                    Message = "I'd like to share this file with you",
                    RequireSignIn = true,
                    SendInvitation = true,
                    Recipients = driveRecipients,
                    Roles = new List<PermissionRole> { PermissionRole.Read }
                };

                var permission = file.CreateSharingInvite(shareRequestOptions);
                Assert.IsNotNull(permission.Id);
                Assert.IsTrue(permission.Roles.Contains(PermissionRole.Read));
            }
            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task ShareFileAnonymousLinkCreateOnlyExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                var shareLinkRequestOptions = new AnonymousLinkOptions()
                {
                    Type = ShareType.CreateOnly
                };

                await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                {
                    await file.CreateAnonymousSharingLinkAsync(shareLinkRequestOptions);
                });
            }
            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task ShareFileUserLinkCreateOnlyExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                var shareLinkRequestOptions = new UserLinkOptions()
                {
                    Type = ShareType.CreateOnly
                };

                await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                {
                    await file.CreateUserSharingLinkAsync(shareLinkRequestOptions);
                });
            }
            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task ShareFileUserLinkNoRecipientsExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                var shareLinkRequestOptions = new UserLinkOptions()
                {
                    Type = ShareType.View
                };
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                {
                    await file.CreateUserSharingLinkAsync(shareLinkRequestOptions);
                });
                   
            }
            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task ShareFileOrganizationalLinkCreateOnlyExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;

            (_, _, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                var shareLinkRequestOptions = new OrganizationalLinkOptions()
                {
                    Type = ShareType.CreateOnly
                };

                await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                {
                    await file.CreateOrganizationalSharingLinkAsync(shareLinkRequestOptions);
                });
            }
            await TestAssets.CleanupTestDocumentAsync(2);
        }

        [TestMethod]
        public async Task GetFolderShareLinksTest()
        {
            TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(y => y.RootFolder);
                
                var folder = await list.RootFolder.AddFolderAsync("GetFolderShareLinksTest");

                var originalSharingLinks = await folder.GetShareLinksAsync();

                Assert.IsNotNull(originalSharingLinks);

                await folder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task DeleteFolderShareLinksTest()
        {
            TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(y => y.RootFolder);

                var folder = await list.RootFolder.AddFolderAsync("DeleteFolderShareLinksTest");

                var originalSharingLinks = await folder.GetShareLinksAsync();

                var shareLinkRequestOptions = new OrganizationalLinkOptions()
                {
                    Type = ShareType.Edit
                };

                folder.CreateOrganizationalSharingLink(shareLinkRequestOptions);

                folder.DeleteShareLinks();

                var permissions = folder.GetShareLinks();
                Assert.AreEqual(0, permissions.Count());

                await folder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ShareFolderUsingLinkOrganizationTest()
        {
            TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(y => y.RootFolder);

                var folder = await list.RootFolder.AddFolderAsync("ShareFolderUsingLinkOrganizationTest");

                var originalSharingLinks = await folder.GetShareLinksAsync();

                var shareLinkRequestOptions = new OrganizationalLinkOptions()
                {
                    Type = ShareType.Edit
                };
                var permission = folder.CreateOrganizationalSharingLink(shareLinkRequestOptions);

                Assert.IsNotNull(permission.Id);
                Assert.AreEqual(permission.Link.Scope, ShareScope.Organization);

                var newSharingLinks = await folder.GetShareLinksAsync();

                Assert.AreEqual(newSharingLinks.Count(), originalSharingLinks.Count() + 1);

                await folder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ShareFolderUsingLinkAnonymousWithEditPermissionsAndPasswordTest()
        {
            TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(y => y.RootFolder);

                var folder = await list.RootFolder.AddFolderAsync("ShareFolderUsingLinkAnonymousWithEditPermissionsAndPasswordTest");

                var originalSharingLinks = await folder.GetShareLinksAsync();

                var shareLinkRequestOptions = new AnonymousLinkOptions()
                {
                    Type = ShareType.Edit,
                    Password = "PnP Rocks!"
                };
                var permission = folder.CreateAnonymousSharingLink(shareLinkRequestOptions);

                Assert.IsNotNull(permission.Id);
                Assert.IsNotNull(permission.Link.WebUrl);

                Assert.AreEqual(permission.Link.Type, ShareType.Edit);
                Assert.AreEqual(permission.Link.Scope, ShareScope.Anonymous);
                Assert.AreEqual(permission.Link.PreventsDownload, false);
                Assert.AreEqual(permission.HasPassword, true);

                var newSharingLinks = await folder.GetShareLinksAsync();

                Assert.AreEqual(newSharingLinks.Count(), originalSharingLinks.Count() + 1);

                await folder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ShareFolderUsingLinkAnonymousWithReadPermissionsAndPreventingDownloadExceptionTest()
        {
            TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(y => y.RootFolder);

                var folder = await list.RootFolder.AddFolderAsync("ShareFolderUsingLinkAnonymousWithReadPermissionsAndPreventingDownloadExceptionTest");

                var originalSharingLinks = await folder.GetShareLinksAsync();

                var shareLinkRequestOptions = new AnonymousLinkOptions()
                {
                    Type = ShareType.BlocksDownload
                };

                await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                {
                    await folder.CreateAnonymousSharingLinkAsync(shareLinkRequestOptions);
                });

                await folder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ShareFolderUsingLinkAnonymousWithReadPermissionsAndAllowingDownloadIncludingPasswordTest()
        {
            TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(y => y.RootFolder);

                var folder = await list.RootFolder.AddFolderAsync("ShareFolderUsingLinkAnonymousWithReadPermissionsAndAllowingDownloadIncludingPasswordTest");

                var originalSharingLinks = await folder.GetShareLinksAsync();

                // Schedule the page publishing
                DateTime expirationDate = DateTime.MinValue;
                if (!TestCommon.Instance.Mocking)
                {
                    expirationDate = DateTime.Now.AddDays(5);
                    Dictionary<string, string> properties = new Dictionary<string, string>
                    {
                        { "Ticks", expirationDate.Ticks.ToString() },
                    };
                    TestManager.SaveProperties(context, properties);
                }
                else
                {
                    var properties = TestManager.GetProperties(context);
                    expirationDate = new DateTime(long.Parse(properties["Ticks"]));
                }

                var shareLinkRequestOptions = new AnonymousLinkOptions()
                {
                    Type = ShareType.View,
                    Password = "PnP Rocks!",
                    ExpirationDateTime = expirationDate
                };

                var permission = folder.CreateAnonymousSharingLink(shareLinkRequestOptions);

                Assert.IsNotNull(permission.Id);
                Assert.IsNotNull(permission.Link.WebUrl);

                Assert.AreEqual(permission.Link.Type, ShareType.View);
                Assert.AreEqual(permission.Link.Scope, ShareScope.Anonymous);
                Assert.AreEqual(permission.Link.PreventsDownload, false);
                Assert.AreEqual(permission.HasPassword, true);
                Assert.AreEqual(permission.ExpirationDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"), shareLinkRequestOptions.ExpirationDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                var newSharingLinks = await folder.GetShareLinksAsync();

                Assert.AreEqual(newSharingLinks.Count(), originalSharingLinks.Count() + 1);

                await folder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ShareFolderUsingLinkAnonymousWithEditPermissionsAndReviewCapabilitiesExceptionTest()
        {
            TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(y => y.RootFolder);

                var folder = await list.RootFolder.AddFolderAsync("ShareFolderUsingLinkAnonymousWithEditPermissionsAndReviewCapabilitiesExceptionTest");

                var originalSharingLinks = await folder.GetShareLinksAsync();

                var shareLinkRequestOptions = new AnonymousLinkOptions()
                {
                    Type = ShareType.Review
                };

                await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                {
                    await folder.CreateAnonymousSharingLinkAsync(shareLinkRequestOptions);
                });

                await folder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ShareFolderUsingLinkUsersReadPermissionsDownloadingBlockedTest()
        {
            TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(y => y.RootFolder);

                var folder = await list.RootFolder.AddFolderAsync("ShareFolderUsingLinkUsersReadPermissionsDownloadingBlockedTest");

                var originalSharingLinks = await folder.GetShareLinksAsync();

                var testUser = context.Web.SiteUsers.FirstOrDefault(p => p.PrincipalType == PrincipalType.User);

                var driveRecipients = new List<IDriveRecipient>()
                {
                    new DriveRecipient
                    {
                        Email = testUser.Mail
                    }
                };

                var shareLinkRequestOptions = new UserLinkOptions()
                {
                    Type = ShareType.BlocksDownload,
                    Recipients = driveRecipients
                };

                await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                {
                    await folder.CreateUserSharingLinkAsync(shareLinkRequestOptions);
                });

                await folder.DeleteAsync();
            }

        }

        [TestMethod]
        public async Task ShareFolderUsingInvitationTest()
        {
            TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(y => y.RootFolder);

                var folder = await list.RootFolder.AddFolderAsync("ShareFolderUsingInvitationTest");

                var testUser = context.Web.SiteUsers.FirstOrDefault(p => p.PrincipalType == PrincipalType.User);

                var driveRecipients = new List<IDriveRecipient>()
                {
                    new DriveRecipient
                    {
                        Email = testUser.Mail
                    }
                };

                var shareRequestOptions = new InviteOptions()
                {
                    Message = "I'd like to share this folder with you",
                    RequireSignIn = true,
                    SendInvitation = true,
                    Recipients = driveRecipients,
                    Roles = new List<PermissionRole> { PermissionRole.Read }
                };

                var permission = folder.CreateSharingInvite(shareRequestOptions);
                Assert.IsNotNull(permission.Id);
                Assert.IsTrue(permission.Roles.Contains(PermissionRole.Read));

                await folder.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task ShareFolderUserLinkNoRecipientsExceptionTest()
        {
            TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Documents");
                await list.LoadAsync(y => y.RootFolder);

                var folder = await list.RootFolder.AddFolderAsync("ShareFolderUserLinkNoRecipientsExceptionTest");

                var shareLinkRequestOptions = new UserLinkOptions()
                {
                    Type = ShareType.View
                };
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                {
                    await folder.CreateUserSharingLinkAsync(shareLinkRequestOptions);
                });

                await folder.DeleteAsync();
            }
        }
    }
}
