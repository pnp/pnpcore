using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ThemeInfo class, write your custom code here
    /// </summary>
    [SharePointType("SP.ThemeInfo", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ThemeInfo : BaseDataModel<IThemeInfo>, IThemeInfo
    {
        #region Construction
        public ThemeInfo()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string AccessibleDescription { get => GetValue<string>(); set => SetValue(value); }

        public string ThemeBackgroundImageUri { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
