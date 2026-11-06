using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
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
using System.Xml.Linq;
using PageLayoutModel = PnP.Core.Provisioning.Model.PageLayout;
using PublishingModel = PnP.Core.Provisioning.Model.Publishing;
using TimeZone = PnP.Core.Admin.Model.SharePoint.TimeZone;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    [TestClass]
    public class PublishingSiteLiveTests : LiveTestBase
    {
        private const string AvailableWebTemplatesKey = "__WebTemplates";
        private const string InheritWebTemplatesKey = "__InheritWebTemplates";
        private const string AvailablePageLayoutsKey = "__PageLayouts";
        private const string DefaultPageLayoutKey = "__DefaultPageLayout";

        /// <summary>SharePoint Server Publishing - web scoped.</summary>
        private static readonly Guid WebPublishingFeature = new Guid("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb");

        /// <summary>SharePoint Server Publishing Infrastructure - site collection scoped.</summary>
        private static readonly Guid SitePublishingFeature = new Guid("f6924d36-2fa8-4f0b-b16d-06b7250180fa");

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        [TestCategory("Publishing")]
        [Timeout(30 * 60 * 1000)]
        public async Task Publishing_AppliesAndReExtractsOnASiteItCreates()
        {
            Uri siteUrl = null;

            try
            {
                using (PnPContext seed = await GetContextAsync().ConfigureAwait(false))
                {
                    string owner = await SiteOwnerAsync(seed).ConfigureAwait(false);

                    siteUrl = new Uri($"https://{seed.Uri.DnsSafeHost}/sites/" +
                        $"pnpcoreprovisioningtestpub{Guid.NewGuid():N}");

                    Console.WriteLine($"Creating {siteUrl}");

                    using (PnPContext admin = await seed.GetSharePointAdmin()
                        .GetTenantAdminCenterContextAsync().ConfigureAwait(false))
                    {
                        var options = new ClassicSiteOptions(siteUrl, $"{TestPrefix}Publishing", "STS#0",
                            owner, Language.English, TimeZone.UTCPLUS0100_BRUSSELS_COPENHAGEN_MADRID_PARIS);

                        using (PnPContext created = await admin.GetSiteCollectionManager()
                            .CreateSiteCollectionAsync(options, CreationOptions(seed)).ConfigureAwait(false))
                        {
                            Console.WriteLine($"Created {created.Uri}");
                        }

                        await AllowScriptingAsync(admin, siteUrl).ConfigureAwait(false);
                    }
                }

                await RunAgainstTheNewSiteAsync(siteUrl).ConfigureAwait(false);
            }
            finally
            {
                await DeleteSiteAsync(siteUrl).ConfigureAwait(false);
            }
        }

        private async Task RunAgainstTheNewSiteAsync(Uri siteUrl)
        {
            using (PnPContext seed = await GetContextAsync(1).ConfigureAwait(false))
            using (PnPContext context = await seed.CloneAsync(siteUrl).ConfigureAwait(false))
            {
                Assert.IsFalse(await IsFeatureActiveAsync(context, WebPublishingFeature).ConfigureAwait(false),
                    "A brand new STS#0 site already had web publishing on - the activation path cannot be proven here.");

                var templatesTemplate = new ProvisioningTemplate
                {
                    Publishing = new PublishingModel
                    {
                        AutoCheckRequirements = AutoCheckRequirementsOptions.MakeCompliant,
                    },
                };

                templatesTemplate.Publishing.AvailableWebTemplates.Add(
                    new AvailableWebTemplate { LanguageCode = 1033, TemplateName = "STS#0" });
                templatesTemplate.Publishing.AvailableWebTemplates.Add(
                    new AvailableWebTemplate { LanguageCode = 1033, TemplateName = "BLANKINTERNET#0" });

                await ApplyAsync(context, templatesTemplate).ConfigureAwait(false);

                using (PnPContext after = await seed.CloneAsync(siteUrl).ConfigureAwait(false))
                {
                    Assert.IsTrue(await IsSiteFeatureActiveAsync(after, SitePublishingFeature).ConfigureAwait(false),
                        "The site scoped publishing feature was not activated.");
                    Assert.IsTrue(await IsFeatureActiveAsync(after, WebPublishingFeature).ConfigureAwait(false),
                        "The web scoped publishing feature was not activated.");
                }

                Dictionary<string, string> written = await ReadPropertiesAsync(siteUrl).ConfigureAwait(false);

                Console.WriteLine($"{AvailableWebTemplatesKey} = {written[AvailableWebTemplatesKey]}");
                Console.WriteLine($"{InheritWebTemplatesKey} = {written[InheritWebTemplatesKey]}");

                XElement webTemplates = XElement.Parse(written[AvailableWebTemplatesKey]);
                Assert.AreEqual("webtemplates", webTemplates.Name.LocalName);

                CollectionAssert.AreEquivalent(
                    new[] { "STS#0", "BLANKINTERNET#0" },
                    webTemplates.Elements("lcid").Where(e => (string)e.Attribute("id") == "1033")
                        .Elements("webtemplate").Select(e => (string)e.Attribute("name")).ToList(),
                    "The web templates SharePoint stored are not the ones the template asked for.");

                Assert.AreEqual("False", written[InheritWebTemplatesKey],
                    $"{InheritWebTemplatesKey} was not turned off, so the template list has no effect.");

                List<string> layoutNames = new List<string>();

                for (int attempt = 1; attempt <= 8; attempt++)
                {
                    using (PnPContext reader = await seed.CloneAsync(siteUrl).ConfigureAwait(false))
                    {
                        layoutNames = await ReadLayoutNamesAsync(reader).ConfigureAwait(false);
                    }

                    if (layoutNames.Count >= 2)
                    {
                        break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(15)).ConfigureAwait(false);
                }

                Console.WriteLine($"Gallery holds {layoutNames.Count} layouts, e.g. " +
                    string.Join(", ", layoutNames.Take(5)));

                if (layoutNames.Count < 2)
                {
                    Assert.Inconclusive("The master page gallery still held fewer than two page layouts " +
                        "two minutes after publishing was activated, so the layout path could not be " +
                        "exercised. The web templates half above did run.");
                }

                string first = layoutNames[0];
                string second = layoutNames[1];

                var layoutTemplate = new ProvisioningTemplate
                {
                    Publishing = new PublishingModel
                    {
                        AutoCheckRequirements = AutoCheckRequirementsOptions.MakeCompliant,
                    },
                };

                layoutTemplate.Publishing.PageLayouts.Add(new PageLayoutModel { Path = first });
                layoutTemplate.Publishing.PageLayouts.Add(new PageLayoutModel { Path = second, IsDefault = true });

                Console.WriteLine($"Asking for {layoutTemplate.Publishing.PageLayouts.Count} layout(s): " +
                    string.Join(", ", layoutTemplate.Publishing.PageLayouts.Select(l => $"{l.Path} (default={l.IsDefault})")));

                await ApplyAsync(context, layoutTemplate).ConfigureAwait(false);

                written = await ReadPropertiesAsync(siteUrl).ConfigureAwait(false);

                Console.WriteLine($"{AvailablePageLayoutsKey} = {written[AvailablePageLayoutsKey]}");
                Console.WriteLine($"{DefaultPageLayoutKey} = {written[DefaultPageLayoutKey]}");

                Assert.IsFalse(string.IsNullOrEmpty(written[AvailablePageLayoutsKey]),
                    $"'{AvailablePageLayoutsKey}' was not written. The apply reported no problem, so " +
                    "the handler either resolved no layouts or wrote them somewhere else.");

                XElement layouts = XElement.Parse(written[AvailablePageLayoutsKey]);
                Assert.AreEqual("pagelayouts", layouts.Name.LocalName);

                List<XElement> layoutElements = layouts.Elements("layout").ToList();
                Assert.AreEqual(2, layoutElements.Count, "Both layouts should have been resolved and written.");

                Dictionary<string, string> gallery = await ReadLayoutIdsAsync(context).ConfigureAwait(false);

                foreach (XElement layout in layoutElements)
                {
                    var url = (string)layout.Attribute("url");
                    var guid = (string)layout.Attribute("guid");

                    Assert.IsTrue(url.StartsWith("_catalogs/masterpage/", StringComparison.OrdinalIgnoreCase),
                        $"The layout url '{url}' is not site relative as the property bag expects.");

                    string name = url.Substring(url.LastIndexOf('/') + 1);

                    Assert.IsTrue(gallery.ContainsKey(name), $"'{name}' is not in the master page gallery.");
                    Assert.AreEqual(gallery[name], guid?.Trim('{', '}'),
                        $"The id written for '{name}' is not the one the gallery holds.");
                }

                Assert.AreEqual(second,
                    ((string)XElement.Parse(written[DefaultPageLayoutKey]).Attribute("url"))
                        .Split('/').Last(),
                    "The wrong layout was recorded as the default.");

                var configuration = new ExtractConfiguration();
                configuration.Handlers.Add(ConfigurationHandler.Publishing);

                ProvisioningTemplate extracted;

                using (PnPContext reader = await seed.CloneAsync(siteUrl).ConfigureAwait(false))
                {
                    extracted = await reader.GetProvisioningManager()
                        .GetTemplateAsync(configuration).ConfigureAwait(false);
                }

                Assert.IsNotNull(extracted.Publishing, "The extract produced no publishing element on a publishing site.");

                CollectionAssert.AreEquivalent(
                    new[] { "STS#0", "BLANKINTERNET#0" },
                    extracted.Publishing.AvailableWebTemplates.Select(t => t.TemplateName).ToList(),
                    "The extracted web templates do not match what was applied.");

                Assert.IsTrue(extracted.Publishing.AvailableWebTemplates.All(t => t.LanguageCode == 1033),
                    "The extracted web templates lost their language.");

                CollectionAssert.AreEquivalent(
                    new[] { first, second },
                    extracted.Publishing.PageLayouts.Select(l => l.Path).ToList(),
                    "The extracted page layout paths are not the bare names a template carries.");

                PageLayoutModel defaultLayout = extracted.Publishing.PageLayouts.SingleOrDefault(l => l.IsDefault);
                Assert.IsNotNull(defaultLayout, "No layout came back marked as the default.");
                Assert.AreEqual(second, defaultLayout.Path, "The wrong layout came back as the default.");
            }
        }

        #region Helpers

        /// <summary>
        /// Turns NoScript off on the site this test created.
        /// </summary>
        private static async Task AllowScriptingAsync(PnPContext admin, Uri siteUrl)
        {
            ISiteCollectionProperties properties = await admin.GetSiteCollectionManager()
                .GetSiteCollectionPropertiesAsync(siteUrl).ConfigureAwait(false);

            properties.DenyAddAndCustomizePages = DenyAddAndCustomizePagesStatus.Disabled;
            await properties.UpdateAsync().ConfigureAwait(false);

            Console.WriteLine("Set DenyAddAndCustomizePages = Disabled, waiting for it to take effect.");

            for (int attempt = 1; attempt <= 12; attempt++)
            {
                using (PnPContext seed = await GetContextAsync(4).ConfigureAwait(false))
                using (PnPContext site = await seed.CloneAsync(siteUrl).ConfigureAwait(false))
                {
                    if (!await site.Web.IsNoScriptSiteAsync().ConfigureAwait(false))
                    {
                        Console.WriteLine($"Scripting allowed after {attempt} check(s).");
                        return;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(15)).ConfigureAwait(false);
            }

            Assert.Inconclusive("The created site was still NoScript three minutes after scripting was " +
                "allowed on it, so the publishing write path could not be reached. This is a tenant " +
                "propagation delay rather than a defect in the handler - rerun the test.");
        }

        private static async Task ApplyAsync(PnPContext context, ProvisioningTemplate template)
        {
            var errors = new List<string>();

            await context.GetProvisioningManager().ApplyTemplateAsync(template, new ApplyConfiguration
            {
                MessagesDelegate = (message, type) =>
                {
                    Console.WriteLine($"[{type}] {message}");

                    if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                    {
                        errors.Add(message);
                    }
                },
            }).ConfigureAwait(false);

            Assert.AreEqual(0, errors.Count,
                $"The apply reported problems:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }

        /// <summary>
        /// Reads the four property bag keys through a <b>fresh</b> context.
        /// </summary>
        private static async Task<Dictionary<string, string>> ReadPropertiesAsync(Uri siteUrl)
        {
            using (PnPContext seed = await GetContextAsync(2).ConfigureAwait(false))
            using (PnPContext context = await seed.CloneAsync(siteUrl).ConfigureAwait(false))
            {
                await context.Web.LoadAsync(w => w.AllProperties).ConfigureAwait(false);

                var result = new Dictionary<string, string>();

                foreach (string key in new[]
                {
                    AvailableWebTemplatesKey, InheritWebTemplatesKey,
                    AvailablePageLayoutsKey, DefaultPageLayoutKey,
                })
                {
                    result[key] = context.Web.AllProperties.Values.TryGetValue(key, out object value)
                        ? value?.ToString()
                        : null;

                    Assert.IsFalse(string.IsNullOrEmpty(result[key]) && key == AvailableWebTemplatesKey,
                        $"'{key}' was not written at all.");
                }

                return result;
            }
        }

        /// <summary>
        /// The page layout file names in the master page gallery.
        /// </summary>
        private static async Task<List<string>> ReadLayoutNamesAsync(PnPContext context)
        {
            IFolder gallery = await GalleryFolderAsync(context).ConfigureAwait(false);

            return gallery.Files.AsRequested()
                .Select(f => f.Name)
                .Where(n => n.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static async Task<Dictionary<string, string>> ReadLayoutIdsAsync(PnPContext context)
        {
            IFolder gallery = await GalleryFolderAsync(context).ConfigureAwait(false);

            return gallery.Files.AsRequested()
                .GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().UniqueId.ToString(), StringComparer.OrdinalIgnoreCase);
        }

        private static async Task<IFolder> GalleryFolderAsync(PnPContext context)
        {
            await context.Web.LoadAsync(w => w.ServerRelativeUrl).ConfigureAwait(false);

            return await context.Web.GetFolderByServerRelativeUrlAsync(
                $"{context.Web.ServerRelativeUrl.TrimEnd('/')}/_catalogs/masterpage",
                f => f.Files.QueryProperties(file => file.Name, file => file.UniqueId)).ConfigureAwait(false);
        }

        private static async Task<bool> IsFeatureActiveAsync(PnPContext context, Guid feature)
        {
            await context.Web.LoadAsync(w => w.Features.QueryProperties(f => f.DefinitionId)).ConfigureAwait(false);
            return context.Web.Features.AsRequested().Any(f => f.DefinitionId == feature);
        }

        private static async Task<bool> IsSiteFeatureActiveAsync(PnPContext context, Guid feature)
        {
            await context.Site.LoadAsync(s => s.Features.QueryProperties(f => f.DefinitionId)).ConfigureAwait(false);
            return context.Site.Features.AsRequested().Any(f => f.DefinitionId == feature);
        }

        /// <summary>
        /// Deletes the site, recycle bin included.
        /// </summary>
        private static async Task DeleteSiteAsync(Uri siteUrl)
        {
            if (siteUrl == null)
            {
                return;
            }

            try
            {
                using (PnPContext context = await GetContextAsync(3).ConfigureAwait(false))
                {
                    await context.GetSiteCollectionManager()
                        .DeleteSiteCollectionAsync(siteUrl).ConfigureAwait(false);

                    Console.WriteLine($"Deleted {siteUrl}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE {siteUrl} - delete it by hand.{Environment.NewLine}{Describe(ex)}");
            }
        }

        #endregion
    }
}
