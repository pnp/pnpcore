using System;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamChannel : BaseDataModel<ITeamChannel>, ITeamChannel
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }
        
        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }
        
        public string Description { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("isFavoriteByDefault", Beta = true)]
        public bool IsFavoriteByDefault { get => GetValue<bool>(); set => SetValue(value); }
        
        public string Email { get => GetValue<string>(); set => SetValue(value); }
        
        public TeamChannelMembershipType MembershipType { get => GetValue<TeamChannelMembershipType>(); set => SetValue(value); }
        
        public Uri WebUrl { get => GetValue<Uri>(); set => SetValue(value); }

        [GraphProperty("tabs", Get = "teams/{Site.GroupId}/channels/{GraphId}/tabs?$expand=teamsApp")]
        public ITeamChannelTabCollection Tabs { get => GetModelCollectionValue<ITeamChannelTabCollection>(); }

        [GraphProperty("messages", Get = "teams/{Site.GroupId}/channels/{GraphId}/messages", Beta = true)]
        public ITeamChatMessageCollection Messages { get => GetModelCollectionValue<ITeamChatMessageCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = (string)value; }

    }
}
