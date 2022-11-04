using System;
using TimeZoneConverter;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// TimeZone class, write your custom code here
    /// </summary>
    [SharePointType("SP.TimeZone", Uri = "_api/web/regionalsettings/timezone", LinqGet = "_api/web/regionalsettings/timezone")]
    internal sealed class TimeZone : BaseDataModel<ITimeZone>, ITimeZone
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

        public TimeZoneInfo GetTimeZoneInfo()
        {
            return GetTimeZoneInfoFromSharePoint(Id);
        }

        private TimeSpan UtcDelta(DateTime dateTime)
        {
            return new TimeSpan(0, Bias + (GetTimeZoneInfoFromSharePoint(Id).IsDaylightSavingTime(dateTime) ? DaylightBias : StandardBias), 0);
        }

        internal static TimeZoneInfo GetTimeZoneInfoFromSharePoint(int timeZoneId)
        {
            var timeZoneInfo = timeZoneId switch
            {
                2 => TZConvert.GetTimeZoneInfo("UTC"), // (UTC) Dublin, Edinburgh, Lisbon, London
                3 => TZConvert.GetTimeZoneInfo("Romance Standard Time"), // (UTC+01:00) Brussels, Copenhagen, Madrid, Paris
                4 => TZConvert.GetTimeZoneInfo("W. Europe Standard Time"), // (UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna
                5 => TZConvert.GetTimeZoneInfo("GTB Standard Time"), // (UTC+02:00) Athens, Bucharest
                6 => TZConvert.GetTimeZoneInfo("Central Europe Standard Time"), // (UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague
                7 => TZConvert.GetTimeZoneInfo("Belarus Standard Time"), // (UTC+02:00) Minsk (old)
                8 => TZConvert.GetTimeZoneInfo("E. South America Standard Time"), // (UTC-03:00) Brasilia
                9 => TZConvert.GetTimeZoneInfo("Atlantic Standard Time"), // (UTC-04:00) Atlantic Time (Canada)
                10 => TZConvert.GetTimeZoneInfo("US Eastern Standard Time"), // (UTC-05:00) Eastern Time (US and Canada)
                11 => TZConvert.GetTimeZoneInfo("Central Standard Time"), // (UTC-06:00) Central Time (US and Canada)
                12 => TZConvert.GetTimeZoneInfo("US Mountain Standard Time"), // (UTC-07:00) Mountain Time (US and Canada)
                13 => TZConvert.GetTimeZoneInfo("Pacific Standard Time"), // (UTC-08:00) Pacific Time (US and Canada)
                14 => TZConvert.GetTimeZoneInfo("Alaskan Standard Time"), // (UTC-09:00) Alaska
                15 => TZConvert.GetTimeZoneInfo("Hawaiian Standard Time"), // (UTC-10:00) Hawaii
                16 => TZConvert.GetTimeZoneInfo("Samoa Standard Time"), // (UTC+13:00) Samoa
                17 => TZConvert.GetTimeZoneInfo("New Zealand Standard Time"), // (UTC+12:00) Auckland, Wellington
                18 => TZConvert.GetTimeZoneInfo("E. Australia Standard Time"), // (UTC+10:00) Brisbane
                19 => TZConvert.GetTimeZoneInfo("Cen. Australia Standard Time"), // (UTC+09:30) Adelaide
                20 => TZConvert.GetTimeZoneInfo("Tokyo Standard Time"), // (UTC+09:00) Osaka, Sapporo, Tokyo
                21 => TZConvert.GetTimeZoneInfo("Singapore Standard Time"), // (UTC+08:00) Kuala Lumpur, Singapore
                22 => TZConvert.GetTimeZoneInfo("SE Asia Standard Time"), // (UTC+07:00) Bangkok, Hanoi, Jakarta
                23 => TZConvert.GetTimeZoneInfo("India Standard Time"), // (UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi
                24 => TZConvert.GetTimeZoneInfo("Arabian Standard Time"), // (UTC+04:00) Abu Dhabi, Muscat
                25 => TZConvert.GetTimeZoneInfo("Iran Standard Time"), // (UTC+03:30) Tehran
                26 => TZConvert.GetTimeZoneInfo("Arabic Standard Time"), // (UTC+03:00) Baghdad
                27 => TZConvert.GetTimeZoneInfo("Israel Standard Time"), // (UTC+02:00) Jerusalem
                28 => TZConvert.GetTimeZoneInfo("Newfoundland Standard Time"), // (UTC-03:30) Newfoundland
                29 => TZConvert.GetTimeZoneInfo("Azores Standard Time"), // (UTC-01:00) Azores
                30 => TZConvert.GetTimeZoneInfo("Mid-Atlantic Standard Time"), // (UTC-02:00) Mid-Atlantic
                31 => TZConvert.GetTimeZoneInfo("Greenwich Standard Time"), // (UTC) Monrovia, Reykjavik
                32 => TZConvert.GetTimeZoneInfo("SA Eastern Standard Time"), // (UTC-03:00) Cayenne, Fortaleza
                33 => TZConvert.GetTimeZoneInfo("SA Western Standard Time"), // (UTC-04:00) Georgetown, La Paz, Manaus, San Juan
                34 => TZConvert.GetTimeZoneInfo("US Eastern Standard Time"), // (UTC-05:00) Indiana (East)
                35 => TZConvert.GetTimeZoneInfo("SA Pacific Standard Time"), // (UTC-05:00) Bogota, Lima, Quito
                36 => TZConvert.GetTimeZoneInfo("Canada Central Standard Time"), // (UTC-06:00) Saskatchewan
                37 => TZConvert.GetTimeZoneInfo("Central Standard Time (Mexico)"), // (UTC-06:00) Guadalajara, Mexico City, Monterrey
                38 => TZConvert.GetTimeZoneInfo("US Mountain Standard Time"), // (UTC-07:00) Arizona
                39 => TZConvert.GetTimeZoneInfo("Dateline Standard Time"), // (UTC-12:00) International Date Line West
                40 => TZConvert.GetTimeZoneInfo("Fiji Standard Time"), // (UTC+12:00) Fiji
                41 => TZConvert.GetTimeZoneInfo("Central Pacific Standard Time"), // (UTC+11:00) Solomon Is., New Caledonia
                42 => TZConvert.GetTimeZoneInfo("Tasmania Standard Time"), // (UTC+10:00) Hobart
                43 => TZConvert.GetTimeZoneInfo("West Pacific Standard Time"), // (UTC+10:00) Guam, Port Moresby
                44 => TZConvert.GetTimeZoneInfo("AUS Central Standard Time"), // (UTC+09:30) Darwin
                45 => TZConvert.GetTimeZoneInfo("China Standard Time"), // (UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi
                46 => TZConvert.GetTimeZoneInfo("N. Central Asia Standard Time"), // (UTC+07:00) Novosibirsk
                47 => TZConvert.GetTimeZoneInfo("West Asia Standard Time"), // (UTC+05:00) Tashkent
                48 => TZConvert.GetTimeZoneInfo("Afghanistan Standard Time"), // (UTC+04:30) Kabul
                49 => TZConvert.GetTimeZoneInfo("Egypt Standard Time"), // (UTC+02:00) Cairo
                50 => TZConvert.GetTimeZoneInfo("South Africa Standard Time"), // (UTC+02:00) Harare, Pretoria
                51 => TZConvert.GetTimeZoneInfo("Russian Standard Time"), // (UTC+03:00) Moscow, St. Petersburg, Volgograd
                53 => TZConvert.GetTimeZoneInfo("Cape Verde Standard Time"), // (UTC-01:00) Cabo Verde
                54 => TZConvert.GetTimeZoneInfo("Azerbaijan Standard Time"), // (UTC+04:00) Baku
                55 => TZConvert.GetTimeZoneInfo("Central America Standard Time"), // (UTC-06:00) Central America
                56 => TZConvert.GetTimeZoneInfo("E. Africa Standard Time"), // (UTC+03:00) Nairobi
                57 => TZConvert.GetTimeZoneInfo("Central European Standard Time"), // (UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb
                58 => TZConvert.GetTimeZoneInfo("Ekaterinburg Standard Time"), // (UTC+05:00) Ekaterinburg
                59 => TZConvert.GetTimeZoneInfo("FLE Standard Time"), // (UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius
                60 => TZConvert.GetTimeZoneInfo("Greenland Standard Time"), // (UTC-03:00) Greenland
                61 => TZConvert.GetTimeZoneInfo("Myanmar Standard Time"), // (UTC+06:30) Yangon (Rangoon)
                62 => TZConvert.GetTimeZoneInfo("Nepal Standard Time"), // (UTC+05:45) Kathmandu
                63 => TZConvert.GetTimeZoneInfo("North Asia East Standard Time"), // (UTC+08:00) Irkutsk
                64 => TZConvert.GetTimeZoneInfo("North Asia Standard Time"), // (UTC+07:00) Krasnoyarsk
                65 => TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"), // (UTC-04:00) Santiago
                66 => TZConvert.GetTimeZoneInfo("Sri Lanka Standard Time"), // (UTC+05:30) Sri Jayawardenepura
                67 => TZConvert.GetTimeZoneInfo("Tonga Standard Time"), // (UTC+13:00) Nuku'alofa
                68 => TZConvert.GetTimeZoneInfo("Vladivostok Standard Time"), // (UTC+10:00) Vladivostok
                69 => TZConvert.GetTimeZoneInfo("W. Central Africa Standard Time"), // (UTC+01:00) West Central Africa
                70 => TZConvert.GetTimeZoneInfo("Yakutsk Standard Time"), // (UTC+09:00) Yakutsk
                71 => TZConvert.GetTimeZoneInfo("Central Asia Standard Time"), // (UTC+06:00) Astana
                72 => TZConvert.GetTimeZoneInfo("Korea Standard Time"), // (UTC+09:00) Seoul
                73 => TZConvert.GetTimeZoneInfo("W. Australia Standard Time"), // (UTC+08:00) Perth
                74 => TZConvert.GetTimeZoneInfo("Arab Standard Time"), // (UTC+03:00) Kuwait, Riyadh
                75 => TZConvert.GetTimeZoneInfo("Taipei Standard Time"), // (UTC+08:00) Taipei
                76 => TZConvert.GetTimeZoneInfo("AUS Eastern Standard Time"), // (UTC+10:00) Canberra, Melbourne, Sydney
                77 => TZConvert.GetTimeZoneInfo("Mountain Standard Time (Mexico)"), // (UTC-07:00) Chihuahua, La Paz, Mazatlan
                78 => TZConvert.GetTimeZoneInfo("Pacific Standard Time (Mexico)"), // (UTC-08:00) Baja California
                79 => TZConvert.GetTimeZoneInfo("Jordan Standard Time"), // (UTC+02:00) Amman
                80 => TZConvert.GetTimeZoneInfo("Middle East Standard Time"), // (UTC+02:00) Beirut
                81 => TZConvert.GetTimeZoneInfo("Central Brazilian Standard Time"), // (UTC-04:00) Cuiaba
                82 => TZConvert.GetTimeZoneInfo("Georgian Standard Time"), // (UTC+04:00) Tbilisi
                83 => TZConvert.GetTimeZoneInfo("Namibia Standard Time"), // (UTC+01:00) Windhoek
                84 => TZConvert.GetTimeZoneInfo("Caucasus Standard Time"), // (UTC+04:00) Yerevan
                85 => TZConvert.GetTimeZoneInfo("Argentina Standard Time"), // (UTC-03:00) Buenos Aires
                86 => TZConvert.GetTimeZoneInfo("Morocco Standard Time"), // (UTC) Casablanca
                87 => TZConvert.GetTimeZoneInfo("Pakistan Standard Time"), // (UTC+05:00) Islamabad, Karachi
                88 => TZConvert.GetTimeZoneInfo("Venezuela Standard Time"), // (UTC-04:30) Caracas
                89 => TZConvert.GetTimeZoneInfo("Mauritius Standard Time"), // (UTC+04:00) Port Louis
                90 => TZConvert.GetTimeZoneInfo("Montevideo Standard Time"), // (UTC-03:00) Montevideo
                91 => TZConvert.GetTimeZoneInfo("Paraguay Standard Time"), // (UTC-04:00) Asuncion
                92 => TZConvert.GetTimeZoneInfo("Kamchatka Standard Time"), // (UTC+12:00) Petropavlovsk-Kamchatsky - Old
                93 => TZConvert.GetTimeZoneInfo("UTC"), // (UTC) Coordinated Universal Time
                94 => TZConvert.GetTimeZoneInfo("Ulaanbaatar Standard Time"), // (UTC+08:00) Ulaanbaatar
                95 => TZConvert.GetTimeZoneInfo("UTC-11"), // (UTC-11:00) Coordinated Universal Time-11
                96 => TZConvert.GetTimeZoneInfo("UTC-02"), // (UTC-02:00) Coordinated Universal Time-02
                97 => TZConvert.GetTimeZoneInfo("UTC+12"), // (UTC+12:00) Coordinated Universal Time+12
                98 => TZConvert.GetTimeZoneInfo("Syria Standard Time"), // (UTC+02:00) Damascus
                99 => TZConvert.GetTimeZoneInfo("Magadan Standard Time"), // (UTC+10:00) Magadan
                100 => TZConvert.GetTimeZoneInfo("Kaliningrad Standard Time"), // (UTC+02:00) Kaliningrad
                101 => TZConvert.GetTimeZoneInfo("Turkey Standard Time"), // (UTC+03:00) Istanbul
                102 => TZConvert.GetTimeZoneInfo("Bangladesh Standard Time"), // (UTC+06:00) Dhaka
                103 => TZConvert.GetTimeZoneInfo("Bahia Standard Time"), // (UTC-03:00) Salvador
                104 => TZConvert.GetTimeZoneInfo("E. Europe Standard Time"), // (UTC+02:00) E. Europe
                106 => TZConvert.GetTimeZoneInfo("Russia Time Zone 3"), // (UTC+04:00) Izhevsk, Samara
                107 => TZConvert.GetTimeZoneInfo("Russia Time Zone 10"), // (UTC+11:00) Chokurdakh
                108 => TZConvert.GetTimeZoneInfo("Russia Time Zone 11"), // (UTC+12:00) Anadyr, Petropavlovsk-Kamchatsky
                109 => TZConvert.GetTimeZoneInfo("Belarus Standard Time"), // (UTC+03:00) Minsk
                110 => TZConvert.GetTimeZoneInfo("Astrakhan Standard Time"), // (UTC+04:00) Astrakhan, Ulyanovsk
                111 => TZConvert.GetTimeZoneInfo("Altai Standard Time"), // (UTC+07:00) Barnaul, Gorno-Altaysk
                112 => TZConvert.GetTimeZoneInfo("Tomsk Standard Time"), // (UTC+07:00) Tomsk
                114 => TZConvert.GetTimeZoneInfo("Sakhalin Standard Time"), // (UTC+11:00) Sakhalin
                115 => TZConvert.GetTimeZoneInfo("Omsk Standard Time") // (UTC+06:00) Omsk
,
                _ => throw new ArgumentException("Unknown timezone mapping")
            };

            return timeZoneInfo; 
        }

        #endregion
    }
}
