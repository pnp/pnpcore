using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a SharingPermissionInformation object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class SharingPermissionInformation : BaseDataModel<ISharingPermissionInformation>, ISharingPermissionInformation
    {

        #region New properties

        public bool IsDefaultPermission { get => GetValue<bool>(); set => SetValue(value); }

        public string PermissionDescription { get => GetValue<string>(); set => SetValue(value); }

        public string PermissionId { get => GetValue<string>(); set => SetValue(value); }

        public int PermissionKind { get => GetValue<int>(); set => SetValue(value); }

        public string PermissionName { get => GetValue<string>(); set => SetValue(value); }

        public int PermissionRoleType { get => GetValue<int>(); set => SetValue(value); }

        #endregion

    }
}
