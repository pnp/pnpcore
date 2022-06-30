using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Security;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using PnP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Me
{
    [TestClass]
    public class MeTest
    {
        
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetMeTest()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var me = await context.Me.GetAsync();
                Assert.IsNotNull(me.Id);
                Assert.IsNotNull(me.DisplayName);
                Assert.IsNotNull(me.UserPrincipalName);
            }
        }

        #region Mails
        
        // Requires Mail.Send delegated permission

        [TestMethod]
        public async Task SendMailAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var toUser = await context.Web.SiteUsers.FirstOrDefaultAsync(p => p.PrincipalType == PrincipalType.User && p.Mail != "");
                var ccUser = await context.Web.SiteUsers.Skip(1).FirstOrDefaultAsync(p => p.PrincipalType == PrincipalType.User && p.Mail != "");
                var bccUser = await context.Web.SiteUsers.Skip(2).FirstOrDefaultAsync(p => p.PrincipalType == PrincipalType.User && p.Mail != "");

                await context.Me.SendMailAsync(
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
                            }
                        },
                        SaveToSentItems = false
                    });
            }
        }

        [TestMethod]
        public async Task SendMailWithAttachmentsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    var toUser = await context.Web.SiteUsers.FirstOrDefaultAsync(p => p.PrincipalType == PrincipalType.User);

                    var file = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    var fileStream = await file.GetContentBytesAsync();
                    
                    MimeTypeMap.TryGetMimeType(documentName, out string mimeType);

                    await context.Me.SendMailAsync(
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
                            }
                        });
                }
            }
            finally 
            {
                await TestAssets.CleanupTestDocumentAsync(2);
            }
        }

        [TestMethod]
        public async Task SendMailExceptionTestAsync()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                MailOptions mailOptions = null;

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    await context.Me.SendMailAsync(mailOptions);
                });

                mailOptions = new MailOptions
                {
                    Message = null
                };

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    await context.Me.SendMailAsync(mailOptions);
                });

                mailOptions.Message = new MessageOptions();

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    await context.Me.SendMailAsync(mailOptions);
                });

                mailOptions.Message.Body = "This is a mail body";

                await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                {
                    await context.Me.SendMailAsync(mailOptions);
                });
            }
        }

        [TestMethod]
        public async Task SendMailApplicationPermissionsExceptionTestAsync()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                TestCommon.Instance.UseApplicationPermissions = true;

                var testUser = await context.Web.SiteUsers.FirstOrDefaultAsync(p => p.PrincipalType == PrincipalType.User);
                var graphUser = await testUser.AsGraphUserAsync();

                await Assert.ThrowsExceptionAsync<MicrosoftGraphServiceException>(async () =>
                {
                    await graphUser.SendMailAsync(null);
                });
            }
        }

        [TestMethod]
        public async Task SendMailApplicationExceptionTestAsync()
        {
            //TestCommon.Instance.Mocking = false;
            try
            {
                TestCommon.Instance.UseApplicationPermissions = true;

                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    await Assert.ThrowsExceptionAsync<MicrosoftGraphServiceException>(async () =>
                    {
                        await context.Me.SendMailAsync(null);
                    });
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
            }
        }
        #endregion
    }


}
