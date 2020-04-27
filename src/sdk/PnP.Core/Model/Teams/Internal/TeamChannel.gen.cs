using System;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamChannel : BaseDataModel<ITeamChannel>, ITeamChannel
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }
        
        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }
        
        public string Description { get => GetValue<string>(); set => SetValue(value); }
        
        public bool IsFavoriteByDefault { get => GetValue<bool>(); set => SetValue(value); }
        
        public string Email { get => GetValue<string>(); set => SetValue(value); }
        
        public TeamChannelMembershipType MembershipType { get => GetValue<TeamChannelMembershipType>(); set => SetValue(value); }
        
        public Uri WebUrl { get => GetValue<Uri>(); set => SetValue(value); }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = (string)value; }
    }
}
