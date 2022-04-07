using PnP.Core.Model.Teams;

namespace PnP.Core.Model.Me
{
    [GraphType]
    internal sealed class ChatMessageContent : BaseDataModel<IChatMessageContent>, IChatMessageContent
    {
        #region Construction
        public ChatMessageContent()
        {
        }
        #endregion

        #region Properties
        public string Content { get => GetValue<string>(); set => SetValue(value); }

        public ChatMessageContentType ContentType { get => GetValue<ChatMessageContentType>(); set => SetValue(value); }
        #endregion
    }
}
