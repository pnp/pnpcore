using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a VisualizationAppSynchronizationResult object
    /// </summary>
    [ConcreteType(typeof(VisualizationAppSynchronizationResult))]
    public interface IVisualizationAppSynchronizationResult : IDataModel<IVisualizationAppSynchronizationResult>, IDataModelUpdate, IDataModelDelete
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

        /// <summary>
        /// To update...
        /// </summary>
        public IViewCollection AppMappedViews { get; }

        #endregion

    }
}
