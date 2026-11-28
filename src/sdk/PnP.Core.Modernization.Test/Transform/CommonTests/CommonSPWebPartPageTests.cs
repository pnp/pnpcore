using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Transform;
using PnP.Core.Modernization.Telemetry.Observers;

namespace PnP.Core.Modernization.Tests.Transform.CommonTests
{
    /// <summary>
    /// Summary description for CommonSP_WebPartPageTests
    /// </summary>
    [TestClass]
    [TestCategory("Integration")]
    public class CommonSPWebPartPageTests
    {
        public CommonSPWebPartPageTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        #endregion
        #region SharePoint 2010 Tests
        #endregion
        #region SharePoint 2013 Tests
        #endregion
        #region SharePoint 2016 Tests
        #endregion
        #region SharePoint 2019 Tests

        #endregion

        #region SharePoint Online Tests

        [TestCategory(TestCategories.SPO)]
        [TestMethod]
        public void AllCommonWikiPages_SPO()
        {
            TransformPage(SPPlatformVersion.SPO);
        }

        #endregion

        #region Code for Tests

        /// <summary>
        /// Different page same test conditions
        /// </summary>
        /// <param name="pageName"></param>
        private void TransformPage(SPPlatformVersion version, string pageNameStartsWith = "Common-WebPartPage")
        {

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateSPPlatformClientContext(version, TransformType.WebPartPage))
                {
                    var pageTransformator = new PageTransformator(sourceClientContext, targetClientContext);
                    pageTransformator.RegisterObserver(new MarkdownObserver(folder: "c:\\temp", includeVerbose: true));
                    pageTransformator.RegisterObserver(new UnitTestLogObserver());

                    var pages = WebExtensions.GetPages(sourceClientContext.Web,pageNameStartsWith);

                    pages.FailTestIfZero();

                    foreach (IListItem page in pages)
                    {
                        var pageName = page.Values["FileLeafRef"].ToString();

                        PageTransformationInformation pti = new PageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,

                            // Don't log test runs
                            SkipTelemetry = true,

                            //Permissions are unlikely to work given cross domain
                            KeepPageSpecificPermissions = false,

                            //Update target to include SP version
                            TargetPageName = TestCommon.UpdatePageToIncludeVersion(version, pageName)

                        };

                        pti.MappingProperties["SummaryLinksToQuickLinks"] = "true";
                        pti.MappingProperties["UseCommunityScriptEditor"] = "true";

                        var result = pageTransformator.Transform(pti);

                        Assert.IsTrue(!string.IsNullOrEmpty(result));
                    }

                    pageTransformator.FlushObservers();
                }
            }
        }

        #endregion

    }
}
