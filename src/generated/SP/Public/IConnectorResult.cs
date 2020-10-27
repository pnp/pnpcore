using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ConnectorResult object
    /// </summary>
    [ConcreteType(typeof(ConnectorResult))]
    public interface IConnectorResult : IDataModel<IConnectorResult>, IDataModelGet<IConnectorResult>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string ContextData { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Value { get; set; }

        #endregion

    }
}
