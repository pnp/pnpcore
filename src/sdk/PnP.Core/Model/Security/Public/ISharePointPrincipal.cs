namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Represents a SharePoint user or group that can be assigned permissions to control security.
    /// </summary>
    public interface ISharePointPrincipal
    {
        /// <summary>
        /// Gets a value that specifies the member identifier for the user or group.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets a value that indicates whether this member should be hidden in the UI.
        /// </summary>
        public bool IsHiddenInUI { get; }

        /// <summary>
        /// Gets a value containing the type of the principal.
        /// </summary>
        public PrincipalType PrincipalType { get; }

        /// <summary>
        /// Gets the login name of the user.
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// Name of the user
        /// </summary>
        public string Title { get; set; }
    }
}
