using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class ListMetaDataMapperTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        //[TestMethod]
        //public void ListTypeTest()
        //{
        //    // Library types
        //    Assert.IsTrue(ListMetaDataMapper.IsLibrary(ListTemplateType.DocumentLibrary));
        //    Assert.IsTrue(ListMetaDataMapper.IsLibrary(ListTemplateType.WebPageLibrary));
        //    Assert.IsTrue(ListMetaDataMapper.IsLibrary(ListTemplateType.XMLForm));
        //    Assert.IsTrue(ListMetaDataMapper.IsLibrary(ListTemplateType.PictureLibrary));
        //    Assert.IsTrue(ListMetaDataMapper.IsLibrary(ListTemplateType.DataConnectionLibrary));
        //    Assert.IsTrue(ListMetaDataMapper.IsLibrary(ListTemplateType.HelpLibrary));
        //    Assert.IsTrue(ListMetaDataMapper.IsLibrary(ListTemplateType.HomePageLibrary));
        //    Assert.IsTrue(ListMetaDataMapper.IsLibrary(ListTemplateType.MySiteDocumentLibrary));
        //    Assert.IsTrue(ListMetaDataMapper.IsLibrary(ListTemplateType.SharingLinks));
        //    Assert.IsTrue(ListMetaDataMapper.IsLibrary((ListTemplateType)10102));
        //    Assert.IsTrue(ListMetaDataMapper.IsLibrary((ListTemplateType)3300));

        //    // Catalogs
        //    Assert.IsTrue(ListMetaDataMapper.IsCatalog(ListTemplateType.AppDataCatalog));
        //    Assert.IsTrue(ListMetaDataMapper.IsCatalog(ListTemplateType.AppFilesCatalog));
        //    Assert.IsTrue(ListMetaDataMapper.IsCatalog(ListTemplateType.DesignCatalog));
        //    Assert.IsTrue(ListMetaDataMapper.IsCatalog(ListTemplateType.ListTemplateCatalog));
        //    Assert.IsTrue(ListMetaDataMapper.IsCatalog(ListTemplateType.MasterPageCatalog));
        //    Assert.IsTrue(ListMetaDataMapper.IsCatalog(ListTemplateType.MaintenanceLogs));
        //    Assert.IsTrue(ListMetaDataMapper.IsCatalog(ListTemplateType.SolutionCatalog));
        //    Assert.IsTrue(ListMetaDataMapper.IsCatalog(ListTemplateType.ThemeCatalog));
        //    Assert.IsTrue(ListMetaDataMapper.IsCatalog(ListTemplateType.WebPartCatalog));
        //    Assert.IsTrue(ListMetaDataMapper.IsCatalog(ListTemplateType.WebTemplateCatalog));
        //    Assert.IsTrue(ListMetaDataMapper.IsCatalog(ListTemplateType.NoCodePublic));

        //    // Lists
        //    Assert.IsTrue(ListMetaDataMapper.IsList(ListTemplateType.GenericList));
        //    Assert.IsTrue(ListMetaDataMapper.IsList(ListTemplateType.Survey));
        //    Assert.IsTrue(ListMetaDataMapper.IsList(ListTemplateType.Links));
        //}

        //[TestMethod]
        //public void RestEntityToServerRelativeUrlTest()
        //{
        //    Assert.AreEqual("https://contoso.sharepoint.com/sites/siteA/lists/Demo",
        //        ListMetaDataMapper.RestEntityTypeNameToUrl(new System.Uri("https://contoso.sharepoint.com/sites/siteA"), "DemoList", ListTemplateType.GenericList));

        //    Assert.AreEqual("https://contoso.sharepoint.com/sites/siteA/lists/199f4c089b2a87472ebd357031a7c11be9threadtacv2_wiki",
        //        ListMetaDataMapper.RestEntityTypeNameToUrl(new System.Uri("https://contoso.sharepoint.com/sites/siteA"), "199f4c089b2a87472ebd357031a7c11be9threadtacv2_x005f_wikiList", ListTemplateType.GenericList));

        //    Assert.AreEqual("https://contoso.sharepoint.com/sites/siteA/_catalogs/design",
        //        ListMetaDataMapper.RestEntityTypeNameToUrl(new System.Uri("https://contoso.sharepoint.com/sites/siteA"), "OData__x005f_catalogs_x002f_design", ListTemplateType.DesignCatalog));

        //    Assert.AreEqual("https://contoso.sharepoint.com/sites/siteA/lists/ContentTypeSyncLog",
        //        ListMetaDataMapper.RestEntityTypeNameToUrl(new System.Uri("https://contoso.sharepoint.com/sites/siteA"), "ContentTypeSyncLogList", ListTemplateType.GenericList));

        //    Assert.AreEqual("https://contoso.sharepoint.com/sites/siteA/Shared Documents",
        //        ListMetaDataMapper.RestEntityTypeNameToUrl(new System.Uri("https://contoso.sharepoint.com/sites/siteA"), "Shared_x0020_Documents", ListTemplateType.DocumentLibrary));

        //    Assert.AreEqual("https://contoso.sharepoint.com/sites/siteA/_catalogs/users",
        //        ListMetaDataMapper.RestEntityTypeNameToUrl(new System.Uri("https://contoso.sharepoint.com/sites/siteA"), "UserInfo", ListTemplateType.UserInformation));
        //}

        [TestMethod]
        public void GraphNameToRestEntityTest()
        {
            Assert.AreEqual("DemoList", ListMetaDataMapper.MicrosoftGraphNameToRestEntityTypeName("Demo", ListTemplateType.GenericList));
            Assert.AreEqual("199f4c089b2a87472ebd357031a7c11be9threadtacv2_x005f_wikiList", ListMetaDataMapper.MicrosoftGraphNameToRestEntityTypeName("199f4c089b2a87472ebd357031a7c11be9threadtacv2_wiki", ListTemplateType.GenericList));
            Assert.AreEqual("OData__x005f_catalogs_x002f_design", ListMetaDataMapper.MicrosoftGraphNameToRestEntityTypeName("design", ListTemplateType.DesignCatalog));
            Assert.AreEqual("ContentTypeSyncLogList", ListMetaDataMapper.MicrosoftGraphNameToRestEntityTypeName("ContentTypeSyncLog", ListTemplateType.GenericList));
            Assert.AreEqual("Shared_x0020_Documents", ListMetaDataMapper.MicrosoftGraphNameToRestEntityTypeName("Shared Documents", ListTemplateType.DocumentLibrary));
            Assert.AreEqual("UserInfo", ListMetaDataMapper.MicrosoftGraphNameToRestEntityTypeName("users", ListTemplateType.UserInformation));
        }

    }

}