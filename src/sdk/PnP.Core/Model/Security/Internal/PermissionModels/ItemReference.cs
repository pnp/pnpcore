namespace PnP.Core.Model.Security
{
    [GraphType]
    internal sealed class ItemReference : IItemReference
    {
        public string DriveId { get; set; }
        public string DriveType { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string ShareId { get; set; }
        public ISharePointIds SharepointIds { get; set; }
        public string SiteId { get; set; }
    }
}
