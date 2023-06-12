using PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant;
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
    internal sealed class SiteCollectionAdmin : ISiteCollectionAdmin
    {
        private readonly PnPContext context;

        public SiteCollectionAdmin(PnPContext pnpContext)
        {
            this.context = pnpContext;
        }
        public Guid Id { get; set; }

        public string LoginName { get; set; }

        public string Name { get; set; }

        public string UserPrincipalName { get; set; }

        public string Mail { get; set; }

        public bool IsSecondaryAdmin { get; set; }

        public bool IsMicrosoft365GroupOwner { get; set; }

        public async Task SetAsPrimarySiteCollectionAdministratorAsync(Uri site)
        {
            // Get the site details to acquire the site id
            List<IRequest<object>> csomRequests = new()
            {
                new GetSiteByUrlRequest(site)
            };

            var result = await (context.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);
            var siteId = (result.ApiCall.CSOMRequests[0].Result as ISite).Id;

            var json = new
            {
                Owner = LoginName,
                OwnerEmail = Mail,
                OwnerName = Name,
                SetOwnerWithoutUpdatingSecondaryAdmin = true
            }.AsExpando();

            var request = new ApiCall($"_api/SPO.Tenant/sites('{siteId}')", ApiType.SPORest, JsonSerializer.Serialize(json, typeof(ExpandoObject)))
            {
                // Extra headers required by SPO.Tenant/sites API
                Headers = new Dictionary<string, string>
                {
                        {"Content-Type", "application/json"},
                        {"Odata-Version", "4.0"},
                        {"Accept", "application/json"}
                    }
            };
            var response = await (context.Web as Web).RawRequestAsync(request, new HttpMethod("PATCH"))
                .ConfigureAwait(false);

            await SiteCollectionManagement.WaitForSpoOperationCompleteAsync(context,
                 JsonSerializer.Deserialize<SpoOperation>(response.Json))
                 .ConfigureAwait(false);
        }

        public void SetAsPrimarySiteCollectionAdministrator(Uri site)
        {
            SetAsPrimarySiteCollectionAdministratorAsync(site).GetAwaiter().GetResult();
        }
    }
}
