using Microsoft.Extensions.Logging;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamChatMessageReaction
    {
        public TeamChatMessageReaction()
        {
            MappingHandler = (FromJson input) =>
            {
                switch (input.TargetType.Name)
                {
                    case "ChatMessageReactionType": return JsonMappingHelper.ToEnum<ChatMessageReactionType>(input.JsonElement);
                }

                input.Log.LogDebug(PnPCoreResources.Log_Debug_JsonCannotMapField, input.FieldName);

                return null;
            };
        }
    }
}
