using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Provisioning.Connectors;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IODirectory = System.IO.Directory;
using IOFile = System.IO.File;
using SiteDesignModel = PnP.Core.Provisioning.Model.SiteDesign;
using SiteScriptModel = PnP.Core.Provisioning.Model.SiteScript;
using StorageEntityModel = PnP.Core.Provisioning.Model.StorageEntity;
using ThemeModel = PnP.Core.Provisioning.Model.Theme;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Live coverage for <c>ObjectTenant</c>
    /// </summary>
    [TestClass]
    public class ObjectTenantLiveTests : LiveTestBase
    {
        private const string ScriptJson = "{\"$schema\":\"schema.json\",\"actions\":[" +
            "{\"verb\":\"applyTheme\",\"themeName\":\"Blue\"}],\"bindata\":{},\"version\":1}";

        #region Site scripts and designs

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        [Timeout(10 * 60 * 1000)]
        public async Task SiteScriptsAndDesigns_AreCreatedAndPublishTheirIdsAsTokens()
        {
            string fixture = Guid.NewGuid().ToString("N").Substring(0, 8);
            string scriptTitle = $"{TestPrefix}Script_{fixture}";
            string designTitle = $"{TestPrefix}Design_{fixture}";

            string folder = Path.Combine(Path.GetTempPath(), $"{TestPrefix}{fixture}");
            IODirectory.CreateDirectory(folder);
            IOFile.WriteAllText(Path.Combine(folder, "script.json"), ScriptJson);

            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                PnPContext admin;

                try
                {
                    admin = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("The tenant admin site", ex);
                    return;
                }

                using (admin)
                {
                    await CleanUpLeakedScriptsAndDesignsAsync(admin).ConfigureAwait(false);

                    try
                    {
                        var template = new ProvisioningTemplate
                        {
                            Connector = new FileSystemConnector(folder, string.Empty),
                        };

                        template.Tenant.SiteScripts.Add(new SiteScriptModel
                        {
                            Title = scriptTitle,
                            Description = "Created by the PnP Core provisioning tests",
                            JsonFilePath = "script.json",
                            Overwrite = true,
                        });

                        var design = new SiteDesignModel
                        {
                            Title = designTitle,
                            Description = "Created by the PnP Core provisioning tests",
                            WebTemplate = SiteDesignWebTemplate.TeamSite,

                            SiteScripts = new List<string> { $"{{sitescriptid:{scriptTitle}}}" },
                        };

                        template.Tenant.SiteDesigns.Add(design);

                        await ApplyAsync(context, template).ConfigureAwait(false);

                        List<SiteScriptUtility.SiteScriptMetadata> scripts =
                            await SiteScriptUtility.GetSiteScriptsAsync(admin).ConfigureAwait(false);

                        SiteScriptUtility.SiteScriptMetadata created =
                            scripts.SingleOrDefault(s => s.Title == scriptTitle);

                        Assert.IsNotNull(created, $"The site script '{scriptTitle}' was not created.");
                        Console.WriteLine($"Site script {created.Id}");

                        List<SiteScriptUtility.SiteDesignMetadata> designs =
                            await SiteScriptUtility.GetSiteDesignsAsync(admin).ConfigureAwait(false);

                        SiteScriptUtility.SiteDesignMetadata createdDesign =
                            designs.SingleOrDefault(d => d.Title == designTitle);

                        Assert.IsNotNull(createdDesign, $"The site design '{designTitle}' was not created.");
                        Console.WriteLine($"Site design {createdDesign.Id}, scripts: " +
                            string.Join(", ", createdDesign.SiteScriptIds));

                        CollectionAssert.Contains(createdDesign.SiteScriptIds, created.Id,
                            "The site design does not reference the script the template named, so the " +
                            "{sitescriptid:...} token did not resolve to the new script's id.");

                        Assert.AreEqual("64", createdDesign.WebTemplate,
                            "The design was not registered against the team site template.");
                    }
                    finally
                    {
                        await DeleteDesignAsync(admin, designTitle).ConfigureAwait(false);
                        await DeleteScriptAsync(admin, scriptTitle).ConfigureAwait(false);

                        TryDeleteFolder(folder);
                    }
                }
            }
        }

        #endregion

        #region Storage entities

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        [Timeout(10 * 60 * 1000)]
        public async Task StorageEntities_AreWrittenToTheAppCatalogAndReadBack()
        {
            string key = $"{TestPrefix}Entity_{Guid.NewGuid():N}";

            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                Uri catalogUri;

                try
                {
                    catalogUri = await context.GetTenantAppManager().GetTenantAppCatalogUriAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("The tenant app catalog", ex);
                    return;
                }

                if (catalogUri == null)
                {
                    Assert.Inconclusive("This tenant has no app catalog, so storage entities cannot be tested.");
                }

                Console.WriteLine($"App catalog: {catalogUri}");

                try
                {
                    var template = new ProvisioningTemplate();

                    template.Tenant.StorageEntities.Add(new StorageEntityModel
                    {
                        Key = key,
                        Value = "provisioned",
                        Description = "Created by the PnP Core provisioning tests",
                        Comment = "Safe to delete",
                    });

                    await ApplyAsync(context, template, catalogUri).ConfigureAwait(false);
                    using (PnPContext seed = await GetContextAsync(1).ConfigureAwait(false))
                    using (PnPContext catalog = await seed.CloneAsync(catalogUri).ConfigureAwait(false))
                    {
                        string value = await StorageEntities.GetAsync(catalog, key).ConfigureAwait(false);

                        Assert.AreEqual("provisioned", value,
                            "The storage entity was not written to the tenant app catalog.");
                    }
                }
                finally
                {
                    await DeleteStorageEntityAsync(catalogUri, key).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Themes

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        [Timeout(10 * 60 * 1000)]
        public async Task Themes_AreAddedToTheTenantThemeList()
        {
            string name = $"{TestPrefix}Theme_{Guid.NewGuid():N}";

            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                PnPContext admin;

                try
                {
                    admin = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("The tenant admin site", ex);
                    return;
                }

                using (admin)
                {
                    try
                    {
                        var template = new ProvisioningTemplate();

                        template.Tenant.Themes.Add(new ThemeModel
                        {
                            Name = name,
                            IsInverted = false,
                            Overwrite = true,
                            Palette = "{\"themePrimary\":\"#0078d4\",\"themeLighterAlt\":\"#eff6fc\"," +
                                "\"themeDark\":\"#005a9e\",\"neutralPrimary\":\"#333333\"," +
                                "\"white\":\"#ffffff\"}",
                        });

                        await ApplyAsync(context, template).ConfigureAwait(false);

                        HashSet<string> names = await TenantThemes.GetNamesAsync(admin).ConfigureAwait(false);

                        Assert.IsTrue(names.Contains(name),
                            $"The theme '{name}' is not in the tenant's theme list after applying it.");
                    }
                    finally
                    {
                        try
                        {
                            await TenantThemes.DeleteAsync(admin, name).ConfigureAwait(false);
                            Console.WriteLine($"Deleted theme {name}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"COULD NOT DELETE THEME {name}: {Describe(ex)}");
                        }
                    }
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Applies a template and fails on anything it reported.
        /// </summary>
        /// <param name="context">The context to apply against</param>
        /// <param name="template">The template to apply</param>
        /// <param name="appCatalog">
        /// When given, an access-denied is reported as a tenant configuration rather than a failure -
        /// see the remarks.
        /// </param>
        private static async Task ApplyAsync(PnPContext context, ProvisioningTemplate template, Uri appCatalog = null)
        {
            var problems = new List<string>();

            await context.GetProvisioningManager().ApplyTemplateAsync(template, new ApplyConfiguration
            {
                MessagesDelegate = (message, type) =>
                {
                    Console.WriteLine($"[{type}] {message}");

                    if (type == ProvisioningMessageType.Warning || type == ProvisioningMessageType.Error)
                    {
                        problems.Add(message);
                    }
                },
            }).ConfigureAwait(false);

            if (appCatalog != null
                && problems.Any(p => p.IndexOf("Access is denied", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                Assert.Inconclusive(
                    $"SharePoint refused the write to the app catalog at {appCatalog}. A storage entity " +
                    "is a property bag entry there, and the app catalog is a NoScript site, so this is a " +
                    "tenant configuration rather than a defect. Either allow scripting on the app " +
                    "catalog, or turn on the tenant setting " +
                    $"AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled." +
                    $"{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }

            Assert.AreEqual(0, problems.Count,
                $"The apply reported problems:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }

        /// <summary>
        /// Removes every site script and site design this suite has left behind.
        /// </summary>
        private static async Task CleanUpLeakedScriptsAndDesignsAsync(PnPContext admin)
        {
            try
            {
                List<SiteScriptUtility.SiteDesignMetadata> designs =
                    await SiteScriptUtility.GetSiteDesignsAsync(admin).ConfigureAwait(false);

                foreach (SiteScriptUtility.SiteDesignMetadata design in designs
                    .Where(d => d.Title != null && d.Title.StartsWith(TestPrefix, StringComparison.Ordinal)))
                {
                    await SiteScriptUtility.DeleteSiteDesignAsync(admin, design.Id).ConfigureAwait(false);
                    Console.WriteLine($"Swept leaked site design '{design.Title}' ({design.Id})");
                }

                List<SiteScriptUtility.SiteScriptMetadata> scripts =
                    await SiteScriptUtility.GetSiteScriptsAsync(admin).ConfigureAwait(false);

                foreach (SiteScriptUtility.SiteScriptMetadata script in scripts
                    .Where(s => s.Title != null && s.Title.StartsWith(TestPrefix, StringComparison.Ordinal)))
                {
                    await SiteScriptUtility.DeleteSiteScriptAsync(admin, script.Id).ConfigureAwait(false);
                    Console.WriteLine($"Swept leaked site script '{script.Title}' ({script.Id})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not sweep leaked site scripts and designs: {Describe(ex)}");
            }
        }

        /// <summary>
        /// Removes a site design by title, tolerating its absence.
        /// </summary>
        private static async Task DeleteDesignAsync(PnPContext admin, string title)
        {
            try
            {
                List<SiteScriptUtility.SiteDesignMetadata> designs =
                    await SiteScriptUtility.GetSiteDesignsAsync(admin).ConfigureAwait(false);

                foreach (SiteScriptUtility.SiteDesignMetadata design in designs.Where(d => d.Title == title))
                {
                    await SiteScriptUtility.DeleteSiteDesignAsync(admin, design.Id).ConfigureAwait(false);
                    Console.WriteLine($"Deleted site design {design.Id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE SITE DESIGN '{title}': {Describe(ex)}");
            }
        }

        private static async Task DeleteScriptAsync(PnPContext admin, string title)
        {
            try
            {
                List<SiteScriptUtility.SiteScriptMetadata> scripts =
                    await SiteScriptUtility.GetSiteScriptsAsync(admin).ConfigureAwait(false);

                foreach (SiteScriptUtility.SiteScriptMetadata script in scripts.Where(s => s.Title == title))
                {
                    await SiteScriptUtility.DeleteSiteScriptAsync(admin, script.Id).ConfigureAwait(false);
                    Console.WriteLine($"Deleted site script {script.Id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE SITE SCRIPT '{title}': {Describe(ex)}");
            }
        }

        private static async Task DeleteStorageEntityAsync(Uri catalogUri, string key)
        {
            try
            {
                using (PnPContext seed = await GetContextAsync(2).ConfigureAwait(false))
                using (PnPContext catalog = await seed.CloneAsync(catalogUri).ConfigureAwait(false))
                {
                    await StorageEntities.RemoveAsync(catalog, key).ConfigureAwait(false);
                    Console.WriteLine($"Removed storage entity {key}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT REMOVE STORAGE ENTITY '{key}': {Describe(ex)}");
            }
        }

        private static void TryDeleteFolder(string folder)
        {
            try
            {
                IODirectory.Delete(folder, recursive: true);
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
