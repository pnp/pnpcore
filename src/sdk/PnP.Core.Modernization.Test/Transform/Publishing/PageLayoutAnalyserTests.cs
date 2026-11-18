using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Publishing;

namespace PnP.Core.Modernization.Tests.Transform.Publishing
{
    [TestClass]
    [TestCategory("Integration")]
    public class PageLayoutAnalyserTests
    {
        [TestMethod]
        public void PageLayoutAnalyse_SimpleReadOutput()
        {
            using (var sourceClientContext = TestCommon.CreateClientContext())
            {
                var pageLayoutAnalyser = new PageLayoutAnalyser(sourceClientContext);
                pageLayoutAnalyser.RegisterObserver(new UnitTestLogObserver());
                                
                //This will need option for target output location
                var result = pageLayoutAnalyser.GenerateMappingFile();
                Console.WriteLine("Mapping file: {0}", result);

                Assert.IsNotNull(result);

            }
        }

        [TestMethod]
        public void PageLayoutAnalyse_GetPageLayouts()
        {
            using (var sourceClientContext = TestCommon.CreateClientContext())
            {
                var pageLayoutAnalyser = new PageLayoutAnalyser(sourceClientContext);
                pageLayoutAnalyser.RegisterObserver(new UnitTestLogObserver());

                var result = pageLayoutAnalyser.GetAllPageLayouts();


                //This will need option for target output location
                Assert.IsNotNull(result);
                Assert.IsTrue(result.AsRequested().Any());

            }
        }

        [TestMethod]
        public void PageLayoutAnalyse_AnalyseAllWithOutput()
        {
            using (var sourceClientContext = TestCommon.CreateClientContext())
            {
                var pageLayoutAnalyser = new PageLayoutAnalyser(sourceClientContext);
                pageLayoutAnalyser.RegisterObserver(new UnitTestLogObserver());

                pageLayoutAnalyser.AnalyseAll();
                var result = pageLayoutAnalyser.GenerateMappingFile();

                //This will need option for target output location
                Assert.IsNotNull(result);
                
            }
        }

        [TestMethod]
        public void PageLayoutAnalyse_AnalyseSingleWithOutput()
        {
            using (var sourceClientContext = TestCommon.CreateClientContext())
            {
                // Source Context could be a site collection
                PnPContext contextToUse;
                if (sourceClientContext.Web.IsSubSite())
                {
                    var siteCollectionUrl = sourceClientContext.Site.EnsureProperty(o => o.Url);
                    contextToUse = sourceClientContext.Clone(siteCollectionUrl);
                }
                else
                {
                    contextToUse = sourceClientContext;
                }

                var pageLayoutAnalyser = new PageLayoutAnalyser(sourceClientContext);
                pageLayoutAnalyser.RegisterObserver(new UnitTestLogObserver());

                var layout = contextToUse.Web.GetFileByServerRelativeUrl($"{contextToUse.Web.EnsureProperty(o => o.ServerRelativeUrl)}/_catalogs/masterpage/ArticleCustom.aspx");

                var result = string.Empty;
                if(layout!= null){
                    var item = layout.EnsureProperty(o=> o.ListItemAllFields);
                    
                    pageLayoutAnalyser.AnalysePageLayout(item);
                    result = pageLayoutAnalyser.GenerateMappingFile();
                }



                Assert.IsTrue(result != string.Empty);

            }
        }

        [TestMethod]
        public void PageLayoutAnalyse_AnalyseByPageWithOutput()
        {
            using (var sourceClientContext = TestCommon.CreateClientContext())
            {
               
                var pageLayoutAnalyser = new PageLayoutAnalyser(sourceClientContext);
                pageLayoutAnalyser.RegisterObserver(new UnitTestLogObserver());

                var pageUrl = $"{sourceClientContext.Web.EnsureProperty(o => o.ServerRelativeUrl)}/pages/Article-Custom.aspx";
                var publishingPage = sourceClientContext.Web.GetFileByServerRelativeUrl(pageUrl);

                var result = string.Empty;
                if (publishingPage != null)
                {
                    var item = publishingPage.EnsureProperty(o => o.ListItemAllFields);

                    pageLayoutAnalyser.AnalysePageLayoutFromPublishingPage(item);
                    result = pageLayoutAnalyser.GenerateMappingFile();
                }
                else
                {
                    Assert.Fail("Failed to retrieve object for the test");
                }



                Assert.IsTrue(result != string.Empty);

            }
        }

        [TestMethod]
        public void PageLayoutAnalyse_AspxHeaderAndNameSpaces()
        {
            using (var sourceClientContext = TestCommon.CreateClientContext())
            {
                // Source Context could be a site collection
                PnPContext contextToUse;
                if (sourceClientContext.Web.IsSubSite())
                {
                    var siteCollectionUrl = sourceClientContext.Site.EnsureProperty(o => o.Url);
                    contextToUse = sourceClientContext.Clone(siteCollectionUrl);
                }
                else
                {
                    contextToUse = sourceClientContext;
                }

                var pageLayoutAnalyser = new PageLayoutAnalyser(sourceClientContext);
                pageLayoutAnalyser.RegisterObserver(new UnitTestLogObserver());

                var layout = contextToUse.Web.GetFileByServerRelativeUrl($"{contextToUse.Web.EnsureProperty(o => o.ServerRelativeUrl)}/_catalogs/masterpage/ArticleCustom.aspx");

                var results = new List<Tuple<string,string>>();
                if (layout != null)
                {
                    var item = layout.EnsureProperty(o => o.ListItemAllFields);

                    results.AddRange(pageLayoutAnalyser.ExtractWebPartPrefixesFromNamespaces(item));
                }
                else
                {
                    Assert.Fail("Layout file is missing, upload ArticleCustom.aspx as a page layout and publish the file");
                }

                Assert.IsTrue(results.Count > 0);
            }
        }

        [TestMethod]
        public void PageLayoutAnalyse_GetListOfWebPartsAssemblyReference()
        {
            var result = WebParts.GetListOfWebParts();

            result.ForEach(o =>
            {
                Console.WriteLine(o);
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public void PageLayoutAnalyse_GetListOfWebPartsAssemblyReferenceWithNameSpace()
        {
            var result = WebParts.GetListOfWebParts("Microsoft.SharePoint.WebPartPages");

            result.ForEach(o =>
            {
                Console.WriteLine(o);
            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

    }
}
