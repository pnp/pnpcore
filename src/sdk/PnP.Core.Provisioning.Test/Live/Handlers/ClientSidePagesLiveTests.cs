using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasControlModel = PnP.Core.Provisioning.Model.CanvasControl;
using CanvasSectionModel = PnP.Core.Provisioning.Model.CanvasSection;
using ClientSidePageModel = PnP.Core.Provisioning.Model.ClientSidePage;

namespace PnP.Core.Provisioning.Test.Live.Handlers
{
    /// <summary>
    /// Live coverage for <c>ObjectClientSidePages</c> and <c>ObjectClientSidePageContents</c>.
    /// </summary>
    [TestClass]
    public class ClientSidePagesLiveTests : LiveTestBase
    {
        #region Extract

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ClientSidePages_ExtractSurveysTheLibraryAndSkipsAnUncustomisedHomePage()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                IPage homePage = (await context.Web.GetPagesAsync("Home.aspx").ConfigureAwait(false)).FirstOrDefault();
                Assert.IsNotNull(homePage, "The site has no Home.aspx, so there is nothing to survey.");

                bool homePageIsCustomised = homePage.Sections.Count > 0 || homePage.Controls.Count > 0;
                Console.WriteLine($"Home page: {homePage.Sections.Count} section(s), {homePage.Controls.Count} control(s), " +
                    $"customised={homePageIsCustomised}");

                ProvisioningTemplate template = await context.GetProvisioningManager()
                    .GetTemplateAsync(new ExtractConfiguration
                    {
                        Handlers = { ConfigurationHandler.Pages },
                    }).ConfigureAwait(false);

                Console.WriteLine($"Pages extracted: {template.ClientSidePages.Count}");
                foreach (ClientSidePageModel page in template.ClientSidePages)
                {
                    Console.WriteLine($"  {page.PageName} - layout {page.Layout}, {page.Sections.Count} section(s)");
                }

