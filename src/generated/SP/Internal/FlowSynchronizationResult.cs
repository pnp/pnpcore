using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FlowSynchronizationResult class, write your custom code here
    /// </summary>
    [SharePointType("SP.FlowSynchronizationResult", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FlowSynchronizationResult : BaseDataModel<IFlowSynchronizationResult>, IFlowSynchronizationResult
    {
        #region Construction
        public FlowSynchronizationResult()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string SynchronizationData { get => GetValue<string>(); set => SetValue(value); }

        public int SynchronizationStatus { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
