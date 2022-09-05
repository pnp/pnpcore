using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a Microsoft graph async, long running, operation
    /// </summary>
    public interface ILongRunningOperation
    {
        /// <summary>
        /// Location to call to request the current status of an async, long running, operation
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// Waits for the async, long running, operation to finish
        /// </summary>
        /// <returns></returns>
        public Task WaitForCompletionAsync(LongRunningOperationOptions options = null);

        /// <summary>
        /// Waits for the async, long running, operation to finish
        /// </summary>
        /// <returns></returns>
        public void WaitForCompletion(LongRunningOperationOptions options = null);
    }
}
