using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class PagesTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        
        #region Page Loading
        [TestMethod]
        public async Task CleanLoad()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var pages = await context.Web.GetPagesAsync();
                Assert.IsTrue(pages.Count > 0);
            }
        }

        [TestMethod]
        public async Task PagesLibraryWasLoadedCorrect()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.Lists.GetByTitleAsync("Site Pages",
                    p => p.Title, p => p.EnableFolderCreation, p => p.EnableMinorVersions, p => p.EnableModeration,
                    p => p.EnableVersioning).ConfigureAwait(false);

                var pages = await context.Web.GetPagesAsync();
                Assert.IsTrue(pages.Count > 0);
            }
        }

        [TestMethod]
        public async Task PagesLibraryWasLoadedIncomplete()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.Lists.GetByTitleAsync("Site Pages",
                    p => p.Title, p => p.EnableModeration, p => p.EnableVersioning).ConfigureAwait(false);

                var pages = await context.Web.GetPagesAsync();
                Assert.IsTrue(pages.Count > 0);
            }
        }

        [TestMethod]
        public async Task PagesLibraryWasLoadedWithoutTitle()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.Lists.GetByTitleAsync("Site Pages", p => p.EnableModeration, p => p.EnableVersioning).ConfigureAwait(false);

                var pages = await context.Web.GetPagesAsync();
                Assert.IsTrue(pages.Count > 0);
            }
        }

        [TestMethod]
        public async Task LoadPagesWithFilter()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var pages = await context.Web.GetPagesAsync("Home");
                Assert.IsTrue(pages.Count == 1);
            }
        }

        [TestMethod]
        public async Task LoadPagesWithASPXFilter()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var pages = await context.Web.GetPagesAsync("HoMe.aspx");
                Assert.IsTrue(pages.Count == 1);
            }
        }

        [TestMethod]
        public async Task LoadPagesWithPartialFilter()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var pages = await context.Web.GetPagesAsync("Hom");
                Assert.IsTrue(pages.Count == 1);
            }
        }

        [TestMethod]
        public async Task LoadPagesWithFilterWithoutResult()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var pages = await context.Web.GetPagesAsync("JLKJLKJLK");
                Assert.IsTrue(pages.Count == 0);
            }
        }

        [TestMethod]
        public async Task LoadPagesInFolder()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = $"{TestCommon.GetPnPSdkTestAssetName("folder1")}/{TestCommon.GetPnPSdkTestAssetName("LoadPagesInFolder.aspx")}";

                // Save the page with a folder in the path
                await newPage.SaveAsync(pageName);

                // Get the page with a folder in the path
                var pages = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages.Count == 1);

                // delete folder with page
                var folderToDelete = await newPage.PagesLibrary.RootFolder.EnsureFolderAsync(TestCommon.GetPnPSdkTestAssetName("folder1"));
                await folderToDelete.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task LoadPagesInNestedFolder()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = $"{TestCommon.GetPnPSdkTestAssetName("folder1")}/{TestCommon.GetPnPSdkTestAssetName("folder2")}/{TestCommon.GetPnPSdkTestAssetName("folder3")}/{TestCommon.GetPnPSdkTestAssetName("LoadPagesInNestedFolder.aspx")}";

                // Save the page with a folder in the path
                await newPage.SaveAsync(pageName);

                // Get the page with a folder in the path
                var pages = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages.Count == 1);
                Assert.IsTrue(pages.AsEnumerable().First().Folder == $"{TestCommon.GetPnPSdkTestAssetName("folder1")}/{TestCommon.GetPnPSdkTestAssetName("folder2")}/{TestCommon.GetPnPSdkTestAssetName("folder3")}");
                Assert.IsTrue(pages.AsEnumerable().First().PageId.Value > 0);

                // delete folder with page
                var folderToDelete = await newPage.PagesLibrary.RootFolder.EnsureFolderAsync(TestCommon.GetPnPSdkTestAssetName("folder1"));
                await folderToDelete.DeleteAsync();
            }
        }
        
        [TestMethod]
        public async Task LoadPagesWhenThereAreMultiplePagesLibraries()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new sub site to test as we don't want to break the main site home page
                string webTitle = "PageTestSubWeb";
                var addedWeb = await context.Web.Webs.AddAsync(new WebOptions { Title = webTitle, Url = webTitle, Language = 1033 });

                // Create a context for the newly created web
                using (var context2 = await TestCommon.Instance.CloneAsync(context, addedWeb.Url, 1))
                {
                    // Create a wiki page library with a name that alphabetically comes before the site pages library name
                    await context2.Web.Lists.AddAsync("AWiki", ListTemplateType.WebPageLibrary);

                    // Read the current home page
                    string pageName = "Home.aspx";
                    var pages = await context2.Web.GetPagesAsync(pageName);

                    Assert.IsTrue(pages.AsEnumerable().First() != null);
                    Assert.IsTrue(pages.AsEnumerable().First().PagesLibrary != null);
                    Assert.IsTrue(pages.AsEnumerable().First().PagesLibrary.Title == "Site Pages");
                }

                // Delete the web to cleanup the test artefacts
                await addedWeb.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task LoadPagesWithSimilarNames()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage1 = await context.Web.NewPageAsync();
                string pageName1 = $"{TestCommon.GetPnPSdkTestAssetName("ADS.aspx")}";

                // Save the page
                await newPage1.SaveAsync(pageName1);

                var pages = await context.Web.GetPagesAsync(TestCommon.GetPnPSdkTestAssetName("AD"));

                Assert.IsTrue(pages.Count == 1);
                Assert.IsTrue(pages.AsEnumerable().FirstOrDefault(p => p.Name == TestCommon.GetPnPSdkTestAssetName("ADS.aspx")) != null);

                // Delete the created pages again
                await newPage1.DeleteAsync();
            }
        }

        [TestMethod]
        public void DeserializeCanvasControlDataTest()
        {
            var controlDataJson = """
            {
              "id": "61f0ef56-b5f0-4d94-b1b9-0a5087332cc7",
              "controlType": 3,
              "position": {
                "layoutIndex": 1,
                "zoneIndex": null,
                "sectionIndex": null,
                "controlIndex": 1,
                "sectionFactor": 0,
                "zoneId": "f419c0ec-ee17-48d1-b8cb-30e296c9d286"
              },
              "webPartId": "cbe7b0a9-3504-44dd-a3a3-0e5cacd07788",
              "reservedHeight": 207,
              "addedFromPersistedData": true,
              "reservedWidth": 1607
            }
            """;
            var controlData = JsonSerializer.Deserialize<CanvasControlData>(controlDataJson, PnPConstants.JsonSerializer_IgnoreNullValues);
            Assert.IsNotNull(controlData.Position.ZoneIndex == 0);
            Assert.IsNotNull(controlData.Position.SectionIndex == 0);

            controlDataJson = """
            {
              "id": "61f0ef56-b5f0-4d94-b1b9-0a5087332cc7",
              "controlType": 3,
              "position": {
                "layoutIndex": 1,
                "zoneIndex": 2,
                "sectionIndex": 3,
                "controlIndex": 1,
                "sectionFactor": 0,
                "zoneId": "f419c0ec-ee17-48d1-b8cb-30e296c9d286"
              },
              "webPartId": "cbe7b0a9-3504-44dd-a3a3-0e5cacd07788",
              "reservedHeight": 207,
              "addedFromPersistedData": true,
              "reservedWidth": 1607
            }
            """;

            controlData = JsonSerializer.Deserialize<CanvasControlData>(controlDataJson, PnPConstants.JsonSerializer_IgnoreNullValues);
            Assert.IsNotNull(controlData.Position.ZoneIndex == 2);
            Assert.IsNotNull(controlData.Position.SectionIndex == 3);
        }
        #endregion

        #region Available web parts tests
        [TestMethod]
        public async Task GetAvailableWebParts()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var pages = await context.Web.GetPagesAsync("Ho");
                var availableComponents = await pages.AsEnumerable().First().AvailablePageComponentsAsync();
                Assert.IsTrue(availableComponents.Count() > 0);

                var imageWebPartId = pages.AsEnumerable().First().DefaultWebPartToWebPartId(DefaultWebPart.Image);
                var imageWebPart = availableComponents.FirstOrDefault(p => p.Id == imageWebPartId);
                Assert.IsTrue(imageWebPart != null);
            }
        }

        [TestMethod]
        public async Task GetAvailableWebPartsWithFilter()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var pages = await context.Web.GetPagesAsync("Ho");
                var imageWebPartId = pages.AsEnumerable().First().DefaultWebPartToWebPartId(DefaultWebPart.Image);
                var availableComponents = await pages.AsEnumerable().First().AvailablePageComponentsAsync(imageWebPartId);
                Assert.IsTrue(availableComponents.Count() == 1);
                Assert.IsTrue(availableComponents.First().Id == imageWebPartId);
            }
        }
        #endregion

        #region Multilingual tests
        [TestMethod]
        public async Task EnsureSiteIsMultilingual()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                // Enable this site for multilingual pages
                await context.Web.EnsureMultilingualAsync(new List<int>() { 1043, 1036 });

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 2))
                {
                    // Verify everything went well
                    await context2.Web.EnsurePropertiesAsync(p => p.Features, p => p.IsMultilingual, p => p.SupportedUILanguageIds);

                    Assert.IsTrue(context2.Web.Features.AsRequested().FirstOrDefault(p => p.DefinitionId == new Guid("24611c05-ee19-45da-955f-6602264abaf8")) != null);
                    Assert.IsTrue(context2.Web.SupportedUILanguageIds.Contains(1043));
                    Assert.IsTrue(context2.Web.SupportedUILanguageIds.Contains(1036));

                    // Run ensure again on a site that was ensured previously to see also test that code path
                    await context2.Web.EnsureMultilingualAsync(new List<int>() { 1043, 1036 });

                    // Turn off multilingual pages again
                    await DisableMultilingual(context2);
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 3))
                {
                    // Verify everything was turned off again
                    await context3.Web.EnsurePropertiesAsync(p => p.Features, p => p.SupportedUILanguageIds);

                    Assert.IsTrue(context3.Web.Features.AsRequested().FirstOrDefault(p => p.DefinitionId == new Guid("24611c05-ee19-45da-955f-6602264abaf8")) == null);
                    Assert.IsFalse(context3.Web.SupportedUILanguageIds.Contains(1043));
                    Assert.IsFalse(context3.Web.SupportedUILanguageIds.Contains(1036));
                }
            }
        }

        private static async Task DisableMultilingual(Core.Services.PnPContext context)
        {
            await context.Web.LoadAsync(p => p.Features, p => p.SupportedUILanguageIds);
            await context.Web.Features.DisableBatchAsync(new Guid("24611c05-ee19-45da-955f-6602264abaf8"));
            context.Web.SupportedUILanguageIds.Remove(1043);
            context.Web.SupportedUILanguageIds.Remove(1036);
            await context.Web.UpdateBatchAsync();
            await context.ExecuteAsync();
        }

        [TestMethod]
        public async Task GenerateGetAndDeletePageTranslations()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                // Enable this site for multilingual pages
                await context.Web.EnsureMultilingualAsync(new List<int>() { 1043, 1036 });

                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PageToTranslate.aspx");

                // Save the page with a folder in the path
                await newPage.SaveAsync(pageName);

                // Page should not yet have translations
                var pageTranslations = await newPage.GetPageTranslationsAsync();

                CultureInfo culture1043 = new CultureInfo(1043);
                CultureInfo culture1036 = new CultureInfo(1036);
                Assert.IsTrue(pageTranslations.TranslatedLanguages.FirstOrDefault(p => p.Culture.Equals(culture1043.Name, StringComparison.InvariantCultureIgnoreCase)) == null);
                Assert.IsTrue(pageTranslations.TranslatedLanguages.FirstOrDefault(p => p.Culture.Equals(culture1036.Name, StringComparison.InvariantCultureIgnoreCase)) == null);

                // generate translation for the page
                pageTranslations = await newPage.TranslatePagesAsync();
                Assert.IsTrue(pageTranslations.TranslatedLanguages.FirstOrDefault(p => p.Culture.Equals(culture1043.Name, StringComparison.InvariantCultureIgnoreCase)) != null);
                Assert.IsTrue(pageTranslations.TranslatedLanguages.FirstOrDefault(p => p.Culture.Equals(culture1036.Name, StringComparison.InvariantCultureIgnoreCase)) != null);

                // Delete the created page and it's translations
                foreach (var translation in pageTranslations.TranslatedLanguages)
                {
                    var folder = translation.Culture.Split('-')[0].ToLower();
                    var pagesToDelete = await context.Web.GetPagesAsync($"{folder}/{pageName}");
                    if (pagesToDelete.Count > 0)
                    {
                        await pagesToDelete.AsEnumerable().FirstOrDefault()?.DeleteAsync();
                    }
                }

                var pages = await context.Web.GetPagesAsync(pageName);
                await pages.AsEnumerable().First().DeleteAsync();

                // turn off multilingual again
                await DisableMultilingual(context);
            }
        }
        #endregion

        #region Page deletion
        [TestMethod]
        public async Task CreateAndDeletePage()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("CreateAndDeletePage.aspx");
                // Save the page
                await newPage.SaveAsync(pageName);
                // Verify the page exists
                var pages = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages.Count == 1);
                Assert.IsTrue(pages.AsEnumerable().First().PageTitle == pageName.Replace(".aspx", ""));
                // Delete the page
                await newPage.DeleteAsync();
                // Verify the page exists
                var pages2 = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages2.Count == 0);
            }
        }

        #endregion

        #region Page content tests

        [TestMethod]
        public async Task PageTextTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PageTextTest.aspx");

                page.AddSection(CanvasSectionTemplate.OneColumn, 1);

                page.AddControl(page.NewTextPart("Normal"), page.Sections[0].Columns[0]);
                page.AddControl(page.NewTextPart("<p>Normal</p><p>Normal</p>"), page.Sections[0].Columns[0]);
                page.AddControl(page.NewTextPart("<h2>Heading1</h2><p>Normal</p>"), page.Sections[0].Columns[0]);
                page.AddControl(page.NewTextPart("<h3>Heading2</h3><p>Normal</p>"), page.Sections[0].Columns[0]);
                page.AddControl(page.NewTextPart("<h4>Heading 3</h4><p>Normal</p>"), page.Sections[0].Columns[0]);
                page.AddControl(page.NewTextPart("<pre>fixed</pre><p>Normal</p>"), page.Sections[0].Columns[0]);
                page.AddControl(page.NewTextPart("<blockquote>quote</blockquote><p>Normal</p>"), page.Sections[0].Columns[0]);
                page.AddControl(page.NewTextPart("<ul><li>fixed</li></ul><p>Normal</p>"), page.Sections[0].Columns[0]);

                await page.SaveAsync(pageName);

                // Load the page again
                var pages = await context.Web.GetPagesAsync(pageName);
                var createdPage = pages.First();

                Assert.IsTrue((createdPage.Sections[0].Columns[0].Controls[0] as PageText).Text == "Normal");
                Assert.IsTrue((createdPage.Sections[0].Columns[0].Controls[1] as PageText).Text == "<p>Normal</p><p>Normal</p>");
                Assert.IsTrue((createdPage.Sections[0].Columns[0].Controls[2] as PageText).Text == "<h2>Heading1</h2><p>Normal</p>");
                Assert.IsTrue((createdPage.Sections[0].Columns[0].Controls[3] as PageText).Text == "<h3>Heading2</h3><p>Normal</p>");
                Assert.IsTrue((createdPage.Sections[0].Columns[0].Controls[4] as PageText).Text == "<h4>Heading 3</h4><p>Normal</p>");
                Assert.IsTrue((createdPage.Sections[0].Columns[0].Controls[5] as PageText).Text == "<pre>fixed</pre><p>Normal</p>");
                Assert.IsTrue((createdPage.Sections[0].Columns[0].Controls[6] as PageText).Text == "<blockquote>quote</blockquote><p>Normal</p>");
                Assert.IsTrue((createdPage.Sections[0].Columns[0].Controls[7] as PageText).Text == "<ul><li>fixed</li></ul><p>Normal</p>");

                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PageText2Test()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PageText2Test.aspx");

                page.AddSection(CanvasSectionTemplate.OneColumn, 1);

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    page.AddControl(null);
                });

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    page.AddControl(page.NewTextPart("<h2>Heading1</h2><p>Normal</p>"), null as ICanvasSection);
                });

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    page.AddControl(null, page.Sections[0]);
                });

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    page.AddControl(null, 10);
                });

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    page.AddControl(null, page.Sections[0], 20);
                });

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    page.AddControl(page.NewTextPart("<h2>Heading1</h2><p>Normal</p>"), null as ICanvasSection, 20);
                });

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    page.AddControl(null, page.Sections[0].Columns[0], 30);
                });

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    page.AddControl(page.NewTextPart("<h3>Heading3</h3><p>Normal</p>"), null as ICanvasColumn, 30);
                });

                page.AddControl(page.NewTextPart("Normal"));
                page.AddControl(page.NewTextPart("<h2>Heading1</h2><p>Normal</p>"), page.Sections[0]);
                page.AddControl(page.NewTextPart("<p>Normal</p><p>Normal</p>"), 10);
                page.AddControl(page.NewTextPart("<h2>Heading1</h2><p>Normal</p>"), page.Sections[0], 20);
                page.AddControl(page.NewTextPart("<h3>Heading3</h3><p>Normal</p>"), page.Sections[0].Columns[0], 30);

                await page.SaveAsync(pageName);

                // Load the page again
                var pages = await context.Web.GetPagesAsync(pageName);
                var createdPage = pages.First();

                Assert.IsTrue((createdPage.Sections[0].Columns[0].Controls[0] as PageText).Text == "Normal");
                Assert.IsTrue((createdPage.Sections[0].Columns[0].Controls[1] as PageText).Text == "<h2>Heading1</h2><p>Normal</p>");
                Assert.IsTrue((createdPage.Sections[0].Columns[0].Controls[2] as PageText).Text == "<p>Normal</p><p>Normal</p>");
                Assert.IsTrue((createdPage.Sections[0].Columns[0].Controls[3] as PageText).Text == "<h2>Heading1</h2><p>Normal</p>");
                Assert.IsTrue((createdPage.Sections[0].Columns[0].Controls[4] as PageText).Text == "<h3>Heading3</h3><p>Normal</p>");

                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PageSectionsCreateTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PageSectionsCreateTest.aspx");

                // Add all the possible sections 
                page.AddSection(CanvasSectionTemplate.OneColumnFullWidth, 1);
                page.AddSection(CanvasSectionTemplate.OneColumn, 2);
                page.AddSection(CanvasSectionTemplate.TwoColumn, 3);
                page.AddSection(CanvasSectionTemplate.TwoColumnLeft, 4);
                page.AddSection(CanvasSectionTemplate.TwoColumnRight, 5);
                page.AddSection(CanvasSectionTemplate.ThreeColumn, 6);

                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.Sections.Count == 6);
                Assert.IsTrue(page.Sections[0].Type == CanvasSectionTemplate.OneColumnFullWidth);
                Assert.IsTrue(page.Sections[1].Type == CanvasSectionTemplate.OneColumn);
                Assert.IsTrue(page.Sections[2].Type == CanvasSectionTemplate.TwoColumn);
                Assert.IsTrue(page.Sections[3].Type == CanvasSectionTemplate.TwoColumnLeft);
                Assert.IsTrue(page.Sections[4].Type == CanvasSectionTemplate.TwoColumnRight);
                Assert.IsTrue(page.Sections[5].Type == CanvasSectionTemplate.ThreeColumn);

                page.ClearPage();
                Assert.IsTrue(page.Sections.Count == 0);

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PageSectionsCreateRemoveTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PageSectionsCreateRemoveTest.aspx");

                // Add all the possible sections 
                page.AddSection(CanvasSectionTemplate.OneColumnFullWidth, 1);
                page.AddSection(CanvasSectionTemplate.OneColumn, 2);
                page.AddSection(CanvasSectionTemplate.TwoColumn, 3);
                page.AddSection(CanvasSectionTemplate.TwoColumnLeft, 4);
                page.AddSection(CanvasSectionTemplate.TwoColumnRight, 5);
                page.AddSection(CanvasSectionTemplate.ThreeColumn, 6);

                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.Sections.Count == 6);
                Assert.IsTrue(page.Sections[0].Type == CanvasSectionTemplate.OneColumnFullWidth);
                Assert.IsTrue(page.Sections[1].Type == CanvasSectionTemplate.OneColumn);
                Assert.IsTrue(page.Sections[2].Type == CanvasSectionTemplate.TwoColumn);
                Assert.IsTrue(page.Sections[3].Type == CanvasSectionTemplate.TwoColumnLeft);
                Assert.IsTrue(page.Sections[4].Type == CanvasSectionTemplate.TwoColumnRight);
                Assert.IsTrue(page.Sections[5].Type == CanvasSectionTemplate.ThreeColumn);

                page.Sections.RemoveAt(5);
                page.Sections.RemoveAt(4);

                await page.SaveAsync(pageName);

                pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.Sections.Count == 4);
                Assert.IsTrue(page.Sections[0].Type == CanvasSectionTemplate.OneColumnFullWidth);
                Assert.IsTrue(page.Sections[1].Type == CanvasSectionTemplate.OneColumn);
                Assert.IsTrue(page.Sections[2].Type == CanvasSectionTemplate.TwoColumn);
                Assert.IsTrue(page.Sections[3].Type == CanvasSectionTemplate.TwoColumnLeft);

                // Delete all sections
                page.ClearPage();

                await page.SaveAsync(pageName);

                pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.Sections.Count == 0);

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PageSectionsWithEmphasisCreateTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PageSectionsWithEmphasisCreateTest.aspx");

                // Add all the possible sections 
                page.AddSection(CanvasSectionTemplate.OneColumnFullWidth, 1, VariantThemeType.Neutral);
                page.AddSection(CanvasSectionTemplate.OneColumn, 2, VariantThemeType.None);
                page.AddSection(CanvasSectionTemplate.TwoColumn, 3, VariantThemeType.Soft);
                page.AddSection(CanvasSectionTemplate.TwoColumnLeft, 4, VariantThemeType.Strong);
                page.AddSection(CanvasSectionTemplate.TwoColumnRight, 5, VariantThemeType.Neutral);
                page.AddSection(CanvasSectionTemplate.ThreeColumn, 6, VariantThemeType.None);

                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.Sections.Count == 6);
                Assert.IsTrue(page.Sections[0].Type == CanvasSectionTemplate.OneColumnFullWidth);
                Assert.IsTrue(page.Sections[0].ZoneEmphasis == (int)VariantThemeType.Neutral);

                Assert.IsTrue(page.Sections[1].Type == CanvasSectionTemplate.OneColumn);
                Assert.IsTrue(page.Sections[1].ZoneEmphasis == (int)VariantThemeType.None);

                Assert.IsTrue(page.Sections[2].Type == CanvasSectionTemplate.TwoColumn);
                Assert.IsTrue(page.Sections[2].ZoneEmphasis == (int)VariantThemeType.Soft);

                Assert.IsTrue(page.Sections[3].Type == CanvasSectionTemplate.TwoColumnLeft);
                Assert.IsTrue(page.Sections[3].ZoneEmphasis == (int)VariantThemeType.Strong);

                Assert.IsTrue(page.Sections[4].Type == CanvasSectionTemplate.TwoColumnRight);
                Assert.IsTrue(page.Sections[4].ZoneEmphasis == (int)VariantThemeType.Neutral);

                Assert.IsTrue(page.Sections[5].Type == CanvasSectionTemplate.ThreeColumn);
                Assert.IsTrue(page.Sections[5].ZoneEmphasis == (int)VariantThemeType.None);

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PageSectionsWithEmphasisAndControlCreateTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PageSectionsWithEmphasisAndControlCreateTest.aspx");

                // Add all the possible sections 
                page.AddSection(CanvasSectionTemplate.OneColumnFullWidth, 1, VariantThemeType.Neutral);
                page.AddSection(CanvasSectionTemplate.OneColumn, 2, VariantThemeType.None);
                page.AddSection(CanvasSectionTemplate.TwoColumn, 3, VariantThemeType.Soft);
                page.AddSection(CanvasSectionTemplate.TwoColumnLeft, 4, VariantThemeType.Strong);
                page.AddSection(CanvasSectionTemplate.TwoColumnRight, 5, VariantThemeType.Neutral);
                page.AddSection(CanvasSectionTemplate.ThreeColumn, 6, VariantThemeType.None);

                // Add a text control in each section
                page.AddControl(page.NewTextPart("PnP"), page.Sections[1].Columns[0]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[2].Columns[0]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[2].Columns[1]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[3].Columns[0]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[3].Columns[1]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[4].Columns[0]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[4].Columns[1]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[5].Columns[0]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[5].Columns[1]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[5].Columns[2]);

                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.Sections.Count == 6);
                Assert.IsTrue(page.Sections[0].Type == CanvasSectionTemplate.OneColumnFullWidth);
                Assert.IsTrue(page.Sections[0].ZoneEmphasis == (int)VariantThemeType.Neutral);


                Assert.IsTrue(page.Sections[1].Type == CanvasSectionTemplate.OneColumn);
                Assert.IsTrue(page.Sections[1].ZoneEmphasis == (int)VariantThemeType.None);
                Assert.IsTrue(page.Sections[1].Columns[0].Controls.Count == 1);

                Assert.IsTrue(page.Sections[2].Type == CanvasSectionTemplate.TwoColumn);
                Assert.IsTrue(page.Sections[2].ZoneEmphasis == (int)VariantThemeType.Soft);
                Assert.IsTrue(page.Sections[2].Columns[0].Controls.Count == 1);
                Assert.IsTrue(page.Sections[2].Columns[1].Controls.Count == 1);

                Assert.IsTrue(page.Sections[3].Type == CanvasSectionTemplate.TwoColumnLeft);
                Assert.IsTrue(page.Sections[3].ZoneEmphasis == (int)VariantThemeType.Strong);
                Assert.IsTrue(page.Sections[3].Columns[0].Controls.Count == 1);
                Assert.IsTrue(page.Sections[3].Columns[1].Controls.Count == 1);

                Assert.IsTrue(page.Sections[4].Type == CanvasSectionTemplate.TwoColumnRight);
                Assert.IsTrue(page.Sections[4].ZoneEmphasis == (int)VariantThemeType.Neutral);
                Assert.IsTrue(page.Sections[4].Columns[0].Controls.Count == 1);
                Assert.IsTrue(page.Sections[4].Columns[1].Controls.Count == 1);

                Assert.IsTrue(page.Sections[5].Type == CanvasSectionTemplate.ThreeColumn);
                Assert.IsTrue(page.Sections[5].ZoneEmphasis == (int)VariantThemeType.None);
                Assert.IsTrue(page.Sections[5].Columns[0].Controls.Count == 1);
                Assert.IsTrue(page.Sections[5].Columns[1].Controls.Count == 1);
                Assert.IsTrue(page.Sections[5].Columns[2].Controls.Count == 1);

                page.ClearPage();
                Assert.IsTrue(page.Sections.Count == 0);
                Assert.IsTrue(page.Controls.Count == 0);

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PageSectionsWithEmphasisAndWebPartsCreateTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PageSectionsWithEmphasisAndWebPartsCreateTest.aspx");

                // Add all the possible sections 
                page.AddSection(CanvasSectionTemplate.OneColumnFullWidth, 1, VariantThemeType.Neutral);
                page.AddSection(CanvasSectionTemplate.OneColumn, 2, VariantThemeType.None);
                page.AddSection(CanvasSectionTemplate.TwoColumn, 3, VariantThemeType.Soft);
                page.AddSection(CanvasSectionTemplate.TwoColumnLeft, 4, VariantThemeType.Strong);
                page.AddSection(CanvasSectionTemplate.TwoColumnRight, 5, VariantThemeType.Neutral);
                page.AddSection(CanvasSectionTemplate.ThreeColumn, 6, VariantThemeType.None);

                var availableComponents = await page.AvailablePageComponentsAsync();
                var imageWebPartComponent = availableComponents.FirstOrDefault(p => p.Id == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));

                // Add a text control in each section
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[0].Columns[0]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[1].Columns[0]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[2].Columns[0]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[2].Columns[1]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[3].Columns[0]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[3].Columns[1]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[4].Columns[0]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[4].Columns[1]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[5].Columns[0]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[5].Columns[1]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[5].Columns[2]);

                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.Sections.Count == 6);
                Assert.IsTrue(page.Sections[0].Type == CanvasSectionTemplate.OneColumnFullWidth);
                Assert.IsTrue(page.Sections[0].ZoneEmphasis == (int)VariantThemeType.Neutral);
                Assert.IsTrue(page.Sections[0].Columns[0].Controls.Count == 1);
                Assert.IsTrue(page.Sections[0].Columns[0].Controls[0] is IPageWebPart);
                Assert.IsTrue((page.Sections[0].Columns[0].Controls[0] as IPageWebPart).WebPartId == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));

                Assert.IsTrue(page.Sections[1].Type == CanvasSectionTemplate.OneColumn);
                Assert.IsTrue(page.Sections[1].ZoneEmphasis == (int)VariantThemeType.None);
                Assert.IsTrue(page.Sections[1].Columns[0].Controls.Count == 1);
                Assert.IsTrue(page.Sections[1].Columns[0].Controls[0] is IPageWebPart);
                Assert.IsTrue((page.Sections[1].Columns[0].Controls[0] as IPageWebPart).WebPartId == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));

                Assert.IsTrue(page.Sections[2].Type == CanvasSectionTemplate.TwoColumn);
                Assert.IsTrue(page.Sections[2].ZoneEmphasis == (int)VariantThemeType.Soft);
                Assert.IsTrue(page.Sections[2].Columns[0].Controls.Count == 1);
                Assert.IsTrue(page.Sections[2].Columns[1].Controls.Count == 1);
                Assert.IsTrue(page.Sections[2].Columns[0].Controls[0] is IPageWebPart);
                Assert.IsTrue((page.Sections[2].Columns[0].Controls[0] as IPageWebPart).WebPartId == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));
                Assert.IsTrue(page.Sections[2].Columns[1].Controls[0] is IPageWebPart);
                Assert.IsTrue((page.Sections[2].Columns[1].Controls[0] as IPageWebPart).WebPartId == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));

                Assert.IsTrue(page.Sections[3].Type == CanvasSectionTemplate.TwoColumnLeft);
                Assert.IsTrue(page.Sections[3].ZoneEmphasis == (int)VariantThemeType.Strong);
                Assert.IsTrue(page.Sections[3].Columns[0].Controls.Count == 1);
                Assert.IsTrue(page.Sections[3].Columns[1].Controls.Count == 1);
                Assert.IsTrue(page.Sections[3].Columns[0].Controls[0] is IPageWebPart);
                Assert.IsTrue((page.Sections[3].Columns[0].Controls[0] as IPageWebPart).WebPartId == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));
                Assert.IsTrue(page.Sections[3].Columns[1].Controls[0] is IPageWebPart);
                Assert.IsTrue((page.Sections[3].Columns[1].Controls[0] as IPageWebPart).WebPartId == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));

                Assert.IsTrue(page.Sections[4].Type == CanvasSectionTemplate.TwoColumnRight);
                Assert.IsTrue(page.Sections[4].ZoneEmphasis == (int)VariantThemeType.Neutral);
                Assert.IsTrue(page.Sections[4].Columns[0].Controls.Count == 1);
                Assert.IsTrue(page.Sections[4].Columns[1].Controls.Count == 1);
                Assert.IsTrue(page.Sections[4].Columns[0].Controls[0] is IPageWebPart);
                Assert.IsTrue((page.Sections[4].Columns[0].Controls[0] as IPageWebPart).WebPartId == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));
                Assert.IsTrue(page.Sections[4].Columns[1].Controls[0] is IPageWebPart);
                Assert.IsTrue((page.Sections[4].Columns[1].Controls[0] as IPageWebPart).WebPartId == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));

                Assert.IsTrue(page.Sections[5].Type == CanvasSectionTemplate.ThreeColumn);
                Assert.IsTrue(page.Sections[5].ZoneEmphasis == (int)VariantThemeType.None);
                Assert.IsTrue(page.Sections[5].Columns[0].Controls.Count == 1);
                Assert.IsTrue(page.Sections[5].Columns[1].Controls.Count == 1);
                Assert.IsTrue(page.Sections[5].Columns[2].Controls.Count == 1);
                Assert.IsTrue(page.Sections[5].Columns[0].Controls[0] is IPageWebPart);
                Assert.IsTrue((page.Sections[5].Columns[0].Controls[0] as IPageWebPart).WebPartId == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));
                Assert.IsTrue(page.Sections[5].Columns[1].Controls[0] is IPageWebPart);
                Assert.IsTrue((page.Sections[5].Columns[1].Controls[0] as IPageWebPart).WebPartId == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));
                Assert.IsTrue(page.Sections[5].Columns[2].Controls[0] is IPageWebPart);
                Assert.IsTrue((page.Sections[5].Columns[2].Controls[0] as IPageWebPart).WebPartId == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CollapsiblePageSections()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("CollapsiblePageSections.aspx");

                // Non collapsible section
                page.AddSection(CanvasSectionTemplate.OneColumn, 1, VariantThemeType.Neutral);

                // Collapsible section - collapsed
                page.AddSection(CanvasSectionTemplate.TwoColumn, 2, VariantThemeType.Soft);
                page.Sections[1].Collapsible = true;
                page.Sections[1].DisplayName = "Section 1";
                page.Sections[1].IsExpanded = false;
                page.Sections[1].ShowDividerLine = false;
                page.Sections[1].IconAlignment = IconAlignment.Right;

                // Collapsible section - expanded
                page.AddSection(CanvasSectionTemplate.ThreeColumn, 3, VariantThemeType.None);
                page.Sections[2].Collapsible = true;
                page.Sections[2].IsExpanded = true;
                page.Sections[2].ShowDividerLine = false;

                var availableComponents = await page.AvailablePageComponentsAsync();
                var imageWebPartComponent = availableComponents.FirstOrDefault(p => p.Id == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));

                // Add a text control in each section
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[1].Columns[0]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[1].Columns[1]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[2].Columns[0]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[2].Columns[1]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[2].Columns[2]);

                // Add a webpart in each section
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[0].Columns[0]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[1].Columns[0]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[1].Columns[1]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[2].Columns[0]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[2].Columns[1]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[2].Columns[2]);

                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.Sections.Count == 3);
                Assert.IsTrue(page.Sections[0].Type == CanvasSectionTemplate.OneColumn);
                Assert.IsTrue(page.Sections[0].ZoneEmphasis == (int)VariantThemeType.Neutral);
                Assert.IsTrue(page.Sections[0].Collapsible == false);
                Assert.IsTrue(page.Sections[0].Columns[0].Controls.Count == 2);

                Assert.IsTrue(page.Sections[1].Type == CanvasSectionTemplate.TwoColumn);
                Assert.IsTrue(page.Sections[1].ZoneEmphasis == (int)VariantThemeType.Soft);
                Assert.IsTrue(page.Sections[1].Collapsible == true);
                Assert.IsTrue(page.Sections[1].DisplayName == "Section 1");
                Assert.IsTrue(page.Sections[1].IsExpanded == false);
                Assert.IsTrue(page.Sections[1].ShowDividerLine == false);
                Assert.IsTrue(page.Sections[1].IconAlignment == IconAlignment.Right);
                Assert.IsTrue(page.Sections[1].Columns[0].Controls.Count == 2);
                Assert.IsTrue(page.Sections[1].Columns[1].Controls.Count == 2);

                Assert.IsTrue(page.Sections[2].Type == CanvasSectionTemplate.ThreeColumn);
                Assert.IsTrue(page.Sections[2].ZoneEmphasis == (int)VariantThemeType.None);
                Assert.IsTrue(page.Sections[2].Collapsible == true);
                Assert.IsTrue(page.Sections[2].DisplayName == null);
                Assert.IsTrue(page.Sections[2].IsExpanded == true);
                Assert.IsTrue(page.Sections[2].ShowDividerLine == false);
                Assert.IsTrue(page.Sections[2].IconAlignment == null);
                Assert.IsTrue(page.Sections[2].Columns[0].Controls.Count == 2);
                Assert.IsTrue(page.Sections[2].Columns[1].Controls.Count == 2);
                Assert.IsTrue(page.Sections[2].Columns[2].Controls.Count == 2);

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdatingPageTextDoesNotMakeSectionCollapsible()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("UpdatingPageTextDoesNotMakeSectionCollapsible.aspx");
                newPage.AddSection(CanvasSectionTemplate.OneColumn, 1);
                newPage.AddControl(newPage.NewTextPart("before"), newPage.Sections[0].Columns[0]);

                // Save the page
                await newPage.SaveAsync(pageName);

                Assert.IsFalse(newPage.Sections[0].Collapsible);

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    newPage = (await context2.Web.GetPagesAsync(pageName)).FirstOrDefault();

                    Assert.IsFalse(newPage.Sections[0].Collapsible);

                    // Update text 
                    (newPage.Controls.First() as IPageText).Text = "after";

                    // Update the page
                    await newPage.SaveAsync(pageName);

                    // Load the page again
                    var pages = await context2.Web.GetPagesAsync(pageName);
                    var updatedPage = pages.AsEnumerable().First();

                    Assert.IsTrue(updatedPage.Sections.Count == 1);
                    Assert.IsFalse(updatedPage.Sections[0].Collapsible);

                    // Delete the page
                    await updatedPage.DeleteAsync();                    
                }
            }
        }

        [TestMethod]
        public async Task PageSectionsAndWebPartsCreateTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PageSectionsAndWebPartsCreateTest.aspx");

                // Add all the possible sections 
                page.AddSection(CanvasSectionTemplate.OneColumnFullWidth, 1);

                // Instantiate a default web part
                var imageWebPartComponent = await page.InstantiateDefaultWebPartAsync(DefaultWebPart.Image);

                // Add a text control in each section
                page.AddControl(imageWebPartComponent, page.Sections[0].Columns[0]);

                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.Sections.Count == 1);
                Assert.IsTrue(page.Sections[0].Type == CanvasSectionTemplate.OneColumnFullWidth);
                Assert.IsTrue(page.Sections[0].Columns[0].Controls.Count == 1);
                Assert.IsTrue(page.Sections[0].Columns[0].Controls[0] is IPageWebPart);
                Assert.IsTrue((page.Sections[0].Columns[0].Controls[0] as IPageWebPart).WebPartId == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task VideoEmbedUsingStreamTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("VideoEmbedUsingStreamTest.aspx");

                // Add all the possible sections 
                page.AddSection(CanvasSectionTemplate.OneColumn, 1);

                // Instantiate a default web part ==> this will result in the 
                var videoWebPart = await page.InstantiateDefaultWebPartAsync(DefaultWebPart.VideoEmbed);
                videoWebPart.PropertiesJson = "{\"controlType\":3,\"id\":\"ebf408b9-aeaf-4dd9-8616-41af322085e9\",\"position\":{\"zoneIndex\":1,\"sectionIndex\":1,\"controlIndex\":1,\"layoutIndex\":1},\"webPartId\":\"275c0095-a77e-4f6d-a2a0-6a7626911518\",\"webPartData\":{\"id\":\"275c0095-a77e-4f6d-a2a0-6a7626911518\",\"instanceId\":\"ebf408b9-aeaf-4dd9-8616-41af322085e9\",\"title\":\"Stream\",\"description\":\"Display a Stream video or channel\",\"audiences\":[],\"serverProcessedContent\":{\"htmlStrings\":{},\"searchablePlainTexts\":{},\"imageSources\":{},\"links\":{\"videoSource\":\"https://web.microsoftstream.com/embed/browse\"}},\"dataVersion\":\"1.4\",\"properties\":{\"showInfo\":false,\"captionText\":\"\",\"embedCode\":\"<iframe width='640' height='475' src='https://web.microsoftstream.com/embed/browse?app=SPO&displayMode=buttons&showDescription=true&sort=trending' frameborder='0' allowfullscreen></iframe>\",\"isStream\":true,\"showvideoStart\":false,\"videoStartAt\":\"\",\"channelSortBy\":\"trending\",\"sourceType\":\"BROWSE\",\"thumbnailUrl\":\"\",\"browseSortBy\":\"trending\"}},\"emphasis\":{},\"reservedHeight\":683,\"reservedWidth\":649,\"addedFromPersistedData\":true}";

                // Add a text control in each section
                page.AddControl(videoWebPart, page.Sections[0].Columns[0]);

                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.Sections.Count == 1);
                Assert.IsTrue(page.Sections[0].Type == CanvasSectionTemplate.OneColumn);
                Assert.IsTrue(page.Sections[0].Columns[0].Controls.Count == 1);
                Assert.IsTrue(page.Sections[0].Columns[0].Controls[0] is IPageWebPart);
                Assert.IsTrue((page.Sections[0].Columns[0].Controls[0] as IPageWebPart).WebPartId == page.DefaultWebPartToWebPartId(DefaultWebPart.VideoEmbed));

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        [DataRow(CanvasSectionTemplate.OneColumnVerticalSection, 1)]
        [DataRow(CanvasSectionTemplate.TwoColumnVerticalSection, 2)]
        [DataRow(CanvasSectionTemplate.TwoColumnLeftVerticalSection, 3)]
        [DataRow(CanvasSectionTemplate.TwoColumnRightVerticalSection, 4)]
        [DataRow(CanvasSectionTemplate.ThreeColumnVerticalSection, 5)]
        public async Task VerticalPageSectionsWithEmphasisCreateTest(CanvasSectionTemplate sectionType, int id)
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, id))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("VerticalPageSectionsWithEmphasisCreateTest.aspx");

                // Add all the possible sections 
                page.AddSection(sectionType, 1, VariantThemeType.Neutral, VariantThemeType.Strong);
                page.AddSection(CanvasSectionTemplate.OneColumn, 2, VariantThemeType.Strong);

                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.Sections.Count == 2);
                Assert.IsTrue(page.Sections[0].Type == sectionType);
                Assert.IsTrue(page.Sections[0].ZoneEmphasis == (int)VariantThemeType.Neutral);
                Assert.IsTrue(page.Sections[0].VerticalSectionColumn != null);
                Assert.IsTrue(page.Sections[0].VerticalSectionColumn.VerticalSectionEmphasis == (int)VariantThemeType.Strong);

                Assert.IsTrue(page.Sections[1].Type == CanvasSectionTemplate.OneColumn);
                Assert.IsTrue(page.Sections[1].ZoneEmphasis == (int)VariantThemeType.Strong);

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task VerticalPageSectionsWithEmphasisAndControlsCreateTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("VerticalPageSectionsWithEmphasisAndControlsCreateTest.aspx");

                // Add all the possible sections 
                page.AddSection(CanvasSectionTemplate.TwoColumnVerticalSection, 1, VariantThemeType.Neutral, VariantThemeType.Strong);
                page.AddSection(CanvasSectionTemplate.OneColumn, 2, VariantThemeType.Strong);

                // Add controls
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[1]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[2]);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[1].Columns[0]);

                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.Sections.Count == 2);
                Assert.IsTrue(page.Sections[0].Type == CanvasSectionTemplate.TwoColumnVerticalSection);
                Assert.IsTrue(page.Sections[0].ZoneEmphasis == (int)VariantThemeType.Neutral);
                Assert.IsTrue(page.Sections[0].VerticalSectionColumn != null);
                Assert.IsTrue(page.Sections[0].VerticalSectionColumn.VerticalSectionEmphasis == (int)VariantThemeType.Strong);
                Assert.IsTrue(page.Sections[0].Columns[0].Controls.Count == 1);
                Assert.IsTrue(page.Sections[0].Columns[1].Controls.Count == 1);
                Assert.IsTrue(page.Sections[0].Columns[2].Controls.Count == 1);

                Assert.IsTrue(page.Sections[1].Type == CanvasSectionTemplate.OneColumn);
                Assert.IsTrue(page.Sections[1].ZoneEmphasis == (int)VariantThemeType.Strong);
                Assert.IsTrue(page.Sections[1].Columns[0].Controls.Count == 1);

                // delete the page
                await page.DeleteAsync();
            }
        }

        //[TestMethod]
        //public async Task PageFullWidthSectionOnNonCommunicationSiteTest()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        var page = await context.Web.NewPageAsync();
        //        string pageName = TestCommon.GetPnPSdkTestAssetName("PageFullWidthSectionOnNonCommunicationSiteTest.aspx");

        //        // Add all the possible sections 
        //        page.AddSection(CanvasSectionTemplate.OneColumnFullWidth, 1);
        //        page.AddSection(CanvasSectionTemplate.OneColumn, 2);

        //        bool exceptionThrown = false;
        //        try
        //        {
        //            await page.SaveAsync(pageName);
        //        }
        //        catch (ClientException ex)
        //        {
        //            if ((ex.Error as ClientError).Type == ErrorType.Unsupported)
        //            {
        //                exceptionThrown = true;
        //            }
        //        }

        //        Assert.IsTrue(exceptionThrown);
        //    }
        //}

        [TestMethod]
        [DataRow(CanvasSectionTemplate.OneColumnVerticalSection, 1)]
        [DataRow(CanvasSectionTemplate.TwoColumnVerticalSection, 2)]
        [DataRow(CanvasSectionTemplate.TwoColumnLeftVerticalSection, 3)]
        [DataRow(CanvasSectionTemplate.TwoColumnRightVerticalSection, 4)]
        [DataRow(CanvasSectionTemplate.ThreeColumnVerticalSection, 5)]
        public async Task PageFullWidthSectionMixTest(CanvasSectionTemplate sectionType, int id)
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, id))
            {
                var page = await context.Web.NewPageAsync();

                bool exceptionThrown = false;
                try
                {
                    // Add all the possible sections 
                    page.AddSection(CanvasSectionTemplate.OneColumnFullWidth, 1);
                    page.AddSection(sectionType, 2);
                }
                catch (ClientException ex)
                {
                    if ((ex.Error as ClientError).Type == ErrorType.Unsupported)
                    {
                        exceptionThrown = true;
                    }
                }
                Assert.IsTrue(exceptionThrown);
            }
        }

        [TestMethod]
        [DataRow(CanvasSectionTemplate.OneColumnFullWidth, 1)]
        [DataRow(CanvasSectionTemplate.TwoColumnVerticalSection, 2)]
        [DataRow(CanvasSectionTemplate.TwoColumnLeftVerticalSection, 3)]
        [DataRow(CanvasSectionTemplate.TwoColumnRightVerticalSection, 4)]
        [DataRow(CanvasSectionTemplate.ThreeColumnVerticalSection, 5)]
        public async Task PageVerticalSectionMixTest(CanvasSectionTemplate sectionType, int id)
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, id))
            {
                var page = await context.Web.NewPageAsync();

                bool exceptionThrown = false;
                try
                {
                    // Add all the possible sections 
                    page.AddSection(CanvasSectionTemplate.OneColumnVerticalSection, 1);
                    page.AddSection(sectionType, 2);
                }
                catch (ClientException ex)
                {
                    if ((ex.Error as ClientError).Type == ErrorType.Unsupported)
                    {
                        exceptionThrown = true;
                    }
                }
                Assert.IsTrue(exceptionThrown);
            }
        }

        [TestMethod]
        public async Task DefaultPageTitleTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("DefaultPageTitleTest.aspx");

                // A simple section and text control to the page                
                page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                // Save the page
                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.PageTitle == pageName.Replace(".aspx", ""));

                // delete the page
                await page.DeleteAsync();
            }
        }


        [TestMethod]
        [DataRow("Custom page title", 0)]
        [DataRow("Custom \"page\" 'title'", 1)]
        public async Task CustomPageTitleTest(string pageTitle, int id)
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, id))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("CustomPageTitleTest.aspx");

                // A simple section and text control to the page                
                page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                page.PageTitle = pageTitle;

                // Save the page
                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.PageTitle == pageTitle);

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task MoveControlsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("MoveControlsTest.aspx");

                // Add all the possible sections 
                page.AddSection(CanvasSectionTemplate.OneColumnFullWidth, 1);
                page.AddSection(CanvasSectionTemplate.TwoColumn, 2);
                page.AddSection(CanvasSectionTemplate.ThreeColumn, 3);

                var availableComponents = await page.AvailablePageComponentsAsync();
                var imageWebPartComponent = availableComponents.FirstOrDefault(p => p.Id == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));

                // Add a text control in each section
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[0].Columns[0]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[0].Columns[0]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[1].Columns[1]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[1].Columns[1]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[2].Columns[2]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[2].Columns[2]);
                page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[2].Columns[2]);

                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.Sections.Count == 3);
                Assert.IsTrue(page.Sections[0].Type == CanvasSectionTemplate.OneColumnFullWidth);
                Assert.IsTrue(page.Sections[0].Columns[0].Controls.Count == 2);
                Assert.IsTrue(page.Sections[0].Columns[0].Controls[0] is IPageWebPart);
                Assert.IsTrue((page.Sections[0].Columns[0].Controls[0] as IPageWebPart).WebPartId == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));

                // Move the image web part
                page.Sections[0].Columns[0].Controls[0].Move(page.Sections[1].Columns[0], 1);
                page.Sections[0].Columns[0].Controls[0].Move(page.Sections[1], 2);

                Assert.IsTrue(page.Sections[0].Controls.Count == 0);
                Assert.IsTrue(page.Sections[1].Columns[0].Controls.Count == 2);

                // Move the image web part, setting position as last control in the column
                page.Sections[1].Columns[1].Controls[0].MovePosition(page.Sections[2].Columns[2], 100);

                Assert.IsTrue(page.Sections[2].Columns[2].Controls.Count == 4);

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PageTextWithInlineImageTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PageTextWithInlineImageTest.aspx");
                page.AddSection(CanvasSectionTemplate.TwoColumn, 1);

                // Add text with 3 inline images
                var textPart = page.NewTextPart("");

                // Input validation
                Assert.ThrowsException<ArgumentNullException>(() => { page.GetInlineImage(null, "/sites/prov-2/siteassets/__siteicon__.png"); });
                Assert.ThrowsException<ArgumentNullException>(() => { page.GetInlineImage(textPart, null); });

                var html1 = page.GetInlineImage(textPart, "/sites/prov-2/siteassets/__siteicon__.png");
                var html2 = page.GetInlineImage(textPart, "/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions() { Alignment = PageImageAlignment.Left});
                var html3 = page.GetInlineImage(textPart, "/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions() { Alignment = PageImageAlignment.Right});
                string htmlAdded = $"<p>Before inline images</p>{html1}<p>Post image</p>{html2}<p>Post image</p>{html3}<p>Post image</p>";
                textPart.Text = htmlAdded;
                page.AddControl(textPart, page.Sections[0].Columns[0]);

                // Add simple text
                page.AddControl(page.NewTextPart("Second editor in this column"), page.Sections[0].Columns[0]);

                // Add text with 2 inline images
                var textPart2 = page.NewTextPart("");
                var html21 = page.GetInlineImage(textPart2, "/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions { Alignment = PageImageAlignment.Center});
                var html22 = page.GetInlineImage(textPart2, "/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions { Alignment = PageImageAlignment.Left });
                var html33 = page.GetInlineImage(textPart2, "/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions { Alignment = PageImageAlignment.Left, Height = 50, Width = 50 });
                textPart2.Text = $"<p>Before inline images</p>{html21}<p>Post image</p>{html22}<p>Post image</p>{html33}<p>Post image</p>";
                page.AddControl(textPart2, page.Sections[0].Columns[1]);

                page.AddSection(CanvasSectionTemplate.TwoColumn, 2);

                // Input validation
                Assert.ThrowsException<ArgumentNullException>(() => { page.GetImageWebPart(null); });

                page.AddControl(page.GetImageWebPart("/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions { Alignment = PageImageAlignment.Left }), page.Sections[1].Columns[0]);
                page.AddControl(page.GetImageWebPart("/sites/prov-2/siteassets/__siteicon__.png"), page.Sections[1].Columns[1]);
                page.AddControl(page.GetImageWebPart("/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions { Alignment = PageImageAlignment.Left, Height = 50, Width = 50 }), page.Sections[1].Columns[1]);

                // Persist the page
                await page.SaveAsync(pageName);

                // load the page again and verify
                var pages = await context.Web.GetPagesAsync(pageName);
                var createdPage = pages.First();

                Assert.IsTrue(!string.IsNullOrEmpty((createdPage.Sections[0].Columns[0].Controls[0] as PageWebPart).RichTextEditorInstanceId));

                // Clone the page
                await createdPage.SaveAsync(TestCommon.GetPnPSdkTestAssetName("ClonePageTextWithInlineImageTest.aspx"));

                await page.DeleteAsync();
                await createdPage.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PageTextWithInlineImageWithOpitonsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync(editorType: EditorType.CK4);
                string pageName = TestCommon.GetPnPSdkTestAssetName("PageTextWithInlineImageWithOpitonsTest.aspx");
                page.AddSection(CanvasSectionTemplate.TwoColumn, 1);

                // Add text with 3 inline images
                var textPart = page.NewTextPart("");

                var html1 = page.GetInlineImage(textPart, "/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions() { Link = "https://aka.ms/m365pnp"});
                var html2 = page.GetInlineImage(textPart, "/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions() { Alignment = PageImageAlignment.Left, Link = "https://aka.ms/m365pnp", Caption = "PnP Rocks caption" });
                var html3 = page.GetInlineImage(textPart, "/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions() { Alignment = PageImageAlignment.Right, Link = "https://aka.ms/m365pnp", Caption = "PnP Rocks caption", AlternativeText = "Alternative text" });
                string htmlAdded = $"<p>Before inline images</p>{html1}<p>Post image</p>{html2}<p>Post image</p>{html3}<p>Post image</p>";
                textPart.Text = htmlAdded;
                page.AddControl(textPart, page.Sections[0].Columns[0]);

                // Persist the page
                await page.SaveAsync(pageName);

                // load the page again and verify
                var pages = await context.Web.GetPagesAsync(pageName);
                var createdPage = pages.First();

                Assert.IsTrue(!string.IsNullOrEmpty((createdPage.Sections[0].Columns[0].Controls[0] as PageWebPart).RichTextEditorInstanceId));

                // Clone the page
                await createdPage.SaveAsync(TestCommon.GetPnPSdkTestAssetName("ClonePageTextWithInlineImageWithOpitonsTest.aspx"));

                await page.DeleteAsync();
                await createdPage.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PageTextWithCK5InlineImageWithOptionsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();

                string pageName = TestCommon.GetPnPSdkTestAssetName("PageTextWithCK5InlineImageWithOptionsTest.aspx");
                page.AddSection(CanvasSectionTemplate.TwoColumn, 1);

                // Add text with 3 inline images
                var textPart = page.NewTextPart("");

                var html1 = page.GetInlineImage(textPart, "/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions() { Link = "https://aka.ms/m365pnp"});
                var html2 = page.GetInlineImage(textPart, "/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions() { Alignment = PageImageAlignment.Left, Link = "https://aka.ms/m365pnp", Caption = "PnP Rocks caption", Width = 96, Height = 96, WidthPercentage = 20 });
                var html3 = page.GetInlineImage(textPart, "/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions() { Alignment = PageImageAlignment.Right, Link = "https://aka.ms/m365pnp", Caption = "PnP Rocks caption", AlternativeText = "Alternative text", Width = 96, Height = 96, WidthPercentage = 20 });
                string htmlAdded = $"<p class=\"noSpacingAbove spacingBelow\" data-text-type=\"withSpacing\">Before inline images </p>{html1}<p class=\"noSpacingAbove spacingBelow\" data-text-type=\"withSpacing\">Post image</p>{html2}<p class=\"noSpacingAbove spacingBelow\" data-text-type=\"withSpacing\">Post image</p>{html3}<p class=\"noSpacingAbove spacingBelow\" data-text-type=\"withSpacing\">Post image</p>";
                textPart.Text = htmlAdded;
                page.AddControl(textPart, page.Sections[0].Columns[0]);

                // Persist the page
                await page.SaveAsync(pageName);

                // load the page again and verify
                var pages = await context.Web.GetPagesAsync(pageName);
                var createdPage = pages.First();

                // in CK5 mode there are no extra hidden image web parts anymore
                Assert.IsTrue(createdPage.Controls.Count == 1);

                // Clone the page
                await createdPage.SaveAsync(TestCommon.GetPnPSdkTestAssetName("ClonePageTextWithCK5InlineImageWithOptionsTest.aspx"));

                await page.DeleteAsync();
                await createdPage.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PageTextWithInlineImageWithSpecialCharInTitleTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IPage page = null;
                try
                {
                    page = await context.Web.NewPageAsync();
                    string pageName = TestCommon.GetPnPSdkTestAssetName("PageTextWithInlineImageWithOpitonsTest.aspx");
                    page.AddSection(CanvasSectionTemplate.TwoColumn, 1);

                    // Add text with 1 inline images
                    var textPart = page.NewTextPart("");

                    var html1 = page.GetInlineImage(textPart, "/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions() { Link = "https://aka.ms/m365pnp" });
                    var html2 = page.GetInlineImage(textPart, "/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions() { Alignment = PageImageAlignment.Left, Link = "https://aka.ms/m365pnp", Caption = "Let's PnP Rock" });
                    var html3 = page.GetInlineImage(textPart, "/sites/prov-2/siteassets/__siteicon__.png", new PageImageOptions() { Alignment = PageImageAlignment.Right, Link = "https://aka.ms/m365pnp", Caption = "PnP \"Rocks\" caption", AlternativeText = "Let's PnP Rock" });
                    string htmlAdded = $"<p>Before inline images</p>{html1}<p>Post image</p>{html2}<p>Post image</p>{html3}<p>Post image</p>";
                    textPart.Text = htmlAdded;
                    page.AddControl(textPart, page.Sections[0].Columns[0]);

                    // Persist the page
                    await page.SaveAsync(pageName);

                    // load the page again and verify
                    var pages = await context.Web.GetPagesAsync(pageName);
                    var createdPage = pages.First();

                    Assert.IsTrue(!string.IsNullOrEmpty((createdPage.Sections[0].Columns[0].Controls[0] as PageWebPart).RichTextEditorInstanceId));
                }
                finally
                {
                    await page.DeleteAsync();
                }
            }
        }

        #endregion

        #region Page Header handling
        [TestMethod]
        public async Task NoPageHeaderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("NoPageHeaderTest.aspx");

                // A simple section and text control to the page                
                page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                // Configure the page header
                page.RemovePageHeader();

                Assert.IsTrue(page.PageHeader.LayoutType == PageHeaderLayoutType.NoImage);
                Assert.IsTrue(page.PageHeader.TextAlignment == PageHeaderTitleAlignment.Left);
                Assert.IsTrue(page.PageHeader.ShowTopicHeader == false);
                Assert.IsTrue(string.IsNullOrEmpty(page.PageHeader.TopicHeader));

                // Save the page
                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.PageHeader.LayoutType == PageHeaderLayoutType.NoImage);
                Assert.IsTrue(page.PageHeader.TextAlignment == PageHeaderTitleAlignment.Left);
                Assert.IsTrue(page.PageHeader.ShowTopicHeader == false);
                Assert.IsTrue(string.IsNullOrEmpty(page.PageHeader.TopicHeader));

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task DefaultPageHeaderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("DefaultPageHeaderTest.aspx");

                // A simple section and text control to the page                
                page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                // Configure the page header
                page.SetDefaultPageHeader();

                Assert.IsTrue(page.PageHeader.LayoutType == PageHeaderLayoutType.FullWidthImage);
                Assert.IsTrue(page.PageHeader.TextAlignment == PageHeaderTitleAlignment.Left);
                Assert.IsTrue(page.PageHeader.ShowTopicHeader == false);
                Assert.IsTrue(string.IsNullOrEmpty(page.PageHeader.TopicHeader));

                // Save the page
                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.PageHeader.LayoutType == PageHeaderLayoutType.FullWidthImage);
                Assert.IsTrue(page.PageHeader.TextAlignment == PageHeaderTitleAlignment.Left);
                Assert.IsTrue(page.PageHeader.ShowTopicHeader == false);
                Assert.IsTrue(string.IsNullOrEmpty(page.PageHeader.TopicHeader));

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CustomizeDefaultPageHeaderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("DefaultPageHeaderTest.aspx");

                // A simple section and text control to the page                
                page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                // Configure the page header
                page.SetDefaultPageHeader();
                page.PageHeader.LayoutType = PageHeaderLayoutType.CutInShape;
                page.PageHeader.ShowTopicHeader = true;
                page.PageHeader.TopicHeader = "I'm a topic header";
                page.PageHeader.TextAlignment = PageHeaderTitleAlignment.Center;
                page.PageHeader.ShowPublishDate = true;

                // Save the page
                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.PageHeader.LayoutType == PageHeaderLayoutType.CutInShape);
                Assert.IsTrue(page.PageHeader.ShowTopicHeader == true);
                Assert.IsTrue(page.PageHeader.TopicHeader == "I'm a topic header");
                Assert.IsTrue(page.PageHeader.TextAlignment == PageHeaderTitleAlignment.Center);
                Assert.IsTrue(page.PageHeader.ShowPublishDate = true);

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CustomizeDefaultPageHeaderWithUserTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("CustomizeDefaultPageHeaderWithUserTest.aspx");

                // A simple section and text control to the page                
                page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                // Get current user
                var currentUser = await context.Web.GetCurrentUserAsync();

                // Configure the page header
                page.SetDefaultPageHeader();
                page.PageHeader.LayoutType = PageHeaderLayoutType.CutInShape;
                page.PageHeader.ShowTopicHeader = true;
                page.PageHeader.TopicHeader = "I'm a topic header";
                page.PageHeader.TextAlignment = PageHeaderTitleAlignment.Center;
                page.PageHeader.ShowPublishDate = true;
                page.PageHeader.AuthorByLineId = currentUser.Id;

                // Save the page
                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.PageHeader.LayoutType == PageHeaderLayoutType.CutInShape);
                Assert.IsTrue(page.PageHeader.ShowTopicHeader == true);
                Assert.IsTrue(page.PageHeader.TopicHeader == "I'm a topic header");
                Assert.IsTrue(page.PageHeader.TextAlignment == PageHeaderTitleAlignment.Center);
                Assert.IsTrue(page.PageHeader.ShowPublishDate = true);
                Assert.IsTrue(page.PageHeader.AuthorByLineId == currentUser.Id);

                // delete the page
                await page.DeleteAsync();
            }
        }


        [TestMethod]
        public async Task CustomPageHeaderWithSiteRelativeImageTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("CustomPageHeaderWithSiteRelativeImageTest.aspx");

                // A simple section and text control to the page                
                page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                // Upload image to site assets library
                IFolder parentFolder = await context.Web.Folders.FirstOrDefaultAsync(f => f.Name == "SiteAssets");
                IFile headerImage = await parentFolder.Files.AddAsync("pageheader.jpg", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}pageheader.jpg"), overwrite: true);

                // Configure the page header
                page.SetCustomPageHeader(headerImage.ServerRelativeUrl);
                page.PageHeader.LayoutType = PageHeaderLayoutType.ColorBlock;
                page.PageHeader.ShowTopicHeader = true;
                page.PageHeader.TopicHeader = "I'm a topic header";
                page.PageHeader.TextAlignment = PageHeaderTitleAlignment.Center;
                page.PageHeader.ShowPublishDate = true;

                // Save the page
                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.PageHeader.ImageServerRelativeUrl == headerImage.ServerRelativeUrl);
                Assert.IsTrue(page.PageHeader.TranslateX.HasValue == false);
                Assert.IsTrue(page.PageHeader.TranslateY.HasValue == false);
                Assert.IsTrue(page.PageHeader.LayoutType == PageHeaderLayoutType.ColorBlock);
                Assert.IsTrue(page.PageHeader.ShowTopicHeader == true);
                Assert.IsTrue(page.PageHeader.TopicHeader == "I'm a topic header");
                Assert.IsTrue(page.PageHeader.TextAlignment == PageHeaderTitleAlignment.Center);
                Assert.IsTrue(page.PageHeader.ShowPublishDate = true);
                Assert.IsTrue((page as Page).PageListItem[PageConstants.BannerImageUrlField] is FieldUrlValue);
                Assert.IsFalse(((page as Page).PageListItem[PageConstants.BannerImageUrlField] as FieldUrlValue).Url.Equals("/_layouts/15/images/sitepagethumbnail.png"));
                // delete the page
                await page.DeleteAsync();

                // delete the page header image
                await headerImage.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CustomPageHeaderWithSiteRelativeImageFocalPointsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("CustomPageHeaderWithSiteRelativeImageFocalPointsTest.aspx");

                // A simple section and text control to the page                
                page.AddControl(page.NewTextPart("PnP"), page.DefaultSection.DefaultColumn);

                // Upload image to site assets library
                IFolder parentFolder = await context.Web.Folders.FirstOrDefaultAsync(f => f.Name == "SiteAssets");
                IFile headerImage = await parentFolder.Files.AddAsync("pageheader.jpg", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}pageheader.jpg"), overwrite: true);

                // Configure the page header
                page.SetCustomPageHeader(headerImage.ServerRelativeUrl, 5.3, 6.2);
                page.PageHeader.LayoutType = PageHeaderLayoutType.ColorBlock;
                page.PageHeader.ShowTopicHeader = true;
                page.PageHeader.TopicHeader = "I'm a topic header";
                page.PageHeader.TextAlignment = PageHeaderTitleAlignment.Center;
                page.PageHeader.ShowPublishDate = true;
                page.PageHeader.AlternativeText = "Alternative text for the header image";

                // Save the page
                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.AsEnumerable().First();

                Assert.IsTrue(page.PageHeader.ImageServerRelativeUrl == headerImage.ServerRelativeUrl);
                Assert.IsTrue(page.PageHeader.TranslateX.HasValue == true);
                Assert.IsTrue(page.PageHeader.TranslateX.Value == 5.3);
                Assert.IsTrue(page.PageHeader.TranslateY.HasValue == true);
                Assert.IsTrue(page.PageHeader.TranslateY.Value == 6.2);
                Assert.IsTrue(page.PageHeader.LayoutType == PageHeaderLayoutType.ColorBlock);
                Assert.IsTrue(page.PageHeader.ShowTopicHeader == true);
                Assert.IsTrue(page.PageHeader.TopicHeader == "I'm a topic header");
                Assert.IsTrue(page.PageHeader.TextAlignment == PageHeaderTitleAlignment.Center);
                Assert.IsTrue(page.PageHeader.ShowPublishDate = true);
                Assert.IsTrue(page.PageHeader.AlternativeText == "Alternative text for the header image");

                // delete the page
                await page.DeleteAsync();

                // delete the page header image
                await headerImage.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddFormattedText()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("AddFormattedText.aspx");

                // adding sections to the page
                page.AddSection(CanvasSectionTemplate.OneColumn, 1);

                // Adding text control to the first section, first column
                page.AddControl(page.NewTextPart("PnP <span class=\"fontSizeXLargePlus\"><span class=\"fontColorRed\"><strong>rocks!</strong></span></span>"), page.Sections[0].Columns[0]);

                // Save the page
                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages.Count == 1);
                page = pages.AsEnumerable().First();

                Assert.IsTrue((page.Controls[0] as IPageText).Text == "PnP <span class=\"fontSizeXLargePlus\"><span class=\"fontColorRed\"><strong>rocks!</strong></span></span>");

                // Delete the page
                await page.DeleteAsync();
            }
        }

        #endregion

        #region Page saving

        [TestMethod]
        public async Task OverwriteHomePage()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new sub site to test as we don't want to break the main site home page
                string webTitle = "OverwriteHomePage";
                var addedWeb = await context.Web.Webs.AddAsync(new WebOptions { Title = webTitle, Url = webTitle });

                // Create a context for the newly created web
                using (var context2 = await TestCommon.Instance.CloneAsync(context, addedWeb.Url, 1))
                {
                    // Read the current home page
                    string pageName = "Home.aspx";
                    var pages = await context2.Web.GetPagesAsync(pageName);
                    var homePage = pages.AsEnumerable().First();

                    // Update the home page
                    homePage.ClearPage();
                    homePage.AddSection(CanvasSectionTemplate.ThreeColumn, 1, VariantThemeType.Soft, VariantThemeType.Strong);
                    homePage.AddControl(homePage.NewTextPart("I"), homePage.Sections[0].Columns[0]);
                    homePage.AddControl(homePage.NewTextPart("like"), homePage.Sections[0].Columns[1]);
                    homePage.AddControl(homePage.NewTextPart("PnP"), homePage.Sections[0].Columns[2]);

                    // Save the home page
                    await homePage.SaveAsync(pageName);

                    // Load the page again
                    pages = await context2.Web.GetPagesAsync(pageName);
                    var updatedHomePage = pages.AsEnumerable().First();

                    // Verify the home page was updated
                    Assert.IsTrue(updatedHomePage.Sections.Count == 1);
                    Assert.IsTrue(updatedHomePage.Sections[0].Columns.Count == 3);
                    Assert.IsTrue(updatedHomePage.Sections[0].Controls.Count == 3);
                }

                // Delete the web to cleanup the test artefacts
                await addedWeb.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CreateAndUpdatePage()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("CreateAndUpdatePage.aspx");
                // Save the page
                await newPage.SaveAsync(pageName);

                // Update the page
                newPage.AddSection(CanvasSectionTemplate.ThreeColumn, 1, VariantThemeType.Soft, VariantThemeType.Strong);
                newPage.AddControl(newPage.NewTextPart("I"), newPage.Sections[0].Columns[0]);
                newPage.AddControl(newPage.NewTextPart("like"), newPage.Sections[0].Columns[1]);
                newPage.AddControl(newPage.NewTextPart("PnP"), newPage.Sections[0].Columns[2]);

                // Update the page
                await newPage.SaveAsync(pageName);

                // Load the page again
                var pages = await context.Web.GetPagesAsync(pageName);
                var updatedPage = pages.AsEnumerable().First();

                Assert.IsTrue(updatedPage.Sections.Count == 1);
                Assert.IsTrue(updatedPage.Sections[0].Columns.Count == 3);
                Assert.IsTrue(updatedPage.Sections[0].Controls.Count == 3);

                // Delete the page
                await updatedPage.DeleteAsync();
                // Verify the page exists
                var pages2 = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages2.Count == 0);
            }
        }

        [TestMethod]
        public async Task CreateAndUpdatePageWithSpaceInName()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("CreateAndUpdatePageWith Space InName.aspx");
                // Save the page
                var createdPage = await newPage.SaveAsync(pageName);

                // Update the page
                newPage.AddSection(CanvasSectionTemplate.ThreeColumn, 1, VariantThemeType.Soft, VariantThemeType.Strong);
                newPage.AddControl(newPage.NewTextPart("I"), newPage.Sections[0].Columns[0]);
                newPage.AddControl(newPage.NewTextPart("like"), newPage.Sections[0].Columns[1]);
                newPage.AddControl(newPage.NewTextPart("PnP"), newPage.Sections[0].Columns[2]);

                // Update the page
                await newPage.SaveAsync(createdPage);

                // Load the page again
                var pages = await context.Web.GetPagesAsync(createdPage);
                var updatedPage = pages.AsEnumerable().First();

                Assert.IsTrue(updatedPage.Sections.Count == 1);
                Assert.IsTrue(updatedPage.Sections[0].Columns.Count == 3);
                Assert.IsTrue(updatedPage.Sections[0].Controls.Count == 3);

                // Delete the page
                await updatedPage.DeleteAsync();
                // Verify the page exists
                var pages2 = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages2.Count == 0);
            }
        }

        [TestMethod]
        public async Task CreateAndUpdatePageWithSpecialCharsInName()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("A&B#C.aspx");
                // Save the page
                var createdPage = await newPage.SaveAsync(pageName);

                // Delete the page
                await newPage.DeleteAsync();

                // Verify the page exists
                var pages2 = await context.Web.GetPagesAsync(createdPage);
                Assert.IsTrue(pages2.Count == 0);
            }
        }

        [TestMethod]
        public async Task CreateAndUpdatePageWithReload()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("CreateAndUpdatePageWithReload.aspx");
                // Save the page
                await newPage.SaveAsync(pageName);

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    newPage = (await context2.Web.GetPagesAsync(pageName)).FirstOrDefault();

                    // Update the page
                    newPage.AddSection(CanvasSectionTemplate.ThreeColumn, 1, VariantThemeType.Soft, VariantThemeType.Strong);
                    newPage.AddControl(newPage.NewTextPart("I"), newPage.Sections[0].Columns[0]);
                    newPage.AddControl(newPage.NewTextPart("like"), newPage.Sections[0].Columns[1]);
                    newPage.AddControl(newPage.NewTextPart("PnP"), newPage.Sections[0].Columns[2]);

                    // Update the page
                    await newPage.SaveAsync(pageName);

                    // Load the page again
                    var pages = await context2.Web.GetPagesAsync(pageName);
                    var updatedPage = pages.AsEnumerable().First();

                    Assert.IsTrue(updatedPage.Sections.Count == 1);
                    Assert.IsTrue(updatedPage.Sections[0].Columns.Count == 3);
                    Assert.IsTrue(updatedPage.Sections[0].Controls.Count == 3);

                    // Delete the page
                    await updatedPage.DeleteAsync();
                    // Verify the page exists
                    var pages2 = await context2.Web.GetPagesAsync(pageName);
                    Assert.IsTrue(pages2.Count == 0);
                }
            }
        }

        [TestMethod]
        public async Task CreateAndUpdatePageWithReloadWithoutPageName()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("CreateAndUpdatePageWithReloadWithoutPageName.aspx");
                // Save the page
                await newPage.SaveAsync(pageName);

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    newPage = (await context2.Web.GetPagesAsync(pageName)).FirstOrDefault();

                    // Update the page
                    newPage.AddSection(CanvasSectionTemplate.ThreeColumn, 1, VariantThemeType.Soft, VariantThemeType.Strong);
                    newPage.AddControl(newPage.NewTextPart("I"), newPage.Sections[0].Columns[0]);
                    newPage.AddControl(newPage.NewTextPart("like"), newPage.Sections[0].Columns[1]);
                    newPage.AddControl(newPage.NewTextPart("PnP"), newPage.Sections[0].Columns[2]);

                    // Check Folder and Name properties
                    Assert.IsTrue(newPage.Name == pageName);
                    Assert.IsTrue(newPage.Folder == "");

                    // Update the page without passing the filename on save
                    await newPage.SaveAsync();

                    // Load the page again
                    var pages = await context2.Web.GetPagesAsync(pageName);
                    var updatedPage = pages.AsEnumerable().First();

                    Assert.IsTrue(updatedPage.Sections.Count == 1);
                    Assert.IsTrue(updatedPage.Sections[0].Columns.Count == 3);
                    Assert.IsTrue(updatedPage.Sections[0].Controls.Count == 3);

                    // Delete the page
                    await updatedPage.DeleteAsync();
                    // Verify the page exists
                    var pages2 = await context2.Web.GetPagesAsync(pageName);
                    Assert.IsTrue(pages2.Count == 0);
                }
            }
        }

        [TestMethod]
        public async Task CreateAndUpdatePageViaPageFile()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("CreateAndUpdatePageViaPageFile.aspx");

                newPage.AddSection(CanvasSectionTemplate.ThreeColumn, 1, VariantThemeType.Soft, VariantThemeType.Strong);
                newPage.AddControl(newPage.NewTextPart("I"), newPage.Sections[0].Columns[0]);
                newPage.AddControl(newPage.NewTextPart("like"), newPage.Sections[0].Columns[1]);
                newPage.AddControl(newPage.NewTextPart("PnP"), newPage.Sections[0].Columns[2]);

                // Save the page
                await newPage.SaveAsync(pageName);

                // Load the Page File
                var pageFile = await newPage.GetPageFileAsync(p => p.UniqueId, p => p.ListId, p => p.ListItemAllFields);

                pageFile.ListItemAllFields["ContentTypeId"] = PageConstants.ModernArticlePage;
                await pageFile.ListItemAllFields.SystemUpdateAsync();


                // Load the page again
                var pages = await context.Web.GetPagesAsync(pageName);
                var updatedPage = pages.AsEnumerable().First();

                Assert.IsTrue(updatedPage.Sections.Count == 1);
                Assert.IsTrue(updatedPage.Sections[0].Columns.Count == 3);
                Assert.IsTrue(updatedPage.Sections[0].Controls.Count == 3);

                // Delete the page
                await updatedPage.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CreateAndUpdatePageAuthorEditorCreatedModified()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("CreateAndUpdatePageAuthorEditorCreatedModified.aspx");

                newPage.AddSection(CanvasSectionTemplate.ThreeColumn, 1, VariantThemeType.Soft, VariantThemeType.Strong);
                newPage.AddControl(newPage.NewTextPart("I"), newPage.Sections[0].Columns[0]);
                newPage.AddControl(newPage.NewTextPart("like"), newPage.Sections[0].Columns[1]);
                newPage.AddControl(newPage.NewTextPart("PnP"), newPage.Sections[0].Columns[2]);

                // Save the page
                await newPage.SaveAsync(pageName);

                // Load the Page File
                var pageFile = await newPage.GetPageFileAsync(p => p.ListItemAllFields);
                
                // load the current user
                var currentUser = await context.Web.GetCurrentUserAsync();
                var newDate = new DateTime(2020, 10, 20);

                // Load the Author and Editor fields                
                var author = newPage.PagesLibrary.Fields.AsRequested().FirstOrDefault(p => p.InternalName == "Author");
                var editor = newPage.PagesLibrary.Fields.AsRequested().FirstOrDefault(p => p.InternalName == "Editor");

                pageFile.ListItemAllFields["Author"] = author.NewFieldUserValue(currentUser);
                pageFile.ListItemAllFields["Editor"] = editor.NewFieldUserValue(currentUser);
                pageFile.ListItemAllFields["Created"] = newDate;
                pageFile.ListItemAllFields["Modified"] = newDate;
                await pageFile.ListItemAllFields.UpdateOverwriteVersionAsync();

                // Load the page again
                var pages = await context.Web.GetPagesAsync(pageName);
                var updatedPage = pages.AsEnumerable().First();

                pageFile = await updatedPage.GetPageFileAsync(p => p.ListItemAllFields);
                
                Assert.IsTrue(((DateTime)pageFile.ListItemAllFields["Created"]).Year == newDate.Year);
                Assert.IsTrue(((DateTime)pageFile.ListItemAllFields["Created"]).Month == newDate.Month);
                Assert.IsTrue(((DateTime)pageFile.ListItemAllFields["Modified"]).Year == newDate.Year);
                Assert.IsTrue(((DateTime)pageFile.ListItemAllFields["Modified"]).Month == newDate.Month);

                // Delete the page
                await updatedPage.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task SavePageAsTemplate()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("SavePageAsTemplate.aspx");
                // Save the page
                await newPage.SaveAsTemplateAsync(pageName);

                // Load the template page again as regular page
                var pages = await context.Web.GetPagesAsync(pageName);
                var templatePage = pages.AsEnumerable().First();

                var templateFolder = await templatePage.GetTemplatesFolderAsync();
                var pageFolder = templatePage.Folder;

                Assert.AreEqual(templateFolder, pageFolder);

                // Delete the page
                await templatePage.DeleteAsync();

            }
        }

        [TestMethod]
        public async Task SavePageAsTemplateInNewWeb()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create a new sub site to test as we don't want to break the main site home page
                string webTitle = "DutchTemplateWeb";
                var addedWeb = await context.Web.Webs.AddAsync(new WebOptions { Title = webTitle, Url = webTitle, Language = 1043 });

                // Create a context for the newly created web
                using (var context2 = await TestCommon.Instance.CloneAsync(context, addedWeb.Url, 1))
                {
                    var newPage = await context2.Web.NewPageAsync();
                    string pageName = TestCommon.GetPnPSdkTestAssetName("SavePageAsTemplate.aspx");
                    // Save the page
                    newPage.SaveAsTemplate(pageName);

                    // Load the template page again as regular page
                    var pages = await context2.Web.GetPagesAsync(pageName);
                    var templatePage = pages.AsEnumerable().First();

                    var templateFolder = templatePage.GetTemplatesFolder();
                    var pageFolder = templatePage.Folder;

                    Assert.AreEqual(templateFolder, pageFolder);
                }

                // Delete the web to cleanup the test artefacts
                await addedWeb.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task SavePageWithDataAsTemplate()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                newPage.AddSection(CanvasSectionTemplate.TwoColumn, 1);
                newPage.AddControl(newPage.NewTextPart("this page rocks!"), newPage.Sections[0].Columns[0]);

                string templatePageName = TestCommon.GetPnPSdkTestAssetName("SavePageWithDataAsTemplate.aspx");
                // Save the page
                await newPage.SaveAsTemplateAsync(templatePageName);

                // Load the template page again as regular page
                var pages = await context.Web.GetPagesAsync(templatePageName);
                var templatePage = pages.AsEnumerable().First();

                // Create new page from this template
                string pageName = TestCommon.GetPnPSdkTestAssetName("FromTemplate.aspx");
                (templatePage.Sections[0].Controls[0] as IPageText).Text = "Updated content";
                templatePage.Save(pageName);

                pages = await context.Web.GetPagesAsync(TestCommon.GetPnPSdkTestAssetName(""));
                var fromTemplatePage = pages.AsEnumerable().FirstOrDefault(p => p.Name == pageName);
                templatePage = pages.AsEnumerable().FirstOrDefault(p => p.Name == templatePageName);

                Assert.IsTrue((fromTemplatePage as Page).PageListItem["Description"].ToString() == "Updated content");

                // Delete the pages
                await templatePage.DeleteAsync();
                await fromTemplatePage.DeleteAsync();
            }
        }

        #endregion

        #region Page publishing and promotion/demotion
        [TestMethod]
        public async Task PublishPage()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PublishPage.aspx");

                // A simple section and text control to the page                
                page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                // Save the page
                await page.SaveAsync(pageName);

                var pageFile = await page.GetPageFileAsync(p => p.Level);
                // Now that co-auth has been rolled out the default level is checkout and not draft
                Assert.IsTrue(pageFile.Level == PublishedStatus.Checkout);

                await page.PublishAsync();

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages.Count == 1);
                page = pages.AsEnumerable().First();

                pageFile = await page.GetPageFileAsync(p => p.Level);
                Assert.IsTrue(pageFile.Level == PublishedStatus.Published);

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        [DoNotParallelize()]
        public async Task PublishPage_NoMinorVersion()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var libraries = await context.Web.Lists.QueryProperties(new Expression<Func<IList, object>>[] { p => p.Title, p => p.TemplateType, p => p.EnableFolderCreation,
                        p => p.EnableMinorVersions, p => p.EnableModeration, p => p.EnableVersioning,p=>p.ForceCheckout, p => p.MaxVersionLimit, p => p.MinorVersionLimit })
                                                       .Where(p => p.TemplateType == ListTemplateType.WebPageLibrary)
                                                       .ToListAsync()
                                                       .ConfigureAwait(false);
                if (libraries.Count == 1)
                {
                    //configure PAges Library for Testcase
                    IList pagesLibrary = libraries.First();
                    bool initialEnableVersioning = pagesLibrary.EnableVersioning;
                    bool initialEnableMinorVersions = pagesLibrary.EnableMinorVersions;
                    bool initialEnableModeration = pagesLibrary.EnableModeration;
                    bool initialForceCheckout = pagesLibrary.ForceCheckout;
                    int initialMaxVersionLimit = pagesLibrary.MaxVersionLimit;
                    int initialMinorVersionLimit = pagesLibrary.MinorVersionLimit;
                    pagesLibrary.EnableVersioning = true;
                    pagesLibrary.EnableMinorVersions = false;
                    pagesLibrary.EnableModeration = false;
                    pagesLibrary.ForceCheckout = false;
                    await pagesLibrary.UpdateAsync();

                    var page = await context.Web.NewPageAsync();
                    string pageName = TestCommon.GetPnPSdkTestAssetName("PublishPage.aspx");

                    // A simple section and text control to the page                
                    page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                    page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                    // Save the page
                    await page.SaveAsync(pageName);

                    var pageFile = await page.GetPageFileAsync(p => p.Level);
                    //as we have no MinorVersions it should be Published
                    Assert.IsTrue(pageFile.Level == PublishedStatus.Published);

                    //call publish again should not fail
                    await page.PublishAsync();

                    // load page again
                    var pages = await context.Web.GetPagesAsync(pageName);
                    Assert.IsTrue(pages.Count == 1);
                    page = pages.AsEnumerable().First();

                    pageFile = await page.GetPageFileAsync(p => p.Level);
                    Assert.IsTrue(pageFile.Level == PublishedStatus.Published);

                    // delete the page
                    await page.DeleteAsync();

                    //revert SitePages Library settings
                    if (initialEnableVersioning)
                        pagesLibrary.MaxVersionLimit = initialMaxVersionLimit;
                    if (initialEnableMinorVersions)
                        pagesLibrary.MinorVersionLimit = initialMinorVersionLimit;
                    pagesLibrary.EnableVersioning = initialEnableVersioning;
                    pagesLibrary.EnableMinorVersions = initialEnableMinorVersions;
                    pagesLibrary.EnableModeration = initialEnableModeration;
                    pagesLibrary.ForceCheckout = initialForceCheckout;
                    await pagesLibrary.UpdateAsync();
                }

            }
        }

        [TestMethod]
        [DoNotParallelize()]
        public async Task PublishPage_ForceCheckout_NoMinorVersion()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var libraries = await context.Web.Lists.QueryProperties(new Expression<Func<IList, object>>[] { p => p.Title, p => p.TemplateType, p => p.EnableFolderCreation,
                        p => p.EnableMinorVersions, p => p.EnableModeration, p => p.EnableVersioning,p=>p.ForceCheckout, p => p.MaxVersionLimit, p => p.MinorVersionLimit })
                                                       .Where(p => p.TemplateType == ListTemplateType.WebPageLibrary)
                                                       .ToListAsync()
                                                       .ConfigureAwait(false);
                if (libraries.Count == 1)
                {
                    //configure PAges Library for Testcase
                    IList pagesLibrary = libraries.First();
                    bool initialEnableVersioning = pagesLibrary.EnableVersioning;
                    bool initialEnableMinorVersions = pagesLibrary.EnableMinorVersions;
                    bool initialEnableModeration = pagesLibrary.EnableModeration;
                    bool initialForceCheckout = pagesLibrary.ForceCheckout;
                    int initialMaxVersionLimit = pagesLibrary.MaxVersionLimit;
                    int initialMinorVersionLimit = pagesLibrary.MinorVersionLimit;
                    pagesLibrary.EnableVersioning = true;
                    pagesLibrary.EnableMinorVersions = false;
                    pagesLibrary.EnableModeration = false;
                    pagesLibrary.ForceCheckout = true;
                    await pagesLibrary.UpdateAsync();

                    var page = await context.Web.NewPageAsync();
                    string pageName = TestCommon.GetPnPSdkTestAssetName("PublishPage.aspx");

                    // A simple section and text control to the page                
                    page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                    page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                    // Save the page
                    await page.SaveAsync(pageName);

                    var pageFile = await page.GetPageFileAsync(p => p.Level, p => p.CheckOutType);
                    Assert.AreNotEqual(CheckOutType.None, pageFile.CheckOutType);
                    //as we have no MinorVersions but ForceCheckout it should not be Published
                    Assert.IsTrue(pageFile.Level != PublishedStatus.Published);

                    //call publish again should not fail
                    await page.PublishAsync("TEST CHECK IN");

                    // load page again
                    var pages = await context.Web.GetPagesAsync(pageName);
                    Assert.IsTrue(pages.Count == 1);
                    page = pages.AsEnumerable().First();

                    pageFile = await page.GetPageFileAsync(p => p.Level, p => p.CheckOutType, p => p.CheckInComment);
                    Assert.AreEqual(CheckOutType.None, pageFile.CheckOutType);
                    Assert.IsTrue(pageFile.Level == PublishedStatus.Published);
                    Assert.AreEqual("TEST CHECK IN", pageFile.CheckInComment);

                    // delete the page
                    await page.DeleteAsync();

                    //revert SitePages Library settings
                    if (initialEnableVersioning)
                        pagesLibrary.MaxVersionLimit = initialMaxVersionLimit;
                    if (initialEnableMinorVersions)
                        pagesLibrary.MinorVersionLimit = initialMinorVersionLimit;
                    pagesLibrary.EnableVersioning = initialEnableVersioning;
                    pagesLibrary.EnableMinorVersions = initialEnableMinorVersions;
                    pagesLibrary.EnableModeration = initialEnableModeration;
                    pagesLibrary.ForceCheckout = initialForceCheckout;
                    await pagesLibrary.UpdateAsync();
                }

            }
        }

        [TestMethod]
        [DoNotParallelize()]
        public async Task PublishPage_ForceCheckout_EnabledModeration_NoMinorVersion()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var libraries = await context.Web.Lists.QueryProperties(new Expression<Func<IList, object>>[] { p => p.Title, p => p.TemplateType, p => p.EnableFolderCreation,
                        p => p.EnableMinorVersions, p => p.EnableModeration, p => p.EnableVersioning,p=>p.ForceCheckout, p => p.MaxVersionLimit, p => p.MinorVersionLimit })
                                                       .Where(p => p.TemplateType == ListTemplateType.WebPageLibrary)
                                                       .ToListAsync()
                                                       .ConfigureAwait(false);
                if (libraries.Count == 1)
                {
                    //configure PAges Library for Testcase
                    IList pagesLibrary = libraries.First();
                    bool initialEnableVersioning = pagesLibrary.EnableVersioning;
                    bool initialEnableMinorVersions = pagesLibrary.EnableMinorVersions;
                    bool initialEnableModeration = pagesLibrary.EnableModeration;
                    bool initialForceCheckout = pagesLibrary.ForceCheckout;
                    int initialMaxVersionLimit = pagesLibrary.MaxVersionLimit;
                    int initialMinorVersionLimit = pagesLibrary.MinorVersionLimit;
                    pagesLibrary.EnableVersioning = true;
                    pagesLibrary.EnableMinorVersions = false;
                    pagesLibrary.EnableModeration = true;
                    pagesLibrary.ForceCheckout = true;
                    await pagesLibrary.UpdateAsync();

                    var page = await context.Web.NewPageAsync();
                    string pageName = TestCommon.GetPnPSdkTestAssetName("PublishPage.aspx");

                    // A simple section and text control to the page                
                    page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                    page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                    // Save the page
                    await page.SaveAsync(pageName);

                    var pageFile = await page.GetPageFileAsync(p => p.Level, p => p.CheckOutType, p => p.ListItemAllFields);
                    Assert.AreNotEqual(CheckOutType.None, pageFile.CheckOutType);
                    //as we have no MinorVersions but ForceCheckout it should not be Published
                    Assert.IsTrue(pageFile.Level != PublishedStatus.Published);
                    //should not be approved
                    Assert.AreNotEqual("0", pageFile.ListItemAllFields["_ModerationStatus"].ToString());

                    //call publish again should not fail
                    await page.PublishAsync("TEST CHECK IN AND APPROVE");

                    // load page again
                    var pages = await context.Web.GetPagesAsync(pageName);
                    Assert.IsTrue(pages.Count == 1);
                    page = pages.AsEnumerable().First();

                    pageFile = await page.GetPageFileAsync(p => p.Level, p => p.CheckOutType, p => p.CheckInComment, p => p.ListItemAllFields);
                    Assert.AreEqual(CheckOutType.None, pageFile.CheckOutType);
                    Assert.IsTrue(pageFile.Level == PublishedStatus.Published);
                    Assert.AreEqual("TEST CHECK IN AND APPROVE", pageFile.CheckInComment);
                    Assert.AreEqual("0", pageFile.ListItemAllFields["_ModerationStatus"].ToString());
                    Assert.AreEqual("TEST CHECK IN AND APPROVE", pageFile.ListItemAllFields["_ModerationComments"].ToString());

                    // delete the page
                    await page.DeleteAsync();

                    //revert SitePages Library settings
                    if (initialEnableVersioning)
                        pagesLibrary.MaxVersionLimit = initialMaxVersionLimit;
                    if (initialEnableMinorVersions)
                        pagesLibrary.MinorVersionLimit = initialMinorVersionLimit;
                    pagesLibrary.EnableVersioning = initialEnableVersioning;
                    pagesLibrary.EnableMinorVersions = initialEnableMinorVersions;
                    pagesLibrary.EnableModeration = initialEnableModeration;
                    pagesLibrary.ForceCheckout = initialForceCheckout;
                    await pagesLibrary.UpdateAsync();
                }

            }
        }

        [TestMethod]
        [DoNotParallelize()]
        public async Task PublishPage_MajorAndMinorVersion_ForceCheckout_EnabledModeration()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var libraries = await context.Web.Lists.QueryProperties(new Expression<Func<IList, object>>[] { p => p.Title, p => p.TemplateType, p => p.EnableFolderCreation,
                        p => p.EnableMinorVersions, p => p.EnableModeration, p => p.EnableVersioning,p=>p.ForceCheckout, p => p.MaxVersionLimit, p => p.MinorVersionLimit })
                                                       .Where(p => p.TemplateType == ListTemplateType.WebPageLibrary)
                                                       .ToListAsync()
                                                       .ConfigureAwait(false);
                if (libraries.Count == 1)
                {
                    //configure PAges Library for Testcase
                    IList pagesLibrary = libraries.First();
                    bool initialEnableVersioning = pagesLibrary.EnableVersioning;
                    bool initialEnableMinorVersions = pagesLibrary.EnableMinorVersions;
                    bool initialEnableModeration = pagesLibrary.EnableModeration;
                    bool initialForceCheckout = pagesLibrary.ForceCheckout;
                    int initialMaxVersionLimit = pagesLibrary.MaxVersionLimit;
                    int initialMinorVersionLimit = pagesLibrary.MinorVersionLimit;
                    pagesLibrary.EnableVersioning = true;
                    pagesLibrary.MaxVersionLimit = 500;
                    pagesLibrary.EnableMinorVersions = true;
                    pagesLibrary.MinorVersionLimit = 0;
                    pagesLibrary.EnableModeration = true;
                    pagesLibrary.ForceCheckout = true;
                    await pagesLibrary.UpdateAsync();

                    var page = await context.Web.NewPageAsync();
                    string pageName = TestCommon.GetPnPSdkTestAssetName("PublishPage.aspx");

                    // A simple section and text control to the page                
                    page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                    page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                    // Save the page
                    await page.SaveAsync(pageName);

                    var pageFile = await page.GetPageFileAsync(p => p.Level, p => p.CheckOutType, p => p.ListItemAllFields);

                    Assert.AreNotEqual(CheckOutType.None, pageFile.CheckOutType);
                    //as we have no MinorVersions but ForceCheckout it should not be Published
                    Assert.IsTrue(pageFile.Level != PublishedStatus.Published);
                    //should not be approved
                    Assert.AreNotEqual("0", pageFile.ListItemAllFields["_ModerationStatus"].ToString());

                    //call publish again should not fail
                    await page.PublishAsync("TEST CHECK IN AND APPROVE");

                    // load page again
                    var pages = await context.Web.GetPagesAsync(pageName);
                    Assert.IsTrue(pages.Count == 1);
                    page = pages.AsEnumerable().First();

                    pageFile = await page.GetPageFileAsync(p => p.Level, p => p.CheckOutType, p => p.CheckInComment, p => p.ListItemAllFields);
                    Assert.AreEqual(CheckOutType.None, pageFile.CheckOutType);
                    Assert.IsTrue(pageFile.Level == PublishedStatus.Published);
                    Assert.AreEqual("0", pageFile.ListItemAllFields["_ModerationStatus"].ToString());
                    Assert.AreEqual("TEST CHECK IN AND APPROVE", pageFile.ListItemAllFields["_ModerationComments"].ToString());

                    // delete the page
                    await page.DeleteAsync();

                    //revert SitePages Library settings
                    if (initialEnableVersioning)
                        pagesLibrary.MaxVersionLimit = initialMaxVersionLimit;
                    if (initialEnableMinorVersions)
                        pagesLibrary.MinorVersionLimit = initialMinorVersionLimit;
                    pagesLibrary.EnableVersioning = initialEnableVersioning;
                    pagesLibrary.EnableMinorVersions = initialEnableMinorVersions;
                    pagesLibrary.EnableModeration = initialEnableModeration;
                    pagesLibrary.ForceCheckout = initialForceCheckout;
                    await pagesLibrary.UpdateAsync();
                }

            }
        }

        [TestMethod]
        public async Task PromoteAsNewsArticle()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PromoteAsNewsArticle.aspx");

                // A simple section and text control to the page                
                page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                // Save the page
                await page.SaveAsync(pageName);

                // promote as news article
                await page.PromoteAsNewsArticleAsync();

                // first publish the page
                await page.PublishAsync();

                // load page again
                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    var pages = await context2.Web.GetPagesAsync(pageName);
                    Assert.IsTrue(pages.Count == 1);
                    page = pages.AsEnumerable().First();

                    Assert.IsTrue(((page as Page).PageListItem[PageConstants.PromotedStateField]).ToString() == ((int)PromotedState.Promoted).ToString());

                    Assert.IsTrue(page.Sections[0].Type == CanvasSectionTemplate.OneColumn);
                    Assert.IsTrue(page.Sections[0].Columns[0].Controls.Count == 1);
                    Assert.IsTrue(page.Sections[0].Columns[0].Controls[0] is IPageText);
                    Assert.IsTrue((page.Sections[0].Columns[0].Controls[0] as IPageText).Text == "PnP");

                    // delete the page
                    await page.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task DemoteAsNewsArticle()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("DemoteAsNewsArticle.aspx");

                // A simple section and text control to the page                
                page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                // Save the page
                await page.SaveAsync(pageName);

                // first publish the page
                await page.PublishAsync();

                // promote as news article
                await page.PromoteAsNewsArticleAsync();

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages.Count == 1);
                page = pages.AsEnumerable().First();

                Assert.IsTrue(((page as Page).PageListItem[PageConstants.PromotedStateField]).ToString() == ((int)PromotedState.Promoted).ToString());

                // demote as news article
                await page.DemoteNewsArticleAsync();

                Assert.IsTrue(((page as Page).PageListItem[PageConstants.PromotedStateField]).ToString() == ((int)PromotedState.NotPromoted).ToString());

                // delete the page
                await page.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PromoteAsHomePage()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                //Load the root folder before
                string welcomePageBefore = (await context.Web.GetAsync(p => p.WelcomePage)).WelcomePage;

                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PromoteAsHomePage.aspx");

                // A simple section and text control to the page                
                page.AddSection(CanvasSectionTemplate.OneColumn, 1);
                page.AddControl(page.NewTextPart("PnP"), page.Sections[0].Columns[0]);

                // Save the page
                await page.SaveAsync(pageName);

                // promote as news article
                await page.PromoteAsHomePageAsync();

                // Load the page file
                var pageFile = await page.GetPageFileAsync(p => p.ServerRelativeUrl);
                // Load the web root folder
                var webAfter = await context.Web.GetAsync(p => p.RootFolder);
                string welcomePageAfter = (await context.Web.GetAsync(p => p.WelcomePage)).WelcomePage;

                Assert.IsTrue(welcomePageAfter != welcomePageBefore);

                // set back the original page as home page
                webAfter.RootFolder.WelcomePage = welcomePageBefore;
                await webAfter.RootFolder.UpdateAsync();

                // delete the page
                await page.DeleteAsync();
            }
        }

        #endregion

        #region Page likes and comments handling

        [TestMethod]
        public async Task DisableEnablePageComments()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("DisableEnablePageComments.aspx");

                // Save the page
                await newPage.SaveAsync(pageName);

                var commentsDisabled = await newPage.AreCommentsDisabledAsync();
                Assert.IsFalse(commentsDisabled);

                // disable comments
                await newPage.DisableCommentsAsync();

                // Load the page again
                var pages = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages.Count == 1);
                newPage = pages.AsEnumerable().First();

                var commentsDisabled2 = await newPage.AreCommentsDisabledAsync();
                Assert.IsTrue(commentsDisabled2);

                // enabled comments again
                await newPage.EnableCommentsAsync();

                // Delete the page
                await newPage.DeleteAsync();
            }
        }
        
        [TestMethod]
        public async Task LikeUnLikePage()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("LikeUnLikePage.aspx");

                // Save the page
                await newPage.SaveAsync(pageName);

                // Publish the page, required before it can be liked
                newPage.Publish();

                // Like the page
                newPage.Like();

                // Get a list of users who liked this page
                var pageLikeInformation = newPage.GetLikedByInformation();

                Assert.IsTrue(pageLikeInformation != null);
                Assert.IsTrue(pageLikeInformation.IsLikedByUser == true);
                Assert.IsTrue(pageLikeInformation.LikeCount == "1");
                Assert.IsTrue(pageLikeInformation.LikedBy.Length == 1);

                var firstUserThatLikedThePage = pageLikeInformation.LikedBy.AsRequested().First();

                Assert.IsTrue(firstUserThatLikedThePage.Id > 0);
                Assert.IsTrue(!string.IsNullOrEmpty(firstUserThatLikedThePage.LoginName));
                Assert.IsTrue(!string.IsNullOrEmpty(firstUserThatLikedThePage.Mail));
                Assert.IsTrue(!string.IsNullOrEmpty(firstUserThatLikedThePage.Name));
                Assert.IsTrue(firstUserThatLikedThePage.CreationDate < DateTime.Now);

                // Unlike the page
                newPage.Unlike();

                // Get a list of users who liked this page
                pageLikeInformation = newPage.GetLikedByInformation();

                Assert.IsTrue(pageLikeInformation != null);
                Assert.IsTrue(pageLikeInformation.IsLikedByUser == false);
                Assert.IsTrue(pageLikeInformation.LikeCount == "0");
                Assert.IsTrue(pageLikeInformation.LikedBy.Length == 0);

                // Delete the page
                await newPage.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PageCommentingTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PageCommentingTest.aspx");

                // Save the page
                await newPage.SaveAsync(pageName);

                // Publish the page, required before it can be liked
                newPage.Publish();

                // Get Page comments                
                var comments = newPage.GetComments();
                Assert.IsTrue(comments.Length == 0);

                // Add a new comment with an at mentioning
                var currentUser = await context.Web.GetCurrentUserAsync();

                var addedComment = comments.Add($"This is great {comments.GetAtMentioningString("Bert", currentUser.UserPrincipalName)}!");

                // verify exception handling of GetAtMentioningString
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    comments.GetAtMentioningString(null, currentUser.UserPrincipalName);
                });

                Assert.ThrowsException<ArgumentException>(() =>
                {
                    comments.GetAtMentioningString("Bert", null);
                });

                // Like the added comment
                addedComment.Like();

                // Add a reply
                var addedReply = addedComment.Replies.Add("this is a reply");

                // Like the reply
                addedReply.Like();

                comments = newPage.GetComments();
                Assert.IsTrue(comments.Length == 1);

                var firstAtMention = comments.AsRequested().First().Mentions.AsRequested().First();
                // loginName: i:0#.f|membership|bert.jansen@bertonline.onmicrosoft.com
                Assert.IsTrue(firstAtMention.LoginName.Split('|')[2] == currentUser.UserPrincipalName);

                // Delete the page
                await newPage.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PageThreadedCommentingTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IPage newPage = null;
                try
                {
                    newPage = await context.Web.NewPageAsync();
                    string pageName = TestCommon.GetPnPSdkTestAssetName("PageThreadedCommentingTest.aspx");

                    // Save the page
                    await newPage.SaveAsync(pageName);

                    // Publish the page, required before it can be liked
                    newPage.Publish();

                    // Get Page comments                
                    var comments = newPage.GetComments();
                    Assert.IsTrue(comments.Length == 0);

                    // Add a new comment with an at mentioning
                    var currentUser = await context.Web.GetCurrentUserAsync();

                    var addedComment = comments.Add($"This is great {comments.GetAtMentioningString("Bert", currentUser.UserPrincipalName)}!");

                    // Like the added comment
                    addedComment.Like();

                    // Add a reply
                    var addedReply = addedComment.Replies.Add("this is a reply");

                    // Like the reply
                    addedReply.Like();

                    // Verify comment loading when the page was not reloaded after comments were added
                    comments = newPage.GetComments(p => p.Author,
                                                   p => p.Mentions,
                                                   p => p.Text,
                                                   p => p.ReplyCount,
                                                   p => p.Replies);

                    Assert.IsTrue(comments.Length == 1);

                    var firstAtMention = comments.AsRequested().First().Mentions.AsRequested().First();
                    // loginName: i:0#.f|membership|bert.jansen@bertonline.onmicrosoft.com
                    Assert.IsTrue(firstAtMention.LoginName.Split('|')[2] == currentUser.UserPrincipalName);

                    // Check if the replies are loaded
                    Assert.IsTrue(comments.AsRequested().First().ReplyCount == 1);
                    Assert.IsTrue(comments.AsRequested().First().Replies.AsRequested().First().Text == "this is a reply");
                }
                finally
                {
                    // Delete the page
                    await newPage.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task PageCommentingTestOnlyReturn30Comments()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IPage newPage = null;
                try
                {
                    newPage = await context.Web.NewPageAsync();
                    string pageName = TestCommon.GetPnPSdkTestAssetName("PageCommentingTestOnlyReturn30Comments.aspx");

                    // Save the page
                    await newPage.SaveAsync(pageName);

                    // Publish the page, required before it can be liked
                    newPage.Publish();

                    // Get Page comments                
                    var comments = newPage.GetComments();
                    Assert.IsTrue(comments.Length == 0);

                    var noCommentsAdded = 45;

                    foreach (var i in Enumerable.Range(1, noCommentsAdded))
                    {
                        // Add a comment
                        await comments.AddBatchAsync($"Comment #: {i} added by unit test");
                    }

                    await context.ExecuteAsync();

                    comments = newPage.GetComments();
                    // Expecting 45 but only 30 is returned. 
                    Assert.IsTrue(comments.Length == noCommentsAdded);

                    comments = newPage.GetComments(p => p.Author,
                                                   p => p.Text,
                                                   p => p.ReplyCount,
                                                   p => p.CreatedDate,
                                                   p => p.Replies);
                    Assert.IsTrue(comments.Length == noCommentsAdded);
                }
                finally
                {
                    // Delete the page
                    await newPage.DeleteAsync();
                }
            }
        }

        #endregion

        #region Page scheduling

        [TestMethod]
        public async Task SchedulePagePublish()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("SchedulePagePublish.aspx");

                // Save the page
                await newPage.SaveAsync(pageName);

                // Schedule the page publishing
                DateTime scheduleDate = DateTime.MinValue;
                if (!TestCommon.Instance.Mocking)
                {
                    scheduleDate = DateTime.Now + new TimeSpan(0, 5, 0);
                    Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "Ticks", scheduleDate.Ticks.ToString() },
                        };
                    TestManager.SaveProperties(context, properties);
                }
                else
                {
                    var properties = TestManager.GetProperties(context);
                    scheduleDate = new DateTime(long.Parse(properties["Ticks"]));
                }

                newPage.SchedulePublish(scheduleDate);

                // Verify the scheduled publishing date
                if (!TestCommon.RunningInGitHubWorkflow())
                {
                    Assert.AreEqual(scheduleDate.Day, newPage.ScheduledPublishDate.Value.Day);
                    Assert.AreEqual(scheduleDate.Hour, newPage.ScheduledPublishDate.Value.Hour);
                    Assert.AreEqual(scheduleDate.Minute, newPage.ScheduledPublishDate.Value.Minute);
                }
                else
                {
                    Assert.IsTrue(newPage.ScheduledPublishDate.Value > DateTime.MinValue);
                }

                // Load the page again
                var pages = await context.Web.GetPagesAsync(pageName);
                var createdPage = pages.First();

                // Verify the scheduled publishing date
                if (!TestCommon.RunningInGitHubWorkflow())
                {
                    Assert.AreEqual(scheduleDate.Day, createdPage.ScheduledPublishDate.Value.Day);
                    Assert.AreEqual(scheduleDate.Hour, createdPage.ScheduledPublishDate.Value.Hour);
                    Assert.AreEqual(scheduleDate.Minute, createdPage.ScheduledPublishDate.Value.Minute);
                }
                else
                {
                    Assert.IsTrue(createdPage.ScheduledPublishDate.Value > DateTime.MinValue);
                }

                // Clear the scheduled publishing
                createdPage.RemoveSchedulePublish();

                Assert.IsFalse(createdPage.ScheduledPublishDate.HasValue);

                // reload the page again
                pages = await context.Web.GetPagesAsync(pageName);
                createdPage = pages.First();

                Assert.IsFalse(createdPage.ScheduledPublishDate.HasValue);

                // Delete the page
                await newPage.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task VerifyScheduledPublishDateOnSubSite()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                var newPage = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("VerifyScheduledPublishDateOnSubSite.aspx");

                // Save the page
                await newPage.SaveAsync(pageName);

                Assert.IsFalse(newPage.ScheduledPublishDate.HasValue);

                // Load the page again
                var pages = await context.Web.GetPagesAsync(pageName);
                var createdPage = pages.First();

                Assert.IsFalse(createdPage.ScheduledPublishDate.HasValue);

                // Delete the page
                await newPage.DeleteAsync();
            }
        }

        #endregion

        #region Other page types: Repost page

        [TestMethod]
        public async Task RePostPageCreate()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Upload image to site assets library
                IFolder parentFolder = await context.Web.Folders.FirstOrDefaultAsync(f => f.Name == "SiteAssets");
                IFile previewImage = await parentFolder.Files.AddAsync("repostpreview.jpg", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}pageheader.jpg"), overwrite: true);

                var newPage = await context.Web.NewPageAsync(PageLayoutType.RepostPage);
                string pageName = TestCommon.GetPnPSdkTestAssetName("RePostPageCreate.aspx");

                // Save the page
                string repostUrl = "https://techcommunity.microsoft.com/t5/sharepoint/how-to-create-repost-page-modern-page-library-with-powershell/m-p/269332";
                newPage.RepostSourceUrl = repostUrl;
                newPage.RepostDescription = "Some description";
                newPage.ThumbnailUrl = previewImage.ServerRelativeUrl;
                newPage.PageTitle = "Custom page title";

                await newPage.SaveAsync(pageName);

                // Load the page again
                var pages = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages.Count == 1);
                newPage = pages.AsEnumerable().First();

                Assert.IsTrue(newPage.LayoutType == PageLayoutType.RepostPage);
                Assert.IsTrue(newPage.RepostSourceUrl == repostUrl);
                Assert.IsTrue(newPage.RepostDescription == "Some description");
                Assert.IsTrue(newPage.PageTitle == "Custom page title");
                Assert.IsTrue(newPage.ThumbnailUrl == previewImage.ServerRelativeUrl);

                // Delete the page
                await newPage.DeleteAsync();

                // delete the page header image
                await previewImage.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task RePostPageToOtherPage()
        {
            //TestCommon.Instance.Mocking = false;
            Guid siteId;
            Guid webId;
            Guid listId;
            Guid itemId;
            string fileServerRelativeUrl;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var pages = await context.Web.GetPagesAsync("Home.aspx");
                var homePage = pages.First();

                var file = await homePage.GetPageFileAsync(p => p.ListId, p => p.UniqueId, p => p.ServerRelativeUrl);
                siteId = context.Site.Id;
                webId = context.Web.Id;
                listId = file.ListId;
                itemId = file.UniqueId;
                fileServerRelativeUrl = file.ServerRelativeUrl;
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {

                var newPage = await context.Web.NewPageAsync(PageLayoutType.RepostPage);
                string pageName = TestCommon.GetPnPSdkTestAssetName("RePostPageToOtherPage.aspx");

                // Save the page
                newPage.RepostSourceUrl = fileServerRelativeUrl;
                newPage.RepostSourceSiteId = siteId;
                newPage.RepostSourceWebId = webId;
                newPage.RepostSourceListId = listId;
                newPage.RepostSourceItemId = itemId;

                await newPage.SaveAsync(pageName);

                // Load the page again
                var pages = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages.Count == 1);
                newPage = pages.AsEnumerable().First();

                Assert.IsTrue(newPage.LayoutType == PageLayoutType.RepostPage);
                Assert.IsTrue(newPage.RepostSourceUrl == fileServerRelativeUrl);
                Assert.IsTrue(newPage.RepostSourceSiteId == siteId);
                Assert.IsTrue(newPage.RepostSourceWebId == webId);
                Assert.IsTrue(newPage.RepostSourceListId == listId);
                Assert.IsTrue(newPage.RepostSourceItemId == itemId);

                // Delete the page
                await newPage.DeleteAsync();
            }
        }
        #endregion

        #region Other page types: Home page

        [TestMethod]
        public async Task HomePageCreate()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Check current site home page is recognized
                var pages = await context.Web.GetPagesAsync("home.aspx");
                Assert.IsTrue(pages.Count == 1);
                Assert.IsTrue(pages.AsEnumerable().First().LayoutType == PageLayoutType.Home);
                // Not reliable at the moment
                //Assert.IsTrue(pages.AsEnumerable().First().KeepDefaultWebParts);

                // Create new "home" page
                var newPage = await context.Web.NewPageAsync(PageLayoutType.Home);
                string pageName = TestCommon.GetPnPSdkTestAssetName("HomePageCreate.aspx");

                // A simple section and text control to the page                
                newPage.AddControl(newPage.NewTextPart("PnP"), newPage.DefaultSection.DefaultColumn);

                // Save the page
                await newPage.SaveAsync(pageName);

                // Load the page again
                pages = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages.Count == 1);
                newPage = pages.AsEnumerable().First();

                Assert.IsTrue(newPage.LayoutType == PageLayoutType.Home);

                // Delete the page
                await newPage.DeleteAsync();
            }
        }
        #endregion

        #region Other page types: Spaces page

        [TestMethod]
        public async Task SpacesPageCreate()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create new "spaces" page
                var newPage = await context.Web.NewPageAsync(PageLayoutType.Spaces);
                string pageName = TestCommon.GetPnPSdkTestAssetName("SpacesPageCreate.aspx");

                // A simple section and text control to the page                
                newPage.AddControl(newPage.NewTextPart("PnP"), newPage.DefaultSection.DefaultColumn);

                // Save the page
                await newPage.SaveAsync(pageName);

                // Load the page again
                var pages = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages.Count == 1);
                newPage = pages.AsEnumerable().First();

                Assert.IsTrue(newPage.LayoutType == PageLayoutType.Spaces);

                // Delete the page
                await newPage.DeleteAsync();
            }
        }
        #endregion

        #region Other page types: Topic page
        [TestMethod]
        public async Task TopicPageClone()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.SharePointVivaTopicsTestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.VivaTopicCenterTestSite))
            {
                // We assume this page exists for now, once there's support to create new topic pages this can be updated
                var pages = await context.Web.GetPagesAsync("topicA");
                var topicPage = pages.AsEnumerable().First();
                string pageName = TestCommon.GetPnPSdkTestAssetName("TopicPageClone.aspx");

                // Save the page under a new name
                topicPage.PageTitle = "TopicPageClone";
                await topicPage.SaveAsync(pageName);

                // Load the page again
                pages = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages.Count == 1);
                topicPage = pages.AsEnumerable().First();

                Assert.IsTrue(topicPage.LayoutType == PageLayoutType.Topic);

                // Delete the page
                await topicPage.DeleteAsync();
            }
        }
        #endregion

        #region Other page types : SingleWebPartAppPage page
        [TestMethod]
        public async Task SingleWebPartAppPageCreate()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Create new "spaces" page
                var newPage = await context.Web.NewPageAsync(PageLayoutType.SingleWebPartAppPage);
                string pageName = TestCommon.GetPnPSdkTestAssetName("SingleWebPartAppPageCreate.aspx");

                // adding sections to the page
                newPage.AddSection(CanvasSectionTemplate.OneColumn, 1);

                // get the web part 'blueprint' --> this uses our standard test app
                // See setuptestenv.ps1 for the PnP PS commands to install the test app (.\TestAssets\pnpcoresdk-test-app.sppkg)
                var availableComponents = await newPage.AvailablePageComponentsAsync();
                var pnpWebPartComponent = availableComponents.FirstOrDefault(p => p.Id == "{9A57F808-CA0E-408E-B28C-319A9C8204ED}");
                var pnpWebPart = newPage.NewWebPart(pnpWebPartComponent);

                // add the web part to the first column of the first section
                newPage.AddControl(pnpWebPart, newPage.Sections[0].Columns[0]);

                // Save the page
                await newPage.SaveAsync(pageName);

                // Load the page again
                var pages = await context.Web.GetPagesAsync(pageName);
                Assert.IsTrue(pages.Count == 1);
                newPage = pages.AsEnumerable().First();

                Assert.IsTrue(newPage.LayoutType == PageLayoutType.SingleWebPartAppPage);

                // Delete the page
                await newPage.DeleteAsync();
            }
        }
        #endregion

    }
}