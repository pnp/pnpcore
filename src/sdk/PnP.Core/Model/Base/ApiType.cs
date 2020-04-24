namespace PnP.Core.Model
{
    /// <summary>
    /// Possible API's used to talk to data stores
    /// </summary>
    internal enum ApiType
    {
        /// <summary>
        /// Using the SharePoint REST API
        /// </summary>
        SPORest = 0,

        /// <summary>
        /// Using the Microsoft Graph REST API
        /// </summary>
        Graph = 1
    }
}
