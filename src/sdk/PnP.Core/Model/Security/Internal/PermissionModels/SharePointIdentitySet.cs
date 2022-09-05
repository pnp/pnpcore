namespace PnP.Core.Model.Security
{
    [GraphType]
    internal sealed class SharePointIdentitySet: ISharePointIdentitySet
    {
        public IIdentity Application { get; set; }

        public IIdentity Device { get; set; }

        public IIdentity User { get; set; }

        public IIdentity Group { get; set; }

        public ISharePointIdentity SiteUser { get; set; }

        public ISharePointIdentity SiteGroup { get; set; }
    }
}
