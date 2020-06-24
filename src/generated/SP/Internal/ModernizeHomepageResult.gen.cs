using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ModernizeHomepageResult object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ModernizeHomepageResult : BaseDataModel<IModernizeHomepageResult>, IModernizeHomepageResult
    {

        #region New properties

        public bool CanModernizeHomepage { get => GetValue<bool>(); set => SetValue(value); }

        public string Reason { get => GetValue<string>(); set => SetValue(value); }

        #endregion

    }
}
