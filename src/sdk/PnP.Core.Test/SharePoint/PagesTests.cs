using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
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
                var availableComponents = await pages.First().AvailableClientSideComponentsAsync();
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
                var availableComponents = await pages.First().AvailableClientSideComponentsAsync(imageWebPartId);
                Assert.IsTrue(availableComponents.Count() == 1);
                Assert.IsTrue(availableComponents.First().Id == imageWebPartId);
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
                newPage.AddSection(CanvasSectionTemplate.ThreeColumnVerticalSection, 1, VariantThemeType.Soft, VariantThemeType.Strong);
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


        //[TestMethod]
        //public async Task Bert1()
        //{
        //    TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {

        //        var pages = await context.Web.GetPagesAsync("Bert1");

        //        Assert.IsTrue(pages.Count > 0);

        //        await pages.First().SaveAsync("Bert1Clone.aspx");

        //    }
        //}

    }
}
