using Microsoft.Extensions.Logging;
using System.Dynamic;
using System.Text.Json;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = V, Beta = true, LinqGet = baseUri)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class TeamChatMessage
    {
        private const string baseUri = "teams/{Site.GroupId}/channels/{Parent.GraphId}/messages";
        private const string V = baseUri + "/{GraphId}";

        public TeamChatMessage()
        {
            // Handler to construct the Add request for this channel
            AddApiCallHandlerAsync = (keyValuePairs) =>
            {
                // Define the JSON body of the update request based on the actual changes
                dynamic body = new ExpandoObject();
                body.body = new ExpandoObject();
                body.body.content = Body.Content;
                body.body.contentType = Body.ContentType.ToString();

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = false });

                return new ApiCall(ApiHelper.ParseApiRequestAsync(this, baseUri), ApiType.GraphBeta, bodyContent);
            };

            MappingHandler = (FromJson input) =>
            {
                switch (input.TargetType.Name)
                {
                    case "ChatMessageType": return JsonMappingHelper.ToEnum<ChatMessageType>(input.JsonElement);
                    case "ChatMessageImportance": return JsonMappingHelper.ToEnum<ChatMessageImportance>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };
        }
    }
}
