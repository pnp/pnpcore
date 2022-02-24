using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Result of search query
    /// </summary>
    public interface ISearchResult
    {
        /// <summary>
        /// Time in milliseconds the search query took server side
        /// </summary>
        public int ElapsedTime { get; }

        /// <summary>
        /// Total rows that match the provided query
        /// </summary>
        public long TotalRows { get; }

        /// <summary>
        /// Total rows, including duplicates, that match the provided query
        /// </summary>
        public long TotalRowsIncludingDuplicates { get; }

        /// <summary>
        /// Collection of search results
        /// </summary>
        public List<Dictionary<string, object>> Rows { get; }

        /// <summary>
        /// Collection of refinement results
        /// </summary>
        public Dictionary<string, List<ISearchRefinementResult>> Refinements { get; }

    }
}
