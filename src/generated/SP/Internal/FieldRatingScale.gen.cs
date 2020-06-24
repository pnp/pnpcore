using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a FieldRatingScale object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class FieldRatingScale : BaseDataModel<IFieldRatingScale>, IFieldRatingScale
    {

        #region New properties

        public int GridEndNumber { get => GetValue<int>(); set => SetValue(value); }

        public string GridNAOptionText { get => GetValue<string>(); set => SetValue(value); }

        public int GridStartNumber { get => GetValue<int>(); set => SetValue(value); }

        public string GridTextRangeAverage { get => GetValue<string>(); set => SetValue(value); }

        public string GridTextRangeHigh { get => GetValue<string>(); set => SetValue(value); }

        public string GridTextRangeLow { get => GetValue<string>(); set => SetValue(value); }

        public int RangeCount { get => GetValue<int>(); set => SetValue(value); }

        #endregion

    }
}
