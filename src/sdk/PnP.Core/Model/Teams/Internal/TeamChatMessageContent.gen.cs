using Microsoft.Extensions.Logging;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamChatMessageContent : BaseDataModel<ITeamChatMessageContent>, ITeamChatMessageContent
    {        
        public TeamChatMessageContent()
        {
            MappingHandler = (FromJson input) =>
            {
                switch (input.TargetType.Name)
                {
                    case "ChatMessageContentType": return JsonMappingHelper.ToEnum<ChatMessageContentType>(input.JsonElement);
                }

                input.Log.LogDebug(PnPCoreResources.Log_Debug_JsonCannotMapField, input.FieldName);

                return null;
            };
        }
        

        public string Content { get => GetValue<string>(); set => SetValue(value); }

        public ChatMessageContentType ContentType { get => GetValue<ChatMessageContentType>(); set => SetValue(value); }
    }
}
