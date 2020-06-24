using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a EventReceiverDefinition object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class EventReceiverDefinition : BaseDataModel<IEventReceiverDefinition>, IEventReceiverDefinition
    {

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

    }
}
