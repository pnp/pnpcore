using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a User object
    /// </summary>
    [ConcreteType(typeof(User))]
    public interface IUser : IDataModel<IUser>, IDataModelGet<IUser>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Expiration { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsEmailAuthenticationGuestUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsShareByEmailGuestUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsSiteAdmin { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IAlertCollection Alerts { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IGroupCollection Groups { get; }

        #endregion

    }
}
