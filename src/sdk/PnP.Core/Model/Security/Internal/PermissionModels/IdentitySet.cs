namespace PnP.Core.Model.Security
{
    [GraphType]
    internal sealed class IdentitySet : IIdentitySet
    {
        public IIdentity Application { get; set; }

        public IIdentity Device { get; set; }

        public IIdentity User { get; set; }
    }
}
