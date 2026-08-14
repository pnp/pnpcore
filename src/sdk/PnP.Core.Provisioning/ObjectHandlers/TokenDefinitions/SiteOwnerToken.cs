using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Text.Json;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{siteowner}",
      Description = "Returns the login name of the site collection owner",
      Example = "{siteowner}",
      Returns = "i:0#.f|membership|user@contoso.onmicrosoft.com")]
    internal class SiteOwnerToken : VolatileTokenDefinition
    {
        public SiteOwnerToken(PnPContext context)
            : base(context, "{siteowner}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                ApiRequestResponse response = await Context.Web.ExecuteRequestAsync(
                    new ApiRequest(ApiRequestType.SPORest, "_api/site/owner?$select=LoginName")).ConfigureAwait(false);

                CacheValue = string.Empty;
                if (!string.IsNullOrEmpty(response.Response))
                {
                    using (JsonDocument document = JsonDocument.Parse(response.Response))
                    {
                        if (document.RootElement.TryGetProperty("LoginName", out JsonElement loginName)
                            && loginName.ValueKind == JsonValueKind.String)
                        {
                            CacheValue = loginName.GetString();
                        }
                    }
                }
            }
            return CacheValue;
        }
    }
}