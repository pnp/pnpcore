using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldDateTime class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldDateTime", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldDateTime : BaseDataModel<IFieldDateTime>, IFieldDateTime
    {
        #region Construction
        public FieldDateTime()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int DateTimeCalendarType { get => GetValue<int>(); set => SetValue(value); }

        public string DateFormat { get => GetValue<string>(); set => SetValue(value); }

        public int DisplayFormat { get => GetValue<int>(); set => SetValue(value); }

        public int FriendlyDisplayFormat { get => GetValue<int>(); set => SetValue(value); }

        public string TimeFormat { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
