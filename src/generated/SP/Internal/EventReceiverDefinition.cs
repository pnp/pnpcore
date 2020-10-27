using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// EventReceiverDefinition class, write your custom code here
    /// </summary>
    [SharePointType("SP.EventReceiverDefinition", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class EventReceiverDefinition : BaseDataModel<IEventReceiverDefinition>, IEventReceiverDefinition
    {
        #region Construction
        public EventReceiverDefinition()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string ReceiverAssembly { get => GetValue<string>(); set => SetValue(value); }

        public string ReceiverClass { get => GetValue<string>(); set => SetValue(value); }

        public Guid ReceiverId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ReceiverName { get => GetValue<string>(); set => SetValue(value); }

        public int SequenceNumber { get => GetValue<int>(); set => SetValue(value); }

        public int Synchronization { get => GetValue<int>(); set => SetValue(value); }

        public int EventType { get => GetValue<int>(); set => SetValue(value); }

        public string ReceiverUrl { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
