namespace PnP.Core.Admin.Model.Microsoft365
{
    /// <summary>
    /// Contains information about a geo location
    /// </summary>
    public interface IGeoLocationInformation
    {
        /// <summary>
        /// Data location code for the geo location
        /// </summary>
        GeoLocation DataLocationCode { get; }

        /// <summary>
        /// SharePoint portal url for the geo location
        /// </summary>
        string SharePointPortalUrl { get; }

        /// <summary>
        /// SharePoint Admin site url for the geo location
        /// </summary>
        string SharePointAdminUrl { get; }

        /// <summary>
        /// SharePoint My site url for the geo location
        /// </summary>
        string SharePointMySiteUrl { get; }
    }
}
