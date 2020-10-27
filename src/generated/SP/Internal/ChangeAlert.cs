using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ChangeAlert class, write your custom code here
    /// </summary>
    [SharePointType("SP.ChangeAlert", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ChangeAlert : BaseDataModel<IChangeAlert>, IChangeAlert
    {
        #region Construction
        public ChangeAlert()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public Guid AlertId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
