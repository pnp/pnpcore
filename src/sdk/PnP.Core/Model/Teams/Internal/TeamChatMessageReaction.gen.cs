using System;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamChatMessageReaction : BaseComplexType<ITeamChatMessageReaction>, ITeamChatMessageReaction
    {
        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public string ReactionType { get => GetValue<string>(); set => SetValue(value); }

        public ITeamIdentitySet User { get => GetValue<ITeamIdentitySet>(); set => SetValue(value); }
    }
}
