using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// AppInstance class, write your custom code here
    /// </summary>
    [SharePointType("SP.AppInstance", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class AppInstance : BaseDataModel<IAppInstance>, IAppInstance
    {
        #region Construction
        public AppInstance()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string AppPrincipalId { get => GetValue<string>(); set => SetValue(value); }

        public string AppWebFullUrl { get => GetValue<string>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string ImageFallbackUrl { get => GetValue<string>(); set => SetValue(value); }

        public string ImageUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool InError { get => GetValue<bool>(); set => SetValue(value); }

        public string StartPage { get => GetValue<string>(); set => SetValue(value); }

        public Guid ProductId { get => GetValue<Guid>(); set => SetValue(value); }

        public string RemoteAppUrl { get => GetValue<string>(); set => SetValue(value); }

        public string SettingsPageUrl { get => GetValue<string>(); set => SetValue(value); }

        public Guid SiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public int Status { get => GetValue<int>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }


        #endregion

        #region Extension methods
        #endregion
    }
}
