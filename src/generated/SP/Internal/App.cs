using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// App class, write your custom code here
    /// </summary>
    [SharePointType("SP.App", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class App : BaseDataModel<IApp>, IApp
    {
        #region Construction
        public App()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string AssetId { get => GetValue<string>(); set => SetValue(value); }

        public string ContentMarket { get => GetValue<string>(); set => SetValue(value); }

        public string VersionString { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
