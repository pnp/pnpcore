using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a VisualizationAppSynchronizationResult object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class VisualizationAppSynchronizationResult : BaseDataModel<IVisualizationAppSynchronizationResult>, IVisualizationAppSynchronizationResult
    {

        #region New properties

        public string SynchronizationData { get => GetValue<string>(); set => SetValue(value); }

        public int SynchronizationStatus { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("AppMappedViews", Expandable = true)]
        public IViewCollection AppMappedViews
        {
            get
            {
                if (!HasValue(nameof(AppMappedViews)))
                {
                    var collection = new ViewCollection(this.PnPContext, this, nameof(AppMappedViews));
                    SetValue(collection);
                }
                return GetValue<IViewCollection>();
            }
        }

        #endregion

    }
}
