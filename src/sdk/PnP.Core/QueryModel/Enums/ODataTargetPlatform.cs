namespace PnP.Core.QueryModel.Enums
{
    /// <summary>
    /// Defines the target platform for the query
    /// </summary>
    public enum ODataTargetPlatform
    {
        /// <summary>
        /// Microsoft Graph (primary choice)
        /// </summary>
        Graph,
        /// <summary>
        /// Microsoft SharePoint Online REST API (fallback)
        /// </summary>
        SPORest
    }
}
