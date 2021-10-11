using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
                throw new NotSupportedException("PnP Core Admin library is still in development, this feature is coming");
            }
            else
            {
                return await GetLabelsUsingDelegatedPermissionsAsync(context).ConfigureAwait(false);
            }
        }

        internal static async Task<List<ISensitivityLabel>> GetLabelsUsingDelegatedPermissionsAsync(PnPContext context)
        {
            var labels = await (context.Web as Web).RawRequestAsync(new ApiCall("me/informationProtection/sensitivityLabels", ApiType.GraphBeta), HttpMethod.Get).ConfigureAwait(false);

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
                        DisplayName = value.GetProperty("displayName").GetString(),
                        Description = value.GetProperty("description").GetString(),
                        IsDefault = value.GetProperty("isDefault").GetBoolean(),
                    };

                    var appliesTo = value.GetProperty("applicableTo").GetString();
                    if (!string.IsNullOrEmpty(appliesTo))
                    {
                        label.ApplicableTo = new List<string>(appliesTo.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    sensitivityLabels.Add(label);
                }

            }

            return sensitivityLabels;
        }
    }
}
