using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// MountedFolderInfo class, write your custom code here
    /// </summary>
    [SharePointType("SP.MountedFolderInfo", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class MountedFolderInfo : BaseDataModel<IMountedFolderInfo>, IMountedFolderInfo
    {
        #region Construction
        public MountedFolderInfo()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string FolderUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool HasEditPermission { get => GetValue<bool>(); set => SetValue(value); }

        public int ItemId { get => GetValue<int>(); set => SetValue(value); }

        public int ListTemplateType { get => GetValue<int>(); set => SetValue(value); }

        public string ListViewUrl { get => GetValue<string>(); set => SetValue(value); }

        public string WebUrl { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
