using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ChangeView object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ChangeView : BaseDataModel<IChangeView>, IChangeView
    {

        #region New properties

        public Guid ViewId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        #endregion

    }
}
