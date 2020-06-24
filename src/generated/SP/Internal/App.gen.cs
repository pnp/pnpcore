using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a App object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class App : BaseDataModel<IApp>, IApp
    {

        #region New properties

        public string AssetId { get => GetValue<string>(); set => SetValue(value); }

        public string ContentMarket { get => GetValue<string>(); set => SetValue(value); }

        public string VersionString { get => GetValue<string>(); set => SetValue(value); }

        #endregion

    }
}
