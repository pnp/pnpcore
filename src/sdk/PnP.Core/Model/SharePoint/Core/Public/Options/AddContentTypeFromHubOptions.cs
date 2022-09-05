namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines the options that control the adding of content types from the content type hub
    /// </summary>
    public class AddContentTypeFromHubOptions
    {
        /// <summary>
        /// Wait for the, possibly async, add operation to complete
        /// </summary>
        public bool WaitForCompletion { get; set; } = false;

        /// <summary>
        /// Defines the wait behavior for when the adding happens async
        /// </summary>
        public LongRunningOperationOptions LongRunningOperationOptions { get; set; }
    }
}
