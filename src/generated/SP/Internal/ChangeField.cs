using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ChangeField class, write your custom code here
    /// </summary>
    [SharePointType("SP.ChangeField", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ChangeField : BaseDataModel<IChangeField>, IChangeField
    {
        #region Construction
        public ChangeField()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public Guid FieldId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
