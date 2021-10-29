namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Indicates whether an anonymous link (also known as share-by-link) should be included
    /// in an invitation, and if so, what permissions should be granted via that link.  
    /// The anonymous link will be created once the invitation is created
    /// </summary>
    public enum AnonymousLinkType
    {
        /// <summary>
        /// No anonymous link is desired
        /// </summary>
        None = 0,

        /// <summary>
        /// A view only anonymous link
        /// </summary>
        View,

        /// <summary>
        /// A read/write anonymous link
        /// </summary>
        Edit
    }
}
