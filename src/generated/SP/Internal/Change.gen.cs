using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a Change object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class Change : BaseDataModel<IChange>, IChange
    {

        #region New properties

        public int ChangeType { get => GetValue<int>(); set => SetValue(value); }

        public string RelativeTime { get => GetValue<string>(); set => SetValue(value); }

        public Guid SiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public DateTime Time { get => GetValue<DateTime>(); set => SetValue(value); }

        #endregion

    }
}
