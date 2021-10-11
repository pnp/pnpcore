using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Site class, write your custom code here
    /// </summary>
    [SharePointType("SP.Site", Uri = "_api/Site")]
    [GraphType(Get = "sites/{hostname}:{serverrelativepath}")]
    internal partial class Site : BaseDataModel<ISite>, ISite
    {
        #region Construction
        public Site()
        {
        }
        #endregion

        #region Properties
        [GraphProperty("sharepointIds", JsonPath = "siteId")]
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid GroupId { get => GetValue<Guid>(); set => SetValue(value); }

        public Uri Url { get => GetValue<Uri>(); set => SetValue(value); }

        public string Classification { get => GetValue<string>(); set => SetValue(value); }

        public IWeb RootWeb { get => GetModelValue<IWeb>(); set => SetModelValue(value); }

        private IWebCollection allWebs;

        // Note: AllWebs is no real property in SharePoint, so expand/select do not return a thing...
        // TODO: evaluate why we need this
        public IWebCollection AllWebs
        {
            get
            {
                if (allWebs == null)
                {
                    allWebs = new WebCollection(PnPContext, this, "AllWebs");
                }

                return allWebs;
            }
            set
            {
                allWebs = value;
            }
        }

        public bool SocialBarOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public IFeatureCollection Features { get => GetModelCollectionValue<IFeatureCollection>(); }

        public SearchBoxInNavBar SearchBoxInNavBar { get => GetValue<SearchBoxInNavBar>(); set => SetValue(value); }

        public bool AllowCreateDeclarativeWorkflow { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowDesigner { get => GetValue<bool>(); set => SetValue(value); }

        public int AllowExternalEmbeddingWrapper { get => GetValue<int>(); set => SetValue(value); }

        public bool AllowMasterPageEditing { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowRevertFromTemplate { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowSaveDeclarativeWorkflowAsTemplate { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowSavePublishDeclarativeWorkflow { get => GetValue<bool>(); set => SetValue(value); }

        public int AuditLogTrimmingRetention { get => GetValue<int>(); set => SetValue(value); }

        public bool CanSyncHubSitePermissions { get => GetValue<bool>(); set => SetValue(value); }

        public Guid ChannelGroupId { get => GetValue<Guid>(); set => SetValue(value); }

        public bool CommentsOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableAppViews { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableCompanyWideSharingLinks { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableFlows { get => GetValue<bool>(); set => SetValue(value); }

        public bool ExternalSharingTipsEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public int ExternalUserExpirationInDays { get => GetValue<int>(); set => SetValue(value); }

        public string GeoLocation { get => GetValue<string>(); set => SetValue(value); }

        public Guid HubSiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public bool IsHubSite { get => GetValue<bool>(); set => SetValue(value); }

        public string LockIssue { get => GetValue<string>(); set => SetValue(value); }

        public int MaxItemsPerThrottledOperation { get => GetValue<int>(); set => SetValue(value); }

        public bool ReadOnly { get => GetValue<bool>(); set => SetValue(value); }

        public Guid RelatedGroupId { get => GetValue<Guid>(); set => SetValue(value); }

        public IRecycleBinItemCollection RecycleBin { get => GetModelCollectionValue<IRecycleBinItemCollection>(); }

        public string SearchBoxPlaceholderText { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("SensitivityLabelInfo", JsonPath = "Id")]
        public Guid SensitivityLabelId { get => GetValue<Guid>(); set => SetValue(value); }

        [SharePointProperty("SensitivityLabelInfo", JsonPath = "DisplayName")]
        public string SensitivityLabel { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ShareByEmailEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShareByLinkEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShowPeoplePickerSuggestionsForGuestUsers { get => GetValue<bool>(); set => SetValue(value); }

        public string StatusBarLink { get => GetValue<string>(); set => SetValue(value); }

        public string StatusBarText { get => GetValue<string>(); set => SetValue(value); }

        public bool ThicketSupportDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool TrimAuditLog { get => GetValue<bool>(); set => SetValue(value); }

        public IUserCustomActionCollection UserCustomActions { get => GetModelCollectionValue<IUserCustomActionCollection>(); }

        public ISharePointGroup HubSiteSynchronizableVisitorGroup { get => GetModelValue<ISharePointGroup>(); set => SetModelValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }
        #endregion

        #region Extension methods

        #region Get compliance tags

        public IEnumerable<IComplianceTag> GetAvailableComplianceTags()
        {
            return GetAvailableComplianceTagsAsync().GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<IComplianceTag>> GetAvailableComplianceTagsAsync()
        {
            var apiCall = new ApiCall("_api/SP.CompliancePolicy.SPPolicyStoreProxy.GetAvailableTagsForSite(siteUrl='{Site.Url}')", ApiType.SPORest);
            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json).GetProperty("d");

                if (json.TryGetProperty("GetAvailableTagsForSite", out JsonElement getAvailableTagsForSite))
                {
                    if (getAvailableTagsForSite.TryGetProperty("results", out JsonElement result))
                    {
                        var returnTags = new List<IComplianceTag>();
                        var tags = JsonSerializer.Deserialize<IEnumerable<ComplianceTag>>(result.GetRawText(), PnPConstants.JsonSerializer_PropertyNameCaseInsensitiveTrue);
                        foreach (var tag in tags)
                        {
                            returnTags.Add(tag);
                        }
                        return returnTags;
                    }
                }
            }

            return new List<IComplianceTag>();
        }

        #endregion
        
        #region Hub Site

        /// <summary>
        /// Registers the current site as a primary hub site
        /// </summary>
        public async Task<IHubSite> RegisterHubSiteAsync()
        {

            await EnsurePropertiesAsync(p => p.IsHubSite).ConfigureAwait(false);

            HubSite hubSite = new HubSite()
            {
                PnPContext = PnPContext
            };

            if (!IsHubSite)
            {
                var apiCall = new ApiCall($"_api/site/RegisterHubSite", ApiType.SPORest);
                await hubSite.RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_SiteIsAlreadyHubSite);
            }

            return hubSite;
        }

        /// <summary>
        /// Unregisters the current site as a primary hub site
        /// </summary>
        public async Task<bool> UnregisterHubSiteAsync()
        {
            var result = false;

            await EnsurePropertiesAsync(p=>p.IsHubSite).ConfigureAwait(false);

            if (IsHubSite)
            {
                var apiCall = new ApiCall($"_api/Site/UnRegisterHubSite", ApiType.SPORest);
                var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
                result = response.StatusCode == System.Net.HttpStatusCode.OK;
            }
            else
            {
                throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_SiteIsNotAHubSite);
            }

            return result;
        }

        /// <summary>
        /// Associates the current site to a primary hub site
        /// </summary>
        public async Task<bool> JoinHubSiteAsync(Guid hubSiteId)
        {
            var result = false;

            await EnsurePropertiesAsync(p => p.IsHubSite).ConfigureAwait(false);

            if (!IsHubSite)
            {
                var apiCall = new ApiCall($"_api/Site/JoinHubSite('{hubSiteId}')", ApiType.SPORest);
                var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
                result = response.StatusCode == System.Net.HttpStatusCode.OK;
            }
            else
            {
                throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_SiteIsAlreadyPartOfAHubSite);
            }

            return result;
        }


        /// <summary>
        /// Disassociates the current site to a primary hub site
        /// </summary>
        public async Task<bool> UnJoinHubSiteAsync()
        {
            return await JoinHubSiteAsync(Guid.Empty).ConfigureAwait(false);
        }


        /// <summary>
        /// Gets hubsite data from the current site OR another specified hub site ID
        /// </summary>
        /// <param name="Id">Hub Site Guid</param>
        /// <returns></returns>
        public async Task<IHubSite> GetHubSiteData(Guid? Id)
        {
            IHubSite hubSite = new HubSite()
            {
                PnPContext = PnPContext,
                Id = Id ?? HubSiteId
            };

            var hubResult = await hubSite.GetAsync().ConfigureAwait(false);

            return hubResult;
        }

        #endregion

        #region Get Changes

        public async Task<IList<IChange>> GetChangesAsync(ChangeQueryOptions query)
        {
            var apiCall = ChangeCollectionHandler.GetApiCall(this, query);
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            return ChangeCollectionHandler.Deserialize(response, this, PnPContext).ToList();
        }

        public IList<IChange> GetChanges(ChangeQueryOptions query)
        {
            return GetChangesAsync(query).GetAwaiter().GetResult();
        }

        #endregion

        #endregion
    }
}
