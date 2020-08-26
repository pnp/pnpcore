using Microsoft.Extensions.Logging;
using System.Dynamic;
using System.Text.Json;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = V, LinqGet = baseUri)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class TeamChannel
    {
        private const string baseUri = "teams/{Parent.GraphId}/channels";
        private const string V = baseUri + "/{GraphId}";

        public TeamChannel()
        {
            MappingHandler = (FromJson input) =>
            {
                switch (input.TargetType.Name)
                {
                    case "TeamChannelMembershipType": return JsonMappingHelper.ToEnum<TeamChannelMembershipType>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };

            // Handler to construct the Add request for this channel
            AddApiCallHandler = async (keyValuePairs) =>
            {
                // Define the JSON body of the update request based on the actual changes
                dynamic body = new ExpandoObject();
                body.displayName = DisplayName;
                if (!string.IsNullOrEmpty(Description))
                {
                    body.description = Description;
                }

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = false });

                var apiCall = await ApiHelper.ParseApiRequestAsync(this, baseUri).ConfigureAwait(false);

                return new ApiCall(apiCall, ApiType.Graph, bodyContent);
            };

            // Validation handler to prevent updating the general channel
            ValidateUpdateHandler = (PropertyUpdateRequest propertyUpdateRequest) =>
            {
                // Prevent setting all values on the general channel
                if (DisplayName == "General")
                {
                    propertyUpdateRequest.CancelUpdate("Updating the general channel is not allowed.");
                }
            };

            UpdateApiCallOverrideHandler = async (ApiCallRequest apiCallRequest) =>
            {
                if (DisplayName == "General")
                {
                    apiCallRequest.CancelRequest("Updating the general channel is not allowed.");
                }

                return apiCallRequest;
            };

            // Check delete, block when needed 
            DeleteApiCallOverrideHandler = async (ApiCallRequest apiCallRequest) =>
            {
                if (DisplayName == "General")
                {
                    apiCallRequest.CancelRequest("Deleting the general channel is not allowed.");
                }

                return apiCallRequest;
            };

        }
    }
}