                if (homePageIsCustomised)
                {
                    Assert.AreEqual(1, template.ClientSidePages.Count,
                        "Without IncludeAllClientSidePages, exactly the home page should be extracted.");
                    Assert.IsFalse(string.IsNullOrEmpty(template.ClientSidePages[0].Layout),
                        "The extracted page has no layout, so the read is not working.");
                }
                else
                {
                    Assert.AreEqual(0, template.ClientSidePages.Count,
                        "An uncustomised home page must not be extracted - an empty page would overwrite the target's default.");
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ClientSidePages_ExtractTokenizesListAndSiteIdsOutOfWebPartData()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string pageName = $"{TestPrefix}Tokenized.aspx";

                await context.Site.LoadAsync(s => s.Id).ConfigureAwait(false);
                await context.Web.LoadAsync(w => w.Id).ConfigureAwait(false);

                IList documents = await context.Web.Lists.GetByTitleAsync("Documents", l => l.Id).ConfigureAwait(false);
                Assert.IsNotNull(documents, "The site has no Documents library to point a web part at.");

                try
                {
                    IPage page = await context.Web.NewPageAsync().ConfigureAwait(false);
                    page.PageTitle = $"{TestPrefix}Tokenized";
                    page.AddSection(CanvasSectionTemplate.OneColumn, 1);

                    IPageWebPart listWebPart = await page.InstantiateDefaultWebPartAsync(DefaultWebPart.List).ConfigureAwait(false);
                    listWebPart.PropertiesJson =
                        $"{{\"isDocumentLibrary\":true,\"selectedListId\":\"{documents.Id}\",\"webId\":\"{context.Web.Id}\",\"siteId\":\"{context.Site.Id}\"}}";

                    page.AddControl(listWebPart, page.Sections[0].Columns[0]);
                    await page.SaveAsync(pageName).ConfigureAwait(false);

                    ProvisioningTemplate template = await context.GetProvisioningManager()
                        .GetTemplateAsync(new ExtractConfiguration
                        {
                            Handlers = { ConfigurationHandler.Pages },
                            Pages = { IncludeAllClientSidePages = true },
                        }).ConfigureAwait(false);

                    ClientSidePageModel extracted = template.ClientSidePages
                        .FirstOrDefault(p => p.PageName != null && p.PageName.EndsWith(pageName, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(extracted,
                        $"The page was not extracted. Pages seen: {string.Join(", ", template.ClientSidePages.Select(p => p.PageName))}");

                    string controlData = string.Join(Environment.NewLine,
                        extracted.Sections.SelectMany(s => s.Controls).Select(c => c.JsonControlData));

                    Console.WriteLine($"Extracted control data: {controlData}");

                    Assert.IsFalse(controlData.Contains(documents.Id.ToString(), StringComparison.OrdinalIgnoreCase),
                        "The list id survived extraction - it should have become a {listid:...} token.");
                    Assert.IsFalse(controlData.Contains(context.Site.Id.ToString(), StringComparison.OrdinalIgnoreCase),
                        "The site collection id survived extraction - it should have become {sitecollectionid}.");
                    Assert.IsFalse(controlData.Contains(context.Web.Id.ToString(), StringComparison.OrdinalIgnoreCase),
                        "The web id survived extraction - it should have become {siteid}.");

                    StringAssert.Contains(controlData, "{listid:", "No list id token was produced at all.");
                    StringAssert.Contains(controlData, "{sitecollectionid}", "No site collection id token was produced at all.");
                    StringAssert.Contains(controlData, "{siteid}", "No web id token was produced at all.");
                }
                finally
                {
                    await DeletePageAsync(pageName).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Apply

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ClientSidePages_CreatesAPageWithSectionsAndAText()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string pageName = $"{TestPrefix}Created.aspx";

                try
                {
                    var page = new ClientSidePageModel
                    {
                        PageName = pageName,
                        Title = $"{TestPrefix}Created",
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
                        ControlProperties = new Dictionary<string, string> { { "Text", "<p>Left column</p>" } },
                    });
                    section.Controls.Add(new CanvasControlModel
                    {
                        Type = WebPartType.Text,
                        Column = 2,
                        Order = 1,
                        ControlProperties = new Dictionary<string, string> { { "Text", "<p>Right column</p>" } },
                    });

                    page.Sections.Add(section);

                    var template = new ProvisioningTemplate();
                    template.ClientSidePages.Add(page);

                    await context.GetProvisioningManager().ApplyTemplateAsync(template).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        IPage created = (await fresh.Web.GetPagesAsync(pageName).ConfigureAwait(false)).FirstOrDefault();

                        Assert.IsNotNull(created, "The page was not created.");
                        Console.WriteLine($"Created '{created.Name}': {created.Sections.Count} section(s), {created.Controls.Count} control(s)");

                        Assert.AreEqual(1, created.Sections.Count, "The page should have exactly one section.");
                        Assert.AreEqual(2, created.Sections[0].Columns.Count, "The section should be two columns.");
                        Assert.AreEqual(2, created.Controls.Count, "Both text controls should be on the page.");

                        List<string> texts = created.Controls.OfType<IPageText>().Select(t => t.Text).ToList();
                        Assert.IsTrue(texts.Any(t => t != null && t.Contains("Left column", StringComparison.Ordinal)),
                            $"The left column's text was not written. Texts found: {string.Join(" | ", texts)}");
                        Assert.IsTrue(texts.Any(t => t != null && t.Contains("Right column", StringComparison.Ordinal)),
                            $"The right column's text was not written. Texts found: {string.Join(" | ", texts)}");
                    }
                }
                finally
                {
                    await DeletePageAsync(pageName).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ClientSidePages_RoundTripsAPageThroughExtractAndApply()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string sourceName = $"{TestPrefix}RoundTripSource.aspx";
                string targetName = $"{TestPrefix}RoundTripTarget.aspx";

                try
                {
                    var sourcePage = new ClientSidePageModel
                    {
                        PageName = sourceName,
                        Title = $"{TestPrefix}RoundTrip",
                        Layout = nameof(PageLayoutType.Article),
                        Overwrite = true,
                        Publish = true,
                    };

                    var section = new CanvasSectionModel { Type = CanvasSectionType.OneColumn, Order = 1 };
                    section.Controls.Add(new CanvasControlModel
                    {
                        Type = WebPartType.Text,
                        Column = 1,
                        Order = 1,
                        ControlProperties = new Dictionary<string, string> { { "Text", "<p>Round trip me</p>" } },
                    });
                    sourcePage.Sections.Add(section);

                    var setup = new ProvisioningTemplate();
                    setup.ClientSidePages.Add(sourcePage);

                    IProvisioningManager manager = context.GetProvisioningManager();
                    await manager.ApplyTemplateAsync(setup).ConfigureAwait(false);

                    ProvisioningTemplate extracted = await manager.GetTemplateAsync(new ExtractConfiguration
                    {
                        Handlers = { ConfigurationHandler.Pages },
                        Pages = { IncludeAllClientSidePages = true },
                    }).ConfigureAwait(false);

                    ClientSidePageModel extractedPage = extracted.ClientSidePages
                        .FirstOrDefault(p => p.PageName != null && p.PageName.EndsWith(sourceName, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(extractedPage,
                        $"The page just created was not found in the extract. Pages seen: {string.Join(", ", extracted.ClientSidePages.Select(p => p.PageName))}");

                    Console.WriteLine($"Extracted '{extractedPage.PageName}': title '{extractedPage.Title}', " +
                        $"{extractedPage.Sections.Count} section(s), {extractedPage.Sections.Sum(s => s.Controls.Count)} control(s)");

                    Assert.AreEqual($"{TestPrefix}RoundTrip", extractedPage.Title, "The title did not survive extraction.");
                    Assert.IsTrue(extractedPage.Sections.Sum(s => s.Controls.Count) >= 1,
                        "The text control did not survive extraction.");

                    extractedPage.PageName = targetName;
                    var replay = new ProvisioningTemplate();
                    replay.ClientSidePages.Add(extractedPage);

                    await manager.ApplyTemplateAsync(replay).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        IPage replayed = (await fresh.Web.GetPagesAsync(targetName).ConfigureAwait(false)).FirstOrDefault();

                        Assert.IsNotNull(replayed, "The extracted page could not be applied back.");
                        Console.WriteLine($"Replayed '{replayed.Name}': {replayed.Sections.Count} section(s), {replayed.Controls.Count} control(s)");

                        List<string> texts = replayed.Controls.OfType<IPageText>().Select(t => t.Text).ToList();
                        Assert.IsTrue(texts.Any(t => t != null && t.Contains("Round trip me", StringComparison.Ordinal)),
                            $"The text did not survive the round trip. Texts found: {string.Join(" | ", texts)}");
                    }
                }
                finally
                {
                    await DeletePageAsync(sourceName).ConfigureAwait(false);
                    await DeletePageAsync(targetName).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ClientSidePages_RefusesToOverwriteWhenTheTemplateSaysNotTo()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string pageName = $"{TestPrefix}NoOverwrite.aspx";

                try
                {
                    IProvisioningManager manager = context.GetProvisioningManager();

                    var page = new ClientSidePageModel
                    {
                        PageName = pageName,
                        Title = "Original",
                        Layout = nameof(PageLayoutType.Article),
                        Overwrite = true,
                    };

                    var template = new ProvisioningTemplate();
                    template.ClientSidePages.Add(page);
                    await manager.ApplyTemplateAsync(template).ConfigureAwait(false);

                    page.Title = "Should not win";
                    page.Overwrite = false;

                    var warnings = new List<string>();
                    var configuration = new ApplyConfiguration
                    {
                        MessagesDelegate = (message, type) =>
                        {
                            if (type == ProvisioningMessageType.Warning) warnings.Add(message);
                        },
                    };

                    await manager.ApplyTemplateAsync(template, configuration).ConfigureAwait(false);

                    using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                    {
                        IPage existing = (await fresh.Web.GetPagesAsync(pageName).ConfigureAwait(false)).FirstOrDefault();

                        Assert.IsNotNull(existing);
                        Assert.AreEqual("Original", existing.PageTitle,
                            "Overwrite=false must leave the existing page alone.");
                    }

                    Assert.IsTrue(warnings.Any(w => w.Contains("overwrit", StringComparison.OrdinalIgnoreCase)),
                        "Skipping a page must be reported, not silent. " +
                        $"Warnings seen: {string.Join(" | ", warnings)}");

                    Console.WriteLine($"Warning reported: {warnings.First(w => w.Contains("overwrit", StringComparison.OrdinalIgnoreCase))}");
                }
                finally
                {
                    await DeletePageAsync(pageName).ConfigureAwait(false);
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Handlers")]
        public async Task ClientSidePages_UniquePagePermissionsWarnRatherThanBeingDroppedSilently()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                string pageName = $"{TestPrefix}Secured.aspx";

                try
                {
                    var page = new ClientSidePageModel
                    {
                        PageName = pageName,
                        Title = $"{TestPrefix}Secured",
                        Layout = nameof(PageLayoutType.Article),
                        Overwrite = true,
                    };

                    page.Security.RoleAssignments.Add(new Model.RoleAssignment
                    {
                        Principal = "someone@example.com",
                        RoleDefinition = "Read",
                    });

                    var template = new ProvisioningTemplate();
                    template.ClientSidePages.Add(page);

                    var warnings = new List<string>();
                    var configuration = new ApplyConfiguration
                    {
                        MessagesDelegate = (message, type) =>
                        {
                            if (type == ProvisioningMessageType.Warning) warnings.Add(message);
                        },
                    };

                    await context.GetProvisioningManager().ApplyTemplateAsync(template, configuration).ConfigureAwait(false);

                    Assert.IsTrue(warnings.Any(w => w.Contains("unique permissions", StringComparison.OrdinalIgnoreCase)),
                        "A page with unique permissions must warn that they were not applied. " +
                        $"Warnings seen: {string.Join(" | ", warnings)}");

                    Console.WriteLine($"Warning reported: {warnings.First(w => w.Contains("unique permissions", StringComparison.OrdinalIgnoreCase))}");
                }
                finally
                {
                    await DeletePageAsync(pageName).ConfigureAwait(false);
                }
            }
        }

        #endregion

        private static async Task DeletePageAsync(string pageName)
        {
            try
            {
                using (PnPContext context = await GetContextAsync(2).ConfigureAwait(false))
                {
                    IPage page = (await context.Web.GetPagesAsync(pageName).ConfigureAwait(false)).FirstOrDefault();
                    if (page != null)
                    {
                        await page.DeleteAsync().ConfigureAwait(false);
                        Console.WriteLine($"Deleted '{pageName}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COULD NOT DELETE '{pageName}': {Describe(ex)}");
            }
        }
    }
}
