namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(SharePointIds))]
    public interface ISharePointIds
    {
        /// <summary>
        /// 
        /// </summary>
        public string ListId { get; }
        /// <summary>
        /// 
        /// </summary>
        public string ListItemId { get; }
        /// <summary>
        /// 
        /// </summary>
        public string ListItemUniqueId { get; }
        /// <summary>
        /// 
        /// </summary>
        public string SiteId { get; }
        /// <summary>
        /// 
        /// </summary>
        public string SiteUrl { get; }
        /// <summary>
        /// 
        /// </summary>
        public string TenantId { get; }
        /// <summary>
        /// 
        /// </summary>
        public string WebId { get; }
    }
}
