namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Defines the status of the event in the calendar
    /// </summary>
    public enum EventShowAs
    {
        /// <summary>
        /// Free
        /// </summary>
        Free,

        /// <summary>
        /// Teams for business meeting
        /// </summary>
        Tentative,

        /// <summary>
        /// Busy
        /// </summary>
        Busy,

        /// <summary>
        /// Oof
        /// </summary>
        Oof,

        /// <summary>
        /// Working elsewhere
        /// </summary>
        WorkingElsewhere,

        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    }
}
