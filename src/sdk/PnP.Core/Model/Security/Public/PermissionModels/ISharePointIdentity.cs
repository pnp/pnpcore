namespace PnP.Core.Model.Security
{
    /// <summary>
    /// This resource extends from the identity resource to provide the ability to expose SharePoint-specific information; for example, loginName or SharePoint IDs.
    /// </summary>
    [ConcreteType(typeof(SharePointIdentity))]
    public interface ISharePointIdentity
    {
        /// <summary>
        /// The identity's display name. Note that this might not always be available or up to date. For example, if a user changes their display name, the API may show the new value in a future response, but the items associated with the user won't show up as having changed when using delta.
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// Unique identifier for the identity. It can be either an Azure Active Directory ID or a SharePoint ID.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// The sign in name of the SharePoint identity.
        /// </summary>
        public string LoginName { get; }

        /// <summary>
        /// The email of the SharePoint identity.
        /// </summary>
        public string Email { get; }

    }
}
