using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
                var pages = await context.Web.GetPagesAsync("Ho");
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

                // delete folder with page
                var folderToDelete = await newPage.PagesLibrary.RootFolder.EnsureFolderAsync(TestCommon.GetPnPSdkTestAssetName("folder1"));
                await folderToDelete.DeleteAsync();
            }
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
                var availableComponents = await pages.First().AvailablePageComponentsAsync();
                Assert.IsTrue(availableComponents.Count() > 0);

                var imageWebPartId = pages.First().DefaultWebPartToWebPartId(DefaultWebPart.Image);
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
                var imageWebPartId = pages.First().DefaultWebPartToWebPartId(DefaultWebPart.Image);
                var availableComponents = await pages.First().AvailablePageComponentsAsync(imageWebPartId);
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
                    // Verify everythign went well
                    await context2.Web.EnsurePropertiesAsync(p => p.Features, p => p.IsMultilingual, p => p.SupportedUILanguageIds);

                    Assert.IsTrue(context2.Web.Features.FirstOrDefault(p => p.DefinitionId == new Guid("24611c05-ee19-45da-955f-6602264abaf8")) != null);
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

                    Assert.IsTrue(context3.Web.Features.FirstOrDefault(p => p.DefinitionId == new Guid("24611c05-ee19-45da-955f-6602264abaf8")) == null);
                    Assert.IsFalse(context3.Web.SupportedUILanguageIds.Contains(1043));
                    Assert.IsFalse(context3.Web.SupportedUILanguageIds.Contains(1036));
                }
            }
        }

        private static async Task DisableMultilingual(Core.Services.PnPContext context)
        {
            await context.Web.EnsurePropertiesAsync(p => p.Features, p => p.SupportedUILanguageIds);
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
                    await pagesToDelete.First().DeleteAsync();
                }

                var pages = await context.Web.GetPagesAsync(pageName);
                await pages.First().DeleteAsync();
                
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
                Assert.IsTrue(pages.First().PageTitle == pageName.Replace(".aspx", ""));
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
                page.AddSection(CanvasSectionTemplate.TwoColumnLeft , 4);
                page.AddSection(CanvasSectionTemplate.TwoColumnRight, 5);
                page.AddSection(CanvasSectionTemplate.ThreeColumn, 6);

                await page.SaveAsync(pageName);

                // load page again
                var pages = await context.Web.GetPagesAsync(pageName);

                Assert.IsTrue(pages.Count == 1);

                page = pages.First();

                Assert.IsTrue(page.Sections.Count == 6);
                Assert.IsTrue(page.Sections[0].Type == CanvasSectionTemplate.OneColumnFullWidth);
                Assert.IsTrue(page.Sections[1].Type == CanvasSectionTemplate.OneColumn);
                Assert.IsTrue(page.Sections[2].Type == CanvasSectionTemplate.TwoColumn);
                Assert.IsTrue(page.Sections[3].Type == CanvasSectionTemplate.TwoColumnLeft);
                Assert.IsTrue(page.Sections[4].Type == CanvasSectionTemplate.TwoColumnRight);
                Assert.IsTrue(page.Sections[5].Type == CanvasSectionTemplate.ThreeColumn);

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

                page = pages.First();

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

                page = pages.First();

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

                page = pages.First();

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

                page = pages.First();

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

                page = pages.First();

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

        [TestMethod]
        public async Task PageFullWidthSectionOnNonCommunicationSiteTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var page = await context.Web.NewPageAsync();
                string pageName = TestCommon.GetPnPSdkTestAssetName("PageFullWidthSectionOnNonCommunicationSiteTest.aspx");

                // Add all the possible sections 
                page.AddSection(CanvasSectionTemplate.OneColumnFullWidth, 1);
                page.AddSection(CanvasSectionTemplate.OneColumn, 2);

                bool exceptionThrown = false;
                try
                {
                    await page.SaveAsync(pageName);
                }
                catch(ClientException ex)
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

        #endregion

        #region Page saving

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
                var updatedPage = pages.First();

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
                var templatePage = pages.First();

                var templateFolder = await templatePage.GetTemplatesFolderAsync();
                var pageFolder = templatePage.Folder;

                Assert.AreEqual(templateFolder, pageFolder);

                // Delete the page
                await templatePage.DeleteAsync();

            }
        }

        //[TestMethod]
        //public async Task Bert()
        //{
        //    TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        var pages = await context.Web.GetPagesAsync("Bert1.aspx");
        //        Assert.IsTrue(pages.Count == 1);

        //        var page = pages.First();

        //        await page.SaveAsync("Bert1Clone.aspx");

        //    }
        //}


        //[TestMethod]
        //public async Task CreateNewPage()
        //{
        //    TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        var newPage = await context.Web.NewPageAsync();


        //        // Format header
        //        newPage.PageHeader.ShowTopicHeader = true;
        //        newPage.PageHeader.TopicHeader = "This is a topic header";

        //        // Add controls
        //        newPage.AddSection(CanvasSectionTemplate.ThreeColumnVerticalSection, 1, VariantThemeType.Soft, VariantThemeType.Strong);
        //        newPage.AddControl(newPage.NewTextPart("I"), newPage.Sections[0].Columns[0]);
        //        newPage.AddControl(newPage.NewTextPart("like"), newPage.Sections[0].Columns[1]);
        //        newPage.AddControl(newPage.NewTextPart("PnP"), newPage.Sections[0].Columns[2]);

        //        newPage.AddSection(CanvasSectionTemplate.TwoColumn, 2);

        //        var availableComponents = await newPage.AvailableClientSideComponentsAsync();

        //        var imageWebPartComponent = availableComponents.FirstOrDefault(p => p.Id == newPage.DefaultWebPartToWebPartId(DefaultWebPart.Image));
        //        var imageWebPart = newPage.NewWebPart(imageWebPartComponent);

        //        var propertiesJson = "{\"id\": \"d1d91016-032f-456d-98a4-721247c305e8\",\"instanceId\": \"157070a0-5d47-40a7-88fa-0051bf81ae1b\",\"title\": \"Image\",  \"description\": \"Image\",  \"serverProcessedContent\": {    \"htmlStrings\": {},    \"searchablePlainTexts\": {},    \"imageSources\": {      \"imageSource\": \"/sites/prov-1/SiteAssets/__siteIcon__.png\"    },    \"links\": {},    \"customMetadata\": {      \"imageSource\": {        \"siteid\": \"b56adf79-ff6a-4964-a63a-ff1fa23be9f8\",        \"webid\": \"8c8e101c-1b0d-4253-85e7-c30039bf46e2\",        \"listid\": \"0da08977-4f32-4cda-ae2d-91ef05fb54f6\",        \"uniqueid\": \"{A14769F2-CB4B-4220-8B11-B4FE755711D2}\",        \"width\": \"96\",        \"height\": \"96\"      }    }},  \"dataVersion\": \"1.9\",  \"properties\": {    \"imageSourceType\": 2,    \"captionText\": \"\",    \"altText\": \"\",    \"linkUrl\": \"\",    \"overlayText\": \"\",    \"fileName\": \"\",    \"siteId\": \"b56adf79-ff6a-4964-a63a-ff1fa23be9f8\",    \"webId\": \"8c8e101c-1b0d-4253-85e7-c30039bf46e2\",    \"listId\": \"0da08977-4f32-4cda-ae2d-91ef05fb54f6\",    \"uniqueId\": \"{A14769F2-CB4B-4220-8B11-B4FE755711D2}\",    \"imgWidth\": 96,    \"imgHeight\": 96,    \"alignment\": \"Center\",    \"fixAspectRatio\": false  }}";
        //        imageWebPart.PropertiesJson = propertiesJson;

        //        newPage.AddControl(imageWebPart, newPage.Sections[0].Columns[3]);

        //        newPage.AddControl(newPage.NewTextPart("I"), newPage.Sections[1].Columns[0]);
        //        newPage.AddControl(newPage.NewTextPart("like PnP"), newPage.Sections[1].Columns[1]);


        //        // TODO prevent adding of control like below
        //        //newPage.Sections[0].Columns[0].Controls.Add(newPage.NewTextPart("I"));
        //        //newPage.Sections[0].Columns[1].Controls.Add(newPage.NewTextPart("like"));
        //        //newPage.Sections[0].Columns[2].Controls.Add(newPage.NewTextPart("PnP"));

        //        await newPage.SaveAsync("test1");

        //        // Delete the created page again
        //        await newPage.DeleteAsync();

        //    }
        //}

        #endregion

    }
}
