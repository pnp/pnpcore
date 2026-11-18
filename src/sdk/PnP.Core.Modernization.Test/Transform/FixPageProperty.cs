using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Transform;

namespace PnP.Core.Modernization.Tests.Transform
{
    [TestClass]
    [TestCategory("Integration")]
    public class Provisioning_FixPageProperty
    {
        [TestMethod]
        public void FixPageProperty_NotReallyATest()
        {
            using (var sourceClientContext = TestCommon.CreateClientContext())
            {
                var pageTransformator = new PageTransformator(sourceClientContext);

                var pages = WebExtensions.GetPages(sourceClientContext.Web,"WPP_Image-Asset-MultipleImages");

                var pageItem = pages.FirstOrDefault();

                if (pageItem != default(IListItem))
                {

                    pageItem.File.LoadBatch(p => p.ServerRelativeUrl);
                    sourceClientContext.Execute();

                    var pageFile = sourceClientContext.Web.GetFileByServerRelativeUrl(pageItem.File.ServerRelativeUrl);
                    pageFile.LoadBatch(p => p.Properties);
                    pageFile.Properties["vti_setuppath"] = "1033\\STS\\doctemp\\smartpgs\\spstd2.aspx";
                    pageFile.Properties["fixing_layout"] = "1033\\STS\\doctemp\\smartpgs\\spstd2.aspx";
                    pageFile.Update();
                    sourceClientContext.Execute();

                    // Try both
                    //page.Properties["vti_setuppath"] = @"1033\STS\doctemp\smartpgs\spstd2.aspx";
                    //page.Update();
                    //sourceClientContext.ExecuteQueryRetry();



                    Assert.IsTrue(true, "File Updated");
                }
                else
                {
                    Assert.Fail("File not found");
                }
            }
        }
    }
}
