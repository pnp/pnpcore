using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Telemetry.Observers;
using static PnP.Core.Modernization.Tests.TestCommon;
using PnP.Core.Modernization.Pages;
using PnP.Core.Modernization.Telemetry;

namespace PnP.Core.Modernization.Tests.Transform.CommonTests
{
    /// <summary>
    /// Summary description for CommonSP_PublishingPages
    /// </summary>
    [TestClass]
    [TestCategory("Integration")]
    public class CommonSPPublishingPages
    {
        #region Test Config

        public CommonSPPublishingPages()
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
        public void AllCommonPublishingPages_SPO()
        {
            TransformPage(SPPlatformVersion.SPO);
        }

        #endregion

        #region Test Code

        // Common Tests

        /// <summary>
        /// Standard Transform Test
        /// </summary>
        /// <param name="version"></param>
        /// <param name="fullPageLayoutMapping"></param>
        /// <param name="pageNameStartsWith"></param>
        private void TransformPage(SPPlatformVersion version, string fullPageLayoutMapping = "", string pageNameStartsWith = "Common")
        {

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateSPPlatformClientContext(version, TransformType.PublishingPage))
                {

                    var  pageTransformator = new PublishingPageTransformator(sourceClientContext, targetClientContext, fullPageLayoutMapping);
                    pageTransformator.RegisterObserver(new MarkdownObserver(folder: TestContext.ResultsDirectory, includeVerbose: true));
                    pageTransformator.RegisterObserver(new UnitTestLogObserver());

                    var pages = sourceClientContext.Web.GetPagesFromList("Pages", pageNameStartsWith);
                    pages.FailTestIfZero();

                    foreach (IListItem page in pages)
                    {
                        var pageName = page.Values["FileLeafRef"].ToString();
                            
                        PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation(page)
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

                        Console.WriteLine("SharePoint Version: {0}", pti.SourceVersion);

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

    /// <summary>
    /// Test class to access base page methods
    /// </summary>
    public class TestBasePage : BasePage
    {
        public TestBasePage(IListItem item, IFile file, PageTransformation pt, IList<ILogObserver> logObservers) : base(item, file, pt, logObservers)
        {

        }
    }


}
