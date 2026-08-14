namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Specifies the type of calendar used in a particular context.
    /// </summary>
    public enum CalendarType
    {
        /// <summary>
        /// The calendar type is not specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Gregorian (localized) calendar.
        /// </summary>
        Gregorian = 1,

        /// <summary>
        /// Japanese Emperor Era calendar.
        /// </summary>
        Japan = 3,

        /// <summary>
        /// Taiwan calendar.
        /// </summary>
        Taiwan = 4,

        /// <summary>
        /// Korean Tangun Era calendar.
        /// </summary>
        Korea = 5,

        /// <summary>
        /// Hijri (Arabic Lunar) calendar.
        /// </summary>
        Hijri = 6,

        /// <summary>
        /// Thai calendar.
        /// </summary>
        Thai = 7,

        /// <summary>
        /// Hebrew (Lunar) calendar.
        /// </summary>
        Hebrew = 8,

        /// <summary>
        /// Gregorian Middle East French calendar.
        /// </summary>
        GregorianMEFrench = 9,

        /// <summary>
        /// Gregorian Arabic calendar.
        /// </summary>
        GregorianArabic = 10,

        /// <summary>
        /// Gregorian transliterated English calendar.
        /// </summary>
        GregorianXLITEnglish = 11,

        /// <summary>
        /// Gregorian transliterated French calendar.
        /// </summary>
        GregorianXLITFrench = 12,

        /// <summary>
        /// Korean and Japanese Lunar calendar.
        /// </summary>
        KoreaJapanLunar = 14,

        /// <summary>
        /// Chinese Lunar calendar.
        /// </summary>
        ChineseLunar = 15,

        /// <summary>
        /// Saka Era calendar. Not represented in PnP Core's CalendarType.
        /// </summary>
        SakaEra = 16,

        /// <summary>
        /// Umm al-Qura calendar. Not represented in PnP Core's CalendarType.
        /// </summary>
        UmAlQura = 23
    }
}
