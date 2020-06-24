using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a User object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class User : BaseDataModel<IUser>, IUser
    {

        #region New properties

        public string Email { get => GetValue<string>(); set => SetValue(value); }

        public string Expiration { get => GetValue<string>(); set => SetValue(value); }

        public bool IsEmailAuthenticationGuestUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsShareByEmailGuestUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSiteAdmin { get => GetValue<bool>(); set => SetValue(value); }

        public string UserPrincipalName { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Alerts", Expandable = true)]
        public IAlertCollection Alerts
        {
            get
            {
                if (!HasValue(nameof(Alerts)))
                {
                    var collection = new AlertCollection(this.PnPContext, this, nameof(Alerts));
                    SetValue(collection);
                }
                return GetValue<IAlertCollection>();
            }
        }

        [SharePointProperty("Groups", Expandable = true)]
        public IGroupCollection Groups
        {
            get
            {
                if (!HasValue(nameof(Groups)))
                {
                    var collection = new GroupCollection(this.PnPContext, this, nameof(Groups));
                    SetValue(collection);
                }
                return GetValue<IGroupCollection>();
            }
        }

        #endregion

    }
}
