namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(ItemReference))]
    public interface IItemReference
    {
        /// <summary>
        /// 
        /// </summary>
        public string DriveId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DriveType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ShareId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ISharePointIds SharepointIds { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SiteId { get; set; }
    }
}
