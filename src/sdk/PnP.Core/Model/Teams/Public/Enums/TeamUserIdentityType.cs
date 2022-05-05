namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// 
    /// </summary>
    public enum TeamUserIdentityType
    {
        /// <summary>
        /// 
        /// </summary>
        aadUser,

        /// <summary>
        /// 
        /// </summary>
        onPremiseAadUser,

        /// <summary>
        /// 
        /// </summary>
        anonymousGuest,

        /// <summary>
        /// 
        /// </summary>
        federatedUser,

        /// <summary>
        /// 
        /// </summary>
        personalMicrosoftAccountUser,

        /// <summary>
        /// 
        /// </summary>
        skypeUser,

        /// <summary>
        /// 
        /// </summary>
        phoneUser,
        
        /// <summary>
        /// 
        /// </summary>
        unknownFutureValue,

        /// <summary>
        /// 
        /// </summary>
        emailUser
    }
}
