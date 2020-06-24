using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a MountedFolderInfo object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class MountedFolderInfo : BaseDataModel<IMountedFolderInfo>, IMountedFolderInfo
    {

        #region New properties

        public string FolderUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool HasEditPermission { get => GetValue<bool>(); set => SetValue(value); }

        public int ItemId { get => GetValue<int>(); set => SetValue(value); }

        public int ListTemplateType { get => GetValue<int>(); set => SetValue(value); }

        public string ListViewUrl { get => GetValue<string>(); set => SetValue(value); }

        public string WebUrl { get => GetValue<string>(); set => SetValue(value); }

        #endregion

    }
}
