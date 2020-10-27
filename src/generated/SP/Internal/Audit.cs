using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Audit class, write your custom code here
    /// </summary>
    [SharePointType("SP.Audit", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class Audit : BaseDataModel<IAudit>, IAudit
    {
        #region Construction
        public Audit()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int AuditFlags { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
