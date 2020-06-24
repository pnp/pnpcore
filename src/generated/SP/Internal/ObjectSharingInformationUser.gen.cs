using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ObjectSharingInformationUser object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ObjectSharingInformationUser : BaseDataModel<IObjectSharingInformationUser>, IObjectSharingInformationUser
    {

        #region New properties

        public string CustomRoleNames { get => GetValue<string>(); set => SetValue(value); }

        public string Department { get => GetValue<string>(); set => SetValue(value); }

        public string Email { get => GetValue<string>(); set => SetValue(value); }

        public bool HasEditPermission { get => GetValue<bool>(); set => SetValue(value); }

        public bool HasReviewPermission { get => GetValue<bool>(); set => SetValue(value); }

        public bool HasViewPermission { get => GetValue<bool>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool IsDomainGroup { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsExternalUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsMemberOfGroup { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSiteAdmin { get => GetValue<bool>(); set => SetValue(value); }

        public string JobTitle { get => GetValue<string>(); set => SetValue(value); }

        public string LoginName { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string Picture { get => GetValue<string>(); set => SetValue(value); }

        public string SipAddress { get => GetValue<string>(); set => SetValue(value); }

        public IPrincipal Principal
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


        public IUser User
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new User
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IUser>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        #endregion

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = (int)value; }


    }
}
