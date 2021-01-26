namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the very basic interface for every collection that is somehow requestable
    /// through an external querying system
    /// </summary>
    public interface IRequestableCollection
    {
        /// <summary>
        /// Was this collection requested, a collection with 0 items and Requested == false was never loaded
        /// </summary>
        /// <returns></returns>
        bool Requested { get; set; }

        /// <summary>
        /// Returns the size of the collection
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Represents the items already requested from 
        /// the back-end service
        /// </summary>
        public System.Collections.IEnumerable RequestedItems { get; }

        /// <summary>
        /// Clears the collection allowing it to be freshly loaded again
        /// </summary>
        public void Clear();
    }
}
