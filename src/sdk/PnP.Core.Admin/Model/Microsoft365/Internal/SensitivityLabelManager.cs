using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.Microsoft365
{
    internal static class SensitivityLabelManager
    {
        internal async static Task<List<ISensitivityLabel>> GetLabelsAsync(PnPContext context)
        {
            if (await context.GetMicrosoft365Admin().AccessTokenUsesApplicationPermissionsAsync().ConfigureAwait(false))
            {
                return await GetLabelsUsingApplicationPermissionsAsync(context).ConfigureAwait(false);
            }
            else
            {
                return await GetLabelsUsingDelegatedPermissionsAsync(context).ConfigureAwait(false);
            }
        }

        internal static async Task<List<ISensitivityLabel>> GetLabelsUsingDelegatedPermissionsAsync(PnPContext context)
        {
            return await GetLabelsImplementationAsync(context, "me/informationprotection/policy/labels").ConfigureAwait(false);   
        }

        internal static async Task<List<ISensitivityLabel>> GetLabelsUsingApplicationPermissionsAsync(PnPContext context)
        {
            return await GetLabelsImplementationAsync(context, "informationprotection/policy/labels").ConfigureAwait(false);
        }

        private static async Task<List<ISensitivityLabel>> GetLabelsImplementationAsync(PnPContext context, string apiRequest)
        {
            // This API requires the user-agent to be explicetely passed in batch requests
            var labels = await (context.Web.WithHeaders(new Dictionary<string, string>() { { "User-Agent", context.GlobalOptions.HttpUserAgent } }) as Web)
                .RawRequestAsync(new ApiCall(apiRequest, ApiType.GraphBeta), HttpMethod.Get).ConfigureAwait(false);

            var json = JsonSerializer.Deserialize<JsonElement>(labels.Json);

            List<ISensitivityLabel> sensitivityLabels = new List<ISensitivityLabel>();

            var values = json.GetProperty("value");
            if (values.ValueKind == JsonValueKind.Array)
            {
                foreach (var value in values.EnumerateArray())
                {
                    var label = new SensitivityLabel()
                    {
                        Id = value.GetProperty("id").GetGuid(),
                        Name = value.GetProperty("name").GetString(),
                        Description = value.GetProperty("description").GetString(),
                        IsActive = value.GetProperty("isActive").GetBoolean(),
                        Sensitivity = value.GetProperty("sensitivity").GetInt32(),
                        Tooltip = value.GetProperty("tooltip").GetString()
                    };

                    sensitivityLabels.Add(label);
                }

            }

            return sensitivityLabels;
        }
    }

}
