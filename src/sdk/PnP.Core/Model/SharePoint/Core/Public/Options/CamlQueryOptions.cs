namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options for making a CAML query to SharePoint Online
    /// </summary>
    public class CamlQueryOptions
    {
        /// <summary>
        /// Caml query to execute
        /// </summary>
        public string ViewXml { get; set; }

        /// <summary>
        /// Can the query return incremental results
        /// </summary>
        public bool? AllowIncrementalResults { get; set; }

        /// <summary>
        /// Return dates in UTC format
        /// </summary>
        public bool? DatesInUtc { get; set; }

        /// <summary>
        /// Specifies the server relative URL of a list folder from which results will be returned
        /// </summary>
        public string FolderServerRelativeUrl { get; set; }

        /// <summary>
        /// Value that specifies information, as name-value pairs, required to get the next page of data for a list view
        /// </summary>
        public string ListItemCollectionPosition { get; set; }
    }
}
