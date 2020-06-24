using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a AlternateUrl object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class AlternateUrl : BaseDataModel<IAlternateUrl>, IAlternateUrl
    {

        #region New properties

        public string Uri { get => GetValue<string>(); set => SetValue(value); }

        public int UrlZone { get => GetValue<int>(); set => SetValue(value); }

        #endregion

    }
}
