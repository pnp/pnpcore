using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// AppInstanceErrorDetails class, write your custom code here
    /// </summary>
    [SharePointType("SP.AppInstanceErrorDetails", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class AppInstanceErrorDetails : BaseDataModel<IAppInstanceErrorDetails>, IAppInstanceErrorDetails
    {
        #region Construction
        public AppInstanceErrorDetails()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public Guid CorrelationId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ErrorDetail { get => GetValue<string>(); set => SetValue(value); }

        public int ErrorType { get => GetValue<int>(); set => SetValue(value); }

        public string ErrorTypeName { get => GetValue<string>(); set => SetValue(value); }

        public string ExceptionMessage { get => GetValue<string>(); set => SetValue(value); }

        public int Source { get => GetValue<int>(); set => SetValue(value); }

        public string SourceName { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
