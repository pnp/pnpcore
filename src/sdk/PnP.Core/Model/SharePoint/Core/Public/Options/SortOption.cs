namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines a field to be used for sorting search query results
    /// </summary>
    public class SortOption
    {
        /// <summary>
        /// Creates SortOption class, defaults to ascending sort order
        /// </summary>
        /// <param name="property">Property to sort on</param>
        public SortOption(string property)
        {
            Property = property;
            Sort = SortDirection.Ascending;
        }

        /// <summary>
        /// Creates SortOption class
        /// </summary>
        /// <param name="property">Property to sort on</param>
        /// <param name="sort">Sort order to use</param>
        public SortOption(string property, SortDirection sort)
        {
            Property = property;
            Sort = sort;
        }

        /// <summary>
        /// Property to sort on
        /// </summary>
        public string Property { get; internal set; }

        /// <summary>
        /// Sort order to use
        /// </summary>
        public SortDirection Sort { get; internal set; }

        /// <summary>
        /// Returns the sort option so it can be used in a search query
        /// </summary>
        /// <returns>Sort option to use in a query</returns>
        public override string ToString()
        {
            return $"{Property}:{Sort.ToString().ToLowerInvariant()}";
        }
    }
}
