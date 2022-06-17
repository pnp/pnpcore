namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Statistics for a given action
    /// </summary>
    public interface IActivityActionStat
    {
        /// <summary>
        /// The number of times the action took place.
        /// </summary>
        int ActionCount { get; }

        /// <summary>
        /// The number of distinct actors that performed the action.
        /// </summary>
        int ActorCount { get; }

        /// <summary>
        /// The time (seconds) spent for the performed action.
        /// </summary>
        int TimeSpentInSeconds { get; }
    }
}
