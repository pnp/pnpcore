namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Represents a SharePoint sharing principal.
    /// </summary>
    [ConcreteType(typeof(SharePointSharingPrincipal))]
    public interface ISharePointSharingPrincipal : IDataModel<ISharePointSharingPrincipal>
    {
        /// <summary>
        /// Gets a value that specifies the member identifier for the user or group.
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// Gets a value containing the type of the principal.
        /// </summary>
        public PrincipalType PrincipalType { get; }

        /// <summary>
        /// Gets the login name of the user.
        /// </summary>
        public string LoginName { get; }

        /// <summary>
        /// User principle name (UPN) of the user.
        /// </summary>
        public string UserPrincipalName { get; }

        /// <summary>
        /// E-mail address of the user.
        /// SP REST property name: Email
        /// </summary>
        public string Mail { get; }

        /// <summary>
        /// When does this sharing principal expire?
        /// </summary>
        public string Expiration { get; }

        /// <summary>
        /// Is this user still active?
        /// </summary>
        public bool IsActive { get;  }

        /// <summary>
        /// Is this user an external user?
        /// </summary>
        public bool IsExternal { get; }

        /// <summary>
        /// Gets the job title of the user.
        /// </summary>
        public string JobTitle { get; }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        public string Name { get; }
    }
}
