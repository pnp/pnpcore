using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldUrl class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldUrl", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldUrl : BaseDataModel<IFieldUrl>, IFieldUrl
    {
        #region Construction
        public FieldUrl()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int DisplayFormat { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
