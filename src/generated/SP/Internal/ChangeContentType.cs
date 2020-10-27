using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ChangeContentType class, write your custom code here
    /// </summary>
    [SharePointType("SP.ChangeContentType", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ChangeContentType : BaseDataModel<IChangeContentType>, IChangeContentType
    {
        #region Construction
        public ChangeContentType()
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
