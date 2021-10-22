namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Indicates whether company wide sharing links are disabled in all the webs of this site. 
    /// </summary>
    /// <remarks>
    /// Note that while this property being Disabled guarantees that no company wide sharing links
    /// are available in any of the child webs, the opposite is not the case. In other words, if this property
    /// is NotDisabled, it does not guarantee that company wide sharing links are available in all of the child
    /// webs. That decision is left up to the individual webs.
    /// </remarks>
    public enum CompanyWideSharingLinksPolicy
    {
        /// <summary>
        /// Internally used
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Indicates that company wide sharing links are disabled in all the webs of this site
        /// </summary>
        Disabled = 1,

        /// <summary>
        /// Indicates that company wide sharing links may be enabled in some of the webs of this site
        /// </summary>
        NotDisabled = 2
    }
}
