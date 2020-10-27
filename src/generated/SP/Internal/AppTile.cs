using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// AppTile class, write your custom code here
    /// </summary>
    [SharePointType("SP.AppTile", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class AppTile : BaseDataModel<IAppTile>, IAppTile
    {
        #region Construction
        public AppTile()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public Guid AppId { get => GetValue<Guid>(); set => SetValue(value); }

        public string AppPrincipalId { get => GetValue<string>(); set => SetValue(value); }

        public int AppSource { get => GetValue<int>(); set => SetValue(value); }

        public int AppStatus { get => GetValue<int>(); set => SetValue(value); }

        public int AppType { get => GetValue<int>(); set => SetValue(value); }

        public string AssetId { get => GetValue<string>(); set => SetValue(value); }

        public int BaseTemplate { get => GetValue<int>(); set => SetValue(value); }

        public int ChildCount { get => GetValue<int>(); set => SetValue(value); }

        public string ContentMarket { get => GetValue<string>(); set => SetValue(value); }

        public string CustomSettingsUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public bool IsCorporateCatalogSite { get => GetValue<bool>(); set => SetValue(value); }

        public string LastModified { get => GetValue<string>(); set => SetValue(value); }

        public DateTime LastModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public Guid ProductId { get => GetValue<Guid>(); set => SetValue(value); }

        public string Target { get => GetValue<string>(); set => SetValue(value); }

        public string Thumbnail { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public string Version { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
