using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using CalendarTypeModel = PnP.Core.Provisioning.Model.CalendarType;
using RegionalSettingsModel = PnP.Core.Provisioning.Model.RegionalSettings;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    [TestClass]
    public class RegionalAndLanguageLiveTests : LiveTestBase
    {
        #region T1 - regional settings

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task RegionalSettings_ExtractReadsThePropertiesPnPCoreCannotModel()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                ProvisioningTemplate template = await context.GetProvisioningManager()
                    .GetTemplateAsync().ConfigureAwait(false);

                Assert.IsNotNull(template.RegionalSettings, "Regional settings were not extracted.");

                RegionalSettingsModel settings = template.RegionalSettings;
                Console.WriteLine($"LocaleId        : {settings.LocaleId}");
                Console.WriteLine($"CalendarType    : {settings.CalendarType}");
                Console.WriteLine($"Collation       : {settings.Collation}");
                Console.WriteLine($"FirstDayOfWeek  : {settings.FirstDayOfWeek}");
                Console.WriteLine($"FirstWeekOfYear : {settings.FirstWeekOfYear}");
                Console.WriteLine($"WorkDays        : {settings.WorkDays}");
                Console.WriteLine($"WorkDayStart    : {settings.WorkDayStartHour}");
                Console.WriteLine($"WorkDayEnd      : {settings.WorkDayEndHour}");
                Console.WriteLine($"TimeZone        : {settings.TimeZone}");

                Assert.AreNotEqual(0, settings.LocaleId, "LocaleId should never be 0 on a real site.");
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task RegionalSettings_RoundTripsAChangedWorkingDay()
        {
            using (PnPContext context = await GetClassicContextAsync().ConfigureAwait(false))
            {
                IProvisioningManager manager = context.GetProvisioningManager();

                ProvisioningTemplate before = await manager.GetTemplateAsync().ConfigureAwait(false);
                RegionalSettingsModel original = before.RegionalSettings;
                Assert.IsNotNull(original);

                try
                {
                    var changed = new ProvisioningTemplate
                    {
                        RegionalSettings = CloneWith(original, w => w.ShowWeeks = !original.ShowWeeks),
                    };

                    await manager.ApplyTemplateAsync(changed).ConfigureAwait(false);

                    using (PnPContext fresh = await GetClassicContextAsync(1).ConfigureAwait(false))
                    {
                        ProvisioningTemplate after = await fresh.GetProvisioningManager()
                            .GetTemplateAsync().ConfigureAwait(false);

                        Assert.AreEqual(!original.ShowWeeks, after.RegionalSettings.ShowWeeks,
                            "ShowWeeks did not round-trip - the MERGE against _api/web/regionalsettings is not landing.");
                    }
                }
                finally
                {
                    try
                    {
                        await manager.ApplyTemplateAsync(new ProvisioningTemplate { RegionalSettings = original })
                            .ConfigureAwait(false);
                        Console.WriteLine("Restored the original regional settings.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"COULD NOT RESTORE regional settings: {Describe(ex)}");
                    }
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task RegionalSettings_UmAlQuraFailsLoudlyRatherThanSilently()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                var template = new ProvisioningTemplate
                {
                    RegionalSettings = new RegionalSettingsModel { CalendarType = CalendarTypeModel.UmAlQura },
                };

                NotSupportedException ex = await Assert.ThrowsExceptionAsync<NotSupportedException>(() =>
                    context.GetProvisioningManager().ApplyTemplateAsync(template)).ConfigureAwait(false);

                StringAssert.Contains(ex.Message, "UmAlQura",
                    "The error should name the offending calendar so the template can be fixed.");
                Console.WriteLine($"Reported: {ex.Message}");
            }
        }

        private static RegionalSettingsModel CloneWith(RegionalSettingsModel source, Action<RegionalSettingsModel> change)
        {
            var clone = new RegionalSettingsModel
            {
                AdjustHijriDays = source.AdjustHijriDays,
                AlternateCalendarType = source.AlternateCalendarType,
                CalendarType = source.CalendarType,
                Collation = source.Collation,
                FirstDayOfWeek = source.FirstDayOfWeek,
                FirstWeekOfYear = source.FirstWeekOfYear,
                LocaleId = source.LocaleId,
                ShowWeeks = source.ShowWeeks,
                Time24 = source.Time24,
                TimeZone = source.TimeZone,
                WorkDayEndHour = source.WorkDayEndHour,
                WorkDayStartHour = source.WorkDayStartHour,
                WorkDays = source.WorkDays,
            };

            change(clone);
            return clone;
        }

        #endregion

        #region T2 - supported UI languages

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task SupportedUILanguages_ExtractListsTheConfiguredLanguages()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                ProvisioningTemplate template = await context.GetProvisioningManager()
                    .GetTemplateAsync().ConfigureAwait(false);

                Assert.IsTrue(template.SupportedUILanguages.Count > 0,
                    "Every web supports at least its own default language.");

                Console.WriteLine($"Supported UI languages: {template.SupportedUILanguages.Count}");
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task SupportedUILanguages_AddsALanguageAndRemovesItAgain()
        {
            using (PnPContext context = await GetClassicContextAsync().ConfigureAwait(false))
            {
                const int probeLcid = 1035; // Finnish

                IWeb web = await context.Web.GetAsync(w => w.SupportedUILanguageIds, w => w.Language).ConfigureAwait(false);
                bool wasPresent = web.SupportedUILanguageIds?.Contains(probeLcid) == true;
                Console.WriteLine($"Finnish initially supported: {wasPresent}");

                if (wasPresent)
                {
                    Assert.Inconclusive(
                        "Finnish is already enabled on the classic site, so adding it proves nothing. " +
                        "Pick a different probe language or disable it first.");
                    return;
                }

                var original = web.SupportedUILanguageIds.ToList();

                try
                {
                    var template = new ProvisioningTemplate();
                    foreach (int lcid in original)
                    {
                        template.SupportedUILanguages.Add(new SupportedUILanguage { LCID = lcid });
                    }
                    template.SupportedUILanguages.Add(new SupportedUILanguage { LCID = probeLcid });

                    await context.GetProvisioningManager().ApplyTemplateAsync(template).ConfigureAwait(false);

                    using (PnPContext fresh = await GetClassicContextAsync(1).ConfigureAwait(false))
                    {
                        IWeb after = await fresh.Web.GetAsync(w => w.SupportedUILanguageIds).ConfigureAwait(false);

                        Assert.IsTrue(after.SupportedUILanguageIds.Contains(probeLcid),
                            $"Language {probeLcid} was not added. Supported: " +
                            string.Join(", ", after.SupportedUILanguageIds));
                    }
                }
                finally
                {
                    try
                    {
                        var restore = new ProvisioningTemplate();
                        foreach (int lcid in original)
                        {
                            restore.SupportedUILanguages.Add(new SupportedUILanguage { LCID = lcid });
                        }

                        await context.GetProvisioningManager().ApplyTemplateAsync(restore).ConfigureAwait(false);
                        Console.WriteLine("Restored the original supported UI languages.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"COULD NOT RESTORE supported UI languages: {Describe(ex)}");
                    }
                }
            }
        }

        #endregion
    }
}
