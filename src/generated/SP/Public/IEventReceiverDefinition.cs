using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a EventReceiverDefinition object
    /// </summary>
    [ConcreteType(typeof(EventReceiverDefinition))]
    public interface IEventReceiverDefinition : IDataModel<IEventReceiverDefinition>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string ReceiverAssembly { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ReceiverClass { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ReceiverId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ReceiverName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Synchronization { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int EventType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ReceiverUrl { get; set; }

        #endregion

    }
}
