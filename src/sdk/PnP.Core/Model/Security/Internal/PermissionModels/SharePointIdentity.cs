namespace PnP.Core.Model.Security
{
    [GraphType]
    internal sealed class SharePointIdentity : ISharePointIdentity
    {
        public string DisplayName { get; set; }
        public string Id { get; set; }
        public string LoginName { get; set; }

    }
}
