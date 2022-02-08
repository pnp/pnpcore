namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Defines which site collections need to be returned by the site collection enumeration methods
    /// </summary>
    public enum SiteCollectionFilter
    {
        /// <summary>
        /// Default filter, currently comes down to all regular site collections and personal site collections (= OneDrive for Business)
        /// </summary>
        Default = 0,

        /// <summary>
        /// Skip personal sites (ODB)
        /// </summary>
        ExcludePersonalSites = 1,
        
        /// <summary>
        /// Return only personal sites (ODB)
        /// </summary>
        OnlyPersonalSites =2
    }
}
