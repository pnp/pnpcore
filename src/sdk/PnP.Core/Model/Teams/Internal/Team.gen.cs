using PnP.Core.Model.Security;
using System;

namespace PnP.Core.Model.Teams
{
    internal partial class Team : BaseDataModel<ITeam>, ITeam
    {
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }
        
        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }
        
        public string Description { get => GetValue<string>(); set => SetValue(value); }
        
        public string InternalId { get => GetValue<string>(); set => SetValue(value); }
        
        public string Classification { get => GetValue<string>(); set => SetValue(value); }
        
        public TeamSpecialization Specialization { get => GetValue<TeamSpecialization>(); set => SetValue(value); }
        
        public TeamVisibility Visibility { get => GetValue<TeamVisibility>(); set => SetValue(value); }
        
        public Uri WebUrl { get => GetValue<Uri>(); set => SetValue(value); }
        
        public bool IsArchived { get => GetValue<bool>(); set => SetValue(value); }
        
        public ITeamMembersSettings MemberSettings { get => GetValue<ITeamMembersSettings>(); set => SetValue(value); }
        
        public ITeamGuestSettings GuestSettings { get => GetValue<ITeamGuestSettings>(); set => SetValue(value); }
        
        public ITeamMessagingSettings MessagingSettings { get => GetValue<ITeamMessagingSettings>(); set => SetValue(value); }
        
        public ITeamFunSettings FunSettings { get => GetValue<ITeamFunSettings>(); set => SetValue(value); }
        
        public ITeamDiscoverySettings DiscoverySettings { get => GetValue<ITeamDiscoverySettings>(); set => SetValue(value); }
        
        public ITeamClassSettings ClassSettings { get => GetValue<ITeamClassSettings>(); set => SetValue(value); }
        
        [GraphProperty("primaryChannel", Expandable = true)]
        public ITeamChannel PrimaryChannel 
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var teamChannel = new TeamChannel
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(teamChannel);
                    InstantiateNavigationProperty();
                }
                return GetValue<ITeamChannel>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        [GraphProperty("channels", ExpandByDefault = true, Get = "teams/{Site.GroupId}/channels")]
        public ITeamChannelCollection Channels
        {
            get
            {
                if (!HasValue(nameof(Channels)))
                {
                    var channels = new TeamChannelCollection(this.PnPContext, this, "Channels");
                    SetValue(channels);
                }
                return GetValue<ITeamChannelCollection>();
            }
        }

        [GraphProperty("installedApps", Get = "teams/{Site.GroupId}/installedapps?$expand=TeamsApp")]
        public ITeamAppCollection InstalledApps
        {
            get
            {
                if (!HasValue(nameof(InstalledApps)))
                {
                    var installedApps = new TeamAppCollection
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(installedApps);
                }
                return GetValue<ITeamAppCollection>();
            }
        }

        [GraphProperty("owners", Get = "groups/{Site.GroupId}/owners")]
        public IGraphUserCollection Owners
        {
            get
            {
                if (!HasValue(nameof(Owners)))
                {
                    var owners = new GraphUserCollection(this.PnPContext, this);
                    SetValue(owners);
                }
                return GetValue<IGraphUserCollection>();
            }
        }

        [GraphProperty("members", Get = "groups/{Site.GroupId}/members")]
        public IGraphUserCollection Members
        {
            get
            {
                if (!HasValue(nameof(Members)))
                {
                    var members = new GraphUserCollection(this.PnPContext, this);

                    SetValue(members);
                }
                return GetValue<IGraphUserCollection>();
            }
        }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
