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
using PageModel = PnP.Core.Provisioning.Model.Page;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Live coverage for <c>ObjectPages</c>
    /// </summary>
    [TestClass]
    public class ObjectPagesLiveTests : LiveTestBase
    {
        private static string PageName => $"{TestPrefix}Wiki.aspx";

        private static string PageUrl => $"SitePages/{PageName}";

        #region Apply

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Pages_CreatesAWikiPageWithItsLayout()
        {
            using (PnPContext context = await GetClassicContextAsync().ConfigureAwait(false))
            {
                if (await IsNoScriptAsync(context).ConfigureAwait(false))
                {
                    Assert.Inconclusive("The classic test site is NoScript, so wiki pages cannot be created on it.");
                }

                try
                {
                    var template = new ProvisioningTemplate();
                    template.Pages.Add(new PageModel
                    {
                        Url = PageUrl,
                        Overwrite = true,
                        Layout = WikiPageLayout.TwoColumnsHeader,
                    });

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, Reporting()).ConfigureAwait(false);

                    using (PnPContext fresh = await GetClassicContextAsync(1).ConfigureAwait(false))
                    {
                        IListItem item = await ReadPageItemAsync(fresh).ConfigureAwait(false);
                        Assert.IsNotNull(item, "The wiki page was not created.");

                        string wikiField = item.Values.TryGetValue("WikiField", out object value)
                            ? value?.ToString()
                            : null;

                        Console.WriteLine($"WikiField length: {wikiField?.Length ?? 0}");

                        Assert.IsFalse(string.IsNullOrEmpty(wikiField),
                            "The page exists but its wiki field is empty - it would open with no editable zones.");

                        StringAssert.Contains(wikiField, "true,false,2",
                            "The page did not get the two-column-with-header layout.");
                    }
                }
                finally
                {
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Pages_OverwriteReplacesTheLayoutAndDoesNotDuplicate()
        {
            using (PnPContext context = await GetClassicContextAsync().ConfigureAwait(false))
            {
                if (await IsNoScriptAsync(context).ConfigureAwait(false))
                {
                    Assert.Inconclusive("The classic test site is NoScript, so wiki pages cannot be created on it.");
                }

                try
                {
                    IProvisioningManager manager = context.GetProvisioningManager();

                    await manager.ApplyTemplateAsync(BuildTemplate(WikiPageLayout.OneColumn), Reporting())
                        .ConfigureAwait(false);

                    await manager.ApplyTemplateAsync(BuildTemplate(WikiPageLayout.ThreeColumns), Reporting())
                        .ConfigureAwait(false);

                    using (PnPContext fresh = await GetClassicContextAsync(1).ConfigureAwait(false))
                    {
                        int count = await CountPagesAsync(fresh).ConfigureAwait(false);
                        Console.WriteLine($"{count} page(s) named '{PageName}'");

                        Assert.AreEqual(1, count, "The second apply created a second page instead of replacing.");

                        IListItem item = await ReadPageItemAsync(fresh).ConfigureAwait(false);
                        string wikiField = item.Values["WikiField"]?.ToString();

                        StringAssert.Contains(wikiField, "false,false,3",
                            "The page was not replaced with the second template's layout.");
                    }
                }
                finally
                {
                    await SweepAsync().ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task Pages_AreSkippedOnANoScriptSite()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                if (!await IsNoScriptAsync(context).ConfigureAwait(false))
                {
                    Assert.Inconclusive("This site is not NoScript, so the refusal path cannot be exercised on it.");
                }

                string warning = null;

                await context.GetProvisioningManager().ApplyTemplateAsync(
                    BuildTemplate(WikiPageLayout.OneColumn),
                    new ApplyConfiguration
                    {
                        MessagesDelegate = (message, type) =>
                        {
                            Console.WriteLine($"[{type}] {message}");
                            if (type == ProvisioningMessageType.Warning)
                            {
                                warning = message;
                            }
                        },
                    }).ConfigureAwait(false);

                Assert.IsNotNull(warning, "Classic pages on a NoScript site were skipped without saying so.");

                StringAssert.Contains(warning, "ClientSidePages",
                    "The warning does not point at the modern page element.");
            }
        }

        #endregion

        #region Helpers

        private static ProvisioningTemplate BuildTemplate(WikiPageLayout layout)
        {
            var template = new ProvisioningTemplate();

            template.Pages.Add(new PageModel
            {
                Url = PageUrl,
                Overwrite = true,
                Layout = layout,
            });

            return template;
        }

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

        private static async Task<IListItem> ReadPageItemAsync(PnPContext context)
        {
            try
            {
                IFile file = await context.Web.GetFileByServerRelativeUrlAsync(
                    await PageServerRelativeUrlAsync(context).ConfigureAwait(false),
                    f => f.ListItemAllFields).ConfigureAwait(false);

                return file.ListItemAllFields;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not read the page item: {Describe(ex)}");
                return null;
            }
        }

        private static async Task<string> PageServerRelativeUrlAsync(PnPContext context)
        {
            await context.Web.LoadAsync(w => w.ServerRelativeUrl).ConfigureAwait(false);
            return $"{context.Web.ServerRelativeUrl.TrimEnd('/')}/{PageUrl}";
        }

        private static async Task<int> CountPagesAsync(PnPContext context)
        {
            await context.Web.LoadAsync(w => w.Lists.QueryProperties(l => l.Id, l => l.Title,
                l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl))).ConfigureAwait(false);

            IList pages = context.Web.Lists.AsRequested()
                .FirstOrDefault(l => l.RootFolder.ServerRelativeUrl.EndsWith("/SitePages", StringComparison.OrdinalIgnoreCase));

            if (pages == null)
            {
                return 0;
            }

            IFolder folder = await context.Web.GetFolderByServerRelativeUrlAsync(pages.RootFolder.ServerRelativeUrl,
                f => f.Files.QueryProperties(file => file.Name)).ConfigureAwait(false);

            return folder.Files.AsRequested().Count(f => string.Equals(f.Name, PageName, StringComparison.OrdinalIgnoreCase));
        }

        private static async Task SweepAsync()
        {
            try
            {
                using (PnPContext context = await GetClassicContextAsync(2).ConfigureAwait(false))
                {
                    string url = await PageServerRelativeUrlAsync(context).ConfigureAwait(false);

                    IFile file = await context.Web.GetFileByServerRelativeUrlAsync(url, f => f.UniqueId)
                        .ConfigureAwait(false);

                    await file.DeleteAsync().ConfigureAwait(false);
                    Console.WriteLine($"Deleted page '{PageName}'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Nothing to sweep, or could not delete '{PageName}': {ex.Message}");
            }
        }

        #endregion
    }
}
