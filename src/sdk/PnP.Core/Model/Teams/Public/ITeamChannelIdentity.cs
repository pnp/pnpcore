namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Defines the channel reference of the message
    /// </summary>
    [ConcreteType(typeof(TeamChannelIdentity))]
    public interface ITeamChannelIdentity : IDataModel<ITeamChannelIdentity>
    {
        /// <summary>
        /// The ID of the channel
        /// </summary>
        public string ChannelId { get; }

        /// <summary>
        /// The ID of the team
        /// </summary>
        public string TeamId { get; set; }
    }
}
