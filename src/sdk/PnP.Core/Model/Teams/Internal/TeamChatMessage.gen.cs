using System;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamChatMessage : BaseDataModel<ITeamChatMessage>, ITeamChatMessage
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string ReplyToId { get => GetValue<string>(); set => SetValue(value); }

        public ITeamIdentitySet From { get => GetModelValue<ITeamIdentitySet>(); }

        public string Etag { get => GetValue<string>(); set => SetValue(value); }

        public ChatMessageType MessageType { get => GetValue<ChatMessageType>(); set => SetValue(value); }

        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public DateTimeOffset LastModifiedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public DateTimeOffset DeletedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public string Subject { get => GetValue<string>(); set => SetValue(value); }

        public ITeamChatMessageContent Body { get => GetModelValue<ITeamChatMessageContent>(); set => SetModelValue(value); }

        public string Summary { get => GetValue<string>(); set => SetValue(value); }

        public Uri WebUrl { get => GetValue<Uri>(); set => SetValue(value); }

        public string Locale { get => GetValue<string>(); set => SetValue(value); }

        public ChatMessageImportance Importance { get => GetValue<ChatMessageImportance>(); set => SetValue(value); }

        public ITeamChatMessageReactionCollection Reactions { get => GetModelCollectionValue<ITeamChatMessageReactionCollection>(); }

        public ITeamChatMessageMentionCollection Mentions { get => GetModelCollectionValue<ITeamChatMessageMentionCollection>(); }

        public ITeamChatMessageAttachmentCollection Attachments { get => GetModelCollectionValue<ITeamChatMessageAttachmentCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
    }
}
