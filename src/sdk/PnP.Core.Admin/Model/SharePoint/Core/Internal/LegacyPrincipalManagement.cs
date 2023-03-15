using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal static class LegacyPrincipalManagement
    {
        internal async static Task<List<SharePointAddIn>> GetSharePointAddInsAsync(PnPContext context, bool includeSubsites, VanityUrlOptions vanityUrlOptions)
        {
            List<SharePointAddIn> sharePointAddIns = new();
            List<LegacyPrincipal> legacyPrincipals = new();

            List<string> serverRelativeUrlsToCheck = new()
            {
                context.Web.Url.LocalPath
            };

            if (includeSubsites)
            {
                var webs = await WebEnumerator.GetWithDetailsAsync(context, null, true).ConfigureAwait(false);
                foreach (var web in webs)
                {
                    serverRelativeUrlsToCheck.Add(web.ServerRelativeUrl);
                }
            }

            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                // Generic principal load, we need this to know the permissions that the AddIns have
                await LoadLegacyPrincipalsAsync(legacyPrincipals, null, serverRelativeUrlsToCheck, tenantAdminContext).ConfigureAwait(false);

                var json = new
                {
                    serverRelativeUrls = serverRelativeUrlsToCheck,
                }.AsExpando();

                var body = JsonSerializer.Serialize(json, typeof(ExpandoObject));

                // Get the AddIns
                var results = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall($"_api/web/AvailableAddIns", ApiType.SPORest, body), HttpMethod.Post).ConfigureAwait(false);

                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(results.Json);
                if (jsonResponse.TryGetProperty("addins", out JsonElement addIns) && addIns.ValueKind == JsonValueKind.Array)
                {
                    foreach (var addIn in addIns.EnumerateArray())
                    {
                        SharePointAddIn sharePointAddIn = new()
                        {
                            AppIdentifier = addIn.GetProperty("appIdentifier").GetString(),
                            ServerRelativeUrl = new Uri(addIn.GetProperty("currentWebUrl").GetString()).PathAndQuery,
                            AppInstanceId = addIn.GetProperty("appInstanceId").GetGuid(),
                            AppSource = (SharePointAddInSource)Enum.Parse(typeof(SharePointAddInSource), addIn.GetProperty("appSource").GetString()),
                            AppWebFullUrl = addIn.GetProperty("appWebFullUrl").GetString(),
                            AppWebId = addIn.GetProperty("appWebId").GetGuid(),
                            AssetId = addIn.GetProperty("assetId").GetString(),
                            CreationTime = addIn.GetProperty("creationTimeUtc").GetDateTime(),
                            CurrentSiteId = addIn.GetProperty("currentSiteId").GetGuid(),
                            CurrentWebId = addIn.GetProperty("currentWebId").GetGuid(),
                            CurrentWebName = addIn.GetProperty("currentWebName").GetString(),
                            CurrentWebFullUrl = addIn.GetProperty("currentWebUrl").GetString(),
                            InstalledSiteId = addIn.GetProperty("installedSiteId").GetGuid(),
                            InstalledWebId = addIn.GetProperty("installedWebId").GetGuid(),
                            InstalledWebName = addIn.GetProperty("installedWebName").GetString(),
                            InstalledWebFullUrl = addIn.GetProperty("installedWebUrl").GetString(),
                            InstalledBy = addIn.GetProperty("installedBy").GetString(),
                            LaunchUrl = addIn.GetProperty("launchUrl").GetString(),
                            LicensePurchaseTime = addIn.GetProperty("licensePurchaseTime").GetDateTime(),
                            Locale = addIn.GetProperty("locale").GetString(),
                            ProductId = addIn.GetProperty("productId").GetGuid(),
                            PurchaserIdentity = addIn.GetProperty("purchaserIdentity").GetString(),
                            Status = (SharePointAddInStatus)Enum.Parse(typeof(SharePointAddInStatus), addIn.GetProperty("status").GetString()),
                            TenantAppData = addIn.GetProperty("tenantAppData").GetString(),
                            TenantAppDataUpdateTime = addIn.GetProperty("tenantAppDataUpdateTime").ValueKind == JsonValueKind.Null ? DateTime.MinValue : addIn.GetProperty("tenantAppDataUpdateTime").GetDateTime(),
                            Title = addIn.GetProperty("title").GetString(),
                        };

                        // Complement the AddIn with the permission related information
                        var legacyPrincipal = legacyPrincipals.FirstOrDefault(p=>p.AppIdentifier == sharePointAddIn.AppIdentifier && p.ServerRelativeUrl == sharePointAddIn.ServerRelativeUrl);
                        if (legacyPrincipal != null)
                        {
                            sharePointAddIn.SiteCollectionScopedPermissions = legacyPrincipal.SiteCollectionScopedPermissions;
                            sharePointAddIn.TenantScopedPermissions = legacyPrincipal.TenantScopedPermissions;
                            sharePointAddIn.AllowAppOnly = legacyPrincipal.AllowAppOnly;
                        }

                        sharePointAddIns.Add(sharePointAddIn);
                    }
                }
            }

            return sharePointAddIns;
        }

        internal async static Task<List<ACSPrincipal>> GetACSPrincipalsAsync(PnPContext context, List<ILegacyServicePrincipal> legacyServicePrincipals, bool includeSubsites, VanityUrlOptions vanityUrlOptions)
        {
            List<ACSPrincipal> acsPrincipals = new();

            List<LegacyPrincipal> legacyPrincipals = new();

            List<string> serverRelativeUrlsToCheck = new()
            {
                context.Web.Url.LocalPath
            };

            if (includeSubsites) 
            {
                var webs = await WebEnumerator.GetWithDetailsAsync(context, null, true).ConfigureAwait(false);
                foreach (var web in webs)
                {
                    serverRelativeUrlsToCheck.Add(web.ServerRelativeUrl);
                }
            }

            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                // Generic principal load, this will get the permission information of the ACS principals
                await LoadLegacyPrincipalsAsync(legacyPrincipals, legacyServicePrincipals, serverRelativeUrlsToCheck, tenantAdminContext).ConfigureAwait(false);

                // ACS specific data loading
                List<string> appIds = new();
                foreach (var app in legacyPrincipals)
                {
                    string appIdToAdd = AppIdFromAppIdentifier(app.AppIdentifier);
                    if (appIdToAdd != null && !appIds.Contains(appIdToAdd))
                    {
                        appIds.Add(appIdToAdd);
                    }
                }

                var json = new
                {
                    appIds,
                }.AsExpando();

                var body = JsonSerializer.Serialize(json, typeof(ExpandoObject));

                // Load the ACS principals
                var results = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall($"_api/web/GetACSServicePrincipals", ApiType.SPORest, body), HttpMethod.Post).ConfigureAwait(false);

                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(results.Json);
                if (jsonResponse.TryGetProperty("value", out JsonElement acsPrincipalArray) && acsPrincipalArray.ValueKind == JsonValueKind.Array)
                {
                    // Load the returned data
                    List<ACSPrincipal> tempACSPrincipals = new();
                    foreach(var acsPrincipal in acsPrincipalArray.EnumerateArray()) 
                    {
                        var tempACSPrincipal = new ACSPrincipal
                        {
                            AppIdentifier = acsPrincipal.GetProperty("appIdentifier").GetString(),
                            AppId = acsPrincipal.GetProperty("appId").GetGuid(),
                            RedirectUri = acsPrincipal.GetProperty("redirectUri").GetString(),
                        };

                        if (legacyServicePrincipals != null)
                        {
                            var legacyServicePrincipal = legacyServicePrincipals.FirstOrDefault(p => p.AppIdentifier == tempACSPrincipal.AppIdentifier);
                            if (legacyServicePrincipal != null)
                            {
                                tempACSPrincipal.ValidUntil = legacyServicePrincipal.ValidUntil;
                            }
                        }

                        if (acsPrincipal.TryGetProperty("appDomains", out JsonElement appDomains) && appDomains.ValueKind == JsonValueKind.Array)
                        {
                            List<string> appDomainList = new();
                            foreach(var appDomain in appDomains.EnumerateArray())
                            {
                                appDomainList.Add(appDomain.GetString());
                            }
                            tempACSPrincipal.AppDomains = appDomainList.ToArray();
                        }

                        tempACSPrincipals.Add(tempACSPrincipal);
                    }

                    // Merge the returned data with the earlier loaded legacy principals
                    foreach(var legacyPrincipal in legacyPrincipals.Where(p=>p.SiteCollectionScopedPermissions.Any() || p.TenantScopedPermissions.Any()))
                    {
                        var tempACSPrincipal = tempACSPrincipals.FirstOrDefault(p=>p.AppIdentifier == legacyPrincipal.AppIdentifier);
                        if (tempACSPrincipal != null) 
                        {
                            acsPrincipals.Add(new ACSPrincipal
                            {
                                AppIdentifier = legacyPrincipal.AppIdentifier,
                                AllowAppOnly = legacyPrincipal.AllowAppOnly,
                                ServerRelativeUrl = legacyPrincipal.ServerRelativeUrl,
                                SiteCollectionScopedPermissions = legacyPrincipal.SiteCollectionScopedPermissions,
                                TenantScopedPermissions = legacyPrincipal.TenantScopedPermissions,
                                Title = legacyPrincipal.Title,
                                AppId = tempACSPrincipal.AppId,
                                RedirectUri = tempACSPrincipal.RedirectUri,
                                AppDomains = tempACSPrincipal.AppDomains,
                                ValidUntil = tempACSPrincipal.ValidUntil,
                            });
                        }
                    }
                }
            }

            return acsPrincipals;
        }

        internal static async Task<List<ILegacyServicePrincipal>> GetValidLegacyServicePrincipalAppIdsAsync(PnPContext context)
        {
            List<ILegacyServicePrincipal> servicePrincipals = new();

            var tenantId = await context.GetTenantIdAsync().ConfigureAwait(false);

            // Get a list of legacy service principals from Azure AD
            var response = await (context.Web as Web).RawRequestAsync(new ApiCall("servicePrincipals?$filter=servicePrincipalType eq 'Legacy'&$select=id,appId,passwordCredentials,displayName", ApiType.Graph), HttpMethod.Get).ConfigureAwait(false);

            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(response.Json);

            string nextPageToken;
            do
            {
                // check for next page
                if (jsonResponse.TryGetProperty("@odata.nextLink", out JsonElement nextLink))
                {
                    nextPageToken = nextLink.GetString().Substring(nextLink.GetString().IndexOf("&$skiptoken"));
                }
                else
                {
                    nextPageToken = null;
                }

                // load the valid principals
                if (jsonResponse.TryGetProperty("value", out JsonElement value) && value.ValueKind == JsonValueKind.Array)
                {
                    foreach(var legacyServicePrincipal in value.EnumerateArray())
                    {
                        if (legacyServicePrincipal.TryGetProperty("passwordCredentials", out JsonElement keyCredentials) && keyCredentials.ValueKind == JsonValueKind.Array)
                        {
                            // Only include service principals which are still valid
                            foreach(var keyCredential in keyCredentials.EnumerateArray())
                            {
                                var endDate = DateTime.MinValue;
                                if (keyCredential.TryGetProperty("endDateTime", out JsonElement endDateTime))
                                {
                                    endDate = endDateTime.GetDateTime();
                                }

                                if (endDate >= DateTime.Now.ToUniversalTime())
                                {
                                    servicePrincipals.Add(new LegacyServicePrincipal
                                    {
                                        AppId = legacyServicePrincipal.GetProperty("appId").GetGuid(),
                                        AppIdentifier = $"i:0i.t|ms.sp.ext|{legacyServicePrincipal.GetProperty("appId").GetGuid()}@{tenantId}",
                                        Name = legacyServicePrincipal.GetProperty("displayName").GetString(),
                                        ValidUntil = endDate
                                    });

                                    // no need to check the next keycredential as we've a valid one
                                    break;
                                }
                            }
                        }
                    }
                }

                if (nextPageToken != null)
                {
                    response = await (context.Web as Web).RawRequestAsync(new ApiCall($"servicePrincipals?$filter=servicePrincipalType eq 'Legacy'&$select=id,appId,passwordCredentials,displayName{nextPageToken}", ApiType.Graph), HttpMethod.Get).ConfigureAwait(false);
                    jsonResponse = JsonSerializer.Deserialize<JsonElement>(response.Json);
                }
            }
            while (nextPageToken != null);

            return servicePrincipals;
        }

        private static async Task LoadLegacyPrincipalsAsync(List<LegacyPrincipal> legacyPrincipals, List<ILegacyServicePrincipal> legacyServicePrincipals, List<string> serverRelativeUrlsToCheck, PnPContext tenantAdminContext)
        {
            // Step 1: Identify which principals have permissions in the web(s)
            var json = new
            {
                serverRelativeUrls = serverRelativeUrlsToCheck,
            }.AsExpando();

            var body = JsonSerializer.Serialize(json, typeof(ExpandoObject));

            var results = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall($"_api/web/GetAddinPrincipalsHavingPermissionsInSites", ApiType.SPORest, body), HttpMethod.Post).ConfigureAwait(false);

            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(results.Json);
            if (jsonResponse.TryGetProperty("addinPrincipals", out JsonElement addInPrincipals))
            {
                if (addInPrincipals.ValueKind == JsonValueKind.Array)
                {
                    foreach (var addInPrincipal in addInPrincipals.EnumerateArray())
                    {
                        legacyPrincipals.Add(new LegacyPrincipal()
                        {
                            AppIdentifier = addInPrincipal.GetProperty("appIdentifier").GetString(),
                            ServerRelativeUrl = addInPrincipal.GetProperty("serverRelativeUrl").GetString(),
                            Title = addInPrincipal.GetProperty("title").GetString(),
                        });
                    }
                }
            }

            // add the legacy service principals (if any). This list will for example contain the ACS principals that have tenant level permissions
            if (legacyServicePrincipals != null) 
            { 
                foreach(var legacyServicePrincipal in legacyServicePrincipals)
                {
                    // As the list also contains the site collection scoped ACS principals we need to check for duplicates
                    if (!legacyPrincipals.Any(p => p.AppIdentifier == legacyServicePrincipal.AppIdentifier && p.ServerRelativeUrl == serverRelativeUrlsToCheck[0]))
                    {
                        legacyPrincipals.Add(new LegacyPrincipal()
                        {
                            AppIdentifier = legacyServicePrincipal.AppIdentifier,
                            ServerRelativeUrl = serverRelativeUrlsToCheck[0],
                            Title = legacyServicePrincipal.Name,
                        });
                    }
                }   
            }

            // Step2: Get the permissions for each principal in each web
            List<ExpandoObject> addinsToQuery = new();
            foreach (var legacyPrincipal in legacyPrincipals)
            {
                var appIdentifiers = new
                {
                    __metadata = new { type = "Collection(Edm.String)" },
                    results = new List<string>() { legacyPrincipal.AppIdentifier }
                }.AsExpando();

                var addin = new
                {
                    serverRelativeUrl = legacyPrincipal.ServerRelativeUrl,
                    appIdentifiers,
                };
                addinsToQuery.Add(addin.AsExpando());
            }

            if (legacyServicePrincipals != null)
            {
                foreach (var legacyPrincipal in legacyServicePrincipals)
                {
                    var appIdentifiers = new
                    {
                        __metadata = new { type = "Collection(Edm.String)" },
                        results = new List<string>() { legacyPrincipal.AppIdentifier }
                    }.AsExpando();

                    var addin = new
                    {
                        serverRelativeUrl = serverRelativeUrlsToCheck[0],
                        appIdentifiers,
                    };
                    addinsToQuery.Add(addin.AsExpando());
                }
            }

            json = new
            {
                addins = addinsToQuery
            }.AsExpando();

            body = JsonSerializer.Serialize(json, typeof(ExpandoObject));

            results = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall($"_api/web/AddinPermissions", ApiType.SPORest, body), HttpMethod.Post).ConfigureAwait(false);

            jsonResponse = JsonSerializer.Deserialize<JsonElement>(results.Json);
            if (jsonResponse.TryGetProperty("addinPermissions", out JsonElement addinPermissions))
            {
                if (addinPermissions.ValueKind == JsonValueKind.Array)
                {
                    foreach (var addInPermission in addinPermissions.EnumerateArray())
                    {
                        string appIdentifier = addInPermission.GetProperty("appIdentifier").GetString();
                        string serverRelativeUrl = addInPermission.GetProperty("serverRelativeUrl").GetString();

                        var legacyPrincipalToUpdate = legacyPrincipals.First(p => p.AppIdentifier == appIdentifier && p.ServerRelativeUrl == serverRelativeUrl);

                        legacyPrincipalToUpdate.AllowAppOnly = addInPermission.GetProperty("allowAppOnly").GetBoolean();

                        if (addInPermission.TryGetProperty("siteCollectionScopedPermissions", out JsonElement siteCollectionScopedPermissions) && siteCollectionScopedPermissions.ValueKind == JsonValueKind.Array)
                        {
                            List<LegacySiteCollectionPermission> legacySiteCollectionPermissions = new();

                            foreach (var siteCollectionScopedPermission in siteCollectionScopedPermissions.EnumerateArray())
                            {
                                LegacySiteCollectionPermission legacySiteCollectionPermission = new()
                                {
                                    SiteId = siteCollectionScopedPermission.GetProperty("siteId").GetGuid(),
                                    WebId = siteCollectionScopedPermission.GetProperty("webId").GetGuid(),
                                    ListId = siteCollectionScopedPermission.GetProperty("listId").GetGuid(),
                                    Right = (LegacySiteCollectionPermissionRight)Enum.Parse(typeof(LegacySiteCollectionPermissionRight), siteCollectionScopedPermission.GetProperty("right").GetString())
                                };

                                legacySiteCollectionPermissions.Add(legacySiteCollectionPermission);
                            }

                            legacyPrincipalToUpdate.SiteCollectionScopedPermissions = legacySiteCollectionPermissions;
                        }

                        if (addInPermission.TryGetProperty("tenantScopedPermissions", out JsonElement tenantScopedPermissions) && tenantScopedPermissions.ValueKind == JsonValueKind.Array)
                        {
                            List<LegacyTenantPermission> legacyTenantScopedPermissions = new();

                            foreach (var tenantScopedPermission in tenantScopedPermissions.EnumerateArray())
                            {
                                LegacyTenantPermission legacyTenantPermission = new()
                                {
                                    ProductFeature = tenantScopedPermission.GetProperty("feature").GetString(),
                                    ResourceId = tenantScopedPermission.GetProperty("id").GetString(),
                                    Scope = tenantScopedPermission.GetProperty("scope").GetString(),
                                    Right = (LegacyTenantPermissionRight)Enum.Parse(typeof(LegacyTenantPermissionRight), tenantScopedPermission.GetProperty("right").GetString())
                                };

                                legacyTenantScopedPermissions.Add(legacyTenantPermission);
                            }

                            legacyPrincipalToUpdate.TenantScopedPermissions = legacyTenantScopedPermissions;
                        }
                    }
                }
            }
        }

        private static string AppIdFromAppIdentifier(string appIdentifier)
        {
            var split1 = appIdentifier.Split('@');
            if (split1.Length == 2) 
            { 
                var split2 = split1[0].Split('|');
                if (split2.Length == 3)
                {
                    return split2[2];
                }
            }

            return null;
        }


    }
}
