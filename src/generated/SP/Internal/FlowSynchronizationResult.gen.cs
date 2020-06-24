using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a FlowSynchronizationResult object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class FlowSynchronizationResult : BaseDataModel<IFlowSynchronizationResult>, IFlowSynchronizationResult
    {

        #region New properties

        public string SynchronizationData { get => GetValue<string>(); set => SetValue(value); }

        public int SynchronizationStatus { get => GetValue<int>(); set => SetValue(value); }

        #endregion

    }
}
