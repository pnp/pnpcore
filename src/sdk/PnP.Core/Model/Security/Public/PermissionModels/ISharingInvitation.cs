namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(SharingInvitation))]
    public interface ISharingInvitation
    {
        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IIdentitySet InvitedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool SignInRequired { get; set; }
    }
}
