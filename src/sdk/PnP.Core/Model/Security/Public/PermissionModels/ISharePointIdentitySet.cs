namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Represents a keyed collection of sharePointIdentity resources. This resource extends from the identitySet resource to provide the ability to expose SharePoint-specific information to the user.
    /// </summary>
    [ConcreteType(typeof(SharePointIdentitySet))]
    public interface ISharePointIdentitySet
    {
        /// <summary>
        /// The application associated with this action. Optional.
        /// </summary>
        public IIdentity Application { get; }

        /// <summary>
        /// The device associated with this action. Optional.
        /// </summary>
        public IIdentity Device { get; }

        /// <summary>
        /// The user associated with this action. Optional.
        /// </summary>
        public IIdentity User { get; }

        /// <summary>
        /// The group associated with this action. Optional.
        /// </summary>
        public IIdentity Group { get; }

        /// <summary>
        /// The SharePoint user associated with this action. Optional.
        /// </summary>
        public ISharePointIdentity SiteUser { get; }

        /// <summary>
        /// The SharePoint group associated with this action. Optional.
        /// </summary>
        public ISharePointIdentity SiteGroup { get; }
    }
}
