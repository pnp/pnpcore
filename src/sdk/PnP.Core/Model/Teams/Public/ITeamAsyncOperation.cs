using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents a Microsoft Teams async operation
    /// </summary>
    public interface ITeamAsyncOperation
    {
        /// <summary>
        /// Location to call to request the current status of an async operation
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// Waits for the async operation to finish
        /// </summary>
        /// <returns></returns>
        public Task WaitForCompletionAsync();

        /// <summary>
        /// Waits for the async operation to finish
        /// </summary>
        /// <returns></returns>
        public void WaitForCompletion();

    }
}
