using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a APIHubConnector object
    /// </summary>
    [ConcreteType(typeof(APIHubConnector))]
    public interface IAPIHubConnector : IDataModel<IAPIHubConnector>, IDataModelGet<IAPIHubConnector>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Id4a81de82eeb94d6080ea5bf63e27023a { get; set; }

        #endregion

    }
}
