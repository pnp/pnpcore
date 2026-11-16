using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Telemetry.Observers;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Tests.Transform.CommonTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class CommonSPWikiTests
    {
        public CommonSPWikiTests()
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
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region SharePoint 2010 Tests

        [TestCategory(TestCategories.SP2010)]
        [TestMethod]
        public void AllCommonWikiPages_SP2010()
        {
            TransformPage(SPPlatformVersion.SP2010);
        }

        #endregion

        #region SharePoint 2013 Tests

        [TestCategory(TestCategories.SP2013)]
        [TestMethod]
        public void AllCommonWikiPages_SP2013()
        {
            TransformPage(SPPlatformVersion.SP2013);
        }

        #endregion

        #region SharePoint 2016 Tests

        [TestCategory(TestCategories.SP2016)]
        [TestMethod]
        public void AllCommonWikiPages_SP2016()
        {
            TransformPage(SPPlatformVersion.SP2016);
        }

        #endregion

        #region SharePoint 2019 Tests

        [TestCategory(TestCategories.SP2019)]
        [TestMethod]
        public void AllCommonWikiPages_SP2019()
        {
            TransformPage(SPPlatformVersion.SP2019);
        }

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
        private void TransformPage(SPPlatformVersion version, string pageNameStartsWith = "Common-WikiPage")
        {

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateSPPlatformClientContext(version, TransformType.WikiPage))
                {
                    var pageTransformator = new PageTransformator(sourceClientContext, targetClientContext);
                    pageTransformator.RegisterObserver(new MarkdownObserver(folder: "c:\\temp", includeVerbose: true));
                    pageTransformator.RegisterObserver(new UnitTestLogObserver());

                    var pages = sourceClientContext.Web.GetPages(pageNameStartsWith);

                    pages.FailTestIfZero();

                    foreach (var page in pages)
                    {
                        var pageName = page.FieldValues["FileLeafRef"].ToString();

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
