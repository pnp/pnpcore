using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using AuditSettingsModel = PnP.Core.Provisioning.Model.AuditSettings;
using ImageRenditionModel = PnP.Core.Provisioning.Model.ImageRendition;
using PublishingModel = PnP.Core.Provisioning.Model.Publishing;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    [TestClass]
    public class ClassicSiteSettingsLiveTests : LiveTestBase
    {
        #region Audit settings

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Audit_AppliesTheRetentionSettings()
        {
            using (PnPContext context = await GetClassicContextAsync().ConfigureAwait(false))
            {
                await context.Site.LoadAsync(s => s.AuditLogTrimmingRetention, s => s.TrimAuditLog).ConfigureAwait(false);

                int originalRetention = context.Site.AuditLogTrimmingRetention;
                bool originalTrim = context.Site.TrimAuditLog;
                Console.WriteLine($"Before: retention {originalRetention}, trim {originalTrim}");

                try
                {
                    var template = new ProvisioningTemplate
                    {
                        AuditSettings = new AuditSettingsModel
                        {
                            TrimAuditLog = true,
                            AuditLogTrimmingRetention = 37,
                        },
                    };

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetClassicContextAsync(1).ConfigureAwait(false))
                    {
                        await fresh.Site.LoadAsync(s => s.AuditLogTrimmingRetention, s => s.TrimAuditLog)
                            .ConfigureAwait(false);

                        Console.WriteLine($"After: retention {fresh.Site.AuditLogTrimmingRetention}, trim {fresh.Site.TrimAuditLog}");

                        Assert.IsTrue(fresh.Site.TrimAuditLog, "Audit log trimming was not turned on.");

                        Assert.AreEqual(37, fresh.Site.AuditLogTrimmingRetention,
                            "The audit log trimming retention was not applied.");
                    }
                }
                finally
                {
                    await RestoreAuditAsync(originalRetention, originalTrim).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Audit_ExtractsNothingWhenTheSiteIsAtItsDefaults()
        {
            using (PnPContext context = await GetClassicContextAsync().ConfigureAwait(false))
            {
                var configuration = new ExtractConfiguration();
                configuration.Handlers.Add(ConfigurationHandler.AuditSettings);

                ProvisioningTemplate extracted = await context.GetProvisioningManager()
                    .GetTemplateAsync(configuration).ConfigureAwait(false);

                if (extracted.AuditSettings == null)
                {
                    Console.WriteLine("No audit settings element - the site is at its defaults.");
                    return;
                }

                Console.WriteLine($"Audit settings: flags {extracted.AuditSettings.AuditFlags}, " +
                    $"trim {extracted.AuditSettings.TrimAuditLog}, retention {extracted.AuditSettings.AuditLogTrimmingRetention}");

                var defaults = new AuditSettingsModel();

                Assert.IsFalse(extracted.AuditSettings.Equals(defaults),
                    "An audit settings element was emitted that is identical to the defaults.");
            }
        }

        #endregion

        #region Site policy

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task SitePolicy_AnUnknownPolicyWarnsRatherThanFailing()
        {
            using (PnPContext context = await GetClassicContextAsync().ConfigureAwait(false))
            {
                string warning = null;

                var template = new ProvisioningTemplate
                {
                    SitePolicy = $"{TestPrefix}NoSuchPolicy",
                };

                var configuration = new ApplyConfiguration
                {
                    MessagesDelegate = (message, type) =>
                    {
                        Console.WriteLine($"[{type}] {message}");

                        if (type == ProvisioningMessageType.Warning && message.Contains($"{TestPrefix}NoSuchPolicy"))
                        {
                            warning = message;
                        }
                    },
                };

                await context.GetProvisioningManager().ApplyTemplateAsync(template, configuration).ConfigureAwait(false);

                Assert.IsNotNull(warning, "Applying an unknown site policy produced no warning naming it.");
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task SitePolicy_ExtractsWhicheverPolicyIsApplied()
        {
            using (PnPContext context = await GetClassicContextAsync().ConfigureAwait(false))
            {
                var configuration = new ExtractConfiguration();
                configuration.Handlers.Add(ConfigurationHandler.SitePolicy);

                ProvisioningTemplate extracted = await context.GetProvisioningManager()
                    .GetTemplateAsync(configuration).ConfigureAwait(false);

                Console.WriteLine($"Applied site policy: {extracted.SitePolicy ?? "(none)"}");

                if (extracted.SitePolicy != null)
                {
                    Assert.AreNotEqual(string.Empty, extracted.SitePolicy,
                        "An empty site policy element was emitted.");
                }
            }
        }

        #endregion

        #region Image renditions

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ImageRenditions_SkipsQuietlyWhenPublishingIsOff()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string warning = null;

                var template = new ProvisioningTemplate
                {
                    Publishing = new PublishingModel
                    {
                        AutoCheckRequirements = AutoCheckRequirementsOptions.SkipIfNotCompliant,
                    },
                };

                template.Publishing.ImageRenditions.Add(new ImageRenditionModel
                {
                    Name = $"{TestPrefix}Rendition",
                    Width = 400,
                    Height = 300,
                });

                var configuration = new ApplyConfiguration
                {
                    MessagesDelegate = (message, type) =>
                    {
                        Console.WriteLine($"[{type}] {message}");

                        if (type == ProvisioningMessageType.Warning)
                        {
                            warning = message;
                        }
                    },
                };

                await context.GetProvisioningManager().ApplyTemplateAsync(template, configuration).ConfigureAwait(false);

                Assert.IsNotNull(warning, "The handler skipped the renditions without saying so.");
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ImageRenditions_RequiredButUnavailableFailsLoudly()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                if (!await IsNoScriptAsync(context).ConfigureAwait(false)
                    && await IsPublishingActiveAsync(context).ConfigureAwait(false))
                {
                    Assert.Inconclusive("This site supports publishing, so the unavailable path cannot be exercised on it.");
                }

                var template = new ProvisioningTemplate
                {
                    Publishing = new PublishingModel
                    {
                        AutoCheckRequirements = AutoCheckRequirementsOptions.FailIfNotCompliant,
                    },
                };

                template.Publishing.ImageRenditions.Add(new ImageRenditionModel
                {
                    Name = $"{TestPrefix}Rendition",
                    Width = 400,
                    Height = 300,
                });

                bool threw = false;
                string lastWarning = null;

                try
                {
                    await context.GetProvisioningManager().ApplyTemplateAsync(template, new ApplyConfiguration
                    {
                        MessagesDelegate = (message, type) =>
                        {
                            Console.WriteLine($"[{type}] {message}");
                            if (type == ProvisioningMessageType.Warning)
                            {
                                lastWarning = message;
                            }
                        },
                    }).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    threw = true;
                    Console.WriteLine($"Threw, as the template required: {ex.Message}");
                }

                Assert.IsTrue(threw || lastWarning != null,
                    "The renditions were neither applied, refused, nor reported - the template's requirement was ignored.");
            }
        }

        #endregion

        #region Helpers

        private static ApplyConfiguration Reporting()
        {
            return new ApplyConfiguration
            {
                MessagesDelegate = (message, type) =>
                {
                    if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                    {
                        Console.WriteLine($"[{type}] {message}");
                    }
                },
            };
        }

        private static async Task<bool> IsPublishingActiveAsync(PnPContext context)
        {
            try
            {
                await context.Web.LoadAsync(w => w.Features.QueryProperties(f => f.DefinitionId)).ConfigureAwait(false);

                return context.Web.Features.AsRequested()
                    .Any(f => f.DefinitionId == new Guid("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb"));
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Puts the site's audit settings back the way the test found them.
        /// </summary>
        private static async Task RestoreAuditAsync(int retention, bool trim)
        {
            try
            {
                using (PnPContext context = await GetClassicContextAsync(2).ConfigureAwait(false))
                {
                    await context.Site.LoadAsync(s => s.AuditLogTrimmingRetention, s => s.TrimAuditLog)
                        .ConfigureAwait(false);

                    context.Site.TrimAuditLog = trim;
                    context.Site.AuditLogTrimmingRetention = retention;

                    await context.Site.UpdateAsync().ConfigureAwait(false);
                    Console.WriteLine($"Restored audit settings to retention {retention}, trim {trim}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT RESTORE the audit settings: {Describe(ex)}");
            }
        }

        #endregion
    }
}
