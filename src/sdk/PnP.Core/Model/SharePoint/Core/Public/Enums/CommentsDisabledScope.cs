namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// An enum to determine the scope for which comments are disabled.
    /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.sharepoint.comments.commentsdisabledscope?view=sharepoint-csom
    /// </summary>
    public enum CommentsDisabledScope
    {
        /// <summary>
        /// No Scope
        /// </summary>
        None = 0,

        /// <summary>
        /// Item Scoped
        /// </summary>
        Item = 1,

        /// <summary>
        /// Web Scoped
        /// </summary>
        Web = 2,

        /// <summary>
        /// Tenant Scoped
        /// </summary>
        Tenant = 3,

        /// <summary>
        /// Site Scoped
        /// </summary>
        Site = 4
    }
}
