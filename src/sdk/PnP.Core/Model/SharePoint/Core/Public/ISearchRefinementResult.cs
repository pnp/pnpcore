namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Search refinement result
    /// </summary>
    public interface ISearchRefinementResult
    {
        /// <summary>
        /// Number of matches for this refinement
        /// </summary>
        public long Count { get; }

        /// <summary>
        /// Refinement name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Refinement token
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// Refinement value
        /// </summary>
        public string Value { get; }
    }
}
