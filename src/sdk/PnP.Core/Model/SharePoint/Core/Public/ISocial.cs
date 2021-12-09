namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Entry point for the social-related APIs
    /// </summary>
    [ConcreteType(typeof(Social))]
    public interface ISocial : IDataModelWithContext
    {
        /// <summary>
        /// A reference to a user profile-related operations
        /// </summary>
        IUserProfile UserProfile { get; }

        /// <summary>
        /// An entry point for the social following APIs
        /// </summary>
        IFollowing Following { get; }
    }
}
