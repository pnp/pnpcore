using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// TimeZone class, write your custom code here
    /// </summary>
    [SharePointType("SP.TimeZone", Uri = "_api/web/regionalsettings/timezone", LinqGet = "_api/web/regionalsettings/timezone")]
    internal partial class TimeZone : BaseDataModel<ITimeZone>, ITimeZone
    {
        #region Construction
        public TimeZone()
        {
        }
        #endregion

        #region Properties

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("Information", JsonPath = "Bias")]
        public int Bias { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("Information", JsonPath = "DaylightBias")]
        public int DaylightBias { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("Information", JsonPath = "StandardBias")]
        public int StandardBias { get => GetValue<int>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }

        #endregion

        #region Extension methods

        public DateTime LocalTimeToUtc(DateTime dateTime)
        {
            return dateTime + UtcDelta(dateTime);
        }

        public DateTime UtcToLocalTime(DateTime dateTime)
        {
            return dateTime - UtcDelta(dateTime);
        }

        private TimeSpan UtcDelta(DateTime dateTime)
        {
            return new TimeSpan(0, Bias + (TimeZoneInfo.Local.IsDaylightSavingTime(dateTime) ? DaylightBias : StandardBias), 0);
        }

        #endregion
    }
}
