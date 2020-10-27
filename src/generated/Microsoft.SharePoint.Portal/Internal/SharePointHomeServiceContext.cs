using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// SharePointHomeServiceContext class, write your custom code here
    /// </summary>
    [SharePointType("Microsoft.SharePoint.Portal.SharePointHomeServiceContext", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class SharePointHomeServiceContext : BaseDataModel<ISharePointHomeServiceContext>, ISharePointHomeServiceContext
    {
        #region Construction
        public SharePointHomeServiceContext()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string CompanyPortalContext { get => GetValue<string>(); set => SetValue(value); }

        public string Payload { get => GetValue<string>(); set => SetValue(value); }

        public ITokenResponse DWEngineToken { get => GetModelValue<ITokenResponse>(); }


        public ITokenResponse Token { get => GetModelValue<ITokenResponse>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
