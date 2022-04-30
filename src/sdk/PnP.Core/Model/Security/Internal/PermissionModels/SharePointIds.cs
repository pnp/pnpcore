namespace PnP.Core.Model.Security
{
    [GraphType]
    internal sealed class SharePointIds : ISharePointIds
    {
        public string ListId { get; set; }
        public string ListItemId { get; set; }
        public string ListItemUniqueId { get; set; }
        public string SiteId { get; set; }
        public string SiteUrl { get; set; }
        public string TenantId { get; set; }
        public string WebId { get; set; }
    }
}
