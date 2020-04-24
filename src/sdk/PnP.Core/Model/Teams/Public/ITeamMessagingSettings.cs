namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define the messaging settings for a Team
    /// </summary>
    public interface ITeamMessagingSettings: IComplexType
    {
        /// <summary>
        /// Defines whether users can edit messages
        /// </summary>
        public bool AllowUserEditMessages { get; set; }

        /// <summary>
        /// Defines whether users can delete messages
        /// </summary>
        public bool AllowUserDeleteMessages { get; set; }

        /// <summary>
        /// Defines whether owners can delete messages
        /// </summary>
        public bool AllowOwnerDeleteMessages { get; set; }

        /// <summary>
        /// Defines whether users can use team mentions
        /// </summary>
        public bool AllowTeamMentions { get; set; }

        /// <summary>
        /// Defines whether users can use channel mentions
        /// </summary>
        public bool AllowChannelMentions { get; set; }
    }
}
