using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FlowSynchronizationResult object
    /// </summary>
    [ConcreteType(typeof(FlowSynchronizationResult))]
    public interface IFlowSynchronizationResult : IDataModel<IFlowSynchronizationResult>, IDataModelGet<IFlowSynchronizationResult>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string SynchronizationData { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int SynchronizationStatus { get; set; }

        #endregion

    }
}
