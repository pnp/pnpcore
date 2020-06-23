namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// A calendar type is a 2-byte integer value that specifies the type of calendar to use in a particular context.
    /// https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-csom/ee543260%28v%3doffice.15%29
    /// </summary>
    public enum CalendarType
    {
        /// <summary>
        /// The calendar type is not specified.
        /// </summary>
        None = 0,
        /// <summary>
        /// Specifies Gregorian (localized) calendar type.
        /// </summary>
        Gregorian = 1,
        /// <summary>
        /// Specifies a Japanese Emperor Era calendar type.
        /// </summary>
        Japan = 3,
        /// <summary>
        /// Specifies a Year of Taiwan calendar type.
        /// </summary>
        Taiwan = 4,
        /// <summary>
        /// Specifies a Korean Tangun Era calendar type.
        /// </summary>
        Korea = 5,
        /// <summary>
        /// Specifies a Hijri (Arabic Lunar) calendar type.
        /// </summary>
        Hijri = 6,
        /// <summary>
        /// Specifies a Thai calendar type.
        /// </summary>
        Thai = 7,
        /// <summary>
        /// Specifies a Hebrew (Lunar) calendar type.
        /// </summary>
        Hebrew = 8,
        /// <summary>
        /// Specifies a Gregorian (Middle East French) calendar type.
        /// </summary>
        GregorianMEFrench = 9,
        /// <summary>
        /// Specifies a Gregorian (Arabic) calendar type.
        /// </summary>
        GregorianArabic = 10,
        /// <summary>
        /// Specifies a Gregorian (transliterated English) calendar type.
        /// </summary>
        GregorianXLITEnglish = 11,
        /// <summary>
        /// Specifies a Gregorian (transliterated French) calendar type.
        /// </summary>
        GregorianXLITFrench = 12,
        /// <summary>
        /// Specifies a Korean and Japanese Lunar calendar type.
        /// </summary>
        KoreaJapanLunar = 14,
        /// <summary>
        /// Specifies Chinese Lunar calendar type.
        /// </summary>
        ChineseLunar = 15,
        /// <summary>
        /// Specifies a Saka Era calendar type.
        /// </summary>
        SakaEra = 16
        // Not Supported (e.g. https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-csom/ee543260%28v%3doffice.15%29#remarks)
        //UmAlQura                , // Specifies an Umm al-Qura calendar type.
    }
}
