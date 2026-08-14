using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Provisioning.Providers;
using PnP.Core.Provisioning.Providers.Xml;
using PnP.Core.Provisioning.Test.Live;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeZone = PnP.Core.Admin.Model.SharePoint.TimeZone;

namespace PnP.Core.Provisioning.Test.Live.BaseTemplates
{
    /// <summary>
    /// One round trip per SharePoint site template: create a site of that template, extract a
    /// provisioning template from it, apply that to a second fresh site, and delete both.
    /// </summary>
    [TestClass]
    public class BaseTemplateRoundTripTests : LiveTestBase
    {
        /// <summary>
        /// Generous, because the work is two site collection creations plus a full extract and apply.
        /// </summary>
        private const int TimeoutMilliseconds = 60 * 60 * 1000;

        /// <summary>
        /// Every site collection these tests create is named with this, so a leaked one is obvious
        /// and can be swept.
        /// </summary>
        private const string SitePrefix = "/sites/pnpbase";

        /// <summary>
        /// Removes any site collection a previous run left behind.
        /// </summary>
        [ClassInitialize]
        public static async Task SweepLeakedSitesAsync(TestContext testContext)
        {
            _ = testContext;

            try
            {
                using (PnPContext seed = await GetContextAsync().ConfigureAwait(false))
                {
                    ISiteCollectionManager manager = seed.GetSiteCollectionManager();

                    List<ISiteCollection> leaked = (await manager.GetSiteCollectionsAsync().ConfigureAwait(false))
                        .Where(s => s.Url != null && s.Url.AbsolutePath.StartsWith(SitePrefix, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    foreach (ISiteCollection site in leaked)
                    {
                        Console.WriteLine($"Sweeping the leaked site {site.Url}");

                        try
                        {
                            await manager.DeleteSiteCollectionAsync(site.Url).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  could not delete it - do it by hand.{Environment.NewLine}{Describe(ex)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not sweep leaked sites.{Environment.NewLine}{Describe(ex)}");
            }
        }

        #region One test per template

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("BaseTemplate")]
        [Timeout(TimeoutMilliseconds)]
        public async Task RoundTrip_STS0_TeamSiteClassic()
        {
            await RunRoundTripAsync("STS#0").ConfigureAwait(false);
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("BaseTemplate")]
        [Timeout(TimeoutMilliseconds)]
        public async Task RoundTrip_BDR0_DocumentCenter()
        {
            await RunRoundTripAsync("BDR#0").ConfigureAwait(false);
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("BaseTemplate")]
        [Timeout(TimeoutMilliseconds)]
        public async Task RoundTrip_DEV0_DeveloperSite()
        {
            await RunRoundTripAsync("DEV#0").ConfigureAwait(false);
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("BaseTemplate")]
        [Timeout(TimeoutMilliseconds)]
        public async Task RoundTrip_OFFILE1_RecordsCenter()
        {
            await RunRoundTripAsync("OFFILE#1").ConfigureAwait(false);
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("BaseTemplate")]
        [Timeout(TimeoutMilliseconds)]
        public async Task RoundTrip_SITEPAGEPUBLISHING0_CommunicationSite()
        {
            await RunRoundTripAsync("SITEPAGEPUBLISHING#0").ConfigureAwait(false);
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("BaseTemplate")]
        [Timeout(TimeoutMilliseconds)]
        public async Task RoundTrip_BLANKINTERNETCONTAINER0_PublishingPortal()
        {
            await RunRoundTripAsync("BLANKINTERNETCONTAINER#0").ConfigureAwait(false);
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("BaseTemplate")]
        [Timeout(TimeoutMilliseconds)]
        public async Task RoundTrip_STS3_TeamSiteNoGroup()
        {
            await RunRoundTripAsync("STS#3").ConfigureAwait(false);
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("BaseTemplate")]
        [Timeout(TimeoutMilliseconds)]
        public async Task RoundTrip_SRCHCEN0_EnterpriseSearchCenter()
        {
            await RunRoundTripAsync("SRCHCEN#0").ConfigureAwait(false);
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("BaseTemplate")]
        [Timeout(TimeoutMilliseconds)]
        public async Task RoundTrip_ENTERWIKI0_EnterpriseWiki()
        {
            await RunRoundTripAsync("ENTERWIKI#0").ConfigureAwait(false);
        }

        #endregion

        #region The round trip

        /// <summary>
        /// Creates a source site, extracts it, applies the result to a fresh target, deletes both.
        /// </summary>
        private async Task RunRoundTripAsync(string webTemplate)
        {
            string fixture = Guid.NewGuid().ToString("N").Substring(0, 10);
            string slug = SlugOf(webTemplate);

            Uri sourceUrl = null;
            Uri targetUrl = null;

            using (PnPContext seed = await GetContextAsync().ConfigureAwait(false))
            {
                PnPContext admin;

                try
                {
                    admin = await seed.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("The tenant admin site", ex);
                    return;
                }

                using (admin)
                {
                    string owner = (await seed.Web.GetCurrentUserAsync().ConfigureAwait(false)).LoginName;
                    string host = seed.Uri.DnsSafeHost;

                    sourceUrl = new Uri($"https://{host}/sites/pnpbase{slug}src{fixture}");
                    targetUrl = new Uri($"https://{host}/sites/pnpbase{slug}tgt{fixture}");

                    try
                    {
                        if (!await TryCreateSiteAsync(admin, sourceUrl, webTemplate, owner, "source").ConfigureAwait(false))
                        {
                            return;
                        }

                        await AllowScriptingAsync(admin, sourceUrl).ConfigureAwait(false);

                        ProvisioningTemplate extracted = await ExtractAsync(seed, sourceUrl, webTemplate)
                            .ConfigureAwait(false);

                        AssertSchemaValid(extracted, webTemplate);
                        SaveExtractedTemplate(extracted, webTemplate);

                        if (!await TryCreateSiteAsync(admin, targetUrl, TargetTemplateFor(webTemplate), owner, "target")
                            .ConfigureAwait(false))
                        {
                            return;
                        }

                        if (!await AllowScriptingAsync(admin, targetUrl).ConfigureAwait(false))
                        {
                            Assert.Inconclusive(
                                $"Scripting could not be enabled on {targetUrl} in time, so applying " +
                                $"the {webTemplate} template there would only have measured NoScript.");
                        }

                        await ApplyAsync(seed, targetUrl, extracted, webTemplate).ConfigureAwait(false);
                    }
                    finally
                    {
                        await DeleteSiteAsync(targetUrl).ConfigureAwait(false);
                        await DeleteSiteAsync(sourceUrl).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// The template the target site is created from.
        /// </summary>
        private static string TargetTemplateFor(string webTemplate) => webTemplate;

        #endregion

        #region Steps

        /// <summary>
        /// Creates one site, or reports the template as unavailable on this tenant.
        /// </summary>
        /// <returns>Whether the site was created.</returns>
        private static async Task<bool> TryCreateSiteAsync(PnPContext admin, Uri url, string webTemplate,
            string owner, string role)
        {
            Console.WriteLine($"Creating the {role} site {url} from {webTemplate}");

            var options = new ClassicSiteOptions(url, $"{TestPrefix}{webTemplate}", webTemplate, owner,
                Language.English, TimeZone.UTCPLUS0100_BRUSSELS_COPENHAGEN_MADRID_PARIS);

            ISiteCollectionManager manager = admin.GetSiteCollectionManager();
            Exception failure = null;

            for (int attempt = 1; attempt <= 2; attempt++)
            {
                if (attempt > 1 && await SiteExistsQuietlyAsync(manager, url).ConfigureAwait(false))
                {
                    Console.WriteLine($"  the first attempt created {url} after all.");
                    return true;
                }

                try
                {
                    using (PnPContext created = await manager
                        .CreateSiteCollectionAsync(options, new SiteCreationOptions
                        {
                            UsingApplicationPermissions = false,
                            WaitForAsyncProvisioning = true,
                        }).ConfigureAwait(false))
                    {
                        Console.WriteLine($"  created {created.Uri}");
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    failure = ex;

                    if (SaysAlreadyExists(ex))
                    {
                        Console.WriteLine($"  {url} was created by an earlier attempt, waiting for it.");

                        if (await WaitForSiteAsync(manager, url).ConfigureAwait(false))
                        {
                            return true;
                        }
                    }
                    else if (attempt < 2)
                    {
                        Console.WriteLine("  creation was refused, waiting a minute and trying once more.");
                        await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                    }
                }
            }

            if (await WaitForSiteAsync(manager, url).ConfigureAwait(false))
            {
                Console.WriteLine($"  {url} exists despite the error, carrying on.");
                return true;
            }

            bool ambiguous403 = failure is ServiceException service
                && service.Error is ServiceError serviceError
                && serviceError.HttpResponseCode == 403;

            string reason;

            if (string.Equals(role, "target", StringComparison.Ordinal))
            {
                reason = $"A source site was already created from '{webTemplate}' in this same test, so the " +
                    $"template is provisionable here and this is not a statement about it. Throttling of back " +
                    $"to back site collection creation is the usual cause; it survived one retry a minute apart.";
            }
            else if (ambiguous403)
            {
                reason = "SharePoint answered HTTP 403, which it returns both for a web template it will not " +
                    "provision and for site collection creation it considers too rapid - the error says which " +
                    "of those it is no more than this message can. Read it against the rest of the run: if " +
                    "other templates were created either side of this one, the template is the likely cause; " +
                    "if none were, the tenant is. It survived one retry a minute apart.";
            }
            else
            {
                reason = "The site was not created. Some of these web templates are no longer provisionable " +
                    "in SharePoint Online and some failures are transient, and this cannot tell which " +
                    "happened - the error below and the rest of the run are the evidence.";
            }

            Assert.Inconclusive(
                $"A {role} site could not be created from the '{webTemplate}' template, so the round " +
                $"trip could not run. {reason}{Environment.NewLine}{Environment.NewLine}{Describe(failure)}");

            return false;
        }

        /// <summary>
        /// Extracts a template from the source site.
        /// </summary>
        private static async Task<ProvisioningTemplate> ExtractAsync(PnPContext seed, Uri sourceUrl, string webTemplate)
        {
            using (PnPContext source = await seed.CloneAsync(sourceUrl).ConfigureAwait(false))
            {
                var warnings = new List<string>();

                var configuration = new ExtractConfiguration
                {
                    MessagesDelegate = (message, type) =>
                    {
                        if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                        {
                            warnings.Add($"[{type}] {message}");
                            Console.WriteLine($"  [{type}] {message}");
                        }
                    },
                };

                Console.WriteLine($"Extracting {webTemplate}...");

                ProvisioningTemplate extracted;

                try
                {
                    extracted = await source.GetProvisioningManager()
                        .GetTemplateAsync(configuration).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new AssertFailedException(
                        $"Extracting the {webTemplate} template from {sourceUrl} threw." +
                        $"{Environment.NewLine}{Describe(ex)}",
                        ex);
                }

                Console.WriteLine($"  extracted: {extracted.SiteFields.Count} site column(s), " +
                    $"{extracted.ContentTypes.Count} content type(s), {extracted.Lists.Count} list(s), " +
                    $"{extracted.ClientSidePages.Count} page(s), {warnings.Count} warning(s)");

                Assert.IsTrue(
                    extracted.Lists.Count > 0 || extracted.SiteFields.Count > 0 || extracted.ContentTypes.Count > 0,
                    $"Extracting a '{webTemplate}' site produced an empty template - no lists, columns " +
                    "or content types at all.");

                return extracted;
            }
        }

        /// <summary>
        /// Checks the extracted template is something a schema-aware tool would accept.
        /// </summary>
        /// <summary>
        /// Whether SharePoint's answer was that the site is already there.
        /// </summary>
        private static bool SaysAlreadyExists(Exception ex)
        {
            for (Exception current = ex; current != null; current = current.InnerException)
            {
                if (current.Message != null
                    && current.Message.IndexOf("already exists", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Waits for a site collection that is part way through provisioning to become visible.
        /// </summary>
        private static async Task<bool> WaitForSiteAsync(ISiteCollectionManager manager, Uri url)
        {
            for (int attempt = 1; attempt <= 20; attempt++)
            {
                if (await SiteExistsQuietlyAsync(manager, url).ConfigureAwait(false))
                {
                    Console.WriteLine($"  {url} is available after {attempt} check(s).");
                    return true;
                }

                await Task.Delay(TimeSpan.FromSeconds(15)).ConfigureAwait(false);
            }

            return false;
        }

        /// <summary>
        /// Whether the site collection exists, treating any failure to ask as "no".
        /// </summary>
        private static async Task<bool> SiteExistsQuietlyAsync(ISiteCollectionManager manager, Uri url)
        {
            try
            {
                return await manager.SiteExistsAsync(url).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Writes the extracted template to disk so a run leaves its evidence behind.
        /// </summary>
        private static void SaveExtractedTemplate(ProvisioningTemplate template, string webTemplate)
        {
            try
            {
                string directory = Path.Combine(Path.GetTempPath(), "pnp-basetemplate-extracts");
                System.IO.Directory.CreateDirectory(directory);

                string path = Path.Combine(directory, $"{SlugOf(webTemplate)}.xml");

                using (Stream serialized = XMLPnPSchemaFormatter.LatestFormatter.ToFormattedTemplate(template))
                using (FileStream file = System.IO.File.Create(path))
                {
                    serialized.CopyTo(file);
                }

                Console.WriteLine($"  extract saved to {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  the extract could not be saved: {ex.Message}");
            }
        }

        private static void AssertSchemaValid(ProvisioningTemplate template, string webTemplate)
        {
            var formatter = new XMLPnPSchemaFormatter();

            using (Stream serialized = XMLPnPSchemaFormatter.LatestFormatter.ToFormattedTemplate(template))
            using (var copy = new MemoryStream())
            {
                serialized.CopyTo(copy);
                copy.Position = 0;

                ValidationResult result = formatter.GetValidationResults(copy);

                if (result.IsValid)
                {
                    Console.WriteLine("  the extracted template validates against the schema.");
                    return;
                }

                string detail = string.Join(
                    Environment.NewLine + "  ",
                    result.Exceptions?.Select(e => e.Message) ?? Enumerable.Empty<string>());

                Assert.Fail($"The template extracted from a '{webTemplate}' site does not validate " +
                    $"against the provisioning schema:{Environment.NewLine}  {detail}");
            }
        }

        /// <summary>
        /// Applies the extracted template to the target site.
        /// </summary>
        private static async Task ApplyAsync(PnPContext seed, Uri targetUrl, ProvisioningTemplate template,
            string webTemplate)
        {
            using (PnPContext target = await seed.CloneAsync(targetUrl).ConfigureAwait(false))
            {
                var problems = new List<string>();

                var configuration = new ApplyConfiguration
                {
                    MessagesDelegate = (message, type) =>
                    {
                        if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                        {
                            problems.Add($"[{type}] {message}");
                            Console.WriteLine($"  [{type}] {message}");
                        }
                    },

                    ProgressDelegate = (step, current, total) =>
                        Console.WriteLine($"  {current}/{total}  {step}"),
                };

                Console.WriteLine($"Applying the {webTemplate} template to {targetUrl}...");

                try
                {
                    await target.GetProvisioningManager().ApplyTemplateAsync(template, configuration)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new AssertFailedException(
                        $"Applying the {webTemplate} template to {targetUrl} threw." +
                        $"{Environment.NewLine}{Describe(ex)}" +
                        $"{Environment.NewLine}{Environment.NewLine}Reported before it threw:" +
                        $"{Environment.NewLine}  {string.Join(Environment.NewLine + "  ", problems)}",
                        ex);
                }

                Console.WriteLine();
                Console.WriteLine(problems.Count == 0
                    ? $"'{webTemplate}' round tripped with nothing reported."
                    : $"'{webTemplate}' round tripped, with {problems.Count} thing(s) reported:");

                foreach (string problem in problems)
                {
                    Console.WriteLine($"  {problem}");
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Deletes a site, recycle bin included, without ever throwing.
        /// </summary>
        private static async Task DeleteSiteAsync(Uri siteUrl)
        {
            if (siteUrl == null)
            {
                return;
            }

            try
            {
                using (PnPContext context = await GetContextAsync(5).ConfigureAwait(false))
                {
                    ISiteCollectionManager manager = context.GetSiteCollectionManager();

                    if (!await manager.SiteExistsAsync(siteUrl).ConfigureAwait(false))
                    {
                        return;
                    }

                    await manager.DeleteSiteCollectionAsync(siteUrl).ConfigureAwait(false);
                    Console.WriteLine($"Deleted {siteUrl}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE {siteUrl} - delete it by hand." +
                    $"{Environment.NewLine}{Describe(ex)}");
            }
        }

        /// <summary>
        /// A url-safe fragment for a web template name.
        /// </summary>
        private static string SlugOf(string webTemplate)
        {
            var builder = new StringBuilder();

            foreach (char c in webTemplate.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        #endregion
    }
}
