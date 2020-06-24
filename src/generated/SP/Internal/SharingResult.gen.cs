using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a SharingResult object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class SharingResult : BaseDataModel<ISharingResult>, ISharingResult
    {

        #region New properties

        public string ErrorMessage { get => GetValue<string>(); set => SetValue(value); }

        public string IconUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string PermissionsPageRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public int StatusCode { get => GetValue<int>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("GroupsSharedWith", Expandable = true)]
        public IGroupCollection GroupsSharedWith
        {
            get
            {
                if (!HasValue(nameof(GroupsSharedWith)))
                {
                    var collection = new GroupCollection(this.PnPContext, this, nameof(GroupsSharedWith));
                    SetValue(collection);
                }
                return GetValue<IGroupCollection>();
            }
        }

        public IGroup GroupUsersAddedTo
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new Group
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IGroup>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("UsersWithAccessRequests", Expandable = true)]
        public IUserCollection UsersWithAccessRequests
        {
            get
            {
                if (!HasValue(nameof(UsersWithAccessRequests)))
                {
                    var collection = new UserCollection(this.PnPContext, this, nameof(UsersWithAccessRequests));
                    SetValue(collection);
                }
                return GetValue<IUserCollection>();
            }
        }

        #endregion

    }
}
