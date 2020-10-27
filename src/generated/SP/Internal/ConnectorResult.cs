using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ConnectorResult class, write your custom code here
    /// </summary>
    [SharePointType("SP.ConnectorResult", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ConnectorResult : BaseDataModel<IConnectorResult>, IConnectorResult
    {
        #region Construction
        public ConnectorResult()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string ContextData { get => GetValue<string>(); set => SetValue(value); }

        public string Value { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
