using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ConnectorResult object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ConnectorResult : BaseDataModel<IConnectorResult>, IConnectorResult
    {

        #region New properties

        public string ContextData { get => GetValue<string>(); set => SetValue(value); }

        public string Value { get => GetValue<string>(); set => SetValue(value); }

        #endregion

    }
}
