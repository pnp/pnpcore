using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ThemeInfo object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ThemeInfo : BaseDataModel<IThemeInfo>, IThemeInfo
    {

        #region New properties

        public string AccessibleDescription { get => GetValue<string>(); set => SetValue(value); }

        public string ThemeBackgroundImageUri { get => GetValue<string>(); set => SetValue(value); }

        #endregion

    }
}
