using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Site class, write your custom code here
    /// </summary>
    [SharePointType("SP.Site", Uri = "_api/Site")]
    [GraphType(Get = "sites/{hostname}:{serverrelativepath}")]
    internal sealed class Site : BaseDataModel<ISite>, ISite
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
            var apiCall = new ApiCall("_api/SP.CompliancePolicy.SPPolicyStoreProxy.GetAvailableTagsForSite(siteUrl=@a1)?@a1='{Site.Url}'", ApiType.SPORest);
            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {

                var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

                if (json.TryGetProperty("value", out JsonElement getAvailableTagsForSite))
                {
                    var returnTags = new List<IComplianceTag>();
                    var tags = JsonSerializer.Deserialize<IEnumerable<ComplianceTag>>(getAvailableTagsForSite.GetRawText(), PnPConstants.JsonSerializer_PropertyNameCaseInsensitiveTrue);
                    foreach (var tag in tags)
                    {
                        returnTags.Add(tag);
                    }
                    return returnTags;
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

        public IHubSite RegisterHubSite()
        {
            return RegisterHubSiteAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Unregisters the current site as a primary hub site
        /// </summary>
        public async Task<bool> UnregisterHubSiteAsync()
        {
            var result = false;

            await EnsurePropertiesAsync(p => p.IsHubSite).ConfigureAwait(false);

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

        public bool UnregisterHubSite()
        {
            return UnregisterHubSiteAsync().GetAwaiter().GetResult();
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

        public bool JoinHubSite(Guid hubSiteId)
        {
            return JoinHubSiteAsync(hubSiteId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Disassociates the current site to a primary hub site
        /// </summary>
        public async Task<bool> UnJoinHubSiteAsync()
        {
            return await JoinHubSiteAsync(Guid.Empty).ConfigureAwait(false);
        }

        public bool UnJoinHubSite()
        {
            return UnJoinHubSiteAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets hubsite data from the current site OR another specified hub site ID
        /// </summary>
        /// <param name="id">Hub Site Guid</param>
        /// <returns></returns>
        public async Task<IHubSite> GetHubSiteDataAsync(Guid? id)
        {
            IHubSite hubSite = new HubSite()
            {
                PnPContext = PnPContext,
                Id = id ?? HubSiteId
            };

            var hubResult = await hubSite.GetAsync().ConfigureAwait(false);

            return hubResult;
        }

        public IHubSite GetHubSiteData(Guid? id)
        {
            return GetHubSiteDataAsync(id).GetAwaiter().GetResult();
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

        public async Task<IEnumerableBatchResult<IChange>> GetChangesBatchAsync(Batch batch, ChangeQueryOptions query)
        {
            var apiCall = ChangeCollectionHandler.GetApiCall(this, query);
            apiCall.RawEnumerableResult = new List<IChange>();
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                var batchFirstRequest = batch.Requests.First().Value;
                ApiCallResponse response = new ApiCallResponse(apiCall, json, System.Net.HttpStatusCode.OK, batchFirstRequest.Id, batchFirstRequest.ResponseHeaders);
                ((List<IChange>)apiCall.RawEnumerableResult).AddRange(ChangeCollectionHandler.Deserialize(response, this, PnPContext).ToList());
            };

            var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);

            return new BatchEnumerableBatchResult<IChange>(batch, batchRequest.Id, (IReadOnlyList<IChange>)apiCall.RawEnumerableResult);
        }

        public IEnumerableBatchResult<IChange> GetChangesBatch(Batch batch, ChangeQueryOptions query)
        {
            return GetChangesBatchAsync(batch, query).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<IChange>> GetChangesBatchAsync(ChangeQueryOptions query)
        {
            return await GetChangesBatchAsync(PnPContext.CurrentBatch, query).ConfigureAwait(false);
        }

        public IEnumerableBatchResult<IChange> GetChangesBatch(ChangeQueryOptions query)
        {
            return GetChangesBatchAsync(query).GetAwaiter().GetResult();
        }

        #endregion

        #region Home site
        /// <summary>
        /// Checks if current site is a HomeSite
        /// </summary>
        public async Task<bool> IsHomeSiteAsync()
        {
            var apiCall = new ApiCall($"_api/SP.SPHSite/Details", ApiType.SPORest);
            var result = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(result.Json))
            {
                HomeSiteReference siteReference = JsonSerializer.Deserialize<HomeSiteReference>(result.Json);
                return siteReference.SiteId != null ? Guid.Parse(siteReference.SiteId) == Id : false;
            }

            return false;
        }

        public bool IsHomeSite()
        {
            return IsHomeSiteAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region Copy / Move Jobs

        public async Task<IList<ICopyMigrationInfo>> CreateCopyJobsAsync(string[] exportObjectUris, string destinationUri, CopyMigrationOptions options, bool waitUntilFinished = false, int waitAfterStatusCheck = 1)
        {
            var apiCall = CreateCopyJobCall(exportObjectUris, destinationUri, options);
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

                if (json.TryGetProperty("value", out JsonElement copyJobs))
                {
                    var returnJobs = new List<ICopyMigrationInfo>();
                    var copyJobsEnumerable = JsonSerializer.Deserialize<IEnumerable<CopyMigrationInfo>>(copyJobs.GetRawText(), PnPConstants.JsonSerializer_PropertyNameCaseInsensitiveTrue);
                    foreach (var copyJob in copyJobsEnumerable)
                    {
                        returnJobs.Add(copyJob);
                    }

                    if (waitUntilFinished)
                    {
                        await EnsureCopyJobHasFinishedAsync(returnJobs, waitAfterStatusCheck).ConfigureAwait(false);
                    }
                    
                    return returnJobs;
                }

            }

            return null;
        }

        public IList<ICopyMigrationInfo> CreateCopyJobs(string[] exportObjectUris, string destinationUri, CopyMigrationOptions options, bool waitUntilFinished = false, int waitAfterStatusCheck = 1)
        {
            return CreateCopyJobsAsync(exportObjectUris, destinationUri, options, waitUntilFinished, waitAfterStatusCheck).GetAwaiter().GetResult();
        }

        public async Task<ICopyJobProgress> GetCopyJobProgressAsync(ICopyMigrationInfo copyMigrationInfo)
        {
            var apiCall = GetCopyJobProgressCall(copyMigrationInfo);
            var result = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            return JsonSerializer.Deserialize<CopyJobProgress>(result.Json);
        }

        public ICopyJobProgress GetCopyJobProgress(ICopyMigrationInfo copyMigrationInfo)
        {
            return GetCopyJobProgressAsync(copyMigrationInfo).GetAwaiter().GetResult();
        }

        private static ApiCall CreateCopyJobCall(string[] exportObjectUris, string destinationUri, CopyMigrationOptions options)
        {
            var requestBody = new
            {
                exportObjectUris,
                destinationUri,
                options
            }.AsExpando();

            return new ApiCall("_api/site/CreateCopyJobs", ApiType.SPORest, jsonBody: JsonSerializer.Serialize(requestBody, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues));
        }

        private static ApiCall GetCopyJobProgressCall(ICopyMigrationInfo copyMigrationInfo)
        {
            var requestBody = new
            {
                copyJobInfo = new 
                {
                    copyMigrationInfo.EncryptionKey,
                    copyMigrationInfo.JobId,
                    copyMigrationInfo.JobQueueUri,
                }
            }.AsExpando();

            return new ApiCall("_api/site/GetCopyJobProgress", ApiType.SPORest, jsonBody: JsonSerializer.Serialize(requestBody, typeof(ExpandoObject)));
        }

        public async Task EnsureCopyJobHasFinishedAsync(IList<ICopyMigrationInfo> copyMigrationInfos, int waitAfterStatusCheck = 1)
        {
            while (copyMigrationInfos.Count > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(waitAfterStatusCheck)).ConfigureAwait(false);

                foreach (var copyMigrationInfo in copyMigrationInfos.ToList())
                {
                    var progress = await GetCopyJobProgressAsync(copyMigrationInfo).ConfigureAwait(false);
                    if (progress.JobState == MigrationJobState.None)
                    {
                        copyMigrationInfos.Remove(copyMigrationInfo);
                    }
                }
            }
        }

        public void EnsureCopyJobHasFinished(IList<ICopyMigrationInfo> copyMigrationInfos, int waitAfterStatusCheck = 1)
        {
            EnsureCopyJobHasFinishedAsync(copyMigrationInfos, waitAfterStatusCheck).GetAwaiter().GetResult();
        }

        #endregion

        #endregion
    }
}
