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

        public ITeamMembersSettings MemberSettings { get => GetModelValue<ITeamMembersSettings>(); }

        public ITeamGuestSettings GuestSettings { get => GetModelValue<ITeamGuestSettings>(); }

        public ITeamMessagingSettings MessagingSettings { get => GetModelValue<ITeamMessagingSettings>(); }

        public ITeamFunSettings FunSettings { get => GetModelValue<ITeamFunSettings>(); }

        public ITeamDiscoverySettings DiscoverySettings { get => GetModelValue<ITeamDiscoverySettings>(); }

        public ITeamClassSettings ClassSettings { get => GetModelValue<ITeamClassSettings>(); }

        [GraphProperty("primaryChannel", Expandable = true)]
        public ITeamChannel PrimaryChannel { get => GetModelValue<ITeamChannel>(); }

        [GraphProperty("channels", Get = "teams/{Site.GroupId}/channels")]
        public ITeamChannelCollection Channels { get => GetModelCollectionValue<ITeamChannelCollection>(); }

        [GraphProperty("installedApps", Get = "teams/{Site.GroupId}/installedapps?$expand=TeamsApp")]
        public ITeamAppCollection InstalledApps { get => GetModelCollectionValue<ITeamAppCollection>(); }

        [GraphProperty("owners", Get = "groups/{Site.GroupId}/owners")]
        public IGraphUserCollection Owners { get => GetModelCollectionValue<IGraphUserCollection>(); }

        [GraphProperty("members", Get = "groups/{Site.GroupId}/members")]
        public IGraphUserCollection Members { get => GetModelCollectionValue<IGraphUserCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
