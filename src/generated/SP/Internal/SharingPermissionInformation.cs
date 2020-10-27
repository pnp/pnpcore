using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// SharingPermissionInformation class, write your custom code here
    /// </summary>
    [SharePointType("SP.SharingPermissionInformation", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class SharingPermissionInformation : BaseDataModel<ISharingPermissionInformation>, ISharingPermissionInformation
    {
        #region Construction
        public SharingPermissionInformation()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool IsDefaultPermission { get => GetValue<bool>(); set => SetValue(value); }

        public string PermissionDescription { get => GetValue<string>(); set => SetValue(value); }

        public string PermissionId { get => GetValue<string>(); set => SetValue(value); }

        public int PermissionKind { get => GetValue<int>(); set => SetValue(value); }

        public string PermissionName { get => GetValue<string>(); set => SetValue(value); }

        public int PermissionRoleType { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
