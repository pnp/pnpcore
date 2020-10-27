using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// AppPrincipalIdentityProvider class, write your custom code here
    /// </summary>
    [SharePointType("SP.AppPrincipalIdentityProvider", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class AppPrincipalIdentityProvider : BaseDataModel<IAppPrincipalIdentityProvider>, IAppPrincipalIdentityProvider
    {
        #region Construction
        public AppPrincipalIdentityProvider()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Id4a81de82eeb94d6080ea5bf63e27023a { get => GetValue<string>(); set => SetValue(value); }

        public IAppPrincipalIdentityProvider External { get => GetModelValue<IAppPrincipalIdentityProvider>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
