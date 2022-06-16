namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines the options that control the long running operation <see cref="ILongRunningOperation"/> polling
    /// </summary>
    public class LongRunningOperationOptions
    {
        /// <summary>
        /// Time in seconds between each completion check, defaults to 5 if not set
        /// </summary>
        public int? WaitTimeInSeconds { get; set; }

        /// <summary>
        /// Maximum number of retries, defaults to unlimited (so wait for the long operation to either succeed or fail)
        /// </summary>
        public int? MaxRetries { get; set; }
    }
}
