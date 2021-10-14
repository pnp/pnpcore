using System;
using TimeZoneConverter;

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

        [SharePointProperty("*")]
        public object All { get => null; }

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
            return new TimeSpan(0, Bias + (GetTimeZoneInfoFromSharePoint(Description).IsDaylightSavingTime(dateTime) ? DaylightBias : StandardBias), 0);
        }

        internal static TimeZoneInfo GetTimeZoneInfoFromSharePoint(string timeZoneDescription)
        {
            var timeZoneInfo = timeZoneDescription switch
            {
                "(UTC-12:00) International Date Line West" => TZConvert.GetTimeZoneInfo("Dateline Standard Time"),
                "(UTC-11:00) Coordinated Universal Time-11" => TZConvert.GetTimeZoneInfo("UTC-11"),
                "(UTC-10:00) Hawaii" => TZConvert.GetTimeZoneInfo("Hawaiian Standard Time"),
                "(UTC-09:00) Alaska" => TZConvert.GetTimeZoneInfo("Alaskan Standard Time"),
                "(UTC-08:00) Baja California" => TZConvert.GetTimeZoneInfo("Pacific Standard Time (Mexico)"),
                "(UTC-08:00) Pacific Time (US and Canada)" => TZConvert.GetTimeZoneInfo("Pacific Standard Time"),
                "(UTC-07:00) Arizona" => TZConvert.GetTimeZoneInfo("US Mountain Standard Time"),
                "(UTC-07:00) Chihuahua, La Paz, Mazatlan" => TZConvert.GetTimeZoneInfo("Mountain Standard Time (Mexico)"),
                "(UTC-07:00) Mountain Time (US and Canada)" => TZConvert.GetTimeZoneInfo("Mountain Standard Time"),
                "(UTC-06:00) Central America" => TZConvert.GetTimeZoneInfo("Central America Standard Time"),
                "(UTC-06:00) Central Time (US and Canada)" => TZConvert.GetTimeZoneInfo("Central Standard Time"),
                "(UTC-06:00) Guadalajara, Mexico City, Monterrey" => TZConvert.GetTimeZoneInfo("Central Standard Time (Mexico)"),
                "(UTC-06:00) Saskatchewan" => TZConvert.GetTimeZoneInfo("Canada Central Standard Time"),
                "(UTC-05:00) Bogota, Lima, Quito" => TZConvert.GetTimeZoneInfo("SA Pacific Standard Time"),
                "(UTC-05:00) Eastern Time (US and Canada)" => TZConvert.GetTimeZoneInfo("Eastern Standard Time"),
                "(UTC-05:00) Indiana (East)" => TZConvert.GetTimeZoneInfo("US Eastern Standard Time"),
                "(UTC-04:30) Caracas" => TZConvert.GetTimeZoneInfo("Venezuela Standard Time"),
                "(UTC-04:00) Asuncion" => TZConvert.GetTimeZoneInfo("Paraguay Standard Time"),
                "(UTC-04:00) Atlantic Time (Canada)" => TZConvert.GetTimeZoneInfo("Atlantic Standard Time"),
                "(UTC-04:00) Cuiaba" => TZConvert.GetTimeZoneInfo("Central Brazilian Standard Time"),
                "(UTC-04:00) Georgetown, La Paz, Manaus, San Juan" => TZConvert.GetTimeZoneInfo("SA Western Standard Time"),
                "(UTC-04:00) Santiago" => TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"),
                "(UTC-03:30) Newfoundland" => TZConvert.GetTimeZoneInfo("Newfoundland Standard Time"),
                "(UTC-03:00) Brasilia" => TZConvert.GetTimeZoneInfo("E. South America Standard Time"),
                "(UTC-03:00) Buenos Aires" => TZConvert.GetTimeZoneInfo("Argentina Standard Time"),
                "(UTC-03:00) Cayenne, Fortaleza" => TZConvert.GetTimeZoneInfo("SA Eastern Standard Time"),
                "(UTC-03:00) Greenland" => TZConvert.GetTimeZoneInfo("Greenland Standard Time"),
                "(UTC-03:00) Montevideo" => TZConvert.GetTimeZoneInfo("Montevideo Standard Time"),
                "(UTC-03:00) Salvador" => TZConvert.GetTimeZoneInfo("Bahia Standard Time"),
                "(UTC-02:00) Coordinated Universal Time-02" => TZConvert.GetTimeZoneInfo("UTC-02"),
                "(UTC-02:00) Mid-Atlantic" => TZConvert.GetTimeZoneInfo("Mid-Atlantic Standard Time"),
                "(UTC-01:00) Azores" => TZConvert.GetTimeZoneInfo("Azores Standard Time"),
                "(UTC-01:00) Cabo Verde" => TZConvert.GetTimeZoneInfo("Cape Verde Standard Time"),
                "(UTC) Casablanca" => TZConvert.GetTimeZoneInfo("Morocco Standard Time"),
                "(UTC) Coordinated Universal Time" => TZConvert.GetTimeZoneInfo("UTC"),
                "(UTC) Dublin, Edinburgh, Lisbon, London" => TZConvert.GetTimeZoneInfo("GMT Standard Time"),
                "(UTC) Monrovia, Reykjavik" => TZConvert.GetTimeZoneInfo("Greenwich Standard Time"),
                "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna" => TZConvert.GetTimeZoneInfo("W. Europe Standard Time"),
                "(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague" => TZConvert.GetTimeZoneInfo("Central Europe Standard Time"),
                "(UTC+01:00) Brussels, Copenhagen, Madrid, Paris" => TZConvert.GetTimeZoneInfo("Romance Standard Time"),
                "(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb" => TZConvert.GetTimeZoneInfo("Central European Standard Time"),
                "(UTC+01:00) West Central Africa" => TZConvert.GetTimeZoneInfo("W. Central Africa Standard Time"),
                "(UTC+01:00) Windhoek" => TZConvert.GetTimeZoneInfo("W. Central Africa Standard Time"),
                "(UTC+02:00) Amman" => TZConvert.GetTimeZoneInfo("Jordan Standard Time"),
                "(UTC+02:00) Athens, Bucharest" => TZConvert.GetTimeZoneInfo("GTB Standard Time"),
                "(UTC+02:00) Beirut" => TZConvert.GetTimeZoneInfo("Middle East Standard Time"),
                "(UTC+02:00) Cairo" => TZConvert.GetTimeZoneInfo("Egypt Standard Time"),
                "(UTC+02:00) Damascus" => TZConvert.GetTimeZoneInfo("Syria Standard Time"),
                "(UTC+02:00) Harare, Pretoria" => TZConvert.GetTimeZoneInfo("South Africa Standard Time"),
                "(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius" => TZConvert.GetTimeZoneInfo("FLE Standard Time"),
                "(UTC+02:00) Jerusalem" => TZConvert.GetTimeZoneInfo("Israel Standard Time"),
                "(UTC+02:00) Minsk (old)" => TZConvert.GetTimeZoneInfo("Israel Standard Time"),
                "(UTC+02:00) E. Europe" => TZConvert.GetTimeZoneInfo("FLE Standard Time"),
                "(UTC+02:00) Kaliningrad" => TZConvert.GetTimeZoneInfo("Kaliningrad Standard Time"),
                "(UTC+03:00) Baghdad" => TZConvert.GetTimeZoneInfo("Arabic Standard Time"),
                "(UTC+03:00) Istanbul" => TZConvert.GetTimeZoneInfo("Turkey Standard Time"),
                "(UTC+03:00) Kuwait, Riyadh" => TZConvert.GetTimeZoneInfo("Arab Standard Time"),
                "(UTC+03:00) Minsk" => TZConvert.GetTimeZoneInfo("Belarus Standard Time"),
                "(UTC+03:00) Moscow, St. Petersburg, Volgograd" => TZConvert.GetTimeZoneInfo("Russian Standard Time"),
                "(UTC+03:00) Nairobi" => TZConvert.GetTimeZoneInfo("E. Africa Standard Time"),
                "(UTC+03:30) Tehran" => TZConvert.GetTimeZoneInfo("Iran Standard Time"),
                "(UTC+04:00) Abu Dhabi, Muscat" => TZConvert.GetTimeZoneInfo("Arabian Standard Time"),
                "(UTC+04:00) Astrakhan, Ulyanovsk" => TZConvert.GetTimeZoneInfo("Astrakhan Standard Time"),
                "(UTC+04:00) Baku" => TZConvert.GetTimeZoneInfo("Azerbaijan Standard Time"),
                "(UTC+04:00) Izhevsk, Samara" => TZConvert.GetTimeZoneInfo("Russia Time Zone 3"),
                "(UTC+04:00) Port Louis" => TZConvert.GetTimeZoneInfo("Mauritius Standard Time"),
                "(UTC+04:00) Tbilisi" => TZConvert.GetTimeZoneInfo("Georgian Standard Time"),
                "(UTC+04:00) Yerevan" => TZConvert.GetTimeZoneInfo("Caucasus Standard Time"),
                "(UTC+04:30) Kabul" => TZConvert.GetTimeZoneInfo("Afghanistan Standard Time"),
                "(UTC+05:00) Ekaterinburg" => TZConvert.GetTimeZoneInfo("Ekaterinburg Standard Time"),
                "(UTC+05:00) Islamabad, Karachi" => TZConvert.GetTimeZoneInfo("Pakistan Standard Time"),
                "(UTC+05:00) Tashkent" => TZConvert.GetTimeZoneInfo("West Asia Standard Time"),
                "(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi" => TZConvert.GetTimeZoneInfo("India Standard Time"),
                "(UTC+05:30) Sri Jayawardenepura" => TZConvert.GetTimeZoneInfo("Sri Lanka Standard Time"),
                "(UTC+05:45) Kathmandu" => TZConvert.GetTimeZoneInfo("Nepal Standard Time"),
                "(UTC+06:00) Astana" => TZConvert.GetTimeZoneInfo("Central Asia Standard Time"),
                "(UTC+06:00) Dhaka" => TZConvert.GetTimeZoneInfo("Bangladesh Standard Time"),
                "(UTC+06:00) Omsk" => TZConvert.GetTimeZoneInfo("Omsk Standard Time"),
                "(UTC+06:30) Yangon (Rangoon)" => TZConvert.GetTimeZoneInfo("Myanmar Standard Time"),
                "(UTC+07:00) Bangkok, Hanoi, Jakarta" => TZConvert.GetTimeZoneInfo("SE Asia Standard Time"),
                "(UTC+07:00) Barnaul, Gorno-Altaysk" => TZConvert.GetTimeZoneInfo("Altai Standard Time"),
                "(UTC+07:00) Krasnoyarsk" => TZConvert.GetTimeZoneInfo("North Asia Standard Time"),
                "(UTC+07:00) Novosibirsk" => TZConvert.GetTimeZoneInfo("N. Central Asia Standard Time"),
                "(UTC+07:00) Tomsk" => TZConvert.GetTimeZoneInfo("Tomsk Standard Time"),
                "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi" => TZConvert.GetTimeZoneInfo("China Standard Time"),
                "(UTC+08:00) Irkutsk" => TZConvert.GetTimeZoneInfo("North Asia East Standard Time"),
                "(UTC+08:00) Kuala Lumpur, Singapore" => TZConvert.GetTimeZoneInfo("Singapore Standard Time"),
                "(UTC+08:00) Perth" => TZConvert.GetTimeZoneInfo("W. Australia Standard Time"),
                "(UTC+08:00) Taipei" => TZConvert.GetTimeZoneInfo("Taipei Standard Time"),
                "(UTC+08:00) Ulaanbaatar" => TZConvert.GetTimeZoneInfo("Ulaanbaatar Standard Time"),
                "(UTC+09:00) Osaka, Sapporo, Tokyo" => TZConvert.GetTimeZoneInfo("Tokyo Standard Time"),
                "(UTC+09:00) Seoul" => TZConvert.GetTimeZoneInfo("Korea Standard Time"),
                "(UTC+09:00) Yakutsk" => TZConvert.GetTimeZoneInfo("Yakutsk Standard Time"),
                "(UTC+09:30) Adelaide" => TZConvert.GetTimeZoneInfo("Cen. Australia Standard Time"),
                "(UTC+09:30) Darwin" => TZConvert.GetTimeZoneInfo("AUS Central Standard Time"),
                "(UTC+10:00) Brisbane" => TZConvert.GetTimeZoneInfo("E. Australia Standard Time"),
                "(UTC+10:00) Canberra, Melbourne, Sydney" => TZConvert.GetTimeZoneInfo("AUS Eastern Standard Time"),
                "(UTC+10:00) Guam, Port Moresby" => TZConvert.GetTimeZoneInfo("West Pacific Standard Time"),
                "(UTC+10:00) Hobart" => TZConvert.GetTimeZoneInfo("Tasmania Standard Time"),
                "(UTC+10:00) Magadan" => TZConvert.GetTimeZoneInfo("Magadan Standard Time"),
                "(UTC+10:00) Vladivostok" => TZConvert.GetTimeZoneInfo("Vladivostok Standard Time"),
                "(UTC+11:00) Chokurdakh" => TZConvert.GetTimeZoneInfo("Russia Time Zone 10"),
                "(UTC+11:00) Sakhalin" => TZConvert.GetTimeZoneInfo("Sakhalin Standard Time"),
                "(UTC+11:00) Solomon Is., New Caledonia" => TZConvert.GetTimeZoneInfo("Central Pacific Standard Time"),
                "(UTC+12:00) Anadyr, Petropavlovsk-Kamchatsky" => TZConvert.GetTimeZoneInfo("Russia Time Zone 11"),
                "(UTC+12:00) Auckland, Wellington" => TZConvert.GetTimeZoneInfo("New Zealand Standard Time"),
                "(UTC+12:00) Coordinated Universal Time+12" => TZConvert.GetTimeZoneInfo("UTC+12"),
                "(UTC+12:00) Fiji" => TZConvert.GetTimeZoneInfo("Fiji Standard Time"),
                "(UTC+12:00) Petropavlovsk-Kamchatsky - Old" => TZConvert.GetTimeZoneInfo("Kamchatka Standard Time"),
                "(UTC+13:00) Nuku'alofa" => TZConvert.GetTimeZoneInfo("Tonga Standard Time"),
                "(UTC+13:00) Samoa" => TZConvert.GetTimeZoneInfo("Samoa Standard Time"),
                _ => null,
            };

            if (timeZoneInfo != null)
            {
                return timeZoneInfo;
            }

            // Generic handling of other strings, above timezones are returned for SharePoint sites in English, some languages 
            // result in partly differing strings
            if (timeZoneDescription.StartsWith("(UTC) ", StringComparison.InvariantCulture))
            {
                return TZConvert.GetTimeZoneInfo("GMT Standard Time");
            }

            return timeZoneDescription.Substring(0, 11) switch
            {
                "(UTC-12:00)" => TZConvert.GetTimeZoneInfo("Dateline Standard Time"),
                "(UTC-11:00)" => TZConvert.GetTimeZoneInfo("UTC-11"),
                "(UTC-10:00)" => TZConvert.GetTimeZoneInfo("Hawaiian Standard Time"),
                "(UTC-09:00)" => TZConvert.GetTimeZoneInfo("Alaskan Standard Time"),
                "(UTC-08:00)" => TZConvert.GetTimeZoneInfo("Pacific Standard Time"),
                "(UTC-07:00)" => TZConvert.GetTimeZoneInfo("Mountain Standard Time"),
                "(UTC-06:00)" => TZConvert.GetTimeZoneInfo("Central Standard Time"),
                "(UTC-05:00)" => TZConvert.GetTimeZoneInfo("Eastern Standard Time"),
                "(UTC-04:30)" => TZConvert.GetTimeZoneInfo("Venezuela Standard Time"),
                "(UTC-04:00)" => TZConvert.GetTimeZoneInfo("Atlantic Standard Time"),
                "(UTC-03:30)" => TZConvert.GetTimeZoneInfo("Newfoundland Standard Time"),
                "(UTC-03:00)" => TZConvert.GetTimeZoneInfo("E. South America Standard Time"),
                "(UTC-02:00)" => TZConvert.GetTimeZoneInfo("Mid-Atlantic Standard Time"),
                "(UTC-01:00)" => TZConvert.GetTimeZoneInfo("Azores Standard Time"),
                "(UTC+01:00)" => TZConvert.GetTimeZoneInfo("Romance Standard Time"),
                "(UTC+02:00)" => TZConvert.GetTimeZoneInfo("FLE Standard Time"),
                "(UTC+03:00)" => TZConvert.GetTimeZoneInfo("Russian Standard Time"),
                "(UTC+04:00)" => TZConvert.GetTimeZoneInfo("Arabian Standard Time"),
                "(UTC+05:00)" => TZConvert.GetTimeZoneInfo("West Asia Standard Time"),
                "(UTC+05:30)" => TZConvert.GetTimeZoneInfo("India Standard Time"),
                "(UTC+05:45)" => TZConvert.GetTimeZoneInfo("Nepal Standard Time"),
                "(UTC+06:00)" => TZConvert.GetTimeZoneInfo("Central Asia Standard Time"),
                "(UTC+06:30)" => TZConvert.GetTimeZoneInfo("Myanmar Standard Time"),
                "(UTC+07:00)" => TZConvert.GetTimeZoneInfo("SE Asia Standard Time"),
                "(UTC+08:00)" => TZConvert.GetTimeZoneInfo("W. Australia Standard Time"),
                "(UTC+09:00)" => TZConvert.GetTimeZoneInfo("Tokyo Standard Time"),
                "(UTC+09:30)" => TZConvert.GetTimeZoneInfo("AUS Central Standard Time"),
                "(UTC+10:00)" => TZConvert.GetTimeZoneInfo("AUS Eastern Standard Time"),
                "(UTC+11:00)" => TZConvert.GetTimeZoneInfo("Central Pacific Standard Time"),
                "(UTC+12:00)" => TZConvert.GetTimeZoneInfo("New Zealand Standard Time"),
                "(UTC+13:00)" => TZConvert.GetTimeZoneInfo("Tonga Standard Time"),
                _ => throw new ArgumentException("Unknown timezone mapping"),
            };

        }

        #endregion
    }
}
