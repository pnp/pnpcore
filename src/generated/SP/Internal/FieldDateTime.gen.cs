using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a FieldDateTime object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class FieldDateTime : BaseDataModel<IFieldDateTime>, IFieldDateTime
    {

        #region New properties

        public int DateTimeCalendarType { get => GetValue<int>(); set => SetValue(value); }

        public int DisplayFormat { get => GetValue<int>(); set => SetValue(value); }

        public int FriendlyDisplayFormat { get => GetValue<int>(); set => SetValue(value); }

        #endregion

    }
}
