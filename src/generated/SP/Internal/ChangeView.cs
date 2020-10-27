using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ChangeView class, write your custom code here
    /// </summary>
    [SharePointType("SP.ChangeView", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ChangeView : BaseDataModel<IChangeView>, IChangeView
    {
        #region Construction
        public ChangeView()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public Guid ViewId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
