using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a CreatablesInfo object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class CreatablesInfo : BaseDataModel<ICreatablesInfo>, ICreatablesInfo
    {

        #region New properties

        public bool CanCreateFolders { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCreateItems { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanUploadFiles { get => GetValue<bool>(); set => SetValue(value); }

        #endregion

    }
}
