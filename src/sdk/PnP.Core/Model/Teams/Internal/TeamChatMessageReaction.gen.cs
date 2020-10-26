using System;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamChatMessageReaction : BaseDataModel<ITeamChatMessageReaction>, ITeamChatMessageReaction
    {
        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public ChatMessageReactionType ReactionType { get => GetValue<ChatMessageReactionType>(); set => SetValue(value); }

        //public ITeamIdentitySet User { get => GetValue<ITeamIdentitySet>(); set => SetValue(value); }
        public ITeamIdentitySet User
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var teamIdentitySet = new TeamIdentitySet
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(teamIdentitySet);
                    InstantiateNavigationProperty();
                }
                return GetValue<ITeamIdentitySet>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }
    }
}
