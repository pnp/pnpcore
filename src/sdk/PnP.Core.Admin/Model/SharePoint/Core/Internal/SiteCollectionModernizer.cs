using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal static class SiteCollectionModernizer
    {
        internal static async Task<bool> HideAddTeamsPromptAsync(PnPContext context, Uri url)
        {
            using (var siteToOperateOn = await GetContextForSiteCollectionAsync(context, url).ConfigureAwait(false))
            {

                VerifySiteIsGroupConnected(siteToOperateOn);

                // Use the url of the site collection, the context could have been created for a sub site
                await siteToOperateOn.Site.EnsurePropertiesAsync(p => p.Url).ConfigureAwait(false);

                var json = new
                {
                    siteUrl = siteToOperateOn.Site.Url
                }.AsExpando();

                var body = JsonSerializer.Serialize(json, typeof(ExpandoObject));

                ApiCall apiCall = new ApiCall($"_api/groupsitemanager/HideTeamifyPrompt", ApiType.SPORest, body);
                var response = await (siteToOperateOn.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

                #region Json response
                /*             
                {
                    "d": {
                        "HideTeamifyPrompt": null
                    }
                }              
                */
                #endregion

                var valueProperty = JsonSerializer.Deserialize<JsonElement>(response.Json).GetProperty("odata.null");

                return valueProperty.GetBoolean() == true;
            }
        }

        internal static async Task<bool> IsAddTeamsPromptHiddenAsync(PnPContext context, Uri url)
        {
            using (var siteToOperateOn = await GetContextForSiteCollectionAsync(context, url).ConfigureAwait(false))
            {
                VerifySiteIsGroupConnected(siteToOperateOn);

                // Use the url of the site collection, the context could have been created for a sub site
                await siteToOperateOn.Site.EnsurePropertiesAsync(p => p.Url).ConfigureAwait(false);

                ApiCall apiCall = new ApiCall($"_api/groupsitemanager/IsTeamifyPromptHidden?siteUrl='{siteToOperateOn.Site.Url}'", ApiType.SPORest);
                var response = await (siteToOperateOn.Web as Web).RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

                #region Json response
                /*
                {
                    "d": {
                        "IsTeamifyPromptHidden": false
                    }
                }
                */
                #endregion

                return JsonSerializer.Deserialize<JsonElement>(response.Json).GetProperty("value").GetBoolean();
            }
        }

        internal static async Task EnableCommunicationSiteFeaturesAsync(PnPContext context, Uri url, Guid designPackageId)
        {
            using (var siteToOperateOn = await GetContextForSiteCollectionAsync(context, url).ConfigureAwait(false))
            {
                if (designPackageId == Guid.Empty || (designPackageId != PnPAdminConstants.CommunicationSiteDesignTopic &&
                                                      designPackageId != PnPAdminConstants.CommunicationSiteDesignShowCase &&
                                                      designPackageId != PnPAdminConstants.CommunicationSiteDesignBlank))
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreAdminResources.Exception_Unsupported_InvalidDesignPackageId);
                }

                var json = new
                {
                    designPackageId
                }.AsExpando();

                var body = JsonSerializer.Serialize(json, typeof(ExpandoObject));

                ApiCall apiCall = new ApiCall($"_api/sitepages/communicationsite/enable", ApiType.SPORest, body);
                await (siteToOperateOn.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
        }

        private static async Task<PnPContext> GetContextForSiteCollectionAsync(PnPContext context, Uri url)
        {
            if (context.Uri == url)
            {
                return context;
            }
            else
            {
                return await context.CloneAsync(url).ConfigureAwait(false);
            }
        }

        private static void VerifySiteIsGroupConnected(PnPContext context)
        {
            if (!context.Site.IsPropertyAvailable(p => p.GroupId) || context.Site.GroupId == Guid.Empty)
            {
                throw new ClientException(ErrorType.Unsupported, PnPCoreAdminResources.Exception_Unsupported_SiteHasToBeGroupConnected);
            }
        }
    }
}
