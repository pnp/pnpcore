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
        /// Number of rows to return
        /// </summary>
        public int? RowLimit { get; set; }

        /// <summary>
        /// Trim duplicate search results
        /// </summary>
        public bool TrimDuplicates { get; set; } = false;

        /// <summary>
        /// Properties to select
        /// </summary>
        public List<string> SelectProperties { get; set; } = new List<string>();

        /// <summary>
        /// Client type used for the search query (read only)
        /// </summary>
        public string ClientType { get; internal set; } = "ContentSearchRegular";
    }
}
