using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a MountPointInfo object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class MountPointInfo : BaseDataModel<IMountPointInfo>, IMountPointInfo
    {

        #region New properties

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string RedirectUrl { get => GetValue<string>(); set => SetValue(value); }

        #endregion

    }
}
