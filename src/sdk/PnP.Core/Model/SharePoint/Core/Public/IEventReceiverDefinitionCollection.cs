using PnP.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(EventReceiverDefinitionCollection))]
    public interface IEventReceiverDefinitionCollection : IQueryable<IEventReceiverDefinition>, IAsyncEnumerable<IEventReceiverDefinition>, IDataModelCollectionLoad<IEventReceiverDefinition>, IDataModelCollection<IEventReceiverDefinition>, IDataModelCollectionDeleteByGuidId, ISupportModules<IEventReceiverDefinitionCollection>
    {

        #region Methods

        /// <summary>
        /// Adds a new eventreceiver to the current web or list
        /// </summary>
        /// <param name="eventReceiverOptions">Options used when creating the new eventreceiver</param>
        /// <returns>The newly created eventreceiver</returns>
        public Task<IEventReceiverDefinition> AddAsync(EventReceiverOptions eventReceiverOptions);

        /// <summary>
        /// Adds a new eventreceiver to the current web or list
        /// </summary>
        /// <param name="eventReceiverOptions">Options used when creating the new eventreceiver</param>
        /// <returns>The newly created eventreceiver</returns>
        public IEventReceiverDefinition Add(EventReceiverOptions eventReceiverOptions);

        /// <summary>
        /// Adds a new eventreceiver to the current web or list
        /// </summary>
        /// <param name="eventReceiverOptions">Options used when creating the new eventreceiver</param>
        /// <returns>The newly created eventreceiver</returns>
        public Task<IEventReceiverDefinition> AddBatchAsync(EventReceiverOptions eventReceiverOptions);

        /// <summary>
        /// Adds a new eventreceiver to the current web or list
        /// </summary>
        /// <param name="eventReceiverOptions">Options used when creating the new eventreceiver</param>
        /// <returns>The newly created eventreceiver</returns>
        public IEventReceiverDefinition AddBatch(EventReceiverOptions eventReceiverOptions);

        /// <summary>
        /// Adds a new eventreceiver to the current web or list
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="eventReceiverOptions">Options used when creating the new eventreceiver</param>
        /// <returns>The newly created eventreceiver</returns>
        public Task<IEventReceiverDefinition> AddBatchAsync(Batch batch, EventReceiverOptions eventReceiverOptions);

        /// <summary>
        /// Adds a new eventreceiver to the current web or list
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="eventReceiverOptions">Options used when creating the new eventreceiver</param>
        /// <returns>The newly created eventreceiver</returns>
        public IEventReceiverDefinition AddBatch(Batch batch, EventReceiverOptions eventReceiverOptions);

        #endregion

    }
}
