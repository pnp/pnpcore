using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{

    [GraphType(Uri = V, LinqGet = baseUri, Beta = true)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class TermGroup
    {
        private const string baseUri = "termstore/groups";
        private const string V = baseUri + "/{GraphId}";

        public TermGroup()
        {
            MappingHandler = (FromJson input) =>
            {
                // Handle the mapping from json to the domain model for the cases which are not generically handled
                switch (input.TargetType.Name)
                {
                    case "TermGroupScope": return JsonMappingHelper.ToEnum<TermGroupScope>(input.JsonElement);
                }

                input.Log.LogDebug(PnPCoreResources.Log_Debug_JsonCannotMapField, input.FieldName);

                return null;
            };

            // Handler to construct the Add request for this group
            AddApiCallHandler = async (keyValuePairs) =>
            {
                // Define the JSON body of the update request based on the actual changes
                dynamic body = new ExpandoObject();
                body.displayName = Name;
                if (this.IsPropertyAvailable(p => p.Description))
                {
                    body.description = Description;
                }
                if (this.IsPropertyAvailable(p => p.Scope))
                {
                    body.scope = Scope;
                }

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = false });

                var apiCall = await ApiHelper.ParseApiRequestAsync(this, baseUri).ConfigureAwait(false);                

                return new ApiCall(apiCall, ApiType.GraphBeta, bodyContent);
            };
        }
    }
}
