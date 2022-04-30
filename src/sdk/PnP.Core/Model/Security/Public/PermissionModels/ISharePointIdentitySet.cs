namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(SharePointIdentitySet))]
    public interface ISharePointIdentitySet
    {
        /// <summary>
        /// 
        /// </summary>
        public IIdentity Application { get; }
        /// <summary>
        /// 
        /// </summary>
        public IIdentity Device { get; }
        /// <summary>
        /// 
        /// </summary>
        public IIdentity User { get; }
        /// <summary>
        /// 
        /// </summary>
        public IIdentity Group { get; }
        /// <summary>
        /// 
        /// </summary>
        public ISharePointIdentity SiteUser { get; }
        /// <summary>
        /// 
        /// </summary>
        public ISharePointIdentity SiteGroup { get; }
    }
}
