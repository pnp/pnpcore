using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasControlModel = PnP.Core.Provisioning.Model.CanvasControl;
using CanvasSectionModel = PnP.Core.Provisioning.Model.CanvasSection;
using ClientSidePageModel = PnP.Core.Provisioning.Model.ClientSidePage;
using FooterModel = PnP.Core.Provisioning.Model.SiteFooter;
using HeaderModel = PnP.Core.Provisioning.Model.SiteHeader;
using ThemeModel = PnP.Core.Provisioning.Model.Theme;

namespace PnP.Core.Provisioning.Test.Scenarios
{
    /// <summary>
    /// Scenario 2 - modern client-side pages, site chrome and a theme.
    /// </summary>
    [TestClass]
    public class Scenario02PagesAndChromeTests : ScenarioTestBase
    {
        private const string Prefix = "PnPCoreScenario2_";

        private static readonly string PageName = $"{Prefix}Home.aspx";

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Scenario")]
        [Timeout(45 * 60 * 1000)]
        public async Task Scenario2_PagesHeaderFooterAndTheme()
        {
            await RunScenarioAsync("s2", BuildTemplate(), new[]
            {
                ConfigurationHandler.Pages,
                ConfigurationHandler.SiteHeader,
                ConfigurationHandler.SiteFooter,
            },
            AssertAsync,

            configuration => configuration.Pages.IncludeAllClientSidePages = true)
            .ConfigureAwait(false);
        }

        private static ProvisioningTemplate BuildTemplate()
        {
            var template = new ProvisioningTemplate { Id = "SCENARIO-2" };

            var page = new ClientSidePageModel
            {
                PageName = PageName,
                Title = $"{Prefix}Home",
                Layout = nameof(PageLayoutType.Article),
                Overwrite = true,
                Publish = true,
                EnableComments = false,
            };

            var section = new CanvasSectionModel { Type = CanvasSectionType.TwoColumn, Order = 1 };

            section.Controls.Add(new CanvasControlModel
            {
                Type = WebPartType.Text,
                Column = 1,
                Order = 1,
                ControlProperties = new Dictionary<string, string> { { "Text", "<p>Scenario 2 left</p>" } },
            });

            section.Controls.Add(new CanvasControlModel
            {
                Type = WebPartType.Text,
                Column = 2,
                Order = 1,
                ControlProperties = new Dictionary<string, string> { { "Text", "<p>Scenario 2 right</p>" } },
            });

            page.Sections.Add(section);
            template.ClientSidePages.Add(page);

            template.Header = new HeaderModel
            {
                Layout = SiteHeaderLayout.Compact,
                MenuStyle = SiteHeaderMenuStyle.MegaMenu,
                BackgroundEmphasis = Emphasis.Strong,
            };

            template.Footer = new FooterModel
            {
                Enabled = true,
                Layout = SiteFooterLayout.Extended,
                BackgroundEmphasis = Emphasis.Neutral,
                RemoveExistingNodes = false,
            };

            template.Theme = new ThemeModel { Name = nameof(SharePointTheme.Blue) };

            return template;
        }

        private static async Task AssertAsync(ProvisioningTemplate extracted, PnPContext site)
        {
            IPage page = (await site.Web.GetPagesAsync(PageName).ConfigureAwait(false)).FirstOrDefault();

            Assert.IsNotNull(page, $"The page '{PageName}' was not created.");

            Console.WriteLine($"Page has {page.Sections.Count} section(s) and {page.Controls.Count} control(s)");

            Assert.AreEqual(1, page.Sections.Count, "The page's canvas has the wrong number of sections.");

            Assert.AreEqual(2, page.Controls.Count,
                "The page was created but its controls were not written - an empty canvas is the " +
                "failure mode here, and it is invisible from the page's existence alone.");

            await site.Web.LoadAsync(w => w.HeaderLayout, w => w.MegaMenuEnabled, w => w.FooterEnabled)
                .ConfigureAwait(false);

            Console.WriteLine($"Header layout: {site.Web.HeaderLayout}, mega menu: {site.Web.MegaMenuEnabled}, " +
                $"footer: {site.Web.FooterEnabled}");

            Assert.AreEqual(HeaderLayoutType.Compact, site.Web.HeaderLayout, "The header layout was not applied.");
            Assert.IsTrue(site.Web.MegaMenuEnabled, "The mega menu was not enabled.");
            Assert.IsTrue(site.Web.FooterEnabled, "The footer was not enabled.");

            ClientSidePageModel extractedPage = extracted.ClientSidePages
                .FirstOrDefault(p => string.Equals(p.PageName, PageName, StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(extractedPage,
                $"The extract did not report the page. Found: " +
                string.Join(", ", extracted.ClientSidePages.Select(p => p.PageName)));

            Assert.IsTrue(extractedPage.Sections.Sum(s => s.Controls.Count) >= 2,
                "The extract reported the page but not its controls, so its canvas was not read back. " +
                $"Sections: {extractedPage.Sections.Count}, controls: " +
                $"{extractedPage.Sections.Sum(s => s.Controls.Count)}");
        }
    }
}
