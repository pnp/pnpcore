using Microsoft.Extensions.Logging;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = V, Beta = true)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class TeamChatMessage
    {
        private const string baseUri = "chats/{Parent.GraphId}/messages";
        private const string V = baseUri + "/{GraphId}";

        internal TeamChatMessage()
        {
            MappingHandler = (FromJson input) =>
            {
                switch (input.TargetType.Name)
                {
                    case "ChatMessageType": return JsonMappingHelper.ToEnum<ChatMessageType>(input.JsonElement);
                    case "ChatMessageImportance": return JsonMappingHelper.ToEnum<ChatMessageImportance>(input.JsonElement);
                }

                input.Log.LogWarning($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };
        }
    }
}
