namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Response of an event invite
    /// </summary>
    public enum EventResponse
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Organizer
        /// </summary>
        Organizer,

        /// <summary>
        /// Tentatively Accepted
        /// </summary>
        TentativelyAccepted,

        /// <summary>
        /// Accepted
        /// </summary>
        Accepted,

        /// <summary>
        /// Declined
        /// </summary>
        Declined,

        /// <summary>
        /// No response given
        /// </summary>
        NotResponded
    }
}
