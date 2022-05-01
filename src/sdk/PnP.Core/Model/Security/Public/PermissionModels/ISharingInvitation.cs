namespace PnP.Core.Model.Security
{
    /// <summary>
    /// The SharingInvitation resource groups invitation-related data items into a single structure.
    /// </summary>
    [ConcreteType(typeof(SharingInvitation))]
    public interface ISharingInvitation
    {
        /// <summary>
        /// The email address provided for the recipient of the sharing invitation. Read-only.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Provides information about who sent the invitation that created this permission, if that information is available. Read-only.
        /// </summary>
        public IIdentitySet InvitedBy { get; }

        /// <summary>
        /// If true the recipient of the invitation needs to sign in in order to access the shared item. Read-only.
        /// </summary>
        public bool SignInRequired { get; }
    }
}
