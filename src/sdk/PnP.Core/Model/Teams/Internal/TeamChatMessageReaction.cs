using System;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class TeamChatMessageReaction : BaseDataModel<ITeamChatMessageReaction>, ITeamChatMessageReaction
    {
        #region Construction
        public TeamChatMessageReaction()
        {
        }
        #endregion

        #region Properties
        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public ChatMessageReactionType ReactionType { get => GetValue<ChatMessageReactionType>(); set => SetValue(value); }

        public ITeamIdentitySet User { get => GetModelValue<ITeamIdentitySet>(); set => SetModelValue(value); }

        [KeyProperty(nameof(CreatedDateTime))]
        public override object Key { get => CreatedDateTime; set => CreatedDateTime = DateTime.Parse(value.ToString()); }
        #endregion
    }
}
