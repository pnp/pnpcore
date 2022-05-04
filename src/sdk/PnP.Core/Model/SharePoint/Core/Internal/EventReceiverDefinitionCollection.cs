using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.Requests.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class EventReceiverDefinitionCollection : QueryableDataModelCollection<IEventReceiverDefinition>, IEventReceiverDefinitionCollection
    {
        public EventReceiverDefinitionCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Methods 

        public IEventReceiverDefinition Add(EventReceiverOptions eventReceiverOptions)
        {
            return AddAsync(eventReceiverOptions).GetAwaiter().GetResult();
        }

        public async Task<IEventReceiverDefinition> AddAsync(EventReceiverOptions eventReceiverOptions)
        {
            VerifyEventReceiverOptions(eventReceiverOptions);

            var newEventReceiver = CreateNewAndAdd() as EventReceiverDefinition;

            // Add the field options as arguments for the add method
            var additionalInfo = new Dictionary<string, object>()
            {
                { EventReceiverDefinition.EventReceiverOptionsAdditionalInformationKey, eventReceiverOptions }
            };

            return await newEventReceiver.AddAsync(additionalInfo).ConfigureAwait(false) as EventReceiverDefinition;
        }

        public IEventReceiverDefinition AddBatch(EventReceiverOptions eventReceiverOptions)
        {
            return AddBatchAsync(PnPContext.CurrentBatch, eventReceiverOptions).GetAwaiter().GetResult();
        }

        public  IEventReceiverDefinition AddBatch(Batch batch, EventReceiverOptions eventReceiverOptions)
        {
            return AddBatchAsync(batch, eventReceiverOptions).GetAwaiter().GetResult();
        }

        public async Task<IEventReceiverDefinition> AddBatchAsync(EventReceiverOptions eventReceiverOptions)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, eventReceiverOptions).ConfigureAwait(false);
        }

        public async Task<IEventReceiverDefinition> AddBatchAsync(Batch batch, EventReceiverOptions eventReceiverOptions)
        {
            VerifyEventReceiverOptions(eventReceiverOptions);

            var newEventReceiver = CreateNewAndAdd() as EventReceiverDefinition;

            // Add the field options as arguments for the add method
            var additionalInfo = new Dictionary<string, object>()
            {
                { EventReceiverDefinition.EventReceiverOptionsAdditionalInformationKey, eventReceiverOptions }
            };

            return await newEventReceiver.AddBatchAsync(batch, additionalInfo).ConfigureAwait(false) as EventReceiverDefinition;
        }

        private void VerifyEventReceiverOptions(EventReceiverOptions eventReceiverOptions)
        {
            if (eventReceiverOptions.EventType == EventReceiverType.InvalidReceiver)
            {
                throw new ArgumentNullException(nameof(eventReceiverOptions.EventType));
            }
            if (string.IsNullOrEmpty(eventReceiverOptions.ReceiverName))
            {
                throw new ArgumentNullException(nameof(eventReceiverOptions.ReceiverName));
            }
            if (string.IsNullOrEmpty(eventReceiverOptions.ReceiverUrl))
            {
                throw new ArgumentNullException(nameof(eventReceiverOptions.ReceiverUrl));
            }
            if (eventReceiverOptions.SequenceNumber < 1)
            {
                throw new ArgumentNullException(nameof(eventReceiverOptions.SequenceNumber));
            }
        }

        #endregion
    }
}
