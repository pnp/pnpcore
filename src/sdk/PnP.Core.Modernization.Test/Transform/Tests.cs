using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;

namespace PnP.Core.Modernization.Tests.Transform
{
    [TestClass]
    [TestCategory("Integration")]
    public class Tests
    {
        class TestLayout : ILayoutTransformator
        {
            public void Transform(Tuple<Pages.PageLayout, List<WebPartEntity>> pageData)
            {
                throw new NotImplementedException();
            }
        }

        class TestTransformator : IContentTransformator
        {
            public void Transform(List<WebPartEntity> webParts)
            {
                throw new NotImplementedException();
            }
        }


        #region Test initialization
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            //using (var cc = TestCommon.CreateClientContext())
            //{
            //    // Clean all migrated pages before next run
            //    var pages = cc.Web.GetPages("Migrated_");

            //    foreach (var page in pages.ToList())
            //    {
            //        page.DeleteObject();
            //    }

            //    cc.ExecuteQueryRetry();
            //}
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {

        }
        #endregion

        [TestMethod]
        public void MetaDataCopyTest()
        {
            using (var cc = TestCommon.CreateClientContext("https://bertonline.sharepoint.com/sites/modernizationtestpages/metadata"))
            {
                var pageTransformator = new PageTransformator(cc);

                var pages = cc.Web.GetPages("meta2");

                foreach (var page in pages)
                {
                    PageTransformationInformation pti = new PageTransformationInformation(page)
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                        //CopyPageMetadata = true,

                        //KeepPageSpecificPermissions = false,

                        //// Modernization center setup
                        //ModernizationCenterInformation = new ModernizationCenterInformation()
                        //{
                        //    AddPageAcceptBanner = true,
                        //},
                    };

                    pageTransformator.Transform(pti);
                }
            }
        }

        [TestMethod]
        public void PageBannerTest()
        {
            using (var cc = TestCommon.CreateClientContext("https://bertonline.sharepoint.com/sites/contosoelectronicsdrones"))
            {
                var pageTransformator = new PageTransformator(cc);

                var pages = cc.Web.GetPages("d95");

                foreach (var page in pages)
                {
                    PageTransformationInformation pti = new PageTransformationInformation(page)
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                        // Modernization center setup
                        ModernizationCenterInformation = new ModernizationCenterInformation()
                        {
                            AddPageAcceptBanner = true,
                        },
                    };

                    pageTransformator.Transform(pti);
                }
            }
        }

        [TestMethod]
        public void FolderTest()
        {
            using (var cc = TestCommon.CreateClientContext("https://bertonline.sharepoint.com/sites/modernizationtestpages/subsite"))
            {
                var pageTransformator = new PageTransformator(cc);

                var pages = cc.Web.GetPages("page", "Folder1/Sub1");

                foreach (var page in pages)
                {
                    PageTransformationInformation pti = new PageTransformationInformation(page)
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                        //TargetPageTakesSourcePageName = true,

                    };

                    pageTransformator.Transform(pti);
                }
            }
        }

        [TestMethod]
        public void FileInRootTest()
        {
            using (var cc = TestCommon.CreateClientContext("https://bertonline.sharepoint.com/sites/projectsitetest"))
            {
                var pageTransformator = new PageTransformator(cc);

                var fileToModernize = cc.Web.GetFileByServerRelativeUrl("/sites/projectsitetest/default.aspx");
                cc.Load(fileToModernize);
                cc.ExecuteQueryRetry();

                PageTransformationInformation pti = new PageTransformationInformation(null)
                {
                    // If target page exists, then overwrite it
                    Overwrite = true,

                    SourceFile = fileToModernize,

                    // Don't log test runs
                    SkipTelemetry = true,

                    // Modernization center setup
                    //ModernizationCenterInformation = new ModernizationCenterInformation()
                    //{
                    //    AddPageAcceptBanner = true,
                    //},
                };

                var resultingpage = pageTransformator.Transform(pti);

            }
        }

        [TestMethod]
        public void WPPerformanceTest()
        {
            using (var cc = TestCommon.CreateClientContext("https://bertonline.sharepoint.com/sites/espctest2"))
            {
                var pageTransformator = new PageTransformator(cc);

                //webparts
                var pages = cc.Web.GetPages("webparts");

                foreach (var page in pages)
                {
                    PageTransformationInformation pti = new PageTransformationInformation(page)
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                        // Modernization center setup
                        //ModernizationCenterInformation = new ModernizationCenterInformation()
                        //{
                        //    AddPageAcceptBanner = true,
                        //},
                    };

                    pageTransformator.Transform(pti);
                }
            }
        }

        [TestMethod]
        public void CacheTest()
        {
            using (var cc = TestCommon.CreateClientContext())
            {
                var pageTransformator = new PageTransformator(cc);

                var pages = cc.Web.GetPages("table_1");

                foreach (var page in pages)
                {
                    PageTransformationInformation pti = new PageTransformationInformation(page)
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                    };

                    pageTransformator.Transform(pti);
                }
            }

            using (var cc = TestCommon.CreateClientContext("https://bertonline.sharepoint.com/sites/temp2"))
            {
                var pageTransformator = new PageTransformator(cc);

                var pages = cc.Web.GetPages("demo5");

                foreach (var page in pages)
                {
                    PageTransformationInformation pti = new PageTransformationInformation(page)
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                    };

                    pageTransformator.Transform(pti);
                }
            }

        }

        [TestMethod]
        public void TransformPagesTest()
        {
            // Local functions
            // string titleOverride(string title)
            // {
            //     return $"{title}_1";
            // }

            // ILayoutTransformator layoutOverride(ClientSidePage cp)
            // {
            //     return new TestLayout();
            // }

            // IContentTransformator contentOverride(ClientSidePage cp, PageTransformation pt)
            // {
            //     return new TestTransformator();
            // }

            using (var cc = TestCommon.CreateClientContext())
            {
                var pageTransformator = new PageTransformator(cc);

                //complexwiki
                //demo1
                //wikitext
                //wiki_li
                //webparts.aspx
                //contentbyquery1.aspx
                //how to use this library.aspx
                var pages = cc.Web.GetPages("w");

                foreach (var page in pages)
                {
                    PageTransformationInformation pti = new PageTransformationInformation(page)
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                        // ModernizationCenter options
                        ModernizationCenterInformation = new ModernizationCenterInformation()
                        {
                            AddPageAcceptBanner = true
                        },

                        // Migrated page gets the name of the original page
                        //TargetPageTakesSourcePageName = true,

                        // Give the migrated page a specific prefix, default is Migrated_
                        //TargetPagePrefix = "Yes_",

                        // Configure the page header, empty value means ClientSidePageHeaderType.None
                        //PageHeader = new ClientSidePageHeader(cc, ClientSidePageHeaderType.None, null),

                        // If the page is a home page then replace with stock home page
                        //ReplaceHomePageWithDefaultHomePage = true,

                        // Replace embedded images and iframes with a placeholder and add respective images and video web parts at the bottom of the page
                        HandleWikiImagesAndVideos = false,
                        
                        // Callout to your custom code to allow for title overriding
                        //PageTitleOverride = titleOverride,
                        
                        // Callout to your custom layout handler
                        //LayoutTransformatorOverride = layoutOverride,

                        // Callout to your custom content transformator...in case you fully want replace the model
                        //ContentTransformatorOverride = contentOverride,
                    };

                    pageTransformator.Transform(pti);
                }

            }
        }


    }
}
