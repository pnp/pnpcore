using PnP.Core.Model.Teams;
using System;

namespace PnP.Core.Model.Me
{
    [GraphType]
    internal sealed class ChatMessageReaction : BaseDataModel<IChatMessageReaction>, IChatMessageReaction
    {
        #region Construction
        public ChatMessageReaction()
        {
        }
        #endregion

        #region Properties
        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public ChatMessageReactionType ReactionType { get => GetValue<ChatMessageReactionType>(); set => SetValue(value); }

        public IChatIdentitySet User { get => GetModelValue<IChatIdentitySet>(); set => SetModelValue(value); }

        [KeyProperty(nameof(CreatedDateTime))]
        public override object Key { get => CreatedDateTime; set => CreatedDateTime = DateTime.Parse(value.ToString()); }
        #endregion
    }
}
