namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(IdentitySet))]
    public interface IIdentitySet
    {
        /// <summary>
        /// 
        /// </summary>
        public IIdentity Application { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IIdentity Device { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IIdentity User { get; set; }
    }
}
