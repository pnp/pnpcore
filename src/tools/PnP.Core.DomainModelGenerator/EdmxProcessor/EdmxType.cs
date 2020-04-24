namespace PnP.M365.DomainModelGenerator
{
    /// <summary>
    /// Defines the flavors of Edmx to process
    /// </summary>
    internal enum EdmxType
    {
        /// <summary>
        /// The SharePoint REST EDMX metadata
        /// </summary>
        SharePointREST,
        /// <summary>
        /// The Microsoft Graph EDMX metadata
        /// </summary>
        MicrosoftGraph
    }
}
