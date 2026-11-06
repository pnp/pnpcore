using PnP.Core.Provisioning.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Provisioning.Providers.Xml
{
    internal static class RegionalSettingsExtensions
    {
        public static V202103.CalendarType FromTemplateToSchemaCalendarTypeV201605(this CalendarType calendarType)
        {
            switch (calendarType)
            {
                case CalendarType.ChineseLunar:
                    return V202103.CalendarType.ChineseLunar;
                case CalendarType.Gregorian:
                    return V202103.CalendarType.Gregorian;
                case CalendarType.GregorianArabic:
                    return V202103.CalendarType.GregorianArabicCalendar;
                case CalendarType.GregorianMEFrench:
                    return V202103.CalendarType.GregorianMiddleEastFrenchCalendar;
                case CalendarType.GregorianXLITEnglish:
                    return V202103.CalendarType.GregorianTransliteratedEnglishCalendar;
                case CalendarType.GregorianXLITFrench:
                    return V202103.CalendarType.GregorianTransliteratedFrenchCalendar;
                case CalendarType.Hebrew:
                    return V202103.CalendarType.Hebrew;
                case CalendarType.Hijri:
                    return V202103.CalendarType.Hijri;
                case CalendarType.Japan:
                    return V202103.CalendarType.Japan;
                case CalendarType.Korea:
                    return V202103.CalendarType.Korea;
                case CalendarType.KoreaJapanLunar:
                    return V202103.CalendarType.KoreaandJapaneseLunar;
                case CalendarType.SakaEra:
                    return V202103.CalendarType.SakaEra;
                case CalendarType.Taiwan:
                    return V202103.CalendarType.Taiwan;
                case CalendarType.Thai:
                    return V202103.CalendarType.Thai;
                case CalendarType.UmAlQura:
                    return V202103.CalendarType.UmmalQura;
                case CalendarType.None:
                default:
                    return V202103.CalendarType.None;
            }
        }

        public static CalendarType FromSchemaToTemplateCalendarTypeV201605(this V202103.CalendarType calendarType)
        {
            switch (calendarType)
            {
                case V202103.CalendarType.ChineseLunar:
                    return CalendarType.ChineseLunar;
                case V202103.CalendarType.Gregorian:
                    return CalendarType.Gregorian;
                case V202103.CalendarType.GregorianArabicCalendar:
                    return CalendarType.GregorianArabic;
                case V202103.CalendarType.GregorianMiddleEastFrenchCalendar:
                    return CalendarType.GregorianMEFrench;
                case V202103.CalendarType.GregorianTransliteratedEnglishCalendar:
                    return CalendarType.GregorianXLITEnglish;
                case V202103.CalendarType.GregorianTransliteratedFrenchCalendar:
                    return CalendarType.GregorianXLITFrench;
                case V202103.CalendarType.Hebrew:
                    return CalendarType.Hebrew;
                case V202103.CalendarType.Hijri:
                    return CalendarType.Hijri;
                case V202103.CalendarType.Japan:
                    return CalendarType.Japan;
                case V202103.CalendarType.Korea:
                    return CalendarType.Korea;
                case V202103.CalendarType.KoreaandJapaneseLunar:
                    return CalendarType.KoreaJapanLunar;
                case V202103.CalendarType.SakaEra:
                    return CalendarType.SakaEra;
                case V202103.CalendarType.Taiwan:
                    return CalendarType.Taiwan;
                case V202103.CalendarType.Thai:
                    return CalendarType.Thai;
                case V202103.CalendarType.UmmalQura:
                    return CalendarType.UmAlQura;
                case V202103.CalendarType.None:
                default:
                    return CalendarType.None;
            }
        }

        public static V202103.WorkHour FromTemplateToSchemaWorkHourV201605(this Model.WorkHour workHour)
        {
            switch (workHour)
            {
                case Model.WorkHour.AM0100:
                    return V202103.WorkHour.Item100AM;
                case Model.WorkHour.AM0200:
                    return V202103.WorkHour.Item200AM;
                case Model.WorkHour.AM0300:
                    return V202103.WorkHour.Item300AM;
                case Model.WorkHour.AM0400:
                    return V202103.WorkHour.Item400AM;
                case Model.WorkHour.AM0500:
                    return V202103.WorkHour.Item500AM;
                case Model.WorkHour.AM0600:
                    return V202103.WorkHour.Item600AM;
                case Model.WorkHour.AM0700:
                    return V202103.WorkHour.Item700AM;
                case Model.WorkHour.AM0800:
                    return V202103.WorkHour.Item800AM;
                case Model.WorkHour.AM0900:
                    return V202103.WorkHour.Item900AM;
                case Model.WorkHour.AM1000:
                    return V202103.WorkHour.Item1000AM;
                case Model.WorkHour.AM1100:
                    return V202103.WorkHour.Item1100AM;
                case Model.WorkHour.AM1200:
                    return V202103.WorkHour.Item1200AM;
                case Model.WorkHour.PM0100:
                    return V202103.WorkHour.Item100PM;
                case Model.WorkHour.PM0200:
                    return V202103.WorkHour.Item200PM;
                case Model.WorkHour.PM0300:
                    return V202103.WorkHour.Item300PM;
                case Model.WorkHour.PM0400:
                    return V202103.WorkHour.Item400PM;
                case Model.WorkHour.PM0500:
                    return V202103.WorkHour.Item500PM;
                case Model.WorkHour.PM0600:
                    return V202103.WorkHour.Item600PM;
                case Model.WorkHour.PM0700:
                    return V202103.WorkHour.Item700PM;
                case Model.WorkHour.PM0800:
                    return V202103.WorkHour.Item800PM;
                case Model.WorkHour.PM0900:
                    return V202103.WorkHour.Item900PM;
                case Model.WorkHour.PM1000:
                    return V202103.WorkHour.Item1000PM;
                case Model.WorkHour.PM1100:
                    return V202103.WorkHour.Item1100PM;
                case Model.WorkHour.PM1200:
                    return V202103.WorkHour.Item1200PM;
                default:
                    return V202103.WorkHour.Item100AM;
            }
        }

        public static Model.WorkHour FromSchemaToTemplateWorkHourV201605(this V202103.WorkHour workHour)
        {
            switch (workHour)
            {
                case V202103.WorkHour.Item100AM:
                    return Model.WorkHour.AM0100;
                case V202103.WorkHour.Item200AM:
                    return Model.WorkHour.AM0200;
                case V202103.WorkHour.Item300AM:
                    return Model.WorkHour.AM0300;
                case V202103.WorkHour.Item400AM:
                    return Model.WorkHour.AM0400;
                case V202103.WorkHour.Item500AM:
                    return Model.WorkHour.AM0500;
                case V202103.WorkHour.Item600AM:
                    return Model.WorkHour.AM0600;
                case V202103.WorkHour.Item700AM:
                    return Model.WorkHour.AM0700;
                case V202103.WorkHour.Item800AM:
                    return Model.WorkHour.AM0800;
                case V202103.WorkHour.Item900AM:
                    return Model.WorkHour.AM0900;
                case V202103.WorkHour.Item1000AM:
                    return Model.WorkHour.AM1000;
                case V202103.WorkHour.Item1100AM:
                    return Model.WorkHour.AM1100;
                case V202103.WorkHour.Item1200AM:
                    return Model.WorkHour.AM1200;
                case V202103.WorkHour.Item100PM:
                    return Model.WorkHour.PM0100;
                case V202103.WorkHour.Item200PM:
                    return Model.WorkHour.PM0200;
                case V202103.WorkHour.Item300PM:
                    return Model.WorkHour.PM0300;
                case V202103.WorkHour.Item400PM:
                    return Model.WorkHour.PM0400;
                case V202103.WorkHour.Item500PM:
                    return Model.WorkHour.PM0500;
                case V202103.WorkHour.Item600PM:
                    return Model.WorkHour.PM0600;
                case V202103.WorkHour.Item700PM:
                    return Model.WorkHour.PM0700;
                case V202103.WorkHour.Item800PM:
                    return Model.WorkHour.PM0800;
                case V202103.WorkHour.Item900PM:
                    return Model.WorkHour.PM0900;
                case V202103.WorkHour.Item1000PM:
                    return Model.WorkHour.PM1000;
                case V202103.WorkHour.Item1100PM:
                    return Model.WorkHour.PM1100;
                case V202103.WorkHour.Item1200PM:
                    return Model.WorkHour.PM1200;
                default:
                    return Model.WorkHour.AM0100;
            }
        }
    }
}
