namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Defines the possible sources from where a SharePoint AddIn can be acquiredd
    /// </summary>
    public enum SharePointAddInSource
    {
        /// <summary>
        /// Marketplace
        /// </summary>
        Marketplace,

        /// <summary>
        /// CorporateCatalog
        /// </summary>
        CorporateCatalog,

        /// <summary>
        /// DeveloperSite
        /// </summary>
        DeveloperSite,

        /// <summary>
        /// ObjectModel
        /// </summary>
        ObjectModel,

        /// <summary>
        /// RemoteObjectModel
        /// </summary>
        RemoteObjectModel,

        /// <summary>
        /// SiteCollectionCorporateCatalog
        /// </summary>
        SiteCollectionCorporateCatalog,

        /// <summary>
        /// InvalidSource
        /// </summary>
        InvalidSource
    }
}
