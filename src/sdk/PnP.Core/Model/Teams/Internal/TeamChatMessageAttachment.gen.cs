using System;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamChatMessageAttachment : BaseDataModel<ITeamChatMessageAttachment>, ITeamChatMessageAttachment
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string ContentType { get => GetValue<string>(); set => SetValue(value); }

        public Uri ContentUrl { get => GetValue<Uri>(); set => SetValue(value); }

        public string Content { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public Uri ThumbnailUrl { get => GetValue<Uri>(); set => SetValue(value); }
    }
}
