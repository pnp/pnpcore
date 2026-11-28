using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Modernization.Tests.Transform
{
    [TestClass]
    [TestCategory("Integration")]
    public class LoggingTests
    {

        [TestMethod]
        public void Logging_ErrorTest()
        {

            // Deliberate Error
            var pageTransformator = new PageTransformator(null);
            pageTransformator.RegisterObserver(new UnitTestLogObserver()); // Example of registering an observer, this can be anything really.

            PageTransformationInformation pti = new PageTransformationInformation(null);

            // Should capture a argument exception
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                pageTransformator.Transform(pti);
            });

        }

        [TestMethod]
        public void Logging_NormalOperationTest()
        {

            using (var sourceClientContext = TestCommon.CreateClientContext())
            {
                var pageTransformator = new PageTransformator(sourceClientContext);
                pageTransformator.RegisterObserver(new UnitTestLogObserver()); // Example of registering an observer, this can be anything really.

                var pages = WebExtensions.GetPages(sourceClientContext.Web,"wk").AsRequested().Take(1);

                foreach (IListItem page in pages)
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

                        // Give the migrated page a specific prefix, default is Migrated_
                        TargetPagePrefix = "Converted_",

                        // Replace embedded images and iframes with a placeholder and add respective images and video web parts at the bottom of the page
                        HandleWikiImagesAndVideos = false,

                    };

                    pageTransformator.Transform(pti);
                }
            }
        }


        [TestMethod]
        public void Logging_HelperFriendlyStringTest()
        {
            var result = LogHelpers.FormatAsFriendlyTitle("ThisIsATestString");
            var expected = "This Is A Test String";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Logging_PageTransInfoReflectionTest()
        {
            PageTransformationInformation pti = new PageTransformationInformation(null);
            List<LogEntry> result = pti.DetailSettingsAsLogEntries("some version");

            Assert.IsTrue(result.Count > 0);
        }
    }
}
