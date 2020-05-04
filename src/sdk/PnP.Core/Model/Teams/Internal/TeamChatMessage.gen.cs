using System;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamChatMessage : BaseDataModel<ITeamChatMessage>, ITeamChatMessage
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string ReplyToId { get => GetValue<string>(); set => SetValue(value); }

        public ITeamIdentitySet From { get => GetValue<ITeamIdentitySet>(); set => SetValue(value); }

        public string Etag { get => GetValue<string>(); set => SetValue(value); }

        public ChatMessageType MessageType { get => GetValue<ChatMessageType>(); set => SetValue(value); }

        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public DateTimeOffset LastModifiedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public DateTimeOffset DeletedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public string Subject { get => GetValue<string>(); set => SetValue(value); }

        public ITeamChatMessageContent Body { get => GetValue<ITeamChatMessageContent>(); set => SetValue(value); }

        public string Summary { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
    }
}
