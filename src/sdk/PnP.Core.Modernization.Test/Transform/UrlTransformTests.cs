using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Telemetry.Observers;
using PnP.Core.Modernization.Transform;

namespace PnP.Core.Modernization.Tests.Transform
{
    [TestClass]
    [TestCategory("Integration")]
    public class UrlTransformTests
    {
        // ********************************************
        // Default URL rewriting logic
        // ********************************************            
        //
        // Root site collection URL rewriting:
        // Scenario 1 - http://contoso.com/sites/portal -> https://contoso.sharepoint.com/sites/hr
        // Scenario 2 - http://contoso.com/sites/portal/pages -> https://contoso.sharepoint.com/sites/hr/sitepages
        // Scenario 3 - /sites/portal -> /sites/hr
        // Scenario 4 - /sites/portal/pages -> /sites/hr/sitepages
        //
        // If site is a sub site then we also by rewrite the sub URL's
        // Scenario 5 - http://contoso.com/sites/portal/hr -> https://contoso.sharepoint.com/sites/hr
        // Scenario 6 - http://contoso.com/sites/portal/hr/pages -> https://contoso.sharepoint.com/sites/hr/sitepages
        // Scenario 7 - /sites/portal/hr -> /sites/hr
        // Scenario 8 - /sites/portal/hr/pages -> /sites/hr/sitepages

        // Scenario 9 - https://contoso.com > /sites/target
        // Scenario 10 - https://contoso.com/sites/source > /

        [TestMethod]
        public void UrlTransform_Scenario1()
        {
            var input = $"https://contoso.sharepoint.com/sites/source";
            var sourceSiteUrl = "https://contoso.sharepoint.com/sites/source";
            var sourceWebUrl = "https://contoso.sharepoint.com/sites/source";
            var targetWebUrl = "https://contoso.sharepoint.com/sites/target";
            var expected = $"https://contoso.sharepoint.com/sites/target";

            TestUrlTransform(input, sourceSiteUrl, sourceWebUrl, targetWebUrl, expected);
        }

        [TestMethod]
        public void UrlTransform_Scenario2()
        {
            var testPage = "mytestpage.aspx";
            var input = $"https://contoso.sharepoint.com/sites/source/pages/{testPage}";
            var sourceSiteUrl = "https://contoso.sharepoint.com/sites/source";
            var sourceWebUrl = "https://contoso.sharepoint.com/sites/source";
            var targetWebUrl = "https://contoso.sharepoint.com/sites/target";
            var expected = $"https://contoso.sharepoint.com/sites/target/sitepages/{testPage}";

            TestUrlTransform(input, sourceSiteUrl, sourceWebUrl, targetWebUrl, expected);
        }

        [TestMethod]
        public void UrlTransform_Scenario3()
        {
            var input = $"/sites/source/";
            var sourceSiteUrl = "https://contoso.sharepoint.com/sites/source";
            var sourceWebUrl = "https://contoso.sharepoint.com/sites/source";
            var targetWebUrl = "https://contoso.sharepoint.com/sites/target";
            var expected = $"/sites/target/";

            TestUrlTransform(input, sourceSiteUrl, sourceWebUrl, targetWebUrl, expected);
        }

        [TestMethod]
        public void UrlTransform_Scenario4()
        {
            var testPage = "mytestpage.aspx";
            var input = $"/sites/source/pages/{testPage}";
            var sourceSiteUrl = "https://contoso.sharepoint.com/sites/source";
            var sourceWebUrl = "https://contoso.sharepoint.com/sites/source";
            var targetWebUrl = "https://contoso.sharepoint.com/sites/target";
            var expected = $"/sites/target/sitepages/{testPage}";

            TestUrlTransform(input, sourceSiteUrl, sourceWebUrl, targetWebUrl, expected);
        }

        [TestMethod]
        public void UrlTransform_Scenario5()
        {
            var input = $"https://contoso.sharepoint.com/sites/source/subsite";
            var sourceSiteUrl = "https://contoso.sharepoint.com/sites/source";
            var sourceWebUrl = "https://contoso.sharepoint.com/sites/source/subsite";
            var targetWebUrl = "https://contoso.sharepoint.com/sites/target";
            var expected = $"https://contoso.sharepoint.com/sites/target";

            TestUrlTransform(input, sourceSiteUrl, sourceWebUrl, targetWebUrl, expected);
        }

