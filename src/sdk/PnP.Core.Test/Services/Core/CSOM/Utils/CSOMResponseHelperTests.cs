using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM.Utils;
using System;

namespace PnP.Core.Test.Services.Core.CSOM.Utils
{
    [TestClass]
    public class CSOMResponseHelperTests
    {
        [TestMethod]
        public void TestProcessResponse()
        {
            string responseString = @"[{ ""SchemaVersion"":""15.0.0.0"",""LibraryVersion"":""16.0.20927.12000"",""ErrorInfo"":null,""TraceCorrelationId"":""d355a89f-80d3-2000-78ab-415b98641aa9""},2,{
                ""IsNull"":false
          },4,{
                ""IsNull"":false
          },5,{
                ""_ObjectType_"":""SP.Web"",""_ObjectIdentity_"":""d355a89f-80d3-2000-78ab-415b98641aa9|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:3429eb18-5157-4ffe-ac09-a5b05edb8148:web:a5355253-94a1-48f1-ada4-da51338c3a5b"",""AllowRssFeeds"":true,""AlternateCssUrl"":"""",""AppInstanceId"":""\/Guid(00000000-0000-0000-0000-000000000000)\/"",""ClassicWelcomePage"":null,""Configuration"":0,""Created"":""\/Date(2019,11,5,17,7,26,0)\/"",""CurrentChangeToken"":{
                    ""_ObjectType_"":""SP.ChangeToken"",""StringValue"":""1;2;a5355253-94a1-48f1-ada4-da51338c3a5b;637481420855100000;285342269""
                },""CustomMasterUrl"":""\u002fsites\u002fdev\u002f_catalogs\u002fmasterpage\u002fseattle.master"",""Description"":"""",""DesignPackageId"":""\/Guid(00000000-0000-0000-0000-000000000000)\/"",""DocumentLibraryCalloutOfficeWebAppPreviewersDisabled"":false,""EnableMinimalDownload"":true,""FooterEmphasis"":0,""FooterEnabled"":false,""FooterLayout"":0,""HeaderEmphasis"":0,""HeaderLayout"":0,""HideTitleInHeader"":false,""HorizontalQuickLaunch"":false,""Id"":""\/Guid(a5355253-94a1-48f1-ada4-da51338c3a5b)\/"",""IsHomepageModernized"":false,""IsMultilingual"":false,""IsRevertHomepageLinkHidden"":false,""KeepFieldUserResources"":false,""Language"":1033,""LastItemModifiedDate"":""\/Date(1612534319000)\/"",""LastItemUserModifiedDate"":""\/Date(1612534319000)\/"",""LogoAlignment"":0,""MasterUrl"":""\u002fsites\u002fdev\u002f_catalogs\u002fmasterpage\u002fseattle.master"",""MegaMenuEnabled"":false,""NavAudienceTargetingEnabled"":false,""NoCrawl"":false,""ObjectCacheEnabled"":false,""OverwriteTranslationsOnChange"":false,""ResourcePath"":{
                    ""_ObjectType_"":""SP.ResourcePath"",""DecodedUrl"":""https:\u002f\u002ftest.sharepoint.com\u002fsites\u002fdev""
              },""QuickLaunchEnabled"":true,""RecycleBinEnabled"":true,""SearchScope"":0,""ServerRelativeUrl"":""\u002fsites\u002fdev"",""SiteLogoUrl"":null,""SyndicationEnabled"":true,""TenantAdminMembersCanShare"":0,""Title"":""Dev"",""TreeViewEnabled"":false,""UIVersion"":15,""UIVersionConfigurationEnabled"":false,""Url"":""https:\u002f\u002ftest.sharepoint.com\u002fsites\u002fdev"",""WebTemplate"":""STS"",""WelcomePage"":""SitePages\u002fDevHome.aspx""
          }
]";
            CSOMResponseHelper helper = new CSOMResponseHelper();
            Web response = helper.ProcessResponse<Web>(responseString, 5);
            Assert.AreEqual("Dev", response.Title);
            Assert.AreEqual(Guid.Parse("a5355253-94a1-48f1-ada4-da51338c3a5b"), response.Id);

            DateTime expectedDate = new DateTime(2019, 12, 5, 17, 7, 26, 0);
            Assert.AreEqual(expectedDate, response.Created);

            DateTime expectedLastItemModifiedDate = new DateTime(2021, 2, 5, 14, 11, 59, 0);
            Assert.AreEqual(expectedLastItemModifiedDate, response.LastItemModifiedDate);
        }
    }
}
