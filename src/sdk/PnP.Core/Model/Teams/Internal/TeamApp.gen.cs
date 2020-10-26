namespace PnP.Core.Model.Teams
{
    internal partial class TeamApp : BaseDataModel<ITeamApp>, ITeamApp
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("teamsApp", JsonPath = "id")]
        public string ExternalId { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("teamsApp", JsonPath = "displayName")]
        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("teamsApp", JsonPath = "distributionMethod")]
        public TeamsAppDistributionMethod DistributionMethod { get => GetValue<TeamsAppDistributionMethod>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = (string)value; }
    }
}