        [TestMethod]
        public void UrlTransform_Scenario6()
        {
            var testPage = "mytestpage.aspx";
            var input = $"https://contoso.sharepoint.com/sites/source/subsite/pages/{testPage}";
            var sourceSiteUrl = "https://contoso.sharepoint.com/sites/source";
            var sourceWebUrl = "https://contoso.sharepoint.com/sites/source/subsite";
            var targetWebUrl = "https://contoso.sharepoint.com/sites/target";
            var expected = $"https://contoso.sharepoint.com/sites/target/sitepages/{testPage}";

            TestUrlTransform(input, sourceSiteUrl, sourceWebUrl, targetWebUrl, expected);
        }

        [TestMethod]
        public void UrlTransform_Scenario7()
        {
            var input = $"/sites/source/subsite";
            var sourceSiteUrl = "https://contoso.sharepoint.com/sites/source";
            var sourceWebUrl = "https://contoso.sharepoint.com/sites/source/subsite";
            var targetWebUrl = "https://contoso.sharepoint.com/sites/target";
            var expected = $"/sites/target";

            TestUrlTransform(input, sourceSiteUrl, sourceWebUrl, targetWebUrl, expected);
        }

        [TestMethod]
        public void UrlTransform_Scenario8()
        {
            var testPage = "mytestpage.aspx";
            var input = $"/sites/source/subsite/pages/{testPage}";
            var sourceSiteUrl = "https://contoso.sharepoint.com/sites/source";
            var sourceWebUrl = "https://contoso.sharepoint.com/sites/source/subsite";
            var targetWebUrl = "https://contoso.sharepoint.com/sites/target";
            var expected = $"/sites/target/sitepages/{testPage}";

            TestUrlTransform(input, sourceSiteUrl, sourceWebUrl, targetWebUrl, expected);
        }

        [TestMethod]
        public void UrlTransform_Scenario9()
        {
            var testPage = "mytestpage.aspx";
            var input = $"https://contoso.sharepoint.com/pages/{testPage}";
            var sourceSiteUrl = "https://contoso.sharepoint.com";
            var sourceWebUrl = "https://contoso.sharepoint.com";
            var targetWebUrl = "https://contoso.sharepoint.com/sites/target";
            var expected = $"https://contoso.sharepoint.com/sites/target/sitepages/{testPage}";

            TestUrlTransform(input, sourceSiteUrl, sourceWebUrl, targetWebUrl, expected);

        }


        [TestMethod]
        public void UrlTransform_Scenario10()
        {
            var testPage = "mytestpage.aspx";
            var input = $"https://contoso.sharepoint.com/sites/source/pages/{testPage}";
            var sourceSiteUrl = "https://contoso.sharepoint.com/sites/source";
            var sourceWebUrl = "https://contoso.sharepoint.com/sites/source";
            var targetWebUrl = "https://contoso.sharepoint.com/";
            var expected = $"https://contoso.sharepoint.com/sitepages/{testPage}";

            TestUrlTransform(input, sourceSiteUrl, sourceWebUrl, targetWebUrl, expected);

        }
                          
        /// <summary>
        /// Common call to class we are testing
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sourceSiteUrl"></param>
        /// <param name="sourceWebUrl"></param>
        /// <param name="targetWebUrl"></param>
        /// <param name="expected"></param>
        public void TestUrlTransform(string input, string sourceSiteUrl, string sourceWebUrl, string targetWebUrl, string expected)
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevSiteUrl")))
                {
                    List<ILogObserver> observers = new List<ILogObserver>
                    {
                        new UnitTestLogObserver()
                    };

                    var pagesLibrary = "pages";
                    
                    // Context doesnt matter what is used
                    UrlTransformator urlTransform = new UrlTransformator(null, sourceClientContext, targetClientContext, observers);
                    var result = urlTransform.ReWriteUrls(input, sourceSiteUrl, sourceWebUrl, targetWebUrl, pagesLibrary);

                    Console.WriteLine(result);
                    Assert.AreEqual(expected, result);
                }
            }
        }


       
    }
}
