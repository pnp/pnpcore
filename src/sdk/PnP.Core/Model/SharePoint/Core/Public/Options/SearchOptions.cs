using System;
using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines the search query to make
    /// </summary>
    public class SearchOptions
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="query">Provide the query to issue</param>
        public SearchOptions(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            Query = query;
        }

        /// <summary>
        /// Search query to issue
        /// </summary>
        public string Query { get; internal set; }

        /// <summary>
        /// Max number of rows to return
        /// </summary>
        public int? RowLimit { get; set; }

        /// <summary>
        /// Start row of the search result set
        /// </summary>
        public int? StartRow { get; set; }

        /// <summary>
        /// Rows to return per page. Defaults to 500
        /// </summary>
        public int? RowsPerPage { get; set; } = 500;

        /// <summary>
        /// Trim duplicate search results
        /// </summary>
        public bool TrimDuplicates { get; set; } = false;

        /// <summary>
        /// Properties to select
        /// </summary>
        public List<string> SelectProperties { get; set; } = new List<string>();

        /// <summary>
        /// Properties to sort the query on
        /// </summary>
        public List<SortOption> SortProperties { get; set; } = new List<SortOption>();

        /// <summary>
        /// Properties used to refine the search results
        /// </summary>
        public List<string> RefineProperties { get; set; } = new List<string>();

        /// <summary>
        /// The set of refinement filters used when issuing a refinement query
        /// </summary>
        public List<string> RefinementFilters { get; set; } = new List<string>();

        /// <summary>
        /// Client type used for the search query (defaults to ContentSearchRegular)
        /// </summary>
        public string ClientType { get; set; } = "ContentSearchRegular";

        /// <summary>
        /// Result source id to use for the search query
        /// </summary>
        public string ResultSourceId { get; set; } = "8413cd39-2156-4e00-b54d-11efd9abdb89";
    }
}