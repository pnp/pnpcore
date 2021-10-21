using Microsoft.Extensions.Logging;
using PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal static class SiteCollectionCreator
    {
        internal static async Task<PnPContext> CreateSiteCollectionAsync(PnPContext context, CommonSiteOptions siteToCreate, SiteCreationOptions creationOptions)
        {
            // Provide default creation options as input
            creationOptions = await EnsureSiteCreationOptionsAsync(context, creationOptions).ConfigureAwait(false);

            if (siteToCreate is ClassicSiteOptions classicSite)
            {
                return await CreateClassicSiteAsync(context, classicSite, creationOptions).ConfigureAwait(false);
            }
            else if (siteToCreate is TeamSiteOptions teamSite)
            {
                return await CreateTeamSiteAsync(context, teamSite, creationOptions).ConfigureAwait(false);
            }
            else if (siteToCreate is CommonNoGroupSiteOptions noGroupSite)
            {
                return await CreateCommonNoGroupSiteAsync(context, noGroupSite, creationOptions).ConfigureAwait(false);
            }

            throw new ArgumentException("Provided site options are not implemented");
        }

        private static async Task<PnPContext> CreateCommonNoGroupSiteAsync(PnPContext context, CommonNoGroupSiteOptions siteToCreate, SiteCreationOptions creationOptions)
        {

            if (string.IsNullOrEmpty(siteToCreate.Owner) && creationOptions.UsingApplicationPermissions.Value)
            {
                throw new ClientException(ErrorType.Unsupported, "You need to set an owner when using Application permissions to create a communicaiton site");
            }

            var payload = BuildBaseCommonNoGroupSiteRequestPayload(siteToCreate);

            var siteDesignId = GetSiteDesignId(siteToCreate);
            if (siteDesignId != Guid.Empty)
            {
                payload.Add("SiteDesignId", siteDesignId);

                // As per https://github.com/SharePoint/sp-dev-docs/issues/4810 the WebTemplateExtensionId property
                // is what currently drives the application of a custom site design during the creation of a modern site.
                payload["WebTemplateExtensionId"] = siteDesignId;
            }

            payload.Add("HubSiteId", siteToCreate.HubSiteId);

            // Sensitivity labels have replaced classification (see https://docs.microsoft.com/en-us/microsoft-365/compliance/sensitivity-labels-teams-groups-sites?view=o365-worldwide#classic-azure-ad-group-classification)
            // once enabled. Therefore we prefer setting a sensitivity label id over classification when specified.
            if (siteToCreate.SensitivityLabelId != Guid.Empty)
            {
                payload.Add("SensitivityLabel", siteToCreate.SensitivityLabelId);
            }
            else
            {
                payload["Classification"] = siteToCreate.Classification ?? "";
            }

            return await CreateSiteUsingSpoRestImplementationAsync(context, true, payload, creationOptions).ConfigureAwait(false);
        }

        private static async Task<PnPContext> CreateTeamSiteAsync(PnPContext context, TeamSiteOptions siteToCreate, SiteCreationOptions creationOptions)
        {

            // If we're using application permissions we use Microsoft Graph to create the site
            if (creationOptions.UsingApplicationPermissions.Value)
            {
                //TODO: implement group creation functionality under Microsoft365 and use it here
                throw new NotSupportedException("Creating Team sites using application permissions is not yet supported in this preview version");
            }
            else
            {
                var creationOptionsValues = new List<string>();
                Dictionary<string, object> payload = new Dictionary<string, object>
                {
                    { "displayName", siteToCreate.DisplayName },
                    { "alias", siteToCreate.Alias },
                    { "isPublic", siteToCreate.IsPublic }
                };

                var optionalParams = new Dictionary<string, object>
                {
                    { "Description", siteToCreate.Description ?? "" }
                };

                // Sensitivity labels have replaced classification (see https://docs.microsoft.com/en-us/microsoft-365/compliance/sensitivity-labels-teams-groups-sites?view=o365-worldwide#classic-azure-ad-group-classification)
                // once enabled. Therefore we prefer setting a sensitivity label id over classification when specified. Also note that for setting sensitivity labels on 
                // group connected sites one needs to have at least one Azure AD P1 license. See https://docs.microsoft.com/en-us/azure/active-directory/enterprise-users/groups-assign-sensitivity-labels
                if (siteToCreate.SensitivityLabelId != Guid.Empty)
                {
                    creationOptionsValues.Add($"SensitivityLabel:{siteToCreate.SensitivityLabelId}");
                }
                else
                {
                    optionalParams.Add("Classification", siteToCreate.Classification ?? "");
                }

                if (siteToCreate.SiteDesignId.HasValue)
                {
                    creationOptionsValues.Add($"implicit_formula_292aa8a00786498a87a5ca52d9f4214a_{siteToCreate.SiteDesignId.Value.ToString("D").ToLower()}");
                }
                if (siteToCreate.Language != Language.Default)
                {
                    creationOptionsValues.Add($"SPSiteLanguage:{(int)siteToCreate.Language}");
                }
                if (!string.IsNullOrEmpty(siteToCreate.SiteAlias))
                {
                    creationOptionsValues.Add($"SiteAlias:{siteToCreate.SiteAlias}");
                }
                creationOptionsValues.Add($"HubSiteId:{siteToCreate.HubSiteId}");

                if (siteToCreate.Owners != null && siteToCreate.Owners.Length > 0)
                {
                    optionalParams.Add("Owners", siteToCreate.Owners);
                }
                if (siteToCreate.PreferredDataLocation.HasValue)
                {
                    optionalParams.Add("PreferredDataLocation", siteToCreate.PreferredDataLocation.Value.ToString());
                }

                if (creationOptionsValues.Any())
                {
                    var creationOptionsValuesBody = new
                    {
                        __metadata = new { type = "Collection(Edm.String)" },
                        results = creationOptionsValues
                    }.AsExpando();
                    optionalParams.Add("CreationOptions", creationOptionsValuesBody);
                }

                payload.Add("optionalParams", optionalParams);

                // Delegated permissions can use the SharePoint endpoints for site collection creation
                return await CreateSiteUsingSpoRestImplementationAsync(context, false, payload, creationOptions).ConfigureAwait(false);
            }
        }

        private static async Task<PnPContext> CreateClassicSiteAsync(PnPContext context, ClassicSiteOptions siteToCreate, SiteCreationOptions creationOptions)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false))
            {
                string owner = siteToCreate.Owner;
                var splitOwner = owner.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitOwner.Length == 3)
                {
                    owner = splitOwner[2];
                }

                List<IRequest<object>> csomRequests = new List<IRequest<object>>
                {
                    new CreateSiteRequest(siteToCreate.Url, siteToCreate.Title, owner, siteToCreate.WebTemplate, (int)siteToCreate.Language, (int)siteToCreate.TimeZone)
                };

                var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);

                SpoOperation op = result.ApiCall.CSOMRequests[0].Result as SpoOperation;

                if (!op.IsComplete)
                {
                    await SiteCollectionManagement.WaitForSpoOperationCompleteAsync(tenantAdminContext, op).ConfigureAwait(false);
                }

                return await context.CloneAsync(siteToCreate.Url).ConfigureAwait(false);
            }
        }

        private static async Task<SiteCreationOptions> EnsureSiteCreationOptionsAsync(PnPContext context, SiteCreationOptions creationOptions)
        {
            if (creationOptions == null)
            {
                creationOptions = new SiteCreationOptions();
            }

            // Ensure there's a value set for UsingApplicationPermissions
            if (!creationOptions.UsingApplicationPermissions.HasValue)
            {
                creationOptions.UsingApplicationPermissions = await context.GetMicrosoft365Admin().AccessTokenUsesApplicationPermissionsAsync().ConfigureAwait(false);
            }

            // Configure the defaults for the status check
            if (!creationOptions.MaxStatusChecks.HasValue)
            {
                creationOptions.MaxStatusChecks = 12;
            }
            if (!creationOptions.WaitAfterStatusCheck.HasValue)
            {
                creationOptions.WaitAfterStatusCheck = 10;
            }

            // Configure the defaults for the wait on async provisioning complete
            if (!creationOptions.MaxAsyncProvisioningStatusChecks.HasValue)
            {
                creationOptions.MaxAsyncProvisioningStatusChecks = 80;
            }
            if (!creationOptions.WaitAfterAsyncProvisioningStatusCheck.HasValue)
            {
                creationOptions.WaitAfterAsyncProvisioningStatusCheck = 15;
            }

            return creationOptions;
        }

        private static async Task<PnPContext> CreateSiteUsingSpoRestImplementationAsync(PnPContext context, bool usingSpSiteManager, Dictionary<string, object> payload, SiteCreationOptions creationOptions)
        {
            string apiCall;
            string statusProperty;
            string body;

            if (usingSpSiteManager)
            {
                apiCall = $"_api/SPSiteManager/Create";
                statusProperty = "Create";
                var json = new { request = payload }.AsExpando();
                body = JsonSerializer.Serialize(json, typeof(ExpandoObject));
            }
            else
            {
                apiCall = $"_api/GroupSiteManager/CreateGroupEx";
                statusProperty = "CreateGroupEx";
                body = JsonSerializer.Serialize(payload, PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase);
            }

            var result = await (context.Web as Web).RawRequestAsync(new ApiCall(apiCall, ApiType.SPORest, body), HttpMethod.Post).ConfigureAwait(false);

            var responseJson = JsonSerializer.Deserialize<JsonElement>(result.Json);

            #region Json Response
            /*
            Success => SiteStatus = 2
            Provisioning => SiteStatus = 1
            Error => all other values

            {
                "d": {
                    "Create": {
                        "__metadata": {
                            "type": "Microsoft.SharePoint.Portal.SPSiteCreationResponse"
                        },
                        "SiteId": "12befc2b-4c17-46a4-985e-530e6745cf35",
                        "SiteStatus": 2,
                        "SiteUrl": "https://bertonline.sharepoint.com/sites/pnpcoresdktestcommsite1"
                    }
                }
            }

            */
            #endregion

            int siteStatus = responseJson.GetProperty("d").GetProperty(statusProperty).GetProperty("SiteStatus").GetInt32();

            PnPContext responseContext;
            if (siteStatus == 2)
            {
                // Site creation succeeded
                responseContext = await context.CloneAsync(new Uri(responseJson.GetProperty("d").GetProperty(statusProperty).GetProperty("SiteUrl").ToString())).ConfigureAwait(false);
            }
            else if (siteStatus == 1)
            {
                Guid groupId = Guid.Empty;
                if (!usingSpSiteManager)
                {
                    groupId = responseJson.GetProperty("d").GetProperty(statusProperty).GetProperty("GroupId").GetGuid();
                }

                // Site creation in progress, let's wait for it to finish
                responseContext = await VerifySiteStatusAsync(context, !usingSpSiteManager ? groupId.ToString() : payload["Url"].ToString(),
                    usingSpSiteManager, creationOptions.MaxStatusChecks.Value, creationOptions.WaitAfterStatusCheck.Value).ConfigureAwait(false);
            }
            else
            {
                // Something went wrong
                throw new ClientException(ErrorType.SharePointRestServiceError, string.Format(PnPCoreAdminResources.Exception_SiteCreation, payload["Url"].ToString(), siteStatus));
            }

            // Apply our "wait" strategy
            if (creationOptions.WaitAfterCreation.HasValue && creationOptions.WaitAfterCreation.Value > 0)
            {
                await responseContext.WaitAsync(TimeSpan.FromSeconds(creationOptions.WaitAfterCreation.Value)).ConfigureAwait(false);
            }
            else
            {
                if (creationOptions.WaitForAsyncProvisioning.HasValue && creationOptions.WaitForAsyncProvisioning.Value)
                {
                    // Let's wait for the async provisioning of features, site scripts and content types to be done before we allow API's to further update the created site
                    await WaitForProvisioningToCompleteAsync(responseContext, creationOptions).ConfigureAwait(false);
                }
            }

            return responseContext;
        }

        private static async Task WaitForProvisioningToCompleteAsync(PnPContext context, SiteCreationOptions creationOptions)
        {
            context.Logger.LogInformation($"Started waiting for the async provisioning of site {context.Uri} to be complete");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            bool isProvisioningComplete = false;
            bool validatePendingWebTemplateExtensionCalled = false;
            var retryAttempt = 1;
            do
            {
                context.Logger.LogDebug($"Elapsed: {stopwatch.Elapsed:mm\\:ss\\.fff} | Attempt {retryAttempt}/{creationOptions.MaxAsyncProvisioningStatusChecks}");

                if (retryAttempt > 1)
                {
                    context.Logger.LogDebug($"Elapsed: {stopwatch.Elapsed:mm\\:ss\\.fff} | Waiting {creationOptions.WaitAfterAsyncProvisioningStatusCheck.Value} seconds");

                    await context.WaitAsync(TimeSpan.FromSeconds(creationOptions.WaitAfterAsyncProvisioningStatusCheck.Value)).ConfigureAwait(false);
                }

                var web = await context.Web.GetAsync(p => p.IsProvisioningComplete).ConfigureAwait(false);
                if (web.IsProvisioningComplete)
                {
                    isProvisioningComplete = true;
                }

                // We waited for more than 90 seconds
                if (!isProvisioningComplete && !validatePendingWebTemplateExtensionCalled && retryAttempt * creationOptions.WaitAfterAsyncProvisioningStatusCheck.Value > 90)
                {
                    context.Logger.LogDebug($"Calling ValidatePendingWebTemplateExtension for site {context.Uri}");

                    // Try "push" the process
                    await (context.Web as Web).RawRequestAsync(
                        new ApiCall("_api/Microsoft.Sharepoint.Utilities.WebTemplateExtensions.SiteScriptUtility.ValidatePendingWebTemplateExtension", ApiType.SPORest),
                        HttpMethod.Post).ConfigureAwait(false);
                    validatePendingWebTemplateExtensionCalled = true;

                    context.Logger.LogDebug($"Calling ValidatePendingWebTemplateExtension for site {context.Uri} done");
                }

                retryAttempt++;

            }
            while (!isProvisioningComplete && retryAttempt <= creationOptions.MaxAsyncProvisioningStatusChecks);

            stopwatch.Stop();

            context.Logger.LogDebug($"Elapsed: {stopwatch.Elapsed:mm\\:ss\\.fff} | Finished");

            if (!isProvisioningComplete)
            {
                // Bummer, sites seems to be still not ready...log a warning but let's not fail
                context.Logger.LogWarning($"Async provisioning of site {context.Uri} did not complete in {stopwatch.Elapsed:mm\\:ss\\.fff}");
            }
            else
            {
                context.Logger.LogInformation($"Async provisioning of site {context.Uri} is complete");
            }
        }

        private static async Task<PnPContext> VerifySiteStatusAsync(PnPContext context, string urlOrGroupToCheck, bool usingSpSiteManager, int maxStatusChecks, int waitAfterStatusCheck)
        {
            var siteCreated = false;
            var siteUrl = string.Empty;
            var statusCheckAttempt = 1;
            int lastSiteStatus = -1;

            string apiCall;
            string statusProperty;

            if (usingSpSiteManager)
            {
                apiCall = $"_api/SPSiteManager/status?url='{HttpUtility.UrlEncode(urlOrGroupToCheck)}'";
                statusProperty = "Status";
            }
            else
            {
                apiCall = $"_api/groupsitemanager/GetSiteStatus('{urlOrGroupToCheck}')";
                statusProperty = "GetSiteStatus";
            }

            do
            {
                if (statusCheckAttempt > 1)
                {
                    await context.WaitAsync(TimeSpan.FromSeconds(statusCheckAttempt * waitAfterStatusCheck)).ConfigureAwait(false);
                }

                try
                {
                    var result = await (context.Web as Web).RawRequestAsync(new ApiCall(apiCall, ApiType.SPORest), HttpMethod.Get).ConfigureAwait(false);
                    var responseJson = JsonSerializer.Deserialize<JsonElement>(result.Json);

                    #region Json response
                    /*
                    {
                        "d": {
                            "Status": {
                                "__metadata": {
                                    "type": "Microsoft.SharePoint.Portal.SPSiteCreationResponse"
                                },
                                "SiteId": "b1ec46e2-c26a-423f-ab01-3aeb951c1c82",
                                "SiteStatus": 2,
                                "SiteUrl": "https://bertonline.sharepoint.com/sites/pnpcoresdktestcommsite1"
                            }
                        }
                    }
                    */
                    #endregion

                    int siteStatus = responseJson.GetProperty("d").GetProperty(statusProperty).GetProperty("SiteStatus").GetInt32();
                    lastSiteStatus = siteStatus;
                    if (siteStatus == 2)
                    {
                        siteCreated = true;
                        siteUrl = responseJson.GetProperty("d").GetProperty(statusProperty).GetProperty("SiteUrl").GetString();
                    }
                }
                catch (Exception ex)
                {
                    // Log and eat exception here
                    context.Logger.LogWarning(PnPCoreAdminResources.Log_Warning_ExceptionWhileGettingSiteStatus, urlOrGroupToCheck, ex.ToString());
                }

                statusCheckAttempt++;
            }
            while (!siteCreated && statusCheckAttempt <= maxStatusChecks);

            if (siteCreated)
            {
                return await context.CloneAsync(new Uri(siteUrl)).ConfigureAwait(false);
            }
            else
            {
                throw new ClientException(ErrorType.SharePointRestServiceError, string.Format(PnPCoreAdminResources.Exception_SiteCreationStatusCheck, siteUrl, lastSiteStatus));
            }
        }

        private static Dictionary<string, object> BuildBaseCommonNoGroupSiteRequestPayload(CommonNoGroupSiteOptions siteCollectionCreationInformation)
        {
            return new Dictionary<string, object>
            {
                { "Title", siteCollectionCreationInformation.Title },
                { "Lcid", (int)siteCollectionCreationInformation.Language },
                { "ShareByEmailEnabled", siteCollectionCreationInformation.ShareByEmailEnabled },
                { "Url", siteCollectionCreationInformation.Url },
                { "Classification", siteCollectionCreationInformation.Classification ?? "" },
                { "Description", siteCollectionCreationInformation.Description ?? "" },
                { "WebTemplate", siteCollectionCreationInformation.WebTemplate },
                { "WebTemplateExtensionId", Guid.Empty },
                { "Owner", siteCollectionCreationInformation.Owner }
            };
        }

        private static Guid GetSiteDesignId(CommonNoGroupSiteOptions siteCollectionCreationInformation)
        {
            if (siteCollectionCreationInformation.SiteDesignId != Guid.Empty)
            {
                return siteCollectionCreationInformation.SiteDesignId;
            }
            else if (siteCollectionCreationInformation is CommunicationSiteOptions communicationSiteOptions)
            {
                switch (communicationSiteOptions.SiteDesign)
                {
                    case CommunicationSiteDesign.Topic:
                        {
                            return Guid.Empty;
                        }
                    case CommunicationSiteDesign.Showcase:
                        {
                            return Guid.Parse("6142d2a0-63a5-4ba0-aede-d9fefca2c767");
                        }
                    case CommunicationSiteDesign.Blank:
                        {
                            return Guid.Parse("f6cc5403-0d63-442e-96c0-285923709ffc");
                        }
                }
            }

            return Guid.Empty;
        }


    }
}
