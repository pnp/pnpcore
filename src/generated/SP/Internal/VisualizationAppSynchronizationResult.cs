using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// VisualizationAppSynchronizationResult class, write your custom code here
    /// </summary>
    [SharePointType("SP.VisualizationAppSynchronizationResult", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class VisualizationAppSynchronizationResult : BaseDataModel<IVisualizationAppSynchronizationResult>, IVisualizationAppSynchronizationResult
    {
        #region Construction
        public VisualizationAppSynchronizationResult()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string SynchronizationData { get => GetValue<string>(); set => SetValue(value); }

        public int SynchronizationStatus { get => GetValue<int>(); set => SetValue(value); }

        public IViewCollection AppMappedViews { get => GetModelCollectionValue<IViewCollection>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
