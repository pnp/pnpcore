using System;
using PnP.Core.Model.Teams.Public.Enums;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamChatMessageReaction : BaseComplexType<ITeamChatMessageReaction>, ITeamChatMessageReaction
    {
        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public ChatMessageReactionType ReactionType { get => GetValue<ChatMessageReactionType>(); set => SetValue(value); }

        public ITeamIdentitySet User { get => GetValue<ITeamIdentitySet>(); set => SetValue(value); }
    }
}
