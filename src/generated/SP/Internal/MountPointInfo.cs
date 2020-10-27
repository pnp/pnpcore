using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// MountPointInfo class, write your custom code here
    /// </summary>
    [SharePointType("SP.MountPointInfo", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class MountPointInfo : BaseDataModel<IMountPointInfo>, IMountPointInfo
    {
        #region Construction
        public MountPointInfo()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string RedirectUrl { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
