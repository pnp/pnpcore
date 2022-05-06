namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Defines the Membership Type for a Team Channel
    /// </summary>
    public enum TeamChannelMembershipType
    {
        /// <summary>
        /// Standard channel membership
        /// </summary>
        Standard,

        /// <summary>
        /// Private channel membership
        /// </summary>
        Private,

        /// <summary>
        /// Shared channel membership
        /// </summary>
        Shared,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        UnknownFutureValue,
    }
}
