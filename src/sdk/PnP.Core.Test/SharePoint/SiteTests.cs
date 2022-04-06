using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class SiteTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            // TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetSiteSimpleProperties_A_G_Test()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Site.LoadAsync(
                    p => p.AllowCreateDeclarativeWorkflow,
                    p => p.AllowDesigner,
                    p => p.AllowExternalEmbeddingWrapper,
                    p => p.AllowMasterPageEditing,
                    p => p.AllowRevertFromTemplate,
                    p => p.AllowSaveDeclarativeWorkflowAsTemplate,
                    p => p.AllowSavePublishDeclarativeWorkflow,
                    p => p.AuditLogTrimmingRetention,
                    p => p.CanSyncHubSitePermissions,
                    p => p.ChannelGroupId,
                    p => p.Classification,
                    p => p.CommentsOnSitePagesDisabled,
                    p => p.DisableAppViews,
                    p => p.DisableCompanyWideSharingLinks,
                    p => p.DisableFlows,
                    p => p.ExternalSharingTipsEnabled,
                    p => p.ExternalUserExpirationInDays,
                    p => p.GeoLocation,
                    p => p.GroupId
                    );

                var site = context.Site;

                Assert.IsNotNull(site);
                Assert.IsTrue(site.AllowCreateDeclarativeWorkflow);
                Assert.IsTrue(site.AllowDesigner);
                Assert.AreNotEqual(0, site.AllowExternalEmbeddingWrapper);
                Assert.IsFalse(site.AllowMasterPageEditing);
                Assert.IsFalse(site.AllowRevertFromTemplate);
                Assert.IsTrue(site.AllowSaveDeclarativeWorkflowAsTemplate);
                Assert.IsTrue(site.AllowSavePublishDeclarativeWorkflow);
                Assert.AreNotEqual(0, site.AuditLogTrimmingRetention);
                Assert.IsFalse(site.CanSyncHubSitePermissions);
                Assert.AreEqual(default, site.ChannelGroupId);
                Assert.IsNotNull(site.Classification);
                Assert.IsFalse(site.CommentsOnSitePagesDisabled);
                Assert.IsFalse(site.DisableAppViews);
                Assert.IsFalse(site.DisableCompanyWideSharingLinks);
                Assert.IsFalse(site.DisableFlows);
                Assert.IsFalse(site.ExternalSharingTipsEnabled);
                Assert.AreEqual(0, site.ExternalUserExpirationInDays);
                Assert.AreNotEqual("", site.GeoLocation);
                Assert.AreNotEqual(default, site.GroupId);
            }
        }

        [TestMethod]
        public async Task GetSiteSimpleProperties_H_R_Test()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Site.LoadAsync(
                    p => p.HubSiteId,
                    p => p.IsHubSite,
                    p => p.LockIssue,
                    p => p.MaxItemsPerThrottledOperation,
                    p => p.ReadOnly,
                    p => p.RelatedGroupId
                    );

                var site = context.Site;

                Assert.IsNotNull(site);
                Assert.AreEqual(default, site.HubSiteId);
                Assert.IsFalse(site.IsHubSite);
                Assert.IsNull(site.LockIssue);
                Assert.AreNotEqual(0, site.MaxItemsPerThrottledOperation);
                Assert.IsFalse(site.ReadOnly);
                Assert.AreNotEqual(default, site.RelatedGroupId);
            }
        }

        [TestMethod]
        public async Task GetSiteSimpleProperties_S_Z_Test()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Site.LoadAsync(
                    p => p.SearchBoxInNavBar,
                    p => p.SearchBoxPlaceholderText,
                    p => p.SensitivityLabelId,
                    p => p.SensitivityLabel,
                    p => p.ServerRelativeUrl,
                    p => p.ShareByEmailEnabled,
                    p => p.ShareByLinkEnabled,
                    p => p.ShowPeoplePickerSuggestionsForGuestUsers,
                    p => p.SocialBarOnSitePagesDisabled,
                    p => p.StatusBarLink,
                    p => p.StatusBarText,
                    p => p.ThicketSupportDisabled,
                    p => p.TrimAuditLog
                    );

                var site = context.Site;

                Assert.IsNotNull(site);
                Assert.AreEqual(SearchBoxInNavBar.Inherit, site.SearchBoxInNavBar);
                Assert.IsNull(site.SearchBoxPlaceholderText);
                Assert.IsTrue(site.SensitivityLabelId == Guid.Empty);
                Assert.IsTrue(site.SensitivityLabel == "");
                Assert.AreNotEqual("", site.ServerRelativeUrl);
                // outcome depends on tenant level settings...just hit the properties
                Assert.IsTrue(site.ShareByEmailEnabled == true || site.ShareByEmailEnabled == false);
                Assert.IsTrue(site.ShareByLinkEnabled == true || site.ShareByLinkEnabled == false);
                Assert.IsFalse(site.ShowPeoplePickerSuggestionsForGuestUsers);
                Assert.IsFalse(site.SocialBarOnSitePagesDisabled);
                Assert.IsNull(site.StatusBarLink);
                Assert.IsNull(site.StatusBarText);
                Assert.IsTrue(site.ThicketSupportDisabled);
                Assert.IsTrue(site.TrimAuditLog);
            }
        }

        [TestMethod]
        public async Task GetSiteChangesAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var changes = await context.Site.GetChangesAsync(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5,
                });

                Assert.IsNotNull(changes);
                Assert.IsTrue(changes.Count > 0);
            }
        }

        [TestMethod]
        public void GetSiteChangesTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var changes = context.Site.GetChanges(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5,
                });

                Assert.IsNotNull(changes);
                Assert.IsTrue(changes.Count > 0);

                var changesBatch = context.Site.GetChangesBatch(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5,
                });


                Assert.IsFalse(changesBatch.IsAvailable);

                context.Execute();

                Assert.IsTrue(changesBatch.IsAvailable);

                Assert.IsTrue(changesBatch.Count > 0);

            }
        }

        [TestMethod]
        public async Task ComplianceTagTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Check if the compliance tag is also visible at site level
                var complianceTags = context.Site.GetAvailableComplianceTags();

                Assert.IsTrue(complianceTags != null);
                Assert.IsTrue(complianceTags.Any() == true);
            }
        }

        [TestMethod]
        public async Task CheckHomeSite_Positive()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.HomeTestSite))
            {
                Assert.IsTrue(context.Site.IsHomeSite());
            }
        }

        [TestMethod]
        public async Task CheckHomeSite_Negative()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsFalse(context.Site.IsHomeSite());
            }
        }

        [TestMethod]
        public async Task CreateCopyJobAsync()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                using (var secondCtx = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 2))
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    string destinationAbsoluteUrl = $"{secondCtx.Uri}/Shared Documents";
                    var jobUris = new List<string> { context.Uri + "/Shared Documents/" + documentName };

                    var copyJob = await context.Site.CreateCopyJobsAsync(jobUris.ToArray(), destinationAbsoluteUrl, new CopyMigrationOptions
                    {
                        AllowSchemaMismatch = true,
                        AllowSmallerVersionLimitOnDestination = true,
                        IgnoreVersionHistory = true,
                        IsMoveMode = false,
                        BypassSharedLock = true,
                        ExcludeChildren = true,
                        NameConflictBehavior = SPMigrationNameConflictBehavior.Replace
                    });

                    int delay = 1;

                    if (TestCommon.Instance.Mocking)
                    {
                        delay = 0;
                    }

                    await context.Site.EnsureCopyJobHasFinishedAsync(copyJob, delay);

                    // Check if file has been moved
                    await secondCtx.Web.LoadAsync(y => y.ServerRelativeUrl);
                    IFile movedFile = await secondCtx.Web.GetFileByServerRelativeUrlAsync($"{secondCtx.Web.ServerRelativeUrl}/Shared Documents/{documentName}");

                    Assert.IsNotNull(movedFile);
                    Assert.AreEqual(documentName, movedFile.Name);

                    // Clean up
                    await movedFile.DeleteAsync();
                }

            }
            await TestAssets.CleanupTestDocumentAsync(3, contextConfig: TestCommon.TestSite);
        }

        [TestMethod]
        public async Task CreateCopyJobMultipleDocumentsAsync()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string firstDocumentName, string firstDocumentUrl) = await TestAssets.CreateTestDocumentAsync(0, fileName: "FirstFile.docx");
            (_, string secondDocumentName, string secondDocumentUrl) = await TestAssets.CreateTestDocumentAsync(1, fileName: "SecondFile.docx");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                using (var secondCtx = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 3))
                {
                    string destinationAbsoluteUrl = $"{secondCtx.Uri}/Shared Documents";
                    var jobUris = new List<string> { context.Uri + "/Shared Documents/" + firstDocumentName, context.Uri + "/Shared Documents/" + secondDocumentName };
                    var copyJob = await context.Site.CreateCopyJobsAsync(jobUris.ToArray(), destinationAbsoluteUrl, new CopyMigrationOptions
                    {
                        AllowSchemaMismatch = true,
                        AllowSmallerVersionLimitOnDestination = true,
                        IgnoreVersionHistory = true,
                        IsMoveMode = false,
                        BypassSharedLock = true,
                        ExcludeChildren = true,
                        NameConflictBehavior = SPMigrationNameConflictBehavior.Replace
                    });

                    int delay = 1;

                    if (TestCommon.Instance.Mocking)
                    {
                        delay = 0;
                    }

                    await context.Site.EnsureCopyJobHasFinishedAsync(copyJob, delay);

                    // Check for copied files on target library
                    await secondCtx.Web.LoadAsync(y => y.ServerRelativeUrl);
                    IFile firstMovedFile = await secondCtx.Web.GetFileByServerRelativeUrlAsync($"{secondCtx.Web.ServerRelativeUrl}/Shared Documents/{firstDocumentName}");
                    IFile secondMovedFile = await secondCtx.Web.GetFileByServerRelativeUrlAsync($"{secondCtx.Web.ServerRelativeUrl}/Shared Documents/{secondDocumentName}");
                    Assert.IsNotNull(firstMovedFile);
                    Assert.IsNotNull(secondMovedFile);
                    Assert.AreEqual(firstMovedFile.Name, firstDocumentName);
                    Assert.AreEqual(secondMovedFile.Name, secondDocumentName);

                    // Clean up
                    await firstMovedFile.DeleteAsync();
                    await secondMovedFile.DeleteAsync();
                }
            }
            await TestAssets.CleanupTestDocumentAsync(4, fileName: firstDocumentName, contextConfig: TestCommon.TestSite);
            await TestAssets.CleanupTestDocumentAsync(5, fileName: secondDocumentName, contextConfig: TestCommon.TestSite);
        }

        [TestMethod]
        public async Task CreateCopyJob()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                using (var secondCtx = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 2))
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    string destinationAbsoluteUrl = $"{secondCtx.Uri}/Shared Documents";
                    var jobUris = new List<string> { context.Uri + "/Shared Documents/" + documentName };
                    var copyJob = context.Site.CreateCopyJobs(jobUris.ToArray(), destinationAbsoluteUrl, new CopyMigrationOptions
                    {
                        AllowSchemaMismatch = true,
                        AllowSmallerVersionLimitOnDestination = true,
                        IgnoreVersionHistory = true,
                        IsMoveMode = false,
                        BypassSharedLock = true,
                        ExcludeChildren = true,
                        NameConflictBehavior = SPMigrationNameConflictBehavior.Replace
                    });

                    int delay = 1;

                    if (TestCommon.Instance.Mocking)
                    {
                        delay = 0;
                    }

                    context.Site.EnsureCopyJobHasFinished(copyJob, delay);

                    // Check if file has been moved
                    await secondCtx.Web.LoadAsync(y => y.ServerRelativeUrl);
                    IFile movedFile = await secondCtx.Web.GetFileByServerRelativeUrlAsync($"{secondCtx.Web.ServerRelativeUrl}/Shared Documents/{documentName}");

                    Assert.IsNotNull(movedFile);
                    Assert.AreEqual(documentName, movedFile.Name);

                    // Clean up
                    await movedFile.DeleteAsync();
                }

            }
            await TestAssets.CleanupTestDocumentAsync(3, contextConfig: TestCommon.TestSite);
        }

        [TestMethod]
        public async Task CreateCopyJobInSameSiteCollection()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IList documentLibrary = await context.Web.Lists.AddAsync("TestLibrary", ListTemplateType.DocumentLibrary);

                string destinationAbsoluteUrl = $"{context.Uri}/TestLibrary";
                var jobUris = new List<string> { context.Uri + "/Shared Documents/" + documentName };
                var copyJob = context.Site.CreateCopyJobs(jobUris.ToArray(), destinationAbsoluteUrl, new CopyMigrationOptions
                {
                    AllowSchemaMismatch = true,
                    AllowSmallerVersionLimitOnDestination = true,
                    IgnoreVersionHistory = true,
                    IsMoveMode = false,
                    BypassSharedLock = true,
                    ExcludeChildren = true,
                    NameConflictBehavior = SPMigrationNameConflictBehavior.Replace
                });

                int delay = 1;

                if (TestCommon.Instance.Mocking)
                {
                    delay = 0;
                }

                context.Site.EnsureCopyJobHasFinished(copyJob, delay);

                // Check if file has been copied.
                IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                IFile testDocumentCopied = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl.Replace("Shared Documents", "TestLibrary"));

                Assert.IsNotNull(testDocument);
                Assert.IsNotNull(testDocumentCopied);
                Assert.AreEqual(testDocument.Name, testDocumentCopied.Name);

                await documentLibrary.DeleteAsync();
            }
            await TestAssets.CleanupTestDocumentAsync(3, contextConfig: TestCommon.TestSite);
        }

        [TestMethod]
        public async Task CreateCopyJobFolderAsync()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 0))
            {
                using (var secondCtx = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 1))
                {
                    IFolder folderToCopy = await context.Web.Lists.GetByTitle("Documents").RootFolder.AddFolderAsync("Test folder");
                    (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(2, parentFolder: folderToCopy);
                    
                    string destinationAbsoluteUrl = $"{secondCtx.Uri}/Shared Documents";
                    var jobUris = new List<string> { context.Uri + "/Shared Documents/" + folderToCopy.Name };
                    var copyJob = await context.Site.CreateCopyJobsAsync(jobUris.ToArray(), destinationAbsoluteUrl, new CopyMigrationOptions
                    {
                        AllowSchemaMismatch = true,
                        AllowSmallerVersionLimitOnDestination = true,
                        IgnoreVersionHistory = true,
                        IsMoveMode = false,
                        BypassSharedLock = true,
                        ExcludeChildren = false,
                        NameConflictBehavior = SPMigrationNameConflictBehavior.Replace
                    });

                    int delay = 1;

                    if (TestCommon.Instance.Mocking)
                    {
                        delay = 0;
                    }

                    await context.Site.EnsureCopyJobHasFinishedAsync(copyJob, delay);

                    await secondCtx.Web.LoadAsync(y => y.ServerRelativeUrl);
                    IFolder movedFolder = await secondCtx.Web.GetFolderByServerRelativeUrlAsync($"{secondCtx.Web.ServerRelativeUrl}/Shared Documents/{folderToCopy.Name}");
                    await movedFolder.LoadAsync(f => f.Files);
                    Assert.IsNotNull(movedFolder);
                    Assert.AreEqual(movedFolder.Files.Length, 1);
                    Assert.AreEqual(movedFolder.Files.FirstOrDefault().Name, documentName);

                    // Clean up
                    await movedFolder.DeleteAsync();
                    await folderToCopy.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task CreateCopyJobIncludingVersionsAsync()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                using (var secondCtx = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 2))
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    // Generate some versions
                    var listItem = testDocument.ListItemAllFields;
                    for (var i = 1; i <= 5; i++)
                    {
                        listItem["Title"] = Guid.NewGuid().ToString();
                        await listItem.UpdateAsync();
                    }

                    string destinationAbsoluteUrl = $"{secondCtx.Uri}/Shared Documents";
                    var jobUris = new List<string> { context.Uri + "/Shared Documents/" + documentName };
                    var copyJob = await context.Site.CreateCopyJobsAsync(jobUris.ToArray(), destinationAbsoluteUrl, new CopyMigrationOptions
                    {
                        AllowSchemaMismatch = true,
                        AllowSmallerVersionLimitOnDestination = true,
                        IgnoreVersionHistory = false,
                        IsMoveMode = false,
                        BypassSharedLock = true,
                        ExcludeChildren = true,
                        NameConflictBehavior = SPMigrationNameConflictBehavior.Replace
                    });

                    int delay = 1;

                    if (TestCommon.Instance.Mocking)
                    {
                        delay = 0;
                    }

                    await context.Site.EnsureCopyJobHasFinishedAsync(copyJob, delay);

                    // Check if file has been moved
                    await secondCtx.Web.LoadAsync(y => y.ServerRelativeUrl);
                    IFile movedFile = await secondCtx.Web.GetFileByServerRelativeUrlAsync($"{secondCtx.Web.ServerRelativeUrl}/Shared Documents/{documentName}");
                    await movedFile.LoadAsync(f => f.Versions);
                    Assert.IsNotNull(movedFile);
                    Assert.AreEqual(documentName, movedFile.Name);
                    Assert.AreEqual(movedFile.Versions.Length, 5);

                    // Clean up
                    await movedFile.DeleteAsync();
                }

            }
            await TestAssets.CleanupTestDocumentAsync(3, contextConfig: TestCommon.TestSite);
        }

        [TestMethod]
        public async Task CreateMoveJobAsync()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                using (var secondCtx = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 2))
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    string destinationAbsoluteUrl = $"{secondCtx.Uri}/Shared Documents";
                    var jobUris = new List<string> { context.Uri + "/Shared Documents/" + documentName };
                    var copyJob = await context.Site.CreateCopyJobsAsync(jobUris.ToArray(), destinationAbsoluteUrl, new CopyMigrationOptions
                    {
                        AllowSchemaMismatch = true,
                        AllowSmallerVersionLimitOnDestination = true,
                        IgnoreVersionHistory = true,
                        IsMoveMode = true,
                        BypassSharedLock = true,
                        ExcludeChildren = true,
                        NameConflictBehavior = SPMigrationNameConflictBehavior.Replace
                    });

                    int delay = 1;

                    if (TestCommon.Instance.Mocking)
                    {
                        delay = 0;
                    }

                    await context.Site.EnsureCopyJobHasFinishedAsync(copyJob, delay);

                    // Check if file exists on moved ctx
                    // Check for moved files on target library

                    await secondCtx.Web.LoadAsync(y => y.ServerRelativeUrl);
                    IFile movedDocument = await secondCtx.Web.GetFileByServerRelativeUrlAsync($"{secondCtx.Web.ServerRelativeUrl}/Shared Documents/{documentName}");
                    
                    Assert.IsNotNull(movedDocument);
                    Assert.AreEqual(movedDocument.Name, documentName);

                    // Clean up moved file
                    await movedDocument.DeleteAsync();
                    
                    // Check for moved files still existing on source library (should not exist!!)

                    await context.Web.LoadAsync(y => y.ServerRelativeUrl);
                    IFolder rootFolder = await context.Web.GetFolderByServerRelativeUrlAsync($"{context.Web.ServerRelativeUrl}/Shared Documents");
                    await rootFolder.LoadAsync(y => y.Files);
                    Assert.IsNull(rootFolder.Files.FirstOrDefault(y => y.Name == documentName));
                }

            }
        }

        [TestMethod]
        public async Task CreateMoveJob()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                using (var secondCtx = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 2))
                {
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                    string destinationAbsoluteUrl = $"{secondCtx.Uri}/Shared Documents";
                    var jobUris = new List<string> { context.Uri + "/Shared Documents/" + documentName };

                    int delay = 1;

                    if (TestCommon.Instance.Mocking)
                    {
                        delay = 0;
                    }

                    var copyJob = context.Site.CreateCopyJobs(jobUris.ToArray(), destinationAbsoluteUrl, new CopyMigrationOptions
                    {
                        AllowSchemaMismatch = true,
                        AllowSmallerVersionLimitOnDestination = true,
                        IgnoreVersionHistory = true,
                        IsMoveMode = true,
                        BypassSharedLock = true,
                        ExcludeChildren = true,
                        NameConflictBehavior = SPMigrationNameConflictBehavior.Replace
                    }, waitUntilFinished: true, waitAfterStatusCheck: delay);

                    // Check for moved files on target library
                    await secondCtx.Web.LoadAsync(y => y.ServerRelativeUrl);
                    IFile movedDocument = await secondCtx.Web.GetFileByServerRelativeUrlAsync($"{secondCtx.Web.ServerRelativeUrl}/Shared Documents/{documentName}");

                    Assert.IsNotNull(movedDocument);
                    Assert.AreEqual(movedDocument.Name, documentName);

                    // Clean up moved file
                    await movedDocument.DeleteAsync();

                    // Check for moved files still existing on source library (should not exist!!)

                    await context.Web.LoadAsync(y => y.ServerRelativeUrl);
                    IFolder rootFolder = await context.Web.GetFolderByServerRelativeUrlAsync($"{context.Web.ServerRelativeUrl}/Shared Documents");
                    await rootFolder.LoadAsync(y => y.Files);
                    Assert.IsNull(rootFolder.Files.FirstOrDefault(y => y.Name == documentName));
                }

            }
        }

        [TestMethod]
        public async Task CreateMoveJobInSameSiteCollection()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string documentName, string documentUrl) = await TestAssets.CreateTestDocumentAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                IList documentLibrary = await context.Web.Lists.AddAsync("TestLibrary", ListTemplateType.DocumentLibrary);

                string destinationAbsoluteUrl = $"{context.Uri}/TestLibrary";
                var jobUris = new List<string> { context.Uri + "/Shared Documents/" + documentName };
                var copyJob = context.Site.CreateCopyJobs(jobUris.ToArray(), destinationAbsoluteUrl, new CopyMigrationOptions
                {
                    AllowSchemaMismatch = true,
                    AllowSmallerVersionLimitOnDestination = true,
                    IgnoreVersionHistory = true,
                    IsMoveMode = true,
                    BypassSharedLock = true,
                    ExcludeChildren = true,
                    NameConflictBehavior = SPMigrationNameConflictBehavior.Replace
                });

                int delay = 1;

                if (TestCommon.Instance.Mocking)
                {
                    delay = 0;
                }

                context.Site.EnsureCopyJobHasFinished(copyJob, delay);

                // Check if file has been copied.
                IFile testDocument = null;
                try
                {
                    // Doc was moved, so should not be available anymore
                    testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);
                }
                catch { }
                IFile testDocumentCopied = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl.Replace("Shared Documents", "TestLibrary"));

                Assert.IsNull(testDocument);
                Assert.IsNotNull(testDocumentCopied);                

                await documentLibrary.DeleteAsync();
            }            
        }

        [TestMethod]
        public async Task CreateMoveJobMultipleDocumentsAsync()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string firstDocumentName, string firstDocumentUrl) = await TestAssets.CreateTestDocumentAsync(0, fileName: "FirstFile.docx");
            (_, string secondDocumentName, string secondDocumentUrl) = await TestAssets.CreateTestDocumentAsync(1, fileName: "SecondFile.docx");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                using (var secondCtx = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 3))
                {
                    string destinationAbsoluteUrl = $"{secondCtx.Uri}/Shared Documents";
                    var jobUris = new List<string> { context.Uri + "/Shared Documents/" + firstDocumentName, context.Uri + "/Shared Documents/" + secondDocumentName };

                    int delay = 1;

                    if (TestCommon.Instance.Mocking)
                    {
                        delay = 0;
                    }

                    var copyJob = await context.Site.CreateCopyJobsAsync(jobUris.ToArray(), destinationAbsoluteUrl, new CopyMigrationOptions
                    {
                        AllowSchemaMismatch = true,
                        AllowSmallerVersionLimitOnDestination = true,
                        IgnoreVersionHistory = true,
                        IsMoveMode = true,
                        BypassSharedLock = true,
                        ExcludeChildren = true,
                        NameConflictBehavior = SPMigrationNameConflictBehavior.Replace
                    }, waitUntilFinished: true, waitAfterStatusCheck: delay);

                    // Check for moved files on target library

                    await secondCtx.Web.LoadAsync(y => y.ServerRelativeUrl);
                    IFile firstMovedDocument = await secondCtx.Web.GetFileByServerRelativeUrlAsync($"{secondCtx.Web.ServerRelativeUrl}/Shared Documents/{firstDocumentName}");
                    IFile secondMovedDocument = await secondCtx.Web.GetFileByServerRelativeUrlAsync($"{secondCtx.Web.ServerRelativeUrl}/Shared Documents/{secondDocumentName}");

                    Assert.IsNotNull(firstMovedDocument);
                    Assert.IsNotNull(secondMovedDocument);
                    Assert.AreEqual(firstMovedDocument.Name, firstDocumentName);
                    Assert.AreEqual(secondMovedDocument.Name, secondDocumentName);

                    // Clean up
                    await firstMovedDocument.DeleteAsync();
                    await secondMovedDocument.DeleteAsync();

                    // Check for moved files still existing on source library (should not exist!!)

                    await context.Web.LoadAsync(y => y.ServerRelativeUrl);
                    IFolder rootFolder = await context.Web.GetFolderByServerRelativeUrlAsync($"{context.Web.ServerRelativeUrl}/Shared Documents");
                    await rootFolder.LoadAsync(y => y.Files);
                    Assert.IsNull(rootFolder.Files.FirstOrDefault(y => y.Name == firstDocumentName));
                    Assert.IsNull(rootFolder.Files.FirstOrDefault(y => y.Name == secondDocumentName));
                }
            }
        }

        [TestMethod]
        public async Task CreateMoveJobMultipleDocumentsAndFolderAsync()
        {
            //TestCommon.Instance.Mocking = false;

            (_, string firstDocumentName, string firstDocumentUrl) = await TestAssets.CreateTestDocumentAsync(0, fileName: "FirstFile.docx");
            (_, string secondDocumentName, string secondDocumentUrl) = await TestAssets.CreateTestDocumentAsync(1, fileName: "SecondFile.docx");
            
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                var testFolder = await context.Web.Lists.GetByTitle("Documents").RootFolder.AddFolderAsync("TestFolder 1");
                (_, string thirdDocumentName, string thirdDocumentUrl) = await TestAssets.CreateTestDocumentAsync(3, fileName: "ThirdFile.docx", parentFolder: testFolder);

                using (var secondCtx = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 4))
                {
                    string destinationAbsoluteUrl = $"{secondCtx.Uri}/Shared Documents";
                    var jobUris = new List<string>
                    {
                        context.Uri + "/Shared Documents/" + firstDocumentName,
                        context.Uri + "/Shared Documents/" + secondDocumentName,
                        context.Uri + "/Shared Documents/" + testFolder.Name,
                    };
                    var copyJob = await context.Site.CreateCopyJobsAsync(jobUris.ToArray(), destinationAbsoluteUrl, new CopyMigrationOptions
                    {
                        AllowSchemaMismatch = true,
                        AllowSmallerVersionLimitOnDestination = true,
                        IgnoreVersionHistory = true,
                        IsMoveMode = true,
                        BypassSharedLock = true,
                        ExcludeChildren = false,
                        NameConflictBehavior = SPMigrationNameConflictBehavior.Replace
                    });

                    int delay = 1;

                    if (TestCommon.Instance.Mocking)
                    {
                        delay = 0;
                    }

                    await context.Site.EnsureCopyJobHasFinishedAsync(copyJob, delay);

                    // Check for copied files on target library
                    await secondCtx.Web.LoadAsync(y => y.ServerRelativeUrl);
                    IFile firstMovedDocument = await secondCtx.Web.GetFileByServerRelativeUrlAsync($"{secondCtx.Web.ServerRelativeUrl}/Shared Documents/{firstDocumentName}");
                    IFile secondMovedDocument = await secondCtx.Web.GetFileByServerRelativeUrlAsync($"{secondCtx.Web.ServerRelativeUrl}/Shared Documents/{secondDocumentName}");
                    IFile thirdMovedDocument = await secondCtx.Web.GetFileByServerRelativeUrlAsync($"{secondCtx.Web.ServerRelativeUrl}/Shared Documents/{testFolder.Name}/{thirdDocumentName}");
                    IFolder movedFolder = await secondCtx.Web.GetFolderByServerRelativeUrlAsync($"{secondCtx.Web.ServerRelativeUrl}/Shared Documents/{testFolder.Name}");

                    Assert.IsNotNull(firstMovedDocument);
                    Assert.IsNotNull(secondMovedDocument);
                    Assert.IsNotNull(thirdMovedDocument);
                    Assert.IsNotNull(movedFolder);
                    Assert.AreEqual(firstMovedDocument.Name, firstDocumentName);
                    Assert.AreEqual(secondMovedDocument.Name, secondDocumentName);
                    Assert.AreEqual(thirdMovedDocument.Name, thirdDocumentName);
                    Assert.AreEqual(movedFolder.Name, testFolder.Name);

                    // Clean up
                    await firstMovedDocument.DeleteAsync();
                    await secondMovedDocument.DeleteAsync();
                    await movedFolder.DeleteAsync();

                    await context.Web.LoadAsync(y => y.ServerRelativeUrl);
                    IFolder rootFolder = await context.Web.GetFolderByServerRelativeUrlAsync($"{context.Web.ServerRelativeUrl}/Shared Documents");
                    await rootFolder.LoadAsync(y => y.Folders, y => y.Files);
                    Assert.IsNull(rootFolder.Files.FirstOrDefault(y => y.Name == firstDocumentName));
                    Assert.IsNull(rootFolder.Files.FirstOrDefault(y => y.Name == secondDocumentName));
                    Assert.IsNull(rootFolder.Folders.FirstOrDefault(y => y.Name == testFolder.Name));
                }
            }
        }
    }
}

