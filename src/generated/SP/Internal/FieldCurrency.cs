using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldCurrency class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldCurrency", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldCurrency : BaseDataModel<IFieldCurrency>, IFieldCurrency
    {
        #region Construction
        public FieldCurrency()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int CurrencyLocaleId { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
