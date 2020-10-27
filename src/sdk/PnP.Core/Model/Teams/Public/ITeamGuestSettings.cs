namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define the guest settings for a Team
    /// </summary>
    [ConcreteType(typeof(TeamGuestSettings))]
    public interface ITeamGuestSettings : IDataModel<ITeamGuestSettings>
    {
        /// <summary>
        /// Defines whether the guests can create or update channels
        /// </summary>
        public bool AllowCreateUpdateChannels { get; set; }

        /// <summary>
        /// Defines whether the guests can delete channels
        /// </summary>
        public bool AllowDeleteChannels { get; set; }
    }
}
