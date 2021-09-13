using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.Microsoft365
{
    internal class Microsoft365Admin : IMicrosoft365Admin
    {
        private readonly PnPContext context;

        internal Microsoft365Admin(PnPContext pnpContext)
        {
            context = pnpContext;
        }

        public async Task<bool> IsMultiGeoTenantAsync()
        {
            var result = await (context.Web as Web).RawRequestAsync(new ApiCall("sites?filter=siteCollection/root%20ne%20null&select=webUrl,siteCollection", ApiType.Graph), HttpMethod.Get).ConfigureAwait(false);

            #region Json responses
            /* Response if not multi-geo

            {
                "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#sites",
                "value": [
                    {
                        "webUrl": "https://bertonline.sharepoint.com/",
                        "siteCollection": {
                            "hostname": "bertonline.sharepoint.com",
                            "root": {}
                        }
                    }
                ]
            }
 
            response if multi geo

            {
                "@odata.context": "https://graph.microsoft.com/beta/$metadata#sites",
                "value": [
                    {
                        "webUrl": "https://contoso.sharepoint.com/",
                        "siteCollection": {
                            "dataLocationCode":"NAM",
                            "hostname": "contoso.sharepoint.com"
                        }
                    },
                    {
                        "webUrl": "https://contosoeur.sharepoint.com/",
                        "siteCollection": {
                            "dataLocationCode":"EUR",
                            "hostname": "contosoeur.sharepoint.com"
                        }
                    },
                    {
                        "webUrl": "https://contosoapc.sharepoint.com/",
                        "siteCollection": {
                            "dataLocationCode":"APC",
                            "hostname": "contosoapc.sharepoint.com"
                        }
                    }
                ]
            }
            */
            #endregion

            var root = JsonDocument.Parse(result.Json).RootElement.GetProperty("value");

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var siteInformation in root.EnumerateArray())
                {
                    if (siteInformation.TryGetProperty("siteCollection", out JsonElement siteCollection))
                    {
                        if (siteCollection.TryGetProperty("dataLocationCode", out JsonElement dataLocationCode))
                        {
                            if (dataLocationCode.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(dataLocationCode.GetString()))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool IsMultiGeoTenant()
        {
            return IsMultiGeoTenantAsync().GetAwaiter().GetResult();
        }

        public async Task<List<IGeoLocation>> GetMultiGeoLocationsAsync()
        {
            var result = await(context.Web as Web).RawRequestAsync(new ApiCall("sites?filter=siteCollection/root%20ne%20null&select=webUrl,siteCollection", ApiType.Graph), HttpMethod.Get).ConfigureAwait(false);

            #region Json responses
            /* Response if not multi-geo

            {
                "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#sites",
                "value": [
                    {
                        "webUrl": "https://bertonline.sharepoint.com/",
                        "siteCollection": {
                            "hostname": "bertonline.sharepoint.com",
                            "root": {}
                        }
                    }
                ]
            }
 
            response if multi geo

            {
                "@odata.context": "https://graph.microsoft.com/beta/$metadata#sites",
                "value": [
                    {
                        "webUrl": "https://contoso.sharepoint.com/",
                        "siteCollection": {
                            "dataLocationCode":"NAM",
                            "hostname": "contoso.sharepoint.com"
                        }
                    },
                    {
                        "webUrl": "https://contosoeur.sharepoint.com/",
                        "siteCollection": {
                            "dataLocationCode":"EUR",
                            "hostname": "contosoeur.sharepoint.com"
                        }
                    },
                    {
                        "webUrl": "https://contosoapc.sharepoint.com/",
                        "siteCollection": {
                            "dataLocationCode":"APC",
                            "hostname": "contosoapc.sharepoint.com"
                        }
                    }
                ]
            }
            */
            #endregion

            var root = JsonDocument.Parse(result.Json).RootElement.GetProperty("value");

            List<IGeoLocation> dataLocations = null;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var siteInformation in root.EnumerateArray())
                {
                    if (siteInformation.TryGetProperty("siteCollection", out JsonElement siteCollection))
                    {
                        if (siteCollection.TryGetProperty("dataLocationCode", out JsonElement dataLocationCode))
                        {
                            if (dataLocationCode.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(dataLocationCode.GetString()))
                            {
                                if (dataLocations == null)
                                {
                                    dataLocations = new List<IGeoLocation>();
                                }

                                var geoLocation = new GeoLocation()
                                {
                                    DataLocationCode = dataLocationCode.GetString(),
                                    SharePointPortalUrl = siteInformation.GetProperty("webUrl").GetString(),
                                    SharePointAdminUrl = SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri(siteInformation.GetProperty("webUrl").GetString())).ToString(),
                                    SharePointMySiteUrl = SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri(siteInformation.GetProperty("webUrl").GetString())).ToString()
                                };

                                if (dataLocations.FirstOrDefault(p => p.DataLocationCode.Equals(geoLocation.DataLocationCode, StringComparison.InvariantCultureIgnoreCase)) == null)
                                {
                                    dataLocations.Add(geoLocation);
                                }
                            }
                        }
                    }
                }
            }

            return dataLocations;
        }

        public List<IGeoLocation> GetMultiGeoLocations()
        {
            return GetMultiGeoLocationsAsync().GetAwaiter().GetResult();
        }
    }
}
