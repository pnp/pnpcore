using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ChangeFile class, write your custom code here
    /// </summary>
    [SharePointType("SP.ChangeFile", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ChangeFile : BaseDataModel<IChangeFile>, IChangeFile
    {
        #region Construction
        public ChangeFile()
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
