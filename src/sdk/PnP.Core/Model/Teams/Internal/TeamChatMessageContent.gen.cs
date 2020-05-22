using Microsoft.Extensions.Logging;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamChatMessageContent : BaseComplexType<ITeamChatMessageContent>, ITeamChatMessageContent
    {        
        public TeamChatMessageContent()
        {
            MappingHandler = (FromJson input) =>
            {
                switch (input.TargetType.Name)
                {
                    case "ChatMessageContentType": return JsonMappingHelper.ToEnum<ChatMessageContentType>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };
        }
        

        public string Content { get => GetValue<string>(); set => SetValue(value); }

        public ChatMessageContentType ContentType { get => GetValue<ChatMessageContentType>(); set => SetValue(value); }
    }
}
