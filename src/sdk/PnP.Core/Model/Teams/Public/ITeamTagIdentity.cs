using PnP.Core.Model.Security;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents a tag in Microsoft Teams. Tags allow users to quickly connect to subset of users in a team.
    /// </summary>
    [ConcreteType(typeof(TeamTagIdentity))]
    public interface ITeamTagIdentity : IDataModel<ITeamTagIdentity>
    {
        /// <summary>
        /// Display name of the tag.
        /// </summary>
        public string DisplayName{ get; set; }

        /// <summary>
        /// ID of the tag.
        /// </summary>
        public string Id { get; set; }
    }
}
