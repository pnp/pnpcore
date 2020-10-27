using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// TenantSettings class, write your custom code here
    /// </summary>
    [SharePointType("SP.TenantSettings", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class TenantSettings : BaseDataModel<ITenantSettings>, ITenantSettings
    {
        #region Construction
        public TenantSettings()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string CorporateCatalogUrl { get => GetValue<string>(); set => SetValue(value); }

        public ITenantSettings Current { get => GetModelValue<ITenantSettings>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
