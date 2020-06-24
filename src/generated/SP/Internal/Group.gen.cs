using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a Group object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class Group : BaseDataModel<IGroup>, IGroup
    {

        #region New properties

        public bool AllowMembersEditMembership { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowRequestToJoinLeave { get => GetValue<bool>(); set => SetValue(value); }

        public bool AutoAcceptRequestToJoinLeave { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserEditMembership { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserManageGroup { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserViewMembership { get => GetValue<bool>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public bool OnlyAllowMembersViewMembership { get => GetValue<bool>(); set => SetValue(value); }

        public string OwnerTitle { get => GetValue<string>(); set => SetValue(value); }

        public string RequestToJoinLeaveEmailSetting { get => GetValue<string>(); set => SetValue(value); }

        public IPrincipal Owner
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new Principal
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IPrincipal>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("Users", Expandable = true)]
        public IUserCollection Users
        {
            get
            {
                if (!HasValue(nameof(Users)))
                {
                    var collection = new UserCollection(this.PnPContext, this, nameof(Users));
                    SetValue(collection);
                }
                return GetValue<IUserCollection>();
            }
        }

        #endregion

    }
}
