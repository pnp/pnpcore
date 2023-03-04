namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Available options for Teams channel
    /// </summary>
    public class TeamChannelOptions 
    {
        /// <summary>
        /// Gets or sets the channel description.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the channel membership type.
        ///
        /// The membership type cannot be changed for existing channels.
        /// </summary>
        public TeamChannelMembershipType? MembershipType { get; set; }

        /// <summary>
        /// Creates a new `TeamChannelOptions` instance
        /// with the provided description. 
        /// </summary>
        /// <param name="description">The channel description.</param>
        public TeamChannelOptions(string description = null)
        {
            Description = description;
        }
    }
}