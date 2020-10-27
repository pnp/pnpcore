using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Change class, write your custom code here
    /// </summary>
    [SharePointType("SP.Change", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class Change : BaseDataModel<IChange>, IChange
    {
        #region Construction
        public Change()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int ChangeType { get => GetValue<int>(); set => SetValue(value); }

        public string RelativeTime { get => GetValue<string>(); set => SetValue(value); }

        public Guid SiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public DateTime Time { get => GetValue<DateTime>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
