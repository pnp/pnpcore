namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Meeting providers for an online meeting
    /// </summary>
    public enum EventOnlineMeetingProvider
    {
        /// <summary>
        /// Default value, no online meeting
        /// </summary>
        Unknown,

        /// <summary>
        /// Teams for business meeting
        /// </summary>
        TeamsForBusiness,

        /// <summary>
        /// Skype for business meeting
        /// </summary>
        SkypeForBusiness,

        /// <summary>
        /// Skype for consumer meeting
        /// </summary>
        SkypeForConsumer
    }
}
