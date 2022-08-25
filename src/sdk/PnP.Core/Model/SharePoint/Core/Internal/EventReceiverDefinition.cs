using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.EventReceiver", Target = typeof(Site), Uri = "_api/Site/EventReceivers(guid'{Id}')", Get = "_api/Site/EventReceivers", LinqGet = "_api/Site/EventReceivers")]
    [SharePointType("SP.EventReceiver", Target = typeof(Web), Uri = "_api/Web/EventReceivers(guid'{Id}')", Get = "_api/Web/EventReceivers", LinqGet = "_api/Web/EventReceivers")]
    [SharePointType("SP.EventReceiver", Target = typeof(List), Uri = "_api/Web/Lists(guid'{Parent.Id}')/EventReceivers(guid'{Id}')", Get = "_api/Web/Lists(guid'{Parent.Id}')/EventReceivers", LinqGet = "_api/Web/Lists(guid'{Parent.Id}')/EventReceivers")]
    internal sealed class EventReceiverDefinition : BaseDataModel<IEventReceiverDefinition>, IEventReceiverDefinition
    {
        internal const string EventReceiverOptionsAdditionalInformationKey = "EventReceiverOptions";

        public EventReceiverDefinition()
        {
            // Handler to construct the Add request for this event receiver
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (additionalInformation) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var eventReceiverOptions = (EventReceiverOptions)additionalInformation[EventReceiverOptionsAdditionalInformationKey];

                // Build body
                var eventReceiverCreationInformation = new
                {
                    eventReceiverOptions.EventType,
                    eventReceiverOptions.ReceiverName,
                    eventReceiverOptions.ReceiverUrl,
                    eventReceiverOptions.SequenceNumber,
                    eventReceiverOptions.Synchronization,
                    eventReceiverOptions.ReceiverAssembly,
                    eventReceiverOptions.ReceiverClass
                }.AsExpando();

                string body = JsonSerializer.Serialize(eventReceiverCreationInformation, typeof(ExpandoObject));
                EntityInfo entity = EntityManager.GetClassInfo(typeof(EventReceiverDefinition), this);

                string endpointUrl = $"{entity.SharePointGet}";
                var apiCall = new ApiCall(endpointUrl, ApiType.SPORest, body)
                {
                    Headers = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "Content-Type", "application/json; charset=utf-8" },
                        { "Accept", "application/json" }
                    }
                };
                return apiCall;
            };
        }

        #region Properties

        public Guid ReceiverId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ReceiverAssembly { get => GetValue<string>(); set => SetValue(value); }

        public string ReceiverName { get => GetValue<string>(); set => SetValue(value); }

        public string ReceiverClass { get => GetValue<string>(); set => SetValue(value); }

        public int SequenceNumber { get => GetValue<int>(); set => SetValue(value); }

        public EventReceiverSynchronization Synchronization { get => GetValue<EventReceiverSynchronization>(); set => SetValue(value); }

        public EventReceiverType EventType { get => GetValue<EventReceiverType>(); set => SetValue(value); }

        public string ReceiverUrl { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(ReceiverId))]
        public override object Key { get => ReceiverId; set => ReceiverId = Guid.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }

        #endregion

        #region Methods



        #endregion
    }
}
