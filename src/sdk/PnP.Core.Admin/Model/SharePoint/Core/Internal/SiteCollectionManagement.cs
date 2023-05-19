using PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal static class SiteCollectionManagement
    {

        internal static async Task<bool> RecycleSiteCollectionAsync(PnPContext context, Uri siteToRecycle, VanityUrlOptions vanityUrlOptions)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                // When recycling a group connected site then use the groupsitemanager endpoint. This will also recycle the connected Microsoft 365 group
                if (await HasGroupAsync(context, siteToRecycle).ConfigureAwait(false))
                {
                    await (context.Web as Web).RawRequestAsync(new ApiCall($"_api/GroupSiteManager/Delete?siteUrl='{siteToRecycle.AbsoluteUri}'", ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
                    return true;
                }
                else
                {
                    List<IRequest<object>> csomRequests = new List<IRequest<object>>
                    {
                        new RemoveSiteRequest(siteToRecycle)
                    };

                    var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);

                    await WaitForSpoOperationCompleteAsync(tenantAdminContext, result.ApiCall.CSOMRequests[0].Result as SpoOperation).ConfigureAwait(false);
                    return false;
                }
            }
        }

        internal static async Task DeleteSiteCollectionAsync(PnPContext context, Uri siteToDelete, VanityUrlOptions vanityUrlOptions)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                // first recycle the site collection
                bool siteHasGroup = await RecycleSiteCollectionAsync(context, siteToDelete, vanityUrlOptions).ConfigureAwait(false);

                // Only supporting permanent delete of non group connected sites. For group connected sites permanently deleting the group will
                // trigger the deletion of the linked resources (like the SharePoint site).  
                if (!siteHasGroup)
                {
                    // once that's done remove the site collection from the recycle bin
                    List<IRequest<object>> csomRequests = new List<IRequest<object>>
                    {
                        new RemoveDeletedSiteRequest(siteToDelete)
                    };

                    var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);

                    await WaitForSpoOperationCompleteAsync(tenantAdminContext, result.ApiCall.CSOMRequests[0].Result as SpoOperation).ConfigureAwait(false);
                }
            }
        }

        internal static async Task DeleteRecycledSiteCollectionAsync(PnPContext context, Uri siteToDelete, VanityUrlOptions vanityUrlOptions)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                List<IRequest<object>> csomRequests = new List<IRequest<object>>
                {
                    new RemoveDeletedSiteRequest(siteToDelete)
                };

                var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);

                await WaitForSpoOperationCompleteAsync(tenantAdminContext, result.ApiCall.CSOMRequests[0].Result as SpoOperation).ConfigureAwait(false);
            }
        }

        internal static async Task RestoreSiteCollectionAsync(PnPContext context, Uri siteToRecycle, VanityUrlOptions vanityUrlOptions)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                // Get information about the site collection to restore from the SharePoint recycle bin
                var deletedSiteCollection = await SiteCollectionEnumerator.GetRecycledWithDetailsViaTenantAdminHiddenListAsync(context, siteToRecycle, vanityUrlOptions).ConfigureAwait(false);

                if (deletedSiteCollection == null)
                {
                    throw new ClientException(ErrorType.Unsupported, string.Format(PnPCoreAdminResources.Exception_SiteRestore_NotFound, siteToRecycle));
                }

                if (deletedSiteCollection.GroupId != Guid.Empty)
                {
                    // First restore the associated Microsoft 365 group (if it was recycled)
                    await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall($"/directory/deletedItems/{deletedSiteCollection.GroupId}/restore", ApiType.Graph), HttpMethod.Post).ConfigureAwait(false);
                }

                // Use the "classic CSOM based approach to restore the SharePoint site and wait for it to complete
                List<IRequest<object>> csomRequests = new List<IRequest<object>>
                {
                    new RestoreDeletedSiteRequest(siteToRecycle)
                };

                var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);

                await WaitForSpoOperationCompleteAsync(tenantAdminContext, result.ApiCall.CSOMRequests[0].Result as SpoOperation).ConfigureAwait(false);
            }
        }

        internal static async Task WaitForSpoOperationCompleteAsync(PnPContext tenantAdminContext, SpoOperation operation, int maxStatusChecks = 10)
        {
            if (operation.IsComplete)
            {
                return;
            }

            List<IRequest<object>> csomRequests = new List<IRequest<object>>
            {
                new SpoOperationRequest(operation.ObjectIdentity)
            };

            bool operationComplete = false;
            var statusCheckAttempt = 1;

            do
            {
                await tenantAdminContext.WaitAsync(TimeSpan.FromMilliseconds(operation.PollingInterval)).ConfigureAwait(false);

                var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);

                operation = result.ApiCall.CSOMRequests[0].Result as SpoOperation;

                if (operation.IsComplete)
                {
                    operationComplete = true;
                }

                statusCheckAttempt++;
            }
            while (!operationComplete && statusCheckAttempt <= maxStatusChecks);

        }

        private static async Task<bool> HasGroupAsync(PnPContext context, Uri siteToCheck)
        {
            if (context.Uri == siteToCheck)
            {
                return context.Site.GroupId != Guid.Empty;
            }
            else
            {
                using (var siteContext = await context.CloneAsync(siteToCheck).ConfigureAwait(false))
                {
                    return siteContext.Site.GroupId != Guid.Empty;
                }
            }
        }

        internal static async Task<ISiteCollectionProperties> GetSiteCollectionPropertiesByUrlAsync(PnPContext context, Uri siteUrl, bool detailed,VanityUrlOptions vanityUrlOptions)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                List<IRequest<object>> csomRequests = new List<IRequest<object>>
                {
                    new GetSitePropertiesRequest(siteUrl, detailed)
                };

                var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);
                (result.ApiCall.CSOMRequests[0].Result as IDataModelWithContext).PnPContext = tenantAdminContext;

                return result.ApiCall.CSOMRequests[0].Result as ISiteCollectionProperties;
            }
        }

        internal static async Task UpdateSiteCollectionPropertiesAsync(PnPContext context, SiteCollectionProperties properties, VanityUrlOptions vanityUrlOptions)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                List<IRequest<object>> csomRequests = new List<IRequest<object>>
                {
                    new SetSitePropertiesRequest(properties)
                };

                var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);
                await WaitForSpoOperationCompleteAsync(tenantAdminContext, result.ApiCall.CSOMRequests[0].Result as SpoOperation).ConfigureAwait(false);
            }
        }

        internal static async Task<List<ISiteCollectionAdmin>> GetSiteCollectionAdminsAsync(PnPContext context, Uri siteUrl, VanityUrlOptions vanityUrlOptions)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                List<ISiteCollectionAdmin> admins = new List<ISiteCollectionAdmin>();

                // Load the site collection properties first as that will tell who is the primary admin (= owner)
                var siteProperties = await tenantAdminContext.GetSiteCollectionManager().GetSiteCollectionPropertiesAsync(siteUrl, vanityUrlOptions).ConfigureAwait(false);
                var adminUser = new SiteCollectionAdmin(tenantAdminContext)
                {
                    IsSecondaryAdmin = false,
                    LoginName = siteProperties.OwnerLoginName,
                    Name = siteProperties.OwnerName,
                    Mail = siteProperties.OwnerEmail,
                    UserPrincipalName = siteProperties.Owner
                };

                // When the site is group connected we'll return the actual group owners instead of the group owner claim
                if (siteProperties.GroupId != Guid.Empty && adminUser.UserPrincipalName == $"{siteProperties.GroupId}_o")
                {
                    var groupOwnerResults = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall($"_api/SP.Directory.DirectorySession/Group('{siteProperties.GroupId}')/owners?&$select=id,mail,principalName,displayName ", ApiType.SPORest), HttpMethod.Get).ConfigureAwait(false);

                    #region Json response
                    /*
                    {
                        "d": {
                            "results": [
                                {
                                    "__metadata": {
                                        "id": "https://bertonline-admin.sharepoint.com/_api/SP.Directory.Usere72febd4-d92d-4fa1-8cdf-1a445c69acd0",
                                        "uri": "https://bertonline-admin.sharepoint.com/_api/SP.Directory.Usere72febd4-d92d-4fa1-8cdf-1a445c69acd0",
                                        "type": "SP.Directory.User"
                                    },
                                    "displayName": "Bert Jansen (Cloud)",
                                    "id": "33aca310-a489-4121-b853-663d0327fe08",
                                    "mail": "bert.jansen@bertonline.onmicrosoft.com",
                                    "principalName": "bert.jansen@bertonline.onmicrosoft.com"
                                },
                                {
                                    "__metadata": {
                                        "id": "https://bertonline-admin.sharepoint.com/_api/SP.Directory.Userb39e850c-3e30-462a-af59-c6b7790aa416",
                                        "uri": "https://bertonline-admin.sharepoint.com/_api/SP.Directory.Userb39e850c-3e30-462a-af59-c6b7790aa416",
                                        "type": "SP.Directory.User"
                                    },
                                    "displayName": "Kevin Cook",
                                    "id": "7cffa5d0-513a-4f44-b40c-c3b0dba47e07",
                                    "mail": "KevinC@bertonline.onmicrosoft.com",
                                    "principalName": "KevinC@bertonline.onmicrosoft.com"
                                },
                                {
                                    "__metadata": {
                                        "id": "https://bertonline-admin.sharepoint.com/_api/SP.Directory.User205270be-595d-4602-b70f-e65b559a805b",
                                        "uri": "https://bertonline-admin.sharepoint.com/_api/SP.Directory.User205270be-595d-4602-b70f-e65b559a805b",
                                        "type": "SP.Directory.User"
                                    },
                                    "displayName": "Test",
                                    "id": "167987be-02e0-41bf-9cc9-a2bf687c530c",
                                    "mail": "test@bertonline.onmicrosoft.com",
                                    "principalName": "test@bertonline.onmicrosoft.com"
                                }
                            ]
                        }
                    }
                    */
                    #endregion

                    var groupOwnerJsonResponse = JsonSerializer.Deserialize<JsonElement>(groupOwnerResults.Json);
                    if (groupOwnerJsonResponse.TryGetProperty("value", out JsonElement valueProperty))
                    {
                        if (valueProperty.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var value in valueProperty.EnumerateArray())
                            {
                                admins.Add(new SiteCollectionAdmin(tenantAdminContext)
                                {
                                    IsSecondaryAdmin = false,
                                    IsMicrosoft365GroupOwner = true,
                                    Id = value.GetProperty("id").GetGuid(),
                                    Mail = value.GetProperty("mail").GetString(),
                                    Name = value.GetProperty("displayName").GetString(),
                                    UserPrincipalName = value.GetProperty("principalName").GetString()
                                });
                            }
                        }
                    }
                }
                else
                {
                    admins.Add(adminUser);
                }

                // Get the site details to acquire the site id
                List<IRequest<object>> csomRequests = new List<IRequest<object>>
                {
                    new GetSiteByUrlRequest(siteUrl)
                };

                var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);
                var siteId = (result.ApiCall.CSOMRequests[0].Result as ISite).Id;

                // Get the secondary admins
                var json = new
                {
                    secondaryAdministratorsFieldsData = new
                    {
                        siteId = siteId
                    }
                }.AsExpando();

                var body = JsonSerializer.Serialize(json, typeof(ExpandoObject));

                var results = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall($"_api/Microsoft.Online.SharePoint.TenantAdministration.Tenant/GetSiteSecondaryAdministrators", ApiType.SPORest, body), HttpMethod.Post).ConfigureAwait(false);

                #region Json response
                /*
                {
                    "d": {
                        "GetSiteSecondaryAdministrators": {
                            "__metadata": {
                                "type": "Collection(Microsoft.Online.SharePoint.TenantAdministration.SecondaryAdministratorsInfo)"
                            },
                            "results": [
                                {
                                    "email": "bert.jansen@bertonline.onmicrosoft.com",
                                    "loginName": "i:0#.f|membership|bert.jansen@bertonline.onmicrosoft.com",
                                    "name": "Bert Jansen (Cloud)",
                                    "userPrincipalName": "bert.jansen@bertonline.onmicrosoft.com"
                                }
                            ]
                        }
                    }
                }
                */
                #endregion

                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(results.Json);
                if (jsonResponse.TryGetProperty("value", out JsonElement resultsProperty))
                {
                    if (resultsProperty.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var admin in resultsProperty.EnumerateArray())
                        {
                            admins.Add(new SiteCollectionAdmin(tenantAdminContext)
                            {
                                IsSecondaryAdmin = true,
                                LoginName = admin.GetProperty("loginName").GetString(),
                                Name = admin.GetProperty("name").GetString(),
                                Mail = admin.GetProperty("email").GetString(),
                                UserPrincipalName = admin.GetProperty("userPrincipalName").GetString(),
                                IsMicrosoft365GroupOwner = false
                            });
                        }
                    }
                }

                return admins;
            }
        }

        internal static async Task SetSiteCollectionAdminsAsync(PnPContext context, Uri siteUrl, List<string> sharePointAdminLoginNames, List<Guid> ownerGroupAzureAdUserIds, VanityUrlOptions vanityUrlOptions)
        {
            if ((sharePointAdminLoginNames == null || sharePointAdminLoginNames.Count == 0) && (ownerGroupAzureAdUserIds == null || ownerGroupAzureAdUserIds.Count == 0))
            {
                // Nothing to set, so bail out
                return;
            }

            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                // Load the site collection props
                var siteProperties = await tenantAdminContext.GetSiteCollectionManager().GetSiteCollectionPropertiesAsync(siteUrl, vanityUrlOptions).ConfigureAwait(false);

                if (siteProperties.GroupId != Guid.Empty && ownerGroupAzureAdUserIds != null && ownerGroupAzureAdUserIds.Count > 0)
                {
                    // We need to set the group owners, passing users who are already group owner will not break this flow
                    var batch = tenantAdminContext.NewBatch();

                    foreach (var owner in ownerGroupAzureAdUserIds)
                    {
                        await (tenantAdminContext.Web as Web).RawRequestBatchAsync(batch, new ApiCall($"_api/SP.Directory.DirectorySession/Group('{siteProperties.GroupId}')/Owners/Add(objectId='{owner}', principalName='')", ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
                        await (tenantAdminContext.Web as Web).RawRequestBatchAsync(batch, new ApiCall($"_api/SP.Directory.DirectorySession/Group('{siteProperties.GroupId}')/Members/Add(objectId='{owner}', principalName='')", ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
                    }

                    // execute batch
                    await tenantAdminContext.ExecuteAsync(batch).ConfigureAwait(false);
                }

                if (sharePointAdminLoginNames != null && sharePointAdminLoginNames.Count > 0)
                {
                    // Let's set the SharePoint admins

                    // When the site is not group connected then we take the first user from the list and that will be primary admin
                    // When group connected the primary admin is the <groupid>_o claim which sets the group owners as admins, that's 
                    // not something we can change
                    if (siteProperties.GroupId == Guid.Empty)
                    {
                        siteProperties.Owner = sharePointAdminLoginNames[0];
                        siteProperties.SetOwnerWithoutUpdatingSecondaryAdmin = true;
                        await siteProperties.UpdateAsync(vanityUrlOptions).ConfigureAwait(false);
                    }

                    // add the other users as secondary SharePoint admins
                    if (sharePointAdminLoginNames.Count > 1)
                    {
                        List<string> userLoginNamesToAdd = new List<string>();

                        bool first = true;
                        foreach(var sharePoint in sharePointAdminLoginNames)
                        {
                            if (first)
                            {
                                // skip the first one
                                first = false;
                            }
                            else
                            {
                                userLoginNamesToAdd.Add(sharePoint);
                            }
                        }

                        // Get the site details to acquire the site id
                        List<IRequest<object>> csomRequests = new List<IRequest<object>>
                        {
                            new GetSiteByUrlRequest(siteUrl)
                        };

                        var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);
                        var siteId = (result.ApiCall.CSOMRequests[0].Result as ISite).Id;

                        var secondaryAdministratorLoginNames = new
                        {
                            __metadata = new { type = "Collection(Edm.String)" },
                            results = userLoginNamesToAdd
                        }.AsExpando();

                        var json = new
                        {
                            secondaryAdministratorsFieldsData = new
                            {
                                siteId,
                                secondaryAdministratorLoginNames
                            }
                        }.AsExpando();

                        var body = JsonSerializer.Serialize(json, typeof(ExpandoObject));

                        await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall($"_api/Microsoft.Online.SharePoint.TenantAdministration.Tenant/SetSiteSecondaryAdministrators", ApiType.SPORest, body), HttpMethod.Post).ConfigureAwait(false);

                    }
                }

            }
        }
    }
}
