using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldNumber class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldNumber", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldNumber : BaseDataModel<IFieldNumber>, IFieldNumber
    {
        #region Construction
        public FieldNumber()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool CommaSeparator { get => GetValue<bool>(); set => SetValue(value); }

        public string CustomUnitName { get => GetValue<string>(); set => SetValue(value); }

        public bool CustomUnitOnRight { get => GetValue<bool>(); set => SetValue(value); }

        public int DisplayFormat { get => GetValue<int>(); set => SetValue(value); }

        public bool ShowAsPercentage { get => GetValue<bool>(); set => SetValue(value); }

        public string Unit { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
