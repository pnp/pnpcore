namespace PnP.Core.Model.AzureActiveDirectory
{
    /// <summary>
    /// The Identity resource represents an identity of an actor. For example, an actor can be a user, device, or application.
    /// </summary>
    public interface IIdentity : IComplexType
    {
        /// <summary>
        /// Unique identifier for the identity.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The identity's display name. Note that this may not always be available or up to date. 
        /// For example, if a user changes their display name, the API may show the new value in a future response, 
        /// but the items associated with the user won't show up as having changed when using delta.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Unique identity of the tenant (optional).
        /// </summary>
        public string TenantId { get; set; }
    }
}
