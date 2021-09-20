namespace PnP.Core.Services
{
    /// <summary>
    /// Types of API requests that can be executed
    /// </summary>
    public enum ApiRequestType
    {
        /// <summary>
        /// Using the SharePoint REST API
        /// </summary>
        SPORest = 0,

        /// <summary>
        /// Using the production v1 Microsoft Graph REST API
        /// </summary>
        Graph = 1,

        /// <summary>
        /// Using the beta Microsoft Graph REST API
        /// </summary>
        GraphBeta = 2,
    }
}
