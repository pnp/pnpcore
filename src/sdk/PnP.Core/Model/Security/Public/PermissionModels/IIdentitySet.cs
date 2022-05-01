namespace PnP.Core.Model.Security
{
    /// <summary>
    /// The identitySet resource is a keyed collection of identity resources.
    /// </summary>
    [ConcreteType(typeof(IdentitySet))]
    public interface IIdentitySet
    {
        /// <summary>
        /// The application associated with this action.
        /// </summary>
        public IIdentity Application { get; }

        /// <summary>
        /// The device associated with this action.
        /// </summary>
        public IIdentity Device { get; }

        /// <summary>
        /// The user associated with this action.
        /// </summary>
        public IIdentity User { get; }
    }
}
