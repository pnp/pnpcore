using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ChangeFolder class, write your custom code here
    /// </summary>
    [SharePointType("SP.ChangeFolder", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ChangeFolder : BaseDataModel<IChangeFolder>, IChangeFolder
    {
        #region Construction
        public ChangeFolder()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public Guid UniqueId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
