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
        // max input objects for the related requests
        private static readonly int MaxServerRelativeUrls = 500;
        private static readonly int MaxPermissions = 500;
        private static readonly int MaxAppIds = 500;

        internal async static Task<List<SharePointAddIn>> GetSharePointAddInsAsync(PnPContext context, bool includeSubsites, VanityUrlOptions vanityUrlOptions, bool loadLegacyPrincipalData = true)
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

            var serverRelativeUrlBuckets = SplitInBuckets(serverRelativeUrlsToCheck, MaxServerRelativeUrls);

            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                if (loadLegacyPrincipalData)
                {
                    // Generic principal load, we need this to know the permissions that the AddIns have
                    await LoadLegacyPrincipalsAsync(legacyPrincipals, null, serverRelativeUrlBuckets, tenantAdminContext, vanityUrlOptions, context.Uri.DnsSafeHost).ConfigureAwait(false);
                }

                foreach (var serverRelativeUrlBucket in serverRelativeUrlBuckets)
                {
                    string body = null;

                    if (vanityUrlOptions != null)
                    {
                        var json = new
                        {
                            urls = AsFullyQualified(serverRelativeUrlBucket, context.Uri.DnsSafeHost),
                        }.AsExpando();

                        body = JsonSerializer.Serialize(json, typeof(ExpandoObject));
                    }
                    else
                    {
                        var json = new
                        {
                            serverRelativeUrls = serverRelativeUrlBucket,
                        }.AsExpando();

                        body = JsonSerializer.Serialize(json, typeof(ExpandoObject));
                    }

                    // Get the AddIns
                    var results = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall($"_api/web/AvailableAddIns", ApiType.SPORest, body), HttpMethod.Post).ConfigureAwait(false);

                    var jsonResponse = JsonSerializer.Deserialize<JsonElement>(results.Json);
                    if (jsonResponse.TryGetProperty("addins", out JsonElement addIns) && addIns.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var addIn in addIns.EnumerateArray())
                        {
                            // Skip some SPFx solutions that accidently might show up (API fix coming)
                            if (addIn.GetProperty("appIdentifier").GetString().Contains("|ms.sp.int|", StringComparison.InvariantCultureIgnoreCase) &&
                                addIn.GetProperty("appWebId").GetGuid() == Guid.Empty)
                            {
                                continue;
                            }

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
                                LicensePurchaseTime = addIn.GetProperty("licensePurchaseTime").ValueKind == JsonValueKind.Null ? DateTime.MinValue : addIn.GetProperty("licensePurchaseTime").GetDateTime(),
                                Locale = addIn.GetProperty("locale").GetString(),
                                ProductId = addIn.GetProperty("productId").GetGuid(),
                                PurchaserIdentity = addIn.GetProperty("purchaserIdentity").GetString(),
                                Status = (SharePointAddInStatus)Enum.Parse(typeof(SharePointAddInStatus), addIn.GetProperty("status").GetString()),
                                TenantAppData = addIn.GetProperty("tenantAppData").GetString(),
                                TenantAppDataUpdateTime = addIn.GetProperty("tenantAppDataUpdateTime").ValueKind == JsonValueKind.Null ? DateTime.MinValue : addIn.GetProperty("tenantAppDataUpdateTime").GetDateTime(),
                                Title = addIn.GetProperty("title").GetString(),
                            };

                            if (loadLegacyPrincipalData)
                            {
                                // Complement the AddIn with the permission related information
                                LegacyPrincipal legacyPrincipal = null;
                                if (vanityUrlOptions != null)
                                {
                                    legacyPrincipal = legacyPrincipals.FirstOrDefault(p => p.AppIdentifier == sharePointAddIn.AppIdentifier && p.AbsoluteUrl == sharePointAddIn.AbsoluteUrl);
                                }
                                else
                                {
                                    legacyPrincipal = legacyPrincipals.FirstOrDefault(p => p.AppIdentifier == sharePointAddIn.AppIdentifier && p.ServerRelativeUrl == sharePointAddIn.ServerRelativeUrl);
                                }
                                
                                if (legacyPrincipal != null)
                                {
                                    sharePointAddIn.SiteCollectionScopedPermissions = legacyPrincipal.SiteCollectionScopedPermissions;
                                    sharePointAddIn.TenantScopedPermissions = legacyPrincipal.TenantScopedPermissions;
                                    sharePointAddIn.AllowAppOnly = legacyPrincipal.AllowAppOnly;
                                }
                            }

                            sharePointAddIns.Add(sharePointAddIn);
                        }
                    }
                }
            }

            return sharePointAddIns;
        }

        internal async static Task<List<ACSPrincipal>> GetACSPrincipalsAsync(PnPContext context, List<ILegacyServicePrincipal> legacyServicePrincipals, bool includeSubsites, bool tenantOnly, VanityUrlOptions vanityUrlOptions)
        {
            List<ACSPrincipal> acsPrincipals = new();

            List<LegacyPrincipal> legacyPrincipals = new();

            List<string> serverRelativeUrlsToCheck = new()
            {
                context.Web.Url.LocalPath
            };

            if (includeSubsites && !tenantOnly)
            {
                var webs = await WebEnumerator.GetWithDetailsAsync(context, null, true).ConfigureAwait(false);
                foreach (var web in webs)
                {
                    serverRelativeUrlsToCheck.Add(web.ServerRelativeUrl);
                }
            }

            var serverRelativeUrlBuckets = SplitInBuckets(serverRelativeUrlsToCheck, MaxServerRelativeUrls);

            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                // Generic principal load, this will get the permission information of the ACS principals
                await LoadLegacyPrincipalsAsync(legacyPrincipals, legacyServicePrincipals, serverRelativeUrlBuckets, tenantAdminContext, vanityUrlOptions, context.Uri.DnsSafeHost).ConfigureAwait(false);

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

                var appIdBuckets = SplitInBuckets(appIds, MaxAppIds);

                foreach (var appIdBucket in appIdBuckets)
                {
                    var json = new
                    {
                        appIds = appIdBucket,
                    }.AsExpando();

                    var body = JsonSerializer.Serialize(json, typeof(ExpandoObject));

                    // Load the ACS principals
                    var results = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall($"_api/web/GetACSServicePrincipals", ApiType.SPORest, body), HttpMethod.Post).ConfigureAwait(false);

                    var jsonResponse = JsonSerializer.Deserialize<JsonElement>(results.Json);
                    if (jsonResponse.TryGetProperty("value", out JsonElement acsPrincipalArray) && acsPrincipalArray.ValueKind == JsonValueKind.Array)
                    {
                        // Load the returned data
                        List<ACSPrincipal> tempACSPrincipals = new();
                        foreach (var acsPrincipal in acsPrincipalArray.EnumerateArray())
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

                            if (tempACSPrincipal.ValidUntil == DateTime.MinValue)
                            {
                                // The principal was not retrieved as part of the legacy service principals, this can happen because
                                // since end of 2024 we're creating ACS principals as regular Entra app which do not have the
                                // legacyServicePrincipal type set to Legacy
                                await UpdateACSPrincipalDataWithEntraAppPropertiesAsync(context, tempACSPrincipal).ConfigureAwait(false);
                            }

                            if (acsPrincipal.TryGetProperty("appDomains", out JsonElement appDomains) && appDomains.ValueKind == JsonValueKind.Array)
                            {
                                List<string> appDomainList = new();
                                foreach (var appDomain in appDomains.EnumerateArray())
                                {
                                    appDomainList.Add(appDomain.GetString());
                                }
                                tempACSPrincipal.AppDomains = appDomainList.ToArray();
                            }

                            tempACSPrincipals.Add(tempACSPrincipal);
                        }

                        // Merge the returned data with the earlier loaded legacy principals
                        foreach (var legacyPrincipal in legacyPrincipals.Where(p => p.SiteCollectionScopedPermissions.Any() || p.TenantScopedPermissions.Any()))
                        {
                            var tempACSPrincipal = tempACSPrincipals.FirstOrDefault(p => p.AppIdentifier == legacyPrincipal.AppIdentifier);
                            if (tempACSPrincipal != null)
                            {
                                bool acsPrincipalFound = false;
                                if (vanityUrlOptions != null)
                                {
                                    acsPrincipalFound = acsPrincipals.Any(p => p.AppIdentifier == legacyPrincipal.AppIdentifier && p.AbsoluteUrl == legacyPrincipal.AbsoluteUrl);
                                }
                                else
                                {
                                    acsPrincipalFound = acsPrincipals.Any(p => p.AppIdentifier == legacyPrincipal.AppIdentifier && p.ServerRelativeUrl == legacyPrincipal.ServerRelativeUrl);
                                }

                                if (!acsPrincipalFound)
                                {
                                    acsPrincipals.Add(new ACSPrincipal
                                    {
                                        AppIdentifier = legacyPrincipal.AppIdentifier,
                                        AllowAppOnly = legacyPrincipal.AllowAppOnly,
                                        ServerRelativeUrl = legacyPrincipal.ServerRelativeUrl,
                                        AbsoluteUrl = legacyPrincipal.AbsoluteUrl,
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

                        // As we've only added principals with tenant permissions for the first site so far we need to copy them for all other investigated webs
                        IEnumerable<ACSPrincipal> acsPrincipalsToCopy = null;

                        if (vanityUrlOptions != null)
                        {
                            acsPrincipalsToCopy = acsPrincipals.ToList().Where(p => p.AbsoluteUrl == $"https://{context.Uri.DnsSafeHost}{serverRelativeUrlsToCheck[0]}" && p.TenantScopedPermissions.Any());
                        }
                        else
                        {
                            acsPrincipalsToCopy = acsPrincipals.ToList().Where(p => p.ServerRelativeUrl == serverRelativeUrlsToCheck[0] && p.TenantScopedPermissions.Any());
                        }

                        foreach (var acsPrincipal in acsPrincipalsToCopy)
                        {
                            for (int i = 1; i < serverRelativeUrlsToCheck.Count; i++)
                            {
                                acsPrincipals.Add(new ACSPrincipal
                                {
                                    AppIdentifier = acsPrincipal.AppIdentifier,
                                    AllowAppOnly = acsPrincipal.AllowAppOnly,
                                    ServerRelativeUrl = serverRelativeUrlsToCheck[i],
                                    AbsoluteUrl = $"https://{context.Uri.DnsSafeHost}{serverRelativeUrlsToCheck[i]}",
                                    SiteCollectionScopedPermissions = acsPrincipal.SiteCollectionScopedPermissions,
                                    TenantScopedPermissions = acsPrincipal.TenantScopedPermissions,
                                    Title = acsPrincipal.Title,
                                    AppId = acsPrincipal.AppId,
                                    RedirectUri = acsPrincipal.RedirectUri,
                                    AppDomains = acsPrincipal.AppDomains,
                                    ValidUntil = acsPrincipal.ValidUntil,
                                });
                            }
                        }

                        // Drop site collection ACS principals (in case there were on the assessed site)
                        if (tenantOnly)
                        {
                            foreach (var acsPrincipal in acsPrincipals.ToList().Where(p => p.SiteCollectionScopedPermissions.Any()))
                            {
                                if (!acsPrincipal.TenantScopedPermissions.Any())
                                {
                                    acsPrincipals.Remove(acsPrincipal);
                                }
                            }

                            // empty server relative url as these apply for all server relative urls
                            foreach (var acsPrincipal in acsPrincipals)
                            {
                                acsPrincipal.ServerRelativeUrl = "";
                                acsPrincipal.AbsoluteUrl = "";
                            }
                        }
                    }
                }
            }

            return acsPrincipals;
        }

        private static async Task UpdateACSPrincipalDataWithEntraAppPropertiesAsync(PnPContext context, ACSPrincipal tempACSPrincipal)
        {
            var response = await (context.Web as Web).RawRequestAsync(new ApiCall(string.Format("applications?$filter=appid eq '{0}'&$select=id,appId,passwordCredentials,displayName", tempACSPrincipal.AppId), ApiType.Graph), HttpMethod.Get).ConfigureAwait(false);

            var jsonResponse2 = JsonSerializer.Deserialize<JsonElement>(response.Json);
            if (jsonResponse2.TryGetProperty("value", out JsonElement appArray) && appArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var acsApp in appArray.EnumerateArray())
                {
                    if (acsApp.TryGetProperty("passwordCredentials", out JsonElement keyCredentials) && keyCredentials.ValueKind == JsonValueKind.Array)
                    {
                        // Only include service principals which are still valid
                        foreach (var keyCredential in keyCredentials.EnumerateArray())
                        {
                            if (keyCredential.TryGetProperty("endDateTime", out JsonElement endDateTime))
                            {
                                tempACSPrincipal.ValidUntil = endDateTime.GetDateTime();
                                return;
                            }
                        }
                    }
                }
            }
        }

        internal static async Task<List<ILegacyServicePrincipal>> GetValidLegacyServicePrincipalAppIdsAsync(PnPContext context, bool includeExpiredPrincipals, VanityUrlOptions vanityUrlOptions)
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
                    foreach (var legacyServicePrincipal in value.EnumerateArray())
                    {
                        if (legacyServicePrincipal.TryGetProperty("passwordCredentials", out JsonElement keyCredentials) && keyCredentials.ValueKind == JsonValueKind.Array)
                        {
                            // Only include service principals which are still valid
                            foreach (var keyCredential in keyCredentials.EnumerateArray())
                            {
                                var endDate = DateTime.MinValue;
                                if (keyCredential.TryGetProperty("endDateTime", out JsonElement endDateTime))
                                {
                                    endDate = endDateTime.GetDateTime();
                                }

                                if (includeExpiredPrincipals || endDate >= DateTime.Now.ToUniversalTime())
                                {
                                    servicePrincipals.Add(new LegacyServicePrincipal
                                    {
                                        AppId = legacyServicePrincipal.GetProperty("appId").GetGuid(),
                                        AppIdentifier = $"i:0i.t|ms.sp.ext|{legacyServicePrincipal.GetProperty("appId").GetGuid()}@{tenantId}",
                                        Name = legacyServicePrincipal.GetProperty("displayName").GetString(),
                                        ValidUntil = endDate
                                    });

                                    // no need to check the next keycredential as we've a valid one, if there's more than one credential they're returned with the 
                                    // most recent first. So the first will be either a valid credential or the last expired one
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

            // While above approach will find all principals registered using appregnew.aspx it's not detecting the case where the app was created using Entra registration and then 
            // later granted permissions using appinv.aspx. This only applies to ACS principals scoped to the full tenant created by calling appinv.asxp from tenant admin center.
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                Uri tenantAdminUrl = vanityUrlOptions != null ? vanityUrlOptions.AdminCenterUri : tenantAdminContext.Uri;
                var json = new
                {
                    urls = new List<string> { tenantAdminUrl.ToString() }
                }.AsExpando();

                string body = JsonSerializer.Serialize(json, typeof(ExpandoObject));

                var results = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall($"_api/web/GetAddinPrincipalsHavingPermissionsInSites", ApiType.SPORest, body), HttpMethod.Post).ConfigureAwait(false);

                jsonResponse = JsonSerializer.Deserialize<JsonElement>(results.Json);
                if (jsonResponse.TryGetProperty("addinPrincipals", out JsonElement addInPrincipals))
                {
                    if (addInPrincipals.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var addInPrincipal in addInPrincipals.EnumerateArray())
                        {
                            Guid appId = Guid.Parse(AppIdFromAppIdentifier(addInPrincipal.GetProperty("appIdentifier").GetString()));

                            if (appId == Guid.Parse("00000003-0000-0ff1-ce00-000000000000"))
                            {

                               // Skip the SharePoint Online principal
                                continue;
                            }

                            if (servicePrincipals.Any(p => p.AppId == appId))
                            {
                                // Skip the principal as it's already loaded
                                continue;
                            }

                            servicePrincipals.Add(new LegacyServicePrincipal()
                            {
                                AppId = Guid.Parse(AppIdFromAppIdentifier(addInPrincipal.GetProperty("appIdentifier").GetString())),
                                AppIdentifier = addInPrincipal.GetProperty("appIdentifier").GetString(),
                                Name = addInPrincipal.GetProperty("title").GetString(),
                                // We don't know 
                                ValidUntil = DateTime.MinValue
                            });
                        }
                    }
                }
            }

            return servicePrincipals;
        }

        private static async Task LoadLegacyPrincipalsAsync(List<LegacyPrincipal> legacyPrincipals, List<ILegacyServicePrincipal> legacyServicePrincipals, List<List<string>> serverRelativeUrlBuckets, PnPContext tenantAdminContext, VanityUrlOptions vanityUrlOptions, string dnsSafeHost)
        {
            int bucketCount = 1;
            foreach (var serverRelativeUrlBucket in serverRelativeUrlBuckets)
            {
                // Step 1: Identify which principals have permissions in the web(s)
                string body = null;

                if (vanityUrlOptions != null)
                {
                    var json = new
                    {
                        urls = AsFullyQualified(serverRelativeUrlBucket, dnsSafeHost)
                    }.AsExpando();

                    body = JsonSerializer.Serialize(json, typeof(ExpandoObject));
                }
                else
                {
                    var json = new
                    {
                        serverRelativeUrls = serverRelativeUrlBucket,
                    }.AsExpando();

                    body = JsonSerializer.Serialize(json, typeof(ExpandoObject));
                }
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
                                AbsoluteUrl = addInPrincipal.GetProperty("absoluteUrl").ToString(),
                                Title = addInPrincipal.GetProperty("title").GetString(),
                                SiteCollectionScopedPermissions = new List<LegacySiteCollectionPermission>(),
                                TenantScopedPermissions = new List<LegacyTenantPermission>()
                            });
                        }
                    }
                }

                // add the legacy service principals (if any). This list will for example contain the ACS principals that have tenant level permissions
                // we're only adding them on the first url for now
                if (legacyServicePrincipals != null && bucketCount == 1)
                {
                    foreach (var legacyServicePrincipal in legacyServicePrincipals)
                    {
                        // As the list also contains the site collection scoped ACS principals we need to check for duplicates
                        bool legacyPrincipalFound = false;

                        if (vanityUrlOptions != null)
                        {
                            legacyPrincipalFound = legacyPrincipals.Any(p => p.AppIdentifier == legacyServicePrincipal.AppIdentifier && p.AbsoluteUrl == $"https://{dnsSafeHost}{serverRelativeUrlBucket[0]}");
                        }
                        else
                        {
                            legacyPrincipalFound = legacyPrincipals.Any(p => p.AppIdentifier == legacyServicePrincipal.AppIdentifier && p.ServerRelativeUrl == serverRelativeUrlBucket[0]);
                        }

                        if (!legacyPrincipalFound)
                        {
                            legacyPrincipals.Add(new LegacyPrincipal()
                            {
                                AppIdentifier = legacyServicePrincipal.AppIdentifier,
                                ServerRelativeUrl = serverRelativeUrlBucket[0],
                                AbsoluteUrl = $"https://{dnsSafeHost}{serverRelativeUrlBucket[0]}",
                                Title = legacyServicePrincipal.Name,
                                SiteCollectionScopedPermissions = new List<LegacySiteCollectionPermission>(),
                                TenantScopedPermissions = new List<LegacyTenantPermission>()
                            });
                        }
                    }
                }

                bucketCount++;
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

                if (vanityUrlOptions != null)
                {
                    var addin = new
                    {
                        url = legacyPrincipal.AbsoluteUrl,
                        appIdentifiers,
                    };
                    addinsToQuery.Add(addin.AsExpando());
                }
                else
                {
                    var addin = new
                    {
                        serverRelativeUrl = legacyPrincipal.ServerRelativeUrl,
                        appIdentifiers,
                    };
                    addinsToQuery.Add(addin.AsExpando());
                }
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

                    if (vanityUrlOptions != null)
                    {
                        var addin = new
                        {
                            url = $"https://{dnsSafeHost}{serverRelativeUrlBuckets[0][0]}",
                            appIdentifiers,
                        };
                        addinsToQuery.Add(addin.AsExpando());
                    }
                    else
                    {
                        var addin = new
                        {
                            serverRelativeUrl = serverRelativeUrlBuckets[0][0],
                            appIdentifiers,
                        };
                        addinsToQuery.Add(addin.AsExpando());
                    }
                }
            }

            var addInsToQueryBuckets = SplitInBuckets(addinsToQuery, MaxPermissions);

            foreach (var addInsToQueryBucket in addInsToQueryBuckets)
            {
                var json = new
                {
                    addins = addInsToQueryBucket
                }.AsExpando();

                var body = JsonSerializer.Serialize(json, typeof(ExpandoObject));

                var results = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall($"_api/web/AddinPermissions", ApiType.SPORest, body), HttpMethod.Post).ConfigureAwait(false);

                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(results.Json);
                if (jsonResponse.TryGetProperty("addinPermissions", out JsonElement addinPermissions))
                {
                    if (addinPermissions.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var addInPermission in addinPermissions.EnumerateArray())
                        {
                            string appIdentifier = addInPermission.GetProperty("appIdentifier").GetString();                            
                            LegacyPrincipal legacyPrincipalToUpdate = null;
                            if (vanityUrlOptions != null)
                            {
                                string absoluteUrl = addInPermission.GetProperty("absoluteUrl").GetString();
                                legacyPrincipalToUpdate = legacyPrincipals.First(p => p.AppIdentifier == appIdentifier && p.AbsoluteUrl == absoluteUrl);
                            }
                            else
                            {
                                string serverRelativeUrl = addInPermission.GetProperty("serverRelativeUrl").GetString();
                                legacyPrincipalToUpdate = legacyPrincipals.First(p => p.AppIdentifier == appIdentifier && p.ServerRelativeUrl == serverRelativeUrl);
                            }

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

        private static List<List<T>> SplitInBuckets<T>(List<T> inputList, int max)
        {
            List<List<T>> splitList = new();

            if (inputList.Count <= max)
            {
                splitList.Add(inputList);
            }
            else
            {
                List<T> newList = new();
                foreach (var input in inputList)
                {
                    newList.Add(input);

                    if (newList.Count >= max)
                    {
                        splitList.Add(newList);
                        newList = new();
                    }
                }

                if (newList.Count > 0)
                {
                    splitList.Add(newList);
                }
            }

            return splitList;
        }

        private static List<string> AsFullyQualified(List<string> serverRelativeUrlList, string dnsSafeHost)
        {
            List<string> fullyQualified = new();

            for (int i = 0; i < serverRelativeUrlList.Count; i++)
            {
                fullyQualified.Add($"https://{dnsSafeHost}{serverRelativeUrlList[i]}");
            }

            return fullyQualified;
        }
    }
}
