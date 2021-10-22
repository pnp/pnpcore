using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                    Assert.IsTrue(siteProperties.AuthContextStrength == null);
                    Assert.IsTrue(siteProperties.AuthenticationContextName == null);
                    Assert.IsTrue(siteProperties.BlockDownloadLinksFileType == Model.SharePoint.BlockDownloadLinksFileTypes.WebPreviewableFiles);
                    Assert.IsFalse(siteProperties.CommentsOnSitePagesDisabled);
                    Assert.IsTrue(siteProperties.CompatibilityLevel == 15);
                    Assert.IsTrue(siteProperties.ConditionalAccessPolicy == Model.SharePoint.SPOConditionalAccessPolicyType.AllowFullAccess);
                    Assert.IsTrue(siteProperties.DefaultLinkPermission == Model.SharePoint.SharingPermissionType.None);
                    Assert.IsFalse(siteProperties.DefaultLinkToExistingAccess);
                    Assert.IsFalse(siteProperties.DefaultLinkToExistingAccessReset);
                    Assert.IsTrue(siteProperties.DefaultSharingLinkType == Model.SharePoint.SharingLinkType.None);
                    Assert.IsTrue(siteProperties.DenyAddAndCustomizePages == Model.SharePoint.DenyAddAndCustomizePagesStatus.Enabled);
                    Assert.IsTrue(siteProperties.Description == context.Web.Description);
                    Assert.IsTrue(siteProperties.DisableAppViews == Model.SharePoint.AppViewsPolicy.NotDisabled);
                    Assert.IsTrue(siteProperties.DisableCompanyWideSharingLinks == Model.SharePoint.CompanyWideSharingLinksPolicy.NotDisabled);
                    Assert.IsTrue(siteProperties.DisableFlows == Model.SharePoint.FlowsPolicy.NotDisabled);
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
                    Assert.IsTrue(siteProperties.RelatedGroupId == context.Site.GroupId);
                    Assert.IsTrue(siteProperties.RestrictedToRegion == Model.SharePoint.RestrictedToRegion.Unknown);
                    Assert.IsTrue(siteProperties.SensitivityLabel == System.Guid.Empty);
                    Assert.IsTrue(siteProperties.SensitivityLabel2 == "");
                    Assert.IsFalse(siteProperties.SetOwnerWithoutUpdatingSecondaryAdmin);
                    Assert.IsTrue(siteProperties.SharingAllowedDomainList == "");
                    Assert.IsTrue(siteProperties.SharingBlockedDomainList == "");
                    Assert.IsTrue(siteProperties.SharingCapability == Model.SharePoint.SharingCapabilities.ExternalUserSharingOnly);
                    Assert.IsTrue(siteProperties.SharingDomainRestrictionMode == Model.SharePoint.SharingDomainRestrictionModes.None);
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


    }
}
