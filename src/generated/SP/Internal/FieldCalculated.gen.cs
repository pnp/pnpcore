using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a FieldCalculated object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class FieldCalculated : BaseDataModel<IFieldCalculated>, IFieldCalculated
    {

        #region New properties

        public int CurrencyLocaleId { get => GetValue<int>(); set => SetValue(value); }

        public int DateFormat { get => GetValue<int>(); set => SetValue(value); }

        public int DisplayFormat { get => GetValue<int>(); set => SetValue(value); }

        public string Formula { get => GetValue<string>(); set => SetValue(value); }

        public int OutputType { get => GetValue<int>(); set => SetValue(value); }

        public bool ShowAsPercentage { get => GetValue<bool>(); set => SetValue(value); }

        #endregion

    }
}
