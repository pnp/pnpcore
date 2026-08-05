// PnP.Core.Model.SharePoint also declares a CalendarType, but it omits SakaEra and UmAlQura which
// the provisioning schema exposes, so the provisioning model copy is the one that must be used.
using CalendarType = PnP.Core.Provisioning.Model.CalendarType;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    internal class CalendarTypeFromSchemaToModelValueResolver : IValueResolver
    {
        public string Name => this.GetType().Name;

        public object Resolve(object source, object destination, object sourceValue)
        {
            var calendarType = sourceValue?.ToString();
            switch (calendarType)
            {
                case "ChineseLunar":
                    return CalendarType.ChineseLunar;
                case "Gregorian":
                    return CalendarType.Gregorian;
                case "GregorianArabicCalendar":
                    return CalendarType.GregorianArabic;
                case "GregorianMiddleEastFrenchCalendar":
                    return CalendarType.GregorianMEFrench;
                case "GregorianTransliteratedEnglishCalendar":
                    return CalendarType.GregorianXLITEnglish;
                case "GregorianTransliteratedFrenchCalendar":
                    return CalendarType.GregorianXLITFrench;
                case "Hebrew":
                    return CalendarType.Hebrew;
                case "Hijri":
                    return CalendarType.Hijri;
                case "Japan":
                    return CalendarType.Japan;
                case "Korea":
                    return CalendarType.Korea;
                case "KoreaandJapaneseLunar":
                    return CalendarType.KoreaJapanLunar;
                case "SakaEra":
                    return CalendarType.SakaEra;
                case "Taiwan":
                    return CalendarType.Taiwan;
                case "Thai":
                    return CalendarType.Thai;
                case "UmmalQura":
                    return CalendarType.UmAlQura;
                case "None":
                default:
                    return CalendarType.None;
            }
        }
    }
}
