namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class TeamChatMessageContent : BaseDataModel<ITeamChatMessageContent>, ITeamChatMessageContent
    {
        #region Construction
        public TeamChatMessageContent()
        {
        }
        #endregion

        #region Properties
        public string Content { get => GetValue<string>(); set => SetValue(value); }

        public ChatMessageContentType ContentType { get => GetValue<ChatMessageContentType>(); set => SetValue(value); }
        #endregion
    }
}
