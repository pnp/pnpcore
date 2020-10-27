using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// CreatablesInfo class, write your custom code here
    /// </summary>
    [SharePointType("SP.CreatablesInfo", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class CreatablesInfo : BaseDataModel<ICreatablesInfo>, ICreatablesInfo
    {
        #region Construction
        public CreatablesInfo()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool CanCreateFolders { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCreateItems { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanUploadFiles { get => GetValue<bool>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
