using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
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


        internal static async Task<PnPContext> CreateCommonNoGroupSiteAsync(PnPContext context, CommonNoGroupSiteOptions siteToCreate, SiteCreationOptions creationOptions)
        {

            if (string.IsNullOrEmpty(siteToCreate.Owner) && await context.GetMicrosoft365Admin().AccessTokenUsesApplicationPermissionsAsync().ConfigureAwait(false))
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

            if (siteToCreate.SensitivityLabelId != Guid.Empty)
            {
                payload.Add("SensitivityLabel", siteToCreate.SensitivityLabelId);
            }
            else
            {
                payload["Classification"] = siteToCreate.Classification ?? "";
            }

            if (siteToCreate.PreferredDataLocation.HasValue)
            {
                payload.Add("PreferredDataLocation", siteToCreate.PreferredDataLocation.Value.ToString());
            }

            return await CreateSiteImplementationAsync(context, payload, creationOptions).ConfigureAwait(false);
        }

        private static async Task<PnPContext> CreateSiteImplementationAsync(PnPContext context, Dictionary<string, object> payload, SiteCreationOptions creationOptions)
        {
            var json = new { request = payload }.AsExpando();
            string body = JsonSerializer.Serialize(json, typeof(ExpandoObject));
            var result = await (context.Web as Web).RawRequestAsync(new ApiCall("_api/SPSiteManager/Create", ApiType.SPORest, body), HttpMethod.Post).ConfigureAwait(false);

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

            int siteStatus = responseJson.GetProperty("d").GetProperty("Create").GetProperty("SiteStatus").GetInt32();

            if (siteStatus == 2)
            {
                // Site creation succeeded
                return await context.CloneAsync(new Uri(responseJson.GetProperty("d").GetProperty("Create").GetProperty("SiteUrl").ToString())).ConfigureAwait(false);
            }
            else if (siteStatus == 1)
            {
                int maxStatusChecks = 12;
                int waitAfterStatusCheck = 10;
                if (creationOptions != null)
                {
                    if (creationOptions.MaxStatusChecks.HasValue)
                    {
                        maxStatusChecks = creationOptions.MaxStatusChecks.Value;
                    }
                    if (creationOptions.WaitAfterStatusCheck.HasValue)
                    {
                        waitAfterStatusCheck = creationOptions.WaitAfterStatusCheck.Value;
                    }
                }

                return await VerifySiteStatusAsync(context, payload["Url"].ToString(), maxStatusChecks, waitAfterStatusCheck).ConfigureAwait(false);
            }
            else
            {
                throw new ClientException(ErrorType.SharePointRestServiceError, string.Format(PnPCoreAdminResources.Exception_SiteCreation, payload["Url"].ToString(), siteStatus));
            }
        }

        private static async Task<PnPContext> VerifySiteStatusAsync(PnPContext context, string urlToCheck, int maxStatusChecks, int waitAfterStatusCheck)
        {
            var siteCreated = false;
            var siteUrl = string.Empty;
            var statusCheckAttempt = 1;
            int lastSiteStatus = -1;

            do
            {
                if (statusCheckAttempt > 1)
                {
                    await Task.Delay(statusCheckAttempt * waitAfterStatusCheck * 1000).ConfigureAwait(false);
                }

                try
                {
                    var result = await (context.Web as Web).RawRequestAsync(new ApiCall($"_api/SPSiteManager/status?url='{HttpUtility.UrlEncode(urlToCheck)}'", ApiType.SPORest), HttpMethod.Get).ConfigureAwait(false);
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

                    int siteStatus = responseJson.GetProperty("d").GetProperty("Status").GetProperty("SiteStatus").GetInt32();
                    lastSiteStatus = siteStatus;
                    if (siteStatus == 2)
                    {
                        siteCreated = true;
                        siteUrl = responseJson.GetProperty("d").GetProperty("Status").GetProperty("SiteUrl").GetString();
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    // Log and eat exception here
                    context.Logger.LogWarning(PnPCoreAdminResources.Log_Warning_ExceptionWhileGettingSiteStatus, urlToCheck, ex.ToString());
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

        internal static async Task<PnPContext> CreateTeamSiteAsync(PnPContext context, TeamSiteOptions siteToCreate, SiteCreationOptions creationOptions)
        {
            return null;
        }

        internal static async Task<PnPContext> CreateClassicSiteAsync(PnPContext context, ClassicSiteOptions siteToCreate, SiteCreationOptions creationOptions)
        {
            return null;
        }

    }
}
