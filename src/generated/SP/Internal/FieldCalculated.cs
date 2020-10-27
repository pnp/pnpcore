using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldCalculated class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldCalculated", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldCalculated : BaseDataModel<IFieldCalculated>, IFieldCalculated
    {
        #region Construction
        public FieldCalculated()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int CurrencyLocaleId { get => GetValue<int>(); set => SetValue(value); }

        public int DateFormat { get => GetValue<int>(); set => SetValue(value); }

        public int DisplayFormat { get => GetValue<int>(); set => SetValue(value); }

        public string Formula { get => GetValue<string>(); set => SetValue(value); }

        public int OutputType { get => GetValue<int>(); set => SetValue(value); }

        public bool ShowAsPercentage { get => GetValue<bool>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
