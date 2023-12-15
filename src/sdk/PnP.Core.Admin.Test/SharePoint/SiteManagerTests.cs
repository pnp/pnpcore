using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Model;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeZone = PnP.Core.Admin.Model.SharePoint.TimeZone;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class SiteManagerTests
    {

        [TestMethod]
        public async Task GetSiteCollectionProperties()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    await context.Web.EnsurePropertiesAsync(p => p.Description, p => p.Title, p => p.Language, p => p.WebTemplate, p => p.WebTemplateConfiguration);

                    var siteProperties = context.GetSiteCollectionManager().GetSiteCollectionProperties(context.Uri);
                    Assert.IsFalse(siteProperties.AllowDownloadingNonWebViewableFiles);
                    Assert.IsTrue(siteProperties.AllowEditing);
                    Assert.IsTrue(siteProperties.AllowSelfServiceUpgrade);
                    Assert.AreEqual(siteProperties.AnonymousLinkExpirationInDays, 0);
#pragma warning disable CS0618 // Type or member is obsolete
                    Assert.IsTrue(siteProperties.AuthContextStrength == null);
#pragma warning restore CS0618 // Type or member is obsolete
                    Assert.IsTrue(siteProperties.AuthenticationContextName == null);
                    Assert.IsTrue(siteProperties.BlockDownloadLinksFileType == Model.SharePoint.BlockDownloadLinksFileTypes.WebPreviewableFiles);
                    Assert.IsFalse(siteProperties.BlockDownloadPolicy);
                    Assert.IsFalse(siteProperties.ClearRestrictedAccessControl);
                    Assert.IsFalse(siteProperties.CommentsOnSitePagesDisabled);
                    Assert.IsTrue(siteProperties.CompatibilityLevel == 15);
                    Assert.IsTrue(siteProperties.ConditionalAccessPolicy == Model.SharePoint.SPOConditionalAccessPolicyType.AllowFullAccess);
                    Assert.IsTrue(siteProperties.DefaultLinkPermission == Model.SharePoint.SharingPermissionType.None);
                    Assert.IsFalse(siteProperties.DefaultLinkToExistingAccess);
                    Assert.IsFalse(siteProperties.DefaultLinkToExistingAccessReset);
                    Assert.IsTrue(siteProperties.DefaultShareLinkRole == Core.Model.SharePoint.Role.None);
                    Assert.IsTrue(siteProperties.DefaultShareLinkScope == (SharingScope.Anyone | SharingScope.Organization |SharingScope.Uninitialized));
                    Assert.IsTrue(siteProperties.DefaultSharingLinkType == Model.SharePoint.SharingLinkType.None);
                    Assert.IsTrue(siteProperties.DenyAddAndCustomizePages == Model.SharePoint.DenyAddAndCustomizePagesStatus.Enabled);
                    Assert.IsTrue(siteProperties.Description == context.Web.Description);
                    Assert.IsTrue(siteProperties.DisableAppViews == Model.SharePoint.AppViewsPolicy.NotDisabled);
                    Assert.IsTrue(siteProperties.DisableCompanyWideSharingLinks == Model.SharePoint.CompanyWideSharingLinksPolicy.NotDisabled);
                    Assert.IsTrue(siteProperties.DisableFlows == Model.SharePoint.FlowsPolicy.NotDisabled);
                    Assert.IsFalse(siteProperties.ExcludeBlockDownloadPolicySiteOwners);
                    Assert.IsTrue(siteProperties.ExternalUserExpirationInDays == 0);
                    Assert.IsTrue(siteProperties.GroupId == context.Site.GroupId);
                    Assert.IsTrue(siteProperties.GroupOwnerLoginName == $"c:0o.c|federateddirectoryclaimprovider|{context.Site.GroupId}_o");
                    Assert.IsFalse(siteProperties.HasHolds);
                    Assert.IsTrue(siteProperties.HubSiteId == System.Guid.Empty);
                    Assert.IsTrue(siteProperties.IBMode == "");
                    Assert.IsTrue(siteProperties.IBSegmentsToAdd == null);
                    Assert.IsTrue(siteProperties.IBSegmentsToRemove == null);
                    Assert.IsTrue(siteProperties.IsGroupOwnerSiteAdmin);
                    Assert.IsFalse(siteProperties.IsHubSite);
                    Assert.IsFalse(siteProperties.IsTeamsChannelConnected);
                    Assert.IsTrue(siteProperties.IsTeamsConnected);
                    Assert.IsTrue(siteProperties.LastContentModifiedDate < System.DateTime.Now);
                    Assert.IsTrue(siteProperties.Lcid > 0);
                    Assert.IsTrue(siteProperties.LimitedAccessFileType == Model.SharePoint.SPOLimitedAccessFileType.WebPreviewableFiles);
                    Assert.IsTrue(siteProperties.LockIssue == null);
                    Assert.IsTrue(siteProperties.LockState == "Unlock");
                    Assert.IsTrue(siteProperties.MediaTranscription == Model.SharePoint.MediaTranscriptionPolicyType.Enabled);
                    Assert.IsTrue(siteProperties.OverrideBlockUserInfoVisibility == Model.SharePoint.SiteUserInfoVisibilityPolicyValue.OrganizationDefault);
                    Assert.IsFalse(siteProperties.OverrideTenantAnonymousLinkExpirationPolicy);
                    Assert.IsFalse(siteProperties.OverrideTenantExternalUserExpirationPolicy);
                    Assert.IsTrue(siteProperties.Owner == $"{context.Site.GroupId}_o");
                    Assert.IsTrue(siteProperties.OwnerEmail == $"{context.Web.Title}@{context.Uri.DnsSafeHost.Replace(".sharepoint.com", ".onmicrosoft.com")}");
                    Assert.IsTrue(siteProperties.OwnerLoginName == $"c:0o.c|federateddirectoryclaimprovider|{context.Site.GroupId}_o");
                    Assert.IsTrue(siteProperties.OwnerName == context.Web.Title);
                    Assert.IsTrue(siteProperties.PWAEnabled == Model.SharePoint.PWAEnabledStatus.Disabled);
                    Assert.IsFalse(siteProperties.ReadOnlyForUnmanagedDevices);
                    Assert.IsTrue(siteProperties.RelatedGroupId == context.Site.GroupId);
                    Assert.IsTrue(siteProperties.RestrictedToRegion == Model.SharePoint.RestrictedToRegion.Unknown);
                    Assert.IsFalse(siteProperties.RequestFilesLinkEnabled);
                    Assert.IsTrue(siteProperties.RequestFilesLinkExpirationInDays == -1);
                    Assert.IsFalse(siteProperties.RestrictedAccessControl);
                    Assert.IsTrue(siteProperties.RestrictedAccessControlGroups == null || siteProperties.RestrictedAccessControlGroups.Any() == false || siteProperties.RestrictedAccessControlGroups.Any() == true);
                    Assert.IsTrue(siteProperties.RestrictedAccessControlGroupsToAdd == null);
                    Assert.IsTrue(siteProperties.RestrictedAccessControlGroupsToRemove == null);
                    Assert.IsTrue(siteProperties.SensitivityLabel == System.Guid.Empty);
                    Assert.IsTrue(siteProperties.SensitivityLabel2 == "");
                    Assert.IsFalse(siteProperties.SetOwnerWithoutUpdatingSecondaryAdmin);
                    Assert.IsTrue(siteProperties.SharingAllowedDomainList == "");
                    Assert.IsTrue(siteProperties.SharingBlockedDomainList == "");
                    Assert.IsTrue(siteProperties.SharingCapability == Model.SharePoint.SharingCapabilities.ExternalUserSharingOnly);
                    Assert.IsTrue(siteProperties.SharingDomainRestrictionMode == Model.SharePoint.SharingDomainRestrictionModes.None);
                    Assert.IsTrue(siteProperties.SharingLockDownCanBeCleared);
                    Assert.IsFalse(siteProperties.SharingLockDownEnabled);
                    Assert.IsFalse(siteProperties.ShowPeoplePickerSuggestionsForGuestUsers);
                    Assert.IsTrue(siteProperties.SiteDefinedSharingCapability == Model.SharePoint.SharingCapabilities.ExternalUserSharingOnly);
                    Assert.IsFalse(siteProperties.SocialBarOnSitePagesDisabled);
                    Assert.IsTrue(siteProperties.Status == "Active");
                    Assert.IsTrue(siteProperties.StorageMaximumLevel > 0);
                    Assert.IsTrue(siteProperties.StorageQuotaType == null);
                    Assert.IsTrue(siteProperties.StorageUsage > 0);
                    Assert.IsTrue(siteProperties.StorageWarningLevel > 0);
                    Assert.IsTrue(siteProperties.TeamsChannelType == Model.SharePoint.TeamsChannelTypeValue.None);
                    Assert.IsTrue(siteProperties.Template == context.Web.WebTemplateConfiguration);
                    Assert.IsTrue(siteProperties.TimeZoneId == (Model.SharePoint.TimeZone)context.Web.RegionalSettings.TimeZone.Id);
                    Assert.IsTrue(siteProperties.Title == context.Web.Title);
                    Assert.IsTrue(siteProperties.Url == context.Uri.ToString());
                    Assert.IsTrue(siteProperties.WebsCount > 0);
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
            }
        }

        [TestMethod]
        public async Task GetSiteCollectionPropertiesForNewSite()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            CommunicationSiteOptions communicationSiteToCreate = null;
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {

                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    if (!TestCommon.Instance.Mocking)
                    {
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsite{Guid.NewGuid().ToString().Replace("-", "")}");
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "SiteUrl", siteUrl.ToString() }
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                    }

                    communicationSiteToCreate = new CommunicationSiteOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                    };

                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = false
                    };

                    using (var newSiteContext = context.GetSiteCollectionManager().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
                    {
                        var siteProperties = newSiteContext.GetSiteCollectionManager().GetSiteCollectionProperties(newSiteContext.Uri);
                        Assert.IsNotNull(siteProperties);
                    }

                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSiteCollectionManager().DeleteSiteCollection(communicationSiteToCreate.Url);
                }
            }
        }

        [TestMethod]
        public async Task SetSiteCollectionProperties()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    var siteProperties = context.GetSiteCollectionManager().GetSiteCollectionProperties(context.Uri);

                    string originalTitle = siteProperties.Title;
                    Model.SharePoint.FlowsPolicy originalFlowsPolicy = siteProperties.DisableFlows;

                    string newTitle = null;
                    Model.SharePoint.FlowsPolicy newFlowsPolicy;

                    if (!TestCommon.Instance.Mocking)
                    {
                        newTitle = $"New title - {DateTime.Now}";
                        newFlowsPolicy = siteProperties.DisableFlows == Model.SharePoint.FlowsPolicy.Disabled ? Model.SharePoint.FlowsPolicy.NotDisabled : Model.SharePoint.FlowsPolicy.Disabled;

                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "Title", newTitle },
                            { "FlowPolicy", newFlowsPolicy.ToString() }
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        newTitle = TestManager.GetProperties(context)["Title"];
                        newFlowsPolicy = (Model.SharePoint.FlowsPolicy)Enum.Parse(typeof(Model.SharePoint.FlowsPolicy), TestManager.GetProperties(context)["FlowPolicy"].ToString());
                    }

                    siteProperties.Title = newTitle;
                    siteProperties.DisableFlows = newFlowsPolicy;

                    siteProperties.Update();

                    // Read the site properties again
                    siteProperties = context.GetSiteCollectionManager().GetSiteCollectionProperties(context.Uri);

                    Assert.IsTrue(siteProperties.Title == newTitle);
                    Assert.IsTrue(siteProperties.DisableFlows == newFlowsPolicy);

                    // Reset the properties back to their defaults
                    siteProperties.Title = originalTitle;
                    siteProperties.DisableFlows = originalFlowsPolicy;

                    await siteProperties.UpdateAsync();
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
            }
        }

        [TestMethod]
        public async Task SetSiteCollectionPropertiesForNewSite()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            CommunicationSiteOptions communicationSiteToCreate = null;
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {

                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    if (!TestCommon.Instance.Mocking)
                    {
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsite{Guid.NewGuid().ToString().Replace("-", "")}");
                        //Dictionary<string, string> properties = new Dictionary<string, string>
                        //{
                        //    { "SiteUrl", siteUrl.ToString() }
                        //};
                        //TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                    }

                    communicationSiteToCreate = new CommunicationSiteOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                    };

                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = false
                    };

                    using (var newSiteContext = context.GetSiteCollectionManager().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
                    {
                        var siteProperties = newSiteContext.GetSiteCollectionManager().GetSiteCollectionProperties(newSiteContext.Uri);
                        Assert.IsNotNull(siteProperties);

                        // Set the properties
                        string originalTitle = siteProperties.Title;
                        Model.SharePoint.FlowsPolicy originalFlowsPolicy = siteProperties.DisableFlows;

                        string newTitle = null;
                        Model.SharePoint.FlowsPolicy newFlowsPolicy;

                        if (!TestCommon.Instance.Mocking)
                        {
                            newTitle = $"New title - {DateTime.Now}";
                            newFlowsPolicy = siteProperties.DisableFlows == Model.SharePoint.FlowsPolicy.Disabled ? Model.SharePoint.FlowsPolicy.NotDisabled : Model.SharePoint.FlowsPolicy.Disabled;

                            Dictionary<string, string> properties = new Dictionary<string, string>
                            {
                                { "SiteUrl", siteUrl.ToString() },
                                { "Title", newTitle },
                                { "FlowPolicy", newFlowsPolicy.ToString() }
                            };
                            TestManager.SaveProperties(context, properties);
                        }
                        else
                        {
                            newTitle = TestManager.GetProperties(context)["Title"];
                            newFlowsPolicy = (Model.SharePoint.FlowsPolicy)Enum.Parse(typeof(Model.SharePoint.FlowsPolicy), TestManager.GetProperties(context)["FlowPolicy"].ToString());
                        }

                        siteProperties.Title = newTitle;
                        siteProperties.DisableFlows = newFlowsPolicy;

                        siteProperties.Update();

                    }

                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSiteCollectionManager().DeleteSiteCollection(communicationSiteToCreate.Url);
                }
            }
        }

        [TestMethod]
        public async Task SetSiteCollectionLockingPropertiesForNewSite()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            CommunicationSiteOptions communicationSiteToCreate = null;
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {

                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    if (!TestCommon.Instance.Mocking)
                    {
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsite{Guid.NewGuid().ToString().Replace("-", "")}");
                        //Dictionary<string, string> properties = new Dictionary<string, string>
                        //{
                        //    { "SiteUrl", siteUrl.ToString() }
                        //};
                        //TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                    }

                    communicationSiteToCreate = new CommunicationSiteOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                    };

                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = false
                    };

                    using (var newSiteContext = context.GetSiteCollectionManager().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
                    {
                        var siteProperties = newSiteContext.GetSiteCollectionManager().GetSiteCollectionProperties(newSiteContext.Uri);
                        Assert.IsNotNull(siteProperties);

                        // Set the properties
                        string originalTitle = siteProperties.Title;
                        string originalLockState = siteProperties.LockState;

                        string newTitle = null;
                        string newLockState;

                        if (!TestCommon.Instance.Mocking)
                        {
                            newTitle = $"New title - {DateTime.Now}";
                            newLockState = "NoAccess";

                            Dictionary<string, string> properties = new Dictionary<string, string>
                            {
                                { "SiteUrl", siteUrl.ToString() },
                                { "Title", newTitle },
                                { "LockState", newLockState }
                            };
                            TestManager.SaveProperties(context, properties);
                        }
                        else
                        {
                            newTitle = TestManager.GetProperties(context)["Title"];
                            newLockState = TestManager.GetProperties(context)["LockState"];
                        }

                        siteProperties.Title = newTitle;
                        siteProperties.LockState = newLockState;

                        // Lock the site
                        siteProperties.Update();

                        // Get the site props again
                        siteProperties = newSiteContext.GetSiteCollectionManager().GetSiteCollectionProperties(newSiteContext.Uri);

                        // Restore the original values
                        siteProperties.LockState = originalLockState;

                        if (context.Mode == TestMode.Record)
                        {
                            // Add a little delay between creation and deletion
                            await Task.Delay(TimeSpan.FromSeconds(10));
                        }

                        siteProperties.Update();
                    }

                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSiteCollectionManager().DeleteSiteCollection(communicationSiteToCreate.Url);
                }
            }
        }


        [TestMethod]
        public async Task SetSiteCollectionAdminsGroupifiedSiteAddOnly_Async()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            TeamSiteWithoutGroupOptions teamSiteToCreate = null;

            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {

                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    string alias = null;
                    if (!TestCommon.Instance.Mocking)
                    {
                        Guid tempId = Guid.NewGuid();
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestteamsite{tempId.ToString().Replace("-", "")}");
                        alias = $"pnpcoresdktestteamsite{tempId.ToString().Replace("-", "")}";
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "SiteUrl", siteUrl.ToString() },
                            { "Alias", alias },
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                        alias = TestManager.GetProperties(context)["Alias"];
                    }

                    teamSiteToCreate = new TeamSiteWithoutGroupOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                    };


                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = false
                    };

                    // first create the site collection
                    using (var newSiteContext = await context.GetSiteCollectionManager().CreateSiteCollectionAsync(teamSiteToCreate, siteCreationOptions))
                    {
                        var web = await newSiteContext.Web.GetAsync(p => p.Url, p => p.Title);

                        var admins = await context.GetSiteCollectionManager()
                            .GetSiteCollectionAdminsAsync(teamSiteToCreate.Url);
                        Assert.AreEqual(1, admins.Count);

                        // Add a group to the created site collection
                        ConnectSiteToGroupOptions groupConnectOptions = new(teamSiteToCreate.Url, alias, web.Title);
                        CreationOptions creationOptions = new CreationOptions() { UsingApplicationPermissions = false };

                        await newSiteContext.GetSiteCollectionManager().ConnectSiteCollectionToGroupAsync(groupConnectOptions, creationOptions);

                        // load site again to see if there's a group connected
                        await newSiteContext.Site.LoadAsync(p => p.GroupId);
                        Assert.IsTrue(newSiteContext.Site.GroupId != Guid.Empty);

                        admins = await context.GetSiteCollectionManager()
                            .GetSiteCollectionAdminsAsync(teamSiteToCreate.Url);

                        // update admins
                        List<string> newAdmins = admins
                            .Where(admin => !string.IsNullOrEmpty(admin.LoginName))
                            .Select(admin => admin.LoginName)
                            .ToList();
                        // everyone claim
                        newAdmins.Add("c:0(.s|true");

                        // set admins
                        await context.GetSiteCollectionManager()
                            .SetSiteCollectionAdminsAsync(teamSiteToCreate.Url, newAdmins);

                        // Get admins again and verify if the added admin is present
                        admins = await context.GetSiteCollectionManager()
                            .GetSiteCollectionAdminsAsync(teamSiteToCreate.Url);
                        
                        Assert.IsTrue(admins.Count == 2);
                        
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                // Cleanup the created site collection
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    await context.GetSiteCollectionManager().DeleteSiteCollectionAsync(teamSiteToCreate.Url);
                }
            }
        }

        [TestMethod]
        public async Task ConnectGroupToExistingSiteUsingDelegatedPermissions()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            TeamSiteWithoutGroupOptions teamSiteToCreate = null;

            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    string alias = null;
                    if (!TestCommon.Instance.Mocking)
                    {
                        Guid tempId = Guid.NewGuid();
                        siteUrl = new Uri(
                            $"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestteamsite{tempId.ToString().Replace("-", "")}");
                        alias = $"pnpcoresdktestteamsite{tempId.ToString().Replace("-", "")}";
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            {"SiteUrl", siteUrl.ToString()}, {"Alias", alias},
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                        alias = TestManager.GetProperties(context)["Alias"];
                    }

                    teamSiteToCreate = new TeamSiteWithoutGroupOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                    };


                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                        {
                            UsingApplicationPermissions = false
                        };

                    // first create the site collection
                    using (var newSiteContext = context.GetSiteCollectionManager()
                               .CreateSiteCollection(teamSiteToCreate, siteCreationOptions))
                    {
                        var web = await newSiteContext.Web.GetAsync(p => p.Url, p => p.Title);

                        // Add a group to the created site collection
                        ConnectSiteToGroupOptions groupConnectOptions =
                            new ConnectSiteToGroupOptions(teamSiteToCreate.Url, alias, web.Title);
                        CreationOptions creationOptions = new CreationOptions() { UsingApplicationPermissions = false };
                        newSiteContext.GetSiteCollectionManager()
                            .ConnectSiteCollectionToGroup(groupConnectOptions, creationOptions);

                        // load site again to see if there's a group connected
                        await context.Site.LoadAsync(p => p.GroupId);
                        Assert.IsTrue(context.Site.GroupId != Guid.Empty);
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                // Cleanup the created site collection
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSiteCollectionManager().DeleteSiteCollection(teamSiteToCreate.Url);
                }

            }
        }

        [TestMethod]
        public async Task GetSiteCollectionAdminsGroupSite()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var admins = context.GetSiteCollectionManager().GetSiteCollectionAdmins(context.Uri);

                Assert.IsTrue(admins.Count >= 2);
            }
        }

        [TestMethod]
        public async Task GetSiteCollectionAdminsRegularSite()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var admins = context.GetSiteCollectionManager().GetSiteCollectionAdmins(context.Uri);

                Assert.IsTrue(admins.Count >= 1);
            }
        }
        
        [TestMethod]
        public async Task SetAsPrimarySiteCollectionAdminRegularSite_Async()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            CommunicationSiteOptions communicationSiteToCreate = null;

            // Create the site collection
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    if (!TestCommon.Instance.Mocking)
                    {
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsite{Guid.NewGuid().ToString().Replace("-", "")}");
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "SiteUrl", siteUrl.ToString() }
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                    }

                    communicationSiteToCreate = new CommunicationSiteOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                    };

                    SiteCreationOptions siteCreationOptions = new()
                    {
                        UsingApplicationPermissions = false
                    };

                    await context.GetSiteCollectionManager().CreateSiteCollectionAsync(communicationSiteToCreate, siteCreationOptions);
                    
                    // new admins
                    List<string> newAdmins = new List<string> 
                    {
                        // everyone claim
                        "c:0(.s|true"
                    };

                    // set admins
                    await context.GetSiteCollectionManager().SetSiteCollectionAdminsAsync(
                        communicationSiteToCreate.Url, 
                        newAdmins,
                        null,
                        CollectionUpdateOptions.AddOnly
                    );

                    // Get admins and verify if the added admin is present
                    List<ISiteCollectionAdmin> admins = await context.GetSiteCollectionManager()
                        .GetSiteCollectionAdminsAsync(communicationSiteToCreate.Url);

                    Assert.IsNotNull(admins.FirstOrDefault(p => p.LoginName == "c:0(.s|true"));
                    Assert.IsTrue(admins.Count > 1);
                    
                    await admins[1].SetAsPrimarySiteCollectionAdministratorAsync(communicationSiteToCreate.Url);
                    
                    admins = await context.GetSiteCollectionManager()
                        .GetSiteCollectionAdminsAsync(communicationSiteToCreate.Url);

                    Assert.IsTrue(admins.FirstOrDefault(p => p.LoginName == "c:0(.s|true").IsSecondaryAdmin);
                    
                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1);
                await context.GetSiteCollectionManager().DeleteSiteCollectionAsync(communicationSiteToCreate.Url);
            }
        }

        [TestMethod]
        public async Task SetAsPrimarySiteCollectionAdminRegularSiteApplicationPermission_Async()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = true;

            CommunicationSiteOptions communicationSiteToCreate = null;

            // Create the site collection
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
                {
                    // Determine the user to set as owner
                    await context.Web.LoadAsync(p => p.AssociatedOwnerGroup.QueryProperties(p => p.Users));
                    var user = context.Web.AssociatedOwnerGroup.Users.AsRequested().FirstOrDefault();

                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    string owner = user.LoginName;
                    if (!TestCommon.Instance.Mocking)
                    {
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsite{Guid.NewGuid().ToString().Replace("-", "")}");
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "SiteUrl", siteUrl.ToString() },
                            { "SiteOwner", owner }
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                        owner = TestManager.GetProperties(context)["SiteOwner"];
                    }

                    communicationSiteToCreate = new CommunicationSiteOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                        Owner = owner
                    };

                    SiteCreationOptions siteCreationOptions = new()
                    {
                        UsingApplicationPermissions = true
                    };

                    await context.GetSiteCollectionManager().CreateSiteCollectionAsync(communicationSiteToCreate, siteCreationOptions);
                    
                    // new admins
                    List<string> newAdmins = new List<string> 
                    {
                        // everyone claim
                        "c:0(.s|true"
                    };

                    // set admins
                    await context.GetSiteCollectionManager().SetSiteCollectionAdminsAsync(
                        communicationSiteToCreate.Url, 
                        newAdmins,
                        null,
                        CollectionUpdateOptions.AddOnly
                    );

                    // Get admins and verify if the added admin is present
                    List<ISiteCollectionAdmin> admins = await context.GetSiteCollectionManager()
                        .GetSiteCollectionAdminsAsync(communicationSiteToCreate.Url);

                    Assert.IsNotNull(admins.FirstOrDefault(p => p.LoginName == "c:0(.s|true"));
                    Assert.IsTrue(admins.Count > 1);
                    
                    await admins[1].SetAsPrimarySiteCollectionAdministratorAsync(communicationSiteToCreate.Url);
                    
                    admins = await context.GetSiteCollectionManager()
                        .GetSiteCollectionAdminsAsync(communicationSiteToCreate.Url);

                    Assert.IsTrue(admins.FirstOrDefault(p => p.LoginName == "c:0(.s|true").IsSecondaryAdmin);
                    
                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1);
                await context.GetSiteCollectionManager().DeleteSiteCollectionAsync(communicationSiteToCreate.Url);
            }
        }

        [TestMethod]
        public async Task SetSiteCollectionAdminsRegularSiteSetExact_Async()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            CommunicationSiteOptions communicationSiteToCreate = null;

            // Create the site collection
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    if (!TestCommon.Instance.Mocking)
                    {
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsite{Guid.NewGuid().ToString().Replace("-", "")}");
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "SiteUrl", siteUrl.ToString() }
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                    }

                    communicationSiteToCreate = new CommunicationSiteOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                    };

                    SiteCreationOptions siteCreationOptions = new()
                    {
                        UsingApplicationPermissions = false
                    };

                    await context.GetSiteCollectionManager().CreateSiteCollectionAsync(communicationSiteToCreate, siteCreationOptions);
                    
                    // new admins
                    List<string> newAdmins = new List<string> 
                    {
                        // everyone claim
                        "c:0(.s|true"
                    };

                    // set admins
                    await context.GetSiteCollectionManager().SetSiteCollectionAdminsAsync(
                        communicationSiteToCreate.Url, 
                        newAdmins,
                        null,
                        CollectionUpdateOptions.SetExact
                    );

                    // Get admins and verify if the added admin is present
                    List<ISiteCollectionAdmin> admins = await context.GetSiteCollectionManager()
                        .GetSiteCollectionAdminsAsync(communicationSiteToCreate.Url);

                    Assert.IsNotNull(admins.FirstOrDefault(p => p.LoginName == "c:0(.s|true"));
                    Assert.IsFalse(admins.FirstOrDefault(p => p.LoginName == "c:0(.s|true").IsSecondaryAdmin);
                    Assert.IsTrue(admins.Count == 1);
                    
                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1);
                await context.GetSiteCollectionManager().DeleteSiteCollectionAsync(communicationSiteToCreate.Url);
            }
        }

        [TestMethod]
        public async Task SetSiteCollectionAdminsRegularSiteAddOnly_Async()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            CommunicationSiteOptions communicationSiteToCreate = null;

            // Create the site collection
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {

                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    if (!TestCommon.Instance.Mocking)
                    {
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsite{Guid.NewGuid().ToString().Replace("-", "")}");
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "SiteUrl", siteUrl.ToString() }
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                    }

                    communicationSiteToCreate = new CommunicationSiteOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                    };


                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = false
                    };

                    await context.GetSiteCollectionManager().CreateSiteCollectionAsync(communicationSiteToCreate, siteCreationOptions);

                    // get current admins
                    var admins = await context.GetSiteCollectionManager().GetSiteCollectionAdminsAsync(communicationSiteToCreate.Url);

                    // update admins
                    List<string> newAdmins = admins
                        .Where(admin => !string.IsNullOrEmpty(admin.LoginName))
                        .Select(admin => admin.LoginName)
                        .ToList();

                    // everyone claim
                    newAdmins.Add("c:0(.s|true");

                    // set admins
                    await context.GetSiteCollectionManager().SetSiteCollectionAdminsAsync(communicationSiteToCreate.Url, newAdmins);

                    // Get admins again and verify if the added admin is present
                    admins = await context.GetSiteCollectionManager().GetSiteCollectionAdminsAsync(communicationSiteToCreate.Url);

                    Assert.IsNotNull(admins.FirstOrDefault(p => p.LoginName == "c:0(.s|true"));
                    Assert.IsTrue(admins.Count == 2);

                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSiteCollectionManager().DeleteSiteCollection(communicationSiteToCreate.Url);
                }

            }
        }

        [TestMethod]
        public async Task SetSiteCollectionAdminsRegularSiteAddOnlyApplicationPermissions_Async()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = true;

            CommunicationSiteOptions communicationSiteToCreate = null;

            // Create the site collection
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
                {
                    // Determine the user to set as owner
                    await context.Web.LoadAsync(p => p.AssociatedOwnerGroup.QueryProperties(p => p.Users));
                    var user = context.Web.AssociatedOwnerGroup.Users.AsRequested().FirstOrDefault();

                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    string owner = user.LoginName;
                    if (!TestCommon.Instance.Mocking)
                    {
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsite{Guid.NewGuid().ToString().Replace("-", "")}");
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "SiteUrl", siteUrl.ToString() },
                            { "SiteOwner", owner }
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                        owner = TestManager.GetProperties(context)["SiteOwner"];
                    }

                    communicationSiteToCreate = new CommunicationSiteOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                        Owner = owner
                    };

                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = true
                    };

                    await context.GetSiteCollectionManager().CreateSiteCollectionAsync(communicationSiteToCreate, siteCreationOptions);

                    // get current admins
                    var admins = await context.GetSiteCollectionManager().GetSiteCollectionAdminsAsync(communicationSiteToCreate.Url);

                    // update admins
                    List<string> newAdmins = admins
                        .Where(admin => !string.IsNullOrEmpty(admin.LoginName))
                        .Select(admin => admin.LoginName)
                        .ToList();

                    // everyone claim
                    newAdmins.Add("c:0(.s|true");

                    // set admins
                    await context.GetSiteCollectionManager().SetSiteCollectionAdminsAsync(
                        communicationSiteToCreate.Url, 
                        newAdmins, 
                        null, 
                        CollectionUpdateOptions.AddOnly);

                    // Get admins again and verify if the added admin is present
                    admins = await context.GetSiteCollectionManager().GetSiteCollectionAdminsAsync(communicationSiteToCreate.Url);

                    Assert.IsNotNull(admins.FirstOrDefault(p => p.LoginName == "c:0(.s|true"));

                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    await context.GetSiteCollectionManager().DeleteSiteCollectionAsync(communicationSiteToCreate.Url);
                }
                TestCommon.Instance.UseApplicationPermissions = false;
            }
        }

        [TestMethod]
        public async Task SetSiteCollectionAdminsRegularSiteSetExactApplicationPermissions_Async()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = true;

            CommunicationSiteOptions communicationSiteToCreate = null;

            // Create the site collection
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
                {
                    // Determine the user to set as owner
                    await context.Web.LoadAsync(p => p.AssociatedOwnerGroup.QueryProperties(p => p.Users));
                    var user = context.Web.AssociatedOwnerGroup.Users.AsRequested().FirstOrDefault();

                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    string owner = user.LoginName;
                    if (!TestCommon.Instance.Mocking)
                    {
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsite{Guid.NewGuid().ToString().Replace("-", "")}");
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "SiteUrl", siteUrl.ToString() },
                            { "SiteOwner", owner }
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                        owner = TestManager.GetProperties(context)["SiteOwner"];
                    }

                    communicationSiteToCreate = new CommunicationSiteOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                        Owner = owner
                    };

                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = true
                    };

                    await context.GetSiteCollectionManager().CreateSiteCollectionAsync(communicationSiteToCreate, siteCreationOptions);

                    // new admins
                    List<string> newAdmins = new List<string> 
                    {
                        // everyone claim
                        "c:0(.s|true"
                    };

                    // set admins
                    await context.GetSiteCollectionManager().SetSiteCollectionAdminsAsync(
                        communicationSiteToCreate.Url, 
                        newAdmins, 
                        null, 
                        CollectionUpdateOptions.SetExact);

                    // Get admins and verify if the added admin is present
                    List<ISiteCollectionAdmin> admins = await context.GetSiteCollectionManager().GetSiteCollectionAdminsAsync(communicationSiteToCreate.Url);

                    Assert.IsNotNull(admins.FirstOrDefault(p => p.LoginName == "c:0(.s|true"));
                    Assert.IsFalse(admins.FirstOrDefault(p => p.LoginName == "c:0(.s|true").IsSecondaryAdmin);
                    Assert.IsTrue(admins.Count == 1);

                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    await context.GetSiteCollectionManager().DeleteSiteCollectionAsync(communicationSiteToCreate.Url);
                }
                TestCommon.Instance.UseApplicationPermissions = false;
            }
        }

        [TestMethod]
        public async Task SetSiteCollectionWithTimeZome_Async()
        {
            //Time zone object to test with
            var selectedTimeZone = TimeZone.UTCPLUS0200_JERUSALEM;

            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            CommunicationSiteOptions communicationSiteToCreate = null;

            // Create the site collection
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    if (!TestCommon.Instance.Mocking)
                    {
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsite{Guid.NewGuid().ToString().Replace("-", "")}");
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "SiteUrl", siteUrl.ToString() }
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                    }

                    communicationSiteToCreate = new CommunicationSiteOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                        TimeZone = selectedTimeZone
                    };

                    SiteCreationOptions siteCreationOptions = new()
                    {
                        UsingApplicationPermissions = false
                    };

                    var siteProperties = context.GetSiteCollectionManager().GetSiteCollectionProperties(context.Uri);

                    //Validating time zone set as expected
                    Assert.IsTrue(siteProperties.TimeZoneId == selectedTimeZone);

                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1);
                await context.GetSiteCollectionManager().DeleteSiteCollectionAsync(communicationSiteToCreate.Url);
            }
        }

        [TestMethod]
        public async Task SetSiteCollectionAdminsGroupSite()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            TeamSiteOptions teamSiteToCreate = null;

            // Create the site collection
            Uri createdSiteCollection = null;
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {

                    // Persist the used site url as we need to have the same url when we run an offline test
                    string alias;
                    if (!TestCommon.Instance.Mocking)
                    {
                        alias = $"pnpcoresdktestteamsite{Guid.NewGuid().ToString().Replace("-", "")}";
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "Alias", alias }
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        alias = TestManager.GetProperties(context)["Alias"];
                    }

                    teamSiteToCreate = new TeamSiteOptions(alias, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                        IsPublic = true,
                    };


                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = false
                    };

                    var newSiteContext = context.GetSiteCollectionManager().CreateSiteCollection(teamSiteToCreate, siteCreationOptions);
                    createdSiteCollection = newSiteContext.Uri;

                    // get current admins
                    var admins = context.GetSiteCollectionManager().GetSiteCollectionAdmins(createdSiteCollection);

                    // update admins
                    List<string> newAdmins = admins
                        .Where(admin => !string.IsNullOrEmpty(admin.LoginName))
                        .Select(admin => admin.LoginName)
                        .ToList();

                    // everyone claim
                    newAdmins.Add("c:0(.s|true");

                    //Also set admin via group owner membership
                    List<Guid> newGroupOwners = new List<Guid>();
                    foreach (var groupOwner in admins.Where(p => p.IsMicrosoft365GroupOwner == true))
                    {
                        newGroupOwners.Add(groupOwner.Id);
                    }

                    // set admins
                    context.GetSiteCollectionManager().SetSiteCollectionAdmins(createdSiteCollection, newAdmins, newGroupOwners);

                    // Get admins again and verify if the added admin is present
                    admins = context.GetSiteCollectionManager().GetSiteCollectionAdmins(createdSiteCollection);

                    Assert.IsNotNull(admins.FirstOrDefault(p => p.LoginName == "c:0(.s|true"));
                    Assert.IsTrue(admins.Count >= 2);

                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSiteCollectionManager().DeleteSiteCollection(createdSiteCollection);
                }

            }
        }

        [TestMethod]
        public async Task SiteCollectionExistsTrue()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var exists = context.GetSiteCollectionManager().SiteExists(context.Uri);

                Assert.IsTrue(exists);
            }
        }

        [TestMethod]
        public async Task SiteCollectionExistsFalse()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var exists = context.GetSiteCollectionManager().SiteExists(new Uri($"https://{context.Uri.DnsSafeHost}/sites/idonotexist12345"));

                Assert.IsFalse(exists);
            }
        }


        #region Handle input exceptions
        [TestMethod]
        public async Task HandleExceptions()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    context.GetSiteCollectionManager().GetSiteCollectionAdmins(null);
                });

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    context.GetSiteCollectionManager().SetSiteCollectionAdmins(null);
                });

            }
        }
        #endregion

    }
}
