namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = "teams/{Parent.GraphId}/installedapps/{GraphId}")]
    internal sealed class TeamApp : BaseDataModel<ITeamApp>, ITeamApp
    {
        #region Construction
        public TeamApp()
        {
        }
        #endregion

        #region Properties
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("teamsApp", JsonPath = "id")]
        public string ExternalId { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("teamsApp", JsonPath = "displayName")]
        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("teamsApp", JsonPath = "distributionMethod")]
        public TeamsAppDistributionMethod DistributionMethod { get => GetValue<TeamsAppDistributionMethod>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (string)value; }
        #endregion
    }
}
