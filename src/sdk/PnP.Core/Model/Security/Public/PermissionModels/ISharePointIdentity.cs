namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(SharePointIdentity))]
    public interface ISharePointIdentity
    {
        /// <summary>
        /// 
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// 
        /// </summary>
        public string LoginName { get; }

    }
}
