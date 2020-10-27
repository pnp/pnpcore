using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ObjectSharingInformationUser class, write your custom code here
    /// </summary>
    [SharePointType("SP.ObjectSharingInformationUser", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ObjectSharingInformationUser : BaseDataModel<IObjectSharingInformationUser>, IObjectSharingInformationUser
    {
        #region Construction
        public ObjectSharingInformationUser()
        {
        }
        #endregion

        #region Properties
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

        public IPrincipal Principal { get => GetModelValue<IPrincipal>(); }


        public IUser User { get => GetModelValue<IUser>(); }


        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }


        #endregion

        #region Extension methods
        #endregion
    }
}
