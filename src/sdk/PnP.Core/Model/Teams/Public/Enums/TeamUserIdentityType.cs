namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// The user identity type
    /// </summary>
    public enum TeamUserIdentityType
    {
        /// <summary>
        /// User is an AAD User
        /// </summary>
        aadUser,

        /// <summary>
        /// User is an on premises AAD User
        /// </summary>
        onPremiseAadUser,

        /// <summary>
        /// User is an anonymous guest user
        /// </summary>
        anonymousGuest,

        /// <summary>
        /// User is a federated user
        /// </summary>
        federatedUser,

        /// <summary>
        /// User is a personal Microsoft account user
        /// </summary>
        personalMicrosoftAccountUser,

        /// <summary>
        /// User is a Skype user
        /// </summary>
        skypeUser,

        /// <summary>
        /// User is a phone user
        /// </summary>
        phoneUser,
        
        /// <summary>
        /// Unknown value, for future reference
        /// </summary>
        unknownFutureValue,

        /// <summary>
        /// User is an email user
        /// </summary>
        emailUser
    }
}
