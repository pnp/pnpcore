using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Web;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CalendarTypeModel = PnP.Core.Provisioning.Model.CalendarType;
using CalendarTypeCore = PnP.Core.Model.SharePoint.CalendarType;
using RegionalSettingsModel = PnP.Core.Provisioning.Model.RegionalSettings;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Provisions and extracts the web's regional settings.
    /// </summary>
    internal class ObjectRegionalSettings : ObjectHandlerBase
    {
        public override string Name => "Regional Settings";

        public override string InternalName => "RegionalSettings";

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return true;
        }

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            return template.RegionalSettings != null;
        }

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                // Read the raw payload rather than IRegionalSettings: PnP Core's model omits most
                // of what the provisioning schema carries. See WebSettingsRestHelper.
                System.Text.Json.JsonElement settings =
                    await WebSettingsRestHelper.GetRegionalSettingsAsync(context).ConfigureAwait(false);

                template.RegionalSettings = new RegionalSettingsModel
                {
                    AdjustHijriDays = WebSettingsRestHelper.GetInt(settings, "AdjustHijriDays"),
                    AlternateCalendarType = (CalendarTypeModel)WebSettingsRestHelper.GetInt(settings, "AlternateCalendarType"),
                    CalendarType = (CalendarTypeModel)WebSettingsRestHelper.GetInt(settings, "CalendarType"),
                    Collation = WebSettingsRestHelper.GetInt(settings, "Collation"),
                    FirstDayOfWeek = (DayOfWeek)WebSettingsRestHelper.GetInt(settings, "FirstDayOfWeek"),
                    FirstWeekOfYear = WebSettingsRestHelper.GetInt(settings, "FirstWeekOfYear"),
                    LocaleId = WebSettingsRestHelper.GetInt(settings, "LocaleId"),
                    ShowWeeks = WebSettingsRestHelper.GetBool(settings, "ShowWeeks"),
                    Time24 = WebSettingsRestHelper.GetBool(settings, "Time24"),
                    WorkDayEndHour = (WorkHour)WebSettingsRestHelper.GetInt(settings, "WorkDayEndHour"),
                    WorkDayStartHour = (WorkHour)WebSettingsRestHelper.GetInt(settings, "WorkDayStartHour"),
                    WorkDays = WebSettingsRestHelper.GetInt(settings, "WorkDays"),
                };

                // The time zone is a navigation property, so it is not in the payload above.
                try
                {
                    IRegionalSettings modelSettings = await context.Web.RegionalSettings
                        .GetAsync(r => r.TimeZone).ConfigureAwait(false);
                    template.RegionalSettings.TimeZone = modelSettings.TimeZone?.Id ?? 0;
                }
                catch (Exception ex)
                {
                    context.Logger?.LogWarning(ex, "{Source}: could not read the web time zone", Constants.LOGGING_SOURCE);
                }

                return template;
            }
        }

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser, ApplyConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                RegionalSettingsModel settings = template.RegionalSettings;
                if (settings == null)
                {
                    return parser;
                }

                var properties = new Dictionary<string, object>
                {
                    ["AdjustHijriDays"] = settings.AdjustHijriDays,
                    ["AlternateCalendarType"] = (int)ToCore(settings.AlternateCalendarType),
                    ["CalendarType"] = (int)ToCore(settings.CalendarType),
                    ["Collation"] = settings.Collation,
                    ["FirstDayOfWeek"] = (int)settings.FirstDayOfWeek,
                    ["FirstWeekOfYear"] = settings.FirstWeekOfYear,
                    ["LocaleId"] = settings.LocaleId,
                    ["ShowWeeks"] = settings.ShowWeeks,
                    ["Time24"] = settings.Time24,
                    ["WorkDayEndHour"] = (int)settings.WorkDayEndHour,
                    ["WorkDayStartHour"] = (int)settings.WorkDayStartHour,
                    ["WorkDays"] = settings.WorkDays,
                };

                // CSOM, not REST: SP.RegionalSettings is not an updatable REST entity - a MERGE is
                // refused with "does not support HTTP PATCH method". See backlog T1.
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                await CsomRequestSender.SendAsync(context,
                    new SetRegionalSettingsRequest(siteId, webId, properties)).ConfigureAwait(false);

                if (settings.TimeZone > 0)
                {
                    await WebSettingsRestHelper.SetTimeZoneAsync(context, settings.TimeZone).ConfigureAwait(false);
                }

                return parser;
            }
        }

        #region CalendarType conversion

        /// <summary>
        /// Converts the template model's <see cref="CalendarTypeModel"/> to PnP Core's.
        /// </summary>
        private static CalendarTypeCore ToCore(CalendarTypeModel value)
        {
            if (value == CalendarTypeModel.UmAlQura)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,
                    "The template requests the '{0}' calendar, which SharePoint does not support and PnP Core " +
                    "therefore does not model. Applying a different calendar silently would misrepresent the " +
                    "template, so this is reported instead. Change the template's CalendarType, or remove its " +
                    "RegionalSettings element.", value));
            }

            return (CalendarTypeCore)(int)value;
        }

        #endregion
    }
}
