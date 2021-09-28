using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal static class SiteCollectionEnumerator
    {

        internal async static Task<List<ISiteCollection>> GetAsync(PnPContext context, bool ignoreUserIsTenantAdmin = false)
        {
            if (!await context.AccessTokenUsesApplicationPermissionsAsync().ConfigureAwait(false))
            {
                if (!ignoreUserIsTenantAdmin && await context.GetSharePointAdmin().IsCurrentUserTenantAdminAsync().ConfigureAwait(false))
                {
                    return await GetViaTenantAdminHiddenListAsync(context).ConfigureAwait(false);
                }
                else
                {
                    return await GetViaGraphSearchApiAsync(context).ConfigureAwait(false);
                }
            }
            else
            {
                return await GetViaGraphSitesApiAsync(context).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Enumerating site collections by querying a hidden list in SharePoint tenant admin. Only works when using 
        /// application permissions with Sites.Read.All or higher or when the user has read access to SharePoint tenant admin,
        /// which is the case for global SharePoint administrators
        /// </summary>
        internal async static Task<List<ISiteCollection>> GetViaTenantAdminHiddenListAsync(PnPContext context, int pageSize = 500)
        {
            // Removed query part to avoid running into list view threshold errors
            string sitesListAllQuery = @"<View Scope='RecursiveAll'>
                                            <ViewFields>
                                                <FieldRef Name='SiteUrl' />
                                                <FieldRef Name='Title' />
                                                <FieldRef Name='SiteId' />
                                                <FieldRef Name='RootWebId' />
                                                <FieldRef Name='TimeDeleted' />
                                            </ViewFields>
                                            <OrderBy Override='TRUE'><FieldRef Name= 'ID' Ascending= 'FALSE' /></OrderBy>
                                            <RowLimit Paged='TRUE'>%PageSize%</RowLimit>
                                         </View>";

            List<ISiteCollection> loadedSites = new List<ISiteCollection>();

            await LoadSitesViaTenantAdminHiddenListAsync(context, sitesListAllQuery, (IEnumerable<IListItem> listItems) =>
            {
                foreach (var listItem in listItems)
                {
                    if (listItem["TimeDeleted"] != null)
                    {
                        continue;
                    }

                    Uri url = new Uri(listItem["SiteUrl"].ToString());
                    Guid siteId = Guid.Parse(listItem["SiteId"].ToString());
                    Guid webId = Guid.Parse(listItem["RootWebId"].ToString());

                    AddLoadedSite(loadedSites,
                                  $"{url.DnsSafeHost},{siteId},{webId}",
                                  listItem["SiteUrl"].ToString(),
                                  siteId,
                                  webId,
                                  listItem["Title"]?.ToString());
                }
            }, pageSize).ConfigureAwait(false);

            return loadedSites;
        }

        private async static Task LoadSitesViaTenantAdminHiddenListAsync(PnPContext context, string viewXml, Action<IEnumerable<IListItem>> processResults, int pageSize = 500)
        {
            string sitesInformationListAllUrl = "DO_NOT_DELETE_SPLIST_TENANTADMIN_ALL_SITES_AGGREGA";

            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false))
            {
                var myList = await tenantAdminContext.Web.Lists.GetByServerRelativeUrlAsync(
                                $"{tenantAdminContext.Uri}Lists/{sitesInformationListAllUrl}",
                                p => p.Title,
                                p => p.Fields.QueryProperties(p => p.InternalName,
                                                              p => p.FieldTypeKind,
                                                              p => p.TypeAsString,
                                                              p => p.Title)).ConfigureAwait(false);
                if (myList != null)
                {
                    bool paging = true;
                    string nextPage = null;
                    while (paging)
                    {
                        var output = await myList.LoadListDataAsStreamAsync(new RenderListDataOptions()
                        {
                            ViewXml = viewXml.Replace("%PageSize%", pageSize.ToString()),
                            RenderOptions = RenderListDataOptionsFlags.ListData,
                            Paging = nextPage ?? null,
                        }).ConfigureAwait(false);

                        if (output.ContainsKey("NextHref"))
                        {
                            nextPage = output["NextHref"].ToString().Substring(1);
                        }
                        else
                        {
                            paging = false;
                        }
                    }

                    if (processResults != null)
                    {
                        processResults.Invoke(myList.Items.AsRequested());
                    }
                }
            }            
        }

        /// <summary>
        /// Enumerating site collections by querying a hidden list in SharePoint tenant admin. Only works when using 
        /// application permissions with Sites.Read.All or higher or when the user has read access to SharePoint tenant admin,
        /// which is the case for global SharePoint administrators
        /// </summary>
        internal async static Task<List<ISiteCollectionWithDetails>> GetWithDetailsViaTenantAdminHiddenListAsync(PnPContext context, int pageSize = 500)
        {
            // Removed query part to avoid running into list view threshold errors
            string sitesListAllQuery = @"<View Scope='RecursiveAll'>
                                            <ViewFields>
                                                <FieldRef Name='SiteUrl' />
                                                <FieldRef Name='Title' />
                                                <FieldRef Name='SiteId' />
                                                <FieldRef Name='RootWebId' />
                                                <FieldRef Name='CreatedBy' />
                                                <FieldRef Name='TimeCreated' />
                                                <FieldRef Name='TimeDeleted' />
                                                <FieldRef Name='ShareByEmailEnabled' />
                                                <FieldRef Name='ShareByLinkEnabled' />
                                                <FieldRef Name='SiteOwnerName' />
                                                <FieldRef Name='SiteOwnerEmail' />
                                                <FieldRef Name='StorageQuota' />
                                                <FieldRef Name='StorageUsed' />
                                                <FieldRef Name='TemplateId' />
                                                <FieldRef Name='TemplateName' />
                                            </ViewFields>
                                            <OrderBy Override='TRUE'><FieldRef Name= 'ID' Ascending= 'FALSE' /></OrderBy>
                                            <RowLimit Paged='TRUE'>%PageSize%</RowLimit>
                                         </View>";

            List<ISiteCollectionWithDetails> loadedSites = new List<ISiteCollectionWithDetails>();

            await LoadSitesViaTenantAdminHiddenListAsync(context, sitesListAllQuery, (IEnumerable<IListItem> listItems) =>
            {
                foreach (var listItem in listItems)
                {
                    if (listItem["TimeDeleted"] != null)
                    {
                        continue;
                    }

                    Uri url = new Uri(listItem["SiteUrl"].ToString());
                    Guid siteId = Guid.Parse(listItem["SiteId"].ToString());
                    Guid webId = Guid.Parse(listItem["RootWebId"].ToString());

                    if (loadedSites.FirstOrDefault(p => p.Id == siteId) == null)
                    {
                        loadedSites.Add(new SiteCollectionWithDetails()
                        {
                            GraphId = $"{url.DnsSafeHost},{siteId},{webId}",
                            Url = new Uri(listItem["SiteUrl"].ToString()),
                            Id = siteId,
                            RootWebId = webId,
                            Name = listItem["Title"]?.ToString(),
                            CreatedBy = listItem["CreatedBy"]?.ToString(),
                            TimeCreated = listItem["TimeCreated"] != null ? (DateTime)listItem["TimeCreated"] : DateTime.MinValue,
                            TimeDeleted = listItem["TimeDeleted"] != null ? (DateTime)listItem["TimeDeleted"] : DateTime.MinValue,
                            ShareByEmailEnabled = (bool)listItem["ShareByEmailEnabled"],
                            ShareByLinkEnabled = (bool)listItem["ShareByLinkEnabled"],
                            SiteOwnerName = listItem["SiteOwnerName"]?.ToString(),
                            SiteOwnerEmail = listItem["SiteOwnerEmail"]?.ToString(),
                            StorageQuota = Convert.ToInt64(listItem["StorageQuota"].ToString()),
                            StorageUsed = Convert.ToInt64(listItem["StorageUsed"].ToString()),
                            TemplateId = (int)listItem["TemplateId"],
                            TemplateName = listItem["TemplateName"]?.ToString(),
                        });
                    }
                }
            }, pageSize).ConfigureAwait(false);

            return loadedSites;
        }

        /// <summary>
        /// Enumerating site collections using Graph Sites endpoint. Only works when using application permissions with Sites.Read.All or higher!
        /// </summary>
        internal async static Task<List<ISiteCollection>> GetViaGraphSitesApiAsync(PnPContext context)
        {
            List<ISiteCollection> loadedSites = new List<ISiteCollection>();

            ApiCall sitesEnumerationApiCall = new ApiCall("sites", ApiType.Graph);
            
            bool paging = true;
            while (paging)
            {
                var result = await (context.Web as Web).RawRequestAsync(sitesEnumerationApiCall, HttpMethod.Get).ConfigureAwait(false);

                #region Json response
                /*              
                "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#sites",
                "@odata.nextLink": "https://graph.microsoft.com/v1.0/sites?$skiptoken=UGFnZWQ9VFJVRSZwX0ZpbGVMZWFmUmVmPTE3MjgyXy4wMDAmcF9JRD0xNzI4Mg",
                "value": [
                    {
                        "createdDateTime": "2017-11-13T10:10:59Z",
                        "id": "bertonline.sharepoint.com,84285b61-b18d-45eb-97c7-014614efa7bc,71fe5817-15b4-4e6a-aeb4-b95c5fe9c31f",
                        "lastModifiedDateTime": "2021-09-24T09:55:42.8767445Z",
                        "name": "ESPC 2017 test",
                        "displayName": "ESPC 2017 test",
                        "webUrl": "https://bertonline.sharepoint.com/sites/espctest1",
                        "sharepointIds": {
                            "siteId": "84285b61-b18d-45eb-97c7-014614efa7bc",
                            "siteUrl": "https://bertonline.sharepoint.com/sites/espctest1",
                            "tenantId": "d8623c9e-30c7-473a-83bc-d907df44a26e",
                            "webId": "71fe5817-15b4-4e6a-aeb4-b95c5fe9c31f"
                        },
                        "siteCollection": {
                            "hostname": "bertonline.sharepoint.com"
                        },
                        "root": {}
                    },
                    {
                        "createdDateTime": "2016-10-10T07:35:53Z",
                        "id": "bertonline.sharepoint.com,0fac3e8a-4157-433f-9cfd-0b10a41cd7b7,074c0dc1-274c-4f92-8a6a-15a3b9088b9b",
                        "lastModifiedDateTime": "2021-09-24T09:55:42.9236211Z",
                        "name": "ab1",
                        "displayName": "ab1",
                        "webUrl": "https://bertonline.sharepoint.com/sites/ab1",
                        "sharepointIds": {
                            "siteId": "0fac3e8a-4157-433f-9cfd-0b10a41cd7b7",
                            "siteUrl": "https://bertonline.sharepoint.com/sites/ab1",
                            "tenantId": "d8623c9e-30c7-473a-83bc-d907df44a26e",
                            "webId": "074c0dc1-274c-4f92-8a6a-15a3b9088b9b"
                        },
                        "siteCollection": {
                            "hostname": "bertonline.sharepoint.com"
                        },
                        "root": {}
                    },
                 */
                #endregion

                var json = JsonSerializer.Deserialize<JsonElement>(result.Json);

                if (json.TryGetProperty("@odata.nextLink", out JsonElement nextLink))
                {
                    sitesEnumerationApiCall = new ApiCall(nextLink.GetString().Replace($"{PnPConstants.MicrosoftGraphBaseUrl}{PnPConstants.GraphV1Endpoint}/", ""), ApiType.Graph);
                }
                else
                {
                    paging = false;
                }

                if (json.GetProperty("value").ValueKind == JsonValueKind.Array)
                {
                    foreach (var siteInformation in json.GetProperty("value").EnumerateArray())
                    {
                        // Root sites have the root property set, ensuring we're not loading up sub sites as site collections
                        if (siteInformation.TryGetProperty("root", out JsonElement _))
                        {
                            var sharePointIds = siteInformation.GetProperty("sharepointIds");

                            AddLoadedSite(loadedSites,
                                          siteInformation.GetProperty("id").GetString(),
                                          siteInformation.GetProperty("webUrl").GetString(),
                                          sharePointIds.GetProperty("siteId").GetGuid(),
                                          sharePointIds.GetProperty("webId").GetGuid(),
                                          siteInformation.TryGetProperty("displayName", out JsonElement rootWebDescription) ? rootWebDescription.GetString() : null);
                        }                        
                    }
                }

            }

            return loadedSites;
        }

        /// <summary>
        /// Enumerating site collections using Graph Search endpoint. Only works when using delegated permissions!
        /// </summary>
        internal async static Task<List<ISiteCollection>> GetViaGraphSearchApiAsync(PnPContext context, int pageSize = 500)
        {
            string requestBody = "{\"requests\": [{ \"entityTypes\": [\"site\"], \"query\": { \"queryString\": \"contentclass:STS_Site\" }, \"from\": %from%, \"size\": %to%, \"fields\": [ \"webUrl\", \"id\", \"name\" ] }]}";

            List<ISiteCollection> loadedSites = new List<ISiteCollection>();

            bool paging = true;
            int from = 0;
            int to = pageSize;
            while (paging)
            {
                ApiCall sitesEnumerationApiCall = new ApiCall("search/query", ApiType.Graph, requestBody.Replace("%from%", from.ToString()).Replace("%to%", to.ToString()));

                var result = await (context.Web as Web).RawRequestAsync(sitesEnumerationApiCall, HttpMethod.Post).ConfigureAwait(false);

                #region Json response
                /*
                {
                "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#microsoft.graph.searchResponse",
                "value": [
                    {
                        "searchTerms": [],
                        "hitsContainers": [
                            {
                                "total": 1313,
                                "moreResultsAvailable": true,
                                "hits": [
                                    {
                                        "hitId": "bertonline.sharepoint.com,b56adf79-ff6a-4964-a63a-ff1fa23be9f8,8c8e101c-1b0d-4253-85e7-c30039bf46e2",
                                        "rank": 1,
                                        "summary": "STS_Site Home {19065987-C276<ddd/>",
                                        "resource": {
                                            "@odata.type": "#microsoft.graph.site",
                                            "id": "bertonline.sharepoint.com,b56adf79-ff6a-4964-a63a-ff1fa23be9f8,8c8e101c-1b0d-4253-85e7-c30039bf46e2",
                                            "createdDateTime": "2018-10-05T19:16:44Z",
                                            "lastModifiedDateTime": "2018-09-29T09:18:41Z",
                                            "name": "prov-1",
                                            "webUrl": "https://bertonline.sharepoint.com/sites/prov-1"
                                        }
                                    },

                */
                #endregion

                var json = JsonSerializer.Deserialize<JsonElement>(result.Json);

                foreach (var queryResult in json.GetProperty("value").EnumerateArray())
                {
                    foreach (var hitsContainer in queryResult.GetProperty("hitsContainers").EnumerateArray())
                    {
                        paging = hitsContainer.GetProperty("moreResultsAvailable").GetBoolean();
                        from += pageSize;
                        to += pageSize;

                        foreach (var hit in hitsContainer.GetProperty("hits").EnumerateArray())
                        {
                            GetSiteAndWebId(hit.GetProperty("hitId").GetString(), out Guid siteId, out Guid webId);

                            AddLoadedSite(loadedSites,
                                          hit.GetProperty("hitId").GetString(),
                                          hit.GetProperty("resource").GetProperty("webUrl").GetString(),
                                          siteId,
                                          webId,
                                          hit.GetProperty("resource").TryGetProperty("name", out JsonElement rootWebDescription) ? rootWebDescription.GetString() : null);
                        }
                    }
                }
            }

            return loadedSites;
        }

        private static void AddLoadedSite(List<ISiteCollection> loadedSites, string graphId, string url, Guid id, Guid rootWebId, string rootWebDescription)
        {
            // Checking for duplicates...should not happen but it does
            if (loadedSites.FirstOrDefault(p => p.Id == id) == null)
            {
                loadedSites.Add(new SiteCollection()
                {
                    Url = new Uri(url),
                    GraphId = graphId,
                    Name = rootWebDescription,
                    Id = id,
                    RootWebId = rootWebId,
                });
            }
        }

        private static void GetSiteAndWebId(string graphId, out Guid siteId, out Guid webId)
        {
            string[] split = graphId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            siteId = Guid.Parse(split[1]);
            webId = Guid.Parse(split[2]);
        }
    }
}
