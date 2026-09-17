using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureModel = PnP.Core.Provisioning.Model.Feature;
using ThemeModel = PnP.Core.Provisioning.Model.Theme;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Live coverage for the wave-1 handlers beyond <c>ObjectSiteSettings</c>.
    /// </summary>
    [TestClass]
    public class WaveOneHandlerLiveTests : LiveTestBase
    {
        #region ObjectSearchSettings

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task SearchSettings_ExtractDoesNotThrowOnASiteWithNoCustomization()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                ProvisioningTemplate template = await context.GetProvisioningManager()
                    .GetTemplateAsync().ConfigureAwait(false);

                Console.WriteLine($"SiteSearchSettings: {(string.IsNullOrEmpty(template.SiteSearchSettings) ? "<none>" : $"{template.SiteSearchSettings.Length} chars")}");
                Console.WriteLine($"WebSearchSettings : {(string.IsNullOrEmpty(template.WebSearchSettings) ? "<none>" : $"{template.WebSearchSettings.Length} chars")}");

                Assert.IsNotNull(template);
            }
        }

        #endregion

        #region ObjectFeatures

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Features_ActivateAndDeactivateAWebFeature()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                Guid featureId = Constants.FeatureId_Web_MinimalDownloadStrategy;

                await context.Web.LoadAsync(w => w.Features.QueryProperties(f => f.DefinitionId)).ConfigureAwait(false);
                bool wasActive = context.Web.Features.AsRequested().Any(f => f.DefinitionId == featureId);
                Console.WriteLine($"MDS feature initially active: {wasActive}");

                IProvisioningManager manager = context.GetProvisioningManager();

                try
                {
                    var template = new ProvisioningTemplate();
                    template.Features.WebFeatures.Add(new FeatureModel { Id = featureId, Deactivate = wasActive });

                    await manager.ApplyTemplateAsync(template).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        await fresh.Web.LoadAsync(w => w.Features.QueryProperties(f => f.DefinitionId)).ConfigureAwait(false);
                        bool nowActive = fresh.Web.Features.AsRequested().Any(f => f.DefinitionId == featureId);

                        Assert.AreEqual(!wasActive, nowActive,
                            $"The feature should now be {(!wasActive ? "active" : "inactive")}.");
                    }
                }
                finally
                {
                    try
                    {
                        var restore = new ProvisioningTemplate();
                        restore.Features.WebFeatures.Add(new FeatureModel { Id = featureId, Deactivate = !wasActive });
                        await manager.ApplyTemplateAsync(restore).ConfigureAwait(false);
                        Console.WriteLine($"Restored MDS feature to active={wasActive}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"COULD NOT RESTORE the MDS feature: {Describe(ex)}");
                    }
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Features_AnUnavailableFeatureWarnsRatherThanFailingTheRun()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                var template = new ProvisioningTemplate();
                template.Features.WebFeatures.Add(new FeatureModel { Id = Guid.NewGuid(), Deactivate = false });

                var warnings = new List<string>();
                var configuration = new ApplyConfiguration
                {
                    MessagesDelegate = (message, type) =>
                    {
                        if (type == ProvisioningMessageType.Warning) warnings.Add(message);
                    },
                };

                await context.GetProvisioningManager().ApplyTemplateAsync(template, configuration).ConfigureAwait(false);

                Assert.IsTrue(warnings.Count > 0,
                    "Activating a non-existent feature should produce a warning, not silence.");
                Console.WriteLine($"Warning reported: {warnings[0]}");
            }
        }

        #endregion

        #region ObjectTheme

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Theme_AppliesABuiltInThemeByName()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                var template = new ProvisioningTemplate
                {
                    Theme = new ThemeModel { Name = nameof(SharePointTheme.Blue) },
                };

                await context.GetProvisioningManager().ApplyTemplateAsync(template).ConfigureAwait(false);

                Console.WriteLine("Applied the built-in 'Blue' theme.");
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Theme_APaletteWarnsThatItWasNotApplied()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                var template = new ProvisioningTemplate
                {
                    Theme = new ThemeModel
                    {
                        Name = $"{TestPrefix}Palette",
                        Palette = "{\"themePrimary\":\"#0078d4\"}",
                    },
                };

                var warnings = new List<string>();
                var configuration = new ApplyConfiguration
                {
                    MessagesDelegate = (message, type) =>
                    {
                        if (type == ProvisioningMessageType.Warning) warnings.Add(message);
                    },
                };

                await context.GetProvisioningManager().ApplyTemplateAsync(template, configuration).ConfigureAwait(false);

                Assert.IsTrue(warnings.Any(w => w.Contains("T13", StringComparison.Ordinal)),
                    "A palette-based theme must warn that it was not applied, naming the backlog item. " +
                    $"Warnings seen: {string.Join(" | ", warnings)}");

                Console.WriteLine($"Warning reported: {warnings.First(w => w.Contains("T13", StringComparison.Ordinal))}");
            }
        }

        #endregion
    }
}
