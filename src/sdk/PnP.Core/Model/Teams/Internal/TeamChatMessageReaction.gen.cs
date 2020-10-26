using System;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamChatMessageReaction : BaseDataModel<ITeamChatMessageReaction>, ITeamChatMessageReaction
    {
        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public ChatMessageReactionType ReactionType { get => GetValue<ChatMessageReactionType>(); set => SetValue(value); }

        public ITeamIdentitySet User { get => GetModelValue<ITeamIdentitySet>(); set => SetModelValue(value); }

        [KeyProperty(nameof(CreatedDateTime))]
        public override object Key { get => this.CreatedDateTime; set => this.CreatedDateTime = DateTime.Parse(value.ToString()); }
    }
}
