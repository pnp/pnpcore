using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// User class, write your custom code here
    /// </summary>
    [SharePointType("SP.User", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class User : BaseDataModel<IUser>, IUser
    {
        #region Construction
        public User()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Email { get => GetValue<string>(); set => SetValue(value); }

        public string Expiration { get => GetValue<string>(); set => SetValue(value); }

        public bool IsEmailAuthenticationGuestUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsShareByEmailGuestUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSiteAdmin { get => GetValue<bool>(); set => SetValue(value); }

        public string UserPrincipalName { get => GetValue<string>(); set => SetValue(value); }

        public IAlertCollection Alerts { get => GetModelCollectionValue<IAlertCollection>(); }


        public IGroupCollection Groups { get => GetModelCollectionValue<IGroupCollection>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
