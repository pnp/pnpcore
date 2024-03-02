using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.Security;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using PnP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Security
{
    [TestClass]
    public class UserTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetSharePointUsers()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.SiteUsers);

                Assert.IsTrue(web.SiteUsers.Length > 0);
            }
        }

        [TestMethod]
        public async Task GetCurrentUser()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Load the current user from current web
                var web = await context.Web.GetAsync(w => w.CurrentUser);

                Assert.IsNotNull(web.CurrentUser);
                Assert.IsTrue(web.CurrentUser.Requested);
                Assert.IsTrue(!string.IsNullOrEmpty(web.CurrentUser.UserPrincipalName));
                Assert.IsTrue(web.CurrentUser.Id > 0);
            }
        }

        [TestMethod]
        public async Task GetCurrentUserWithGroups()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Load the current user from current web
                var web = await context.Web.GetAsync(w => w.CurrentUser.QueryProperties(p => p.Groups));

                Assert.IsNotNull(web.CurrentUser);
                Assert.IsTrue(web.CurrentUser.Requested);
                Assert.IsTrue(web.CurrentUser.Id > 0);

                Assert.IsTrue(web.CurrentUser.Groups.Requested);
                Assert.IsTrue(web.CurrentUser.Groups.Length > 0);
            }
        }

        [TestMethod]
        public async Task GetSharePointUserAsGraphUser()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.SiteUsers);

                Assert.IsTrue(web.SiteUsers.Length > 0);

                // get the first real user
                var testUser = web.SiteUsers.AsRequested().FirstOrDefault(p => p.PrincipalType == PrincipalType.User);
                Assert.IsTrue(testUser != null);

                // Get that user as a Graph user
                var graphUser = await testUser.AsGraphUserAsync();

                Assert.IsTrue(graphUser != null);
                Assert.IsTrue(!string.IsNullOrEmpty(graphUser.Id));
                Assert.IsTrue(!string.IsNullOrEmpty(graphUser.UserPrincipalName));
                Assert.IsTrue((graphUser as GraphUser).Metadata.ContainsKey(PnPConstants.MetaDataGraphId));
                Assert.IsTrue((graphUser as GraphUser).Metadata[PnPConstants.MetaDataGraphId] == testUser.AadObjectId);
            }
        }

        [TestMethod]
        public async Task GetGraphUsersViaGroupMembership()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(p => p.Description, p => p.Owners, p => p.Members);

                Assert.IsTrue(team.IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(team.IsPropertyAvailable(p => p.Owners));
                Assert.IsTrue(team.Owners.Length > 0);
                Assert.IsTrue(team.IsPropertyAvailable(p => p.Members));
                Assert.IsTrue(team.Members.Length > 0);

                // Get the first owner
                var graphUser = team.Owners.AsRequested().FirstOrDefault();

                Assert.IsTrue(graphUser != null);
                Assert.IsTrue(!string.IsNullOrEmpty(graphUser.UserPrincipalName));

                // get sharepoint user for graph user
                var sharePointUser = await graphUser.AsSharePointUserAsync();
                Assert.IsTrue(sharePointUser != null);
                Assert.IsTrue(sharePointUser.Id > 0);
                Assert.IsTrue(sharePointUser.UserPrincipalName == graphUser.UserPrincipalName);
            }
        }

        #region Mails

        // Requires Mail.Send application permission

        [TestMethod]
        public async Task SendMailAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = true;
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    var testUser = await context.Web.SiteUsers.FirstOrDefaultAsync(p => p.PrincipalType == PrincipalType.User && p.Mail != "");
                    var graphUser = await testUser.AsGraphUserAsync();

                    var toUser = await context.Web.SiteUsers.Skip(1).FirstOrDefaultAsync(p => p.PrincipalType == PrincipalType.User && p.Mail != "");
                    var ccUser = await context.Web.SiteUsers.Skip(2).FirstOrDefaultAsync(p => p.PrincipalType == PrincipalType.User && p.Mail != "");
                    var bccUser = await context.Web.SiteUsers.Skip(3).FirstOrDefaultAsync(p => p.PrincipalType == PrincipalType.User && p.Mail != "");

                    await graphUser.SendMailAsync(
                        new MailOptions
                        {
                            Message = new MessageOptions
                            {
                                Subject = "Mail subject - PnP Rocks",
                                Body = "This is a mail body - PnP Rocks",
                                ToRecipients = new List<RecipientOptions>
                                {
                                new RecipientOptions
                                {
                                    EmailAddress = toUser.Mail
                                }
                                },
                                CcRecipients = new List<RecipientOptions>
                                {
                                new RecipientOptions
                                {
                                    EmailAddress = ccUser.Mail
                                }
                                },
                                BccRecipients = new List<RecipientOptions>
                                {
                                new RecipientOptions
                                {
                                    EmailAddress = bccUser.Mail
                                }
                                },
                                Importance = MessageImportance.High
                            },
                            UsingApplicationPermissions = true
                        });
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
            }

        }

        [TestMethod]
        public async Task SendMailWithAttachmentsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            try
            {
                TestCommon.Instance.UseApplicationPermissions = true;

                (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

                try
                {
                    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                    {
                        var testUser = await context.Web.SiteUsers.FirstOrDefaultAsync(p => p.PrincipalType == PrincipalType.User && p.Mail != "");
                        var graphUser = await testUser.AsGraphUserAsync();

                        var toUser = await context.Web.SiteUsers.Skip(1).FirstOrDefaultAsync(p => p.PrincipalType == PrincipalType.User && p.Mail != "");

                        var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                        var fileStream = await file.GetContentBytesAsync();

                        MimeTypeMap.TryGetMimeType(documentName, out string mimeType);

                        await graphUser.SendMailAsync(
                            new MailOptions
                            {
                                Message = new MessageOptions
                                {
                                    Subject = "Mail subject - PnP Rocks",
                                    Body = "This is a mail body including a file attachment - PnP Rocks",
                                    ToRecipients = new List<RecipientOptions>
                                    {
                                    new RecipientOptions
                                    {
                                        EmailAddress = toUser.Mail
                                    }
                                    },
                                    Attachments = new List<MessageAttachmentOptions>
                                    {
                                    new MessageAttachmentOptions
                                    {
                                        Name = documentName,
                                        ContentBytes = Convert.ToBase64String(fileStream),
                                        ContentType = mimeType
                                    }
                                    }
                                },
                                UsingApplicationPermissions = true
                            });
                    }
                }
                finally
                {
                    await TestAssets.CleanupTestDocumentAsync(2);
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
            }
        }

        [TestMethod]
        public async Task SendMailExceptionTestAsync()
        {
            //TestCommon.Instance.Mocking = false;

            try
            {
                TestCommon.Instance.UseApplicationPermissions = true;

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var testUser = await context.Web.SiteUsers.FirstOrDefaultAsync(p => p.PrincipalType == PrincipalType.User);
                    var graphUser = await testUser.AsGraphUserAsync();

                    MailOptions mailOptions = null;

                    await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                    {
                        await graphUser.SendMailAsync(mailOptions);
                    });

                    mailOptions = new MailOptions
                    {
                        Message = null,
                        UsingApplicationPermissions = true
                    };

                    await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                    {
                        await graphUser.SendMailAsync(mailOptions);
                    });

                    mailOptions.Message = new MessageOptions();

                    await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                    {
                        await graphUser.SendMailAsync(mailOptions);
                    });

                    mailOptions.Message.Body = "This is a mail body";

                    await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                    {
                        await graphUser.SendMailAsync(mailOptions);
                    });
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
            }

        }

        [TestMethod]
        public async Task SendMailDelegatedExceptionTestAsync()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                var testUser = await context.Web.SiteUsers.FirstOrDefaultAsync(p => p.PrincipalType == PrincipalType.User);
                var graphUser = await testUser.AsGraphUserAsync();

                MailOptions mailOptions = new()
                {
                    Message = null,
                    UsingApplicationPermissions = false
                };

                await Assert.ThrowsExceptionAsync<MicrosoftGraphServiceException>(async () =>
                {
                    await graphUser.SendMailAsync(mailOptions);
                });
            }
        }

        #endregion
    }
}
