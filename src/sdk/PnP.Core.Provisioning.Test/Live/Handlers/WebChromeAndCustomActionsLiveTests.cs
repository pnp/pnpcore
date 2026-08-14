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
using CustomActionModel = PnP.Core.Provisioning.Model.CustomAction;
using FooterModel = PnP.Core.Provisioning.Model.SiteFooter;
using HeaderModel = PnP.Core.Provisioning.Model.SiteHeader;
using WebSettingsModel = PnP.Core.Provisioning.Model.WebSettings;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Live coverage for the second half of wave 1: the site header and footer, web settings,
    /// custom actions and the template-info pair.
    /// </summary>
    [TestClass]
    public class WebChromeAndCustomActionsLiveTests : LiveTestBase
    {
        #region ObjectSiteHeaderSettings

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task SiteHeader_ExtractReadsTheRealChromeOptions()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                ProvisioningTemplate template = await context.GetProvisioningManager()
                    .GetTemplateAsync(new ExtractConfiguration
                    {
                        Handlers = { ConfigurationHandler.SiteHeader },
                    }).ConfigureAwait(false);

                Assert.IsNotNull(template.Header, "The header handler produced nothing at all.");

                IChromeOptions chrome = await context.Web.GetBrandingManager().GetChromeOptionsAsync().ConfigureAwait(false);

                Console.WriteLine($"Layout       : {template.Header.Layout} (chrome: {chrome.Header.Layout})");
                Console.WriteLine($"Emphasis     : {template.Header.BackgroundEmphasis} (chrome: {chrome.Header.Emphasis})");
                Console.WriteLine($"MenuStyle    : {template.Header.MenuStyle} (megamenu: {chrome.Navigation.MegaMenuEnabled})");
                Console.WriteLine($"ShowSiteTitle: {template.Header.ShowSiteTitle} (hideTitle: {chrome.Header.HideTitle})");
                Console.WriteLine($"ShowSiteNav  : {template.Header.ShowSiteNavigation} (visible: {chrome.Navigation.Visible})");

                Assert.AreEqual(!chrome.Header.HideTitle, template.Header.ShowSiteTitle);
                Assert.AreEqual(chrome.Navigation.Visible, template.Header.ShowSiteNavigation);
                Assert.AreEqual(chrome.Navigation.MegaMenuEnabled,
                    template.Header.MenuStyle == SiteHeaderMenuStyle.MegaMenu);
                Assert.AreEqual(chrome.Header.Emphasis.ToString(), template.Header.BackgroundEmphasis.ToString());
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task SiteHeader_AppliesLayoutAndMenuStyleAndRestoresThem()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                IBrandingManager branding = context.Web.GetBrandingManager();
                IChromeOptions before = await branding.GetChromeOptionsAsync().ConfigureAwait(false);

                HeaderLayoutType originalLayout = before.Header.Layout;
                bool originalMegaMenu = before.Navigation.MegaMenuEnabled;
                bool originalHideTitle = before.Header.HideTitle;

                Console.WriteLine($"Before: layout={originalLayout}, megaMenu={originalMegaMenu}, hideTitle={originalHideTitle}");

                SiteHeaderLayout wantedLayout = originalLayout == HeaderLayoutType.Compact
                    ? SiteHeaderLayout.Standard
                    : SiteHeaderLayout.Compact;

                try
                {
                    var template = new ProvisioningTemplate
                    {
                        Header = new HeaderModel
                        {
                            Layout = wantedLayout,
                            MenuStyle = originalMegaMenu ? SiteHeaderMenuStyle.Cascading : SiteHeaderMenuStyle.MegaMenu,
                            BackgroundEmphasis = Emphasis.None,
                            ShowSiteTitle = originalHideTitle,
                            ShowSiteNavigation = true,
                        },
                    };

                    await context.GetProvisioningManager().ApplyTemplateAsync(template).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        IChromeOptions after = await fresh.Web.GetBrandingManager().GetChromeOptionsAsync().ConfigureAwait(false);

                        Console.WriteLine($"After : layout={after.Header.Layout}, megaMenu={after.Navigation.MegaMenuEnabled}, hideTitle={after.Header.HideTitle}");

                        Assert.AreEqual(wantedLayout.ToString(), after.Header.Layout.ToString(),
                            "The header layout was not applied.");
                        Assert.AreEqual(!originalMegaMenu, after.Navigation.MegaMenuEnabled,
                            "The menu style was not applied.");
                        Assert.AreEqual(!originalHideTitle, after.Header.HideTitle,
                            "ShowSiteTitle was not applied.");
                    }
                }
                finally
                {
                    try
                    {
                        IChromeOptions restore = await branding.GetChromeOptionsAsync().ConfigureAwait(false);
                        restore.Header.Layout = originalLayout;
                        restore.Navigation.MegaMenuEnabled = originalMegaMenu;
                        restore.Header.HideTitle = originalHideTitle;
                        await branding.SetChromeOptionsAsync(restore).ConfigureAwait(false);
                        Console.WriteLine("Restored the original header chrome.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"COULD NOT RESTORE the header chrome: {Describe(ex)}");
                    }
                }
            }
        }

        #endregion

        #region ObjectSiteFooterSettings

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task SiteFooter_RoundTripsLinksOnACommunicationSite()
        {
            using (PnPContext context = await GetNoGroupContextAsync().ConfigureAwait(false))
            {
                await context.Site.RootWeb.LoadAsync(w => w.WebTemplate).ConfigureAwait(false);
                if (!"SITEPAGEPUBLISHING".Equals(context.Site.RootWeb.WebTemplate, StringComparison.OrdinalIgnoreCase))
                {
                    Assert.Inconclusive(
                        $"The configured NoGroupTestSite is a '{context.Site.RootWeb.WebTemplate}' site. " +
                        "Only communication sites have a modern footer, so this cannot be verified here. " +
                        "Point NoGroupTestSite at a communication site to cover it.");
                }

                IProvisioningManager manager = context.GetProvisioningManager();

                ProvisioningTemplate before = await manager.GetTemplateAsync(new ExtractConfiguration
                {
                    Handlers = { ConfigurationHandler.SiteFooter },
                }).ConfigureAwait(false);

                Assert.IsNotNull(before.Footer, "The footer handler produced nothing on a communication site.");
                Console.WriteLine($"Before: enabled={before.Footer.Enabled}, layout={before.Footer.Layout}, links={before.Footer.FooterLinks.Count}");

                bool originalEnabled = before.Footer.Enabled;
                string linkName = $"{TestPrefix}FooterLink";
                int linksBefore = before.Footer.FooterLinks.Count;

                try
                {
                    var footer = new FooterModel
                    {
                        Enabled = true,
                        Layout = before.Footer.Layout,
                        BackgroundEmphasis = before.Footer.BackgroundEmphasis,
                        DisplayName = before.Footer.DisplayName,
                        Name = before.Footer.Name,
                        RemoveExistingNodes = true,
                    };
                    footer.FooterLinks.Add(new SiteFooterLink { DisplayName = linkName, Url = "https://aka.ms/pnp" });

                    await manager.ApplyTemplateAsync(new ProvisioningTemplate { Footer = footer }).ConfigureAwait(false);

                    using (PnPContext fresh = await GetNoGroupContextAsync(1).ConfigureAwait(false))
                    {
                        ProvisioningTemplate after = await fresh.GetProvisioningManager()
                            .GetTemplateAsync(new ExtractConfiguration
                            {
                                Handlers = { ConfigurationHandler.SiteFooter },
                            }).ConfigureAwait(false);

                        Console.WriteLine($"After : enabled={after.Footer.Enabled}, links={after.Footer.FooterLinks.Count}");
                        foreach (SiteFooterLink link in after.Footer.FooterLinks)
                        {
                            Console.WriteLine($"  {link.DisplayName} -> {link.Url}");
                        }

                        Assert.IsTrue(after.Footer.Enabled, "The footer should be enabled.");
                        Assert.IsTrue(after.Footer.FooterLinks.Any(l => l.DisplayName == linkName),
                            $"The link '{linkName}' was not written to the footer.");

                        Assert.AreEqual(1, after.Footer.FooterLinks.Count,
                            $"RemoveExistingNodes should leave exactly one link; found {after.Footer.FooterLinks.Count}. " +
                            "SaveMenuState merges - nodes must be marked IsDeleted, not omitted.");
                    }
                }
                finally
                {
                    try
                    {
                        var restore = new FooterModel
                        {
                            Enabled = originalEnabled,
                            Layout = before.Footer.Layout,
                            BackgroundEmphasis = before.Footer.BackgroundEmphasis,
                            DisplayName = before.Footer.DisplayName,
                            Name = before.Footer.Name,
                            Logo = before.Footer.Logo,
                            RemoveExistingNodes = true,
                        };

                        foreach (SiteFooterLink link in before.Footer.FooterLinks
                            .Where(l => l.DisplayName == null || !l.DisplayName.StartsWith(TestPrefix, StringComparison.Ordinal)))
                        {
                            restore.FooterLinks.Add(link);
                        }

                        await manager.ApplyTemplateAsync(new ProvisioningTemplate { Footer = restore }).ConfigureAwait(false);
                        Console.WriteLine($"Restored the footer with {restore.FooterLinks.Count} link(s) (was {linksBefore}).");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"COULD NOT RESTORE the footer: {Describe(ex)}");
                    }
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task SiteFooter_WarnsRatherThanFailingOnAGroupSite()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                await context.Site.RootWeb.LoadAsync(w => w.WebTemplate).ConfigureAwait(false);
                if ("SITEPAGEPUBLISHING".Equals(context.Site.RootWeb.WebTemplate, StringComparison.OrdinalIgnoreCase))
                {
                    Assert.Inconclusive("The default test site is a communication site, so this case cannot be exercised here.");
                }

                var warnings = new List<string>();
                var configuration = new ApplyConfiguration
                {
                    MessagesDelegate = (message, type) =>
                    {
                        if (type == ProvisioningMessageType.Warning) warnings.Add(message);
                    },
                };

                await context.GetProvisioningManager().ApplyTemplateAsync(
                    new ProvisioningTemplate { Footer = new FooterModel { Enabled = true } }, configuration).ConfigureAwait(false);

                Assert.IsTrue(warnings.Any(w => w.Contains("communication site", StringComparison.OrdinalIgnoreCase)),
                    "Applying a footer to a non-communication site must say it did nothing. " +
                    $"Warnings seen: {string.Join(" | ", warnings)}");

                Console.WriteLine($"Warning reported: {warnings.First(w => w.Contains("communication site", StringComparison.OrdinalIgnoreCase))}");
            }
        }

        #endregion

        #region ObjectWebSettings

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task WebSettings_ExtractReadsValuesOnlyAWorkingReadCanProduce()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                ProvisioningTemplate template = await context.GetProvisioningManager()
                    .GetTemplateAsync(new ExtractConfiguration
                    {
                        Handlers = { ConfigurationHandler.WebSettings },
                    }).ConfigureAwait(false);

                Assert.IsNotNull(template.WebSettings, "The web settings handler produced nothing at all.");

                WebSettingsModel settings = template.WebSettings;

                Console.WriteLine($"WelcomePage    : {settings.WelcomePage}");
                Console.WriteLine($"MasterPageUrl  : {settings.MasterPageUrl}");
                Console.WriteLine($"SiteLogo       : {settings.SiteLogo}");
                Console.WriteLine($"SearchScope    : {settings.SearchScope}");
                Console.WriteLine($"SearchBox      : {settings.SearchBoxInNavBar}");
                Console.WriteLine($"SearchCenterUrl: {settings.SearchCenterUrl ?? "<inherited>"}");
                Console.WriteLine($"HubSiteUrl     : {settings.HubSiteUrl ?? "<not joined>"}");
                Console.WriteLine($"NoCrawl        : {settings.NoCrawl}");
                Console.WriteLine($"MembersCanShare: {settings.MembersCanShare}");

                Assert.IsFalse(string.IsNullOrEmpty(settings.WelcomePage),
                    "WelcomePage came back empty. Every site has one, so the read is not working.");
                Assert.IsFalse(string.IsNullOrEmpty(settings.MasterPageUrl),
                    "MasterPageUrl came back empty. Every site has one, so the read is not working.");
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task WebSettings_AppliesAndRestoresTheFlagsItOwns()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                IWeb web = await context.Web.GetAsync(
                    w => w.CommentsOnSitePagesDisabled, w => w.ExcludeFromOfflineClient, w => w.NoCrawl).ConfigureAwait(false);

                bool originalComments = web.CommentsOnSitePagesDisabled;
                bool originalExclude = web.ExcludeFromOfflineClient;
                bool isNoScript = await IsNoScriptAsync(context).ConfigureAwait(false);

                Console.WriteLine($"Before: comments disabled={originalComments}, excludeFromOfflineClient={originalExclude}, noScript={isNoScript}");

                IProvisioningManager manager = context.GetProvisioningManager();

                try
                {
                    var template = new ProvisioningTemplate
                    {
                        WebSettings = new WebSettingsModel
                        {
                            CommentsOnSitePagesDisabled = !originalComments,
                            ExcludeFromOfflineClient = !originalExclude,
                            NoCrawl = web.NoCrawl,
                        },
                    };

                    await manager.ApplyTemplateAsync(template).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        IWeb after = await fresh.Web.GetAsync(
                            w => w.CommentsOnSitePagesDisabled, w => w.ExcludeFromOfflineClient).ConfigureAwait(false);

                        Console.WriteLine($"After : comments disabled={after.CommentsOnSitePagesDisabled}, excludeFromOfflineClient={after.ExcludeFromOfflineClient}");

                        Assert.AreEqual(!originalComments, after.CommentsOnSitePagesDisabled,
                            "CommentsOnSitePagesDisabled was not applied.");
                        Assert.AreEqual(!originalExclude, after.ExcludeFromOfflineClient,
                            "ExcludeFromOfflineClient was not applied.");
                    }
                }
                finally
                {
                    try
                    {
                        await manager.ApplyTemplateAsync(new ProvisioningTemplate
                        {
                            WebSettings = new WebSettingsModel
                            {
                                CommentsOnSitePagesDisabled = originalComments,
                                ExcludeFromOfflineClient = originalExclude,
                                NoCrawl = web.NoCrawl,
                            },
                        }).ConfigureAwait(false);
                        Console.WriteLine("Restored the original web settings.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"COULD NOT RESTORE the web settings: {Describe(ex)}");
                    }
                }
            }
        }

        #endregion

        #region ObjectCustomActions

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task CustomActions_AddUpdateAndRemoveAWebScopedAction()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string actionName = $"{TestPrefix}Action";

                var componentId = new Guid("d0454bb0-3b4d-4e6d-9b0e-2e7ff5b6b2ea");

                IProvisioningManager manager = context.GetProvisioningManager();

                try
                {
                    var template = new ProvisioningTemplate();
                    template.CustomActions.WebCustomActions.Add(new CustomActionModel
                    {
                        Name = actionName,
                        Title = actionName,
                        Description = "Created by the PnP Core provisioning tests",
                        Location = "ClientSideExtension.ApplicationCustomizer",
                        ClientSideComponentId = componentId,
                        ClientSideComponentProperties = "{\"testMessage\":\"one\"}",
                        Sequence = 100,
                        Enabled = true,
                    });

                    await manager.ApplyTemplateAsync(template).ConfigureAwait(false);

                    IUserCustomAction added = await FindActionAsync(actionName).ConfigureAwait(false);
                    Assert.IsNotNull(added, "The custom action was not created.");
                    Assert.AreEqual(componentId, added.ClientSideComponentId);
                    Console.WriteLine($"Added   : {added.Name} ({added.ClientSideComponentProperties})");

                    template.CustomActions.WebCustomActions[0].ClientSideComponentProperties = "{\"testMessage\":\"two\"}";
                    template.CustomActions.WebCustomActions[0].Sequence = 200;

                    await manager.ApplyTemplateAsync(template).ConfigureAwait(false);

                    IUserCustomAction updated = await FindActionAsync(actionName).ConfigureAwait(false);
                    Assert.IsNotNull(updated, "The custom action disappeared on the second apply.");
                    Assert.AreEqual(200, updated.Sequence, "The sequence was not updated.");
                    StringAssert.Contains(updated.ClientSideComponentProperties, "two",
                        "The client side component properties were not updated.");
                    Console.WriteLine($"Updated : {updated.Name} ({updated.ClientSideComponentProperties}, sequence {updated.Sequence})");

                    template.CustomActions.WebCustomActions[0].Remove = true;
                    await manager.ApplyTemplateAsync(template).ConfigureAwait(false);

                    Assert.IsNull(await FindActionAsync(actionName).ConfigureAwait(false),
                        "Remove=true did not delete the custom action.");
                    Console.WriteLine("Removed : the action is gone.");
                }
                finally
                {
                    try
                    {
                        IUserCustomAction leftOver = await FindActionAsync(actionName).ConfigureAwait(false);
                        if (leftOver != null)
                        {
                            await leftOver.DeleteAsync().ConfigureAwait(false);
                            Console.WriteLine("Swept up a leftover custom action.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"COULD NOT CLEAN UP the custom action: {Describe(ex)}");
                    }
                }
            }
        }

        private static async Task<IUserCustomAction> FindActionAsync(string name)
        {
            using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
            {
                await fresh.Web.LoadAsync(w => w.UserCustomActions).ConfigureAwait(false);

                return fresh.Web.UserCustomActions.AsRequested()
                    .FirstOrDefault(a => string.Equals(a.Name, name, StringComparison.Ordinal));
            }
        }

        #endregion

        #region ObjectPersistTemplateInfo / ObjectRetrieveTemplateInfo

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task TemplateInfo_IsWrittenOnApplyAndReadBackOnExtract()
        {
            using (PnPContext context = await GetClassicContextAsync().ConfigureAwait(false))
            {
                if (await IsNoScriptAsync(context).ConfigureAwait(false))
                {
                    Assert.Inconclusive(
                        "The configured classic test site is NoScript, so the template info is deliberately not " +
                        "recorded and this round trip cannot be verified. Disable NoScript on it to cover this.");
                }

                string templateId = $"{TestPrefix}{Guid.NewGuid():N}".ToUpperInvariant();

                IProvisioningManager manager = context.GetProvisioningManager();

                await manager.ApplyTemplateAsync(new ProvisioningTemplate
                {
                    Id = templateId,
                    Version = 7,
                }).ConfigureAwait(false);

                using (PnPContext fresh = await GetClassicContextAsync(1).ConfigureAwait(false))
                {
                    ProvisioningTemplate extracted = await fresh.GetProvisioningManager()
                        .GetTemplateAsync(new ExtractConfiguration
                        {
                            Handlers = { ConfigurationHandler.PropertyBagEntries },
                        }).ConfigureAwait(false);

                    Console.WriteLine($"Extracted id={extracted.Id}, version={extracted.Version}, scope={extracted.Scope}");

                    Assert.AreEqual(templateId, extracted.Id,
                        "The template id was not round tripped through the property bag.");
                    Assert.AreEqual(7, extracted.Version,
                        "The template version was not round tripped through the property bag.");

                    Assert.IsFalse(extracted.PropertyBagEntries.Any(e => e.Key == "_PnP_ProvisioningTemplateId"),
                        "_PnP_ProvisioningTemplateId leaked into the extracted property bag entries.");
                    Assert.IsFalse(extracted.PropertyBagEntries.Any(e => e.Key == "_PnP_ProvisioningTemplateInfo"),
                        "_PnP_ProvisioningTemplateInfo leaked into the extracted property bag entries.");

                    Assert.IsFalse(string.IsNullOrEmpty(extracted.BaseSiteTemplate),
                        "The base site template was not stamped onto the extracted template.");
                }
            }
        }

        #endregion
    }
}
