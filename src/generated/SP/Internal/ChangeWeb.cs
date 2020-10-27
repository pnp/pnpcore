using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ChangeWeb class, write your custom code here
    /// </summary>
    [SharePointType("SP.ChangeWeb", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ChangeWeb : BaseDataModel<IChangeWeb>, IChangeWeb
    {
        #region Construction
        public ChangeWeb()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
