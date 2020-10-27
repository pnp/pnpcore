using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldRatingScale class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldRatingScale", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldRatingScale : BaseDataModel<IFieldRatingScale>, IFieldRatingScale
    {
        #region Construction
        public FieldRatingScale()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int GridEndNumber { get => GetValue<int>(); set => SetValue(value); }

        public string GridNAOptionText { get => GetValue<string>(); set => SetValue(value); }

        public int GridStartNumber { get => GetValue<int>(); set => SetValue(value); }

        public string GridTextRangeAverage { get => GetValue<string>(); set => SetValue(value); }

        public string GridTextRangeHigh { get => GetValue<string>(); set => SetValue(value); }

        public string GridTextRangeLow { get => GetValue<string>(); set => SetValue(value); }

        public int RangeCount { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
